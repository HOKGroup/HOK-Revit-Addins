using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace HOK.RoomsToMass.ToMass
{

    public enum SourceType
    {
        Rooms, Areas, Floors, None
    }
    /// <summary>
    /// Interaction logic for MassSourceWindow.xaml
    /// </summary>
    public partial class MassSourceWindow : Window
    {
        private UIApplication m_app = null;
        private Document m_doc = null;
        private Dictionary<string, RevitDocumentProperties> modelDictionary = new Dictionary<string,RevitDocumentProperties>();
        private SourceType selectedSource = SourceType.None;
        private List<MassProperties> massList = new List<MassProperties>();
        private Dictionary<string, RoomProperties> roomDictionary = new Dictionary<string, RoomProperties>();
        private Dictionary<string, AreaProperties> areaDictionary = new Dictionary<string, AreaProperties>();
        private Dictionary<string, FloorProperties> floorDictionary = new Dictionary<string, FloorProperties>();

        public SourceType SelectedSourceType { get { return selectedSource; } set { selectedSource = value; } }
        public Dictionary<string, RoomProperties> RoomDictionary { get { return roomDictionary; } set { roomDictionary = value; } }
        public Dictionary<string, AreaProperties> AreaDictionary { get { return areaDictionary; } set { areaDictionary = value; } }
        public Dictionary<string, FloorProperties> FloorDictionary { get { return floorDictionary; } set { floorDictionary = value; } }

        public MassSourceWindow(UIApplication uiapp, Dictionary<string, RevitDocumentProperties> models)
        {
            m_app = uiapp;
            m_doc = uiapp.ActiveUIDocument.Document;
            modelDictionary = models;
            InitializeComponent();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CollectMassInfo(SourceType sourceType)
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<Element> elements = collector.OfCategory(BuiltInCategory.OST_Mass).OfClass(typeof(DirectShape)).ToElements().ToList();
                if (elements.Count > 0)
                {
                    string hostCategory = "";
                    string hostUniqueId = "";
                    XYZ hostCentroid = null;
                    double massHeight = 0;
                    foreach (Element element in elements)
                    {
                        if (MassDataStorageUtil.GetLinkedHostInfo(element, out hostCategory, out hostUniqueId, out hostCentroid, out massHeight))
                        {
                            if (hostCategory == sourceType.ToString())
                            {
                                MassProperties mp = new MassProperties(element);
                                mp.SetHostInfo(hostUniqueId, sourceType, hostCentroid, massHeight);
                                massList.Add(mp);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect masses information.\n" + ex.Message, "Collect Mass Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonRooms_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                selectedSource = SourceType.Rooms;
                CollectMassInfo(selectedSource);

                List<Room> sourceRooms = new List<Room>();
                if ((bool)checkBoxSelected.IsChecked)
                {
                    UIDocument uidoc = m_app.ActiveUIDocument;
                    Selection selection = uidoc.Selection;
                    ICollection<ElementId> selectedIds = selection.GetElementIds();
                    if (selectedIds.Count > 0)
                    {
                        FilteredElementCollector collector = new FilteredElementCollector(m_doc, selection.GetElementIds());
                        ElementClassFilter classFilter = new ElementClassFilter(typeof(DirectShape), true);
                        sourceRooms = collector.OfCategory(BuiltInCategory.OST_Rooms).WherePasses(classFilter).ToElements().Cast<Room>().ToList();
                        if (sourceRooms.Count > 0)
                        {
                            CollectRoomsInfo(sourceRooms, Autodesk.Revit.DB.Transform.Identity);
                        }
                    }
                }
                else
                {
                    if (modelDictionary.Count > 0)
                    {
                        foreach (RevitDocumentProperties rdp in modelDictionary.Values)
                        {
                            FilteredElementCollector collector = new FilteredElementCollector(rdp.DocumentObj);
                            ElementClassFilter classFilter = new ElementClassFilter(typeof(DirectShape), true);
                            List<Room> rooms = collector.OfCategory(BuiltInCategory.OST_Rooms).WherePasses(classFilter).ToElements().Cast<Room>().ToList();

                            if (rooms.Count > 0)
                            {
                                if (rdp.IsLinked)
                                {
                                    foreach (int linkId in rdp.LinkedInstances.Keys)
                                    {
                                        LinkedInstanceProperties lip = rdp.LinkedInstances[linkId];
                                        CollectRoomsInfo(rooms, lip.TransformValue);
                                    }
                                }
                                else
                                {
                                    CollectRoomsInfo(rooms, Autodesk.Revit.DB.Transform.Identity);
                                }
                            }
                        }
                    }
                }

                if (roomDictionary.Count > 0)
                {
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Rooms don't exist in the current Revit project.", "Empty Rooms", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to gather information of rooms.\n" + ex.Message, "Room Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CollectRoomsInfo(List<Room> roomList, Autodesk.Revit.DB.Transform transform)
        {
            try
            {
                MassConfiguration massConfig = MassConfigDataStorageUtil.GetMassConfiguration(m_doc, SourceType.Rooms);
                SpatialElementBoundaryLocation boundaryLocation = massConfig.RoomBoundaryAtCenterLine ? SpatialElementBoundaryLocation.Center : SpatialElementBoundaryLocation.Finish;

                SpatialElementBoundaryOptions spatialOpts = new SpatialElementBoundaryOptions();
                spatialOpts.SpatialElementBoundaryLocation = boundaryLocation;

                SpatialElementGeometryCalculator calculator = new SpatialElementGeometryCalculator(m_doc, spatialOpts);

                foreach (Room room in roomList)
                {
                    if (room.Area == 0) { continue; }
   
                    RoomProperties rp = new RoomProperties(room);
                    rp.RoomTransform = transform;
                    rp.GetRoomGeometry(transform, calculator);

                    StringBuilder strBuilder = new StringBuilder();
                    var mass3dFound = from mass in massList 
                                    where mass.HostType == SourceType.Rooms && mass.HostUniqueId == rp.RoomUniqueId && mass.MassElementType == MassType.MASS3D 
                                    select mass;
                    if (mass3dFound.Count() > 0)
                    {
                        MassProperties mp = mass3dFound.First();
                        rp.Linked3dMass = mp;
                        rp.Linked3d = true;
                        rp.UserHeight = mp.MassHeight;
                        strBuilder.AppendLine("Mass 3D Id: " + mp.MassId);
                        if (null != mp.MassSolid)
                        {
                            if (!rp.RoomSolidCentroid.IsAlmostEqualTo(mp.HostSolidCentroid))
                            {
                                rp.ModifiedHost = true;
                                strBuilder.Append(" (the room has been modified)");
                            }
                        }
                    }

                    var mass2dFound = from mass in massList
                                      where mass.HostType == SourceType.Rooms && mass.HostUniqueId == rp.RoomUniqueId && mass.MassElementType == MassType.MASS2D
                                      select mass;
                    if (mass2dFound.Count() > 0)
                    {
                        MassProperties mp = mass2dFound.First();
                        rp.Linked2dMass = mp;
                        rp.Linked2d = true;
                        strBuilder.AppendLine("Mass 2D Id: " + mp.MassId);
                        if (null != mp.MassSolid)
                        {
                            if (!rp.RoomSolidCentroid.IsAlmostEqualTo(mp.HostSolidCentroid))
                            {
                                rp.ModifiedHost = true;
                                strBuilder.Append(" (the room has been modified)");
                            }
                        }
                    }

                    if (strBuilder.Length > 0)
                    {
                        rp.ToolTip = strBuilder.ToString();
                    }

                    if (!roomDictionary.ContainsKey(rp.RoomUniqueId))
                    {
                        roomDictionary.Add(rp.RoomUniqueId, rp);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect rooms Information.\n"+ex.Message, "Collect Rooms Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonAreas_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                selectedSource = SourceType.Areas;
                CollectMassInfo(selectedSource);

                List<Area> sourceAreas = new List<Area>();
                if ((bool)checkBoxSelected.IsChecked)
                {
                    UIDocument uidoc = m_app.ActiveUIDocument;
                    Selection selection = uidoc.Selection;
                    ICollection<ElementId> selectedIds = selection.GetElementIds();
                    if (selectedIds.Count > 0)
                    {
                        FilteredElementCollector collector = new FilteredElementCollector(m_doc, selection.GetElementIds());
                        sourceAreas = collector.OfCategory(BuiltInCategory.OST_Areas).Cast<Area>().ToList();
                        if (sourceAreas.Count > 0)
                        {
                            CollectAreaInfo(sourceAreas); // host model only
                        }
                    }
                }
                else
                {
                    if (modelDictionary.Count > 0)
                    {
                        foreach (RevitDocumentProperties rdp in modelDictionary.Values)
                        {
                            if (rdp.IsLinked) { continue; } //host model only
                            
                            FilteredElementCollector collector = new FilteredElementCollector(rdp.DocumentObj);
                            List<Area> areas = collector.OfCategory(BuiltInCategory.OST_Areas).Cast<Area>().ToList();

                            if (areas.Count > 0)
                            {
                                CollectAreaInfo(areas);
                            }
                        }
                    }
                }

                if (areaDictionary.Count > 0)
                {
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Area don't exist in the current Revit project.", "Empty Area", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to gather information of areas.\n" + ex.Message, "Area Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CollectAreaInfo(List<Area> areaList)
        {
            try
            {
                foreach (Area area in areaList)
                {
                    if (area.Area == 0) { continue; }

                    AreaProperties ap = new AreaProperties(area);
                    ap.GetAreaProfile();

                    StringBuilder strBuilder = new StringBuilder();
                    var mass3dFound = from mass in massList
                                      where mass.HostType == SourceType.Areas && mass.HostUniqueId == ap.AreaUniqueId && mass.MassElementType == MassType.MASS3D
                                      select mass;
                    if (mass3dFound.Count() > 0)
                    {
                        MassProperties mp = mass3dFound.First();
                        ap.Linked3dMass = mp;
                        ap.Linked3d = true;
                        ap.UserHeight = mp.MassHeight;
                        strBuilder.AppendLine("Mass 3D Id: " + mp.MassId);
                        if (null != mp.MassSolid)
                        {
                            if (!ap.AreaCenterPoint.IsAlmostEqualTo(mp.HostSolidCentroid))
                            {
                                ap.ModifiedHost = true;
                                strBuilder.Append(" (the area has been modified)");
                            }
                        }
                    }

                    var mass2dFound = from mass in massList
                                      where mass.HostType == SourceType.Areas && mass.HostUniqueId == ap.AreaUniqueId && mass.MassElementType == MassType.MASS2D
                                      select mass;
                    if (mass2dFound.Count() > 0)
                    {
                        MassProperties mp = mass2dFound.First();
                        ap.Linked2dMass = mp;
                        ap.Linked2d = true;
                        strBuilder.AppendLine("Mass 2D Id: " + mp.MassId);
                        if (null != mp.MassSolid)
                        {
                            if (!ap.AreaCenterPoint.IsAlmostEqualTo(mp.HostSolidCentroid))
                            {
                                ap.ModifiedHost = true;
                                strBuilder.Append(" (the area has been modified)");
                            }
                        }
                    }

                    if (strBuilder.Length > 0)
                    {
                        ap.ToolTip = strBuilder.ToString();
                    }

                    if (!areaDictionary.ContainsKey(ap.AreaUniqueId))
                    {
                        areaDictionary.Add(ap.AreaUniqueId, ap);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect area Information.\n" + ex.Message, "Collect Areas Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonFloors_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                selectedSource = SourceType.Floors;
                CollectMassInfo(selectedSource);

                List<Floor> sourceFloors = new List<Floor>();
                if ((bool)checkBoxSelected.IsChecked)
                {
                    UIDocument uidoc = m_app.ActiveUIDocument;
                    Selection selection = uidoc.Selection;
                    ICollection<ElementId> selectedIds = selection.GetElementIds();
                    if (selectedIds.Count > 0)
                    {
                        FilteredElementCollector collector = new FilteredElementCollector(m_doc, selection.GetElementIds());
                        sourceFloors = collector.OfCategory(BuiltInCategory.OST_Floors).WhereElementIsNotElementType().Cast<Floor>().ToList();
                        if (sourceFloors.Count > 0)
                        {
                            CollectFloorsInfo(sourceFloors, Autodesk.Revit.DB.Transform.Identity); // host model only
                        }
                    }
                }
                else
                {
                    if (modelDictionary.Count > 0)
                    {
                        foreach (RevitDocumentProperties rdp in modelDictionary.Values)
                        {
                            FilteredElementCollector collector = new FilteredElementCollector(rdp.DocumentObj);
                            List<Floor> floors = collector.OfCategory(BuiltInCategory.OST_Floors).WhereElementIsNotElementType().Cast<Floor>().ToList();

                            if (floors.Count > 0)
                            {
                                if (rdp.IsLinked)
                                {
                                    foreach (int linkId in rdp.LinkedInstances.Keys)
                                    {
                                        LinkedInstanceProperties lip = rdp.LinkedInstances[linkId];
                                        CollectFloorsInfo(floors, lip.TransformValue);
                                    }
                                }
                                else
                                {
                                    CollectFloorsInfo(floors, Autodesk.Revit.DB.Transform.Identity);
                                }
                            }
                        }
                    }
                }

                if (floorDictionary.Count > 0)
                {
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Floors don't exist in the current Revit project.", "Empty Floor", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to gather information of floors.\n" + ex.Message, "Floors Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CollectFloorsInfo(List<Floor> floorList, Autodesk.Revit.DB.Transform floorTransform)
        {
            try
            {
                foreach (Floor floor in floorList)
                {
                    FloorProperties fp = new FloorProperties(floor);
                    fp.GetFloorGeometry(floorTransform);

                    StringBuilder strBuilder = new StringBuilder();
                    var mass3dFound = from mass in massList
                                      where mass.HostType == SourceType.Floors && mass.HostUniqueId == fp.FloorUniqueId && mass.MassElementType == MassType.MASS3D
                                      select mass;
                    if (mass3dFound.Count() > 0)
                    {
                        MassProperties mp = mass3dFound.First();
                        fp.Linked3dMass = mp;
                        fp.Linked3d = true;
                        fp.UserHeight = mp.MassHeight;
                        strBuilder.AppendLine("Mass 3D Id: " + mp.MassId);
                        if (null != mp.MassSolid)
                        {
                            if (!fp.FloorSolidCentroid.IsAlmostEqualTo(mp.HostSolidCentroid))
                            {
                                fp.ModifiedHost = true;
                                strBuilder.Append(" (the floor has been modified)");
                            }
                        }
                    }

                    var mass2dFound = from mass in massList
                                      where mass.HostType == SourceType.Floors && mass.HostUniqueId == fp.FloorUniqueId && mass.MassElementType == MassType.MASS2D
                                      select mass;
                    if (mass2dFound.Count() > 0)
                    {
                        MassProperties mp = mass2dFound.First();
                        fp.Linked2dMass = mp;
                        fp.Linked2d = true;
                        strBuilder.AppendLine("Mass 2D Id: " + mp.MassId);
                        if (null != mp.MassSolid)
                        {
                            if (!fp.FloorSolidCentroid.IsAlmostEqualTo(mp.HostSolidCentroid))
                            {
                                fp.ModifiedHost = true;
                                strBuilder.Append(" (the floor has been modified)");
                            }
                        }
                    }

                    if (strBuilder.Length > 0)
                    {
                        fp.ToolTip = strBuilder.ToString();
                    }

                    if (!floorDictionary.ContainsKey(fp.FloorUniqueId))
                    {
                        floorDictionary.Add(fp.FloorUniqueId, fp);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect floors Information.\n" + ex.Message, "Collect Floors Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
    }
}
