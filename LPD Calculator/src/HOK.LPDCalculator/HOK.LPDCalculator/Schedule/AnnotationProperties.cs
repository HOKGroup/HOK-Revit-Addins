using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace HOK.LPDCalculator.Schedule
{
    public enum CalculationTypes { SpaceBySpace, BuildingArea }

    public class AnnotationProperties
    {
        private FamilySymbol annotationType = null;
        private CalculationTypes calculationType;

        private string lpdUnit = "W/ft²";
        private string loadUnit = "VA";

        public AnnotationProperties(FamilySymbol symbolType, CalculationTypes calType)
        {
            annotationType = symbolType;
            calculationType = calType;
        }

        //BuildingArea
        public string ASHRAELPDCategory
        {
            get { return GetStringValue("ASHRAELPDCategory"); }
            set { SetStringValue("ASHRAELPDCategory", value); }
        }

        //BuildingArea
        public double ASHRAEAllowableLPD
        {
            get { return GetDoubleValue("ASHRAEAllowableLPD"); }
            set { SetValueString("ASHRAEAllowableLPD", value+" "+lpdUnit); }
        }

        //BuildingArea
        public double TargetLPD
        {
            get { return GetDoubleValue("TargetLPD"); }
            set { SetValueString("TargetLPD", value + " " + lpdUnit); }
        }

        //BuildingArea
        public double ActualLightingLoad
        {
            get { return GetDoubleValue("ActualLightingLoad"); }
            set { SetValueString("ActualLightingLoad", value+" "+loadUnit); }
        }

        //SpaceBySpace
        public double TotalAllowableLightingLoad
        {
            get { return GetDoubleValue("TotalAllowableLightingLoad"); }
            set { SetValueString("TotalAllowableLightingLoad", value+" "+loadUnit); }
        }

        //SpaceBySpace
        public double TotalActualLightingLoad
        {
            get { return GetDoubleValue("TotalActualLightingLoad"); }
            set { SetValueString("TotalActualLightingLoad", value + " "+loadUnit); }
        }

        //SpaceBySpace
        public double TotalSavingsOverage
        {
            get { return GetDoubleValue("TotalSavings/Overage"); }
            set { SetValueString("TotalSavings/Overage", value+" "+loadUnit); }  
        }

        //BuildingArea, SpaceBySpace
        public double Area
        {
            get { return GetDoubleValue("Area"); }
            set { SetDoubleValue("Area", value); }
        }

        //BuildingArea, SpaceBySpace
        public double ActualLPD
        {
            get { return GetDoubleValue("ActualLPD"); }
            set { SetValueString("ActualLPD", value + " " + lpdUnit); }
        }

        //BuildingArea, SpaceBySpace
        public double Reduction
        {
            get { return GetDoubleValue("%Reduction"); }
            set { SetDoubleValue("%Reduction", value); }
        }

        //BuildingArea, SpaceBySpace
        public string LPDCalculatedBy
        {
            get { return GetStringValue("LPDCalculatedBy"); }
            set { SetStringValue("LPDCalculatedBy", value); }
        }

        //BuildingArea, SpaceBySpace
        public string LightingSpecifier
        {
            get { return GetStringValue("LightingSpecifier"); }
            set { SetStringValue("LightingSpecifier", value); }
        }


        private string GetStringValue(string paramName)
        {
            string paramValue = "";
            try
            {
#if RELEASE2015 || RELEASE2016 || RELEASE2017
                Parameter param = annotationType.LookupParameter(paramName);
#else
                Parameter param = annotationType.get_Parameter(paramName);
#endif
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
#if RELEASE2015 || RELEASE2016 || RELEASE2017
                Parameter param = annotationType.LookupParameter(paramName);
#else
                Parameter param = annotationType.get_Parameter(paramName);
#endif

                if (null != param)
                {
                    param.Set(value);
                }
            }
            catch { }
        }

        private double GetDoubleValue(string paramName)
        {
            double paramValue = 0;
            try
            {
#if RELEASE2015 || RELEASE2016 || RELEASE2017
                Parameter param = annotationType.LookupParameter(paramName);
#else
                Parameter param = annotationType.get_Parameter(paramName);
#endif
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
#if RELEASE2015 || RELEASE2016 || RELEASE2017
                Parameter param = annotationType.LookupParameter(paramName);
#else
                Parameter param = annotationType.get_Parameter(paramName);
#endif

                if (null != param)
                {
                    param.Set(value);
                }
            }
            catch { }
        }

        private void SetValueString(string paramName, string value)
        {
            try
            {
#if RELEASE2015 || RELEASE2016 || RELEASE2017
                Parameter param = annotationType.LookupParameter(paramName);
#else
                Parameter param = annotationType.get_Parameter(paramName);
#endif

                if (null != param)
                {
                    param.SetValueString(value);
                }
            }
            catch { }
        }

    }
}
