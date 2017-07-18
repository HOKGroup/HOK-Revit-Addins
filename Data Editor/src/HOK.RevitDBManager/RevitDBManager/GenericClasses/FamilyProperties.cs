using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;

namespace RevitDBManager.Classes
{
    
    public class InstanceProperties
    {
        private FamilyInstance m_instance;
        public InstanceProperties(FamilyInstance instacne)
        {
            m_instance = instacne;
        }

        public FamilyInstance FamilyInstanceObject { get { try { return m_instance; } catch { return null; } } }
        public string GUID { get { try { return m_instance.UniqueId; } catch { return ""; } } }
        public int InstanceID { get { try { return m_instance.Id.IntegerValue; } catch { return 0; } } }
        public int TypeID { get { try { return m_instance.Symbol.Id.IntegerValue; } catch { return 0; } } }
        public Category Category { get { try { return m_instance.Category; } catch { return null; } } }
        public string CategoryName { get { try { return m_instance.Category.Name; } catch { return "null"; } } }
        public int FamilyID { get { try { return m_instance.Symbol.Family.Id.IntegerValue; } catch { return 0; } } }
        public string FamilyName { get { try { return m_instance.Symbol.Family.Name; } catch { return "null"; } } }
        public string FamilyTypeName { get { try { return m_instance.Symbol.Name; } catch { return "null"; } } }
        
        //Revit Internal Data Properties
        public bool FacingFlipped { get { try { return m_instance.FacingFlipped; } catch { return false; } } }
        public Room FromRoom { get { try { return m_instance.FromRoom; } catch { return null; } } }
        public bool HandFlipped { get { try { return m_instance.HandFlipped; } catch { return false; } } }
        public Element Host { get { try { return m_instance.Host; } catch { return null; } } }
        public bool IsSlantedColumn { get { try { return m_instance.IsSlantedColumn; } catch { return false; } } }
        public Location Location { get { try { return m_instance.Location; } catch { return null; } } }
        public bool Mirrored { get { try { return m_instance.Mirrored; } catch { return false; } } }
        public Room Room { get { try { return m_instance.Room; } catch { return null; } } }
        public Room ToRoom { get { try { return m_instance.ToRoom; } catch { return null; } } }
 
        /*
        public List<ParamProperties> InstParameters
        {
            get
            {
                List<ParamProperties> instParameters = new List<ParamProperties>();
                try
                {
                    foreach (Parameter param in m_instance.Parameters)
                    {
                        if (param.StorageType == StorageType.ElementId) { continue; }
                        InternalDefinition definition = param.Definition as InternalDefinition;
                        if (definition.Visible == false) { continue; }
                        if (param.Definition.Name.Contains(".")) { continue; }

                        ParamProperties pp = new ParamProperties(param);
                        instParameters.Add(pp);
                    }
                    return instParameters;
                }
                catch
                {
                    return instParameters;
                }
            }
        }
        */

        public Dictionary<int, ParamProperties> InstParameters
        {
            get
            {
                Dictionary<int, ParamProperties> instParameters = new Dictionary<int, ParamProperties>();
                try
                {
                    foreach (Parameter param in m_instance.Parameters)
                    {
                        if (param.StorageType == StorageType.ElementId) { continue; }
                        InternalDefinition definition = param.Definition as InternalDefinition;
                        if (definition.Visible == false) { continue; }
                        if (param.Definition.Name.Contains(".")) { continue; }
                        ParamProperties pp = new ParamProperties(param);
                        instParameters.Add(pp.ParamID, pp);
                    }
                    return instParameters;
                }
                catch
                {
                    return instParameters;
                }
            }
        }
    }

