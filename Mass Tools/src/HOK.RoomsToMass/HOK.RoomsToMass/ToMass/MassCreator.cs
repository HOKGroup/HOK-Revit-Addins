using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System.IO;
using System.Windows.Forms;
using Autodesk.Revit.DB.Structure;
using System.Drawing;

namespace HOK.RoomsToMass.ToMass
{
    class MassCreator
    {
        private Autodesk.Revit.ApplicationServices.Application m_app;
        private Document doc;
        private Dictionary<int, RoomProperties> roomDictionary = new Dictionary<int, RoomProperties>();
        private Dictionary<int, AreaProperties> areaDictionary = new Dictionary<int, AreaProperties>();
        private Dictionary<int, FloorProperties> floorDictionary = new Dictionary<int, FloorProperties>();
        private string massFolder = "";
        private StringBuilder failureMessage = new StringBuilder();
        private Dictionary<string, Definition> defDictionary = new Dictionary<string, Definition>();

        public Dictionary<int, RoomProperties> RoomDictionary { get { return roomDictionary; } set { roomDictionary = value; } }
        public Dictionary<int, AreaProperties> AreaDictionary { get { return areaDictionary; } set { areaDictionary = value; } }
        public Dictionary<int, FloorProperties> FloorDictionary { get { return floorDictionary; } set { floorDictionary = value; } }
        public StringBuilder FailureMessage { get { return failureMessage; } set { failureMessage = value; } }
        public string MassFolder { get { return massFolder; } set { massFolder = value; } }
        public Dictionary<string, Definition> DefDictionary { get { return defDictionary; } set { defDictionary = value; } }

        public MassCreator(UIApplication uiapp)
        {
            m_app = uiapp.Application;
            doc = uiapp.ActiveUIDocument.Document;
        }

