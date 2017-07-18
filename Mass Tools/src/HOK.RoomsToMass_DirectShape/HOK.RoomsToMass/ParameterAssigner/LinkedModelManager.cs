using System.Collections.Generic;
using Autodesk.Revit.DB;
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
        public Transform TransformValue { get { try { return m_instance.GetTotalTransform(); } catch { return null; } } }
        public List<Element> MassElements { get; set; }
        public Dictionary<int/*massId*/, MassProperties> MassContainers { get; set; }
    }

    public class MassProperties
    {
        private Element m_mass;
        private List<Solid> solids = new List<Solid>();

        public MassProperties(Element instance) 
        {
            m_mass=instance;
        }

        public Element MassInstance { get{try{return m_mass;}catch{return null;}} set{m_mass=value;} } //from linked document
        public int MassId { get { try { return m_mass.Id.IntegerValue; } catch { return 0; } } } //from linked document
        public string MassName { get { try { return m_mass.Name; } catch { return ""; } }  }
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
        public Document Doc { get; set; } = null;
        public int ElementId { get; set; } = -1;
        public string ElementName { get; set; } = "";
        public int HostElementId { get; set; } = -1;
        public Element ElementObj { get; set; } = null;

        public Element CopiedElement { get; set; } = null;
        public ElementId CopiedElementId { get; set; } = Autodesk.Revit.DB.ElementId.InvalidElementId;
        public Transform TransformValue { get; set; } = Transform.Identity;

        public int CategoryId { get; set; } = -1;
        public string CategoryName { get; set; } = "";
        public Dictionary<int/*massId*/, Solid/*MassSolid*/> MassContainers { get; set; } = new Dictionary<int, Solid>();
        public Solid ElementSolid { get; set; } = null;
        public Dictionary<int/*massId*/, double/*percentage of intersected*/> OpverappingMaps { get; set; } = new Dictionary<int, double>();

        //use for split options
        public bool LinkedElement { get; set; } = false;

        public int SelectedMassId { get; set; } = -1;
        public List<Element> PrimaryElements { get; set; } = new List<Element>();
//splited element from selected mass
        public List<Element> SecondaryElements { get; set; } = new List<Element>();
//result of the differecne operation of the mass

        public bool SplitSucceed { get; set; } = false;

        public ElementProperties(Element element)
        {
            ElementObj = element;
            Doc = ElementObj.Document;
            ElementId = ElementObj.Id.IntegerValue;
            ElementName=element.Name;

            if (null != ElementObj.Category)
            {
                CategoryId = ElementObj.Category.Id.IntegerValue;
                CategoryName = ElementObj.Category.Name;
            }
           
            Doc = element.Document;

        }

        
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

        public List<ElementId> ElementIDValues { get; set; } = new List<ElementId>();

        public List<double> DoubleValues { get; set; } = new List<double>();

        public List<int> IntValues { get; set; } = new List<int>();

        public List<string> StringValues { get; set; } = new List<string>();

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
