using Autodesk.Revit.DB;

namespace HOK.ProjectSheetManager.Classes
{
    public class Parameter
    {
        private Autodesk.Revit.DB.Parameter RevitParameter;
        private string PropName;
        private string PropValue;

        public Parameter(Autodesk.Revit.DB.Parameter Parameter)
        {
            RevitParameter = Parameter;
        }

        public Parameter(string PropertyName, string PropertyValue)
        {
            PropName = PropertyName;
            PropValue = PropertyValue;
        }

        public string DataSourceName
        {
            get
            {
                return PropName;
            }
        }

        public string DataSourceValue
        {
            get
            {
                return PropValue;
            }
        }

        public string DisplayUnitType
        {
            get
            {
                try
                {
                    return RevitParameter.Definition.GetType().ToString();
                }
                catch
                {
                    return "";
                }
            }
        }

        public Autodesk.Revit.DB.Parameter ParameterObject
        {
            get
            {
                try
                {
                    return RevitParameter;
                }
                catch
                {
                    return null;
                }
            }
        }

        public bool ParameterIsReadOnly
        {
            get
            {
                try
                {
                    return RevitParameter.IsReadOnly;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool ParameterIsShared
        {
            get
            {
                try
                {
                    return RevitParameter.IsShared;
                }
                catch
                {
                    return false;
                }
            }
        }

        public string Type
        {
            get
            {
                try
                {
                    return RevitParameter.GetType().Name;
                }
                catch
                {
                    return "";
                }
            }
        }
        public string Name
        {
            get
            {
                try
                {
                    return RevitParameter.Definition.Name;
                }
                catch
                {
                    return "";
                }
            }
        }
        public string Format
        {
            get
            {
                try
                {
                    return RevitParameter.StorageType.ToString();
                }
                catch
                {
                    return "";
                }
            }
        }
        public string Value
        {
            get
            {
                try
                {
                    return GetParameterValue(RevitParameter);
                }
                catch
                {
                    return "";
                }
            }
        }
        public string GetParameterValue(Autodesk.Revit.DB.Parameter parameter)
        {
            switch(parameter.StorageType)
            {
                case StorageType.Double:
                    return parameter.AsDouble().ToString();
                case StorageType.ElementId:
                    return parameter.AsElementId().ToString();
                case StorageType.Integer:
                    return parameter.AsInteger().ToString();
                case StorageType.None:
                    return parameter.AsValueString();
                case StorageType.String:
                    return parameter.AsString();
                default:
                    return "";
            }
        }
        public void SetParameterValue(Autodesk.Revit.DB.Parameter parameter, object value)
        {
            if (parameter.IsReadOnly)
                return;
            try
            {
                switch(parameter.StorageType)
                {
                    case StorageType.Double:
                        double doubleValue;
                        if (Double.TryParse(value.ToString(), out doubleValue))
                            parameter.Set(doubleValue);
                        break;
                    case StorageType.ElementId:
                        long elemId;
                        if (long.TryParse(value.ToString(), out elemId))
                        {
                            ElementId elementId = new ElementId(elemId);
                            parameter.Set(elementId);
                        }                           
                        break;
                    case StorageType.Integer:
                        int intValue;
                        switch(value.ToString().ToUpper())
                        {
                            case "0":
                                intValue = 0;
                                break;
                            case "1":
                                intValue = 1;
                                break;
                            case "N":
                                intValue = 0;
                                break;
                            case "Y":
                                intValue = 1;
                                break;
                            case "NO":
                                intValue = 0;
                                break;
                            case "YES":
                                intValue = 0;
                                break;
                            case "":
                                intValue = 0;
                                break;
                            case "X":
                                intValue = 0;
                                break;
                            default:
                                intValue = 0;
                                break;
                        }
                        break;
                    case StorageType.String:
                        parameter.Set((string)value);
                        break;
                }
            }
            catch
            {
                return;
            }
        }
    }
}
