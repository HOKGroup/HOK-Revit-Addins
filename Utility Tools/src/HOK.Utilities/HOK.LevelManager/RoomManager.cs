using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;

namespace HOK.LevelManager
{
    public class RoomManager
    {
        private readonly UIApplication m_app;
        private readonly Document m_doc;

        public List<Room> RoomElements { get; set; } = new List<Room>();
        public Dictionary<int, RoomProperties> RoomDictionary { get; set; } = new Dictionary<int, RoomProperties>();
        public List<ElementId> NewRooms { get; set; } = new List<ElementId>();

        public RoomManager(UIApplication application)
        {
            m_app = application;
            m_doc = m_app.ActiveUIDocument.Document;
        }

        public void CollectRooms()
        {
            try
            {
                foreach (var room in RoomElements)
                {
                    var rp = new RoomProperties();
                    rp.RoomObject = room;
                    rp.RoomId = room.Id.IntegerValue;
                    rp.RoomName = room.Name;
                    rp.RoomNumber = room.Number;
                    rp.SourceView = FindViewSource(room);

                    var locationPoint = room.Location as LocationPoint;
                    rp.RoomLocation = locationPoint.Point;

                    var roomSeparations = new List<ModelCurve>();
                    var roomSeparationIds = new List<ElementId>();
                    var opt = new SpatialElementBoundaryOptions();
                    var boundaryListList = room.GetBoundarySegments(opt);
                    if (null != boundaryListList)
                    {
                        if (boundaryListList.Count > 0)
                        {
                            foreach ( var boundaryList in boundaryListList)
                            {
                                foreach (var segment in boundaryList)
                                {
                                    var element = m_doc.GetElement(segment.ElementId);
                                    if (null != element)
                                    {
                                        if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_RoomSeparationLines)
                                        {
                                            var modelCurve = element as ModelCurve;
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
                    RoomDictionary.Add(rp.RoomId, rp);
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
                var roomIds = new List<ElementId>();
                var curveIds = new List<ElementId>();
                foreach (var roomId in RoomDictionary.Keys)
                {
                    var eId = new ElementId(roomId);
                    roomIds.Add(eId);

                    var rp = RoomDictionary[roomId];
                    if (rp.RoomSeparationLines.Count > 0)
                    {
                        foreach (var curve in rp.RoomSeparationLines)
                        {
                            curveIds.Add(curve.Id);
                        }
                    }
                }

                var deletedIds = new List<ElementId>();
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
            var succeed = false;
            try
            {
                var destinationView = FindViewPlan(toLevel);
                var transform = Transform.Identity;
                var opt = new CopyPasteOptions();
                var boundaryIds = new List<ElementId>();

                foreach (var roomId in RoomDictionary.Keys)
                {
                    var rp = RoomDictionary[roomId];
                    var sourceView = rp.SourceView;
                    
                    if(rp.RoomSeparationLinesIds.Count>0)
                    {
                        var boundariesToCopy = new List<ElementId>();
                        foreach (var eId in rp.RoomSeparationLinesIds)
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

                    var elementsToCopy = new List<ElementId>();
                    elementsToCopy.Add(rp.RoomObject.Id);
                    var copiedRooms=ElementTransformUtils.CopyElements(sourceView, elementsToCopy, destinationView, transform, opt).ToList();
                    NewRooms.AddRange(copiedRooms);
                    var element = m_doc.GetElement(copiedRooms.First());
                    if (null != element)
                    {
                        var newRoom = element as Room;
                        newRoom.Number = rp.RoomNumber;
                    }
                }
                if (NewRooms.Count > 0)
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
                var collector = new FilteredElementCollector(m_doc);
                var elements = collector.OfClass(typeof(ViewPlan)).WhereElementIsNotElementType().ToElements().Cast<ViewPlan>().ToList();

                var views = from aview in elements where aview.ViewType == ViewType.FloorPlan select aview;
                foreach (var vp in views)
                {
                    if (null != vp.GenLevel)
                    {
                        var genLevel = vp.GenLevel;
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
                var collector = new FilteredElementCollector(m_doc);
                var viewPlans = collector.OfClass(typeof(ViewPlan)).WhereElementIsNotElementType().ToElements().Cast<ViewPlan>().ToList();
                var floorPlans = from plan in viewPlans where plan.ViewType == ViewType.FloorPlan select plan;
                foreach (var vp in floorPlans)
                {
                    if (null != vp.GenLevel && !vp.IsTemplate)
                    {
                        if (vp.GenLevel.Id == room.LevelId)
                        {
                            var eCollector = new FilteredElementCollector(m_doc, vp.Id);
                            var elementList = eCollector.WhereElementIsNotElementType().ToElementIds().ToList();
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
