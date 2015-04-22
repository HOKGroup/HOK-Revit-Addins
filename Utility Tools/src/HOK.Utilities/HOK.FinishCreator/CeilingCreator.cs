using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace HOK.FinishCreator
{
    public class CeilingCreator
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;
        private List<Room> selectedRooms = new List<Room>();
        private List<CeilingType> ceilingTypes = new List<CeilingType>();
        private Dictionary<int/*roomId*/, List<Ceiling>> createdCeilings = new Dictionary<int, List<Ceiling>>();

        public Dictionary<int, List<Ceiling>> CreatedCeilings { get { return createdCeilings; } set { createdCeilings = value; } }

        public CeilingCreator(UIApplication application, List<Room> rooms)
        {
            m_app = application;
            m_doc = m_app.ActiveUIDocument.Document;

            selectedRooms = rooms;

            FilteredElementCollector collector = new FilteredElementCollector(m_doc);
            ceilingTypes = collector.OfClass(typeof(CeilingType)).ToElements().Cast<CeilingType>().ToList();
        }

        public bool CreateCeiling()
        {
            bool created = false;
            try
            {
                StringBuilder failureMessages = new StringBuilder();
              
                using (TransactionGroup tg = new TransactionGroup(m_doc))
                {
                    tg.Start("Create Ceilings");
                    foreach (Room room in selectedRooms)
                    {
                        List<Ceiling> ceilingsFound = new List<Ceiling>();
                        CeilingType ceilingType = null;
                        
                        using (Transaction trans = new Transaction(m_doc))
                        {
                            trans.Start("Create a Ceiling");
                            try
                            {
                                ceilingsFound = FindCeilings(room);
                                if (ceilingsFound.Count > 0)
                                {
                                    ceilingType = FindCeilingType(room);
                                }
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                failureMessages.AppendLine(room.Name + ": "+ex.Message);
                            }
                        }

                        if(ceilingsFound.Count>0 && null!= ceilingType)
                        {
                            List<Ceiling> copiedCeilings = new List<Ceiling>();
                            foreach (Ceiling ceiling in ceilingsFound)
                            {
                                using (Transaction trans = new Transaction(m_doc))
                                {
                                    trans.Start("Copy Ceiling");
                                    try
                                    {
                                        double finishThickness = 0;
                                        Parameter param = ceilingType.get_Parameter(BuiltInParameter.CEILING_THICKNESS);
                                        if (null != param)
                                        {
                                            if (param.HasValue)
                                            {
                                                finishThickness = param.AsDouble();
                                            }
                                        }

                                        ICollection<ElementId> copiedIds = ElementTransformUtils.CopyElement(m_doc, ceiling.Id, new XYZ(0, 0, -finishThickness));
                                        trans.Commit();

                                        trans.Start("Change Ceiling Type");
                                        if (copiedIds.Count > 0)
                                        {
                                            ElementId copiedCeilingId = copiedIds.First();
                                            Ceiling copiedCeiling = m_doc.GetElement(copiedCeilingId) as Ceiling;
                                            if (null != copiedCeiling)
                                            {
                                                ElementId changedTypeId = copiedCeiling.ChangeTypeId(ceilingType.Id);
                                                copiedCeilings.Add(copiedCeiling);
                                            }
                                        }
                                        trans.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        failureMessages.AppendLine(ceiling.Name + " [" + ceiling.Id.IntegerValue + "]: cannot be copied\n"+ex.Message);
                                        trans.RollBack();
                                    }
                                }
                            }

                            if (copiedCeilings.Count > 0 && !createdCeilings.ContainsKey(room.Id.IntegerValue))
                            {
                                createdCeilings.Add(room.Id.IntegerValue, copiedCeilings);
                                created = true;
                            }
                        }
                    }
                    tg.Assimilate();
                }

                if (failureMessages.Length > 0)
                {
                    MessageBox.Show("Following items were failed to create ceilings.\n"+failureMessages.ToString(), "Failure Messages - Create Ceilings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot create ceilings from the selected rooms.\n"+ex.Message, "Create Ceilings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return created;
        }

        private List<Ceiling> FindCeilings(Room room)
        {
            List<Ceiling> ceilings = new List<Ceiling>();
            try
            {
                Solid roomSolid = FindRoomSolid(room);
                if (null != roomSolid)
                {
                    FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                    ElementIntersectsSolidFilter solidFilter = new ElementIntersectsSolidFilter(roomSolid);
                    ceilings = collector.OfClass(typeof(Ceiling)).WherePasses(solidFilter).WhereElementIsNotElementType().ToElements().Cast<Ceiling>().ToList();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(room.Name+": Cannot find ceilings from the selected rooms.\n"+ex.Message, "Find Ceilings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return ceilings;
        }

        private Solid FindRoomSolid(Room room)
        {
            Solid roomSolid = null;
            try
            {
                GeometryElement geomElem = room.ClosedShell;
                foreach (GeometryObject geoObj in geomElem)
                {
                    Solid solid = geoObj as Solid;
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

        private CeilingType FindCeilingType(Room room)
        {
            CeilingType ceilingType = null;
            try
            {
                string typeName = "Ceiling Finish";
                Parameter param = room.get_Parameter(BuiltInParameter.ROOM_FINISH_CEILING);
                if (null != param)
                {
                    if (param.HasValue)
                    {
                        typeName = param.AsString();
                    }
                }

                var query = from element in ceilingTypes where element.Name == typeName select element;
                if (query.Count() > 0)
                {
                    ceilingType = query.First();
                }
                else
                {
                    //create a ceiling type
                    CeilingType typeToCopy = null;
                    ElementId materialId = ElementId.InvalidElementId;
                    foreach (CeilingType cType in ceilingTypes)
                    {
#if RELEASE2015
                        if (!cType.CanBeCopied) { continue; }
#endif
                        if (null != cType.GetCompoundStructure())
                        {
                            CompoundStructure cStructure = cType.GetCompoundStructure();
                            foreach (CompoundStructureLayer layer in cStructure.GetLayers())
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
                        if (null != ceilingType)
                        {
                            break;
                        }
                    }

                    CompoundStructure compoundStructure = ceilingType.GetCompoundStructure();
                    double layerThickness = 0.020833;
                    int layerIndex = compoundStructure.GetFirstCoreLayerIndex();
                    compoundStructure.SetLayerFunction(layerIndex, MaterialFunctionAssignment.Finish1);
                    compoundStructure.SetLayerWidth(layerIndex, layerThickness);
                    compoundStructure.SetMaterialId(layerIndex, materialId);

                    for (int i = compoundStructure.LayerCount - 1; i > -1; i--)
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