    public class TypeProperties
    {
        private FamilySymbol m_symbol;
        public TypeProperties(FamilySymbol symbol)
        {
            m_symbol = symbol;
        }
        public FamilySymbol FamilySymbolObject { get { try { return m_symbol; } catch { return null; } } }
        public string GUID { get { try { return m_symbol.UniqueId; } catch { return "null"; } } }
        public int TypeID { get { try { return m_symbol.Id.IntegerValue; } catch { return 0; } } }
        public Category Category { get { try { return m_symbol.Category; } catch { return null; } } }
        public string CategoryName { get { try { return m_symbol.Category.Name; } catch { return "null"; } } }
        public int FamilyID { get { try { return m_symbol.Family.Id.IntegerValue; } catch { return 0; } } }
        public string FamilyName { get { try { return m_symbol.Family.Name; } catch { return "null"; } } }
        public string FamilyTypeName { get { try { return m_symbol.Name; } catch { return "null"; } } }
        /*
        public List<ParamProperties> TypeParameters
        {
            get
            {
                List<ParamProperties> typeParameters = new List<ParamProperties>();
                try
                {
                    foreach (Parameter param in m_symbol.Parameters)
                    {
                        if (param.StorageType == StorageType.ElementId) { continue; }
                        InternalDefinition definition = param.Definition as InternalDefinition;
                        if (definition.Visible == false) { continue; }
                        if (param.Definition.Name.Contains(".")) { continue; }
                        ParamProperties pp = new ParamProperties(param);
                        typeParameters.Add(pp);
                    }
                    return typeParameters;
                }
                catch
                {
                    return typeParameters;
                }
            }
        }*/

        public Dictionary<int, ParamProperties> TypeParameters
        {
            get
            {
                Dictionary<int, ParamProperties> typeParameters = new Dictionary<int, ParamProperties>();
                try
                {
                    foreach (Parameter param in m_symbol.Parameters)
                    {
                        if (param.StorageType == StorageType.ElementId) { continue; }
                        InternalDefinition definition = param.Definition as InternalDefinition;
                        if (definition.Visible == false) { continue; }
                        if (param.Definition.Name.Contains(".")) { continue; }
                        ParamProperties pp = new ParamProperties(param);
                        typeParameters.Add(pp.ParamID,pp);
                    }
                    return typeParameters;
                }
                catch
                {
                    return typeParameters;
                }
            }
        }
    }

    public class ElementTypeProperties
    {
        private ElementType m_elementType;
        private FamilySymbol m_symbol;

        public ElementTypeProperties(ElementType elementType)
        {
            m_elementType = elementType;
            m_symbol = m_elementType as FamilySymbol;
        }

        public ElementType ElementTypeObject { get { return m_elementType; } }
        public string GUID { get { try { return m_elementType.UniqueId; } catch { return "null"; } } }
        public int TypeID { get { try { return m_elementType.Id.IntegerValue; } catch { return 0; } } }
        public string CategoryName { get { try { return m_elementType.Category.Name; } catch { return "null"; } } }
        public int CategoryID { get { try { return m_elementType.Category.Id.IntegerValue; } catch { return 0; } } }
        public BuiltInCategory BuiltInCategory { get { try { return (BuiltInCategory)m_elementType.Category.Id.IntegerValue; } catch { return BuiltInCategory.INVALID; } } }
        public string ElementTypeName { get { try { return m_elementType.Name; } catch { return "null"; } } }
        
        public bool IsFamilySymbol
        {
            get
            {
                if (null != m_symbol) { return true; }
                else { return false; }
            }
        }
        public FamilySymbol FamilySymbolObject { get { return m_symbol; } }
        public int FamilyID { get { try { return m_symbol.Family.Id.IntegerValue; } catch { return 0; } } }
        public string FamilyName { get { try { return m_symbol.Family.Name; } catch { return "null"; } } }

        /*
        public List<ParamProperties> ElementTypeParameters 
        {
            get
            {
                List<ParamProperties> typeParameters = new List<ParamProperties>();
                try
                {
                    foreach (Parameter param in m_elementType.Parameters)
                    {
                        if (param.StorageType == StorageType.ElementId) { continue; }
                        InternalDefinition definition = param.Definition as InternalDefinition;
                        if (definition.Visible == false) { continue; }
                        if (param.Definition.Name.Contains(".")) { continue; }
                        ParamProperties pp = new ParamProperties(param);
                        typeParameters.Add(pp);
                    }
                    return typeParameters;
                }
                catch
                {
                    return typeParameters;
                }
            }
        }*/

