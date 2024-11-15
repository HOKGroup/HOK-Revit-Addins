#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.Core.WpfUtilities;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.StylesManager.Utilities;

#endregion

namespace HOK.MissionControl.StylesManager.Tabs
{
    public class DimensionOverridesModel
    {
        private Document _doc { get; }

        public DimensionOverridesModel(Document doc)
        {
            _doc = doc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dims"></param>
        /// <returns></returns>
        public List<DimensionWrapper> ClearOverrides(List<DimensionWrapper> dims)
        {
            var result = new List<DimensionWrapper>();
            using (var trans = new Transaction(_doc, "Clear Dimension Overrides"))
            {
                trans.Start();
                StatusBarManager.InitializeProgress("Removing Overrides...", dims.Count);

                foreach (var dw in dims)
                {
                    StatusBarManager.StepForward();
                    try
                    {
                        var dim = (Dimension)_doc.GetElement(dw.DimensionId);
                        if (dim == null) return null;

                        if (dim.NumberOfSegments == 0)
                        {
                            dim.ValueOverride = string.Empty;
                            result.Add(dw);
                        }
                        else
                        {
                            foreach (DimensionSegment s in dim.Segments)
                            {
                                if (s.GetHashCode() != dw.Hash) continue;

                                s.ValueOverride = string.Empty;
                                result.Add(dw);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                    }
                }

                StatusBarManager.FinalizeProgress();
                trans.Commit();
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<DimensionWrapper> CollectDimensionOverrides()
        {
            var dInstances = new FilteredElementCollector(_doc)
                .OfClass(typeof(Dimension))
                .WhereElementIsNotElementType()
                .Cast<Dimension>();

            // (Konrad) There is a user override in Configuration that controls what dimension overrides are ignored
            var centralPath = FileInfoUtil.GetCentralFilePath(_doc);
            var config = MissionControlSetup.Configurations.ContainsKey(centralPath)
                ? MissionControlSetup.Configurations[centralPath]
                : null;

            var dimensionValueCheck = new List<string> { "EQ" }; //defaults
            if (config != null)
            {
                dimensionValueCheck = config.Updaters.First(x => string.Equals(x.UpdaterId,
                    Properties.Resources.HealthReportTrackerGuid, StringComparison.OrdinalIgnoreCase)).UserOverrides.DimensionValueCheck.Values;
            }

            var units = _doc.GetUnits();
            var dims = new List<DimensionWrapper>();
            foreach (var d in dInstances)
            {
                if (d.NumberOfSegments == 0)
                {
                    if (string.IsNullOrEmpty(d.ValueOverride)) continue;

                    // dim w/ zero segments
                    dims.Add(new DimensionWrapper(d)
                    {
                        DimensionId = d.Id,
                        OwnerViewType = d.ViewSpecific
                            ? ((View)_doc.GetElement(d.OwnerViewId)).ViewType.ToString()
                            : string.Empty,
                        OwnerViewId = d.OwnerViewId,
#if REVIT2021_OR_GREATER
                        ValueString = UnitFormatUtils.Format(units, d.DimensionType.GetSpecTypeId(), (double)d.Value, false),
#else
                        ValueString = UnitFormatUtils.Format(units, d.DimensionType.UnitType, (double)d.Value, false, false),
#endif
                        IsValueOverrideHuge = EvaluateValueOverride(d.ValueOverride, d.Value),
                        IsVisible = !dimensionValueCheck.Any(d.ValueOverride.Contains),
                        IsFiltered = dimensionValueCheck.Any(d.ValueOverride.Contains)
                    });
                }
                else
                {
                    // dim w/ multiple segments
                    foreach (DimensionSegment s in d.Segments)
                    {
                        // not every segment has to be overriden
                        if (string.IsNullOrEmpty(s.ValueOverride)) continue;

                        dims.Add(new DimensionWrapper(s)
                        {
                            DimensionId = d.Id,
                            OwnerViewType = d.ViewSpecific
                                ? ((View)_doc.GetElement(d.OwnerViewId)).ViewType.ToString()
                                : string.Empty,
                            OwnerViewId = d.OwnerViewId,
#if REVIT2021_OR_GREATER
                            ValueString = UnitFormatUtils.Format(units, d.DimensionType.GetSpecTypeId(), (double)s.Value, false),
#else
                            ValueString = UnitFormatUtils.Format(units, d.DimensionType.UnitType, (double)s.Value, false, false),
#endif
                            IsValueOverrideHuge = EvaluateValueOverride(s.ValueOverride, s.Value),
                            IsVisible = !dimensionValueCheck.Any(s.ValueOverride.Contains),
                            IsFiltered = dimensionValueCheck.Any(s.ValueOverride.Contains)
                        });
                    }
                }
            }

            return new ObservableCollection<DimensionWrapper>(dims);
        }

        #region Utilities

        /// <summary>
        /// Checks if Value Override exceeded 1/8" max limit.
        /// </summary>
        /// <param name="overriden">String value for the override.</param>
        /// <param name="actual">Actual number value.</param>
        /// <returns>True if override larger than 1/8", False if it is not. Null if indeterminate.</returns>
        private bool? EvaluateValueOverride(string overriden, double? actual)
        {
            const double tolerance = 0.125;
            var units = _doc.DisplayUnitSystem;
            switch (units)
            {
                case DisplayUnit.METRIC:
                    return null;
                case DisplayUnit.IMPERIAL:
                    if (overriden.Contains('-'))
                    {
                        var arr = overriden.Split('-');
                        if (arr.Length != 2) return null;
                        if (!TryGetValue(arr.First().Trim(), out var part1)) return false;
                        if (!TryGetValue(arr.Last().Trim(), out var part2)) return false;

                        double first = -1;
                        double second = -1;
                        double third = -1;
                        try
                        {
                            first = (double)actual;
                            second = (double)part1;
                            third = (double)part2;
                        }
                        catch (Exception e)
                        {
                            Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                        }

                        if (first == -1 || second == -1 || third == -1) return null;

                        if (part1 < part2)
                        {
                            // we have a range ex. 12" - 16"
                            // return true if value falls outside of range
                            // there is 1/8" tolerance for all dimensions
                            return (first + tolerance) < second || first > (third + tolerance);
                        }

                        // we have a normal dimension 12' - 1 1/2"
                        // return true if difference between override and value is more than 1/8"
                        return Math.Abs(first - (second + third)) > tolerance;
                    }
                    else
                    {
                        // we are dealing with a single dim string like 16"
                        if (!TryGetValue(overriden, out var part1)) return false;

                        double first = -1;
                        double second = -1;
                        try
                        {
                            first = (double)actual;
                            second = (double)part1;
                        }
                        catch (Exception e)
                        {
                            Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                        }

                        if (first == -1 || second == -1) return null;

                        // return true if difference between override and value is more than 1/8"
                        return Math.Abs(first - second) > tolerance;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool TryGetValue(string value, out double? result)
        {
            result = null;
            var s = string.Empty;
            var match = Regex.Match(value, @"^[0-9\'\-\/\""\s]*", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                s = match.Value.Trim();
            }

            if (string.IsNullOrEmpty(s)) return false;

            var verify = Regex.Match(s, @"^[0-9\'\-\/\""\s]*$", RegexOptions.IgnoreCase);
            if (!verify.Success) return false;

#if REVIT2021_OR_GREATER
            if (!UnitFormatUtils.TryParse(_doc.GetUnits(), SpecTypeId.Length, s,
                new ValueParsingOptions(),
                out var second)) return false;
#else
            if (!UnitFormatUtils.TryParse(_doc.GetUnits(), UnitType.UT_Length, s,
                new ValueParsingOptions(),
                out var second)) return false;
#endif

            result = second;
            return true;
        }

        #endregion
    }
}
