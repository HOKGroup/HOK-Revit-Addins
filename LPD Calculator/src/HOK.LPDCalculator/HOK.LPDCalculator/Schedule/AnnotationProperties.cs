using Autodesk.Revit.DB;

namespace HOK.LPDCalculator.Schedule
{
    public enum CalculationTypes
    {
        SpaceBySpace,
        BuildingArea
    }

    public class AnnotationProperties
    {
        private readonly FamilySymbol annotationType;
        //private CalculationTypes calculationType;
        private const string lpdUnit = "W/ft²";
        private const string loadUnit = "VA";

        public AnnotationProperties(FamilySymbol symbolType, CalculationTypes calType)
        {
            annotationType = symbolType;
            //calculationType = calType;
        }

        //BuildingArea
        public string ASHRAELPDCategory
        {
            get => GetStringValue("ASHRAELPDCategory");
            set => SetStringValue("ASHRAELPDCategory", value);
        }

        //BuildingArea
        public double ASHRAEAllowableLPD
        {
            get => GetDoubleValue("ASHRAEAllowableLPD");
            set => SetValueString("ASHRAEAllowableLPD", value + " " + lpdUnit);
        }

        //BuildingArea
        public double TargetLPD
        {
            get => GetDoubleValue("TargetLPD");
            set => SetValueString("TargetLPD", value + " " + lpdUnit);
        }

        //BuildingArea
        public double ActualLightingLoad
        {
            get => GetDoubleValue("ActualLightingLoad");
            set => SetValueString("ActualLightingLoad", value+" "+loadUnit);
        }

        //SpaceBySpace
        public double TotalAllowableLightingLoad
        {
            get => GetDoubleValue("TotalAllowableLightingLoad");
            set => SetValueString("TotalAllowableLightingLoad", value+" "+loadUnit);
        }

        //SpaceBySpace
        public double TotalActualLightingLoad
        {
            get => GetDoubleValue("TotalActualLightingLoad");
            set => SetValueString("TotalActualLightingLoad", value + " "+loadUnit);
        }

        //SpaceBySpace
        public double TotalSavingsOverage
        {
            get => GetDoubleValue("TotalSavings/Overage");
            set => SetValueString("TotalSavings/Overage", value+" "+loadUnit);
        }

        //BuildingArea, SpaceBySpace
        public double Area
        {
            get => GetDoubleValue("Area");
            set => SetDoubleValue("Area", value);
        }

        //BuildingArea, SpaceBySpace
        public double ActualLPD
        {
            get => GetDoubleValue("ActualLPD");
            set => SetValueString("ActualLPD", value + " " + lpdUnit);
        }

        //BuildingArea, SpaceBySpace
        public double Reduction
        {
            get => GetDoubleValue("%Reduction");
            set => SetDoubleValue("%Reduction", value);
        }

        //BuildingArea, SpaceBySpace
        public string LPDCalculatedBy
        {
            get => GetStringValue("LPDCalculatedBy");
            set => SetStringValue("LPDCalculatedBy", value);
        }

        //BuildingArea, SpaceBySpace
        public string LightingSpecifier
        {
            get => GetStringValue("LightingSpecifier");
            set => SetStringValue("LightingSpecifier", value);
        }


        private string GetStringValue(string paramName)
        {
            var paramValue = "";
            try
            {
                var param = annotationType.LookupParameter(paramName);
                if (null != param)
                {
                    paramValue = param.AsString();
                }
                return paramValue;
            }
            catch
            {
                return paramValue;
            }
        }

        private void SetStringValue(string paramName, string value)
        {
            try
            {
                var param = annotationType.LookupParameter(paramName);

                param?.Set(value);
            }
            catch
            {
                // ignored
            }
        }

        private double GetDoubleValue(string paramName)
        {
            double paramValue = 0;
            try
            {
                var param = annotationType.LookupParameter(paramName);
                if (null != param)
                {
                    paramValue = param.AsDouble();
                }
                return paramValue;
            }
            catch { return paramValue; }
        }

        private void SetDoubleValue(string paramName, double value)
        {
            try
            {
                var param = annotationType.LookupParameter(paramName);

                param?.Set(value);
            }
            catch
            {
                // ignored
            }
        }

        private void SetValueString(string paramName, string value)
        {
            try
            {
                var param = annotationType.LookupParameter(paramName);

                param?.SetValueString(value);
            }
            catch
            {
                // ignored
            }
        }

    }
}