        public Dictionary<int, ParamProperties> ElementTypeParameters
        {
            get
            {
                Dictionary<int, ParamProperties> typeParameters = new Dictionary<int, ParamProperties>();
                try
                {
                    foreach (Parameter param in m_elementType.Parameters)
                    {
                        if (param.StorageType == StorageType.ElementId) { continue; }
                        InternalDefinition definition = param.Definition as InternalDefinition;
                        if (definition.Visible == false) { continue; }
                        if (param.Definition.Name.Contains(".")) { continue; }
                        ParamProperties pp = new ParamProperties(param);
                        typeParameters.Add(pp.ParamID,pp);
                    }
                    return typeParameters;
                }
                catch
                {
                    return typeParameters;
                }
            }
        }

    }
    
    //System Family
    public class ElementProperties
    {
        private Element m_element;
        public ElementProperties(Element element)
        {
            m_element = element;
        }

        public Element ElementObject { get { return m_element; } }
        public int ElemntID { get { try { return m_element.Id.IntegerValue; } catch { return 0; } } }
        public string ElementName { get { try { return m_element.Name; } catch { return "null"; } } }
        public int CategoryID { get { try { return m_element.Category.Id.IntegerValue; } catch { return 0; } } }
        public string CategoryName { get { try { return m_element.Category.Name; } catch { return "null"; } } }
        public int TypeID { get { try { return m_element.GetTypeId().IntegerValue; } catch { return 0; } } }
        public string TypeName { get { try { return m_element.GetType().Name; } catch { return "null"; } } }

        /*
        public List<ParamProperties> ElementParameters
        {
            get
            {
                List<ParamProperties> elementParameters = new List<ParamProperties>();
                try
                {
                    foreach (Parameter param in m_element.Parameters)
                    {
                        if (param.StorageType == StorageType.ElementId) { continue; }
                        InternalDefinition definition = param.Definition as InternalDefinition;
                        if (definition.Visible == false) { continue; }
                        if (param.Definition.Name.Contains(".")) { continue; }
                        ParamProperties pp = new ParamProperties(param);
                        elementParameters.Add(pp);
                    }
                    return elementParameters;
                }
                catch
                {
                    return elementParameters;
                }
            }
        }
        */

        public Dictionary<int, ParamProperties> ElementParameters
        {
            get
            {
                Dictionary<int, ParamProperties> elementParameters = new Dictionary<int, ParamProperties>();
                try
                {
                    foreach (Parameter param in m_element.Parameters)
                    {
                        if (param.StorageType == StorageType.ElementId) { continue; }
                        InternalDefinition definition = param.Definition as InternalDefinition;
                        if (definition.Visible == false) { continue; }
                        if (param.Definition.Name.Contains(".")) { continue; }
                        ParamProperties pp = new ParamProperties(param);
                        elementParameters.Add(pp.ParamID, pp);
                    }
                    return elementParameters;
                }
                catch
                {
                    return elementParameters;
                }
            }
        }
    }

    public class RoomProperties
    {
        private Room m_room;
        private Document m_doc;

        public RoomProperties(Room room, Document doc)
        {
            m_room = room;
            m_doc = doc;
        }

        public string GUID { get { try { return m_room.UniqueId; } catch { return "null"; } } }
        public int RoomID { get { try { return m_room.Id.IntegerValue; } catch { return 0; } } }
        public string Name { get { try { return m_room.Name; } catch { return "null"; } } }
        public string Number { get { try { return m_room.Number; } catch { return "0"; } } }
#if RELEASE2013
        public string Level { get { try { return m_room.Level.Name; } catch { return "null"; } } }
#elif RELEASE2014 || RELEASE2015 || RELEASE2016 || RELEASE2017 || RELEASE2018
        public string Level 
        { 
            get 
            { 
                try 
                {
                    ElementId levelId = m_room.LevelId;
                    Level level = m_doc.GetElement(levelId) as Level;
                    if (null != level)
                    {
                        return level.Name;
                    }
                    else { return "null"; }
                } 
                catch { return "null"; } 
            } 
        }
#endif


