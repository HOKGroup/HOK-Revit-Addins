using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.RoomsToMass.ToMass;

namespace HOK.RoomsToMass
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class Command : IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;
        private readonly Dictionary<string, RevitDocumentProperties> modelDictionary = new Dictionary<string, RevitDocumentProperties>();
        private SourceType selectedSource = SourceType.None;
        private Dictionary<string, RoomProperties> roomDictionary = new Dictionary<string, RoomProperties>();
        private Dictionary<string, AreaProperties> areaDictionary = new Dictionary<string, AreaProperties>();
        private Dictionary<string, FloorProperties> floorDictionary = new Dictionary<string, FloorProperties>();
      
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;
                Log.AppendLog(LogMessageType.INFO, "Started");

                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                AddinUtilities.PublishAddinLog(new AddinLog("MassTools-CreateMass", m_app.Application.VersionNumber));

                GetModelInformation();
                var sourceWindow = new MassSourceWindow(m_app, modelDictionary);
                if (sourceWindow.ShowDialog() == true)
                {
                    selectedSource = sourceWindow.SelectedSourceType;
                    switch (selectedSource)
                    {
                        case SourceType.Rooms:
                            roomDictionary = sourceWindow.RoomDictionary;
                            var roomWindow = new RoomWindow(m_app, modelDictionary, roomDictionary);
                            roomWindow.ShowDialog();
                            break;
                        case SourceType.Areas:
                            areaDictionary = sourceWindow.AreaDictionary;
                            var areaWindow = new AreaWindow(m_app, modelDictionary, areaDictionary);
                            areaWindow.ShowDialog();
                            break;
                        case SourceType.Floors:
                            floorDictionary = sourceWindow.FloorDictionary;
                            var floorWindow = new FloorWindow(m_app, modelDictionary, floorDictionary);
                            floorWindow.ShowDialog();
                            break;
                    }
                }

                Log.AppendLog(LogMessageType.INFO, "Ended.");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                return Result.Failed;
            }
        }

        private void GetModelInformation()
        {
            try
            {
                //host project
                if (!string.IsNullOrEmpty(m_doc.Title))
                {
                    var hostModel = new RevitDocumentProperties(m_doc);
                    modelDictionary.Add(hostModel.DocumentTitle, hostModel);

                    var collector = new FilteredElementCollector(m_doc);
                    var linkInstances = collector.OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().Cast<RevitLinkInstance>().ToList();
                    foreach (var instance in linkInstances)
                    {
                        var lip = new LinkedInstanceProperties(instance);
                        var linkedDoc = instance.GetLinkDocument();
                        if (null != linkedDoc)
                        {
                            if (modelDictionary.ContainsKey(linkedDoc.Title))
                            {
                                modelDictionary[linkedDoc.Title].LinkedInstances.Add(lip.InstanceId, lip);
                            }
                            else
                            {
                                var linkedModel = new RevitDocumentProperties(linkedDoc);
                                linkedModel.LinkedInstances.Add(lip.InstanceId, lip);
                                modelDictionary.Add(linkedModel.DocumentTitle, linkedModel);
                            }
                        }
                    }
                }
                else
                {
                    Log.AppendLog(LogMessageType.ERROR, "The current Revit model hasn't been saved yet.");
                    MessageBox.Show("The current Revit model hasn't been saved yet.", "Empty Document Title", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }

    public class RevitDocumentProperties
    {
        public bool IsLinked { get; set; }
        public Document DocumentObj { get; set; }
        public string DocumentTitle { get; set; }
        public Dictionary<int, LinkedInstanceProperties> LinkedInstances { get; set; } = new Dictionary<int,LinkedInstanceProperties>();

        public RevitDocumentProperties(Document doc)
        {
            DocumentObj = doc;
            DocumentTitle = doc.Title;
            IsLinked = doc.IsLinked;
        }
    }


    public class LinkedInstanceProperties
    {
        public RevitLinkInstance Instance { get; set; }
        public int InstanceId { get; set; }
        public Transform TransformValue { get; set; }

        public LinkedInstanceProperties(RevitLinkInstance instance)
        {
            Instance = instance;
            InstanceId = instance.Id.IntegerValue;
            
            if (null != instance.GetTotalTransform())
            {
                TransformValue = instance.GetTotalTransform();
            }
        }
    }
}
