using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace HOK.RoomsToMass.ToMass
{
    public class AreaProperties
    {
        private Area m_area;
        private Document m_doc;

        public AreaProperties(Document document, Area area)
        {
            m_doc = document;
            m_area = area;
        }

        public Area AreaObject { get { return m_area; } }
        public string Name { get { try { return m_area.Name; } catch { return ""; } } }
        public string Number { get { try { return m_area.Number; } catch { return ""; } } }
        public int ID { get { try { return m_area.Id.IntegerValue; } catch { return 0; } } }

        public double Height { get; set; }
        public string Level 
        { 
            get 
            { 
                try
                {
#if RELEASE2013
                    ElementId levelId = m_area.Level.Id;
#elif RELEASE2014||RELEASE2015
                    ElementId levelId = m_area.LevelId;
#endif
                    Level level = m_doc.GetElement(levelId) as Level;
                    return level.Name; 
                } 
                catch { return ""; } 
            } 
        }
        public double Area { get { try { return Math.Round(m_area.Area, 3, MidpointRounding.ToEven); } catch { return 0; } } }
        public double Perimeter { get { try { return m_area.Perimeter; } catch { return 0; } } }
        public string DesignOption { get { try { return m_area.DesignOption.Name; } catch { return ""; } } }
        public string AreaType { get { try { return m_area.get_Parameter(BuiltInParameter.AREA_TYPE_TEXT).AsString(); } catch { return ""; } } }

        public XYZ AreaLocation
        {
            get
            {
                LocationPoint location = m_area.Location as LocationPoint;
                try { return location.Point; }
                catch { return null; }
            }
        }

        public CurveArrArray CurveArrArray
        {
            get
            {
                CurveArrArray profiles = new CurveArrArray();
                try
                {
                    if (m_area.GetBoundarySegments(new SpatialElementBoundaryOptions()) != null)
                    {
                        foreach (IList<Autodesk.Revit.DB.BoundarySegment> boundarySegments in m_area.GetBoundarySegments(new SpatialElementBoundaryOptions()))
                        {
                            if (boundarySegments.Count > 0)
                            {
                                CurveArray profile = new CurveArray();
                                foreach (Autodesk.Revit.DB.BoundarySegment boundarySegment in boundarySegments)
                                {
                                    Curve curve = boundarySegment.Curve;
                                    profile.Append(curve);
                                }
                                profiles.Append(profile);
                            }
                        }
                    }
                    return profiles;
                }
                catch
                {
                    return profiles;
                }
            }
        }

        public bool IsModified { get; set; }

        public bool IsPlaced { get; set; }

        public Dictionary<string, Parameter> Parameters
        {
            get
            {
                Dictionary<string, Parameter> parameters = new Dictionary<string, Parameter>();
                try
                {
                    foreach (Parameter param in m_area.Parameters)
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
