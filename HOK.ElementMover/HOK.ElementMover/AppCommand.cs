using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using Nice3point.Revit.Toolkit.External;


namespace HOK.ElementMover
{
    public class AppCommand : ExternalApplication
    {
        internal static AppCommand thisApp;
        private ControlledApplication ctrApp;
        private MainWindow mainWindow;
        private MoverHandler handler;
        private const string tabName = "   HOK   ";

        public override void OnStartup()
        {
            thisApp = this;
            mainWindow = null;
            ctrApp = Application.ControlledApplication;

            try
            {
                Application.CreateRibbonTab(tabName);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }

            var created = Application.GetRibbonPanels(tabName).FirstOrDefault(x => x.Name == "Customizations");
            var panel = created ?? Application.CreateRibbonPanel(tabName, "Customizations");

            var currentAssembly = Assembly.GetAssembly(GetType());
            var moverImage = ButtonUtil.LoadBitmapImage(currentAssembly, typeof(AppCommand).Namespace, "elementMover_32.png");

            var moverButton = (PushButton)panel.AddItem(new PushButtonData("ElementMoverCommand", 
                "Element" + Environment.NewLine + "Mover", 
                currentAssembly.Location, 
                "HOK.ElementMover.MoverCommand"));

            moverButton.LargeImage = moverImage;

            const string instructionFile = @"V:\RVT-Data\HOK Program\Documentation\Element Mover_Instruction.pdf";
            if (File.Exists(instructionFile))
            {
                var contextualHelp = new ContextualHelp(ContextualHelpType.Url, instructionFile);
                moverButton.SetContextualHelp(contextualHelp);
            }

            ctrApp.DocumentChanged += CtrApp_DocumentChanged;
        }

        public override void OnShutdown()
        {
            ctrApp.DocumentChanged -= CtrApp_DocumentChanged;
        }

        public void ShowMover(UIApplication uiapp)
        {
            if (mainWindow == null)
            {
                handler = new MoverHandler(uiapp);
                var exEvent = ExternalEvent.Create(handler);

                mainWindow = new MainWindow(exEvent, handler);
                mainWindow.Closed += WindowClosed;
                mainWindow.Show();
            }
        }

        public void WindowClosed(object sender, EventArgs e)
        {
            mainWindow = null;
        }

        private void CtrApp_DocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs args)
        {
            try
            {
                if (null != mainWindow && null != handler)
                {
                    var doc = args.GetDocument();
                    var modifiedElementIds = args.GetModifiedElementIds().ToList();
                    if (modifiedElementIds.Count > 0)
                    {
                        //bool updated = UpdateModifiedElements(doc, modifiedElementIds);
                    }
                    var deletedElementIds = args.GetDeletedElementIds().ToList();
                    if (deletedElementIds.Count > 0)
                    {
                        var updated = UpdateDeletedElements(doc, deletedElementIds);
                    }
                }
            
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private bool UpdateModifiedElements(Document doc, List<ElementId> modifiedElementIds)
        {
            var updated = false;
            try
            {
                var linkedInstances = handler.LinkInstances;
                foreach (var elementId in modifiedElementIds)
                {
                    var element = doc.GetElement(elementId);
                    if (element.GetEntitySchemaGuids().Count > 0)
                    {
                        var linkInfo = MoverDataStorageUtil.GetLinkedElementInfo(element);
                        if (null != linkInfo)
                        {
                            if (linkedInstances.ContainsKey(linkInfo.SourceLinkInstanceId))
                            {
                                var lip = linkedInstances[linkInfo.SourceLinkInstanceId];
                                if (lip.LinkedElements.ContainsKey(linkInfo.LinkedElementId))
                                {
                                    var sourceElement = lip.LinkedDocument.GetElement(linkInfo.SourceElementId);
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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return updated;
        }

        private bool UpdateDeletedElements(Document doc, List<ElementId> deletedElementIds)
        {
            var updated = false;
            try
            {
                var linkedInstances = handler.LinkInstances;
                var removed = false;

                foreach (var deletedId in deletedElementIds)
                {
                    var instanceIds = linkedInstances.Keys.ToList();
                    foreach (var instanceId in instanceIds)
                    {
                        var lip = linkedInstances[instanceId];
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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return updated;
        }

        private XYZ GetPointLocation(Element element)
        {
            XYZ point = null;
            try
            {
                var location = element.Location;
                if (location is LocationPoint)
                {
                    var locationPt = location as LocationPoint;
                    point = locationPt.Point;
                }
                else if (location is LocationCurve)
                {
                    var locationCurve = location as LocationCurve;
                    point = locationCurve.Curve.Evaluate(0.5, true);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
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
