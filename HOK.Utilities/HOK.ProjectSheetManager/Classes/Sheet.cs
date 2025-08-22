using Autodesk.Revit.DB;

namespace HOK.ProjectSheetManager.Classes
{
    public class Sheet
    {
        private List<Parameter> sheetParameters;
        private ViewSheet revitSheet;

        public Sheet(List<Parameter> parameters, ViewSheet sheet)
        {
            sheetParameters = parameters;
            revitSheet = sheet;
        }

        public ViewSheet RevitSheet
        {
            get { return revitSheet; }
        }
    }

    public class Titleblock
    {
        private List<Parameter> sheetParameters;
        private Element element;

        public Titleblock(List<Parameter> parameters, Element e)
        {
            sheetParameters = parameters;
            element = e;
        }

        public Element Element
        {
            get { return element; }
        }
    }
}