        public FamilyInstance CreateFamily(RoomProperties rp)
        {
            FamilyInstance familyInstance = null;
            try
            {
                string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
                string massTemplate = Path.GetDirectoryName(currentAssembly) + "/Resources/Mass.rfa";
                Document familyDoc = null;
                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Open Document");
                    familyDoc = m_app.OpenDocumentFile(massTemplate);
                    trans.Commit();
                }

                if (null != familyDoc)
                {
                    bool createdParam = false;
                    using (Transaction trans = new Transaction(familyDoc))
                    {
                        FailureHandlingOptions failureHandlingOptions = trans.GetFailureHandlingOptions();
                        FailureHandler failureHandler = new FailureHandler();
                        failureHandlingOptions.SetFailuresPreprocessor(failureHandler);
                        failureHandlingOptions.SetClearAfterRollback(true);
                        trans.SetFailureHandlingOptions(failureHandlingOptions);

                        trans.Start("Create Extrusion");
                        try
                        {
                            FamilyType newFamilyType = familyDoc.FamilyManager.NewType(rp.Name);

                            bool createdMass = CreateExtrusion(familyDoc, rp);

                            Dictionary<string, Parameter> parameters = new Dictionary<string, Parameter>();
                            parameters = rp.Parameters;
                            if (createdMass) { createdParam = CreateNewParameters(familyDoc, parameters); }
         
                            if (failureHandler.FailureMessageInfoList.Count > 0)
                            {
                                failureMessage.AppendLine("[" + rp.ID + ": " + rp.Name + "] :" + failureHandler.FailureMessageInfoList[0].ErrorMessage);
                            }
                            
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to create family.\n" + ex.Message, "MassCreator: CreateFamily", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            trans.RollBack();
                        }
                    }
                    if (createdParam)
                    {
                        SaveAsOptions opt = new SaveAsOptions();
                        opt.OverwriteExistingFile = true;
                        string fileName = Path.Combine(massFolder, rp.ID + ".rfa");
                        familyDoc.SaveAs(fileName, opt);
                        familyDoc.Close(true);
                        familyInstance = LoadMassFamily(fileName, rp.Level);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create family.\n" + ex.Message, "MassCreator: CreateFamily", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return familyInstance;
        }

        public FamilyInstance CreateFamily(AreaProperties ap)
        {
            FamilyInstance familyInstance = null;
            try
            {
                string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
                string massTemplate = Path.GetDirectoryName(currentAssembly) + "/Resources/Mass.rfa";
                Document familyDoc = null;
                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Open Document");
                    familyDoc = m_app.OpenDocumentFile(massTemplate);
                    trans.Commit();
                }
                
                if (null != familyDoc)
                {
                    bool createdParam = false;
                    using (Transaction trans = new Transaction(familyDoc))
                    {
                        FailureHandlingOptions failureHandlingOptions = trans.GetFailureHandlingOptions();
                        FailureHandler failureHandler = new FailureHandler();
                        failureHandlingOptions.SetFailuresPreprocessor(failureHandler);
                        failureHandlingOptions.SetClearAfterRollback(true);
                        trans.SetFailureHandlingOptions(failureHandlingOptions);

                        trans.Start("Create Extrusion");
                        try
                        {
                            FamilyType newFamilyType = familyDoc.FamilyManager.NewType(ap.Name);
                            bool createdMass = CreateExtrusion(familyDoc, ap);

                            Dictionary<string, Parameter> parameters = new Dictionary<string, Parameter>();
                            parameters = ap.Parameters;
                            if (createdMass) { createdParam = CreateNewParameters(familyDoc, parameters); }
                            if (failureHandler.FailureMessageInfoList.Count > 0)
                            {
                                failureMessage.AppendLine("[" + ap.ID + ": " + ap.Name + "] :" + failureHandler.FailureMessageInfoList[0].ErrorMessage);
                            }
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to create family.\n" + ex.Message, "MassCreator: CreateFamily", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            trans.RollBack();
                        }
                    }
                    if (createdParam)
                    {
                        SaveAsOptions opt = new SaveAsOptions();
                        opt.OverwriteExistingFile = true;
                        string fileName = Path.Combine(massFolder, ap.ID + ".rfa");
                        familyDoc.SaveAs(fileName, opt);
                        familyDoc.Close(true);

                        familyInstance = LoadMassFamily(fileName, ap.Level);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create family.\n" + ex.Message, "MassCreator: CreateFamily", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return familyInstance;
        }

        public FamilyInstance CreateFamily(FloorProperties fp)
        {
            FamilyInstance familyInstance = null;
            try
            {
                string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
                string massTemplate = Path.GetDirectoryName(currentAssembly) + "/Resources/Mass.rfa";
                Document familyDoc = null;
                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Open Document");
                    familyDoc = m_app.OpenDocumentFile(massTemplate);
                    trans.Commit();
                }

                if (null != familyDoc)
                {
                    bool createdParam = false;
                    using (Transaction trans = new Transaction(familyDoc))
                    {
                        FailureHandlingOptions failureHandlingOptions = trans.GetFailureHandlingOptions();
                        FailureHandler failureHandler = new FailureHandler();
                        failureHandlingOptions.SetFailuresPreprocessor(failureHandler);
                        failureHandlingOptions.SetClearAfterRollback(true);
                        trans.SetFailureHandlingOptions(failureHandlingOptions);

                        trans.Start("Create Extrusion");
                        try
                        {
                            FamilyType newFamilyType = familyDoc.FamilyManager.NewType(fp.TypeName);
                            bool createdMass = CreateExtrusion(familyDoc, fp);

                            Dictionary<string, Parameter> parameters = new Dictionary<string, Parameter>();
                            parameters = fp.Parameters;
                            if (createdMass) { createdParam = CreateNewParameters(familyDoc, parameters); }
                            
                            if (failureHandler.FailureMessageInfoList.Count > 0)
                            {
                                failureMessage.AppendLine("[" + fp.ID + ": " + fp.TypeName + "] :" + failureHandler.FailureMessageInfoList[0].ErrorMessage);
                            }
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to create family.\n" + ex.Message, "MassCreator: CreateFamily", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            trans.RollBack();
                        }
                    }
                    if (createdParam)
                    {
                        SaveAsOptions opt = new SaveAsOptions();
                        opt.OverwriteExistingFile = true;
                        string fileName = Path.Combine(massFolder, fp.ID + ".rfa");
                        familyDoc.SaveAs(fileName, opt);
                        familyDoc.Close(true);
                        familyInstance = LoadMassFamily(fileName, fp.Level);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create family.\n" + ex.Message, "MassCreator: CreateFamily", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return familyInstance;
        }

        private bool CreateExtrusion(Document fdoc, RoomProperties rp)
        {
            bool result = false;
            try
            {
                if (true == fdoc.IsFamilyDocument)
                {
                    Plane plane = fdoc.Application.Create.NewPlane(XYZ.BasisZ, new XYZ(0, 0, 0));
#if RELEASE2013
                    SketchPlane skPlane = fdoc.FamilyCreate.NewSketchPlane(plane);
#elif RELEASE2014||RELEASE2015
                    SketchPlane skPlane = SketchPlane.Create(fdoc, plane);
#endif

                    double maxCircum = 0;//to find out the largest outer loop.
                    List<ReferenceArray> profiles = new List<ReferenceArray>();

                    foreach (EdgeArray edgeArray in rp.EdgeArrayArray)
                    {
                        Curve curve = null;
                        List<XYZ> pointList = new List<XYZ>();
                        ReferenceArray refArray = new ReferenceArray();
                        bool first = true;
                        double circumference = 0;

                        foreach (Edge edge in edgeArray)
                        {
                            circumference += edge.ApproximateLength;
                            int pointCount = edge.Tessellate().Count;
                            if (pointCount > 2)//edge from a circular face 
                            {
                                IList<XYZ> tPoints = edge.Tessellate();
                                tPoints.RemoveAt(tPoints.Count - 1);
                                foreach (XYZ point in tPoints)
                                {
                                    XYZ tempPoint = new XYZ(point.X, point.Y, 0);
                                    pointList.Add(tempPoint);
                                }
                            }
                            else if (pointCount == 2)
                            {
                                curve = edge.AsCurve();
#if RELEASE2013
                                XYZ point = curve.get_EndPoint(0);
#elif RELEASE2014||RELEASE2015
                                XYZ point = curve.GetEndPoint(0);
#endif

                                XYZ tempPoint = new XYZ(point.X, point.Y, 0);
                                if (first)
                                {
                                    pointList.Add(tempPoint); first = false;
                                }
                                else if (pointList[pointList.Count - 1].DistanceTo(tempPoint) > 0.0026)
                                {
                                    pointList.Add(tempPoint);
                                }
                            }
                        }

                        if (maxCircum == 0) { maxCircum = circumference; }
                        else if (maxCircum < circumference) { maxCircum = circumference; }

                        int num = pointList.Count;
                        if (num > 0)
                        {
                            for (int i = 0; i < num; i++)
                            {
                                if (i == num - 1)
                                {
#if RELEASE2013
                                    curve = doc.Application.Create.NewLineBound(pointList[i], pointList[0]);
#elif RELEASE2014||RELEASE2015
                                    curve = Autodesk.Revit.DB.Line.CreateBound(pointList[i], pointList[0]);
#endif
                                }
                                else
                                {
#if RELEASE2013
                                    curve = doc.Application.Create.NewLineBound(pointList[i], pointList[i + 1]);
#elif RELEASE2014||RELEASE2015
                                    curve = Autodesk.Revit.DB.Line.CreateBound(pointList[i], pointList[i + 1]);
#endif
                                }
                                ModelCurve modelCurve = fdoc.FamilyCreate.NewModelCurve(curve, skPlane);
                                refArray.Append(modelCurve.GeometryCurve.Reference);
                            }
                            //first index of profile list will be the outer loop
                            if (maxCircum == circumference) { profiles.Insert(0, refArray); }
                            else { profiles.Add(refArray); }
                        }
                    }

                    if (profiles.Count > 0)
                    {
                        double height = (rp.IsDefaultHeight == true) ? rp.DefaultHeight : rp.UnboundedHeight;
                        XYZ direction = new XYZ(0, 0, height);
                        FamilyParameter fparam = fdoc.FamilyManager.get_Parameter("Height");
                        fdoc.FamilyManager.Set(fparam, height);

                        for (int i = 0; i < profiles.Count; i++)
                        {
                            Autodesk.Revit.DB.Form solidForm = fdoc.FamilyCreate.NewExtrusionForm(true, profiles[i], direction);
                            CreateHeightLabel(fdoc, solidForm);

                            if (i > 0)
                            {
                                Parameter param = solidForm.get_Parameter(BuiltInParameter.ELEMENT_IS_CUTTING);
                                param.Set(1); //void form
                            }
                        }
                    }
                    result = true;
                }
                else
                {
                    result = false;
                    throw new Exception("Please open a Family document before invoking this command.");
                }
                return result;
            }
            catch (Exception ex)
            {
                result = false;
                MessageBox.Show("Failed to create a extruded form.\n\nRoom Number: " + rp.Number + "  Room Name: " + rp.Name + "\nPlease Make the room boundary lines simple if there's overlapping lines.\n\n" + ex.Message, "MassCreator: CreateExtrusion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return result;
            }
        }

        private bool CreateExtrusion(Document fdoc, AreaProperties ap)
        {
            bool result = false;
            try
            {
                // make sure we have a family document
                if (true == fdoc.IsFamilyDocument)
                {
                    Plane plane = fdoc.Application.Create.NewPlane(XYZ.BasisZ, new XYZ(0, 0, 0));
#if RELEASE2013
                    SketchPlane skPlane = fdoc.FamilyCreate.NewSketchPlane(plane);
#elif RELEASE2014||RELEASE2015
                    SketchPlane skPlane = SketchPlane.Create(fdoc, plane);
#endif

                    double maxCircum = 0;//to find out the largest outer loop.
                    List<ReferenceArray> profiles = new List<ReferenceArray>();

                    foreach (CurveArray curveArray in ap.CurveArrArray)
                    {
                        List<XYZ> pointList = new List<XYZ>();
                        ReferenceArray refArray = new ReferenceArray();
                        double circumference = 0;

                        foreach (Curve curve in curveArray)
                        {
                            circumference += curve.ApproximateLength;
                            int pointCount = curve.Tessellate().Count;
                            if (pointCount > 2)//edge from a circular face 
                            {
                                IList<XYZ> tPoints = curve.Tessellate();
                                tPoints.RemoveAt(tPoints.Count - 1);
                                foreach (XYZ point in tPoints)
                                {
                                    XYZ tempPoint = new XYZ(point.X, point.Y, 0);
                                    pointList.Add(tempPoint);
                                }
                            }
                            else if (pointCount == 2)
                            {
#if RELEASE2013
                                XYZ pt = curve.get_EndPoint(0);
#elif RELEASE2014||RELEASE2015
                                XYZ pt = curve.GetEndPoint(0);
#endif
                                XYZ tempPoint = new XYZ(pt.X, pt.Y, 0);
                                if (pointList.Count == 0) { pointList.Add(tempPoint); }
                                if (pointList.Count > 0 && pointList[pointList.Count - 1].DistanceTo(tempPoint) > 0.0026)//revit tolerance will be 1/32"
                                {
                                    pointList.Add(tempPoint);
                                }
                            }
                        }

                        if (maxCircum == 0) { maxCircum = circumference; }
                        else if (maxCircum < circumference) { maxCircum = circumference; }

                        int num = pointList.Count;
                        if (num > 2)
                        {
                            Curve newCurve = null;
                            for (int i = 0; i < pointList.Count; i++)
                            {
                                if (i == num - 1)
                                {
#if RELEASE2013
                                    newCurve = fdoc.Application.Create.NewLineBound(pointList[i], pointList[0]);
#elif RELEASE2014||RELEASE2015
                                    newCurve = Autodesk.Revit.DB.Line.CreateBound(pointList[i], pointList[0]);
#endif
                                }
                                else
                                {
#if RELEASE2013
                                    newCurve = fdoc.Application.Create.NewLineBound(pointList[i], pointList[i + 1]);
#elif RELEASE2014||RELEASE2015
                                    newCurve = Autodesk.Revit.DB.Line.CreateBound(pointList[i], pointList[i + 1]);
#endif
                                }
                                ModelCurve modelcurve = fdoc.FamilyCreate.NewModelCurve(newCurve, skPlane);
                                refArray.Append(modelcurve.GeometryCurve.Reference);
                            }
                            //first index of profile list will be the outer loop
                            if (maxCircum == circumference) { profiles.Insert(0, refArray); }
                            else { profiles.Add(refArray); }
                        }
                    }

                    if (profiles.Count > 0)
                    {
                        XYZ direction = new XYZ(0, 0, ap.Height);
                        FamilyParameter fparam = fdoc.FamilyManager.get_Parameter("Height");
                        fdoc.FamilyManager.Set(fparam, ap.Height);

                        for (int i = 0; i < profiles.Count; i++)
                        {
                            Autodesk.Revit.DB.Form solidForm = fdoc.FamilyCreate.NewExtrusionForm(true, profiles[i], direction);
                            CreateHeightLabel(fdoc, solidForm);

                            if (i > 0)
                            {
                                Parameter param = solidForm.get_Parameter(BuiltInParameter.ELEMENT_IS_CUTTING);
                                param.Set(1); //void form
                            }
                        }
                    }
                    result = true;
                }
                else
                {
                    result = false;
                    throw new Exception("Please open a Family document before invoking this command.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create extrusion.\n" + ex.Message, "MassCreator: CreateExtrusion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            return result;
        }

        private bool CreateExtrusion(Document fdoc, FloorProperties fp)
        {
            bool result = false;
            try
            {
                if (true == fdoc.IsFamilyDocument)
                {
                    double maxCircum = 0;//to find out the largest outer loop.
                    List<ReferenceArray> profiles = new List<ReferenceArray>();

                    foreach (EdgeArray edgeArray in fp.EdgeArrayArray)
                    {
                        Curve curve = null;
                        List<XYZ> pointList = new List<XYZ>();
                        ReferenceArray refArray = new ReferenceArray();
                        bool first = true;
                        double circumference = 0;

                        foreach (Edge edge in edgeArray)
                        {
                            circumference += edge.ApproximateLength;
                            int pointCount = edge.Tessellate().Count;
                            if (pointCount > 2)//edge from a circular face 
                            {
                                IList<XYZ> tPoints = edge.Tessellate();
                                tPoints.RemoveAt(tPoints.Count - 1);
                                foreach (XYZ point in tPoints)
                                {
                                    XYZ tempPoint = new XYZ(point.X, point.Y, 0);
                                    pointList.Add(tempPoint);
                                }
                            }
                            else if (pointCount == 2)
                            {
                                curve = edge.AsCurve();
#if RELEASE2013
                                XYZ point = curve.get_EndPoint(0);
#elif RELEASE2014||RELEASE2015
                                XYZ point = curve.GetEndPoint(0);
#endif
                                XYZ tempPoint = new XYZ(point.X, point.Y, 0);
                                if (first)
                                {
                                    pointList.Add(tempPoint); first = false;
                                }
                                else if (pointList[pointList.Count - 1].DistanceTo(tempPoint) > 0.0026)
                                {
                                    pointList.Add(tempPoint);
                                }
                            }
                        }

                        if (maxCircum == 0) { maxCircum = circumference; }
                        else if (maxCircum < circumference) { maxCircum = circumference; }

                        int num = pointList.Count;
                        if (num > 0)
                        {
                            Plane plane = fdoc.Application.Create.NewPlane(XYZ.BasisZ, new XYZ(0, 0, 0));
#if RELEASE2013
                            SketchPlane skPlane = fdoc.FamilyCreate.NewSketchPlane(plane);
#elif RELEASE2014||RELEASE2015
                            SketchPlane skPlane = SketchPlane.Create(fdoc, plane);
#endif

                            for (int i = 0; i < num; i++)
                            {
                                if (i == num - 1)
                                {
#if RELEASE2013
                                    curve = fdoc.Application.Create.NewLineBound(pointList[i], pointList[0]);
#elif RELEASE2014||RELEASE2015
                                    curve = Autodesk.Revit.DB.Line.CreateBound(pointList[i], pointList[0]);
#endif
                                }
                                else
                                {
#if RELEASE2013
                                    curve = fdoc.Application.Create.NewLineBound(pointList[i], pointList[i + 1]);
#elif RELEASE2014||RELEASE2015
                                    curve = Autodesk.Revit.DB.Line.CreateBound(pointList[i], pointList[i + 1]);
#endif
                                }
                                ModelCurve modelCurve = fdoc.FamilyCreate.NewModelCurve(curve, skPlane);
                                refArray.Append(modelCurve.GeometryCurve.Reference);
                            }
                            //first index of profile list will be the outer loop
                            if (maxCircum == circumference) { profiles.Insert(0, refArray); }
                            else { profiles.Add(refArray); }
                        }
                    }

                    if (profiles.Count > 0)
                    {
                        XYZ direction = new XYZ(0, 0, fp.Height);
                        FamilyParameter fparam = fdoc.FamilyManager.get_Parameter("Height");
                        fdoc.FamilyManager.Set(fparam, fp.Height);

                        for (int i = 0; i < profiles.Count; i++)
                        {
                            Autodesk.Revit.DB.Form solidForm = fdoc.FamilyCreate.NewExtrusionForm(true, profiles[i], direction);
                            CreateHeightLabel(fdoc, solidForm);

                            if (i > 0)
                            {
                                Parameter param = solidForm.get_Parameter(BuiltInParameter.ELEMENT_IS_CUTTING);
                                param.Set(1); //void form
                            }
                        }
                    }
                    result = true;
                }
                else
                {
                    result = false;
                    throw new Exception("Please open a Family document before invoking this command.");
                }
                return result;
            }
            catch (Exception ex)
            {
                result = false;
                MessageBox.Show("Failed to create an extruded form.\n\nFloorName: " + fp.TypeName + "  FloorLevel: " + fp.Level + "\nBoundary lines of floor cannot create a profile..\n\n" + ex.Message, "MassCreator: CreateExtrusion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return result;
            }
        }

        private void CreateHeightLabel(Document fdoc, Autodesk.Revit.DB.Form extruedForm)
        {
            try
            {
                Options opt = m_app.Create.NewGeometryOptions();
                opt.ComputeReferences = true;
                opt.IncludeNonVisibleObjects = true;

                PlanarFace topFace = null;

                GeometryElement geometryElement = extruedForm.get_Geometry(opt);
                foreach (GeometryObject obj in geometryElement)
                {
                    Solid solid = obj as Solid;
                    if (solid != null)
                    {
                        UV uv = new UV(0, 0);
                        foreach (Face face in solid.Faces)
                        {
                            XYZ normal = face.ComputeNormal(uv);

                            if (normal.IsAlmostEqualTo(new XYZ(0, 0, 1)))//top face
                            {
                                topFace = face as PlanarFace;
                            }
                        }
                    }
                }

                if (null != topFace)
                {
                    Dimension dimension = fdoc.FamilyCreate.NewAlignment(FindView(fdoc, "South"), FindLevel(fdoc, "Level 2").PlaneReference, topFace.Reference);
                    dimension.IsLocked = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a dimension for Height.\n" + ex.Message, "MassCreator : CreateHeightLabel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private Level FindLevel(Document fdoc, string levelName)
        {
            Level level = null;
            FilteredElementCollector collector = new FilteredElementCollector(fdoc);
            List<Element> dimensionTypes = collector.OfClass(typeof(Level)).ToElements().ToList();
            var query = from element in dimensionTypes
                        where element.Name == levelName
                        select element;
            level = query.ToList().First() as Level;
            return level;
        }

        private Autodesk.Revit.DB.View FindView(Document fdoc, string viewName)
        {
            Autodesk.Revit.DB.View view = null;
            FilteredElementCollector collector = new FilteredElementCollector(fdoc);
            List<Element> Views = collector.OfClass(typeof(Autodesk.Revit.DB.View)).ToElements().ToList();
            var query = from element in Views
                        where element.Name == viewName
                        select element;
            view = query.ToList().First() as Autodesk.Revit.DB.View;
            return view;
        }

        private bool CreateNewParameters(Document familyDoc, Dictionary<string, Parameter> parameters)
        {
            bool result = false;
            try
            {
                FamilyManager familyManager = familyDoc.FamilyManager;

                foreach (string paramName in defDictionary.Keys)
                {
                    ExternalDefinition extDefinition = defDictionary[paramName] as ExternalDefinition;
                    FamilyParameter familyParam = familyManager.AddParameter(extDefinition, BuiltInParameterGroup.INVALID, true);
                    string originParamName = paramName.Replace("Mass_", "");
                    if (parameters.ContainsKey(originParamName))
                    {
                        Parameter param = parameters[originParamName];

                        switch (param.StorageType)
                        {
                            case StorageType.Double:
                                familyManager.Set(familyParam, param.AsDouble());
                                break;
                            case StorageType.Integer:
                                familyManager.Set(familyParam, param.AsInteger());
                                break;
                            case StorageType.String:
                                if (null != param.AsString())
                                {
                                    familyManager.Set(familyParam, param.AsString());
                                }
                                break;
                        }
                    }
                }
                result = true;
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create new instance parameters.\n" + ex.Message, "MassCreator:CreateNewParameters", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return result;
            }
        }

        private FamilyInstance LoadMassFamily(string fileName, string levelName)
        {
            FamilyInstance familyInstance = null;
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Load Mass Family");
                try
                {
                    Family newFamily = null;
                    doc.LoadFamily(fileName, new FamilyOption(), out newFamily);

#if RELEASE2013||RELEASE2014
                    FamilySymbolSetIterator symbolIterator = newFamily.Symbols.ForwardIterator();
                    symbolIterator.MoveNext();
                    FamilySymbol symbol = symbolIterator.Current as FamilySymbol;
                    FilteredElementCollector collector = new FilteredElementCollector(doc);
                    SketchPlane skPlane = collector.OfClass(typeof(SketchPlane)).First<Element>(e => e.Name.Equals(levelName)) as SketchPlane;

#if RELEASE2013
                    familyInstance = doc.Create.NewFamilyInstance(new XYZ(0, 0, skPlane.Plane.Origin.Z), symbol, skPlane, StructuralType.NonStructural);
#elif RELEASE2014
                    familyInstance = doc.Create.NewFamilyInstance(new XYZ(0, 0, skPlane.GetPlane().Origin.Z), symbol, skPlane, StructuralType.NonStructural);
#endif

#elif RELEASE2015
                    List<ElementId> elementIds = newFamily.GetFamilySymbolIds().ToList();
                    if (elementIds.Count > 0)
                    {
                        ElementId symbolId = elementIds.First();
                        FamilySymbol fSymbol = doc.GetElement(symbolId) as FamilySymbol;
                        if (null != fSymbol)
                        {
                            FilteredElementCollector collector = new FilteredElementCollector(doc);
                            SketchPlane skPlane = collector.OfClass(typeof(SketchPlane)).First<Element>(e => e.Name.Equals(levelName)) as SketchPlane;
                            familyInstance = doc.Create.NewFamilyInstance(new XYZ(0, 0, skPlane.GetPlane().Origin.Z), fSymbol, skPlane, StructuralType.NonStructural);
                        }
                    }
#endif
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load mass family." + ex.Message, "MassCreator:LoadMassFamily", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    trans.RollBack();
                }
            }
            return familyInstance;
        }
    }

    public class FamilyOption : IFamilyLoadOptions
    {

        bool IFamilyLoadOptions.OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            overwriteParameterValues = true;
            return true;
        }

        bool IFamilyLoadOptions.OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            source = FamilySource.Family;
            overwriteParameterValues = true;
            return true;
        }
    }

    public class FailureHandler : IFailuresPreprocessor
    {
        private List<FailureMessageInfo> failureMessageInfoList = new List<FailureMessageInfo>();

        public List<FailureMessageInfo> FailureMessageInfoList { get { return failureMessageInfoList; } set { failureMessageInfoList = value; } }

        
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {

            IList<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();
            //if (failureMessages.Count == 0) { return FailureProcessingResult.Continue; }

            bool needRollBack = false;
            string transactionName = failuresAccessor.GetTransactionName();
            foreach (FailureMessageAccessor fma in failureMessages)
            {
                FailureMessageInfo messageInfo = new FailureMessageInfo();
                try { messageInfo.ErrorMessage = fma.GetDescriptionText(); }
                catch { messageInfo.ErrorMessage = "Unknown Error"; }

                FailureSeverity severity = fma.GetSeverity();
                try
                {
                    if (severity == FailureSeverity.Warning)
                    {
                        failuresAccessor.DeleteWarning(fma);
                    }
                    else
                    {
                        messageInfo.ErrorSeverity = severity.ToString();
                        messageInfo.FailingElementIds = fma.GetFailingElementIds().ToList();
                        failureMessageInfoList.Add(messageInfo);
                        needRollBack = true;
                    }
                }
                catch { }
            }

            if (needRollBack) { return FailureProcessingResult.ProceedWithRollBack; }
            else { return FailureProcessingResult.Continue; }
        }
    }

    public class FailureMessageInfo
    {
        public string ErrorMessage { get; set; }
        public string ErrorSeverity { get; set; }
        public List<ElementId> FailingElementIds { get; set; }
    }
}
