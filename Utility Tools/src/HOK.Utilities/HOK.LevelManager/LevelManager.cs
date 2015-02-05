using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using System.Windows.Forms;

namespace HOK.LevelManager
{
    public static class LevelManager
    {
        public static bool MoveColumn(Document doc, Element column, Level toLevel, bool maintain)
        {
            bool result = false;
            try
            {
                Parameter baseLevelParam = column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM);
                Parameter baseOffsetParam = column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM);
                Parameter topLevelParam = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);
                Parameter topOffsetParam = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM);

                double baseLevel = 0;
                double baseOffset = 0;
                double topLevel = 0;
                double topOffset = 0;

                if (null!=baseLevelParam && null != baseOffsetParam && null != topLevelParam && null != topOffsetParam)
                {
                    ElementId levelId = baseLevelParam.AsElementId();
                    if (ElementId.InvalidElementId != levelId && null != levelId)
                    {
                        Level level = doc.GetElement(levelId) as Level;
                        baseLevel = level.Elevation;
                    }
                    baseOffset = baseOffsetParam.AsDouble();

                    levelId = topLevelParam.AsElementId();
                    if (ElementId.InvalidElementId != levelId && null != levelId)
                    {
                        Level level = doc.GetElement(levelId) as Level;
                        topLevel = level.Elevation;
                    }
                    topOffset = topOffsetParam.AsDouble();

                    double startElevation = baseLevel + baseOffset;
                    double endElevation = topLevel + topOffset;
                    double totalLength = endElevation - startElevation;

                    if (!topLevelParam.IsReadOnly) { topLevelParam.Set(toLevel.Id); }
                    if (!baseLevelParam.IsReadOnly) { baseLevelParam.Set(toLevel.Id); }
                    
                    if (maintain)
                    {
                        if (!baseOffsetParam.IsReadOnly) { baseOffsetParam.Set(startElevation - toLevel.Elevation); }
                        if (!topOffsetParam.IsReadOnly) { topOffsetParam.Set(endElevation - toLevel.Elevation); }
                    }
                    else
                    {
                        if (!baseOffsetParam.IsReadOnly) { baseOffsetParam.Set(baseOffset); }
                        if (!topOffsetParam.IsReadOnly) { topOffsetParam.Set(baseOffset + totalLength); }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not move column.\n" + ex.Message, "LevelManager : Move Column", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        public static bool MoveWalls(Document doc, Element wall, Level toLevel, bool maintain)
        {
            bool result = false;
            try
            {
                Parameter baseConstraint = wall.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT);
                Parameter baseOffsetParam = wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET);
                Parameter topConstraint = wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE);
                Parameter unconnectedHeight = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);

                double baseLevel = 0;
                double baseOffset = 0;
                double wallHeight = 0;

                if (null != baseConstraint && null != baseOffsetParam && null != topConstraint && null != unconnectedHeight)
                {
                    ElementId levelId = baseConstraint.AsElementId();
                    if (ElementId.InvalidElementId != levelId && null != levelId)
                    {
                        Level level = doc.GetElement(levelId) as Level;
                        baseLevel = level.Elevation;
                    }

                    baseOffset = baseOffsetParam.AsDouble();
                    wallHeight = unconnectedHeight.AsDouble();

                    double startElevation = baseLevel + baseOffset;

                    baseConstraint.Set(toLevel.Id);
                    topConstraint.Set(ElementId.InvalidElementId);

                    if (maintain)
                    {
                        baseOffsetParam.Set(startElevation - toLevel.Elevation);
                        if (!unconnectedHeight.IsReadOnly)
                        {
                            unconnectedHeight.Set(wallHeight);
                        }
                    }
                    else
                    {
                        baseOffsetParam.Set(baseOffset);
                        if (!unconnectedHeight.IsReadOnly)
                        {
                            unconnectedHeight.Set(wallHeight);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not move wall.\n"+ex.Message, "LevelManager : Move Walls", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        public static bool MoveStairs(Document doc, Element stair, Level toLevel, bool maintain)
        {
            bool result = false;
            try
            {
                Parameter baseLevelParam = stair.get_Parameter(BuiltInParameter.STAIRS_BASE_LEVEL_PARAM);
                Parameter baseOffsetParam = stair.get_Parameter(BuiltInParameter.STAIRS_BASE_OFFSET);
                Parameter topLevelParam = stair.get_Parameter(BuiltInParameter.STAIRS_TOP_LEVEL_PARAM);
                Parameter topOffsetParam = stair.get_Parameter(BuiltInParameter.STAIRS_TOP_OFFSET);

                double baseLevel = 0;
                double baseOffset = 0;
                double topLevel = 0;
                double topOffset = 0;

                if (null != baseLevelParam && null != baseOffsetParam && null != topLevelParam && null != topOffsetParam)
                {
                    ElementId levelId = baseLevelParam.AsElementId();
                    if (ElementId.InvalidElementId != levelId && null != levelId)
                    {
                        Level level = doc.GetElement(levelId) as Level;
                        baseLevel = level.Elevation;
                    }
                    baseOffset = baseOffsetParam.AsDouble();

                    levelId = topLevelParam.AsElementId();
                    if (ElementId.InvalidElementId != levelId && null != levelId)
                    {
                        Level level = doc.GetElement(levelId) as Level;
                        topLevel = level.Elevation;
                    }
                    topOffset = topOffsetParam.AsDouble();

                    double startElevation = baseLevel + baseOffset;
                    double endElevation = topLevel + topOffset;
                    double totalLength = endElevation - startElevation;

                    topLevelParam.Set(toLevel.Id);
                    if (maintain)
                    {
                        baseOffsetParam.Set(startElevation - toLevel.Elevation);
                        topOffsetParam.Set(endElevation - toLevel.Elevation);
                    }
                    else
                    {
                        baseOffsetParam.Set(baseOffset);
                        topOffsetParam.Set(baseOffset + totalLength);
                    }
                    baseLevelParam.Set(toLevel.Id);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not move stair.\n" + ex.Message, "LevelManager : Move Stairs", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        public static bool MoveFloors(Document doc, Element floor, Level toLevel, bool maintain)
        {
            bool result = false;
            try
            {
                Parameter levelParam = floor.get_Parameter(BuiltInParameter.LEVEL_PARAM);
                if (null != levelParam)
                {
                    double baseLevel = 0;
                    double baseOffset = 0;
                    double startElevation = 0;

                    ElementId levelId = levelParam.AsElementId();
                    if (ElementId.InvalidElementId != levelId && null != levelId)
                    {
                        Level level = doc.GetElement(levelId) as Level;
                        baseLevel = level.Elevation;
                    }

                    Parameter baseOffsetParam = floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);
                    if (null != baseOffsetParam)
                    {
                        baseOffset = baseOffsetParam.AsDouble();
                        startElevation = baseLevel + baseOffset;
                    }

                    levelParam.Set(toLevel.Id);
                    if (maintain)
                    {
                        if (!baseOffsetParam.IsReadOnly)
                        {
                            baseOffsetParam.Set(startElevation - toLevel.Elevation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not move floor.\n" + ex.Message, "LevelManager : Move Floors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        public static bool MoveCeiling(Document doc, Element ceiling, Level toLevel, bool maintain)
        {
            bool result = false;
            try
            {
                Parameter levelParam = ceiling.get_Parameter(BuiltInParameter.LEVEL_PARAM);
                if (null != levelParam)
                {
                    double baseLevel = 0;
                    double baseOffset = 0;
                    double startElevation = 0;

                    ElementId levelId = levelParam.AsElementId();
                    if (ElementId.InvalidElementId != levelId && null != levelId)
                    {
                        Level level = doc.GetElement(levelId) as Level;
                        baseLevel = level.Elevation;
                    }

                    Parameter baseOffsetParam = ceiling.get_Parameter(BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM);
                    if (null != baseOffsetParam)
                    {
                        baseOffset = baseOffsetParam.AsDouble();
                        startElevation = baseLevel + baseOffset;
                    }

                    levelParam.Set(toLevel.Id);
                    if (maintain)
                    {
                        if (!baseOffsetParam.IsReadOnly)
                        {
                            baseOffsetParam.Set(startElevation - toLevel.Elevation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not move ceiling.\n" + ex.Message, "LevelManager : Move Ceiling", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        public static bool MovePads(Document doc, Element pad, Level toLevel, bool maintain)
        {
            bool result = false;
            try
            {
                Parameter levelParam = pad.get_Parameter(BuiltInParameter.LEVEL_PARAM);
                if (null != levelParam)
                {
                    double baseLevel = 0;
                    double baseOffset = 0;
                    double startElevation = 0;

                    ElementId levelId = levelParam.AsElementId();
                    if (ElementId.InvalidElementId != levelId && null != levelId)
                    {
                        Level level = doc.GetElement(levelId) as Level;
                        baseLevel = level.Elevation;
                    }

                    Parameter baseOffsetParam = pad.get_Parameter(BuiltInParameter.BUILDINGPAD_HEIGHTABOVELEVEL_PARAM);
                    if (null != baseOffsetParam)
                    {
                        baseOffset = baseOffsetParam.AsDouble();
                        startElevation = baseLevel + baseOffset;
                    }

                    levelParam.Set(toLevel.Id);
                    if (maintain)
                    {
                        if (!baseOffsetParam.IsReadOnly)
                        {
                            baseOffsetParam.Set(startElevation - toLevel.Elevation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not move building pad.\n" + ex.Message, "LevelManager : Move BuildingPad", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        public static bool MoveRoof(Document doc, Element roof, Level toLevel, bool maintain)
        {
            bool result = false;
            try
            {
                Parameter levelParam = roof.get_Parameter(BuiltInParameter.ROOF_BASE_LEVEL_PARAM);
                if (null != levelParam)
                {
                    double baseLevel = 0;
                    double baseOffset = 0;
                    double startElevation = 0;

                    ElementId levelId = levelParam.AsElementId();
                    if (ElementId.InvalidElementId != levelId && null != levelId)
                    {
                        Level level = doc.GetElement(levelId) as Level;
                        baseLevel = level.Elevation;
                    }

                    Parameter baseOffsetParam = roof.get_Parameter(BuiltInParameter.ROOF_LEVEL_OFFSET_PARAM);
                    if (null != baseOffsetParam)
                    {
                        baseOffset = baseOffsetParam.AsDouble();
                        startElevation = baseLevel + baseOffset;
                    }

                    levelParam.Set(toLevel.Id);
                    if (maintain)
                    {
                        if (!baseOffsetParam.IsReadOnly)
                        {
                            baseOffsetParam.Set(startElevation - toLevel.Elevation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not move roof.\n" + ex.Message, "LevelManager : Move Roof", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        public static bool MoveRailings(Document doc, Element railings, Level toLevel, bool maintain)
        {
            bool result = false;
            try
            {
                Parameter levelParam = railings.get_Parameter(BuiltInParameter.STAIRS_RAILING_BASE_LEVEL_PARAM);
                if (null != levelParam)
                {
                    double baseLevel = 0;
                    double baseOffset = 0;
                    double startElevation = 0;

                    ElementId levelId = levelParam.AsElementId();
                    if (ElementId.InvalidElementId != levelId && null != levelId)
                    {
                        Level level = doc.GetElement(levelId) as Level;
                        baseLevel = level.Elevation;
                    }

                    Parameter baseOffsetParam = railings.get_Parameter(BuiltInParameter.STAIRS_RAILING_HEIGHT_OFFSET);
                    if (null != baseOffsetParam)
                    {
                        baseOffset = baseOffsetParam.AsDouble();
                        startElevation = baseLevel + baseOffset;
                    }

                    levelParam.Set(toLevel.Id);
                    if (maintain)
                    {
                        if (!baseOffsetParam.IsReadOnly)
                        {
                            baseOffsetParam.Set(startElevation - toLevel.Elevation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not move railing.\n" + ex.Message, "LevelManager : Move Railing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        public static bool MoveGroups(Document doc, Element groupElement, Level toLevel, bool maintain)
        {
            bool result = false;
            try
            {
                Parameter levelParam = groupElement.get_Parameter(BuiltInParameter.GROUP_LEVEL);
                Parameter baseOffsetParam = groupElement.get_Parameter(BuiltInParameter.GROUP_OFFSET_FROM_LEVEL);
                if (null != levelParam && null!=baseOffsetParam)
                {
                    if (!levelParam.IsReadOnly && !baseOffsetParam.IsReadOnly)
                    {
                        double baseLevel = 0;
                        double baseOffset = 0;
                        double startElevation = 0;

                        ElementId levelId = levelParam.AsElementId();
                        if (ElementId.InvalidElementId != levelId && null != levelId)
                        {
                            Level level = doc.GetElement(levelId) as Level;
                            baseLevel = level.Elevation;
                        }

                        baseOffset = baseOffsetParam.AsDouble();
                        startElevation = baseLevel + baseOffset;

                        bool setLevel=levelParam.Set(toLevel.Id);
                        if (maintain)
                        {
                            bool setOffset=baseOffsetParam.Set(startElevation - toLevel.Elevation);
                        }
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not move floor.\n" + ex.Message, "LevelManager : Move Floors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        public static bool MoveMEP(Document doc, Element mep, Level toLevel, bool maintain)
        {
            bool result = false;
            try
            {
                Parameter levelParam = mep.get_Parameter(BuiltInParameter.RBS_START_LEVEL_PARAM);
                if (null != levelParam)
                {
                    double baseLevel = 0;
                    double baseOffset = 0;
                    double startElevation = 0;

                    ElementId levelId = levelParam.AsElementId();
                    if (ElementId.InvalidElementId != levelId && null != levelId)
                    {
                        Level level = doc.GetElement(levelId) as Level;
                        baseLevel = level.Elevation;
                    }

                    Parameter baseOffsetParam = mep.get_Parameter(BuiltInParameter.RBS_OFFSET_PARAM);
                    if (null != baseOffsetParam)
                    {
                        baseOffset = baseOffsetParam.AsDouble();
                        startElevation = baseLevel + baseOffset;
                    }

                    levelParam.Set(toLevel.Id);
                    if (maintain)
                    {
                        if (!baseOffsetParam.IsReadOnly)
                        {
                            baseOffsetParam.Set(startElevation - toLevel.Elevation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not move MEP element.\n" + ex.Message, "LevelManager : Move MEP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }
    }
}
