using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using Autodesk.Revit.DB.Structure;
using HOK.RoomsToMass.ToMass;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Electrical;

namespace HOK.RoomsToMass.ParameterAssigner
{
    public class ElementSpliter
    {
        private BuiltInParameter[] parametersToSkip = new BuiltInParameter[] 
        { 
            BuiltInParameter.WALL_USER_HEIGHT_PARAM/*wall*/, 
            BuiltInParameter.WALL_BASE_OFFSET/*wall*/,
            BuiltInParameter.WALL_TOP_OFFSET/*wall*/,
            BuiltInParameter.SCHEDULE_BASE_LEVEL_OFFSET_PARAM/*columns*/,
            BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM/*columns*/,
            BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM/*columns*/, 
            BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM/*columns*/,
            BuiltInParameter.RBS_OFFSET_PARAM/*pipes*/
        };

        private Document primaryDoc;

        public ElementSpliter(Document document)
        {
            primaryDoc = document;
        }

        public ElementProperties SplitElement( ElementProperties ep)
        {
            ElementProperties newEP = ep;
            //Dictionary<int, FamilyInstanceProperties> familyinstances = new Dictionary<int, FamilyInstanceProperties>();
            List<Element> primaryElements = new List<Element>();
            List<Element> secondaryElements = new List<Element>();
            try
            {
#if RELEASE2014 || RELEASE2015  || RELEASE2016 || RELEASE2017
                if (ep.LinkedElement)
                {
                    using (Transaction trans = new Transaction(primaryDoc))
                    {
                        trans.Start("Copy Element");
                        FailureHandlingOptions failureHandlingOptions = trans.GetFailureHandlingOptions();
                        FailureHandler failureHandler = new FailureHandler();
                        failureHandlingOptions.SetFailuresPreprocessor(failureHandler);
                        failureHandlingOptions.SetClearAfterRollback(true);
                        trans.SetFailureHandlingOptions(failureHandlingOptions);
                        try
                        {
                            List<ElementId> toCopy = new List<ElementId>();
                            toCopy.Add(ep.ElementObj.Id);
                            if (ep.ElementObj is HostObject)
                            {
                                HostObject host = ep.ElementObj as HostObject;
                                IList<ElementId> eIds = host.FindInserts(false, false, false, false);
                                toCopy.AddRange(eIds);
                            }

                            CopyPasteOptions options = new CopyPasteOptions();
                            options.SetDuplicateTypeNamesHandler(new HideAndAcceptDuplicateTypeNamesHandler());
                            ICollection<ElementId> copiedElements = ElementTransformUtils.CopyElements(ep.Doc, toCopy, primaryDoc, ep.TransformValue, options);
                            if (copiedElements.Count > 0)
                            {
                                Element copiedElement = primaryDoc.GetElement(copiedElements.First());
                                if (null != copiedElement)
                                {
                                    ep.CopiedElement = copiedElement;
                                    ep.CopiedElementId = copiedElement.Id;
                                }
                            }
                            trans.Commit();
                        }
                        catch
                        {
                            trans.RollBack();
                        }
                    }
                }
#elif RELEASE2013
                
#endif
                if (null == ep.CopiedElement) { ep.SplitSucceed = false; return ep; }

                using (Transaction trans = new Transaction(primaryDoc))
                {
                    FailureHandlingOptions failureHandlingOptions = trans.GetFailureHandlingOptions();
                    FailureHandler failureHandler = new FailureHandler();
                    failureHandlingOptions.SetFailuresPreprocessor(failureHandler);
                    failureHandlingOptions.SetClearAfterRollback(true);
                    trans.SetFailureHandlingOptions(failureHandlingOptions);

                    switch (ep.CategoryName)
                    {
                        case "Floors":
                            trans.Start("New Split Floors");
                            primaryElements = NewPrimaryFloor(ep);
                            secondaryElements = NewSecondaryFloor(ep);
                            break;
                        case "Walls":
                            Solid profileSolid= GetNativeWallSolid(ep, trans);
                            trans.Start("New Split Walls");
                            if (null != profileSolid) //by profile solid
                            {
                                primaryElements = NewPrimaryWall(ep, profileSolid);
                                secondaryElements = NewSecondaryWall(ep, profileSolid);
                            }
                            else //by location curve
                            {
                                SplitWalls(ep, out primaryElements, out secondaryElements);
                            }
                            trans.Commit();
                            trans.Start("Place Family Instances");
                            if (primaryElements.Count > 0 && secondaryElements.Count > 0)
                            {
                                PlaceFamilyInstances(ep, primaryElements, secondaryElements);
                            }
                            
                            break;
                        case "Roofs":
                            trans.Start("New Split Roofs");
                            primaryElements = NewPrimaryRoof(ep);
                            secondaryElements = NewSecondaryRoof( ep);
                            break;
                        case "Ducts":
                            trans.Start("New Split Ducts");
                            SplitDucts(ep, out primaryElements, out secondaryElements);
                            break;
                        case "Pipes":
                            trans.Start("New Split Pipes");
                            SplitPipes(ep, out primaryElements, out secondaryElements);
                            break;
                        case "Conduits":
                            trans.Start("New Split Conduits");
                            SplitConduit(ep, out primaryElements, out secondaryElements);
                            break;
                        case "Structural Framing":
                            trans.Start("New Split Structural Framing");
                            SplitBeam(ep, out primaryElements, out secondaryElements);
                            break;
                        case "Structural Columns":
                            trans.Start("New Split Structural Columns");
                            SplitColumns(ep, out primaryElements, out secondaryElements);
                            break;
                        case "Columns":
                            trans.Start("New Split Columns");
                            SplitColumns(ep, out primaryElements, out secondaryElements);
                            break;
                    }
                    if (primaryElements.Count > 0 && secondaryElements.Count > 0)
                    {
                        ep.PrimaryElements = primaryElements;
                        ep.SecondaryElements = secondaryElements;
                        trans.Commit();
                        
                        if (failureHandler.FailureMessageInfoList.Count > 0)
                        {
                            ep.SplitSucceed = false;
                            LogFileManager.AppendLog("SplitElement", primaryDoc, ep.ElementObj, failureHandler.FailureMessageInfoList);
                        }
                        else
                        {
                            ep.SplitSucceed = true;
                        }
                    }
                    else
                    {
                        trans.RollBack();
                        ep.SplitSucceed = false;
                    }
                }
                if (ep.SplitSucceed)
                {
                    ep = TransferParameterValues(ep);
                }
                
                //if fails, return null
            }
            catch(Exception ex) 
            {
                string message = ex.Message;
                //MessageBox.Show("["+ep.ElementId+"]"+ep.ElementName+"\nFailed to split element.\n" + ex.Message, "ElementSpliter:SplitElement", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //LogFileManager.AppendLog("SplitElement", ex.Message);
            }
            return newEP;
        }

