using System;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.Messaging;
using HOK.Core.Utilities;
using HOK.MissionControl.GroupsManager.Utilities;
using Nice3point.Revit.Toolkit.External;

namespace HOK.MissionControl.GroupsManager
{
    public class AppCommand : ExternalApplication
    {
        private const string tabName = "   HOK   ";
        public static GroupManagerRequestHandler GroupManagerHandler { get; set; }
        public static ExternalEvent GroupManagerEvent { get; set; }

        public override void OnStartup()
        {
            try
            {
                Application.CreateRibbonTab(tabName);
            }
            catch
            {
                Log.AppendLog(LogMessageType.INFO, "Ribbon tab was not created because it already exists: " + tabName);
            }
            var assembly = Assembly.GetAssembly(GetType());
            var panel = Application.GetRibbonPanels(tabName).FirstOrDefault(x => x.Name == "Mission Control")
                        ?? Application.CreateRibbonPanel(tabName, "Mission Control");
            var unused = (PushButton)panel.AddItem(new PushButtonData("GroupManager_Command", "  Groups  " + Environment.NewLine + "Manager",
                assembly.Location, "HOK.MissionControl.GroupsManager.GroupsManagerCommand")
            {
                LargeImage = ButtonUtil.LoadBitmapImage(assembly, "HOK.MissionControl.GroupsManager", Properties.Resources.GroupsManager_ImageName),
                ToolTip = Properties.Resources.GroupsManager_Description
            });

            Application.ControlledApplication.DocumentChanged += OnDocumentChanged;

            GroupManagerHandler = new GroupManagerRequestHandler();
            GroupManagerEvent = ExternalEvent.Create(GroupManagerHandler);

        }

        private void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new DocumentChanged(e.GetDeletedElementIds(), e.GetAddedElementIds(), e.GetDocument()));
        }

        public override void OnShutdown()
        {
            Application.ControlledApplication.DocumentChanged -= OnDocumentChanged;
        }
    }
}
