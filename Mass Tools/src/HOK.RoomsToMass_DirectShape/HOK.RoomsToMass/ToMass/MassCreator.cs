using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using Autodesk.Revit.DB.IFC;

namespace HOK.RoomsToMass.ToMass
{
    public static class MassCreator
    {
        private static BuiltInCategory massCategory = BuiltInCategory.OST_Mass;

        public static bool CreateRoomSolid(Document doc, RoomProperties rp, out MassProperties createdMass)
        {
            bool created = false;
            createdMass = null;
            try
            {
                string appGuid = doc.Application.ActiveAddInId.GetGUID().ToString();
                if (null != rp.Linked3dMass)
                {
                    //delete existing mass first
                    MassProperties existingMass = rp.Linked3dMass;
                    doc.Delete(new ElementId(existingMass.MassId));
                }

                IList<GeometryObject> roomGeometries = new List<GeometryObject>();
                if (rp.RoomHeight == rp.UserHeight)
                {
                    roomGeometries.Add(rp.RoomSolid);
                }
                else if (rp.RoomProfiles.Count > 0)
                {
                    XYZ extrusionDir = new XYZ(0, 0, 1);
                    Solid roomSolid = GeometryCreationUtilities.CreateExtrusionGeometry(rp.RoomProfiles, extrusionDir, rp.UserHeight);
                    if (null != roomSolid)
                    {
                        roomGeometries.Add(roomSolid);
                    }
                }
#if RELEASE2015 || RELEASE2016
                DirectShape createdShape = DirectShape.CreateElement(doc, new ElementId(massCategory), appGuid, rp.RoomId.ToString());
#else
                DirectShape createdShape = DirectShape.CreateElement(doc, new ElementId(massCategory));
                createdShape.ApplicationId = appGuid;
                createdShape.ApplicationDataId = rp.RoomId.ToString();
#endif

#if RELEASE2016
                DirectShapeOptions options = createdShape.GetOptions();
                options.ReferencingOption = DirectShapeReferencingOption.Referenceable;
                createdShape.SetOptions(options);
#endif
                createdShape.SetShape(roomGeometries);
                createdShape.SetName(rp.RoomName);


                Element massElement = doc.GetElement(createdShape.Id);
                if (null != massElement)
                {
                    createdMass = new MassProperties(massElement);
                    createdMass.SetHostInfo(rp.RoomUniqueId, SourceType.Rooms, rp.RoomSolidCentroid, rp.UserHeight);
                    bool stored = MassDataStorageUtil.SetLinkedHostInfo(massElement, SourceType.Rooms.ToString(), rp.RoomUniqueId, rp.RoomSolidCentroid, rp.UserHeight);
                    created = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create room solid.\n" + ex.Message, "Create Room Solid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
            return created;
        }

        public static bool CreateRoomFace(Document doc, RoomProperties rp, out MassProperties createdMass)
        {
            bool created = false;
            createdMass = null;
            try
            {
                string appGuid = doc.Application.ActiveAddInId.GetGUID().ToString();
                if (null != rp.Linked2dMass)
                {
                    //delete existing mass first
                    MassProperties existingMass = rp.Linked2dMass;
                    doc.Delete(new ElementId(existingMass.MassId));
                }

                if (null != rp.BottomFace)
                {
                    IList<GeometryObject> roomGeometries = GetGeometryObjectsFromFace(rp.BottomFace);

#if RELEASE2015 || RELEASE2016
                    DirectShape createdShape = DirectShape.CreateElement(doc, new ElementId(massCategory), appGuid, rp.RoomId.ToString());
#else
                    DirectShape createdShape = DirectShape.CreateElement(doc, new ElementId(massCategory));
                    createdShape.ApplicationId = appGuid;
                    createdShape.ApplicationDataId = rp.RoomId.ToString();
#endif
                    createdShape.SetShape(roomGeometries);
                    createdShape.SetName(rp.RoomName);

                    Element massElement = doc.GetElement(createdShape.Id);
                    if (null != massElement)
                    {
                        createdMass = new MassProperties(massElement);
                        createdMass.MassElementType = MassType.MASS2D;
                        createdMass.SetHostInfo(rp.RoomUniqueId, SourceType.Rooms, rp.RoomSolidCentroid, 0);
                        bool stored = MassDataStorageUtil.SetLinkedHostInfo(massElement, SourceType.Rooms.ToString(), rp.RoomUniqueId, rp.RoomSolidCentroid, 0);
                        created = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create room face.\n" + ex.Message, "Create Room Face", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return created;
        }

        public static bool CreateAreaSolid(Document doc, AreaProperties ap, out MassProperties createdMass)
        {
            bool created = false;
            createdMass = null;
            try
            {
                string appGuid = doc.Application.ActiveAddInId.GetGUID().ToString();
                if (null != ap.Linked3dMass)
                {
                    //delete existing mass first
                    MassProperties existingMass = ap.Linked3dMass;
                    doc.Delete(new ElementId(existingMass.MassId));
                }

                IList<GeometryObject> areaGeometries = new List<GeometryObject>();
                if (ap.AreaProfile.Count > 0)
                {
                    XYZ extrusionDir = new XYZ(0, 0, 1);
                    Solid areaSolid = GeometryCreationUtilities.CreateExtrusionGeometry(ap.AreaProfile, extrusionDir, ap.UserHeight);
                    if (null != areaSolid)
                    {
                        areaGeometries.Add(areaSolid);
                    }
                }
#if RELEASE2015 || RELEASE2016
                DirectShape createdShape = DirectShape.CreateElement(doc, new ElementId(massCategory), appGuid, ap.AreaId.ToString());
#else
                DirectShape createdShape = DirectShape.CreateElement(doc, new ElementId(massCategory));
                createdShape.ApplicationId = appGuid;
                createdShape.ApplicationDataId = ap.AreaId.ToString();
#endif


#if RELEASE2016
                    DirectShapeOptions options = createdShape.GetOptions();
                    options.ReferencingOption = DirectShapeReferencingOption.Referenceable;
                    createdShape.SetOptions(options);
#endif
                createdShape.SetShape(areaGeometries);
                createdShape.SetName(ap.AreaName);

                Element massElement = doc.GetElement(createdShape.Id);
                if (null != massElement)
                {
                    createdMass = new MassProperties(massElement);
                    createdMass.SetHostInfo(ap.AreaUniqueId, SourceType.Areas, ap.AreaCenterPoint, ap.UserHeight);
                    bool stored = MassDataStorageUtil.SetLinkedHostInfo(massElement, SourceType.Areas.ToString(), ap.AreaUniqueId, ap.AreaCenterPoint, ap.UserHeight);
                    created = true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create area solid.\n" + ex.Message, "Create Area Solid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return created;
        }

        public static bool CreateAreaFace(Document doc, AreaProperties ap, out MassProperties createdMass)
        {
            bool created = false;
            createdMass = null;
            try
            {
                string appGuid = doc.Application.ActiveAddInId.GetGUID().ToString();
                if (null != ap.Linked2dMass)
                {
                    //delete existing mass first
                    MassProperties existingMass = ap.Linked2dMass;
                    doc.Delete(new ElementId(existingMass.MassId));
                }

                IList<GeometryObject> areaGeometries = GetGeometryObjectsFromFace(ap.AreaFace);

#if RELEASE2015 || RELEASE2016
                DirectShape createdShape = DirectShape.CreateElement(doc, new ElementId(massCategory), appGuid, ap.AreaId.ToString());
#else
                DirectShape createdShape = DirectShape.CreateElement(doc, new ElementId(massCategory));
                createdShape.ApplicationId = appGuid;
                createdShape.ApplicationDataId = ap.AreaId.ToString();
#endif

#if RELEASE2016
                DirectShapeOptions options = createdShape.GetOptions();
                options.ReferencingOption = DirectShapeReferencingOption.Referenceable;
                createdShape.SetOptions(options);
#endif
                createdShape.SetShape(areaGeometries);
                createdShape.SetName(ap.AreaName);

                Element massElement = doc.GetElement(createdShape.Id);
                if (null != massElement)
                {
                    createdMass = new MassProperties(massElement);
                    createdMass.MassElementType = MassType.MASS2D;
                    createdMass.SetHostInfo(ap.AreaUniqueId, SourceType.Areas, ap.AreaCenterPoint,0);
                    bool stored = MassDataStorageUtil.SetLinkedHostInfo(massElement, SourceType.Areas.ToString(), ap.AreaUniqueId, ap.AreaCenterPoint, 0);
                    created = true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create area face.\n" + ex.Message, "Create Area Face", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return created;
        }

        public static bool CreateFloorSolid(Document doc, FloorProperties fp, out MassProperties createdMass)
        {
            bool created = false;
            createdMass = null;
            try
            {
                string appGuid = doc.Application.ActiveAddInId.GetGUID().ToString();
                if (null != fp.Linked3dMass)
                {
                    //delete existing mass first
                    MassProperties existingMass = fp.Linked3dMass;
                    doc.Delete(new ElementId(existingMass.MassId));
                }

                IList<GeometryObject> floorGeometries = new List<GeometryObject>();
                if (fp.FloorProfiles.Count > 0)
                {
                    XYZ extrusionDir = new XYZ(0, 0, 1);
                    Solid floorSolid = GeometryCreationUtilities.CreateExtrusionGeometry(fp.FloorProfiles, extrusionDir, fp.UserHeight);
                    if (null != floorSolid)
                    {
                        floorGeometries.Add(floorSolid);
                    }
                }
#if RELEASE2015 || RELEASE2016
                DirectShape createdShape = DirectShape.CreateElement(doc, new ElementId(massCategory), appGuid, fp.FloorId.ToString());
#else
                DirectShape createdShape = DirectShape.CreateElement(doc, new ElementId(massCategory));
                createdShape.ApplicationId = appGuid;
                createdShape.ApplicationDataId = fp.FloorId.ToString();
#endif

#if RELEASE2016
                    DirectShapeOptions options = createdShape.GetOptions();
                    options.ReferencingOption = DirectShapeReferencingOption.Referenceable;
                    createdShape.SetOptions(options);
#endif
                createdShape.SetShape(floorGeometries);
                createdShape.SetName(fp.FloorName);

                Element massElement = doc.GetElement(createdShape.Id);
                if (null != massElement)
                {
                    createdMass = new MassProperties(massElement);
                    createdMass.SetHostInfo(fp.FloorUniqueId, SourceType.Floors, fp.FloorSolidCentroid, fp.UserHeight);
                    bool stored = MassDataStorageUtil.SetLinkedHostInfo(massElement, SourceType.Floors.ToString(), fp.FloorUniqueId, fp.FloorSolidCentroid, fp.UserHeight);
                    created = true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create room solid.\n" + ex.Message, "Create Room Solid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return created;
        }

        public static bool CreateFloorFace(Document doc, FloorProperties fp, out MassProperties createdMass)
        {
            bool created = false;
            createdMass = null;
            try
            {
                string appGuid = doc.Application.ActiveAddInId.GetGUID().ToString();
                if (null != fp.Linked2dMass)
                {
                    //delete existing mass first
                    MassProperties existingMass = fp.Linked2dMass;
                    doc.Delete(new ElementId(existingMass.MassId));
                }

                IList<GeometryObject> floorGeometries = GetGeometryObjectsFromFace(fp.TopFace);

#if RELEASE2015 || RELEASE2016
                DirectShape createdShape = DirectShape.CreateElement(doc, new ElementId(massCategory), appGuid, fp.FloorId.ToString());
#else
                DirectShape createdShape = DirectShape.CreateElement(doc, new ElementId(massCategory));
                createdShape.ApplicationId = appGuid;
                createdShape.ApplicationDataId = fp.FloorId.ToString();
#endif

#if RELEASE2016
                DirectShapeOptions options = createdShape.GetOptions();
                options.ReferencingOption = DirectShapeReferencingOption.Referenceable;
                createdShape.SetOptions(options);
#endif
                createdShape.SetShape(floorGeometries);
                createdShape.SetName(fp.FloorName);

                Element massElement = doc.GetElement(createdShape.Id);
                if (null != massElement)
                {
                    createdMass = new MassProperties(massElement);
                    createdMass.MassElementType = MassType.MASS2D;
                    createdMass.SetHostInfo(fp.FloorUniqueId, SourceType.Floors, fp.FloorSolidCentroid, 0);
                    bool stored = MassDataStorageUtil.SetLinkedHostInfo(massElement, SourceType.Floors.ToString(), fp.FloorUniqueId, fp.FloorSolidCentroid, 0);
                    created = true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create room solid.\n" + ex.Message, "Create Room Solid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return created;
        }

        private static IList<GeometryObject> GetGeometryObjectsFromFace(Face face)
        {
            IList<GeometryObject> shapeGeometries = null;
            try
            {
                List<CurveLoop> profiles = new List<CurveLoop>();
                XYZ normal = face.ComputeNormal(new UV(0, 0));
 
                IList<CurveLoop> curveLoops = face.GetEdgesAsCurveLoops();
                IList<IList<CurveLoop>> sortedCurveLoops = ExporterIFCUtils.SortCurveLoops(curveLoops);
                foreach (IList<CurveLoop> curveLoopList in sortedCurveLoops)
                {
                    foreach (CurveLoop curveLoop in curveLoopList)
                    {
                        if (curveLoop.IsCounterclockwise(normal))
                        {
                            profiles.Insert(0, curveLoop);
                        }
                        else
                        {
                            profiles.Add(curveLoop);
                        }
                    }
                }

                List<List<XYZ>> allLoopVertices = new List<List<XYZ>>();
                for (int i = 0; i < profiles.Count; i++)
                {
                    List<XYZ> vertices = new List<XYZ>();
                    CurveLoop curveLoop = profiles[i];
                    foreach (Curve curve in curveLoop)
                    {
                        IList<XYZ> tessellatedVertices = curve.Tessellate();
                        tessellatedVertices.RemoveAt(tessellatedVertices.Count - 1);
                        vertices.AddRange(tessellatedVertices);
                    }
                    allLoopVertices.Add(vertices);
                }

                TessellatedShapeBuilder builder = new TessellatedShapeBuilder();
                builder.OpenConnectedFaceSet(false);

                TessellatedFace tesseFace = new TessellatedFace(allLoopVertices.ToArray(), ElementId.InvalidElementId);
                if (builder.DoesFaceHaveEnoughLoopsAndVertices(tesseFace))
                {
                    builder.AddFace(tesseFace);
                }

                builder.CloseConnectedFaceSet();
#if RELEASE2015 || RELEASE2016
                TessellatedShapeBuilderResult result = builder.Build(TessellatedShapeBuilderTarget.AnyGeometry, TessellatedShapeBuilderFallback.Mesh, ElementId.InvalidElementId);

#else
                builder.Target = TessellatedShapeBuilderTarget.AnyGeometry;
                builder.Fallback = TessellatedShapeBuilderFallback.Mesh;
                builder.Build();
                TessellatedShapeBuilderResult result = builder.GetBuildResult();
#endif
                shapeGeometries = result.GetGeometricalObjects();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get geometry objects from a face.\n" + ex.Message, "Get Geometry Objects", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return shapeGeometries;
        }

    }

    public enum MassType
    {
        MASS3D, MASS2D, NONE
    }

    public class MassProperties
    {
        private Element massElement = null;
        private MassType massElementType = MassType.NONE;
        private int massId = -1;
        private string massUniqueId = "";
        private string hostUniqueId = "";
        private SourceType hostType = SourceType.None;
        private Solid massSolid = null;
        private XYZ massSolidCentroid = null;
        private XYZ hostSolidCentroid = null;
        private double massHeight = 0;

        public Element MassElement { get { return massElement; } set { massElement = value; } }
        public MassType MassElementType { get { return massElementType; } set { massElementType = value; } }
        public int MassId { get { return massId; } set { massId = value; } }
        public string MassUniqueId { get { return massUniqueId; } set { massUniqueId = value; } }
        public string HostUniqueId { get { return hostUniqueId; } set { hostUniqueId = value; } }
        public SourceType HostType { get { return hostType; } set { hostType = value; } }
        public Solid MassSolid { get { return massSolid; } set { massSolid = value; } }
        public XYZ MassSolidCentroid { get { return massSolidCentroid; } set { massSolidCentroid = value; } }
        public XYZ HostSolidCentroid { get { return hostSolidCentroid; } set { hostSolidCentroid = value; } }
        public double MassHeight { get { return massHeight; } set { massHeight = value; } }

        public MassProperties(Element element)
        {
            massElement = element;
            massId = element.Id.IntegerValue;
            massUniqueId = element.UniqueId;
            GetMassGeometry();
        }

        public void SetHostInfo(string hostId, SourceType sourceType, XYZ centroid, double height)
        {
            hostUniqueId = hostId;
            hostType = sourceType;
            hostSolidCentroid = centroid;
            massHeight = height;
            if (height == 0)
            {
                massElementType = MassType.MASS2D;
            }
            else
            {
                massElementType = MassType.MASS3D;
            }
        }

        private void GetMassGeometry()
        {
            try
            {
                GeometryElement geomElement = massElement.get_Geometry(new Options());
                if (null != geomElement)
                {
                    foreach (GeometryObject geomObj in geomElement)
                    {
                        if (null != geomObj)
                        {
                            Solid solid = geomObj as Solid;
                            if (null != solid)
                            {
                                massSolid = solid; break;
                            }
                        }
                    }
                }

                if (null != massSolid)
                {
                    massSolidCentroid = massSolid.ComputeCentroid();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

    }
}
