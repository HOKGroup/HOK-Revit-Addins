using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;

namespace HOK.RoomsToMass.ToMass
{
    public class RoomProperties
    {
        private Room m_room;
        private Document m_doc;
        private double defaultHeight = 0;
        private bool isDefaultHeight = false;

        public RoomProperties(Document document, Room room)
        {
            m_doc = document;
            m_room = room;
        }

        public Room RoomObject { get { return m_room; } }
        public string Name { get { try { return m_room.Name; } catch { return ""; } } }
        public string Number { get { try { return m_room.Number; } catch { return ""; } } }
        public int ID { get { try { return m_room.Id.IntegerValue; } catch { return 0; } } }

#if RELEASE2013||RELEASE2014
        public string Department { get { try { return m_room.get_Parameter("Department").AsString(); } catch { return ""; } } }
#elif RELEASE2015
        public string Department { get { try { return m_room.LookupParameter("Department").AsString(); } catch { return ""; } } }
#endif


        public double UnboundedHeight { get { try { return m_room.UnboundedHeight; } catch { return 0; } } }
        public double DefaultHeight { get { return defaultHeight; } set { defaultHeight = value; } }
        public string Level 
        { 
            get 
            { 
                try
                {
#if RELEASE2013
                    ElementId levelId = m_room.Level.Id;
#elif RELEASE2014||RELEASE2015
                    ElementId levelId = m_room.LevelId;
#endif
                    Level level = m_doc.GetElement(levelId) as Level;
                    return level.Name; 
                } 
                catch { return ""; } 
            } 
        }
        public double Area { get { try { return m_room.Area; } catch { return 0; } } }
        public double Perimeter { get { try { return m_room.Perimeter; } catch { return 0; } } }
        public string DesignOption { get { try { return m_room.DesignOption.Name; } catch { return ""; } } }

        public XYZ RoomLocation
        {
            get
            {
                LocationPoint location = m_room.Location as LocationPoint;
                try { return location.Point; }
                catch { return null; }
            }
        }

        public string Phase
        {
            get
            {
                try
                {
#if RELEASE2013||RELEASE2014
                    ElementId elementId = m_room.get_Parameter("Phase").AsElementId();
#elif RELEASE2015
                    ElementId elementId = m_room.LookupParameter("Phase").AsElementId();
#endif
                    return m_doc.GetElement(elementId).Name;
                }
                catch
                {
                    return "";
                }
            }
        }

        public EdgeArrayArray EdgeArrayArray
        {
            get
            {
                EdgeArrayArray edgeArrayArray = new EdgeArrayArray();
                try
                {
                    GeometryElement geomElem = m_room.ClosedShell;
                    if (geomElem != null)
                    {
                        foreach (GeometryObject geomObj in geomElem)
                        {
                            Solid solid = geomObj as Solid;
                            if (solid != null)
                            {
                                foreach (Face face in solid.Faces)
                                {
                                    UV uv = new UV(0, 0);
                                    XYZ normal = face.ComputeNormal(uv);
                                    if (normal.IsAlmostEqualTo(new XYZ(0, 0, -1)))
                                    {
                                        edgeArrayArray = face.EdgeLoops;
                                    }
                                }
                            }
                        }
                    }

                    return edgeArrayArray;
                }
                catch
                {
                    return edgeArrayArray;
                }
            }
        }

        public bool IsModified { get; set; }

        public bool IsPlaced { get; set; }

        public bool IsDefaultHeight { get { return isDefaultHeight; } set { isDefaultHeight = value; } }

        public XYZ RoomCenter
        {
            get
            {
                try
                {
                    BoundingBoxXYZ bounding = m_room.get_BoundingBox(null);
                    XYZ center = (bounding.Max + bounding.Min) * 0.5;
                    LocationPoint location = m_room.Location as LocationPoint;
                    XYZ roomCenter = new XYZ(center.X, center.Y, location.Point.Z);
                    return roomCenter;
                }
                catch
                {
                    return new XYZ(0, 0, 0);
                }
            }
        }

        public Dictionary<string, Parameter> Parameters
        {
            get
            {
                Dictionary<string, Parameter> parameters = new Dictionary<string, Parameter>();
                try
                {
                    foreach (Parameter param in m_room.Parameters)
                    {
                        if (param.Definition.Name.Contains("Extensions.")) { continue; }
                        parameters.Add(param.Definition.Name, param);
                    }
                    return parameters;
                }
                catch
                {
                    return parameters;
                }
            }
        }

        public MassProperties MassInfo { get; set; }
    }
}
