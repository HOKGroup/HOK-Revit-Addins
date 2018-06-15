using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB;

namespace HOK.MissionControl.StylesManager.Tabs
{
    public class DimensionOverridesModel
    {
        private Document _doc { get; set; }

        public DimensionOverridesModel(Document doc)
        {
            _doc = doc;
        }

        private bool EvaluateValueOverride(string value)
        {
            var units = _doc.DisplayUnitSystem;

            var result = false;
            switch (units)
            {
                case DisplayUnit.METRIC:
                    break;
                case DisplayUnit.IMPERIAL:
                    var s = string.Empty;
                    var match = Regex.Match(value, @"^[0-9\'\-\/\""\s]*", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        s = match.Value.Trim();
                    }

                    if (!string.IsNullOrEmpty(s))
                    {
                        var verify = Regex.Match(s, @"^[0-9\'\-\/\""\s]*$", RegexOptions.IgnoreCase);
                        if (verify.Success)
                        {
                            // we have a winner string
                        }
                    }

                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }

        /// <summary>
        /// Replace invalid characters with empty strings.
        /// </summary>
        /// <param name="strIn"></param>
        /// <returns></returns>
        private static string CleanInput(string strIn)
        {
            try
            {
                return Regex.Replace(strIn, @"[^0-9\'\-\/\""\s]+", "",
                    RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            catch (RegexMatchTimeoutException)
            {
                return string.Empty;
            }
        }

        public ObservableCollection<DimensionWrapper> CollectDimensionOverrides()
        {
            var dInstances = new FilteredElementCollector(_doc)
                .OfClass(typeof(Dimension))
                .WhereElementIsNotElementType()
                .Cast<Dimension>();

            var dims = new List<DimensionWrapper>();
            foreach (var d in dInstances)
            {
                if (string.IsNullOrEmpty(d.ValueOverride)) continue;
                if (d.Segments.Size == 0)
                {
                    // dim w/ zero segments
                    dims.Add(new DimensionWrapper(d)
                    {
                        DimensionId = d.Id,
                        OwnerViewType = d.ViewSpecific
                            ? ((View)_doc.GetElement(d.OwnerViewId)).ViewType.ToString()
                            : string.Empty,
                        OwnerViewId = d.OwnerViewId
                    });
                    var test = EvaluateValueOverride(d.ValueOverride);
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
                            OwnerViewId = d.OwnerViewId
                        });

                        var test = EvaluateValueOverride(s.ValueOverride);
                    }
                }
            }

            return new ObservableCollection<DimensionWrapper>(dims);
        }
    }
}
