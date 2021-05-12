using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Electrical;
using HOK.Core.Utilities;

namespace HOK.RoomsToMass.ParameterAssigner
{
    public class ElementSpliter
    {
        private readonly BuiltInParameter[] parametersToSkip = {
            BuiltInParameter.WALL_USER_HEIGHT_PARAM/*wall*/,
            BuiltInParameter.WALL_BASE_OFFSET/*wall*/,
            BuiltInParameter.WALL_TOP_OFFSET/*wall*/,
            BuiltInParameter.SCHEDULE_BASE_LEVEL_OFFSET_PARAM/*columns*/,
            BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM/*columns*/,
            BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM/*columns*/,
            BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM/*columns*/,
            BuiltInParameter.RBS_OFFSET_PARAM/*pipes*/
        };

        private readonly Document primaryDoc;

        public ElementSpliter(Document document)
        {
            primaryDoc = document;
        }

        public ElementProperties SplitElement(ElementProperties ep)
        {
            var newEP = ep;
            var primaryElements = new List<Element>();
            var secondaryElements = new List<Element>();
            try
            {
                if (ep.LinkedElement)
                {
                    using (var trans = new Transaction(primaryDoc))
                    {
                        trans.Start("Copy Element");
                        var failureHandlingOptions = trans.GetFailureHandlingOptions();
                        var failureHandler = new FailureHandler();
                        failureHandlingOptions.SetFailuresPreprocessor(failureHandler);
                        failureHandlingOptions.SetClearAfterRollback(true);
                        trans.SetFailureHandlingOptions(failureHandlingOptions);
                        try
                        {
                            var toCopy = new List<ElementId> { ep.ElementObj.Id };
                            if (ep.ElementObj is HostObject)
                            {
                                var host = ep.ElementObj as HostObject;
                                var eIds = host.FindInserts(false, false, false, false);
                                toCopy.AddRange(eIds);
                            }

                            var options = new CopyPasteOptions();
                            options.SetDuplicateTypeNamesHandler(new HideAndAcceptDuplicateTypeNamesHandler());
                            var copiedElements = ElementTransformUtils.CopyElements(ep.Doc, toCopy, primaryDoc, ep.TransformValue, options);
                            if (copiedElements.Count > 0)
                            {
                                var copiedElement = primaryDoc.GetElement(copiedElements.First());
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

                if (null == ep.CopiedElement) { ep.SplitSucceed = false; return ep; }

                using (var trans = new Transaction(primaryDoc))
                {
                    var failureHandlingOptions = trans.GetFailureHandlingOptions();
                    var failureHandler = new FailureHandler();
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
                            var profileSolid = GetNativeWallSolid(ep, trans);
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
                            secondaryElements = NewSecondaryRoof(ep);
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
                            Log.AppendLog(LogMessageType.WARNING, string.Join(",", failureHandler.FailureMessageInfoList));
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
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return newEP;
        }

        #region new floors
        private List<Element> NewPrimaryFloor(ElementProperties ep)
        {
            var primaryElements = new List<Element>();
            try
            {
                var massSolid = ep.MassContainers[ep.SelectedMassId];
                var intersectSolid = BooleanOperationsUtils.ExecuteBooleanOperation(massSolid, ep.ElementSolid, BooleanOperationsType.Intersect);
                var profiles = GetFloorBoundary(intersectSolid);

                var floor = ep.CopiedElement as Floor;
                var floortype = floor.FloorType;
                var levelId = floor.LevelId;
                var level = primaryDoc.GetElement(levelId) as Level;
                var structural = floortype.IsFoundationSlab;
                foreach (var profile in profiles)
                {
#if RELEASE2022
                    var loopProfile = new List<CurveLoop>(1);
                    List<Curve> curves = new List<Curve>();
                    foreach (Curve curve in profile)
                    {
                        curves.Append(curve);
                    }
                    CurveLoop curveLoop = CurveLoop.Create(curves);
                    loopProfile.Add(curveLoop);
                    var primaryFloor = Floor.Create(primaryDoc, loopProfile, floortype, level);
#else
                    var primaryFloor = primaryDoc.Create.NewFloor(profile, floortype, level, structural);
#endif
                    if (null != primaryFloor)
                    {
                        primaryElements.Add(primaryFloor);
                    }
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return primaryElements;
        }

        private List<Element> NewSecondaryFloor(ElementProperties ep)
        {
            var secondaryElements = new List<Element>();
            try
            {
                var massSolid = ep.MassContainers[ep.SelectedMassId];
                var differenceSolid = BooleanOperationsUtils.ExecuteBooleanOperation(ep.ElementSolid, massSolid, BooleanOperationsType.Difference);
                var profiles = GetFloorBoundary(differenceSolid);
                var floor = ep.CopiedElement as Floor;
                var floortype = floor.FloorType;
                var floortypeId = floor.FloorType.Id;
                var levelId = floor.LevelId;
                var level = primaryDoc.GetElement(levelId) as Level;
                var structural = floortype.IsFoundationSlab;
                foreach (var profile in profiles)
                {
#if RELEASE2022
                    var loopProfile = new List<CurveLoop>(1);
                    List<Curve> curves = new List<Curve>();
                    foreach (Curve curve in profile)
                    {
                        curves.Append(curve);
                    }
                    CurveLoop curveLoop = CurveLoop.Create(curves);
                    loopProfile.Add(curveLoop);
                    var secondaryFloor = Floor.Create(primaryDoc, loopProfile, floortypeId, levelId, structural, null, 0);
#else
                    var secondaryFloor = primaryDoc.Create.NewFloor(profile, floortype, level, structural);
#endif
                    if (null != secondaryFloor)
                    {
                        secondaryElements.Add(secondaryFloor);
                    }
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return secondaryElements;
        }

        private List<CurveArray> GetFloorBoundary(Solid solid)
        {
            var profileList = new List<CurveArray>();
            try
            {
                foreach (Face face in solid.Faces)
                {
                    if (face is PlanarFace)
                    {
                        var normal = ComputeNormalOfFace(face);
                        if (normal.Z > 0)
                        {
                            if (Math.Abs(normal.Z) > Math.Abs(normal.X) && Math.Abs(normal.Z) > Math.Abs(normal.Y))
                            {
                                foreach (EdgeArray edgeArray in face.EdgeLoops)
                                {
                                    var curveArray = new CurveArray();
                                    foreach (Edge edge in edgeArray)
                                    {
                                        var curve = edge.AsCurveFollowingFace(face);
                                        curveArray.Append(curve);
                                    }
                                    profileList.Add(curveArray);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
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
                var wall = ep.CopiedElement as Wall;
                var elementIds = primaryDoc.Delete(ep.CopiedElementId);
                trans.RollBack();

                Sketch sketch = null;
                foreach (var eId in elementIds)
                {
                    var element = primaryDoc.GetElement(eId);
                    if (element is Sketch)
                    {
                        sketch = element as Sketch;
                    }
                }

                if (null != sketch)
                {
                    var profile = sketch.Profile;
                    var curveLoops = new List<CurveLoop>();
                    foreach (CurveArray curveArray in profile)
                    {
                        var curveLoop = new CurveLoop();
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
            catch (Exception)
            {
                // ignored
            }
            return wallSolid;
        }

        private List<Element> NewPrimaryWall(ElementProperties ep, Solid profileSolid)
        {
            var primaryElements = new List<Element>();
            try
            {
                ep.ElementSolid = profileSolid;
                var massSolid = ep.MassContainers[ep.SelectedMassId];
                var intersectSolid = BooleanOperationsUtils.ExecuteBooleanOperation(massSolid, ep.ElementSolid, BooleanOperationsType.Intersect);
                var profile = GetWallProfile(ep, intersectSolid);
                if (profile.Count > 0)
                {
                    var wall = ep.CopiedElement as Wall;
                    var wallTypeId = wall.WallType.Id;
                    var levelId = wall.LevelId;

                    var isStructural = true;
                    if (wall.StructuralUsage == StructuralWallUsage.NonBearing)
                    {
                        isStructural = false;
                    }
                    Wall primaryWall = null;
                    try { primaryWall = Wall.Create(primaryDoc, profile, wallTypeId, levelId, isStructural); primaryDoc.Regenerate(); }
                    catch { return primaryElements; }

                    if (null != primaryWall)
                    {
                        var angle = wall.Orientation.AngleTo(primaryWall.Orientation);
                        if (Math.Round(angle, 2) > 0) { primaryWall.Flip(); }
                        primaryElements.Add(primaryWall);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return primaryElements;
        }

        private List<Element> NewSecondaryWall(ElementProperties ep, Solid profileSolid)
        {
            var secondaryElements = new List<Element>();
            try
            {
                ep.ElementSolid = profileSolid;
                var massSolid = ep.MassContainers[ep.SelectedMassId];
                var differenceSolid = BooleanOperationsUtils.ExecuteBooleanOperation(ep.ElementSolid, massSolid, BooleanOperationsType.Difference);
                var profile = GetWallProfile(ep, differenceSolid);
                if (profile.Count > 0)
                {
                    var wall = ep.CopiedElement as Wall;
                    var wallTypeId = wall.WallType.Id;
                    var levelId = wall.LevelId;

                    var isStructural = true;
                    if (wall.StructuralUsage == StructuralWallUsage.NonBearing)
                    {
                        isStructural = false;
                    }
                    Wall secondaryWall = null;
                    try { secondaryWall = Wall.Create(primaryDoc, profile, wallTypeId, levelId, isStructural); primaryDoc.Regenerate(); }
                    catch { return secondaryElements; }

                    if (null != secondaryWall)
                    {
                        var angle = wall.Orientation.AngleTo(secondaryWall.Orientation);
                        if (Math.Round(angle, 2) > 0) { secondaryWall.Flip(); }

                        secondaryElements.Add(secondaryWall);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return secondaryElements;
        }

        private void SplitWalls(ElementProperties ep, out List<Element> primaryElementsList, out List<Element> secondaryElementsList)
        {
            primaryElementsList = new List<Element>();
            secondaryElementsList = new List<Element>();
            try
            {
                var wall = ep.CopiedElement as Wall;
                var height = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
                var offset = wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble();
                var wallStructural = true;
                if (wall.StructuralUsage == StructuralWallUsage.NonBearing) { wallStructural = false; }

                var locationCurve = wall.Location as LocationCurve;
                var wallCurve = locationCurve.Curve;

                XYZ interPoint;
                var newLocationCurves = GetWallLocationCurve(ep.MassContainers[ep.SelectedMassId], wallCurve, out interPoint);
                if (newLocationCurves.Count == 2)
                {
                    Wall newWall = null;
                    var wallLevelId = wall.LevelId;

                    try { newWall = Wall.Create(primaryDoc, newLocationCurves[0], wall.WallType.Id, wallLevelId, height, offset, wall.Flipped, wallStructural); }
                    catch { }
                    if (null != newWall) { primaryElementsList.Add(newWall); }

                    newWall = null;
                    try { newWall = Wall.Create(primaryDoc, newLocationCurves[1], wall.WallType.Id, wallLevelId, height, offset, wall.Flipped, wallStructural); }
                    catch { }
                    if (null != newWall) { secondaryElementsList.Add(newWall); }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private List<Curve> GetWallProfile(ElementProperties ep, Solid solid)
        {
            var profile = new List<Curve>();
            try
            {
                var wall = ep.CopiedElement as Wall;
                var orientation = wall.Orientation;
                var offsetVector = orientation.Normalize().Negate().Multiply(0.5 * wall.Width);
                var transform = Transform.CreateTranslation(offsetVector);

                foreach (Face face in solid.Faces)
                {
                    var normal = ComputeNormalOfFace(face);
                    if (normal.AngleTo(orientation) == 0) //paralle to the orientation with same direction
                    {
                        foreach (EdgeArray edgeArray in face.EdgeLoops)
                        {
                            foreach (Edge edge in edgeArray)
                            {
                                var curve = edge.AsCurve();
                                curve = curve.CreateTransformed(transform);
                                profile.Add(curve);
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return profile;
        }

        private void PlaceFamilyInstances(ElementProperties ep, List<Element> primaryElements, List<Element> secondaryElements)
        {
            var wall = ep.CopiedElement as Wall;
            try
            {
                var attachedElementIds = wall.FindInserts(false, false, false, false);
                var familyInstances = new List<FamilyInstance>();
                foreach (var elementId in attachedElementIds)
                {
                    var element = primaryDoc.GetElement(elementId);
                    if (element is FamilyInstance)
                    {
                        familyInstances.Add(element as FamilyInstance);
                    }
                }
                var outlines = new Dictionary<Element, Outline>();
                foreach (var element in primaryElements)
                {
                    var box = element.get_BoundingBox(null);
                    if (null != box)
                    {
                        var outline = new Outline(box.Min, box.Max);
                        outlines.Add(element, outline);
                    }
                }
                foreach (var element in secondaryElements)
                {
                    var box = element.get_BoundingBox(null);
                    if (null != box)
                    {
                        var outline = new Outline(box.Min, box.Max);
                        outlines.Add(element, outline);
                    }
                }

                foreach (var familyInstance in familyInstances)
                {
                    var location = familyInstance.Location as LocationPoint;
                    var elementId = familyInstance.LevelId;
                    var instanceLevel = primaryDoc.GetElement(elementId) as Level;
                    var point = location.Point;
                    foreach (var element in outlines.Keys)
                    {
                        var outline = outlines[element];
                        if (outline.Contains(point, 0))
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
            catch (Exception)
            {
                // ignored
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

                    var parameterName = originParam.Definition.Name;
                    if (parameterName.Contains("Extensions")) { continue; }

                    var fiParam = newInstance.LookupParameter(parameterName);

                    if (null != fiParam)
                    {
                        switch (fiParam.StorageType)
                        {
                            case StorageType.ElementId:
                                var elementIdValue = originParam.AsElementId();
                                if (null != elementIdValue)
                                {
                                    try { fiParam.Set(elementIdValue); }
                                    catch { }
                                }
                                break;
                            case StorageType.Double:
                                var doubleValue = originParam.AsDouble();
                                if (0 != doubleValue)
                                {
                                    try { fiParam.Set(doubleValue); }
                                    catch { }
                                }
                                break;
                            case StorageType.Integer:
                                var intValue = originParam.AsInteger();
                                if (0 != intValue)
                                {
                                    try { fiParam.Set(intValue); }
                                    catch { }
                                }
                                break;
                            case StorageType.String:
                                var strValue = originParam.AsString();
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
            catch (Exception)
            {
                // ignored
            }
        }

        private List<Curve> GetWallLocationCurve(Solid massSolid, Curve originalCurve, out XYZ intersectingPoint)
        {
            var locationCurves = new List<Curve>();
            intersectingPoint = null;
            try
            {
                foreach (Face face in massSolid.Faces)
                {
                    if (null != intersectingPoint) { break; }
                    var resultArray = new IntersectionResultArray();
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
                    var intersectionResult = originalCurve.Project(intersectingPoint);
                    var parameter = intersectionResult.Parameter;
                    if (originalCurve.IsInside(parameter))
                    {
                        var firstCurve = originalCurve.Clone();
                        firstCurve.MakeBound(firstCurve.GetEndParameter(0), parameter);
                        locationCurves.Add(firstCurve);

                        var secondCurve = originalCurve.Clone();
                        secondCurve.MakeBound(parameter, secondCurve.GetEndParameter(1));
                        locationCurves.Add(secondCurve);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
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
        private List<Element> NewPrimaryRoof(ElementProperties ep)
        {
            var primaryElements = new List<Element>();
            try
            {
                var footPrintRoof = ep.CopiedElement as FootPrintRoof;
                var elementId = footPrintRoof.LevelId;
                var roofLevel = primaryDoc.GetElement(elementId) as Level;
                if (null != footPrintRoof)
                {
                    var profiles = GetRoofFootPrint(ep, true);

                    foreach (var profile in profiles)
                    {
                        try
                        {
                            var modelCurveMapping = new ModelCurveArray();
                            FootPrintRoof newRoof = null;
                            newRoof = primaryDoc.Create.NewFootPrintRoof(profile, roofLevel, footPrintRoof.RoofType, out modelCurveMapping);
                            if (null != newRoof)
                            {
                                if (footPrintRoof.SlabShapeEditor.IsEnabled)
                                {
                                    var intersectFace = GetIntersectFace(ep.MassContainers[ep.SelectedMassId], profile);
                                    EditSlabShape(intersectFace, footPrintRoof.SlabShapeEditor, newRoof.SlabShapeEditor);
                                }
                                primaryElements.Add(newRoof);
                            }
                        }
                        catch (Exception ex) { MessageBox.Show("[" + ep.ElementId + "]" + ep.ElementName + "\nFailed to split roof.\n" + ex.Message, "ElementSpliter:NewPrimaryRoof", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return primaryElements;
        }

        private List<Element> NewSecondaryRoof(ElementProperties ep)
        {
            var secondaryElements = new List<Element>();
            try
            {
                var footPrintRoof = ep.CopiedElement as FootPrintRoof;
                var elementId = footPrintRoof.LevelId;

                var roofLevel = primaryDoc.GetElement(elementId) as Level;
                if (null != footPrintRoof)
                {
                    var profiles = GetRoofFootPrint(ep, false);
                    foreach (var profile in profiles)
                    {
                        try
                        {
                            var modelCurveMapping = new ModelCurveArray();
                            FootPrintRoof newRoof = null;
                            newRoof = primaryDoc.Create.NewFootPrintRoof(profile, roofLevel, footPrintRoof.RoofType, out modelCurveMapping);
                            if (null != newRoof)
                            {
                                if (footPrintRoof.SlabShapeEditor.IsEnabled)
                                {
                                    var intersectFace = GetIntersectFace(ep.MassContainers[ep.SelectedMassId], profile);
                                    EditSlabShape(intersectFace, footPrintRoof.SlabShapeEditor, newRoof.SlabShapeEditor);
                                }
                                secondaryElements.Add(newRoof);
                            }
                        }
                        catch (Exception ex) { MessageBox.Show("[" + ep.ElementId + "]" + ep.ElementName + "\nFailed to split roofs.\n" + ex.Message, "ElementSpliter:NewSecondaryRoof", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return secondaryElements;
        }

        private List<CurveArray> GetRoofFootPrint(ElementProperties ep, bool primary)
        {
            var profile = new List<CurveArray>();
            try
            {
                //extrusion of roof profile>> intersect to mass solid>> get bottom face to get roof profile
                var footPrintRoof = ep.CopiedElement as FootPrintRoof;
                if (null != footPrintRoof)
                {
                    var roofProfile = footPrintRoof.GetProfiles();
                    var curveLoopList = new List<CurveLoop>();
                    foreach (ModelCurveArray curveArray in roofProfile)
                    {
                        var curveLoop = new CurveLoop();
                        foreach (ModelCurve modelCurve in curveArray)
                        {
                            curveLoop.Append(modelCurve.GeometryCurve);
                        }
                        curveLoopList.Add(curveLoop);
                    }
                    var profileSolid = GeometryCreationUtilities.CreateExtrusionGeometry(curveLoopList, new XYZ(0, 0, 1), 10);
                    var massSolid = ep.MassContainers[ep.SelectedMassId];
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
                            var normal = ComputeNormalOfFace(face);
                            if (normal.Z < 0) //get bottom face
                            {
                                if (Math.Abs(normal.Z) > Math.Abs(normal.X) && Math.Abs(normal.Z) > Math.Abs(normal.Y))
                                {
                                    foreach (EdgeArray edgeArray in face.EdgeLoops)
                                    {
                                        var curveArray = new CurveArray();
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
            catch (Exception)
            {
                // ignored
            }
            return profile;
        }

        private void EditSlabShape(Face intersectFace, SlabShapeEditor originEditor, SlabShapeEditor newEditor)
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
                            var comparisonResult = intersectFace.Intersect(crease.Curve, out resultArray);
                            if (comparisonResult == SetComparisonResult.Overlap)
                            {
                                try
                                {
                                    var intersectPoint = resultArray.get_Item(0).XYZPoint;
                                    newEditor.DrawPoint(intersectPoint);
                                }
                                catch { continue; }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private Face GetIntersectFace(Solid massSolid, CurveArray profileCurve)
        {
            Face intersectFace = null;
            try
            {
                foreach (Face face in massSolid.Faces)
                {
                    if (null != intersectFace) { break; }
                    foreach (Curve curve in profileCurve)
                    {
                        if (SetComparisonResult.Subset == face.Intersect(curve))
                        {
                            intersectFace = face;
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return intersectFace;
        }

        #endregion

        #region new structural framing
        private void SplitBeam(ElementProperties ep, out List<Element> primaryElementsList, out List<Element> secondaryElementsList)
        {
            primaryElementsList = new List<Element>();
            secondaryElementsList = new List<Element>();
            try
            {
                var originInstance = ep.CopiedElement as FamilyInstance;
                if (null != originInstance)
                {
                    var locationCurve = originInstance.Location as LocationCurve;
                    var curve = locationCurve.Curve.Clone();

                    XYZ interPoint;
                    var newLocationCurves = GetBeamLocationCurve(ep.MassContainers[ep.SelectedMassId], curve, out interPoint);
                    if (newLocationCurves.Count == 2)
                    {
                        FamilyInstance newBeam = null;
                        var hostLevel = originInstance.Host as Level;
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
            catch (Exception)
            {
                // ignored
            }
        }

        private List<Curve> GetBeamLocationCurve(Solid massSolid, Curve originalCurve, out XYZ intersectingPoint)
        {
            var locationCurves = new List<Curve>();
            intersectingPoint = null;
            try
            {
                foreach (Face face in massSolid.Faces)
                {
                    if (null != intersectingPoint) { break; }
                    var resultArray = new IntersectionResultArray();
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
                    var intersectionResult = originalCurve.Project(intersectingPoint);
                    var parameter = intersectionResult.Parameter;
                    if (originalCurve.IsInside(parameter))
                    {
                        var firstCurve = originalCurve.Clone();
                        firstCurve.MakeBound(firstCurve.GetEndParameter(0), parameter);
                        locationCurves.Add(firstCurve);

                        var secondCurve = originalCurve.Clone();
                        secondCurve.MakeBound(parameter, secondCurve.GetEndParameter(1));
                        locationCurves.Add(secondCurve);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
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
                var originInstance = ep.CopiedElement as FamilyInstance;
                var elementId = originInstance.LevelId;

                var instanceLevel = primaryDoc.GetElement(elementId) as Level;
                if (null != originInstance)
                {
                    Curve curve = null;
                    if (originInstance.IsSlantedColumn)
                    {
                        var locationCurve = originInstance.Location as LocationCurve;
                        curve = locationCurve.Curve.Clone();
                    }
                    else if (originInstance.Location is LocationPoint)
                    {
                        var boundingBox = originInstance.get_BoundingBox(primaryDoc.ActiveView);
                        var locationPoint = originInstance.Location as LocationPoint;
                        var pointXYZ = locationPoint.Point;
                        var startPoint = new XYZ(pointXYZ.X, pointXYZ.Y, boundingBox.Min.Z);
                        var endPoint = new XYZ(pointXYZ.X, pointXYZ.Y, boundingBox.Max.Z);

                        curve = Autodesk.Revit.DB.Line.CreateBound(startPoint, endPoint);

                    }
                    if (null != curve)
                    {
                        XYZ interPoint;
                        var newCurves = GetColumnCurve(ep.MassContainers[ep.SelectedMassId], curve, out interPoint);
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
            catch (Exception)
            {
                // ignored
            }
        }

        private List<Curve> GetColumnCurve(Solid massSolid, Curve originalCurve, out XYZ intersectingPoint)
        {
            var locationCurves = new List<Curve>();
            intersectingPoint = null;
            try
            {
                foreach (Face face in massSolid.Faces)
                {
                    if (null != intersectingPoint) { break; }
                    var resultArray = new IntersectionResultArray();
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
                    var intersectionResult = originalCurve.Project(intersectingPoint);
                    var parameter = intersectionResult.Parameter;
                    if (originalCurve.IsInside(parameter))
                    {
                        var firstCurve = originalCurve.Clone();
                        firstCurve.MakeBound(firstCurve.GetEndParameter(0), parameter);
                        locationCurves.Add(firstCurve);

                        var secondCurve = originalCurve.Clone();
                        secondCurve.MakeBound(parameter, secondCurve.GetEndParameter(1));
                        locationCurves.Add(secondCurve);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
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
                var originPipe = ep.CopiedElement as Pipe;

                if (null != originPipe)
                {
                    var locationCurve = originPipe.Location as LocationCurve;
                    var curve = locationCurve.Curve.Clone();

                    var intersectingPoint = GetIntersectingPoint(ep.MassContainers[ep.SelectedMassId], curve);
                    if (null != intersectingPoint)
                    {
                        Pipe newPipe = null;
                        try { newPipe = Pipe.Create(primaryDoc, originPipe.MEPSystem.GetTypeId(), originPipe.GetTypeId(), originPipe.ReferenceLevel.Id, curve.GetEndPoint(0), intersectingPoint); }
                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                        if (null != newPipe) { primaryElementsList.Add(newPipe); }

                        newPipe = null;
                        try { newPipe = Pipe.Create(primaryDoc, originPipe.MEPSystem.GetTypeId(), originPipe.GetTypeId(), originPipe.ReferenceLevel.Id, intersectingPoint, curve.GetEndPoint(1)); }

                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                        if (null != newPipe) { secondaryElementsList.Add(newPipe); }
                        //ICollection<ElementId> pipeIds= PlumbingUtils.ConvertPipePlaceholders(doc, placeHolderIds);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
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
                    var resultArray = new IntersectionResultArray();
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
            catch (Exception)
            {
                // ignored
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
                var originDuct = ep.CopiedElement as Duct;

                if (null != originDuct)
                {
                    var locationCurve = originDuct.Location as LocationCurve;
                    var curve = locationCurve.Curve.Clone();

                    var intersectingPoint = GetIntersectingPoint(ep.MassContainers[ep.SelectedMassId], curve);
                    if (null != intersectingPoint)
                    {
                        Duct newDuct = null;
                        try { newDuct = Duct.Create(primaryDoc, originDuct.MEPSystem.GetTypeId(), originDuct.DuctType.Id, originDuct.LevelId, curve.GetEndPoint(0), intersectingPoint); }

                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                        if (null != newDuct) { primaryElementsList.Add(newDuct); }

                        newDuct = null;
                        try { newDuct = Duct.Create(primaryDoc, originDuct.MEPSystem.GetTypeId(), originDuct.DuctType.Id, originDuct.LevelId, intersectingPoint, curve.GetEndPoint(1)); }

                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                        if (null != newDuct) { secondaryElementsList.Add(newDuct); }

                        //ICollection<ElementId> pipeIds= PlumbingUtils.ConvertPipePlaceholders(doc, placeHolderIds);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
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
                var originConduit = ep.CopiedElement as Conduit;

                if (null != originConduit)
                {
                    var locationCurve = originConduit.Location as LocationCurve;
                    var curve = locationCurve.Curve.Clone();

                    var intersectingPoint = GetIntersectingPoint(ep.MassContainers[ep.SelectedMassId], curve);
                    if (null != intersectingPoint)
                    {
                        Conduit newConduit = null;
                        try { newConduit = Conduit.Create(primaryDoc, originConduit.GetTypeId(), curve.GetEndPoint(0), intersectingPoint, originConduit.ReferenceLevel.Id); }

                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                        if (null != newConduit) { primaryElementsList.Add(newConduit); }

                        newConduit = null;
                        try { newConduit = Conduit.Create(primaryDoc, originConduit.GetTypeId(), intersectingPoint, curve.GetEndPoint(1), originConduit.ReferenceLevel.Id); }

                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                        if (null != newConduit) { secondaryElementsList.Add(newConduit); }
                        //ICollection<ElementId> pipeIds= PlumbingUtils.ConvertPipePlaceholders(doc, placeHolderIds);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
        #endregion

        private XYZ ComputeNormalOfFace(Face face)
        {
            XYZ normal;
            var bbuv = face.GetBoundingBox();
            var faceCenter = (bbuv.Min + bbuv.Max) / 2;
            normal = face.ComputeNormal(faceCenter);
            return normal;
        }

        private ElementProperties TransferParameterValues(ElementProperties ep)
        {
            try
            {
                var primaryIds = new List<ElementId>();
                var secondaryIds = new List<ElementId>();
                foreach (var element in ep.PrimaryElements)
                {
                    primaryIds.Add(element.Id);
                }
                foreach (var element in ep.SecondaryElements)
                {
                    secondaryIds.Add(element.Id);
                }

                using (var trans = new Transaction(primaryDoc))
                {
                    trans.Start("Transfer parameters");
                    var failureHandlingOptions = trans.GetFailureHandlingOptions();
                    var failureHandler = new FailureHandler();
                    failureHandlingOptions.SetFailuresPreprocessor(failureHandler);
                    failureHandlingOptions.SetClearAfterRollback(true);
                    trans.SetFailureHandlingOptions(failureHandlingOptions);

                    var originalElement = ep.CopiedElement;
                    foreach (Parameter param in originalElement.Parameters)
                    {
                        var paramName = param.Definition.Name;
                        if (paramName.Contains("Extensions")) { continue; }
                        //if (param.IsReadOnly) { continue; }
                        if (!param.HasValue) { continue; }
                        var definition = param.Definition as InternalDefinition;
                        if (null != definition)
                        {
                            if (parametersToSkip.Contains(definition.BuiltInParameter)) { continue; }
                            if (definition.BuiltInParameter == BuiltInParameter.ALL_MODEL_MARK)
                            {
                                var markVal = param.AsString();
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

                        foreach (var primaryElement in ep.PrimaryElements)
                        {
                            var parameter = primaryElement.LookupParameter(paramName);
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
                        foreach (var secondaryElement in ep.SecondaryElements)
                        {
                            var parameter = secondaryElement.LookupParameter(paramName);
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

                var primaryElements = new List<Element>();
                var secondaryElements = new List<Element>();

                foreach (var elementId in primaryIds)
                {
                    var element = primaryDoc.GetElement(elementId);
                    if (null != element)
                    {
                        primaryElements.Add(element);
                    }
                }
                ep.PrimaryElements = primaryElements;

                foreach (var elementId in secondaryIds)
                {
                    var element = primaryDoc.GetElement(elementId);
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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                return ep;
            }
        }

        public void DeleteOriginalElement(ElementProperties ep)
        {
            try
            {
                using (var trans = new Transaction(primaryDoc))
                {
                    trans.Start("Delete original element");
                    try { primaryDoc.Delete(ep.CopiedElementId); trans.Commit(); }
                    catch { }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

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
}