        //Room Dimensions
        public double Area { get { try { return m_room.Area; } catch { return 0; } } }
        public double BaseOffset { get { try { return m_room.BaseOffset; } catch { return 0; } } }
        public double LimitOffset { get { try { return m_room.LimitOffset; } catch { return 0; } } }
        public Location Location { get { try { return m_room.Location; } catch { return null; } } }
        public double UnboundedHeight { get { try { return m_room.UnboundedHeight; } catch { return 0; } } }
        public string UpperLimit { get { try { return m_room.UpperLimit.Name; } catch { return "null"; } } } //level name
        public double Volume { get { try { return m_room.Volume; } catch { return 0; } } }
        
        public Dictionary<int, ParamProperties> RoomParameters
        {
            get
            {
                Dictionary<int, ParamProperties> roomParameters = new Dictionary<int, ParamProperties>();
                try
                {
                    foreach (Parameter param in m_room.Parameters)
                    {
                        //if (param.StorageType == StorageType.ElementId) { continue; }
                        InternalDefinition definition = param.Definition as InternalDefinition;
                        if (definition.Visible == false) { continue; }
                        if (param.Definition.Name.Contains(".")) { continue; }
                        ParamProperties pp = new ParamProperties(param);
                        roomParameters.Add(pp.ParamID, pp);
                    }
                    return roomParameters;
                }
                catch
                {
                    return roomParameters;
                }
            }
        }
    }

    public class SpaceProperties
    {
        private Space m_space;
        private Document m_doc;

        public SpaceProperties(Space space, Document doc)
        {
            m_space = space;
            m_doc = doc;
        }

        public string GUID { get { try { return m_space.UniqueId; } catch { return "null"; } } }
        public int SpaceID { get { try { return m_space.Id.IntegerValue; } catch { return 0; } } }
        public string Name { get { try { return m_space.Name; } catch { return "null"; } } set { m_space.Name = value; } }
        public string Number { get { try { return m_space.Number; } catch { return "0"; } } set { m_space.Number = value; } }

#if RELEASE2013
        public string Level { get { try { return m_space.Level.Name; } catch { return "null"; } } }
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
        public string Level
        {
            get
            {
                try
                {
                    ElementId elementId = m_space.LevelId;
                    Level level = m_doc.GetElement(elementId) as Level;
                    if (null != level)
                    {
                        return level.Name;
                    }
                    else
                    {
                        return "null";
                    }
                }
                catch { return "null"; }
            }
        }
#endif


        public Dictionary<int, ParamProperties> SpaceParameters
        {
            get
            {
                Dictionary<int, ParamProperties> spaceParameters = new Dictionary<int, ParamProperties>();
                try
                {
                    foreach (Parameter param in m_space.Parameters)
                    {
                        if (param.StorageType == StorageType.ElementId) { continue; } //skip to elementId 
                        InternalDefinition definition = param.Definition as InternalDefinition;
                        if (definition.Visible == false) { continue; }
                        if (param.Definition.Name.Contains(".")) { continue; }
                        ParamProperties pp = new ParamProperties(param);
                        spaceParameters.Add(pp.ParamID,pp);
                    }
                    return spaceParameters;
                }
                catch
                {
                    return spaceParameters;
                }
            }
        }
    }

    public class AreaProperties
    {
        private Area m_area;
        private Document m_doc;

        public AreaProperties(Area area, Document doc)
        {
            m_area = area;
            m_doc = doc;
        }

        public string GUID { get { try { return m_area.UniqueId; } catch { return "null"; } } }
        public int AreaID { get { try { return m_area.Id.IntegerValue; } catch { return 0; } } }
        public string Name { get { try { return m_area.Name; } catch { return "null"; } } set { m_area.Name = value; } }
        public string Number { get { try { return m_area.Number; } catch { return "0"; } } set { m_area.Number = value; } }
#if RELEASE2013
        public string Level { get { try { return m_area.Level.Name; } catch { return "null"; } } }
#elif RELEASE2014 ||RELEASE2015 || RELEASE2016 || RELEASE2017
        public string Level
        {
            get
            {
                try
                {
                    ElementId elementId = m_area.LevelId;
                    Level level = m_doc.GetElement(elementId) as Level;
                    if (null != level)
                    {
                        return level.Name;
                    }
                    else
                    {
                        return "null";
                    }
                }
                catch { return "null"; }
            }
        }
#endif


