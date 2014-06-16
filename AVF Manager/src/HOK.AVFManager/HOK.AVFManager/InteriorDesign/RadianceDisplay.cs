using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using HOK.AVFManager.GenericClasses;
using System.Windows.Forms;
using Autodesk.Revit.DB.Analysis;
using System.IO;
using System.Globalization;

namespace HOK.AVFManager.InteriorDesign
{
    public class RadianceDisplay
    {
        private Document doc;
        private SettingProperties settings;
        private ToolStripProgressBar progressBar;
        private SpatialFieldManager sfm;
        private Dictionary<int/*index*/, RadianceResult> radianceDictionary = new Dictionary<int, RadianceResult>();
        private bool overwriteResult = false;

        private List<string> unitNames = new List<string>();
        private List<double> multipliers = new List<double>();

        public List<string> UnitNames { get { return unitNames; } set { unitNames = value; } }
        public List<double> Multipliers { get { return multipliers; } set { multipliers = value; } }

        public RadianceDisplay(Document document, SettingProperties settingProperties, ToolStripProgressBar toolStripProgressBar, SpatialFieldManager fieldManager)
        {
            doc = document;
            settings = settingProperties;
            progressBar = toolStripProgressBar;
            sfm = fieldManager;
        }

        public bool ReadDatFile()
        {
            bool readed = false;
            try
            {
                using (StreamReader sr = new StreamReader(settings.ReferenceDataFile))
                {
                    string line;
                    int index = 0;
                    double unitFactor=3.28084; //meter to feet
                    while ((line = sr.ReadLine()) != null)
                    {
                        RadianceResult rr = new RadianceResult();

                        if (line.Contains("#")) { break; }
                        string[] points = line.Split(' ');

                        double x = double.Parse(points[0]) * unitFactor;
                        double y = double.Parse(points[1]) * unitFactor;
                        double z = double.Parse(points[2]) * unitFactor;
                        XYZ xyz = new XYZ(x, y, z);
                        rr.PointXYZ = xyz;

                        string[] values = points[6].Split('\t');
                        rr.Value1 = double.Parse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                        rr.Value2 = double.Parse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                        rr.Value3 = double.Parse(values[2], NumberStyles.Any, CultureInfo.InvariantCulture);

                        if (null != rr.PointXYZ)
                        {
                            radianceDictionary.Add(index, rr);
                            index++;
                        }
                    }
                }
                readed = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read the data file. The format is invalid.\n"+ex.Message, "RadianceDisplay:ReadDatFile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                readed = false;
            }
            return readed;
        }

        public bool FindFace()
        {
            bool found = false;
            try
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find a face from a boundingbox.\n" + ex.Message, "RadianceDisplay:FindFace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return found;
        }

        public bool AnalysisByElements()
        {
            bool result = false;
            try
            {
                if (radianceDictionary.Count > 0)
                {
                    int resultIndex = FindIndexOfResult(sfm);
                    bool firstLoop = true;
                    int index = sfm.AddSpatialFieldPrimitive();//not associated with any geometry
                    IList<XYZ> xyzPoints = new List<XYZ>();
                    IList<ValueAtPoint> valList = new List<ValueAtPoint>();

                    foreach (int keyIndex in radianceDictionary.Keys)
                    {
                        RadianceResult rr = radianceDictionary[keyIndex];
                        List<double> dblList = new List<double>(); //double values to be displayed on the face.
                        
                        dblList.Add(rr.Value1);
                        dblList.Add(rr.Value2);
                        dblList.Add(rr.Value3);

                        if (dblList.Count != sfm.NumberOfMeasurements) { continue; }

                        xyzPoints.Add(rr.PointXYZ);
                        valList.Add(new ValueAtPoint(dblList));
                        //dblList.Clear();
                    }
                    FieldDomainPointsByXYZ domainPoints = new FieldDomainPointsByXYZ(xyzPoints);
                    FieldValues values = new FieldValues(valList);

                    AnalysisResultSchema resultSchema = new AnalysisResultSchema(settings.LegendTitle, settings.LegendDescription);
                    resultSchema.SetUnits(unitNames, multipliers);
                    if (unitNames.Contains(settings.Units)) { resultSchema.CurrentUnits = unitNames.IndexOf(settings.Units); }

                    if (overwriteResult) { sfm.SetResultSchema(resultIndex, resultSchema); }
                    else if (firstLoop) { resultIndex = sfm.RegisterResult(resultSchema); firstLoop = false; }
                    else { sfm.SetResultSchema(resultIndex, resultSchema); }

                    sfm.UpdateSpatialFieldPrimitive(index, domainPoints, values, resultIndex);

                    SetCurrentStyle();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to visulaize the FAR data. \n" + ex.Message, "FARCalculator : AnalysisByElements", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        private void SetCurrentStyle()
        {
            ElementId displayStyleId = AnalysisDisplayStyle.FindByName(doc, settings.DisplayStyle);
            if (null != displayStyleId)
            {
                doc.ActiveView.AnalysisDisplayStyleId = displayStyleId;
            }
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

    public class RadianceResult
    {
        public RadianceResult()
        {
        }

        public XYZ PointXYZ { get; set; }
        public double Value1 { get; set; }
        public double Value2 { get; set; }
        public double Value3 { get; set; }
    }

}
