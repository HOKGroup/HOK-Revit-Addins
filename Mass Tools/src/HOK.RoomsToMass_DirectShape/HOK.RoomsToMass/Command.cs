using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using HOK.RoomsToMass.ToMass;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace HOK.RoomsToMass
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class Command:IExternalCommand
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;
        private Dictionary<string, RevitDocumentProperties> modelDictionary = new Dictionary<string, RevitDocumentProperties>();
        private SourceType selectedSource = SourceType.None;
        private Dictionary<string, RoomProperties> roomDictionary = new Dictionary<string, RoomProperties>();
        private Dictionary<string, AreaProperties> areaDictionary = new Dictionary<string, AreaProperties>();
        private Dictionary<string, FloorProperties> floorDictionary = new Dictionary<string, FloorProperties>();
      
        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;

                GetModelInformation();
                MassSourceWindow sourceWindow = new MassSourceWindow(m_app, modelDictionary);
                if (sourceWindow.ShowDialog() == true)
                {
                    selectedSource = sourceWindow.SelectedSourceType;
                    switch (selectedSource)
                    {
                        case SourceType.Rooms:
                            roomDictionary = sourceWindow.RoomDictionary;
                            RoomWindow roomWindow = new RoomWindow(m_app, modelDictionary, roomDictionary);
                            roomWindow.ShowDialog();
                            break;
                        case SourceType.Areas:
                            areaDictionary = sourceWindow.AreaDictionary;
                            AreaWindow areaWindow = new AreaWindow(m_app, modelDictionary, areaDictionary);
                            areaWindow.ShowDialog();
                            break;
                        case SourceType.Floors:
                            floorDictionary = sourceWindow.FloorDictionary;
                            FloorWindow floorWindow = new FloorWindow(m_app, modelDictionary, floorDictionary);
                            floorWindow.ShowDialog();
                            break;
                    }
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: cannot start Rooms to Mass.\n" + ex.Message, "Rooms to Mass");
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
                    RevitDocumentProperties hostModel = new RevitDocumentProperties(m_doc);
                    modelDictionary.Add(hostModel.DocumentTitle, hostModel);

                    FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                    List<RevitLinkInstance> linkInstances = collector.OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().Cast<RevitLinkInstance>().ToList();
                    foreach (RevitLinkInstance instance in linkInstances)
                    {
                        LinkedInstanceProperties lip = new LinkedInstanceProperties(instance);
                        Document linkedDoc = instance.GetLinkDocument();
                        if (null != linkedDoc)
                        {
                            if (modelDictionary.ContainsKey(linkedDoc.Title))
                            {
                                modelDictionary[linkedDoc.Title].LinkedInstances.Add(lip.InstanceId, lip);
                            }
                            else
                            {
                                RevitDocumentProperties linkedModel = new RevitDocumentProperties(linkedDoc);
                                linkedModel.LinkedInstances.Add(lip.InstanceId, lip);
                                modelDictionary.Add(linkedModel.DocumentTitle, linkedModel);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("The current Revit model hasn't been saved yet.", "Empty Document Title", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get the inforamtion of models loaded into this project.\n"+ex.Message, "Get Model Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

    }

    public class RevitDocumentProperties
    {
        private bool isLinked = false;
        private Document documentObj = null;
        private string documentTitle = "";
        private Dictionary<int, LinkedInstanceProperties> linkedInstances = new Dictionary<int,LinkedInstanceProperties>();

        public bool IsLinked { get { return isLinked; } set { isLinked = value; } }
        public Document DocumentObj { get { return documentObj; } set { documentObj = value; } }
        public string DocumentTitle { get { return documentTitle; } set { documentTitle = value; } }
        public Dictionary<int, LinkedInstanceProperties> LinkedInstances { get { return linkedInstances; } set { linkedInstances = value; } }

        public RevitDocumentProperties(Document doc)
        {
            documentObj = doc;
            documentTitle = doc.Title;
            isLinked = doc.IsLinked;
        }
    }


    public class LinkedInstanceProperties
    {
        private RevitLinkInstance m_instance = null;
        private int instanceId = -1;
        private Autodesk.Revit.DB.Transform transformValue = null;

        public RevitLinkInstance Instance { get { return m_instance; } set { m_instance = value; } }
        public int InstanceId { get { return instanceId; } set { instanceId = value; } }
        public Autodesk.Revit.DB.Transform TransformValue { get { return transformValue; } set { transformValue = value; } }

        public LinkedInstanceProperties(RevitLinkInstance instance)
        {
            m_instance = instance;
            instanceId = instance.Id.IntegerValue;
            
            if (null != instance.GetTotalTransform())
            {
                transformValue = instance.GetTotalTransform();
            }
        }
    }

}
