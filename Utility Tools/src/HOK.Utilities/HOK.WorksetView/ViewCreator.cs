using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using System.Windows.Forms;

namespace HOK.WorksetView
{
    public static class ViewCreator
    {
        public static View3D Create3DView(Document doc, ItemInfo itemInfo, ViewFamilyType view3dFamilyType, bool overwrite, List<Category> List2DCategories, List<Category> List3DCategories, Autodesk.Revit.DB.View TemplateView)
        {
            View3D view3D = null;
            try
            {
                switch (itemInfo.ItemType)
                {
                    case ViewBy.Workset:
                        view3D = CreateWorkset3DView(doc, itemInfo, view3dFamilyType, overwrite);
                        break;
                    case ViewBy.Phase:
                        view3D = CreatePhase3DView(doc, itemInfo, view3dFamilyType, overwrite);
                        break;
                    case ViewBy.DesignOption:
                        view3D = CreateDesignOption3DView(doc, itemInfo, view3dFamilyType, overwrite);
                        break;
                    case ViewBy.Link:
                        view3D = CreateLink3DView(doc, itemInfo, view3dFamilyType, overwrite);
                        break;
                    case ViewBy.Category:
                        view3D = CreateCategory3DView(doc, itemInfo, view3dFamilyType, overwrite, List2DCategories, List3DCategories, TemplateView);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create 3d view.\n" + ex.Message, "Create 3D View", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return view3D;
        }

        public static View3D CreateWorkset3DView(Document doc, ItemInfo itemInfo, ViewFamilyType view3dFamilyType, bool overwrite)
        {
            View3D view3D = null;
            try
            {
                string viewName = "WS - 3D - " + itemInfo.ItemName;
                using (TransactionGroup tg = new TransactionGroup(doc))
                {
                    tg.Start("Create 3D View");
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
                            using (Transaction trans = new Transaction(doc))
                            {
                                trans.Start("Create Isometric");
                                try
                                {
                                    view3D = View3D.CreateIsometric(doc, view3dFamilyType.Id);
                                    view3D.Name = viewName;
                                    if (view3D.CanModifyViewDiscipline())
                                    {
                                        view3D.Discipline = ViewDiscipline.Coordination;
                                    }
                                    trans.Commit();
                                }
                                catch (Exception ex)
                                {
                                    trans.RollBack();
                                    MessageBox.Show("Failed to create Isometric.\n" + ex.Message, "Create Isometric", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }

                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Set Visibility");
                            try
                            {
                                FilteredWorksetCollector worksetCollector = new FilteredWorksetCollector(doc);
                                IList<Workset> worksetList = worksetCollector.ToWorksets();
                                var worksets = from workset in worksetList where workset.Kind == WorksetKind.UserWorkset select workset;
                                foreach (Workset ws in worksets)
                                {
                                    if (ws.Kind == WorksetKind.UserWorkset)
                                    {
                                        if (ws.Id.IntegerValue == itemInfo.ItemId)
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
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                MessageBox.Show("Failed to set visibility.\n" + ex.Message, "Set Visibility", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }

                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Set SectionBox");
                            try
                            {
                                collector = new FilteredElementCollector(doc, view3D.Id);
                                List<Element> elements = collector.ToElements().ToList();
                                if (elements.Count > 0)
                                {
                                    BoundingBoxXYZ boundingBox = GetBoundingBox(elements);
                                    if (null != boundingBox)
                                    {
                                        view3D.SetSectionBox(boundingBox);
                                    }
                                }
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                MessageBox.Show("Failed to set sectionbox.\n" + ex.Message, "Set Sectionbox", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to create 3d views by worksets.\n" + ex.Message, "Create 3D Views by Worksets", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        tg.RollBack();
                    }
                    tg.Assimilate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create workset views.\n" + ex.Message, "Create Workset 3D View", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return view3D;
        }

        public static View3D CreatePhase3DView(Document doc, ItemInfo itemInfo, ViewFamilyType view3dFamilyType, bool overwrite)
        {
            View3D view3D = null;
            try
            {
                string viewName = "PH - 3D - " + itemInfo.ItemName;
                using (Transaction trans = new Transaction(doc))
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
                        try
                        {
                            view3D = View3D.CreateIsometric(doc, view3dFamilyType.Id);
                            view3D.Name = viewName;
                            if (view3D.CanModifyViewDiscipline())
                            {
                                view3D.Discipline = ViewDiscipline.Coordination;
                            }

                            Parameter param = view3D.get_Parameter(BuiltInParameter.VIEW_PHASE);
                            if (null != param)
                            {
                                if (!param.IsReadOnly)
                                {
                                    param.Set(itemInfo.ItemId);
                                }
                            }
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans.RollBack();
                            MessageBox.Show("Failed to create 3d view by phases.\n" + ex.Message, "Create 3D View", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create 3d views by phases.\n" + ex.Message, "Create 3D Views by Phases", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return view3D;
        }

        public static View3D CreateDesignOption3DView(Document doc, ItemInfo itemInfo, ViewFamilyType view3dFamilyType, bool overwrite)
        {
            View3D view3D = null;
            try
            {
                string viewName = "OP - 3D - " + itemInfo.ItemName;
                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Create 3D View");
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
                            view3D = View3D.CreateIsometric(doc, view3dFamilyType.Id);
                            view3D.Name = viewName;
                            if (view3D.CanModifyViewDiscipline())
                            {
                                view3D.Discipline = ViewDiscipline.Coordination;
                            }

                            trans.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        MessageBox.Show("Failed to create a 3d view for the design option, " + itemInfo.ItemName + "\n" + ex.Message, "Create 3D View", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create 3d views by design options.\n" + ex.Message, "Create 3D Views by Design Options", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return view3D;
        }

        public static View3D CreateLink3DView(Document doc, ItemInfo itemInfo, ViewFamilyType view3dFamilyType, bool overwrite)
        {
            View3D view3D = null;
            try
            {
                string viewName = "LINKRVT - 3D - " + itemInfo.ItemName;
                using (TransactionGroup tg = new TransactionGroup(doc))
                {
                    tg.Start("Create 3D View");
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
                            using (Transaction trans = new Transaction(doc))
                            {
                                trans.Start("Create Isometric");
                                try
                                {
                                    view3D = View3D.CreateIsometric(doc, view3dFamilyType.Id);
                                    view3D.Name = viewName;
                                    if (view3D.CanModifyViewDiscipline())
                                    {
                                        view3D.Discipline = ViewDiscipline.Coordination;
                                    }
                                    trans.Commit();
                                }
                                catch (Exception ex)
                                {
                                    trans.RollBack();
                                    MessageBox.Show("Failed to create Isometric.\n" + ex.Message, "Create Isometric", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }


                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Set SectionBox");
                            try
                            {
                                collector = new FilteredElementCollector(doc);
                                List<Element> linkInstances = collector.OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().ToElements().ToList();
                                var selectedLinks = from link in linkInstances where link.Name.Contains(itemInfo.ItemName) && null != link.Location select link;
                                if (selectedLinks.Count() > 0)
                                {
                                    BoundingBoxXYZ boundingBox = GetBoundingBox(selectedLinks.ToList());
                                    if (null != boundingBox)
                                    {
                                        view3D.SetSectionBox(boundingBox);
                                    }
                                }
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                MessageBox.Show("Failed to set sectionbox.\n" + ex.Message, "Set Sectionbox", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to create 3d views by worksets.\n" + ex.Message, "Create 3D Views by Worksets", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        tg.RollBack();
                    }
                    tg.Assimilate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create workset views.\n" + ex.Message, "Create Workset 3D View", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return view3D;
        }

        public static View3D CreateCategory3DView(Document doc, ItemInfo itemInfo, ViewFamilyType view3dFamilyType, bool overwrite, List<Category> List2DCategories, List<Category> List3DCategories, Autodesk.Revit.DB.View TemplateView)
        {
            View3D view3D = null;
            try
            {
                string viewName = itemInfo.ItemName;
                using (TransactionGroup tg = new TransactionGroup(doc))
                {
                    tg.Start("Create 3D View");
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
                            using (Transaction trans = new Transaction(doc))
                            {
                                trans.Start("Create Isometric");
                                try
                                {
                                    view3D = View3D.CreateIsometric(doc, view3dFamilyType.Id);
                                    view3D.Name = viewName;
                                    if (TemplateView != null)
                                    {
                                        view3D.ViewTemplateId = TemplateView.Id;
                                    }
                                    trans.Commit();

                                    trans.Start("Create Isometric");
                                    view3D.ViewTemplateId = new ElementId(-1);
                                    #region Isolate Categories
                                    Categories Cats = doc.Settings.Categories;
                                    foreach (Category cat in Cats)
                                    {
                                        Category catCheck = null;
                                        if (cat.CategoryType == CategoryType.Annotation)
                                        {
                                            catCheck = List2DCategories.Where(m => m.Name == cat.Name).FirstOrDefault();
                                        }
                                        else if (cat.CategoryType == CategoryType.Model)
                                        {
                                            catCheck = List3DCategories.Where(m => m.Name == cat.Name).FirstOrDefault();
                                        }
                                        if (catCheck == null)
                                        {
                                            try
                                            {
                                                cat.set_Visible(view3D, false);
                                            }
                                            catch { }

                                        }
                                    }
                                    #endregion
                                    trans.Commit();

                                }
                                catch (Exception ex)
                                {
                                    trans.RollBack();
                                    MessageBox.Show("Failed to create Isometric.\n" + ex.Message, "Create Isometric", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }


                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Set SectionBox");
                            try
                            {
                                collector = new FilteredElementCollector(doc);
                                List<Element> linkInstances = collector.OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().ToElements().ToList();
                                var selectedLinks = from link in linkInstances where link.Name.Contains(itemInfo.ItemName) && null != link.Location select link;
                                if (selectedLinks.Count() > 0)
                                {
                                    BoundingBoxXYZ boundingBox = GetBoundingBox(selectedLinks.ToList());
                                    if (null != boundingBox)
                                    {
                                        view3D.SetSectionBox(boundingBox);
                                    }
                                }
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                MessageBox.Show("Failed to set sectionbox.\n" + ex.Message, "Set Sectionbox", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to create 3d views by worksets.\n" + ex.Message, "Create 3D Views by Worksets", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        tg.RollBack();
                    }
                    tg.Assimilate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create workset views.\n" + ex.Message, "Create Workset 3D View", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return view3D;
        }

        private static BoundingBoxXYZ GetBoundingBox(List<Element> elements)
        {
            BoundingBoxXYZ boundingBox = new BoundingBoxXYZ();
            try
            {
                boundingBox.Enabled = true;
                for (int i = 0; i < 3; i++)
                {
                    boundingBox.set_MinEnabled(i, true);
                    boundingBox.set_MaxEnabled(i, true);
                    boundingBox.set_BoundEnabled(0, i, true);
                    boundingBox.set_BoundEnabled(1, i, true);
                }

                BoundingBoxXYZ tempBoundingBox = elements.First().get_BoundingBox(null);
                if (null != tempBoundingBox)
                {
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
                            BoundingBoxXYZ bbBox = element.get_BoundingBox(null);
                            if (null != boundingBox)
                            {
                                bbBox.Enabled = true;
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get bounding box XYZ.\n" + ex.Message, "Get Bounding Box", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return boundingBox;
        }

        public static ViewPlan CreateFloorPlan(Document doc, ItemInfo itemInfo, ViewFamilyType viewPlanFamilyType, Level planLevel, bool overwrite, List<Category> List2DCategories, List<Category> List3DCategories, Autodesk.Revit.DB.View TemplateView)
        {
            ViewPlan viewPlan = null;
            try
            {
                switch (itemInfo.ItemType)
                {
                    case ViewBy.Workset:
                        viewPlan = CreateWorksetFloorPlan(doc, itemInfo, viewPlanFamilyType, planLevel, overwrite);
                        break;
                    case ViewBy.Phase:
                        viewPlan = CreatePhaseFloorPlan(doc, itemInfo, viewPlanFamilyType, planLevel, overwrite);
                        break;
                    case ViewBy.DesignOption:
                        viewPlan = CreateDesignOptionFloorPlan(doc, itemInfo, viewPlanFamilyType, planLevel, overwrite);
                        break;
                    case ViewBy.Category:
                        viewPlan = CreateCategoryFloorPlan(doc, itemInfo, viewPlanFamilyType, overwrite, List2DCategories, List3DCategories, planLevel, TemplateView); ;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create floor plan.\n" + ex.Message, "Create Floor Plan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return viewPlan;
        }

        public static ViewPlan CreateWorksetFloorPlan(Document doc, ItemInfo itemInfo, ViewFamilyType viewPlanFamilyType, Level planLevel, bool overwrite)
        {
            ViewPlan viewPlan = null;
            string viewName = planLevel.Name + " - " + itemInfo.ItemName;
            using (TransactionGroup tg = new TransactionGroup(doc))
            {
                tg.Start("Create Floor Plan");
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
                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Create Plan View");
                            try
                            {
                                viewPlan = ViewPlan.Create(doc, viewPlanFamilyType.Id, planLevel.Id);
                                viewPlan.Name = viewName;
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                MessageBox.Show("Failed to create plan view.\n" + ex.Message, "Create Plan View", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }


                    using (Transaction trans = new Transaction(doc))
                    {
                        trans.Start("Set Visibility");
                        try
                        {
                            FilteredWorksetCollector worksetCollector = new FilteredWorksetCollector(doc);
                            IList<Workset> worksetList = worksetCollector.ToWorksets();
                            var worksets = from workset in worksetList where workset.Kind == WorksetKind.UserWorkset select workset;
                            foreach (Workset ws in worksets)
                            {
                                if (ws.Kind == WorksetKind.UserWorkset)
                                {
                                    if (ws.Id.IntegerValue == itemInfo.ItemId)
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
                            MessageBox.Show("Failed to set visibility.\n" + ex.Message, "Set Visibility", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    tg.RollBack();
                    MessageBox.Show("Failed to create floor plans by worksets.\n" + ex.Message, "Create Floor Plans by Worksets", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                tg.Assimilate();
            }
            return viewPlan;
        }

        public static ViewPlan CreatePhaseFloorPlan(Document doc, ItemInfo itemInfo, ViewFamilyType viewPlanFamilyType, Level planLevel, bool overwrite)
        {
            ViewPlan viewPlan = null;
            try
            {
                string viewName = planLevel.Name + " - " + itemInfo.ItemName;
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
                                if (!param.IsReadOnly)
                                {
                                    param.Set(itemInfo.ItemId);
                                }
                            }
                            trans.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        MessageBox.Show("Failed to create a plan view for the phase, " + itemInfo.ItemName + "\n" + ex.Message, "Create Plan View", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create floor plans by phases.\n" + ex.Message, "Create Floor Plans by Phases", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return viewPlan;
        }

        public static ViewPlan CreateDesignOptionFloorPlan(Document doc, ItemInfo itemInfo, ViewFamilyType viewPlanFamilyType, Level planLevel, bool overwrite)
        {
            ViewPlan viewPlan = null;
            try
            {
                string viewName = planLevel.Name + " - " + itemInfo.ItemName;
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
                        MessageBox.Show("Failed to create a plan view for the design option, " + itemInfo.ItemName + "\n" + ex.Message, "Create Plan View", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create floor plans by design options.\n" + ex.Message, "Create Floor Plans by Design Options", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return viewPlan;
        }

        public static ViewPlan CreateCategoryFloorPlan(Document doc, ItemInfo itemInfo, ViewFamilyType viewPlanFamilyType, bool overwrite, List<Category> List2DCategories, List<Category> List3DCategories, Level planLevel, Autodesk.Revit.DB.View TemplateView)
        {
            ViewPlan viewPlan = null;
            try
            {
                string viewName = itemInfo.ItemName;
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
                            Autodesk.Revit.DB.View V = itemInfo.ItemObj as Autodesk.Revit.DB.View;
                            viewPlan = ViewPlan.Create(doc, viewPlanFamilyType.Id, planLevel.Id);
                            viewPlan.Name = viewName;
                            if (TemplateView != null)
                            {
                                viewPlan.ViewTemplateId = TemplateView.Id;
                            }
                            trans.Commit();

                            trans.Start("Create Plan View");
                            viewPlan.ViewTemplateId = new ElementId(-1);
                            #region Isolate Categories
                            Categories Cats = doc.Settings.Categories;
                            foreach (Category cat in Cats)
                            {
                                Category catCheck = null;
                                if (cat.CategoryType == CategoryType.Annotation)
                                {
                                    catCheck = List2DCategories.Where(m => m.Name == cat.Name).FirstOrDefault();
                                }
                                else if (cat.CategoryType == CategoryType.Model)
                                {
                                    catCheck = List3DCategories.Where(m => m.Name == cat.Name).FirstOrDefault();
                                }
                                if (catCheck == null)
                                {
                                    try
                                    {
                                        cat.set_Visible(viewPlan, false);
                                    }
                                    catch { }

                                }
                            }
                            #endregion
                            trans.Commit();
                        }

                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        MessageBox.Show("Failed to create a plan view, " + itemInfo.ItemName + "\n" + ex.Message, "Create Plan View", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create floor plans by categories.\n" + ex.Message, "Create Floor Plans by Categories", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return viewPlan;
        }

    }
}
