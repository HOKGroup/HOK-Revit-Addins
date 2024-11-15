﻿using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using Nice3point.Revit.Toolkit.External;
using static HOK.Core.Utilities.ElementIdExtension;

namespace HOK.RoomElevation
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class ElevationCommand : ExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;
        private UIDocument uidoc;
        private Dictionary<int, LinkedInstanceProperties> linkedDocuments = new Dictionary<int, LinkedInstanceProperties>();

        public override void Execute()
        {
            try
            {
                m_app = Context.UiApplication;
                m_doc = m_app.ActiveUIDocument.Document;
                uidoc = m_app.ActiveUIDocument;
                Log.AppendLog(LogMessageType.INFO, "Started");

                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                AddinUtilities.PublishAddinLog(new AddinLog("Utilities-RoomElevation", Application.VersionNumber));

                var elevationWindow = new ElevationWindow(m_app);
                if (elevationWindow.CheckPrerequisites())
                {
                    if (elevationWindow.DisplayUI())
                    {
                        var dr = elevationWindow.ShowDialog();
                        if (dr == true)
                        {
                            var toolSettings = elevationWindow.ToolSettings;
                            var linkedInstances = elevationWindow.LinkedDocuments;
                            var roomDictionary = elevationWindow.RoomDictionary;
                            elevationWindow.Close();
                            //pick elements mode
                            var elevationCreator = new ElevationByPickElements(m_app, toolSettings, linkedInstances, roomDictionary);
                            elevationCreator.StartSelection();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }

            Log.AppendLog(LogMessageType.INFO, "Ended");
        }
    }

    public class LinkedInstanceProperties
    {
        public RevitLinkInstance Instance { get; set; }
        public long InstanceId { get; set; }
        public Document LinkedDocument { get; set; }
        public string DocumentTitle { get; set; }
        public Transform TransformValue { get; set; }

        public LinkedInstanceProperties(RevitLinkInstance instance)
        {
            Instance = instance;
            InstanceId = GetElementIdValue(instance.Id);
            LinkedDocument = instance.GetLinkDocument();
            DocumentTitle = LinkedDocument.Title;
            TransformValue = instance.GetTotalTransform();
        }
    }
}
