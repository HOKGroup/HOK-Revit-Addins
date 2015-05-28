using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.UI;

namespace HOK.JsonExporter
{
    class ImportCommand:IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to import JSON to Revit.\n"+ex.Message, "Import JSON", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return Result.Succeeded;
        }
    }
}
