using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using HOK.AVFManager.GenericForms;

namespace HOK.AVFManager.GenericClasses
{
    public enum DisplayingFaces { Top = 0, Side, Bottom, All, Custom }
    public enum ResultType { CustomizedAnalysis=0, MassAnalysis, FARCalculator, Topography, BuildingNetwork, FacadeAnalysis, HeatMap, RadianceAnalysis, FieldOfView }

    public class SettingProperties
    {
        //Input
        public string ResultName { get; set; }
        public ResultType ResultType { get; set; }
        public AnalysisCategory AnalysisCategory { get; set; }
        public string CategoryName { get; set; }
        public string[] CategoryOptions { get; set; }
        public List<string> ParameterList { get; set; }
        public string ParameterName { get; set; }
        public bool IsPicked { get; set; }
        public bool SetReference { get; set; }
        public string RefCategory { get; set; }
        public string RefDescription { get; set; }
        public List<Element> SelectedElements { get; set; }
        public List<Element> ReferenceElements { get; set; }
        public string ReferenceDataFile { get; set; }
        //public List<Face> SelectedFaces { get; set; }
        public Dictionary<ElementId, Dictionary<Reference, Face>> SelectedFaces { get; set; }
        public DisplayingFaces DisplayFace { get; set; }
        public string DisplayStyle { get; set; }
        public string Units { get; set; }
        public string LegendTitle { get; set; }
        public string LegendDescription { get; set; }
        public int NumberOfMeasurement { get; set; }
        public Dictionary<string, string> Configurations { get; set; }
    }
}
