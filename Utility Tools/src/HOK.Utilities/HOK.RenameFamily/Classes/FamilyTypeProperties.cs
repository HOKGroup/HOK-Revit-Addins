using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.RenameFamily.Classes
{
    public class FamilyTypeProperties : INotifyPropertyChanged
    {
        private ElementId familyTypeId = ElementId.InvalidElementId;
        private int familyTypeIdInt = -1;
        private string modelName = "";
        private string categoryName = "";
        private string familyName = ""; //from Excel
        private string typeName = ""; //from Excel
        private string currentFamilyName = ""; //from Revit
        private string currentTypeName = ""; // from Revit
        private string toolTip = "";

        private bool isSelected = false;
        private bool isLinked = false;

        public ElementId FamilyTypeId { get { return familyTypeId; } set { familyTypeId = value; NotifyPropertyChanged("FamilyTypeId"); } }
        public int FamilyTypeIdInt { get { return familyTypeIdInt; } set { familyTypeIdInt = value; NotifyPropertyChanged("FamilyTypeIdInt"); } }
        public string ModelName { get { return modelName; } set { modelName = value; NotifyPropertyChanged("ModelName"); } }
        public string CategoryName { get { return categoryName; } set { categoryName = value; NotifyPropertyChanged("CategoryName"); } }
        public string FamilyName { get { return familyName; } set { familyName = value; NotifyPropertyChanged("FamilyName"); } }
        public string TypeName { get { return typeName; } set { typeName = value; NotifyPropertyChanged("TypeName"); } }
        public string CurrentFamilyName { get { return currentFamilyName; } set { currentFamilyName = value; NotifyPropertyChanged("CurrentFamilyName"); } }
        public string CurrentTypeName { get { return currentTypeName; } set { currentTypeName = value; NotifyPropertyChanged("CurrentTypeName"); } }
        public string ToolTip { get { return toolTip; } set { toolTip = value; NotifyPropertyChanged("ToolTip"); } }

        public bool IsSelected { get { return isSelected; } set { isSelected = value; NotifyPropertyChanged("IsSelected"); } }
        public bool IsLinked { get { return isLinked; } set { isLinked = value; NotifyPropertyChanged("IsLinked"); } }

        public FamilyTypeProperties(string model, int typeId, string fName, string tName)
        {
            modelName = model;
            familyTypeIdInt = typeId;
            familyName = fName;
            typeName = tName;
        }

        public void SetCurrentFamily(FamilySymbol symbol)
        {
            familyTypeId = symbol.Id;
            if (!string.IsNullOrEmpty(symbol.Category.Name))
            {
                categoryName = symbol.Category.Name;
            }
            currentFamilyName = symbol.Family.Name;
            currentTypeName = symbol.Name;

            if (familyName == currentFamilyName && typeName == currentTypeName)
            {
                isLinked = true;
            }

            toolTip = "Current Family Name: " + currentFamilyName + ", Current Tyle Name: " + currentTypeName;

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
