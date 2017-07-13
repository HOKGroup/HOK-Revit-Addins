using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Linq;
using System.Windows;
using Autodesk.Revit.Attributes;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;


namespace HOK.RoomMeasure
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class MeasureCommand : IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public const string roomWidthParamName = "Room Width";
        public const string roomLengthParamName = "Room Length";


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_app = commandData.Application;
            m_doc = m_app.ActiveUIDocument.Document;
            Log.AppendLog(LogMessageType.INFO, "Started");

            // (Konrad) We are gathering information about the addin use. This allows us to
            // better maintain the most used plug-ins or discontiue the unused ones.
            AddinUtilities.PublishAddinLog(new AddinLog("Utilities-RoomMeasure", m_doc));

            try
            {
                var collector = new FilteredElementCollector(m_doc);
                var rooms = collector.OfCategory(BuiltInCategory.OST_Rooms).ToElements().Cast<Room>().ToList();
                if (rooms.Count > 0)
                {
                    var sampleRoom = rooms.First();
                    if (MeasureUtil.ExistRoomParameter(sampleRoom))
                    {
                        using (var tg = new TransactionGroup(m_doc))
                        {
                            tg.Start("Calculate Rooms");
                            try
                            {
                                var roomCount = 0;
                                foreach (var room in rooms)
                                {
                                    if (room.Area == 0) { continue; }
                                    using (var trans = new Transaction(m_doc))
                                    {
                                        double width = 0;
                                        double length = 0;

                                        var directShapeId = ElementId.InvalidElementId;
                                        var perpendicularSwitch = false;
                                        trans.Start("Create Room DirecShape");
                                        try
                                        {
                                            var directShape = MeasureUtil.CreateRoomDirectShape(room);
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
                                            var exMsg = ex.Message;
                                        }

                                        trans.Start("Calculate Dimension");
                                        try
                                        {
                                            var directShape = m_doc.GetElement(directShapeId) as DirectShape;
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
                                            var exMsg = ex.Message;
                                        }

                                        trans.Start("Set Parameter");
                                        try
                                        {
                                            if (perpendicularSwitch)
                                            {
                                                var tempVal = width;
                                                width = length;
                                                length = tempVal;
                                            }
                                            var param = room.LookupParameter(roomWidthParamName);
                                            param?.Set(width);
                                            param = room.LookupParameter(roomLengthParamName);
                                            param?.Set(length);


                                            trans.Commit();
                                        }
                                        catch (Exception ex)
                                        {
                                            trans.RollBack();
                                            var exMsg = ex.Message;
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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }

            Log.AppendLog(LogMessageType.INFO, "Ended");
            return Result.Succeeded;
        }


    }
}
