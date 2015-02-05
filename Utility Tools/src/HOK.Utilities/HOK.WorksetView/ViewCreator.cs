using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using System.Windows.Forms;

namespace HOK.WorksetView
{
    public static class ViewCreator
    {
        public static ToolStripProgressBar progressBar;

        public static View3D Create3DView(Document doc, WorksetInfo worksetInfo, ViewFamilyType view3dFamilyType, bool overwrite)
        {
            View3D view3D = null;
            try
            {
                string viewName = "WS - 3D - " + worksetInfo.WorksetName;
                using (Transaction trans = new Transaction(doc))
                {
                    try
                    {
                        FilteredElementCollector collector = new FilteredElementCollector(doc);
                        List<View3D> view3ds = collector.OfClass(typeof(View3D)).ToElements().Cast<View3D>().ToList();
                        var views = from view in view3ds where view.Name == viewName select view;
                        if (views.Count() > 0)
                        {
                            if (overwrite)
                            {
                                view3D = views.First();
                            }
                            else
                            {
                                return view3D;
                            }
                        }

                        if (null == view3D)
                        {
                            trans.Start("Create 3D View");
                            view3D = View3D.CreateIsometric(doc, view3dFamilyType.Id);
                            view3D.Name = viewName;
                            view3D.Discipline = ViewDiscipline.Coordination;
                            trans.Commit();
                        }

                        trans.Start("Set Visibility");
                        FilteredWorksetCollector worksetCollector = new FilteredWorksetCollector(doc);
                        IList<Workset> worksetList = worksetCollector.ToWorksets();
                        var worksets = from workset in worksetList where workset.Kind == WorksetKind.UserWorkset select workset;
                        foreach (Workset ws in worksets)
                        {
                            if (ws.Kind == WorksetKind.UserWorkset)
                            {
                                if (ws.Id.IntegerValue == worksetInfo.WorksetId.IntegerValue)
                                {
                                    view3D.SetWorksetVisibility(ws.Id, WorksetVisibility.Visible);
                                }
                                else
                                {
                                    view3D.SetWorksetVisibility(ws.Id, WorksetVisibility.Hidden);
                                }
                            }
                        }
                        trans.Commit();


                        trans.Start("Get Bounding Box");
                        collector = new FilteredElementCollector(doc, view3D.Id);
                        List<Element> elements = collector.ToElements().ToList();

                        BoundingBoxXYZ boundingBox = GetBoundingBox(elements);
#if RELEASE2013
                        view3D.SectionBox = boundingBox;
#else
                        view3D.SetSectionBox(boundingBox);
#endif
                        //view3d.GetSectionBox().Enabled = true;
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        MessageBox.Show("Failed to create a 3d view for the workset, " + worksetInfo.WorksetName + "\n" + ex.Message, "Create 3D View", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create 3d views by worksets.\n"+ex.Message, "Create 3D Views by Worksets", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return view3D;
        }

        private static BoundingBoxXYZ GetBoundingBox(List<Element> elements)
        {
            BoundingBoxXYZ boundingBox = new BoundingBoxXYZ();
            try
            {
                progressBar.Value = 0;
                progressBar.Maximum = elements.Count;

                boundingBox.Enabled = true;
                for (int i = 0; i < 3; i++)
                {
                    boundingBox.set_MinEnabled(i, true);
                    boundingBox.set_MaxEnabled(i, true);
                    boundingBox.set_BoundEnabled(0, i, true);
                    boundingBox.set_BoundEnabled(1, i, true);
                }

                BoundingBoxXYZ tempBoundingBox = elements.First().get_BoundingBox(null);
                tempBoundingBox.Enabled = true;

                double maxX = tempBoundingBox.Max.X;
                double maxY = tempBoundingBox.Max.Y;
                double maxZ = tempBoundingBox.Max.Z;
                double minX = tempBoundingBox.Min.X;
                double minY = tempBoundingBox.Min.Y;
                double minZ = tempBoundingBox.Min.Z;

                foreach (Element element in elements)
                {
                    try
                    {
                        progressBar.PerformStep();

                        BoundingBoxXYZ bbBox = element.get_BoundingBox(null);
                        bbBox.Enabled = true;
                        if (null != boundingBox)
                        {
                            if (bbBox.Max.X > maxX) { maxX = bbBox.Max.X; }
                            if (bbBox.Max.Y > maxY) { maxY = bbBox.Max.Y; }
                            if (bbBox.Max.Z > maxZ) { maxZ = bbBox.Max.Z; }
                            if (bbBox.Min.X < minX) { minX = bbBox.Min.X; }
                            if (bbBox.Min.Y < minY) { minY = bbBox.Min.Y; }
                            if (bbBox.Min.Z < minZ) { minZ = bbBox.Min.Z; }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                XYZ xyzMax = new XYZ(maxX, maxY, maxZ);
                XYZ xyzMin = new XYZ(minX, minY, minZ);

                boundingBox.set_Bounds(0, xyzMin);
                boundingBox.set_Bounds(1, xyzMax);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get bounding box XYZ.\n" + ex.Message, "Get Bounding Box", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return boundingBox;
        }

        public static View3D Create3DView(Document doc, PhaseInfo phaseInfo, ViewFamilyType view3dFamilyType, bool overwrite)
        {
            View3D view3D = null;
            try
            {
                string viewName = "PH - 3D - " + phaseInfo.PhaseName;
                using (Transaction trans = new Transaction(doc))
                {
                    try
                    {
                        FilteredElementCollector collector = new FilteredElementCollector(doc);
                        List<View3D> view3ds = collector.OfClass(typeof(View3D)).ToElements().Cast<View3D>().ToList();
                        var views = from view in view3ds where view.Name == viewName select view;
                        if (views.Count() > 0)
                        {
                            if (overwrite)
                            {
                                view3D = views.First();
                            }
                            else
                            {
                                return view3D;
                            }
                        }

                        if (null == view3D)
                        {
                            trans.Start("Create 3D View");
                            view3D = View3D.CreateIsometric(doc, view3dFamilyType.Id);
                            view3D.Name = viewName;
                            view3D.Discipline = ViewDiscipline.Coordination;
                            Parameter param = view3D.get_Parameter(BuiltInParameter.VIEW_PHASE);
                            if (null != param)
                            {
                                param.Set(phaseInfo.PhaseId);
                            }
                            trans.Commit();
                        }
                        
                        /*
                        trans.Start("Set Phase");
                        BuiltInParameter bip = BuiltInParameter.PHASE_CREATED;
                        FilterRule filterRule = ParameterFilterRuleFactory.CreateNotEqualsRule(new ElementId(bip), phaseInfo.PhaseId);
                        ElementParameterFilter filter = new ElementParameterFilter(filterRule);
                        FilteredElementCollector viewCollector = new FilteredElementCollector(doc, view3D.Id);
                        List<Element> elements = viewCollector.WherePasses(filter).ToElements().ToList();
                        if (elements.Count > 0)
                        {
                            progressBar.Value = 0;
                            progressBar.Maximum = elements.Count;

                            foreach (Element element in elements)
                            {
                                progressBar.PerformStep();
                                Parameter param = element.get_Parameter(bip);
                                if (null != param)
                                {
                                    if (!param.IsReadOnly)
                                    {
                                        param.Set(phaseInfo.PhaseId);
                                    }
                                }
                            }
                        }
                        trans.Commit();
                        */
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        MessageBox.Show("Failed to create a 3d view for the phase, " + phaseInfo.PhaseName + "\n" + ex.Message, "Create 3D View", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create 3d views by phases.\n" + ex.Message, "Create 3D Views by Phases", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return view3D;
        }

        public static View3D Create3DView(Document doc, DesignOptionInfo designOptionInfo, ViewFamilyType view3dFamilyType, bool overwrite)
        {
            View3D view3D = null;
            try
            {
                string viewName = "OP - 3D - " + designOptionInfo.DesignOptionName;
                using (Transaction trans = new Transaction(doc))
                {
                    try
                    {
                        FilteredElementCollector collector = new FilteredElementCollector(doc);
                        List<View3D> view3ds = collector.OfClass(typeof(View3D)).ToElements().Cast<View3D>().ToList();
                        var views = from view in view3ds where view.Name == viewName select view;
                        if (views.Count() > 0)
                        {
                            if (overwrite)
                            {
                                view3D = views.First();
                            }
                            else
                            {
                                return view3D;
                            }
                        }

                        if (view3D == null)
                        {
                            trans.Start("Create 3D View");
                            view3D = View3D.CreateIsometric(doc, view3dFamilyType.Id);
                            view3D.Name = viewName;
                            view3D.Discipline = ViewDiscipline.Coordination;
                            trans.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        MessageBox.Show("Failed to create a 3d view for the design option, " + designOptionInfo.DesignOptionName + "\n" + ex.Message, "Create 3D View", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create 3d views by design options.\n" + ex.Message, "Create 3D Views by Design Options", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return view3D;
        }

        public static ViewPlan CreateFloorPlan(Document doc, WorksetInfo worksetInfo, ViewFamilyType viewPlanFamilyType, Level planLevel, bool overwrite)
        {
            ViewPlan viewPlan = null;
            try
            {
                string viewName = planLevel.Name + " - " + worksetInfo.WorksetName;
                using (Transaction trans = new Transaction(doc))
                {
                    try
                    {
                        FilteredElementCollector collector = new FilteredElementCollector(doc);
                        List<ViewPlan> viewPlans = collector.OfClass(typeof(ViewPlan)).ToElements().Cast<ViewPlan>().ToList();
                        var views = from view in viewPlans where view.Name == viewName select view;
                        if (views.Count() > 0)
                        {
                            if (overwrite)
                            {
                                viewPlan = views.First();
                            }
                            else
                            {
                                return viewPlan;
                            }
                        }
                        if (null == viewPlan)
                        {
                            trans.Start("Create Plan View");
                            viewPlan = ViewPlan.Create(doc, viewPlanFamilyType.Id, planLevel.Id);
                            viewPlan.Name = viewName;
                            trans.Commit();
                        }

                        trans.Start("Set Visibility");
                        FilteredWorksetCollector worksetCollector = new FilteredWorksetCollector(doc);
                        IList<Workset> worksetList = worksetCollector.ToWorksets();
                        var worksets = from workset in worksetList where workset.Kind == WorksetKind.UserWorkset select workset;
                        foreach (Workset ws in worksets)
                        {
                            if (ws.Kind == WorksetKind.UserWorkset)
                            {
                                if (ws.Id.IntegerValue == worksetInfo.WorksetId.IntegerValue)
                                {
                                    viewPlan.SetWorksetVisibility(ws.Id, WorksetVisibility.Visible);
                                }
                                else
                                {
                                    viewPlan.SetWorksetVisibility(ws.Id, WorksetVisibility.Hidden);
                                }
                            }
                        }
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        MessageBox.Show("Failed to create a plan view for the workset, " + worksetInfo.WorksetName + "\n" + ex.Message, "Create Plan View", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create floor plans by worksets.\n"+ex.Message, "Create Floor Plans by Worksets", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return viewPlan;
        }

        public static ViewPlan CreateFloorPlan(Document doc, PhaseInfo phaseInfo, ViewFamilyType viewPlanFamilyType, Level planLevel, bool overwrite)
        {
            ViewPlan viewPlan = null;
            try
            {
                string viewName = planLevel.Name + " - " + phaseInfo.PhaseName;
                using (Transaction trans = new Transaction(doc))
                {
                    try
                    {
                        FilteredElementCollector collector = new FilteredElementCollector(doc);
                        List<ViewPlan> viewPlans = collector.OfClass(typeof(ViewPlan)).ToElements().Cast<ViewPlan>().ToList();
                        var views = from view in viewPlans where view.Name == viewName select view;
                        if (views.Count() > 0)
                        {
                            if (overwrite)
                            {
                                viewPlan = views.First();
                            }
                            else
                            {
                                return viewPlan;
                            }
                        }

                        if (null == viewPlan)
                        {
                            trans.Start("Create Plan View");
                            viewPlan = ViewPlan.Create(doc, viewPlanFamilyType.Id, planLevel.Id);
                            viewPlan.Name = viewName;
                            Parameter param = viewPlan.get_Parameter(BuiltInParameter.VIEW_PHASE);
                            if (null != param)
                            {
                                param.Set(phaseInfo.PhaseId);
                            }
                            trans.Commit();
                        }
                        /*
                        trans.Start("Set Phase");
                        BuiltInParameter bip = BuiltInParameter.PHASE_CREATED;
                        FilterRule filterRule = ParameterFilterRuleFactory.CreateNotEqualsRule(new ElementId(bip), phaseInfo.PhaseId);
                        ElementParameterFilter filter = new ElementParameterFilter(filterRule);
                        FilteredElementCollector viewCollector = new FilteredElementCollector(doc, viewPlan.Id);
                        List<Element> elements = viewCollector.WherePasses(filter).ToElements().ToList();
                        if (elements.Count > 0)
                        {
                            progressBar.Value = 0;
                            progressBar.Maximum = elements.Count;

                            foreach (Element element in elements)
                            {
                                progressBar.PerformStep();

                                Parameter param = element.get_Parameter(bip);
                                if (null != param)
                                {
                                    if (!param.IsReadOnly)
                                    {
                                        param.Set(phaseInfo.PhaseId);
                                    }
                                }
                            }
                        }
                        trans.Commit();
                         */
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        MessageBox.Show("Failed to create a plan view for the phase, " + phaseInfo.PhaseName + "\n" + ex.Message, "Create Plan View", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create floor plans by phases.\n" + ex.Message, "Create Floor Plans by Phases", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return viewPlan;
        }

        public static ViewPlan CreateFloorPlan(Document doc, DesignOptionInfo optionInfo, ViewFamilyType viewPlanFamilyType, Level planLevel, bool overwrite)
        {
            ViewPlan viewPlan = null;
            try
            {
                string viewName = planLevel.Name + " - " + optionInfo.DesignOptionName;
                using (Transaction trans = new Transaction(doc))
                {
                    try
                    {
                        FilteredElementCollector collector = new FilteredElementCollector(doc);
                        List<ViewPlan> viewPlans = collector.OfClass(typeof(ViewPlan)).ToElements().Cast<ViewPlan>().ToList();
                        var views = from view in viewPlans where view.Name == viewName select view;
                        if (views.Count() > 0)
                        {
                            if (overwrite)
                            {
                                viewPlan = views.First();
                            }
                            else
                            {
                                return viewPlan;
                            }
                        }

                        if (null == viewPlan)
                        {
                            trans.Start("Create Plan View");
                            viewPlan = ViewPlan.Create(doc, viewPlanFamilyType.Id, planLevel.Id);
                            viewPlan.Name = viewName;
                            trans.Commit();
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        MessageBox.Show("Failed to create a plan view for the design option, " + optionInfo.DesignOptionName + "\n" + ex.Message, "Create Plan View", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create floor plans by design options.\n" + ex.Message, "Create Floor Plans by Design Options", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return viewPlan;
        }

    }
}
