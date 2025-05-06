using System.IO;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;

namespace HOK.ProjectSheetManager.Classes
{
    public class Settings
    {
        private ExternalCommandData commandData;
        private string iniFileName = "SheetMaker.ini";
        private string iniPath;
        private string accessFilePath;
        private string excelFilePath;
        private UIApplication uiApp;
        private Document doc;
        private List<Autodesk.Revit.DB.View> views;
        private List<ViewSheet> sheets;
    }
}
