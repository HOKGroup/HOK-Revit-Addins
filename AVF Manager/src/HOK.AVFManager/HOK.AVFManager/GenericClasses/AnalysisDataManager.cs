using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using System.Windows.Forms;
using HOK.AVFManager.GenericForms;
using HOK.AVFManager.InteriorDesign;
using HOK.AVFManager.UrbanPlanning;
using System.IO;

namespace HOK.AVFManager.GenericClasses
{
    public class AnalysisDataManager
    {
        private Document doc;
        private SettingProperties settings;
        private bool overwriteResult = false;
        private List<string> unitNames = new List<string>();
        private List<double> multipliers = new List<double>();
        private List<string> measurementNames = new List<string>();
        private ToolStripProgressBar progressBar;

        public AnalysisDataManager(Document document, SettingProperties settingProperties,ToolStripProgressBar toolStripProgressBar)
        {
            doc = document;
            settings = settingProperties;
            progressBar = toolStripProgressBar;
            AddUnitSets();
            
        }

        private void AddUnitSets()
        {
            unitNames.Add("none");
            unitNames.Add("feet");
            unitNames.Add("inches");
            unitNames.Add("meters");
            unitNames.Add("square feet");
            unitNames.Add("square meters");
            unitNames.Add("cubic feet");
            unitNames.Add("cubic meters");

            multipliers.Add(1);
            multipliers.Add(1);
            multipliers.Add(12);
            multipliers.Add(0.3048);
            multipliers.Add(1);
            multipliers.Add(0.092903);
            multipliers.Add(1);
            multipliers.Add(0.0283168);
        }

        private void SetCurrentStyle()
        {
            ElementId displayStyleId = AnalysisDisplayStyle.FindByName(doc, settings.DisplayStyle);
            if (null != displayStyleId)
            {
                doc.ActiveView.AnalysisDisplayStyleId = displayStyleId;
            }
        }

