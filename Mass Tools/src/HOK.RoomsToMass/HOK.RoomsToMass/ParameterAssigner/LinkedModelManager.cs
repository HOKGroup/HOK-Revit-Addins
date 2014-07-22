using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using Autodesk.Revit.DB.Structure;

namespace HOK.RoomsToMass.ParameterAssigner
{
    public class LinkedInstanceProperties
    {
        private RevitLinkInstance m_instance;
        private string[] info; //LinkInstance.rvt:3:location<Not Shared>

        public LinkedInstanceProperties(RevitLinkInstance instance)
        {
            m_instance = instance;
            try { info = instance.Name.Split(':'); }
            catch { }
        }

        public int TypeId { get; set; }
        public int InstanceId { get { try { return m_instance.Id.IntegerValue; } catch { return 0; } } }
        public string InstanceName { get; set; }
        public string FileName { get; set; }
        public Document LinkedDocument { get; set; }
        public Transform TransformValue { get { try { return m_instance.GetTransform(); } catch { return null; } } }
        public List<Element> MassElements { get; set; }
        public Dictionary<int/*massId*/, MassProperties> MassContainers { get; set; }
    }

    public class MassProperties
    {
        private FamilyInstance m_mass;
        private List<Solid> solids = new List<Solid>();

        public MassProperties(FamilyInstance instance) 
        {
            m_mass=instance;
        }

        public FamilyInstance MassInstance { get{try{return m_mass;}catch{return null;}} set{m_mass=value;} } //from linked document
        public int MassId { get { try { return m_mass.Id.IntegerValue; } catch { return 0; } } } //from linked document
        public string MassName { get { try { return m_mass.Symbol.Name; } catch { return ""; } }  }
        public int WorksetId { get; set; } //from host project/linked document
        public Solid MassSolid { get; set; } //from linked document
        public List<Parameter> MassParameters 
        {
            get
            {
                List<Parameter> parameters = new List<Parameter>();
                foreach (Parameter param in m_mass.Parameters)
                {
                    if (param.StorageType == StorageType.ElementId) { continue; }
                    if (param.Definition.Name.Contains("Extensions.")) { continue; }
                    if (param.Id.IntegerValue == (int)BuiltInParameter.ELEM_PARTITION_PARAM) { continue; }
                    parameters.Add(param);
                }
                return parameters;
            }
        } //from linked document.
        public List<Element> ElementContainer { get; set; } //from host project
        public List<Element> FilteredContainer { get; set; }
        public int ElementCount { get; set; }//from host project
        public List<Element> LinkedElementContainer { get; set; }
        public int LinkedElementCount { get; set; }
        public bool IsHost { get; set; }
        public string LInkedFileName { get; set; }
    }

    public class ElementProperties
    {
        private Element m_elem;
        public ElementProperties(Element element)
        {
            m_elem = element;
        }

        public Document Doc { get; set; }
        public int ElementId { get { try { return m_elem.Id.IntegerValue; } catch { return 0; } } }
        public string ElementName { get { try { return m_elem.Name; } catch { return ""; } } }
        public int HostElementId { get; set; }
        public Element ElementObj { get { return m_elem; } set { m_elem = value; } }

        public Element CopiedElement { get; set; }
        public ElementId CopiedElementId { get; set; }

        public int CategoryId { get { try { return m_elem.Category.Id.IntegerValue; } catch { return 0; } } }
        public string CategoryName { get { try { return m_elem.Category.Name; } catch { return ""; } } }
        public Dictionary<int/*massId*/, Solid/*MassSolid*/> MassContainers { get; set; }
        public Solid ElementSolid { get; set; }
        public Dictionary<int/*massId*/, double/*percentage of intersected*/> OpverappingMaps { get; set; }
        
        //use for split options
        public bool LinkedElement { get; set; }
        public int SelectedMassId { get; set; }
        public List<Element> PrimaryElements { get; set; } //splited element from selected mass
        public List<Element> SecondaryElements { get; set; } //result of the differecne operation of the mass

        public bool SplitSucceed { get; set; }
    }

    public class FamilyInstanceProperties
    {
        public FamilyInstanceProperties() { }
        public int InstanceId { get; set; }
        public string InstanceName { get; set; }
        public int HostId { get; set; }
        public FamilySymbol Symbol { get; set; }
        public XYZ PointLocation { get; set; }
        public StructuralType StucturalType { get; set; }
        public bool FacingFlipped { get; set; }
        public bool HandFlipped { get; set; }
        public bool Mirrored { get; set; }
        public Dictionary<string,ParameterProperties> Parameters { get; set; }
    }

    public class ParameterProperties
    {
        private Parameter m_param;


        public ParameterProperties(Parameter param) { m_param = param; }

        public string ParamName { get { return m_param.Definition.Name; } }

        public int ParamId { get { return m_param.Id.IntegerValue; } }

        public StorageType ParamStorageType { get { return m_param.StorageType; } }

        private List<ElementId> elementIds = new List<ElementId>();
        public List<ElementId> ElementIDValues { get { return elementIds; } set { elementIds = value; } }

        private List<double> doubleValues = new List<double>();
        public List<double> DoubleValues { get { return doubleValues; } set { doubleValues = value; } }

        private List<int> intValues = new List<int>();
        public List<int> IntValues { get { return intValues; } set { intValues = value; } }

        private List<string> strValues = new List<string>();
        public List<string> StringValues { get { return strValues; } set { strValues = value; } }

        public void AddValue(Parameter param)
        {
            switch (param.StorageType)
            {
                case StorageType.ElementId:
                    ElementId eId = param.AsElementId();
                    if (eId!=ElementId.InvalidElementId && !ElementIDValues.Contains(eId))
                    {
                        ElementIDValues.Add(eId);
                    }
                    break;
                case StorageType.Double:
                    double dbl = param.AsDouble();
                    if (!DoubleValues.Contains(dbl))
                    {
                        DoubleValues.Add(dbl);
                    }
                    break;
                case StorageType.Integer:
                    int intVal = param.AsInteger();
                    if (!IntValues.Contains(intVal))
                    {
                        IntValues.Add(intVal);
                    }
                    break;
                case StorageType.String:
                    string str = param.AsString();
                    if(!string.IsNullOrEmpty(str) && !StringValues.Contains(str))
                    {
                        StringValues.Add(str);
                    }
                    break;
            }
        }

        
        
    }
}
