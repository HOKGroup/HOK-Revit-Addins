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
            get
            {
                return revitSheet;
            }
        }

        public List<Parameter> SheetParameters
        {
            get
            {
                return sheetParameters;
            }
        }

        public Parameter Parameter(string pName)
        {
            if (pName == "")
                return null;
            foreach(Parameter p in sheetParameters)
            {
                if(p.Name.ToUpper() == pName)
                {
                    return p;
                }
            }
            return null;
        }
    }

    public class Tblk
    {
        private List<Parameter> sheetParameters;
        private Element element;

        public Tblk(List<Parameter> parameters, Element e)
        {
            sheetParameters = parameters;
            element = e;
        }

        public Element Element
        {
            get
            {
                return element;
            }
        }
        public List<Parameter> Parameters
        {
            get
            {
                return sheetParameters;
            }
        }
    }
}
