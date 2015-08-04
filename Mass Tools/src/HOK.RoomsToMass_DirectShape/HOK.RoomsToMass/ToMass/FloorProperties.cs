using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace HOK.RoomsToMass.ToMass
{
    public class FloorProperties
    {
        private Floor m_floor = null;
        private string floorDocument = "";
        private ElementId floorElementId = ElementId.InvalidElementId;
        private int floorId = -1;
        private string floorUniqueId = "";
        private string floorName = "";
        private string floorTypeName = "";
        private double floorThickness = 0;
        private double userHeight = 10;
        private ElementId levelId = ElementId.InvalidElementId;
        private string levelName = "";
        private ElementId phaseId = ElementId.InvalidElementId;
        private string phaseName = "";
        private string designOption = "";
        private Solid floorSolid = null;
        private XYZ floorSolidCentroid = null;
        private IList<CurveLoop> floorProfiles = new List<CurveLoop>();

        private MassProperties linkedMass = null;
        private bool linked = false;
        private bool modifiedHost = false;

        private bool isSelected = false;
        private string toolTip = "The linked mass does not exist.";


        public Floor FloorElement { get { return m_floor; } set { m_floor = value; } }
        public string FloorDocument { get { return floorDocument; } set { floorDocument = value; } }
        public ElementId FloorElementId { get { return floorElementId; } set { floorElementId = value; } }
        public int FloorId { get { return floorId; } set { floorId = value; } }
        public string FloorUniqueId { get { return floorUniqueId; } set { floorUniqueId = value; } }
        public string FloorName { get { return floorName; } set { floorName = value; } }
        public string FloorTypeName { get { return floorTypeName; } set { floorTypeName = value; } }
        public double FloorThickness { get { return floorThickness; } set { floorThickness = value; } }
        public double UserHeight { get { return userHeight; } set { userHeight = value; } }
        public ElementId LevelId { get { return levelId; } set { levelId = value; } }
        public string LevelName { get { return levelName; } set { levelName = value; } }
        public ElementId PhaseId { get { return phaseId; } set { phaseId = value; } }
        public string PhaseName { get { return phaseName; } set { phaseName = value; } }
        public string FloorDesignOption { get { return designOption; } set { designOption = value; } }
        public Solid FloorSolid { get { return floorSolid; } set { floorSolid = value; } }
        public XYZ FloorSolidCentroid { get { return floorSolidCentroid; } set { floorSolidCentroid = value; } }
        public IList<CurveLoop> FloorProfiles { get { return floorProfiles; } set { floorProfiles = value; } }
        public MassProperties LinkedMass { get { return linkedMass; } set { linkedMass = value; } }
        public bool Linked { get { return linked; } set { linked = value; } }
        public bool ModifiedHost { get { return modifiedHost; } set { modifiedHost = value; } }

        public bool IsSelected { get { return isSelected; } set { isSelected = value; } }
        public string ToolTip { get { return toolTip; } set { toolTip = value; } }

        public FloorProperties(Floor floor)
        {
            m_floor = floor;
            floorDocument = m_floor.Document.Title;

            GetFloorInfo();
        }

        public FloorProperties(FloorProperties fp)
        {
            this.FloorElement = fp.FloorElement;
            this.FloorDocument = fp.FloorDocument;
            this.FloorElementId = fp.FloorElementId;
            this.FloorId = fp.FloorId;
            this.FloorUniqueId = fp.FloorUniqueId;
            this.FloorName = fp.FloorName;
            this.FloorTypeName = fp.FloorTypeName;
            this.UserHeight = fp.UserHeight;
            this.LevelId = fp.LevelId;
            this.LevelName = fp.LevelName;
            this.PhaseId = fp.PhaseId;
            this.PhaseName = fp.PhaseName;
            this.FloorDesignOption = fp.FloorDesignOption;
            this.FloorSolid = fp.FloorSolid;
            this.FloorSolidCentroid = fp.FloorSolidCentroid;
            this.FloorProfiles = fp.FloorProfiles;
            this.LinkedMass = fp.LinkedMass;
            this.Linked = fp.Linked;
            this.ModifiedHost = fp.ModifiedHost;
            this.IsSelected = fp.IsSelected;
            this.ToolTip = fp.ToolTip;
        }

        private void GetFloorInfo()
        {
            try
            {
                floorElementId = m_floor.Id;
                floorId = m_floor.Id.IntegerValue;
                floorUniqueId = m_floor.UniqueId;
                floorName = m_floor.Name;
                floorTypeName = m_floor.FloorType.Name;

                Parameter thicknessParam = m_floor.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM);
                if (null != thicknessParam)
                {
                    floorThickness = thicknessParam.AsDouble();
                }

                levelId = m_floor.LevelId;
                Level level = m_floor.Document.GetElement(levelId) as Level;
                if (null != level)
                {
                    levelName = level.Name;
                }

                if (null != m_floor.DesignOption)
                {
                    designOption = m_floor.DesignOption.Name;
                }

                Parameter phaseParam = m_floor.get_Parameter(BuiltInParameter.PHASE_CREATED);
                if (null != phaseParam)
                {
                    phaseId = phaseParam.AsElementId();
                    phaseName = phaseParam.AsValueString();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void GetFloorGeometry(Transform linkTransform)
        {
            try
            {
                Options opt = m_floor.Document.Application.Create.NewGeometryOptions();
                GeometryElement geomElem = m_floor.get_Geometry(opt);
                if (null != geomElem)
                {
                    geomElem = geomElem.GetTransformed(linkTransform);
                    foreach (GeometryObject geomObj in geomElem)
                    {
                        Solid solid = geomObj as Solid;
                        if (null != solid)
                        {
                            if (solid.Volume > 0)
                            {
                                floorSolid = solid; break;
                            }
                        }
                    }
                }
                if (null != floorSolid)
                {
                    floorSolidCentroid = floorSolid.ComputeCentroid();

                    Face topFace = null;
                    foreach (Face face in floorSolid.Faces)
                    {
                        XYZ normal = face.ComputeNormal(new UV(0, 0));
                        if (normal.Z > 0 && normal.X <normal.Z && normal.Y<normal.Z)
                        {
                            topFace = face; break;
                        }
                    }
                    if (null != topFace)
                    {
                        floorProfiles = topFace.GetEdgesAsCurveLoops();
                    }

                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    }
}
