using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace HOK.ElementMover
{
    public class AppCommand:IExternalApplication
    {
        internal static AppCommand thisApp = null;
        private ControlledApplication ctrApp = null;
        private MainWindow mainWindow;
        private MoverHandler handler = null;
        private string tabName = "  HOK - Beta";

        public Result OnShutdown(UIControlledApplication application)
        {
            ctrApp.DocumentChanged -= CtrApp_DocumentChanged;
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            thisApp = this;
            mainWindow = null;
            ctrApp = application.ControlledApplication;

            try { application.CreateRibbonTab(tabName); }
            catch { }
            
            RibbonPanel panel = application.CreateRibbonPanel(tabName,"Linked Elements");
            string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

            BitmapSource moverImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.mover.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            PushButton moverButton = panel.AddItem(new PushButtonData("ElementMover", "Element Mover", currentAssembly, "HOK.ElementMover.MoverCommand")) as PushButton;
            moverButton.LargeImage = moverImage;

            string instructionFile = @"V:\RVT-Data\HOK Program\Documentation\Element Mover_Instruction.pdf";
            if (File.Exists(instructionFile))
            {
                ContextualHelp contextualHelp = new ContextualHelp(ContextualHelpType.Url, instructionFile);
                moverButton.SetContextualHelp(contextualHelp);
            }

            ctrApp.DocumentChanged += new EventHandler<Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(CtrApp_DocumentChanged);
            return Result.Succeeded;
        }

        public void ShowMover(UIApplication uiapp)
        {
            if (mainWindow == null)
            {
                
                handler = new MoverHandler(uiapp);
                ExternalEvent exEvent = ExternalEvent.Create(handler);

                mainWindow = new MainWindow(exEvent, handler);
                mainWindow.Closed += WindowClosed;
                mainWindow.Show();
            }
        }

        public void WindowClosed(object sender, System.EventArgs e)
        {
            mainWindow = null;
        }

        private void CtrApp_DocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs args)
        {
            try
            {
                if (null != mainWindow && null != handler)
                {
                    Document doc = args.GetDocument();
                    List<ElementId> modifiedElementIds = args.GetModifiedElementIds().ToList();
                    if (modifiedElementIds.Count > 0)
                    {
                        //bool updated = UpdateModifiedElements(doc, modifiedElementIds);
                    }
                    List<ElementId> deletedElementIds = args.GetDeletedElementIds().ToList();
                    if (deletedElementIds.Count > 0)
                    {
                        bool updated = UpdateDeletedElements(doc, deletedElementIds);
                    }
                }
            
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private bool UpdateModifiedElements(Document doc, List<ElementId> modifiedElementIds)
        {
            bool updated = false;
            try
            {
                Dictionary<ElementId, LinkedInstanceProperties> linkedInstances = handler.LinkInstances;
                foreach (ElementId elementId in modifiedElementIds)
                {
                    Element element = doc.GetElement(elementId);
                    if (element.GetEntitySchemaGuids().Count > 0)
                    {
                        LinkedElementInfo linkInfo = MoverDataStorageUtil.GetLinkedElementInfo(element);
                        if (null != linkInfo)
                        {
                            if (linkedInstances.ContainsKey(linkInfo.SourceLinkInstanceId))
                            {
                                LinkedInstanceProperties lip = linkedInstances[linkInfo.SourceLinkInstanceId];
                                if (lip.LinkedElements.ContainsKey(linkInfo.LinkedElementId))
                                {
                                    Element sourceElement = lip.LinkedDocument.GetElement(linkInfo.SourceElementId);
                                    if (null != sourceElement)
                                    {
                                        linkInfo.Matched = LinkedElementInfo.CompareLocation(sourceElement, element, lip.TransformValue);
                                        lip.LinkedElements.Remove(linkInfo.LinkedElementId);
                                        lip.LinkedElements.Add(linkInfo.LinkedElementId, linkInfo);

                                        linkedInstances.Remove(lip.InstanceId);
                                        linkedInstances.Add(lip.InstanceId, lip);
                                    }
                                }
                            }
                        }
                    }
                }

                handler.LinkInstances = linkedInstances;
                if (linkedInstances.ContainsKey(handler.SelectedLink.InstanceId))
                {
                    handler.SelectedLink = linkedInstances[handler.SelectedLink.InstanceId];
                }
                handler.ApplyDocumentChanged();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return updated;
        }

        private bool UpdateDeletedElements(Document doc, List<ElementId> deletedElementIds)
        {
            bool updated = false;
            try
            {
                Dictionary<ElementId, LinkedInstanceProperties> linkedInstances = handler.LinkInstances;
                bool removed = false;

                foreach (ElementId deletedId in deletedElementIds)
                {
                    List<ElementId> instanceIds = linkedInstances.Keys.ToList();
                    foreach (ElementId instanceId in instanceIds)
                    {
                        LinkedInstanceProperties lip = linkedInstances[instanceId];
                        if (lip.LinkedElements.ContainsKey(deletedId))
                        {
                            lip.LinkedElements.Remove(deletedId);
                            linkedInstances.Remove(lip.InstanceId);
                            linkedInstances.Add(lip.InstanceId, lip);
                            removed = true;
                        }
                    }
                }

                if (removed)
                {
                    handler.LinkInstances = linkedInstances;
                    if (linkedInstances.ContainsKey(handler.SelectedLink.InstanceId))
                    {
                        handler.SelectedLink = linkedInstances[handler.SelectedLink.InstanceId];
                    }
                    handler.ApplyDocumentChanged();
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return updated;
        }

        private XYZ GetPointLocation(Element element)
        {
            XYZ point = null;
            try
            {
                Location location = element.Location;
                if (location is LocationPoint)
                {
                    LocationPoint locationPt = location as LocationPoint;
                    point = locationPt.Point;
                }
                else if (location is LocationCurve)
                {
                    LocationCurve locationCurve = location as LocationCurve;
                    point = locationCurve.Curve.Evaluate(0.5, true);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return point;
        }

    }

    public static class AbortFlag
    {
        private static bool abortFlag = false;

        public static bool GetAbortFlag()
        {
            return abortFlag;
        }

        public static void SetAbortFlag(bool abort)
        {
            abortFlag = abort;
        }
    }
}
