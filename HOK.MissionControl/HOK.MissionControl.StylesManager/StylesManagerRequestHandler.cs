#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.Messaging;
using HOK.Core.Utilities;
using HOK.Core.WpfUtilities;
using HOK.MissionControl.StylesManager.Utilities;

#endregion

namespace HOK.MissionControl.StylesManager
{
    public class StylesManagerRequestHandler : IExternalEventHandler
    {
        public StylesManagerRequest Request { get; set; } = new StylesManagerRequest();
        public object Arg1 { get; set; }
        public object Arg2 { get; set; }

        public string GetName()
        {
            return "Styles Manager External Event";
        }

        public void Execute(UIApplication app)
        {
            try
            {
                switch (Request.Take())
                {
                    case StylesRequestType.None:
                        {
                            return;
                        }
                    case StylesRequestType.FindView:
                        FindView(app);
                        break;
                    case StylesRequestType.ClearOverrides:
                        ClearOverrides(app);
                        break;
                    case StylesRequestType.ReplaceDimensionTypes:
                        ReplaceAndDeleteDimensionTypes(app);
                        break;
                    case StylesRequestType.ReplaceTextStyles:
                        ReplaceAndDeleteTextStyles(app);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        private void ReplaceAndDeleteDimensionTypes(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;
            var dims = (List<DimensionTypeWrapper>)Arg1;
            var repalcementDim = (DimensionTypeWrapper)Arg2;
            var deleted = new List<DimensionTypeWrapper>();

            using (var trans = new Transaction(doc, "Delete Dimension Types"))
            {
                trans.Start();

                var replace = dims.ToDictionary(x => x.Id, x => x);
                var replacement = (DimensionType)doc.GetElement(repalcementDim.Id);
                var allDims = new FilteredElementCollector(doc)
                    .OfClass(typeof(Dimension))
                    .WhereElementIsNotElementType()
                    .Where(x => replace.ContainsKey(x.GetTypeId()))
                    .Cast<Dimension>()
                    .ToList();

                // (Konrad) We need to first replace all instances that use the Types
                // that will be deleted.
                StatusBarManager.InitializeProgress("Replacing Types...", allDims.Count);
                foreach (var d in allDims)
                {
                    StatusBarManager.StepForward();
                    d.DimensionType = replacement;
                }
                StatusBarManager.FinalizeProgress();

                // (Konrad) We can then safely delete the dimension type
                StatusBarManager.InitializeProgress("Deleting Types...", dims.Count);
                foreach (var dt in dims)
                {
                    try
                    {
                        doc.Delete(dt.Id);
                        deleted.Add(dt);
                    }
                    catch (Exception e)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                    }
                }
                StatusBarManager.FinalizeProgress();

                trans.Commit();
            }

            WeakReferenceMessenger.Default.Send(new DimensionsDeleted { Dimensions = deleted });
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        private void ReplaceAndDeleteTextStyles(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;
            var styles = (List<TextStyleWrapper>)Arg1;
            var repalcementStyle = (TextStyleWrapper)Arg2;
            var deleted = new List<TextStyleWrapper>();

            using (var trans = new Transaction(doc, "Delete Text Styles"))
            {
                trans.Start();

                var replace = styles.ToDictionary(x => x.Id, x => x);
                var replacement = (TextNoteType)doc.GetElement(repalcementStyle.Id);
                var allStyles = new FilteredElementCollector(doc)
                    .OfClass(typeof(TextNote))
                    .WhereElementIsNotElementType()
                    .Where(x => replace.ContainsKey(x.GetTypeId()))
                    .Cast<TextNote>()
                    .ToList();

                // (Konrad) We need to first replace all instances that use the Types
                // that will be deleted.
                StatusBarManager.InitializeProgress("Replacing Styles...", allStyles.Count);
                foreach (var d in allStyles)
                {
                    StatusBarManager.StepForward();
                    d.TextNoteType = replacement;
                }
                StatusBarManager.FinalizeProgress();

                // (Konrad) We can then safely delete the dimension type
                StatusBarManager.InitializeProgress("Deleting Styles...", styles.Count);
                foreach (var ts in styles)
                {
                    try
                    {
                        doc.Delete(ts.Id);
                        deleted.Add(ts);
                    }
                    catch (Exception e)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                    }
                }
                StatusBarManager.FinalizeProgress();

                trans.Commit();
            }

            WeakReferenceMessenger.Default.Send(new TextStylesDeleted { TextStyles = deleted });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        private void ClearOverrides(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;
            var dims = (List<DimensionWrapper>)Arg1;
            var cleared = new List<DimensionWrapper>();
            using (var trans = new Transaction(doc, "Remove Dimension Overrides"))
            {
                trans.Start();
                StatusBarManager.InitializeProgress("Removing Overrides...", dims.Count);

                foreach (var dw in dims)
                {
                    StatusBarManager.StepForward();
                    try
                    {
                        var dim = (Dimension)doc.GetElement(dw.DimensionId);
                        if (dim == null) return;

                        if (dim.NumberOfSegments == 0)
                        {
                            dim.ValueOverride = string.Empty;
                            cleared.Add(dw);
                        }
                        else
                        {
                            foreach (DimensionSegment s in dim.Segments)
                            {
                                if (s.GetHashCode() != dw.Hash) continue;

                                s.ValueOverride = string.Empty;
                                cleared.Add(dw);
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

            WeakReferenceMessenger.Default.Send(new OverridesCleared { Dimensions = cleared });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        private void FindView(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;
            var dw = (DimensionWrapper)Arg1;
            var view = doc.GetElement(dw.OwnerViewId) as View;
            if (view == null) return;

            var dim = doc.GetElement(dw.DimensionId); 
            app.ActiveUIDocument.ActiveView = view;
            app.ActiveUIDocument.ShowElements(dim);
        }
    }

    public class StylesManagerRequest
    {
        private int _request = (int)StylesRequestType.None;

        public StylesRequestType Take()
        {
            return (StylesRequestType)Interlocked.Exchange(ref _request, (int)StylesRequestType.None);
        }

        public void Make(StylesRequestType request)
        {
            Interlocked.Exchange(ref _request, (int)request);
        }
    }

    public enum StylesRequestType
    {
        None,
        FindView,
        ClearOverrides,
        ReplaceDimensionTypes,
        ReplaceTextStyles
    }
}