        #region new floors
        private List<Element> NewPrimaryFloor(ElementProperties ep)
        {
            List<Element> primaryElements = new List<Element>();
            try
            {
                Solid massSolid = ep.MassContainers[ep.SelectedMassId];
                Solid intersectSolid = BooleanOperationsUtils.ExecuteBooleanOperation(massSolid, ep.ElementSolid, BooleanOperationsType.Intersect);
                List<CurveArray> profiles = GetFloorBoundary(intersectSolid);

                Floor floor = ep.CopiedElement as Floor;
                FloorType floortype = floor.FloorType;
#if RELEASE2013
                ElementId levelId = floor.Level.Id;
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                ElementId levelId = floor.LevelId;
#endif
                Level level = primaryDoc.GetElement(levelId) as Level;
                bool structural = floortype.IsFoundationSlab;
                foreach (CurveArray profile in profiles)
                {
                    Floor primaryFloor = primaryDoc.Create.NewFloor(profile, floortype, level, structural);
                    if (null != primaryFloor)
                    {
                        primaryElements.Add(primaryFloor);
                    }
                }
            }
            catch(Exception ex)
            {
                string message = ex.Message;
                //MessageBox.Show("[" + ep.ElementId + "]" + ep.ElementName + "\nFailed to split floors.\n" + ex.Message, "ElementSpliter:SplitFloor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //LogFileManager.AppendLog("NewPrimaryFloor", ex.Message);
            }
            return primaryElements;
        }

        private List<Element> NewSecondaryFloor(ElementProperties ep)
        {
            List<Element> secondaryElements = new List<Element>();
            try
            {
                Solid massSolid = ep.MassContainers[ep.SelectedMassId];
                Solid differenceSolid = BooleanOperationsUtils.ExecuteBooleanOperation(ep.ElementSolid, massSolid, BooleanOperationsType.Difference);
                List<CurveArray> profiles = GetFloorBoundary(differenceSolid);
                Floor floor = ep.CopiedElement as Floor;
                FloorType floortype = floor.FloorType;
#if RELEASE2013
                ElementId levelId = floor.Level.Id;
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                ElementId levelId = floor.LevelId;
#endif
                Level level = primaryDoc.GetElement(levelId) as Level;
                bool structural = floortype.IsFoundationSlab;
                foreach (CurveArray profile in profiles)
                {
                    Floor secondaryFloor = primaryDoc.Create.NewFloor(profile, floortype, level, structural);
                    if (null != secondaryFloor)
                    {
                        secondaryElements.Add(secondaryFloor);
                    }
                }
            }
            catch(Exception ex) 
            {
                string message = ex.Message;
                //MessageBox.Show("[" + ep.ElementId + "]" + ep.ElementName + "\nFailed to split floors.\n" + ex.Message, "ElementSpliter:SplitFloor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //LogFileManager.AppendLog("NewSecondaryFloor", ex.Message);
            }
            return secondaryElements;
        }

