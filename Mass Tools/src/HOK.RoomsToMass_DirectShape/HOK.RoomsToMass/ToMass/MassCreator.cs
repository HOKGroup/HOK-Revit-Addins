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
                if (rp.Linked && null != rp.LinkedMass)
                {
                    //delete existing mass first
                    MassProperties existingMass = rp.LinkedMass;
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

                DirectShape createdShape = DirectShape.CreateElement(doc, new ElementId(massCategory), appGuid, rp.RoomId.ToString());
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

        public static bool CreateAreaSolid(Document doc, AreaProperties ap, out MassProperties createdMass)
        {
            bool created = false;
            createdMass = null;
            try
            {
                string appGuid = doc.Application.ActiveAddInId.GetGUID().ToString();
                if (ap.Linked && null != ap.LinkedMass)
                {
                    //delete existing mass first
                    MassProperties existingMass = ap.LinkedMass;
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
                

                DirectShape createdShape = DirectShape.CreateElement(doc, new ElementId(massCategory), appGuid, ap.AreaId.ToString());
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
                MessageBox.Show("Failed to create room solid.\n" + ex.Message, "Create Room Solid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                if (fp.Linked && null != fp.LinkedMass)
                {
                    //delete existing mass first
                    MassProperties existingMass = fp.LinkedMass;
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


                DirectShape createdShape = DirectShape.CreateElement(doc, new ElementId(massCategory), appGuid, fp.FloorId.ToString());
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

    }

    public class MassProperties
    {
        private Element massElement = null;
        private int massId = -1;
        private string massUniqueId = "";
        private string hostUniqueId = "";
        private SourceType hostType = SourceType.None;
        private Solid massSolid = null;
        private XYZ massSolidCentroid = null;
        private XYZ hostSolidCentroid = null;
        private double massHeight = 0;

        public Element MassElement { get { return massElement; } set { massElement = value; } }
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
                                if (solid.Volume > 0)
                                {
                                    massSolid = solid; break;
                                }
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
