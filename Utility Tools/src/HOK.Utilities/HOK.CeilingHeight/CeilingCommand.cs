using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.CeilingHeight
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class CeilingCommand : IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_app = commandData.Application;
            m_doc = m_app.ActiveUIDocument.Document;
            Log.AppendLog(LogMessageType.INFO, "Started");

            // (Konrad) We are gathering information about the addin use. This allows us to
            // better maintain the most used plug-ins or discontiue the unused ones.
            AddinUtilities.PublishAddinLog(new AddinLog("Utilities-CeilingHeight", commandData.Application.Application.VersionNumber));

            try
            {
                var uidoc = m_app.ActiveUIDocument;
                var room = new FilteredElementCollector(m_doc).OfCategory(BuiltInCategory.OST_Rooms).FirstElement() as Room;
#if RELEASE2022 || RELEASE2023
                var parameterRequirements = new Dictionary<string, ForgeTypeId>
                {
                    {"Ceiling Height", SpecTypeId.Length},
                    {"Secondary Ceiling Heights", SpecTypeId.String.Text},
                    {"Ceiling Plan", SpecTypeId.String.Text},
                    {"Ceiling Type", SpecTypeId.String.Text}
                };
#else
                var parameterRequirements = new Dictionary<string, ParameterType>
                {
                    {"Ceiling Height", ParameterType.Length},
                    {"Secondary Ceiling Heights", ParameterType.Text},
                    {"Ceiling Plan", ParameterType.Text},
                    {"Ceiling Type", ParameterType.Text}
                };
#endif
                var notFoundParams = new StringBuilder();
                foreach (var paramName in parameterRequirements.Keys)
                {
                    var paramType = parameterRequirements[paramName];
                    if (!FindParameter(room, paramName, paramType))
                    {
                        notFoundParams.AppendLine("[" + paramName + " : " + paramType + "] ");
                    }
                }

                if (notFoundParams.Length > 0)
                {
                    var dresult = MessageBox.Show("The following parameters are required in Room elements.\n" + notFoundParams + "\nThe tool will create the parameters in your current shared parameter file.  Would you like to proceed?",
                        "Parameters Required", 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Question);

                    if (dresult == DialogResult.Yes)
                    {
                        var dr = MessageBox.Show("Start selecting rooms to create floors and click Finish on the options bar.The windowed area will filter out Room category only.", 
                            "Select Rooms",
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Information);

                        if (dr == DialogResult.OK)
                        {
                            var selectedElement = uidoc.Selection.PickObjects(ObjectType.Element, new RoomElementFilter(), "Select rooms to measure the height from floors to ceiling.");
                            var selectedRooms = new List<Element>();
                            foreach (var reference in selectedElement)
                            {
                                var element = m_doc.GetElement(reference.ElementId);
                                if (null != element)
                                {
                                    selectedRooms.Add(element);
                                }
                            }

                            if (selectedRooms.Count > 0)
                            {
                                var util = new CeilingHeightUtil(m_app, selectedRooms);
                                util.MeasureHeight();
                            }
                        }
                    }
                }
                else
                {
                    var dr = MessageBox.Show("Start selecting rooms to create floors and click Finish on the options bar.The windowed area will filter out Room category only.", 
                        "Select Rooms", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Information);

                    if (dr == DialogResult.OK)
                    {
                        var selectedElement = uidoc.Selection.PickObjects(ObjectType.Element, new RoomElementFilter(), "Select rooms to measure the height from floors to ceiling.");
                        var selectedRooms = new List<Element>();
                        foreach (var reference in selectedElement)
                        {
                            var element = m_doc.GetElement(reference.ElementId);
                            if (null != element)
                            {
                                selectedRooms.Add(element);
                            }
                        }

                        if (selectedRooms.Count > 0)
                        {
                            var util = new CeilingHeightUtil(m_app, selectedRooms);
                            util.MeasureHeight();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                return Result.Failed;
            }

            Log.AppendLog(LogMessageType.INFO, "Ended");
            return Result.Succeeded;
        }

#if RELEASE2022 || RELEASE2023
        private static bool FindParameter(Room room, string paramName, ForgeTypeId paramType)
        {
#else
        private static bool FindParameter(Room room, string paramName, ParameterType paramType)
        {
#endif
            try
            {
                var param = room.LookupParameter(paramName);
#if RELEASE2022 || RELEASE2023
                return param?.Definition.GetDataType() == paramType;
#else
                return param?.Definition.ParameterType == paramType;
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find parameters.\n" + ex.Message, "Find Parameters", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return false;
        }
    }
}