        public bool CreateSpatialField()
        {
            bool result = false;
            try
            {
                SpatialFieldManager sfm = SpatialFieldManager.GetSpatialFieldManager(doc.ActiveView);
                switch (settings.ResultType)
                {
                    case ResultType.MassAnalysis:
                        if (sfm == null) { sfm = SpatialFieldManager.CreateSpatialFieldManager(doc.ActiveView, settings.NumberOfMeasurement); }
                        sfm.SetMeasurementNames(settings.Configurations.Keys.ToList());
                        sfm.SetMeasurementDescriptions(settings.Configurations.Values.ToList());

                        if (IsAnalysisByFaces()) { result=AnalysisByFaces(sfm); }
                        else { result=AnalysisByElements(sfm); }
                        break;
                    case ResultType.FARCalculator:
                        if (sfm == null) { sfm = SpatialFieldManager.CreateSpatialFieldManager(doc.ActiveView, settings.NumberOfMeasurement); }
                        sfm.SetMeasurementNames(settings.Configurations.Keys.ToList());
                        sfm.SetMeasurementDescriptions(settings.Configurations.Values.ToList());

                        FARCalculator farCalculator = new FARCalculator(doc, settings, progressBar, sfm);
                        if (farCalculator.MapMassToArea())
                        {
                            farCalculator.UnitNames = unitNames;
                            farCalculator.Multipliers = multipliers;
                            result = farCalculator.AnalysisByElements();
                        }
                        break;
                    case ResultType.Topography:
                        break;
                    case ResultType.BuildingNetwork:
                        break;
                    case ResultType.FacadeAnalysis:
                        break;
                    case ResultType.HeatMap:
                        break;
                    case ResultType.RadianceAnalysis:
                        if (sfm == null) { sfm = SpatialFieldManager.CreateSpatialFieldManager(doc.ActiveView, 3); }
                        List<string> measurementNames = new List<string>();
                        measurementNames.Add("Value 1"); measurementNames.Add("Value 2"); measurementNames.Add("Value 3");
                        sfm.SetMeasurementNames(measurementNames);
                        sfm.SetMeasurementDescriptions(measurementNames);

                        RadianceDisplay radianceDisplay = new RadianceDisplay(doc, settings, progressBar, sfm);
                        if (File.Exists(settings.ReferenceDataFile))
                        {
                            if (radianceDisplay.ReadDatFile())
                            {
                                radianceDisplay.UnitNames = unitNames;
                                radianceDisplay.Multipliers = multipliers;
                                result = radianceDisplay.AnalysisByElements();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Please import a data file to display the result of the readiance analysis.", "File Not Found : Data File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        break;
                    case ResultType.FieldOfView:
                        if (sfm == null) { sfm = SpatialFieldManager.CreateSpatialFieldManager(doc.ActiveView, settings.NumberOfMeasurement); }
                        sfm.SetMeasurementNames(settings.Configurations.Keys.ToList());
                        sfm.SetMeasurementDescriptions(settings.Configurations.Values.ToList());

                        FieldOfViewAnalysis viewAnalysis = new FieldOfViewAnalysis(doc, settings, progressBar, sfm);
                        if (null!= viewAnalysis.FindPointOfView())
                        {
                            viewAnalysis.UnitNames = unitNames;
                            viewAnalysis.Multipliers = multipliers;
                            result = viewAnalysis.AnalysisByElements();
                        }
                        break;
                    case ResultType.CustomizedAnalysis:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create spatial filed. \n"+ex.Message, "AnalysisDataManager : CreateSpatialField", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        private bool IsAnalysisByFaces()
        {
            bool faceSelected = false;

            if (null != settings.SelectedFaces)
            {
                if (settings.SelectedFaces.Count > 0) { faceSelected = true; }
            }
            return faceSelected;
        }

        private bool AnalysisByElements(SpatialFieldManager sfm)
        {
            bool result = false;
            progressBar.Maximum = settings.SelectedElements.Count;
            try
            {
                Options options = new Options();
                options.ComputeReferences = true;
                Dictionary<Face, List<double>> faceDictionary = new Dictionary<Face, List<double>>();

                foreach (Element element in settings.SelectedElements)
                {
                    List<double> valueLists = new List<double>();
                    foreach (string paramName in settings.Configurations.Keys)
                    {
#if RELEASE2015
                        Parameter param = element.LookupParameter(paramName);
#else
                        Parameter param = element.get_Parameter(paramName);
#endif
                        if (param != null)
                        {
                            double value = param.AsDouble();
                            valueLists.Add(value);
                        }
                        else
                        {
                            valueLists.Add(0);
                        }
                    }
                    
                    GeometryElement geomElem = element.get_Geometry(options);

                    if (geomElem != null && valueLists.Count==settings.NumberOfMeasurement)
                    {
                        foreach (GeometryObject geomObj in geomElem)
                        {
                            Solid solid = geomObj as Solid;
                            if (solid != null)
                            {
                                foreach (Face face in solid.Faces)
                                {
                                    if (AnalysisHelper.DetermineFaceDirection(face, settings.DisplayFace))
                                    {
                                        faceDictionary.Add(face, valueLists);
                                    }
                                }
                            }
                        }
                    }
                    progressBar.PerformStep();
                }

                int resultIndex = FindIndexOfResult(sfm);
                bool firstLoop = true;
                foreach (Face face in faceDictionary.Keys)
                {
                    List<double> dblList = new List<double>(); //double values to be displayed on the face.
                    dblList = faceDictionary[face];

                    if (dblList.Count != sfm.NumberOfMeasurements) { continue; }

                    int index = sfm.AddSpatialFieldPrimitive(face.Reference);
                    IList<UV> uvPts = new List<UV>();
                    IList<ValueAtPoint> valList = new List<ValueAtPoint>();
                    BoundingBoxUV bb = face.GetBoundingBox();
                    UV faceCenter = (bb.Min + bb.Max) / 2; //only collect a center point.

                    uvPts.Add(faceCenter);
                    valList.Add(new ValueAtPoint(dblList));
                    //dblList.Clear();

                    FieldDomainPointsByUV domainPoints = new FieldDomainPointsByUV(uvPts);
                    FieldValues values = new FieldValues(valList);

                    AnalysisResultSchema resultSchema = new AnalysisResultSchema(settings.LegendTitle, settings.LegendDescription);
                    resultSchema.SetUnits(unitNames, multipliers);
                    if (unitNames.Contains(settings.Units)) { resultSchema.CurrentUnits = unitNames.IndexOf(settings.Units); }

                    if (overwriteResult) { sfm.SetResultSchema(resultIndex, resultSchema); }
                    else if (firstLoop) { resultIndex = sfm.RegisterResult(resultSchema); firstLoop = false; }
                    else { sfm.SetResultSchema(resultIndex, resultSchema); }

                    sfm.UpdateSpatialFieldPrimitive(index, domainPoints, values, resultIndex);
                }
                SetCurrentStyle();
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to analyze elements. \n" + ex.Message, "AnalysisDataManager : AnalysisByElements", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        private bool AnalysisByFaces(SpatialFieldManager sfm)
        {
            bool result = false;
            progressBar.Maximum = settings.SelectedElements.Count;
            try
            {
                Options options = new Options();
                options.ComputeReferences = true;
                
                Dictionary<ElementId, Dictionary<Reference, Face>> selectedFaces = new Dictionary<ElementId, Dictionary<Reference, Face>>();
                selectedFaces = settings.SelectedFaces;

                Dictionary<Face, Dictionary<Reference, List<double>/*paramValues*/>> faceDictionary = new Dictionary<Face, Dictionary<Reference, List<double>/*paramValues*/>>();
                foreach (Element element in settings.SelectedElements)
                {
                    ElementId elementId = element.Id;
                    if (!selectedFaces.ContainsKey(elementId)) { continue; }

                    List<double> valueLists = new List<double>();
                    foreach (string paramName in settings.Configurations.Keys)
                    {
#if RELEASE2015
                        Parameter param = element.LookupParameter(paramName);
#else
                        Parameter param = element.get_Parameter(paramName);
#endif

                        if (param != null)
                        {
                            double value = param.AsDouble();
                            valueLists.Add(value);
                        }
                        else
                        {
                            valueLists.Add(0);
                        }
                    }

                    foreach (Reference reference in selectedFaces[elementId].Keys)
                    {
                        if (null != selectedFaces[elementId][reference] && valueLists.Count == settings.NumberOfMeasurement)
                        {
                            Face face = selectedFaces[elementId][reference];
                            faceDictionary.Add(face, new Dictionary<Reference, List<double>>());
                            faceDictionary[face].Add(reference, valueLists);
                            //break;
                        }
                    }
                    progressBar.PerformStep();
                }

                int resultIndex = FindIndexOfResult(sfm);
                bool firstLoop = true;
                foreach (Face face in faceDictionary.Keys)
                {
                    Reference reference = faceDictionary[face].Keys.First();
                    List<double> dblList = new List<double>();//double values to be displayed on the face.
                    dblList = faceDictionary[face][reference];
                    int index = sfm.AddSpatialFieldPrimitive(reference);
                    IList<UV> uvPts = new List<UV>();
                    IList<ValueAtPoint> valList = new List<ValueAtPoint>();
                    BoundingBoxUV bb = face.GetBoundingBox();
                    UV faceCenter = (bb.Min + bb.Max) / 2; //only collect a center point.

                    uvPts.Add(faceCenter);
                    valList.Add(new ValueAtPoint(dblList));
                    //dblList.Clear();

                    FieldDomainPointsByUV domainPoints = new FieldDomainPointsByUV(uvPts);
                    FieldValues values = new FieldValues(valList);

                    AnalysisResultSchema resultSchema = new AnalysisResultSchema(settings.LegendTitle, settings.LegendDescription);
                    resultSchema.SetUnits(unitNames, multipliers);
                    if (unitNames.Contains(settings.Units)) { resultSchema.CurrentUnits = unitNames.IndexOf(settings.Units); }

                    if (overwriteResult) { sfm.SetResultSchema(resultIndex, resultSchema); }
                    else if (firstLoop) { resultIndex = sfm.RegisterResult(resultSchema); firstLoop = false; }
                    else { sfm.SetResultSchema(resultIndex, resultSchema); }

                    sfm.UpdateSpatialFieldPrimitive(index, domainPoints, values, resultIndex);
                }
                SetCurrentStyle();
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to analyze elements. \n" + ex.Message, "AnalysisDataManager : ElementAnalysis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        private int FindIndexOfResult(SpatialFieldManager sfm)
        {
            int index = 0;
            overwriteResult = false;
            IList<int> regIndices = sfm.GetRegisteredResults();

            foreach (int i in regIndices)
            {
                AnalysisResultSchema result = sfm.GetResultSchema(i);
                if (result.Name == settings.LegendTitle)
                {
                    index = i;
                    overwriteResult = true;
                    break;
                }
            }
            return index;
        }
    }
}