        public Dictionary<int, ParamProperties> AreaParameters
        {
            get
            {
                Dictionary<int, ParamProperties> areaParameters = new Dictionary<int, ParamProperties>();
                try
                {
                    foreach (Parameter param in m_area.Parameters)
                    {
                        if (param.StorageType == StorageType.ElementId) { continue; } //skip to elementId 
                        InternalDefinition definition = param.Definition as InternalDefinition;
                        if (definition.Visible == false) { continue; }
                        if (param.Definition.Name.Contains(".")) { continue; }
                        ParamProperties pp = new ParamProperties(param);
                        areaParameters.Add(pp.ParamID, pp);
                    }
                    return areaParameters;
                }
                catch
                {
                    return areaParameters;
                }
            }
        }
    }

    public class ViewProperties
    {
        private View m_view;
        private Document m_doc;

        public ViewProperties(View view, Document doc)
        {
            m_view = view;
            m_doc = doc;
        }

        public string GUID { get { try { return m_view.UniqueId; } catch { return "null"; } } }
        public int ViewID { get { try { return m_view.Id.IntegerValue; } catch { return 0; } } }
        public int TypeID { get { try { return m_view.GetTypeId().IntegerValue; } catch { return 0; } } }
        public ViewType ViewType { get { try { return m_view.ViewType; } catch { return Autodesk.Revit.DB.ViewType.Undefined; } } }
        public string ViewTypeName { get { try { return m_view.ViewType.ToString(); } catch { return "UndefinedView"; } } }
        public string Name { get { try { return m_view.Name; } catch { return "null"; } } set { m_view.Name = value; } }
#if RELEASE2013
        public string Level { get { try { return m_view.Level.Name; } catch { return "null"; } } }
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
        public string Level
        {
            get
            {
                try
                {
                    ElementId elementId = m_view.LevelId;
                    Level level = m_doc.GetElement(elementId) as Level;
                    if (null != level)
                    {
                        return level.Name;
                    }
                    else
                    {
                        return "null";
                    }
                }
                catch { return "null"; }
            }
        }
#endif
        

        public Dictionary<int, ParamProperties> ViewParameters
        {
            get
            {
                Dictionary<int, ParamProperties> viewParameters = new Dictionary<int, ParamProperties>();
                try
                {
                    foreach (Parameter param in m_view.Parameters)
                    {
                        if (param.StorageType == StorageType.ElementId) { continue; } //skip to elementId 
                        InternalDefinition definition = param.Definition as InternalDefinition;
                        if (definition.Visible == false) { continue; }
                        if (param.Definition.Name.Contains(".")) { continue; }
                        ParamProperties pp = new ParamProperties(param);
                        viewParameters.Add(pp.ParamID, pp);
                    }
                    return viewParameters;
                }
                catch
                {
                    return viewParameters;
                }
            }
        }
    }

    public class ViewTypeProperties
    {
        public static string[] excludeViewFamily = new string[] { "Invalid", "Schedule", "Walkthrough", "ImageView", "CostReport", "LoadsReport", 
            "PressureLossReport", "PanelSchedule", "GraphicalColumnSchedule", "StructuralPlan" };

        private ViewFamilyType m_viewType;

        public ViewTypeProperties(ViewFamilyType viewType)
        {
            m_viewType = viewType;
        }

        public string GUID { get { try { return m_viewType.UniqueId; } catch { return "null"; } } }
        public int ViewTypeID { get { try { return m_viewType.Id.IntegerValue; } catch { return 0; } } }
        public ViewFamilyType ViewType { get { try { return m_viewType; } catch { return null; } } }
        public string ViewTypeName { get { try { return m_viewType.Name; } catch { return "null"; } } }
        public string ViewFamilyName { get { try { return m_viewType.ViewFamily.ToString(); } catch { return "null"; } } }
        public string CategoryName { get { return "Views"; } }

