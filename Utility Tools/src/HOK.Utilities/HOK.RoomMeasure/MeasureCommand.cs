using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace HOK.RoomMeasure
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class MeasureCommand:IExternalCommand
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;

        public const string roomWidthParamName = "Room Width";
        public const string roomLengthParamName = "Room Length";


        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            m_app = commandData.Application;
            m_doc = m_app.ActiveUIDocument.Document;
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<Room> rooms = collector.OfCategory(BuiltInCategory.OST_Rooms).ToElements().Cast<Room>().ToList();
                if (rooms.Count > 0)
                {
                    Room sampleRoom = rooms.First();
                    if (MeasureUtil.ExistRoomParameter(sampleRoom))
                    {
                        using (TransactionGroup tg = new TransactionGroup(m_doc))
                        {
                            tg.Start("Calculate Rooms");
                            try
                            {
                                int roomCount = 0;
                                foreach (Room room in rooms)
                                {
                                    if (room.Area == 0) { continue; }
                                    using (Transaction trans = new Transaction(m_doc))
                                    {
                                        double width = 0;
                                        double length = 0;

                                        ElementId directShapeId = ElementId.InvalidElementId;
                                        bool perpendicularSwitch = false;
                                        trans.Start("Create Room DirecShape");
                                        try
                                        {
                                            DirectShape directShape = MeasureUtil.CreateRoomDirectShape(room);
                                            if (null != directShape)
                                            {
                                                directShapeId = directShape.Id;
                                                MeasureUtil.RotateDirectShape(directShape, room, out perpendicularSwitch);
                                            }
                                            trans.Commit();
                                        }
                                        catch (Exception ex)
                                        {
                                            trans.RollBack();
                                            string exMsg = ex.Message;
                                        }

                                        trans.Start("Calculate Dimension");
                                        try
                                        {
                                            DirectShape directShape = m_doc.GetElement(directShapeId) as DirectShape;
                                            if (null != directShape)
                                            {
                                                MeasureUtil.CalculateWidthAndLength(directShape, out width, out length);
                                            }
                                            m_doc.Delete(directShapeId);
                                            trans.Commit();
                                        }
                                        catch (Exception ex)
                                        {
                                            trans.RollBack();
                                            string exMsg = ex.Message;
                                        }

                                        trans.Start("Set Parameter");
                                        try
                                        {
                                            if (perpendicularSwitch)
                                            {
                                                double tempVal = width;
                                                width = length;
                                                length = tempVal;
                                            }
                                            Parameter param = room.LookupParameter(roomWidthParamName);
                                            if (null != param)
                                            {
                                                param.Set(width);
                                            }
                                            param = room.LookupParameter(roomLengthParamName);
                                            if (null != param)
                                            {
                                                param.Set(length);
                                            }


                                            trans.Commit();
                                        }
                                        catch (Exception ex)
                                        {
                                            trans.RollBack();
                                            string exMsg = ex.Message;
                                        }
                                        roomCount++;
                                    }
                                }
                                tg.Assimilate();
                                MessageBox.Show("Parameters [Room Width] and [Room Length] have been updated in " + roomCount + " rooms.", "Successfully Completed!", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                tg.RollBack();
                                MessageBox.Show("Failed to measure the width and length of rooms.\n" + ex.Message, "Measuring Rooms", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
  
                }
                else
                {
                    MessageBox.Show("Room doesn't exist in the current project.", "Room Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to measure the width and length of rooms.\n"+ex.Message, "Measuring Rooms", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return Result.Succeeded;
        }


    }
}
