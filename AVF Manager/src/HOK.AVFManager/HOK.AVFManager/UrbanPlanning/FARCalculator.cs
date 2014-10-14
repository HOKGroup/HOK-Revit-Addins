using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using HOK.AVFManager.GenericClasses;
using System.Windows.Forms;
using Autodesk.Revit.DB.Analysis;
using System.IO;
using Autodesk.Revit.DB.Structure;

namespace HOK.AVFManager.UrbanPlanning
{
    public class FARCalculator
    {
        private Document doc;
        private SettingProperties settings;
        private ToolStripProgressBar progressBar;
        private SpatialFieldManager sfm;
        private bool overwriteResult = false;
        private Dictionary<int, MassProperties> massDictionary = new Dictionary<int, MassProperties>();
        private Dictionary<int, AreaProperties> areaDictionary = new Dictionary<int, AreaProperties>();

        private XYZ minXYZ = new XYZ();//to create boundingbox
        private XYZ maxXYZ = new XYZ();//to create boundingbox

        private List<string> unitNames = new List<string>();
        private List<double> multipliers = new List<double>();

        public List<string> UnitNames { get { return unitNames; } set { unitNames = value; } }
        public List<double> Multipliers { get { return multipliers; } set { multipliers = value; } }

        public FARCalculator(Document document, SettingProperties settingProperties, ToolStripProgressBar toolStripProgressBar, SpatialFieldManager fieldManager)
        {
            doc = document;
            settings = settingProperties;
            progressBar = toolStripProgressBar;
            progressBar.Maximum = settings.SelectedElements.Count;
            progressBar.Value = 0;
            sfm = fieldManager;
        }

