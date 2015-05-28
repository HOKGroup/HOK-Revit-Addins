using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace HOK.JsonExporter
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class ExportCommand:IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;

                View3D view = m_doc.ActiveView as View3D;
                if (null != view)
                {
                    Selection selection = m_app.ActiveUIDocument.Selection;
                    Reference reference = selection.PickObject(ObjectType.Face);
                    string jsonFileName = SaveJsonFile();

                    if (!string.IsNullOrEmpty(jsonFileName) && null != reference)
                    {
                        JsonExporter exporter = new JsonExporter(m_doc, jsonFileName, reference);
                        bool exported = exporter.ExportJSON();
                        if (exported)
                        {
                            MessageBox.Show("AVF result data has been exported.\n"+jsonFileName , "Export to JSON", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("3D view should be an active view to export.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to export 3d view to JSON.\n"+ex.Message, "Export to JSON", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return Result.Succeeded;
        }


        private string SaveJsonFile()
        {
            string jsonFileName = "";
            try
            {
                if (!string.IsNullOrEmpty(m_doc.Title))
                {
                    jsonFileName = m_doc.Title.Replace(".rvt", ".js");
                }
                if (!string.IsNullOrEmpty(jsonFileName))
                {
                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.Title = "Select JSON Output File";
                    dlg.Filter = "JSON files|*.js";

                    string directory = Path.GetDirectoryName(m_doc.PathName);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        dlg.InitialDirectory = directory;
                    }
                    dlg.FileName = jsonFileName;

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        jsonFileName = dlg.FileName;
                    }
                }
                else
                {
                    MessageBox.Show("Please save the Revit project file first", "Save File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save JSON file.\n"+ex.Message, "Save JSON File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return jsonFileName;
        }
    }
}
