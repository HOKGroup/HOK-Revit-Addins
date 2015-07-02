using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace HOK.RoomsToMass.ToMass
{
    public class FloorProperties
    {
        private Floor m_floor;
        private Document m_doc;

        public FloorProperties(Document document, Floor floor)
        {
            m_doc = document;
            m_floor = floor;
        }

        public Floor FloorObject { get { return m_floor; } }
        public string TypeName { get { try { return m_floor.FloorType.Name; } catch { return ""; } } }
        public int ID { get { try { return m_floor.Id.IntegerValue; } catch { return 0; } } }
        public double Height { get; set; }
        public Level LevelObj
        {
            get
            {
                try
                {
#if RELEASE2013
                    ElementId levelId = m_floor.Level.Id;
#elif RELEASE2014||RELEASE2015 ||RELEASE2016
                    ElementId levelId = m_floor.LevelId;
#endif
                    Level level = m_doc.GetElement(levelId) as Level;
                    return level;
                }
                catch { return null; }
            }
        }
        public string Level 
        { 
            get 
            { 
                try
                {
#if RELEASE2013
                    ElementId levelId = m_floor.Level.Id;
#elif RELEASE2014||RELEASE2015 || RELEASE2016
                    ElementId levelId = m_floor.LevelId;
#endif

                    Level level = m_doc.GetElement(levelId) as Level;
                    return level.Name; 
                } 
                catch { return ""; } 
            } 
        }
        public string DesignOption { get { try { return m_floor.DesignOption.Name; } catch { return ""; } } }
        public string Comments { get { try { return m_floor.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString(); } catch { return ""; } } }

        public string Phase
        {
            get
            {
                try
                {
#if RELEASE2013||RELEASE2014
                    ElementId elementId = m_floor.get_Parameter("Phase").AsElementId();
#elif RELEASE2015 || RELEASE2016
                    ElementId elementId = m_floor.LookupParameter("Phase").AsElementId();
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
                Options opt = m_doc.Application.Create.NewGeometryOptions();

                try
                {
                    GeometryElement geomElem = m_floor.get_Geometry(opt);
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

        public XYZ FloorCenter
        {
            get
            {
                try
                {
                    BoundingBoxXYZ bounding = m_floor.get_BoundingBox(null);
                    XYZ center = (bounding.Max + bounding.Min) * 0.5;
                    XYZ floorCenter = new XYZ(center.X, center.Y, center.Z);
                    return floorCenter;
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
                    foreach (Parameter param in m_floor.Parameters)
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

    }
}