        public bool MapMassToArea()
        {
            bool mapped = false;
            try
            {
                List<Element> areas = settings.SelectedElements;
                
                if (areas.Count > 0)
                {
                    foreach (Element element in areas)
                    {
                        progressBar.PerformStep();
                        Area area = element as Area;
                        if (area.Area == 0) { continue; }
#if RELEASE2015
                        if (null == area.LookupParameter("Plot Code"))
                        {
                            MessageBox.Show("Cannot find a parameter named Plot Code in Area elements\n Please add required parameters in both Area and Mass to calculate FAR."
                                , "Missing Parameter: Plot Code", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        }
                        string plotCode = area.LookupParameter("Plot Code").AsString();
#else
                        if (null == area.get_Parameter("Plot Code"))
                        {
                            MessageBox.Show("Cannot find a parameter named Plot Code in Area elements\n Please add required parameters in both Area and Mass to calculate FAR."
                                , "Missing Parameter: Plot Code", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        }
                        string plotCode = area.get_Parameter("Plot Code").AsString();
#endif

                        if (plotCode == null || plotCode == "0") { continue; }

                        minXYZ = new XYZ();
                        maxXYZ = new XYZ();

                        IList<CurveLoop> profiles = CreateProfiles(area);
                        Solid extrusion = null;
                        if (profiles.Count > 0)
                        {
                            try
                            {
                                extrusion = GeometryCreationUtilities.CreateExtrusionGeometry(profiles, XYZ.BasisZ, 100);
                            }
                            catch { extrusion = null; return false; }
                        }

                        if (null != extrusion && !areaDictionary.ContainsKey(area.Id.IntegerValue))
                        {
                            AreaProperties ap = new AreaProperties(area);
                            ap.SolidArea = extrusion;
                            ap.BaseFace = GetBottomFace(extrusion);
                            ap.MassIDs = FindMass(ap);
                            ap.FAR = CalculateFAR(ap);
                            ap.FARParameter = ap.FAR;
                            areaDictionary.Add(ap.AreaId, ap);
                        }
                    }
                }
                mapped = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to map between areas and mass object to write plot codes.\n" + ex.Message, "FARCalcuator:MapMassToArea", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return mapped;
        }

        private Face GetBottomFace(Solid solid)
        {
            Face bottomFace = null;
            try
            {
                foreach (Face face in solid.Faces)
                {
                    if (AnalysisHelper.DetermineFaceDirection(face, DisplayingFaces.Bottom))
                    {
                        return face;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get a base face of the extruded solid. \n"+ex.Message, "FARCalculator:GetBottomFace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return bottomFace;
        }

        private List<int> FindMass(AreaProperties ap)
        {
            List<int> massIds = new List<int>();
            try
            {
                Outline outline = new Outline(minXYZ, maxXYZ);
                BoundingBoxIntersectsFilter intersectFilter = new BoundingBoxIntersectsFilter(outline);
                BoundingBoxIsInsideFilter insideFilter = new BoundingBoxIsInsideFilter(outline);
                LogicalOrFilter orFilter = new LogicalOrFilter(intersectFilter, insideFilter);

                FilteredElementCollector collector = new FilteredElementCollector(doc);
                collector.OfCategory(BuiltInCategory.OST_Mass);

                IList<Element> elements = collector.WherePasses(orFilter).ToElements();

                foreach (Element element in elements)
                {
#if RELEASE2015
                    Parameter parameter = element.LookupParameter("Building Function");
#else 
                    Parameter parameter = element.get_Parameter("Building Function");
#endif

                    if (null != parameter)
                    {
                        MassProperties mp = new MassProperties(element);
                        if (mp.GrossFloorArea != 0 && null != mp.Centroid)
                        {
                            if (null != ap.BaseFace.Project(mp.Centroid))
                            {
                                mp.AreaId = ap.AreaId;
                                mp.PlotCode = ap.PlotCode;
                                mp.DistricCode = ap.DistricCode;
                                if (!massDictionary.ContainsKey(mp.MassId))
                                {
                                    massDictionary.Add(mp.MassId, mp);
                                }
                                massIds.Add(mp.MassId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find mass instances within an area object.\n"+ex.Message, "FARCacalculator:FindMass", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return massIds;
        }

        private double CalculateFAR(AreaProperties ap)
        {
            double far = 0;
            try
            {
                double sum = 0;

                foreach (int massId in ap.MassIDs)
                {
                    if (massDictionary.ContainsKey(massId))
                    {
                        MassProperties mp = massDictionary[massId];
                        sum = sum + mp.GrossFloorArea;
                    }
                }

                if (sum != 0)
                {
                    far = sum / ap.Area;
                }
                return far;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to calculate FAR.\n"+ex.Message, "FARCalculator:CalculateFAR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return far;
            }
        }

        private List<CurveLoop> CreateProfiles(Area area)
        {
            List<CurveLoop> profiles = new List<CurveLoop>();
            try
            {
                CurveArrArray curveArrArray = new CurveArrArray();

                if (area.GetBoundarySegments(new SpatialElementBoundaryOptions()) != null)
                {
                    foreach (IList<Autodesk.Revit.DB.BoundarySegment> boundarySegments in area.GetBoundarySegments(new SpatialElementBoundaryOptions()))
                    {
                        if (boundarySegments.Count > 0)
                        {
                            CurveArray curveArray = new CurveArray();
                            foreach (Autodesk.Revit.DB.BoundarySegment boundarySegment in boundarySegments)
                            {
                                Curve curve = boundarySegment.Curve;
                                curveArray.Append(curve);
                            }
                            curveArrArray.Append(curveArray);
                        }
                    }
                }
                double zValue = 0;
                double maxCircum = 0;
                bool isFirst = true;

                foreach (CurveArray curveArray in curveArrArray)
                {
                    List<XYZ> pointList = new List<XYZ>();
                    CurveLoop profile = new CurveLoop();
                    double circumference = 0;

                    foreach (Curve curve in curveArray)
                    {
                        int pointCount = curve.Tessellate().Count;
                        if (pointCount > 2)
                        {
                            IList<XYZ> tPoints = curve.Tessellate();
                           
                            //to make equivalent z value to avoid slightly line offset error.
                            if (isFirst) { zValue = tPoints[0].Z; isFirst = false; }
                            //remove the last point on the curve.
                            for (int i = 0; i < tPoints.Count-1; i++)
                            {
                                XYZ tempPoint = new XYZ(tPoints[i].X, tPoints[i].Y, zValue);
                                if (pointList.Count == 0) { pointList.Add(tempPoint); }
                                else
                                {
                                    double distance = pointList[pointList.Count - 1].DistanceTo(tempPoint);
                                    if (distance > 0.004)
                                    {
                                        pointList.Add(tempPoint);
                                    }
                                }
                            }
                        }
                        else if (pointCount == 2)
                        {
#if RELEASE2013
                            XYZ pt = curve.get_EndPoint(0);
#else
                             XYZ pt = curve.GetEndPoint(0);
#endif

                            if (isFirst) { zValue = pt.Z; isFirst = false; }

                            XYZ tempPoint = new XYZ(pt.X, pt.Y, zValue);
                            if (pointList.Count == 0) { pointList.Add(tempPoint); }
                            else
                            {
                                double distance = pointList[pointList.Count - 1].DistanceTo(tempPoint);
                                if (distance > 0.004)
                                {
                                    pointList.Add(tempPoint);
                                }
                            }
                        }
                    }

                    int num = pointList.Count;
                    double minX = pointList[0].X;
                    double minY = pointList[0].Y;
                    double minZ = pointList[0].Z;
                    double maxX = pointList[0].X;
                    double maxY = pointList[0].Y;
                    double maxZ = minZ + 100;

                    if (num > 2)
                    {
                        Curve newCurve = null;
                        for (int i = 0; i < num; i++)
                        {

                            if (i == num - 1)
                            {
#if RELEASE2013
                                newCurve = doc.Application.Create.NewLine(pointList[i], pointList[0],true);
#else
                                 newCurve = Line.CreateBound(pointList[i], pointList[0]);
#endif
                            }
                            else
                            {
#if RELEASE2013
                                newCurve = doc.Application.Create.NewLine(pointList[i], pointList[i + 1],true);
#else
                                newCurve = Line.CreateBound(pointList[i], pointList[i + 1]);
#endif
                            }
                            profile.Append(newCurve);

                            XYZ xyz = pointList[i];
                            if (xyz.X < minX) { minX = xyz.X; }
                            if (xyz.Y < minY) { minY = xyz.Y; }
                            if (xyz.X > maxX) { maxX = xyz.X; }
                            if (xyz.Y > maxY) { maxY = xyz.Y; }
                        }

                        minXYZ = new XYZ(minX, minY, minZ);
                        maxXYZ = new XYZ(maxX, maxY, maxZ);
                        circumference = profile.GetExactLength();

                        if (maxCircum == 0) { maxCircum = circumference; }
                        else if (maxCircum < circumference) { maxCircum = circumference; }

                        if (maxCircum == circumference) { profiles.Insert(0, profile); }
                        else { profiles.Add(profile); }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Area Id:"+area.Id.IntegerValue+"\nFailed to create profiles from an area object.\n"+ex.Message, "FARCalculator:CreateProfiles", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return profiles;
        }

        public bool AnalysisByElements()
        {
            bool result = false;
            try
            {
                if (areaDictionary.Count > 0)
                {
                    int resultIndex = FindIndexOfResult(sfm);
                    bool firstLoop = true;
                    foreach (int areaId in areaDictionary.Keys)
                    {
                        AreaProperties ap = areaDictionary[areaId];
                        List<double> dblList = new List<double>(); //double values to be displayed on the face.
                        Face face = ap.BaseFace;
                        dblList.Add(ap.FAR);

                        if (dblList.Count != sfm.NumberOfMeasurements) { continue; }

                        int index = sfm.AddSpatialFieldPrimitive(face,Transform.Identity);
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to visulaize the FAR data. \n" + ex.Message, "FARCalculator : AnalysisByElements", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void SetCurrentStyle()
        {
            ElementId displayStyleId = AnalysisDisplayStyle.FindByName(doc, settings.DisplayStyle);
            if (null != displayStyleId)
            {
                doc.ActiveView.AnalysisDisplayStyleId = displayStyleId;
            }
        }
    }

    public class MassProperties
    {
        private Element m_mass;
        public MassProperties(Element fi) 
        {
            m_mass = fi;
        }
        public int MassId
        {
            get { try { return m_mass.Id.IntegerValue; } catch { return 0; } }
        }
        public string DistricCode
        {
#if RELEASE2015
            get { try { return m_mass.LookupParameter("District Code").AsString(); } catch { return ""; } }
            set { try { m_mass.LookupParameter("District Code").Set(value); } catch { } }
#else
            get { try { return m_mass.get_Parameter("District Code").AsString(); } catch { return ""; } }
            set { try { m_mass.get_Parameter("District Code").Set(value); } catch { } }
#endif
        }
        public string PlotCode
        {
#if RELEASE2015
            get { try { return m_mass.LookupParameter("Plot Code").AsString(); } catch { return ""; } }
            set { try { m_mass.LookupParameter("Plot Code").Set(value); } catch { } }
#else
            get { try { return m_mass.get_Parameter("Plot Code").AsString(); } catch { return ""; } }
            set { try { m_mass.get_Parameter("Plot Code").Set(value); } catch { } }
#endif
        }
        public double GrossFloorArea
        {
#if RELEASE2015
            get { try { return m_mass.LookupParameter("Gross Floor Area").AsDouble(); } catch { return 0; } }
#else
            get { try { return m_mass.get_Parameter("Gross Floor Area").AsDouble(); } catch { return 0; } }
#endif
        }
        public int AreaId { get; set; } //ParseId
        public XYZ Centroid
        {
            get
            {
                try
                {
                    GeometryElement geomElement = m_mass.get_Geometry(new Options());
                    foreach (GeometryObject geomObject in geomElement)
                    {
                        Solid solid = geomObject as Solid;
                        if (null != solid)
                        {
                            if (solid.Volume > 0)
                            {
                                return solid.ComputeCentroid();
                            }
                        }
                    }
                    return null;
                }
                catch { return null; }
            }
        }
        public Element MassObject { get { return m_mass; } }
    }

    public class AreaProperties
    {
        private Area m_area;
        private double farValue;

        public AreaProperties(Area area)
        {
            m_area = area;
        }
        public int AreaId
        {
            get { try { return m_area.Id.IntegerValue; } catch { return 0; } }
        }
        public string DistricCode
        {
#if RELEASE2015
            get { try { return m_area.LookupParameter("District Code").AsString(); } catch { return ""; } }
#else
             get { try { return m_area.get_Parameter("District Code").AsString(); } catch { return ""; } }
#endif
        }
        public string PlotCode
        {
#if RELEASE2015
            get { try { return m_area.LookupParameter("Plot Code").AsString(); } catch { return ""; } }
#else
            get { try { return m_area.get_Parameter("Plot Code").AsString(); } catch { return ""; } }
#endif
        }
        public double Area
        {
#if RELEASE2015
            get { try { return m_area.LookupParameter("Area").AsDouble(); } catch { return 0; } }
#else
             get { try { return m_area.get_Parameter("Area").AsDouble(); } catch { return 0; } }
#endif
        }
        public double FARParameter 
        {
#if RELEASE2015
            get { try { return m_area.LookupParameter("FAR").AsDouble(); } catch { return 0; } }
            set { try { m_area.LookupParameter("FAR").Set(value); } catch { } }
#else
            get { try { return m_area.get_Parameter("FAR").AsDouble(); } catch { return 0; } }
            set { try { m_area.get_Parameter("FAR").Set(value); } catch { } }
#endif
        }
        public double FAR { get; set; }
        public double RequiredFAR { get; set; }
        //public Level Level { get { return m_area.Level; } }
        
        public Area AreaObject { get { return m_area; } }
        public List<int> MassIDs { get; set; }
        public Solid SolidArea{get;set;}
        public Face BaseFace { get; set; }
    }
}
