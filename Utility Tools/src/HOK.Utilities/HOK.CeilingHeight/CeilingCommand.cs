using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace HOK.CeilingHeight
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class CeilingCommand : IExternalCommand
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            m_app = commandData.Application;
            m_doc = m_app.ActiveUIDocument.Document;
            
            try
            {
                UIDocument uidoc = m_app.ActiveUIDocument;

                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                Room room = collector.OfCategory(BuiltInCategory.OST_Rooms).ToElements().ToList().First() as Room;

                Dictionary<string, ParameterType> parameterRequirements = new Dictionary<string, ParameterType>();
                parameterRequirements.Add("Ceiling Height", ParameterType.Length);
                parameterRequirements.Add("Secondary Ceiling Heights", ParameterType.Text);
                parameterRequirements.Add("Ceiling Plan", ParameterType.Text);
                parameterRequirements.Add("Ceiling Type", ParameterType.Text);

                StringBuilder notFoundParams = new StringBuilder();
                foreach (string paramName in parameterRequirements.Keys)
                {
                    ParameterType paramType = parameterRequirements[paramName];
                    if (!FindParameter(room, paramName, paramType))
                    {
                        notFoundParams.AppendLine("[" + paramName + " : " + paramType.ToString() + "] ");
                    }
                }

                if (notFoundParams.Length > 0)
                {
                    DialogResult dresult = MessageBox.Show("The following parameters are required in Room elements.\n" + notFoundParams.ToString() + "\nThe tool will create the parameters in your current shared parameter file.  Would you like to proceed?", "Parameters Required", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dresult == DialogResult.Yes)
                    {
                        DialogResult dr = MessageBox.Show("Start selecting rooms to create floors and click Finish on the options bar.The windowed area will filter out Room category only.", "Select Rooms", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if (dr == DialogResult.OK)
                        {
                            IList<Reference> selectedElement = uidoc.Selection.PickObjects(ObjectType.Element, new RoomElementFilter(), "Select rooms to measure the height from floors to ceiling.");
                            List<Element> selectedRooms = new List<Element>();
                            foreach (Reference reference in selectedElement)
                            {
                                Element element = m_doc.GetElement(reference.ElementId);
                                if (null != element)
                                {
                                    selectedRooms.Add(element);
                                }
                            }

                            if (selectedRooms.Count > 0)
                            {
                                CeilingHeightUtil util = new CeilingHeightUtil(m_app, selectedRooms);
                                util.MeasureHeight();
                            }
                        }
                    }
                }
                else
                {
                    DialogResult dr = MessageBox.Show("Start selecting rooms to create floors and click Finish on the options bar.The windowed area will filter out Room category only.", "Select Rooms", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (dr == DialogResult.OK)
                    {
                        IList<Reference> selectedElement = uidoc.Selection.PickObjects(ObjectType.Element, new RoomElementFilter(), "Select rooms to measure the height from floors to ceiling.");
                        List<Element> selectedRooms = new List<Element>();
                        foreach (Reference reference in selectedElement)
                        {
                            Element element = m_doc.GetElement(reference.ElementId);
                            if (null != element)
                            {
                                selectedRooms.Add(element);
                            }
                        }

                        if (selectedRooms.Count > 0)
                        {
                            CeilingHeightUtil util = new CeilingHeightUtil(m_app, selectedRooms);
                            util.MeasureHeight();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                string exMessage = ex.Message;
                return Result.Failed;
            }
            return Result.Succeeded;
        }

        private bool FindParameter(Room room, string paramName, ParameterType paramType)
        {
            bool result = false;
            try
            {
#if RELEASE2015||RELEASE2016 || RELEASE2017
                Parameter param = room.LookupParameter(paramName);
#else
                Parameter param = room.get_Parameter(paramName);
#endif

                if (null != param)
                {
                    if (param.Definition.ParameterType == paramType)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find parameters.\n" + ex.Message, "Find Parameters", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }
    }

    public class RoomElementFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (null != elem.Category)
            {
                if (elem.Category.Name == "Rooms")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}
