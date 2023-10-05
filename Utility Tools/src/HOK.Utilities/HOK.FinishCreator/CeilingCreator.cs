using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using static HOK.Core.Utilities.ElementIdExtension;

namespace HOK.FinishCreator
{
    public class CeilingCreator
    {
        private readonly UIApplication App;
        private readonly Document Doc;
        private readonly List<Room> SelectedRooms = new List<Room>();
        private List<LinkedRoomProperties> selectedLinkedRooms = new List<LinkedRoomProperties>();
        private readonly List<CeilingType> CeilingTypes;
        private Dictionary<long/*roomId*/, List<Ceiling>> createdCeilings = new Dictionary<long, List<Ceiling>>();

        public Dictionary<long, List<Ceiling>> CreatedCeilings { get { return createdCeilings; } set { createdCeilings = value; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="application"></param>
        /// <param name="rooms"></param>
        public CeilingCreator(UIApplication application, List<Room> rooms)
        {
            App = application;
            Doc = App.ActiveUIDocument.Document;
            SelectedRooms = rooms;

            CeilingTypes = new FilteredElementCollector(Doc)
                .OfClass(typeof(CeilingType))
                .Cast<CeilingType>()
                .ToList();
        }


        public CeilingCreator(UIApplication application, List<LinkedRoomProperties> rooms)
        {
            App = application;
            Doc = App.ActiveUIDocument.Document;

            selectedLinkedRooms = rooms;

            var collector = new FilteredElementCollector(Doc);
            CeilingTypes = collector.OfClass(typeof(CeilingType)).ToElements().Cast<CeilingType>().ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool CreateCeilingFromRoom()
        {
            var created = false;
            using (var tg = new TransactionGroup(Doc))
            {
                tg.Start("Create Ceilings");
                var failureMessages = new StringBuilder();
                try
                {
                    foreach (var room in SelectedRooms)
                    {
                        var ceilingsFound = new List<Ceiling>();
                        CeilingType ceilingType = null;

                        using (var trans = new Transaction(Doc))
                        {
                            trans.Start("Create a Ceiling");
                            try
                            {
                                ceilingsFound = FindCeilings(room, Transform.Identity);
                                if (ceilingsFound.Count > 0)
                                {
                                    ceilingType = FindCeilingType(room);
                                }
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                failureMessages.AppendLine(room.Name + ": " + ex.Message);
                            }
                        }

                        if (ceilingsFound.Count > 0 && null != ceilingType)
                        {
                            var copiedCeilings = CreateCeilings(room, ceilingsFound, ceilingType, ref failureMessages);
                            if (copiedCeilings.Count > 0 && !createdCeilings.ContainsKey(GetElementIdValue(room.Id)))
                            {
                                createdCeilings.Add(GetElementIdValue(room.Id), copiedCeilings);
                                created = true;
                            }
                        }
                    }
                    tg.Assimilate();
                }
                catch (Exception ex)
                {
                    tg.RollBack();
                    MessageBox.Show("Cannot create ceilings from the selected rooms.\n" + ex.Message, "Create Ceilings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            return created;
        }

        public bool CreateCeilingFromLink()
        {
            var created = false;
            using (var tg = new TransactionGroup(Doc))
            {
                tg.Start("Create Ceilings");
                var failureMessages = new StringBuilder();
                try
                {
                    foreach (var lrp in selectedLinkedRooms)
                    {
                        var room = lrp.LinkedRoom;
                        var ceilingsFound = new List<Ceiling>();
                        CeilingType ceilingType = null;

                        using (var trans = new Transaction(Doc))
                        {
                            trans.Start("Create a Ceiling");
                            try
                            {
                                ceilingsFound = FindCeilings(room, lrp.TransformValue);
                                if (ceilingsFound.Count > 0)
                                {
                                    ceilingType = FindCeilingType(room);
                                }
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                failureMessages.AppendLine(room.Name + ": " + ex.Message);
                            }
                        }

                        if (ceilingsFound.Count > 0 && null != ceilingType)
                        {
                            var copiedCeilings = CreateCeilings(room, ceilingsFound, ceilingType, ref failureMessages);
                            if (copiedCeilings.Count > 0 && !createdCeilings.ContainsKey(GetElementIdValue(room.Id)))
                            {
                                createdCeilings.Add(GetElementIdValue(room.Id), copiedCeilings);
                                created = true;
                            }
                        }
                    }
                    tg.Assimilate();
                }
                catch (Exception ex)
                {
                    tg.RollBack();
                    MessageBox.Show("Cannot create ceilings from the selected rooms.\n" + ex.Message, "Create Ceilings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            return created;
        }

        private List<Ceiling> CreateCeilings(Room room, List<Ceiling> ceilingsFound, CeilingType ceilingType, ref StringBuilder msgBuilder)
        {
            var copiedCeilings = new List<Ceiling>();
            try
            {
                foreach (var ceiling in ceilingsFound)
                {
                    using (var trans = new Transaction(Doc))
                    {
                        trans.Start("Copy Ceiling");
                        try
                        {
                            double finishThickness = 0;
                            var param = ceilingType.get_Parameter(BuiltInParameter.CEILING_THICKNESS);
                            if (null != param)
                            {
                                if (param.HasValue)
                                {
                                    finishThickness = param.AsDouble();
                                }
                            }

                            var copiedIds = ElementTransformUtils.CopyElement(Doc, ceiling.Id, new XYZ(0, 0, -finishThickness));
                            trans.Commit();

                            trans.Start("Change Ceiling Type");
                            if (copiedIds.Count > 0)
                            {
                                var copiedCeilingId = copiedIds.First();
                                var copiedCeiling = Doc.GetElement(copiedCeilingId) as Ceiling;
                                if (null != copiedCeiling)
                                {
                                    var changedTypeId = copiedCeiling.ChangeTypeId(ceilingType.Id);
                                    copiedCeilings.Add(copiedCeiling);
                                }
                            }
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            msgBuilder.AppendLine(ceiling.Name + " [" + GetElementIdValue(ceiling.Id) + "]: cannot be copied\n" + ex.Message);
                            trans.RollBack();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot create ceilings from the selected rooms.\nRoom Id:"+GetElementIdValue(room.Id) +"\n\n" + ex.Message, "Create Ceilings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return copiedCeilings;
        }

        private List<Ceiling> FindCeilings(Room room, Transform transformValue)
        {
            var ceilings = new List<Ceiling>();
            try
            {
                var roomSolid = FindRoomSolid(room, transformValue);
                if (null != roomSolid)
                {
                    var collector = new FilteredElementCollector(Doc);
                    var solidFilter = new ElementIntersectsSolidFilter(roomSolid);
                    ceilings = collector.OfClass(typeof(Ceiling)).WherePasses(solidFilter).WhereElementIsNotElementType().ToElements().Cast<Ceiling>().ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(room.Name+": Cannot find ceilings from the selected rooms.\n"+ex.Message, "Find Ceilings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return ceilings;
        }

        private Solid FindRoomSolid(Room room, Transform transformValue)
        {
            Solid roomSolid = null;
            try
            {
                var geomElem = room.ClosedShell;
                geomElem = geomElem.GetTransformed(transformValue);
                foreach (var geoObj in geomElem)
                {
                    var solid = geoObj as Solid;
                    if (null != solid)
                    {
                        if (solid.Volume > 0)
                        {
                            roomSolid = solid;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not find the room solid.\n" + ex.Message, "Find Room Solid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return roomSolid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        private CeilingType FindCeilingType(Room room)
        {
            CeilingType ceilingType = null;
            try
            {
                var typeName = "Ceiling Finish";
                if (!room.Document.IsLinked)
                {
                    var param = room.get_Parameter(BuiltInParameter.ROOM_FINISH_CEILING);
                    if (null != param)
                    {
                        if (param.HasValue)
                        {
                            typeName = param.AsString();
                        }
                    }
                }

                var query = CeilingTypes.FirstOrDefault(x => x.Name == typeName);
                if (query != null)
                {
                    ceilingType = query;
                }
                else
                {
                    //create a ceiling type
                    CeilingType typeToCopy;
                    var materialId = ElementId.InvalidElementId;
                    foreach (var cType in CeilingTypes)
                    {
                        if (!cType.CanBeCopied) continue;

                        if (cType.GetCompoundStructure() != null)
                        {
                            var cStructure = cType.GetCompoundStructure();
                            foreach (var layer in cStructure.GetLayers())
                            {
                                if (layer.Function == MaterialFunctionAssignment.Finish1 || layer.Function == MaterialFunctionAssignment.Finish2)
                                {
                                    typeToCopy = cType;
                                    ceilingType = typeToCopy.Duplicate(typeName) as CeilingType;
                                    materialId = layer.MaterialId;
                                    break;
                                }
                            }
                        }
                        if (ceilingType != null)
                        {
                            break;
                        }
                    }

                    var compoundStructure = ceilingType.GetCompoundStructure();
                    const double layerThickness = 0.020833;
                    var layerIndex = compoundStructure.GetFirstCoreLayerIndex();
                    compoundStructure.SetLayerFunction(layerIndex, MaterialFunctionAssignment.Finish1);
                    compoundStructure.SetLayerWidth(layerIndex, layerThickness);
                    compoundStructure.SetMaterialId(layerIndex, materialId);

                    for (var i = compoundStructure.LayerCount - 1; i > -1; i--)
                    {
                        if (i == layerIndex) { continue; }
                        compoundStructure.DeleteLayer(i);
                    }

                    compoundStructure.StructuralMaterialIndex = 0;
                    ceilingType.SetCompoundStructure(compoundStructure);
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(room.Name+": Cannot find a ceiling type.\n"+ex.Message, "Find Ceiling Type", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return ceilingType;
        }
    }
}
