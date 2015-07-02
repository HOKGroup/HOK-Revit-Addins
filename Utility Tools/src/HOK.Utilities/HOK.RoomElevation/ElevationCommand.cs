using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace HOK.RoomElevation
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class ElevationCommand : IExternalCommand
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;
        private UIDocument uidoc;
        private Dictionary<int, LinkedInstanceProperties> linkedDocuments = new Dictionary<int, LinkedInstanceProperties>();

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;
                uidoc = m_app.ActiveUIDocument;

                ElevationWindow elevationWindow = new ElevationWindow(m_app);
                if (elevationWindow.CheckPrerequisites())
                {
                    if (elevationWindow.DisplayUI())
                    {
                        bool? dr = elevationWindow.ShowDialog();
                        if (dr == true)
                        {
                            ElevationCreatorSettings toolSettings = elevationWindow.ToolSettings;
                            Dictionary<int, LinkedInstanceProperties> linkedInstances = elevationWindow.LinkedDocuments;
                            Dictionary<int, RoomElevationProperties> roomDictionary = elevationWindow.RoomDictionary;
                            elevationWindow.Close();
                            //pick elements mode
                            ElevationByPickElements elevationCreator = new ElevationByPickElements(m_app, toolSettings, linkedInstances, roomDictionary);
                            elevationCreator.StartSelection();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Initialization Problem:\n" + ex.Message, "Room Elevation Creator", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return Result.Succeeded;
        }

    }

    public class LinkedInstanceProperties
    {
        private RevitLinkInstance m_instance = null;
        private int instanceId = -1;
        private Document linkedDocument = null;
        private string documentTitle = "";
        private Autodesk.Revit.DB.Transform transformValue = null;

        public RevitLinkInstance Instance { get { return m_instance; } set { m_instance = value; } }
        public int InstanceId { get { return instanceId; } set { instanceId = value; } }
        public Document LinkedDocument { get { return linkedDocument; } set { linkedDocument = value; } }
        public string DocumentTitle { get { return documentTitle; } set { documentTitle = value; } }
        public Autodesk.Revit.DB.Transform TransformValue { get { return transformValue; } set { transformValue = value; } }

        public LinkedInstanceProperties(RevitLinkInstance instance)
        {
            m_instance = instance;
            instanceId = instance.Id.IntegerValue;
#if RELEASE2013
            linkedDocument = instance.Document;
#elif RELEASE2014 || RELEASE2015 || RELEASE2016
            linkedDocument = instance.GetLinkDocument();
#endif
            documentTitle = linkedDocument.Title;
            transformValue = instance.GetTotalTransform();
        }
    }
}
