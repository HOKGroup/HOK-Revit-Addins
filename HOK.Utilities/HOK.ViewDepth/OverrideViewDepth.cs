using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;

//code source:http://puntorevit.blogspot.com.au/

namespace HOK.ViewDepth
{
    public class OverrideViewDepth
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;
        private Autodesk.Revit.DB.View currentView;

        public OverrideViewDepth(UIApplication uiapp)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;
            currentView = m_doc.ActiveView;
            try
            {
                bool viewOverriden = ViewDepthDataStorageUtil.GetOverridenViews(m_doc, currentView.Id);
                
                if (viewOverriden)
                {
                    TaskDialog mainDialog = new TaskDialog("View Depth - Override Graphics");
                    mainDialog.MainInstruction = "The current view has already applied graphics overriden by the View Depth tool.\n";
                    mainDialog.MainContent = "Select an option below.";

                    mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Override Graphics");
                    mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Clear All Overrides");

                    TaskDialogResult tResult = mainDialog.Show();
                    if (TaskDialogResult.CommandLink1 == tResult)
                    {
                        bool overriden = OverrideGraphics(currentView);
                        bool updatedStorage = ViewDepthDataStorageUtil.UpdateDataStorage(m_doc, currentView.Id, overriden);
                    }
                    else if (TaskDialogResult.CommandLink2 == tResult)
                    {
                        bool cleared = ClearAllOverrides(currentView);
                        if (cleared)
                        {
                            bool updatedStorage = ViewDepthDataStorageUtil.UpdateDataStorage(m_doc, currentView.Id, false);
                        }
                    }
                }
                else
                {
                    bool overriden = OverrideGraphics(currentView);
                    bool updatedStorage = ViewDepthDataStorageUtil.UpdateDataStorage(m_doc, currentView.Id, overriden);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to override the depth of view.\n"+ex.Message, "ViewDepthOverride", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool OverrideGraphics(Autodesk.Revit.DB.View activeView)
        {
            bool result = false;
            try
            {
                //known issues: doesn't work with linked files and 3D Views
                //Creates the lists of ElementId to pass to the Projection Color Override by Element
                //those are just empty container at the moment
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);

                List<ElementId> ids0 = new List<ElementId>();
                List<ElementId> ids1 = new List<ElementId>();
                List<ElementId> ids2 = new List<ElementId>();


                //It works fine for 2D views, building sections and elevations for example
                //it should work also with floor plans and even tilted Detail Views
                //but it wasn't my goal when I started
                //won't work for a 3D View
                Parameter clip = activeView.get_Parameter(BuiltInParameter.VIEWER_BOUND_FAR_CLIPPING);
                //If the far clipping is not active the default depth is 10 feet and won't work correctly
                if (clip != null && clip.AsInteger() == 0)
                {
                    TaskDialog.Show("View Depth Override", "In order to use this macro far clipping must be activated.");
                }
                else
                {
                    //If the far clipping is active then the view depth can be subdivided into 3 segments:
                    //foreground (0)
                    //middle (1)
                    //background (2)
                    ids0 = IntegralCollection(activeView, 0);
                    ids1 = IntegralCollection(activeView, 1);
                    ids2 = IntegralCollection(activeView, 2);
                    //Just a check to handle some common errors, for instance not even one
                    //ElementId was found in the foreground view interval
                    if (ids0.Count == 0)
                    {
                        TaskDialog.Show("View Depth Override", "Something went wrong in the closer segment.\n\nPlease adjust the view depth to include some objects.");

                        ElementId e = new FilteredElementCollector(m_doc).OfCategory(BuiltInCategory.OST_Walls).FirstElement().Id;
                        ids0.Add(e);
                        ids1.Add(e);
                        ids2.Add(e);
                    }
                    else
                    {
                        //Again just a check to handle some common errors, in this case not even one
                        //ElementId was found in the background view interval
                        //because the view depth is too much rather then just enough to enclos
                        //the objects in the model
                        if (ids2.Count == 0)
                        {
                            TaskDialog.Show("View Depth Override", "Something went wrong in the farther segment to be overridden in Grey 192.\n\nPlease check that the view depth in the current view is just enough to include the objects you need.");
                            ElementId e = new FilteredElementCollector(m_doc).OfCategory(BuiltInCategory.OST_Walls).FirstElement().Id;
                            ids2.Add(e);
                            ids1.Add(e);
                        }
                    }
                }
                //Begins the transaction to override the elements
                using (Transaction t = new Transaction(m_doc, "View Depth Override"))
                {
                    t.Start();
                    while (ids0.Count > 0)
                    {
                        //Stores the color for the foreground
                        OverrideGraphicSettings gSettings = activeView.GetElementOverrides(ids0.First());
                        Color Color0 = gSettings.ProjectionLineColor;

                        if (ids1.Count != 0)
                        {
                            OverrideGraphicSettings gSettings1 = new OverrideGraphicSettings();
                            gSettings1.SetProjectionLineColor(new Color((byte)128, (byte)128, (byte)128));

                            foreach (ElementId eId in ids1)
                            {
                                activeView.SetElementOverrides(eId, gSettings1);
                            }
                        }
                        else
                        {
                            //Just a precaution, not sure it is really necessary
                            TaskDialog.Show("View Depth Override", "Something went wrong in the middle segment to be overridden in Grey 128.\n\nPlease check that the view depth in the current view is just enough to include the objects you need.");
                            break;
                        }
                        if (ids2.Count != 0)
                        {
                            OverrideGraphicSettings gSettings2 = new OverrideGraphicSettings();
                            gSettings2.SetProjectionLineColor(new Color((byte)192, (byte)192, (byte)192));

                            foreach (ElementId eId in ids2)
                            {
                                activeView.SetElementOverrides(eId, gSettings2);
                            }
                        }
                        else
                        {
                            //Overrides the background segment
                            TaskDialog.Show("View Depth Override", "Something went wrong in the farther segment to be overridden in Grey 192.\n\nPlease check that the view depth in the current view is just enough to include the objects you need.");
                            break;
                        }
                        //Resets the foreground color in case of objects overlapping
                        //foreground and middle segment
                        gSettings.SetProjectionLineColor(Color0);
                        foreach (ElementId eId in ids0)
                        {
                            activeView.SetElementOverrides(eId, gSettings);
                        }
                        break;
                    }
                    m_doc.Regenerate();
                    m_app.ActiveUIDocument.RefreshActiveView();
                    t.Commit();
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to override graphics.\n" + ex.Message, "Override Graphics - " + activeView.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        private bool ClearAllOverrides(Autodesk.Revit.DB.View activeView)
        {
            bool cleared = false;
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc, activeView.Id);
                List<ElementId> elementIds = collector.ToElementIds().ToList();

                OverrideGraphicSettings settings = new OverrideGraphicSettings();
                settings.SetProjectionLineColor(Autodesk.Revit.DB.Color.InvalidColorValue);
                using (Transaction trans = new Transaction(m_doc))
                {
                    try
                    {
                        trans.Start("Clear overrides");
                        foreach (ElementId eId in elementIds)
                        {
                            activeView.SetElementOverrides(eId, settings);
                        }
                        trans.Commit();
                        cleared = true;
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        string message = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to clear all overrides.\n"+ex.Message, "Clear All Overrides", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return cleared;
        }

        private List<ElementId> IntegralCollection(Autodesk.Revit.DB.View view, int i)
        {
            List<ElementId> elementIds = new List<ElementId>();
            try
            {
                //Here is where the coordinates of the Bounding Box of the view are calculated
                //I don't like that this calculations are repeted each time this function is called (one for each segment)
                //it could be done more efficiently and for sure more clearly using the Transform...
                //but I didn't even know what that was when I recorded the video
                //the double scale is set to 1 because at first I started from the UV Bounding Box,
                //which contains also annotations and not just the model categories
                //I then used the Bounding BOX XYZ and reused the code I wrote before
                XYZ CurrentViewOrigin = view.Origin;
                double scale = 1;
                XYZ VRight = view.RightDirection;
                XYZ VUp = view.UpDirection;
                XYZ Vdir = view.ViewDirection;
                XYZ Vmin = view.CropBox.Min;
                XYZ Vmax = view.CropBox.Max;
                //I used letters for the Vertices of the main Bounding Box
                //rather then numbers for the inner box that defines the middle segment
                //I know this part looks messy but it was good to refresh some algebra concepts :)
                //here is were I should use the Transform
                XYZ Va = new XYZ(CurrentViewOrigin.X + scale * (Vmin.X * VRight.X + Vmin.Y * VUp.X) + Vmax.Z * Vdir.X, CurrentViewOrigin.Y + scale * (Vmin.X * VRight.Y + Vmin.Y * VUp.Y) + Vmax.Z * Vdir.Y, CurrentViewOrigin.Z + scale * (Vmin.X * VRight.Z + Vmin.Y * VUp.Z) + Vmax.Z * Vdir.Z);
                XYZ Vb = new XYZ(CurrentViewOrigin.X + scale * (Vmin.X * VRight.X + Vmax.Y * VUp.X) + Vmax.Z * Vdir.X, CurrentViewOrigin.Y + scale * (Vmin.X * VRight.Y + Vmax.Y * VUp.Y) + Vmax.Z * Vdir.Y, CurrentViewOrigin.Z + scale * (Vmin.X * VRight.Z + Vmax.Y * VUp.Z) + Vmax.Z * Vdir.Z);
                XYZ Vc = new XYZ(CurrentViewOrigin.X + scale * (Vmax.X * VRight.X + Vmax.Y * VUp.X) + Vmax.Z * Vdir.X, CurrentViewOrigin.Y + scale * (Vmax.X * VRight.Y + Vmax.Y * VUp.Y) + Vmax.Z * Vdir.Y, CurrentViewOrigin.Z + scale * (Vmax.X * VRight.Z + Vmax.Y * VUp.Z) + Vmax.Z * Vdir.Z);
                XYZ Vd = new XYZ(CurrentViewOrigin.X + scale * (Vmax.X * VRight.X + Vmin.Y * VUp.X) + Vmax.Z * Vdir.X, CurrentViewOrigin.Y + scale * (Vmax.X * VRight.Y + Vmin.Y * VUp.Y) + Vmax.Z * Vdir.Y, CurrentViewOrigin.Z + scale * (Vmax.X * VRight.Z + Vmin.Y * VUp.Z) + Vmax.Z * Vdir.Z);
                XYZ Ve = new XYZ(CurrentViewOrigin.X + scale * (Vmin.X * VRight.X + Vmin.Y * VUp.X) + Vmin.Z * Vdir.X, CurrentViewOrigin.Y + scale * (Vmin.X * VRight.Y + Vmin.Y * VUp.Y) + Vmin.Z * Vdir.Y, CurrentViewOrigin.Z + scale * (Vmin.X * VRight.Z + Vmin.Y * VUp.Z) + Vmin.Z * Vdir.Z);
                XYZ Vf = new XYZ(CurrentViewOrigin.X + scale * (Vmin.X * VRight.X + Vmax.Y * VUp.X) + Vmin.Z * Vdir.X, CurrentViewOrigin.Y + scale * (Vmin.X * VRight.Y + Vmax.Y * VUp.Y) + Vmin.Z * Vdir.Y, CurrentViewOrigin.Z + scale * (Vmin.X * VRight.Z + Vmax.Y * VUp.Z) + Vmin.Z * Vdir.Z);
                XYZ Vg = new XYZ(CurrentViewOrigin.X + scale * (Vmax.X * VRight.X + Vmax.Y * VUp.X) + Vmin.Z * Vdir.X, CurrentViewOrigin.Y + scale * (Vmax.X * VRight.Y + Vmax.Y * VUp.Y) + Vmin.Z * Vdir.Y, CurrentViewOrigin.Z + scale * (Vmax.X * VRight.Z + Vmax.Y * VUp.Z) + Vmin.Z * Vdir.Z);
                XYZ Vh = new XYZ(CurrentViewOrigin.X + scale * (Vmax.X * VRight.X + Vmin.Y * VUp.X) + Vmin.Z * Vdir.X, CurrentViewOrigin.Y + scale * (Vmax.X * VRight.Y + Vmin.Y * VUp.Y) + Vmin.Z * Vdir.Y, CurrentViewOrigin.Z + scale * (Vmax.X * VRight.Z + Vmin.Y * VUp.Z) + Vmin.Z * Vdir.Z);
                XYZ V1 = Ve + (Va - Ve) * 2 / 3;
                XYZ V2 = Vf + (Vb - Vf) * 2 / 3;
                XYZ V3 = Vg + (Vc - Vg) * 2 / 3;
                XYZ V4 = Vh + (Vd - Vh) * 2 / 3;
                XYZ V5 = Ve + (Va - Ve) / 3;
                XYZ V6 = Vf + (Vb - Vf) / 3;
                XYZ V7 = Vg + (Vc - Vg) / 3;
                XYZ V8 = Vh + (Vd - Vh) / 3;
                //This function returns a list of ElementId to be overridden depending on which one of
                //the three segments the objects fall in

                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                elementIds = collector.WhereElementIsNotElementType().ToElementIds().ToList();
                elementIds.Clear();

                if (i == 0)
                {
                    elementIds = Integral(Va, V2, Vd, V3);
                }
                else
                {
                    if (i == 1)
                    {
                        elementIds = Integral(V1, Vf, V4, Vg);
                    }
                    else
                    {
                        elementIds = Integral(V5, Vf, V8, Vg);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to calculate the coordinates of the bounding box of the view\n" + ex.Message, "IntegralCollection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return elementIds;
        }

        private List<ElementId> Integral(XYZ min1, XYZ max1, XYZ min2, XYZ max2)
        {
            //If the view is not parrallel to X or Y axis in Global Coordinates,
            //the selecting process is done using an integral approach
            //each segment is subdivided into a high number of subdivisions (200)
            //and smaller bounding boxes are used to filter the objects
            //that's the reason why with a non-parallel view the command is slower
            //This is the list of ElementId created and immediatly cleared
            List<ElementId> elementIds = new List<ElementId>();
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                elementIds = collector.WhereElementIsNotElementType().ToElementIds().ToList();

                elementIds.Clear();

                foreach (Document doc in m_app.Application.Documents)
                {
                    int subdivisions = 200;
                    //Here determines the minimum and the maximum point for the segment based on the four vertices
                    //that have been passed from the other function
                    XYZ a = MinVertex2(min1, max1);
                    XYZ a1 = a;
                    XYZ b = MaxVertex2(min1, max1);
                    XYZ c = MinVertex2(min2, max2);
                    XYZ d = MaxVertex2(min2, max2);
                    XYZ increment = (d - b) / subdivisions;
                    for (int i = 0; i < subdivisions; i++)
                    {
                        //This is tricky: sometimes if the view is perfectly parallel something
                        //about the vertices goes wrong (I guess there's a small tolerance)
                        //Anyway to reduce the amount of calculations if the coordinates are the same
                        //at the fifth decimal digit I assumed the coordinates to be equal
                        //I know it isn't perfect but it worked for me
                        if (Math.Round(a.X, 5) == Math.Round(b.X, 5) || Math.Round(a.Y, 5) == Math.Round(b.Y, 5))
                        {
                            i = subdivisions + 1;
                            a = MinVertex2(a1, d);
                            b = MaxVertex2(a1, d);
                        }
                        Outline ol = new Outline(a, b);
                        if (ol.IsEmpty == false)
                        {
                            //Here comes the filtering part where I tried to avoid all kinds of objects
                            //that are not useful for this task because they can't be overridden
                            //such as Element Types or Sketches of Floors/Roofs/ ceilings and so on
                            BoundingBoxIntersectsFilter BBIIF = new BoundingBoxIntersectsFilter(ol);

                            FilteredElementCollector elementCollector = new FilteredElementCollector(doc);
                            List<Element> elements = elementCollector.WherePasses(BBIIF).WhereElementIsNotElementType().WhereElementIsViewIndependent().ToElements().ToList();
                            var rElements = from element in elements where element.Category != null && element.Category.HasMaterialQuantities select element;
                            if (rElements.Count() > 0)
                            {
                                foreach (Element e in rElements)
                                {
                                    elementIds.Add(e.Id);
                                }
                            }
                        }
                        else
                        {
                            a = a + increment;
                            b = b + increment;
                        }
                        a = a + increment;
                        b = b + increment;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed do an integral process for the non parrallel coordinates.\n" + ex.Message, "Integral", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return elementIds;
        }

        private XYZ MaxVertex2(XYZ a, XYZ b)
        {
            //Given two points determines the maximum of the Bounding Box that encloses them
            double X = a.X;
            double Y = a.Y;
            double Z = a.Z;
            if (Math.Round(b.X, 5) > X)
                X = b.X;
            if (Math.Round(b.Y, 5) > Y)
                Y = b.Y;
            if (Math.Round(b.Z, 5) > Z)
                Z = b.Z;
            XYZ max = new XYZ(X, Y, Z);
            return max;
        }

        private XYZ MinVertex2(XYZ a, XYZ b)
        {
            //Given two points determines the minimum of the Bounding Box that encloses them
            double X = a.X;
            double Y = a.Y;
            double Z = a.Z;
            if (Math.Round(b.X, 5) < X)
                X = b.X;
            if (Math.Round(b.Y, 5) < Y)
                Y = b.Y;
            if (Math.Round(b.Z, 5) < Z)
                Z = b.Z;
            XYZ min = new XYZ(X, Y, Z);
            return min;
        }
    }
}