        private List<CurveArray> GetFloorBoundary(Solid solid)
        {
            List<CurveArray> profileList = new List<CurveArray>();
            try
            {
                foreach (Face face in solid.Faces)
                {
                    if (face is PlanarFace)
                    {
                        XYZ normal = ComputeNormalOfFace(face);
                        if (normal.Z > 0)
                        {
                            if (Math.Abs(normal.Z) > Math.Abs(normal.X) && Math.Abs(normal.Z) > Math.Abs(normal.Y))
                            {
                                foreach (EdgeArray edgeArray in face.EdgeLoops)
                                {
                                    CurveArray curveArray = new CurveArray();
                                    foreach (Edge edge in edgeArray)
                                    {
                                        Curve curve = edge.AsCurveFollowingFace(face);
                                        curveArray.Append(curve);
                                    }
                                    profileList.Add(curveArray);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                //MessageBox.Show("Failed to get boundary lines from solid.\n" + ex.Message, "ElementSpliter:GetFloorBoundary", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //LogFileManager.AppendLog("GetFloorBoundary", ex.Message);
            }
            return profileList;
        }
        #endregion

        #region new walls
        private Solid GetNativeWallSolid(ElementProperties ep, Transaction trans)
        {
            Solid wallSolid = null;
            try
            {
                trans.Start("Delete a Wall");
                Wall wall=ep.CopiedElement as Wall;
                ICollection<ElementId> elementIds = primaryDoc.Delete(ep.CopiedElementId);
                trans.RollBack();

                Sketch sketch = null;
                foreach (ElementId eId in elementIds)
                {
                    Element element = primaryDoc.GetElement(eId);
                    if (element is Sketch)
                    {
                        sketch = element as Sketch;
                    }
                }

                if (null != sketch)
                {
                    CurveArrArray profile = sketch.Profile;
                    List<CurveLoop> curveLoops = new List<CurveLoop>();
                    foreach (CurveArray curveArray in profile)
                    {
                        CurveLoop curveLoop = new CurveLoop();
                        foreach (Curve curve in curveArray)
                        {
                            curveLoop.Append(curve);
                        }
                        curveLoops.Add(curveLoop);
                    }
                    try { wallSolid = GeometryCreationUtilities.CreateExtrusionGeometry(curveLoops, wall.Orientation, wall.Width * 0.5); }
                    catch { wallSolid = null; }
                }
            }
            catch 
            {
                //MessageBox.Show("Failed to get wall solid.\n" + ex.Message, "ElementSpliter:GetNativeWallSolid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //LogFileManager.AppendLog("GetNativeWallSolid", ex.Message);
            }
            return wallSolid;
        }
        
        private List<Element> NewPrimaryWall(ElementProperties ep, Solid profileSolid)
        {
            List<Element> primaryElements = new List<Element>();
            try
            {
                ep.ElementSolid = profileSolid;
                Solid massSolid = ep.MassContainers[ep.SelectedMassId];
                Solid intersectSolid = BooleanOperationsUtils.ExecuteBooleanOperation(massSolid, ep.ElementSolid, BooleanOperationsType.Intersect);
                List<Curve> profile = GetWallProfile(ep, intersectSolid);
                if (profile.Count > 0)
                {
                    Wall wall = ep.CopiedElement as Wall;
                    ElementId wallTypeId = wall.WallType.Id;
#if RELEASE2013
                    ElementId levelId = wall.Level.Id;
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                    ElementId levelId = wall.LevelId;
#endif

                    bool isStructural = true;
                    if (wall.StructuralUsage == StructuralWallUsage.NonBearing)
                    {
                        isStructural = false;
                    }
                    Wall primaryWall = null;
                    try { primaryWall = Wall.Create(primaryDoc, profile, wallTypeId, levelId, isStructural); primaryDoc.Regenerate(); }
                    catch { return primaryElements; }
                    
                    if (null != primaryWall)
                    {
                        double angle = wall.Orientation.AngleTo(primaryWall.Orientation);
                        if (Math.Round(angle, 2) > 0) { primaryWall.Flip(); }
                        primaryElements.Add(primaryWall);
                    }
                }
            }
            catch 
            {
                //MessageBox.Show("[" + ep.ElementId + "] " + ep.ElementName + "\nFailed to split walls.\n" + ex.Message, "ElementSpliter:NewPrimaryWall", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //LogFileManager.AppendLog("NewPrimaryWall", ex.Message);
            }
            return primaryElements;
        }

        private List<Element> NewSecondaryWall(ElementProperties ep, Solid profileSolid)
        {
            List<Element> secondaryElements = new List<Element>();
            try
            {
                ep.ElementSolid = profileSolid;
                Solid massSolid = ep.MassContainers[ep.SelectedMassId];
                Solid differenceSolid = BooleanOperationsUtils.ExecuteBooleanOperation(ep.ElementSolid, massSolid, BooleanOperationsType.Difference);
                List<Curve> profile = GetWallProfile(ep, differenceSolid);
                if (profile.Count > 0)
                {
                    Wall wall = ep.CopiedElement as Wall;
                    ElementId wallTypeId = wall.WallType.Id;
#if RELEASE2013
                    ElementId levelId = wall.Level.Id;
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                    ElementId levelId = wall.LevelId;
#endif

                    bool isStructural = true;
                    if (wall.StructuralUsage == StructuralWallUsage.NonBearing)
                    {
                        isStructural = false;
                    }
                    Wall secondaryWall = null;
                    try { secondaryWall = Wall.Create(primaryDoc, profile, wallTypeId, levelId, isStructural); primaryDoc.Regenerate(); }
                    catch { return secondaryElements; }

                    if (null != secondaryWall)
                    {
                        double angle = wall.Orientation.AngleTo(secondaryWall.Orientation);
                        if (Math.Round(angle, 2) > 0) { secondaryWall.Flip(); }

                        secondaryElements.Add(secondaryWall);
                    }
                }
            }
            catch 
            {
                //MessageBox.Show("[" + ep.ElementId + "] " + ep.ElementName + "\nFailed to split walls.\n" + ex.Message, "ElementSpliter:NewSecondaryWall", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //LogFileManager.AppendLog("NewSecondaryWall", ex.Message);
            }
            return secondaryElements;
        }

        private void SplitWalls(ElementProperties ep, out List<Element> primaryElementsList, out List<Element> secondaryElementsList)
        {
            primaryElementsList = new List<Element>();
            secondaryElementsList = new List<Element>();
            try
            {
                Wall wall = ep.CopiedElement as Wall;
                double height=wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
                double offset=wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble();
                bool wallStructural=true;
                if(wall.StructuralUsage==StructuralWallUsage.NonBearing) { wallStructural=false; }

                LocationCurve locationCurve = wall.Location as LocationCurve;
                Curve wallCurve = locationCurve.Curve;

                XYZ interPoint;
                List<Curve> newLocationCurves = GetWallLocationCurve(ep.MassContainers[ep.SelectedMassId], wallCurve, out interPoint);
                if (newLocationCurves.Count == 2)
                {
                    Wall newWall = null;
#if RELEASE2013
                    ElementId wallLevelId = wall.Level.Id;
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                    ElementId wallLevelId = wall.LevelId;
#endif

                    try { newWall = Wall.Create(primaryDoc, newLocationCurves[0], wall.WallType.Id, wallLevelId, height, offset, wall.Flipped, wallStructural); }
                    catch { }
                    if (null != newWall) { primaryElementsList.Add(newWall); }

                    newWall = null;
                    try { newWall = Wall.Create(primaryDoc, newLocationCurves[1], wall.WallType.Id, wallLevelId, height, offset, wall.Flipped, wallStructural); }
                    catch { }
                    if (null != newWall) { secondaryElementsList.Add(newWall); }
                }
            }
            catch 
            {
                //MessageBox.Show("Failed to split Wall.\n" + ex.Message, "ElementSpliter:SplitWalls", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //LogFileManager.AppendLog("SplitWalls", ex.Message);
            }
        }

        private List<Curve> GetWallProfile(ElementProperties ep, Solid solid)
        {
            List<Curve> profile = new List<Curve>();
            try
            {
                Wall wall = ep.CopiedElement as Wall;
                XYZ orientation = wall.Orientation;
                XYZ offsetVector = orientation.Normalize().Negate().Multiply(0.5 * wall.Width);
#if RELEASE2013
                Transform transform = Transform.get_Translation(offsetVector);
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                Transform transform = Transform.CreateTranslation(offsetVector);
#endif

                foreach (Face face in solid.Faces)
                {
                    XYZ normal = ComputeNormalOfFace(face);
                    if (normal.AngleTo(orientation) == 0) //paralle to the orientation with same direction
                    {
                        foreach (EdgeArray edgeArray in face.EdgeLoops)
                        {
                            foreach (Edge edge in edgeArray)
                            {
                                Curve curve = edge.AsCurve();
#if RELEASE2013
                                curve = curve.get_Transformed(transform);
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                                curve = curve.CreateTransformed(transform);
#endif
                                profile.Add(curve);
                            }
                        }
                        break;
                    }
                }
            }
            catch 
            {
                //MessageBox.Show("Failed to get boundary lines from solid.\n" + ex.Message, "ElementSpliter:GetWallProfile", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
            return profile;
        }

        private void PlaceFamilyInstances(ElementProperties ep, List<Element> primaryElements, List<Element> secondaryElements)
        {
            Wall wall = ep.CopiedElement as Wall;
            try
            {
                IList<ElementId> attachedElementIds = wall.FindInserts(false, false, false, false);
                List<FamilyInstance> familyInstances = new List<FamilyInstance>();
                foreach (ElementId elementId in attachedElementIds)
                {
                    Element element = primaryDoc.GetElement(elementId);
                    if (element is FamilyInstance)
                    {
                        familyInstances.Add(element as FamilyInstance);
                    }
                }
                Dictionary<Element, Outline> outlines = new Dictionary<Element, Outline>();
                foreach (Element element in primaryElements)
                {
                    BoundingBoxXYZ box = element.get_BoundingBox(null);
                    if(null!=box)
                    {
                        Outline outline = new Outline(box.Min, box.Max);
                        outlines.Add(element, outline);
                    }
                }
                foreach (Element element in secondaryElements)
                {
                    BoundingBoxXYZ box = element.get_BoundingBox(null);
                    if (null != box)
                    {
                        Outline outline = new Outline(box.Min, box.Max);
                        outlines.Add(element, outline);
                    }
                }

                foreach (FamilyInstance familyInstance in familyInstances)
                {
                    LocationPoint location = familyInstance.Location as LocationPoint;
#if RELEASE2013
                    ElementId elementId = familyInstance.Level.Id;
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                    ElementId elementId = familyInstance.LevelId;
#endif
                    Level instanceLevel = primaryDoc.GetElement(elementId) as Level;
                    XYZ point = location.Point;
                    foreach (Element element in outlines.Keys)
                    {
                        Outline outline = outlines[element];
                        if (outline.Contains(point,0))
                        {
                            FamilyInstance newInstance = null;
                            try { newInstance = primaryDoc.Create.NewFamilyInstance(point, familyInstance.Symbol, element, instanceLevel, familyInstance.StructuralType); }
                            catch { continue; }
                            if (null != newInstance)
                            {
                                TransferInstanceData(familyInstance, newInstance);
                                break;
                            }
                        }
                    }
                }
            }
            catch 
            {
                //MessageBox.Show("ElementName: " + wall.Name + "ElementId: " + wall.Id.IntegerValue + "\nFailed to place family instances hosted by a wall.\n" + ex.Message, "ElementSpliter:PlaceFamilyInstances", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void TransferInstanceData(FamilyInstance originInstance, FamilyInstance newInstance)
        {
            try
            {
                if (originInstance.CanFlipFacing)
                {
                    if (originInstance.FacingFlipped != newInstance.FacingFlipped)
                    {
                        newInstance.flipFacing();
                    }
                }
                if (originInstance.CanFlipHand)
                {
                    if (originInstance.HandFlipped != newInstance.HandFlipped)
                    {
                        newInstance.flipHand();
                    }
                }

                foreach (Parameter originParam in originInstance.Parameters)
                {
                    if (originParam.IsReadOnly) { continue; }
                    
                    string parameterName = originParam.Definition.Name;
                    if (parameterName.Contains("Extensions")) { continue; }

#if RELEASE2013||RELEASE2014
                    Parameter fiParam = newInstance.get_Parameter(parameterName);
#elif RELEASE2015 || RELEASE2016 || RELEASE2017
                    Parameter fiParam = newInstance.LookupParameter(parameterName);
#endif

                    if (null != fiParam)
                    {
                        switch (fiParam.StorageType)
                        {
                            case StorageType.ElementId:
                                ElementId elementIdValue = originParam.AsElementId();
                                if (null != elementIdValue)
                                {
                                    try { fiParam.Set(elementIdValue); }
                                    catch { }
                                }
                                break;
                            case StorageType.Double:
                                double doubleValue = originParam.AsDouble();
                                if (0 != doubleValue)
                                {
                                    try { fiParam.Set(doubleValue); }
                                    catch { }
                                }
                                break;
                            case StorageType.Integer:
                                int intValue = originParam.AsInteger();
                                if (0 != intValue)
                                {
                                    try { fiParam.Set(intValue); }
                                    catch { }
                                }
                                break;
                            case StorageType.String:
                                string strValue = originParam.AsString();
                                if (null != strValue)
                                {
                                    try { fiParam.Set(strValue); }
                                    catch { }
                                }
                                break;
                        }
                    }
                }
            }
            catch 
            {
                //MessageBox.Show("Failed to transfer family instance data from the original.\n" + ex.Message, "ElementSpliter:TransferInstanceData", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private List<Curve> GetWallLocationCurve(Solid massSolid, Curve originalCurve, out XYZ intersectingPoint)
        {
            List<Curve> locationCurves = new List<Curve>();
            intersectingPoint = null;
            try
            {
                foreach (Face face in massSolid.Faces)
                {
                    if (null != intersectingPoint) { break; }
                    IntersectionResultArray resultArray = new IntersectionResultArray();
                    if (SetComparisonResult.Overlap == face.Intersect(originalCurve, out resultArray))
                    {
                        foreach (IntersectionResult result in resultArray)
                        {
                            if (null != result.XYZPoint)
                            {
                                intersectingPoint = result.XYZPoint;
                                break;
                            }
                        }
                    }
                }

                if (null != intersectingPoint)
                {
                    IntersectionResult intersectionResult = originalCurve.Project(intersectingPoint);
                    double parameter = intersectionResult.Parameter;
                    if (originalCurve.IsInside(parameter))
                    {
                        Curve firstCurve = originalCurve.Clone();
#if RELEASE2013
                        firstCurve.MakeBound(firstCurve.get_EndParameter(0), parameter);
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                        firstCurve.MakeBound(firstCurve.GetEndParameter(0), parameter);
#endif
                        locationCurves.Add(firstCurve);

                        Curve secondCurve = originalCurve.Clone();
#if RELEASE2013
                        secondCurve.MakeBound(parameter, secondCurve.get_EndParameter(1));
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                        secondCurve.MakeBound(parameter, secondCurve.GetEndParameter(1));
#endif
                        locationCurves.Add(secondCurve);
                    }
                }
            }
            catch 
            {
                //MessageBox.Show("Failed to get boundary lines from solid.\n" + ex.Message, "ElementSpliter:GetWallLocationCurve", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return locationCurves;
        }

        //place doors and windows
        #endregion

        #region new roofs
        //1. get profiles of the original roof and create a extrusion solid
        //2. find intersection between the extruded solid and mass solid
        //3. find bottom face and edges of the bottom face of the  intersected solid
        //4. create footprintRoof
        //5. add inner vertices from the original shape editor
        //6. find intersection points between creases and a face of the mass solid to add vertices on the roof
        private  List<Element> NewPrimaryRoof(ElementProperties ep)
        {
            List<Element> primaryElements = new List<Element>();
            try
            {
                FootPrintRoof footPrintRoof = ep.CopiedElement as FootPrintRoof;
#if RELEASE2013
                ElementId elementId = footPrintRoof.Level.Id;
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                ElementId elementId = footPrintRoof.LevelId;
#endif
                Level roofLevel = primaryDoc.GetElement(elementId) as Level;
                if (null != footPrintRoof)
                {
                    List<CurveArray> profiles = GetRoofFootPrint(ep, true);

                    foreach (CurveArray profile in profiles)
                    {
                        try
                        {
                            ModelCurveArray modelCurveMapping = new ModelCurveArray();
                            FootPrintRoof newRoof = null;
                            newRoof = primaryDoc.Create.NewFootPrintRoof(profile, roofLevel, footPrintRoof.RoofType, out modelCurveMapping);
                            if (null != newRoof)
                            {
                                if (footPrintRoof.SlabShapeEditor.IsEnabled)
                                {
                                    Face intersectFace = GetIntersectFace(ep.MassContainers[ep.SelectedMassId], profile);
                                    EditSlabShape(intersectFace, footPrintRoof.SlabShapeEditor, newRoof.SlabShapeEditor);
                                }
                                primaryElements.Add(newRoof);
                            }
                        }
                        catch (Exception ex) { MessageBox.Show("[" + ep.ElementId + "]" + ep.ElementName + "\nFailed to split roof.\n" + ex.Message, "ElementSpliter:NewPrimaryRoof", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
                    }
                }
            }
            catch
            {
                //MessageBox.Show("[" + ep.ElementId + "]" + ep.ElementName + "\nFailed to split roof.\n" + ex.Message, "ElementSpliter:NewPrimaryRoof", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return primaryElements;
        }

        private  List<Element> NewSecondaryRoof(ElementProperties ep)
        {
            List<Element> secondaryElements = new List<Element>();
            try
            {
                FootPrintRoof footPrintRoof = ep.CopiedElement as FootPrintRoof;
#if RELEASE2013
                ElementId elementId = footPrintRoof.Level.Id;
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                ElementId elementId = footPrintRoof.LevelId;
#endif

                Level roofLevel = primaryDoc.GetElement(elementId) as Level;
                if (null != footPrintRoof)
                {
                    List<CurveArray> profiles = GetRoofFootPrint(ep, false);
                    foreach (CurveArray profile in profiles)
                    {
                        try
                        {
                            ModelCurveArray modelCurveMapping = new ModelCurveArray();
                            FootPrintRoof newRoof = null;
                            newRoof = primaryDoc.Create.NewFootPrintRoof(profile, roofLevel, footPrintRoof.RoofType, out modelCurveMapping);
                            if (null != newRoof)
                            {
                                if (footPrintRoof.SlabShapeEditor.IsEnabled)
                                {
                                    Face intersectFace = GetIntersectFace(ep.MassContainers[ep.SelectedMassId], profile);
                                    EditSlabShape(intersectFace, footPrintRoof.SlabShapeEditor, newRoof.SlabShapeEditor);
                                }
                                secondaryElements.Add(newRoof);
                            }
                        }
                        catch (Exception ex) { MessageBox.Show("[" + ep.ElementId + "]" + ep.ElementName + "\nFailed to split roofs.\n" + ex.Message, "ElementSpliter:NewSecondaryRoof", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
                    }
                }
            }
            catch 
            {
                //MessageBox.Show("[" + ep.ElementId + "]" + ep.ElementName + "\nFailed to split roofs.\n" + ex.Message, "ElementSpliter:NewSecondaryRoof", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return secondaryElements;
        }

        private  List<CurveArray> GetRoofFootPrint(ElementProperties ep, bool primary)
        {
            List<CurveArray> profile = new List<CurveArray>();
            try
            {
                //extrusion of roof profile>> intersect to mass solid>> get bottom face to get roof profile
                FootPrintRoof footPrintRoof = ep.CopiedElement as FootPrintRoof;
                if (null != footPrintRoof)
                {
                    ModelCurveArrArray roofProfile = footPrintRoof.GetProfiles();
                    List<CurveLoop> curveLoopList = new List<CurveLoop>();
                    foreach (ModelCurveArray curveArray in roofProfile)
                    {
                        CurveLoop curveLoop = new CurveLoop();
                        foreach (ModelCurve modelCurve in curveArray)
                        {
                            curveLoop.Append(modelCurve.GeometryCurve);
                        }
                        curveLoopList.Add(curveLoop);
                    }
                    Solid profileSolid = GeometryCreationUtilities.CreateExtrusionGeometry(curveLoopList, new XYZ(0, 0, 1), 10);
                    Solid massSolid = ep.MassContainers[ep.SelectedMassId];
                    Solid solidForProfile = null;
                    if (primary)
                    {
                        solidForProfile = BooleanOperationsUtils.ExecuteBooleanOperation(profileSolid, massSolid, BooleanOperationsType.Intersect);
                    }
                    else
                    {
                        solidForProfile = BooleanOperationsUtils.ExecuteBooleanOperation(profileSolid, massSolid, BooleanOperationsType.Difference);
                    }
                    if (null != solidForProfile)
                    {
                        foreach (Face face in solidForProfile.Faces)
                        {
                            XYZ normal = ComputeNormalOfFace(face);
                            if (normal.Z < 0) //get bottom face
                            {
                                if (Math.Abs(normal.Z) > Math.Abs(normal.X) && Math.Abs(normal.Z) > Math.Abs(normal.Y))
                                {
                                    foreach (EdgeArray edgeArray in face.EdgeLoops)
                                    {
                                        CurveArray curveArray = new CurveArray();
                                        foreach (Edge edge in edgeArray)
                                        {
                                            curveArray.Append(edge.AsCurve());
                                        }
                                        profile.Add(curveArray);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch 
            {
                //MessageBox.Show("Failed to get boundary lines from solid.\n" + ex.Message, "ElementSpliter:GetFloorBoundary", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return profile;
        }

        private  void EditSlabShape(Face intersectFace, SlabShapeEditor originEditor, SlabShapeEditor newEditor)
        {
            try
            {

                if (null != originEditor.SlabShapeVertices)
                {
                    foreach (SlabShapeVertex vertex in originEditor.SlabShapeVertices)
                    {
                        if (vertex.VertexType == SlabShapeVertexType.Interior)
                        {
                            try { newEditor.DrawPoint(vertex.Position); }
                            catch { continue; }
                        }
                    }

                    foreach (SlabShapeCrease crease in originEditor.SlabShapeCreases)
                    {
                        if (crease.CreaseType == SlabShapeCreaseType.UserDrawn)
                        {
                            IntersectionResultArray resultArray;
                            SetComparisonResult comparisonResult = intersectFace.Intersect(crease.Curve, out resultArray);
                            if (comparisonResult == SetComparisonResult.Overlap)
                            {
                                try
                                {
                                    XYZ intersectPoint = resultArray.get_Item(0).XYZPoint;
                                    newEditor.DrawPoint(intersectPoint);
                                }
                                catch { continue; }
                            }
                        }
                    }
                }
            }
            catch
            {
                //MessageBox.Show("Failed to draw point for sub elements in roof.\n" + ex.Message, "ElementSpliter:EditSlabShae", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private  Face GetIntersectFace(Solid massSolid, CurveArray profileCurve)
        {
            Face intersectFace = null;
            try
            {
                foreach (Face face in massSolid.Faces)
                {
                    if (null != intersectFace) { break; }
                    foreach (Curve curve in profileCurve)
                    {
                        if (SetComparisonResult.Subset== face.Intersect(curve))
                        {
                            intersectFace = face;
                            break;
                        }
                    }
                }
            }
            catch 
            {
                //MessageBox.Show("Failed to find intersect face.\n" + ex.Message, "ElementSpliter:GetIntersectFace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return intersectFace;
        }

        #endregion

        #region new structural framing
        private  void SplitBeam(ElementProperties ep, out List<Element> primaryElementsList, out List<Element> secondaryElementsList)
        {
            primaryElementsList = new List<Element>();
            secondaryElementsList = new List<Element>();
            try
            {
                FamilyInstance originInstance = ep.CopiedElement as FamilyInstance;
                if (null != originInstance)
                {
                    LocationCurve locationCurve = originInstance.Location as LocationCurve;
                    Curve curve = locationCurve.Curve.Clone();

                    XYZ interPoint;
                    List<Curve> newLocationCurves = GetBeamLocationCurve(ep.MassContainers[ep.SelectedMassId], curve, out interPoint);
                    if (newLocationCurves.Count == 2)
                    {
                        FamilyInstance newBeam = null;
                        Level hostLevel = originInstance.Host as Level;
                        try { newBeam = primaryDoc.Create.NewFamilyInstance(newLocationCurves[0], originInstance.Symbol, hostLevel, originInstance.StructuralType); }
                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                        if (null != newBeam) { primaryElementsList.Add(newBeam); }

                        newBeam = null;
                        try { newBeam = primaryDoc.Create.NewFamilyInstance(newLocationCurves[1], originInstance.Symbol, hostLevel, originInstance.StructuralType); }
                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                        if (null != newBeam) { secondaryElementsList.Add(newBeam); }
                    }
                }
            }
            catch 
            {
                //MessageBox.Show("Failed to split Beam.\n" + ex.Message, "ElementSpliter:SplitBeam", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private  List<Curve> GetBeamLocationCurve(Solid massSolid, Curve originalCurve, out XYZ intersectingPoint)
        {
            List<Curve> locationCurves = new List<Curve>();
            intersectingPoint = null;
            try
            {
                foreach (Face face in massSolid.Faces)
                {
                    if (null != intersectingPoint) { break; }
                    IntersectionResultArray resultArray = new IntersectionResultArray();
                    if (SetComparisonResult.Overlap == face.Intersect(originalCurve, out resultArray))
                    {
                        foreach (IntersectionResult result in resultArray)
                        {
                            if (null != result.XYZPoint)
                            {
                                intersectingPoint = result.XYZPoint;
                                break;
                            }
                        }
                    }
                }

                if (null != intersectingPoint)
                {
                    IntersectionResult intersectionResult = originalCurve.Project(intersectingPoint);
                    double parameter = intersectionResult.Parameter;
                    if (originalCurve.IsInside(parameter))
                    {
                        Curve firstCurve = originalCurve.Clone();
#if RELEASE2013
                        firstCurve.MakeBound(firstCurve.get_EndParameter(0), parameter);
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                        firstCurve.MakeBound(firstCurve.GetEndParameter(0), parameter);
#endif
                        locationCurves.Add(firstCurve);

                        Curve secondCurve = originalCurve.Clone();
#if RELEASE2013
                        secondCurve.MakeBound(parameter, secondCurve.get_EndParameter(1));
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                        secondCurve.MakeBound(parameter, secondCurve.GetEndParameter(1));
#endif
                        locationCurves.Add(secondCurve);
                    }
                }
            }
            catch 
            {
                //MessageBox.Show("Failed to get boundary lines from solid.\n" + ex.Message, "ElementSpliter:GetFloorBoundary", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return locationCurves;
        }
        #endregion

        #region new structural columns
        private void SplitColumns(ElementProperties ep, out List<Element> primaryElementsList, out List<Element> secondaryElementsList)
        {
            primaryElementsList = new List<Element>();
            secondaryElementsList = new List<Element>();
            try
            {
                FamilyInstance originInstance = ep.CopiedElement as FamilyInstance;
#if RELEASE2013
                ElementId elementId = originInstance.Level.Id;
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                ElementId elementId = originInstance.LevelId;
#endif

                Level instanceLevel = primaryDoc.GetElement(elementId) as Level;
                if (null != originInstance)
                {
                    Curve curve = null;
                    if (originInstance.IsSlantedColumn)
                    {
                        LocationCurve locationCurve = originInstance.Location as LocationCurve;
                        curve = locationCurve.Curve.Clone();
                    }
                    else if (originInstance.Location is LocationPoint)
                    {
                        BoundingBoxXYZ boundingBox = originInstance.get_BoundingBox(primaryDoc.ActiveView);
                        LocationPoint locationPoint = originInstance.Location as LocationPoint;
                        XYZ pointXYZ = locationPoint.Point;
                        XYZ startPoint = new XYZ(pointXYZ.X, pointXYZ.Y, boundingBox.Min.Z);
                        XYZ endPoint = new XYZ(pointXYZ.X, pointXYZ.Y, boundingBox.Max.Z);

#if RELEASE2013
                        curve = primaryDoc.Application.Create.NewLineBound(startPoint, endPoint);
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                        curve = Autodesk.Revit.DB.Line.CreateBound(startPoint, endPoint);
#endif

                    }
                    if (null != curve)
                    {
                        XYZ interPoint;
                        List<Curve> newCurves = GetColumnCurve(ep.MassContainers[ep.SelectedMassId], curve, out interPoint);
                        if (newCurves.Count == 2)
                        {
                            FamilyInstance newColumn = null;

                            try { newColumn = primaryDoc.Create.NewFamilyInstance(newCurves[0], originInstance.Symbol, instanceLevel, originInstance.StructuralType); }
                            catch (Exception ex) { MessageBox.Show(ex.Message); }
                            if (null != newColumn) { primaryElementsList.Add(newColumn); }

                            newColumn = null;
                            try { newColumn = primaryDoc.Create.NewFamilyInstance(newCurves[1], originInstance.Symbol, instanceLevel, originInstance.StructuralType); }
                            catch (Exception ex) { MessageBox.Show(ex.Message); }
                            if (null != newColumn) { secondaryElementsList.Add(newColumn); }
                        }
                    }
                }
            }
            catch 
            {
                //MessageBox.Show("Failed to split Column.\n" + ex.Message, "ElementSpliter:SplitColumns", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private List<Curve> GetColumnCurve(Solid massSolid, Curve originalCurve, out XYZ intersectingPoint)
        {
            List<Curve> locationCurves = new List<Curve>();
            intersectingPoint = null;
            try
            {
                foreach (Face face in massSolid.Faces)
                {
                    if (null != intersectingPoint) { break; }
                    IntersectionResultArray resultArray = new IntersectionResultArray();
                    if (SetComparisonResult.Overlap == face.Intersect(originalCurve, out resultArray))
                    {
                        foreach (IntersectionResult result in resultArray)
                        {
                            if (null != result.XYZPoint)
                            {
                                intersectingPoint = result.XYZPoint;
                                break;
                            }
                        }
                    }
                }

                if (null != intersectingPoint)
                {
                    IntersectionResult intersectionResult = originalCurve.Project(intersectingPoint);
                    double parameter = intersectionResult.Parameter;
                    if (originalCurve.IsInside(parameter))
                    {
                        Curve firstCurve = originalCurve.Clone();
#if RELEASE2013
                        firstCurve.MakeBound(firstCurve.get_EndParameter(0), parameter);
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                        firstCurve.MakeBound(firstCurve.GetEndParameter(0), parameter);
#endif
                        locationCurves.Add(firstCurve);

                        Curve secondCurve = originalCurve.Clone();
#if RELEASE2013
                        secondCurve.MakeBound(parameter, secondCurve.get_EndParameter(1));         
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                        secondCurve.MakeBound(parameter, secondCurve.GetEndParameter(1));
#endif
                        locationCurves.Add(secondCurve);
                    }
                }
            }
            catch 
            {
                //MessageBox.Show("Failed to get curve lines from solid.\n" + ex.Message, "ElementSpliter:GetColumnCurve", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return locationCurves;
        }
        #endregion

        #region new pipes
        private void SplitPipes(ElementProperties ep, out List<Element> primaryElementsList, out List<Element> secondaryElementsList)
        {
            primaryElementsList = new List<Element>();
            secondaryElementsList = new List<Element>();
            try
            {
                Pipe originPipe = ep.CopiedElement as Pipe;

                if (null != originPipe)
                {
                    LocationCurve locationCurve = originPipe.Location as LocationCurve;
                    Curve curve = locationCurve.Curve.Clone();

                    XYZ intersectingPoint = GetIntersectingPoint(ep.MassContainers[ep.SelectedMassId], curve);
                    if (null != intersectingPoint)
                    {
                        Pipe newPipe = null;
#if RELEASE2013
                        try { newPipe = primaryDoc.Create.NewPipe(curve.get_EndPoint(0), intersectingPoint, originPipe.PipeType); }
#elif RELEASE2014
                        try { newPipe = primaryDoc.Create.NewPipe(curve.GetEndPoint(0), intersectingPoint, originPipe.PipeType); }
#elif RELEASE2015|| RELEASE2016 || RELEASE2017
                        try { newPipe = Pipe.Create(primaryDoc, originPipe.MEPSystem.GetTypeId(), originPipe.GetTypeId(), originPipe.ReferenceLevel.Id, curve.GetEndPoint(0), intersectingPoint); }
#endif
                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                        if (null != newPipe) { primaryElementsList.Add(newPipe); }

                        newPipe = null;
#if RELEASE2013
                        try { newPipe = primaryDoc.Create.NewPipe(intersectingPoint, curve.get_EndPoint(1), originPipe.PipeType); }
#elif RELEASE2014
                        try { newPipe = primaryDoc.Create.NewPipe(intersectingPoint, curve.GetEndPoint(1), originPipe.PipeType); }
#elif RELEASE2015 || RELEASE2016 || RELEASE2017
                        try { newPipe = Pipe.Create(primaryDoc, originPipe.MEPSystem.GetTypeId(), originPipe.GetTypeId(), originPipe.ReferenceLevel.Id, intersectingPoint, curve.GetEndPoint(1)); }
#endif
                        
                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                        if (null != newPipe) { secondaryElementsList.Add(newPipe); }
                        //ICollection<ElementId> pipeIds= PlumbingUtils.ConvertPipePlaceholders(doc, placeHolderIds);
                    }
                }
            }
            catch 
            {
                //MessageBox.Show("Failed to split pipes.\n" + ex.Message, "ElementSpliter:SplitPipes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private XYZ GetIntersectingPoint(Solid massSolid, Curve originalCurve)
        {
            XYZ intersectingPoint = null;
            try
            {
                foreach (Face face in massSolid.Faces)
                {
                    if (null != intersectingPoint) { break; }
                    IntersectionResultArray resultArray = new IntersectionResultArray();
                    if (SetComparisonResult.Overlap == face.Intersect(originalCurve, out resultArray))
                    {
                        foreach (IntersectionResult result in resultArray)
                        {
                            if (null != result.XYZPoint)
                            {
                                intersectingPoint = result.XYZPoint;
                                break;
                            }
                        }
                    }
                }
            }
            catch 
            {
                //MessageBox.Show("Failed to get intersectinb points from solid.\n" + ex.Message, "ElementSpliter:GetIntersectingPoint", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return intersectingPoint;
        }
        #endregion

        #region new ducts
        private void SplitDucts(ElementProperties ep, out List<Element> primaryElementsList, out List<Element> secondaryElementsList)
        {
            primaryElementsList = new List<Element>();
            secondaryElementsList = new List<Element>();
            try
            {
                Duct originDuct = ep.CopiedElement as Duct;

                if (null != originDuct)
                {
                    LocationCurve locationCurve = originDuct.Location as LocationCurve;
                    Curve curve = locationCurve.Curve.Clone();

                    XYZ intersectingPoint = GetIntersectingPoint(ep.MassContainers[ep.SelectedMassId], curve);
                    if (null != intersectingPoint)
                    {
                        Duct newDuct = null;
#if RELEASE2013
                        try { newDuct = primaryDoc.Create.NewDuct(curve.get_EndPoint(0), intersectingPoint, originDuct.DuctType); }
#elif RELEASE2014||RELEASE2015 || RELEASE2016 
                        try { newDuct = primaryDoc.Create.NewDuct(curve.GetEndPoint(0), intersectingPoint, originDuct.DuctType); }
#elif RELEASE2017
                        try { newDuct = Duct.Create(primaryDoc, originDuct.MEPSystem.GetTypeId(), originDuct.DuctType.Id, originDuct.LevelId, curve.GetEndPoint(0), intersectingPoint); }
#endif

                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                        if (null != newDuct) { primaryElementsList.Add(newDuct); }

                        newDuct = null;
#if RELEASE2013
                        try { newDuct = primaryDoc.Create.NewDuct(intersectingPoint, curve.get_EndPoint(1), originDuct.DuctType); }
#elif RELEASE2014||RELEASE2015 || RELEASE2016
                        try { newDuct = primaryDoc.Create.NewDuct(intersectingPoint, curve.GetEndPoint(1), originDuct.DuctType); }
#elif RELEASE2017
                        try { newDuct = Duct.Create(primaryDoc, originDuct.MEPSystem.GetTypeId(), originDuct.DuctType.Id, originDuct.LevelId, intersectingPoint, curve.GetEndPoint(1)); }
#endif

                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                        if (null != newDuct) { secondaryElementsList.Add(newDuct); }

                        //ICollection<ElementId> pipeIds= PlumbingUtils.ConvertPipePlaceholders(doc, placeHolderIds);
                    }
                }
            }
            catch 
            {
                //MessageBox.Show("Failed to split ducts.\n" + ex.Message, "ElementSpliter:SplitDucts", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region new conduit
        private void SplitConduit(ElementProperties ep, out List<Element> primaryElementsList, out List<Element> secondaryElementsList)
        {
            primaryElementsList = new List<Element>();
            secondaryElementsList = new List<Element>();
            try
            {
                Conduit originConduit = ep.CopiedElement as Conduit;

                if (null != originConduit)
                {
                    LocationCurve locationCurve = originConduit.Location as LocationCurve;
                    Curve curve = locationCurve.Curve.Clone();

                    XYZ intersectingPoint = GetIntersectingPoint(ep.MassContainers[ep.SelectedMassId], curve);
                    if (null != intersectingPoint)
                    {
                        Conduit newConduit = null;
#if RELEASE2013
                        try { newConduit = Conduit.Create(primaryDoc, originConduit.GetTypeId(), curve.get_EndPoint(0), intersectingPoint, originConduit.ReferenceLevel.Id); }
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                        try { newConduit = Conduit.Create(primaryDoc, originConduit.GetTypeId(), curve.GetEndPoint(0), intersectingPoint, originConduit.ReferenceLevel.Id); }
#endif

                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                        if (null != newConduit) { primaryElementsList.Add(newConduit); }

                        newConduit = null;
#if RELEASE2013
                        try { newConduit = Conduit.Create(primaryDoc, originConduit.GetTypeId(), intersectingPoint, curve.get_EndPoint(1), originConduit.ReferenceLevel.Id); }
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                        try { newConduit = Conduit.Create(primaryDoc, originConduit.GetTypeId(), intersectingPoint, curve.GetEndPoint(1), originConduit.ReferenceLevel.Id); }
#endif

                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                        if (null != newConduit) { secondaryElementsList.Add(newConduit); }
                        //ICollection<ElementId> pipeIds= PlumbingUtils.ConvertPipePlaceholders(doc, placeHolderIds);
                    }
                }
            }
            catch 
            {
                //MessageBox.Show("Failed to split conduits.\n" + ex.Message, "ElementSpliter:SplitConduit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        private XYZ ComputeNormalOfFace(Face face)
        {
            XYZ normal;
            BoundingBoxUV bbuv = face.GetBoundingBox();
            UV faceCenter = (bbuv.Min + bbuv.Max) / 2;
            normal = face.ComputeNormal(faceCenter);
            return normal;
        }

        private ElementProperties TransferParameterValues(ElementProperties ep)
        {
            try
            {
                List<ElementId> primaryIds = new List<ElementId>();
                List<ElementId> secondaryIds = new List<ElementId>();
                foreach (Element element in ep.PrimaryElements)
                {
                    primaryIds.Add(element.Id);
                }
                foreach (Element element in ep.SecondaryElements)
                {
                    secondaryIds.Add(element.Id);
                }

                using (Transaction trans = new Transaction(primaryDoc))
                {
                    trans.Start("Transfer parameters");
                    FailureHandlingOptions failureHandlingOptions = trans.GetFailureHandlingOptions();
                    FailureHandler failureHandler = new FailureHandler();
                    failureHandlingOptions.SetFailuresPreprocessor(failureHandler);
                    failureHandlingOptions.SetClearAfterRollback(true);
                    trans.SetFailureHandlingOptions(failureHandlingOptions);

                    Element originalElement = ep.CopiedElement;
                    foreach (Parameter param in originalElement.Parameters)
                    {
                        string paramName = param.Definition.Name;
                        if (paramName.Contains("Extensions")) { continue; }
                        //if (param.IsReadOnly) { continue; }
                        if (!param.HasValue) { continue; }
                        InternalDefinition definition = param.Definition as InternalDefinition;
                        if (null != definition)
                        {
                            if (parametersToSkip.Contains(definition.BuiltInParameter)) { continue; }
                            if (definition.BuiltInParameter == BuiltInParameter.ALL_MODEL_MARK)
                            {
                                string markVal = param.AsString();
                                if (markVal.Length > 0)
                                {
                                    try
                                    {
                                        ep.PrimaryElements[0].get_Parameter(BuiltInParameter.ALL_MODEL_MARK).Set(markVal + "_A");
                                        ep.SecondaryElements[0].get_Parameter(BuiltInParameter.ALL_MODEL_MARK).Set(markVal + "_B");
                                        continue;
                                    }
                                    catch { continue; }
                                }
                            }
                        }

                        foreach (Element primaryElement in ep.PrimaryElements)
                        {
#if RELEASE2013||RELEASE2014
                            Parameter parameter = primaryElement.get_Parameter(paramName);
#elif RELEASE2015 || RELEASE2016 || RELEASE2017
                            Parameter parameter = primaryElement.LookupParameter(paramName);
#endif
                            if (null != parameter)
                            {
                                if (!parameter.IsReadOnly)
                                {
                                    switch (parameter.StorageType)
                                    {
                                        case StorageType.ElementId:
                                            //parameter.Set(param.AsElementId());
                                            break;
                                        case StorageType.String:
                                            parameter.Set(param.AsString());
                                            break;
                                        case StorageType.Double:

                                            parameter.Set(param.AsDouble());
                                            break;
                                        case StorageType.Integer:
                                            parameter.Set(param.AsInteger());
                                            break;
                                    }
                                }
                            }
                        }
                        foreach (Element secondaryElement in ep.SecondaryElements)
                        {
#if RELEASE2013||RELEASE2014
                            Parameter parameter = secondaryElement.get_Parameter(paramName);
#elif RELEASE2015 || RELEASE2016 || RELEASE2017
                            Parameter parameter = secondaryElement.LookupParameter(paramName);
#endif
                            if (null != parameter)
                            {
                                if (!parameter.IsReadOnly)
                                {
                                    switch (parameter.StorageType)
                                    {
                                        case StorageType.ElementId:
                                            //parameter.Set(param.AsElementId());
                                            break;
                                        case StorageType.String:
                                            parameter.Set(param.AsString());
                                            break;
                                        case StorageType.Double:
                                            parameter.Set(param.AsDouble());
                                            break;
                                        case StorageType.Integer:
                                            parameter.Set(param.AsInteger());
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    trans.Commit();
                }

                List<Element> primaryElements = new List<Element>();
                List<Element> secondaryElements = new List<Element>();

                foreach (ElementId elementId in primaryIds)
                {
                    Element element = primaryDoc.GetElement(elementId);
                    if (null != element)
                    {
                        primaryElements.Add(element);
                    }
                }
                ep.PrimaryElements = primaryElements;
                
                foreach (ElementId elementId in secondaryIds)
                {
                    Element element = primaryDoc.GetElement(elementId);
                    if (null != element)
                    {
                        secondaryElements.Add(element);
                    }
                }
                ep.SecondaryElements = secondaryElements;
             
                return ep;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("["+ep.ElementId+"] "+ep.ElementName+"\nFailed to transfer parameters values.\n" + ex.Message, "ElementSpliter:TransferParameterValues", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LogFileManager.AppendLog("TransferParameterValue", "["+ep.ElementId+"] "+ep.ElementName+"\t"+ex.Message );
                return ep;
            }
        }

        public void DeleteOriginalElement(ElementProperties ep)
        {
            try
            {
                using (Transaction trans = new Transaction(primaryDoc))
                {
                    trans.Start("Delete original element");
                    try { primaryDoc.Delete(ep.CopiedElementId); trans.Commit(); }
                    catch { }
                }
            }
            catch
            {
                //MessageBox.Show("[" + ep.ElementId + "] " + ep.ElementName + "\nFailed to delete original elements.\n" + ex.Message, "ElementSpliter:DeleteOriginalElement", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

#if RELEASE2014 || RELEASE2015 || RELEASE2016 || RELEASE2017
    public class HideAndAcceptDuplicateTypeNamesHandler : IDuplicateTypeNamesHandler
    {
        #region IDuplicateTypeNamesHandler Members

        /// <summary>
        /// Implementation of the IDuplicateTypeNameHandler
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public DuplicateTypeAction OnDuplicateTypeNamesFound(DuplicateTypeNamesHandlerArgs args)
        {
            // Always use duplicate destination types when asked
            return DuplicateTypeAction.UseDestinationTypes;
        }

        #endregion
    }
#endif
    
}
