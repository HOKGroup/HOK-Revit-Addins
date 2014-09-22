using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;

namespace HOK.Utilities.LevelManager
{
#if RELEASE2013
#else
    public class RoomManager
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;
        private List<Room> roomElements = new List<Room>();
        private List<ElementId> newRooms = new List<ElementId>();
        private Dictionary<int/*roomId*/, RoomProperties> roomDictionary = new Dictionary<int, RoomProperties>();

        public List<Room> RoomElements { get { return roomElements; } set { roomElements = value; } }
        public Dictionary<int, RoomProperties> RoomDictionary { get { return roomDictionary; } set { roomDictionary = value; } }
        public List<ElementId> NewRooms { get { return newRooms; } set { newRooms = value; } }

        public RoomManager(UIApplication application)
        {
            m_app = application;
            m_doc = m_app.ActiveUIDocument.Document;
        }

        public void CollectRooms()
        {
            try
            {
                foreach (Room room in roomElements)
                {
                    RoomProperties rp = new RoomProperties();
                    rp.RoomObject = room;
                    rp.RoomId = room.Id.IntegerValue;
                    rp.RoomName = room.Name;
                    rp.RoomNumber = room.Number;
                    rp.SourceView = FindViewSource(room);

                    LocationPoint locationPoint = room.Location as LocationPoint;
                    rp.RoomLocation = locationPoint.Point;

                    List<ModelCurve> roomSeparations = new List<ModelCurve>();
                    List<ElementId> roomSeparationIds = new List<ElementId>();
                    SpatialElementBoundaryOptions opt = new SpatialElementBoundaryOptions();
                    IList<IList<Autodesk.Revit.DB.BoundarySegment>> boundaryListList = room.GetBoundarySegments(opt);
                    if (null != boundaryListList)
                    {
                        if (boundaryListList.Count > 0)
                        {
                            foreach ( IList<Autodesk.Revit.DB.BoundarySegment> boundaryList in boundaryListList)
                            {
                                foreach (Autodesk.Revit.DB.BoundarySegment segment in boundaryList)
                                {
                                    Element element = segment.Element;
                                    if (null != element)
                                    {
                                        if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_RoomSeparationLines)
                                        {
                                            ModelCurve modelCurve = element as ModelCurve;
                                            roomSeparations.Add(modelCurve);
                                            roomSeparationIds.Add(modelCurve.Id);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    rp.RoomSeparationLines = roomSeparations;
                    rp.RoomSeparationLinesIds = roomSeparationIds;
                    roomDictionary.Add(rp.RoomId, rp);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect rooms.\n"+ex.Message, "Collect Rooms", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void DeleteRooms()
        {
            try
            {
                List<ElementId> roomIds = new List<ElementId>();
                List<ElementId> curveIds = new List<ElementId>();
                foreach (int roomId in roomDictionary.Keys)
                {
                    ElementId eId = new ElementId(roomId);
                    roomIds.Add(eId);

                    RoomProperties rp = roomDictionary[roomId];
                    if (rp.RoomSeparationLines.Count > 0)
                    {
                        foreach (ModelCurve curve in rp.RoomSeparationLines)
                        {
                            curveIds.Add(curve.Id);
                        }
                    }
                }

                List<ElementId> deletedIds = new List<ElementId>();
                if (roomIds.Count > 0)
                {
                    deletedIds = m_doc.Delete(roomIds).ToList();
                }
                if (curveIds.Count > 0)
                {
                    deletedIds.AddRange(m_doc.Delete(curveIds).ToList());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete rooms.\n"+ex.Message, "Delete Rooms", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public bool CopyRooms(Level toLevel)
        {
            bool succeed = false;
            try
            {
                ViewPlan destinationView = FindViewPlan(toLevel);
                Transform transform = Transform.Identity;
                CopyPasteOptions opt = new CopyPasteOptions();
                List<ElementId> boundaryIds = new List<ElementId>();

                foreach (int roomId in roomDictionary.Keys)
                {
                    RoomProperties rp = roomDictionary[roomId];
                    ViewPlan sourceView = rp.SourceView;
                    
                    if(rp.RoomSeparationLinesIds.Count>0)
                    {
                        List<ElementId> boundariesToCopy = new List<ElementId>();
                        foreach (ElementId eId in rp.RoomSeparationLinesIds)
                        {
                            if (!boundaryIds.Contains(eId))
                            {
                                boundariesToCopy.Add(eId);
                                boundaryIds.Add(eId);
                            }
                        }
                        if (boundariesToCopy.Count > 0)
                        {
                            IList<ElementId> copiedBoundaries = ElementTransformUtils.CopyElements(sourceView, boundariesToCopy, destinationView, transform, opt).ToList();
                        }
                    }

                    List<ElementId> elementsToCopy = new List<ElementId>();
                    elementsToCopy.Add(rp.RoomObject.Id);
                    List<ElementId> copiedRooms=ElementTransformUtils.CopyElements(sourceView, elementsToCopy, destinationView, transform, opt).ToList();
                    newRooms.AddRange(copiedRooms);
                    Element element = m_doc.GetElement(copiedRooms.First());
                    if (null != element)
                    {
                        Room newRoom = element as Room;
                        newRoom.Number = rp.RoomNumber;
                    }
                }
                if (newRooms.Count > 0)
                {
                    succeed = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to recreate rooms.\n"+ex.Message, "Recreate Rooms", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                succeed = false;
            }
            return succeed;
        }

        private ViewPlan FindViewPlan(Level toLevel)
        {
            ViewPlan viewPlan = null;
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<ViewPlan> elements = collector.OfClass(typeof(ViewPlan)).WhereElementIsNotElementType().ToElements().Cast<ViewPlan>().ToList();

                var views = from aview in elements where aview.ViewType == ViewType.FloorPlan select aview;
                foreach (ViewPlan vp in views)
                {
                    if (null != vp.GenLevel)
                    {
                        Level genLevel = vp.GenLevel;
                        if (genLevel.Id == toLevel.Id)
                        {
                            viewPlan = vp;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find a view Plan.\n"+ex.Message, "Find View Plan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return viewPlan;
        }

        private ViewPlan FindViewSource(Room room)
        {
            ViewPlan viewPlan = null;
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<ViewPlan> viewPlans = collector.OfClass(typeof(ViewPlan)).WhereElementIsNotElementType().ToElements().Cast<ViewPlan>().ToList();
                var floorPlans = from plan in viewPlans where plan.ViewType == ViewType.FloorPlan select plan;
                foreach (ViewPlan vp in floorPlans)
                {
                    if (null != vp.GenLevel && !vp.IsTemplate)
                    {
                        if (vp.GenLevel.Id == room.LevelId)
                        {
                            FilteredElementCollector eCollector = new FilteredElementCollector(m_doc, vp.Id);
                            List<ElementId> elementList = eCollector.WhereElementIsNotElementType().ToElementIds().ToList();
                            if (elementList.Contains(room.Id))
                            {
                                viewPlan = vp;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find the view source.\n" + ex.Message, "Find View Source", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return viewPlan;
        }
    }
#endif

    public class RoomProperties
    {
        public Room RoomObject { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public string RoomNumber { get; set; }
        public ViewPlan SourceView { get; set; }
        public XYZ RoomLocation { get; set; }
        public List<ModelCurve> RoomSeparationLines { get; set; }
        public List<ElementId> RoomSeparationLinesIds { get; set; }
    }
}