        public Dictionary<int, ParamProperties> ViewTypeParameters
        {
            get
            {
                Dictionary<int, ParamProperties> viewTypeParameters = new Dictionary<int, ParamProperties>();
                try
                {
                    foreach (Parameter param in m_viewType.Parameters)
                    {
                        if (param.StorageType == StorageType.ElementId) { continue; } //skip to elementId 
                        InternalDefinition definition = param.Definition as InternalDefinition;
                        if (definition.Visible == false) { continue; }
                        if (param.Definition.Name.Contains(".")) { continue; }
                        ParamProperties pp = new ParamProperties(param);
                        viewTypeParameters.Add(pp.ParamID, pp);
                    }
                    return viewTypeParameters;
                }
                catch
                {
                    return viewTypeParameters;
                }
            }
        }
    }

    /************************************************************************************
     * LockType Enum
     * 0, None  >> writable to both Revit and Database
     * 1, Loack All >>cannot synchronize both direction 
     * 2, Revit Lock >> only managed in Revit
     * 3, Database Lock >> only managed in database
     * 4, Readonly >> lockType option disabled
     * *********************************************************************************/
    public enum LockType { None = 0, LockAll, Editable, ReadOnly,Calculated }

    public class ParamProperties
    {
        private Parameter m_param;

        public ParamProperties(Parameter param)
        {
            m_param=param;
        }

        public ParamProperties()
        {
        }

        public int ParamID { get { return m_param.Id.IntegerValue; } }
        public string ParamName { get { try { return m_param.Definition.Name; } catch { return ""; } } }
        public string ParamGroup{get{try { return m_param.Definition.ParameterGroup.ToString(); }catch { return ""; }}}
        public bool IsReadOnly { get { try { return m_param.IsReadOnly; } catch { return true; } } }
        public bool IsShared { get { try { return m_param.IsShared; } catch { return true; } } }
        public bool IsVisible { get; set; }
        public bool IsEditable { get; set; }
        public bool IsInstance { get; set; }
        //public bool IsRevitOnly { get; set; }
        public bool IsLockAll { get; set; }
        public bool IsProject { get; set; }//set true, if project parameter
        public string CategoryName { get; set; }
        public string FamilyName { get; set; }
        public int FamilyID { get; set; }
        public string ParamType { get { try { return m_param.Definition.ParameterType.ToString(); } catch { return ""; } } }
        public string ParamFormat { get { try { return m_param.StorageType.ToString(); } catch { return ""; } } }
        public string DisplayUnitType { get { try { return m_param.DisplayUnitType.ToString(); } catch { return ""; } } }
        public Parameter Parameter { get { return m_param; } }

        public string ParamValue
         {
             get
             {
                 try { return GetParmaValue(m_param); }
                 catch { return ""; }
             }
             set
             {
                 try { SetparameterValue(m_param, value); }
                 catch { }
             }
         }

        public string DoubleValueAsString
        {
            get
            {
                if (m_param.StorageType == StorageType.Double)
                {
                    try { return m_param.AsValueString(); }
                    catch { return ""; }
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (m_param.StorageType == StorageType.Double)
                {
                   try
                   {
                       m_param.SetValueString(value);
                   }
                   catch{}
                }
            }
        }

        public string GetParmaValue(Parameter parameter)
        {
            switch (parameter.StorageType)
            {
                case StorageType.Double:
                    //return parameter.AsDouble().ToString();
                    return parameter.AsValueString();
                case StorageType.ElementId:
                    return parameter.AsElementId().ToString();
                case StorageType.Integer:
                    return parameter.AsInteger().ToString();
                case StorageType.None:
                    return parameter.AsValueString();
                case StorageType.String:
                    return parameter.AsString();
                default:
                    return "";
            }
        }

        public void SetparameterValue(Parameter parameter, object value)
        {
            if (!parameter.IsReadOnly)
            {
                switch (parameter.StorageType)
                {
                    case StorageType.Double:
                        parameter.SetValueString((string)value);
                        break;
                    case StorageType.ElementId:
                        parameter.Set((ElementId)value);
                        break;
                    case StorageType.Integer:
                        parameter.Set((int)value);
                        break;
                    case StorageType.None:
                        parameter.SetValueString((string)value);
                        break;
                    case StorageType.String:
                        parameter.Set((string)value);
                        break;
                }
            }
        }
    }

    public enum SyncType { Category, Family }

    public class SyncProperties
    {
        public SyncProperties()
        {
        }

        public SyncType SyncType { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int FamilyID { get; set; }
        public string FamilyName { get; set; }
        
    }
}
