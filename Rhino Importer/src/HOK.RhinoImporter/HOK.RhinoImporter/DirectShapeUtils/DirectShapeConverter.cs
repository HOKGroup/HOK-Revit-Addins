using Autodesk.Revit.DB;
using HOK.RhinoImporter.RhinoUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HOK.RhinoImporter.DirectShapeUtils
{
    public static class DirectShapeConverter
    {
        public static DirectShapeContainer ConverToDirectShape(Document doc, RhinoGeometryContainer container)
        {
            DirectShapeContainer shapeContainer = new DirectShapeContainer();
            try
            {
                using (TransactionGroup tg = new TransactionGroup(doc))
                {
                    tg.Start("Start Convert");
                    try
                    {
                        //Brep
                        foreach(RhinoObjectInfo rInfo in container.Breps)
                        {
                            using (Transaction trans = new Transaction(doc))
                            {
                                try
                                {
                                    trans.Start("Convert To DirectShape");
                                    DirectShapeInfo shapeInfo = ConvertBrep(doc, rInfo);
                                    if (shapeInfo.DirectShapeId != ElementId.InvalidElementId)
                                    {
                                        shapeContainer.DirectShapes.Add(shapeInfo);
                                    }
                                    trans.Commit();
                                }
                                catch (Exception ex)
                                {
                                    trans.RollBack();
                                    MessageBox.Show("Cannot Connvert.\n" + ex.Message);
                                    string message = ex.Message;
                                }
                            }
                        }
                        tg.Assimilate();
                    }
                    catch(Exception ex)
                    {
                        string message = ex.Message;
                        MessageBox.Show("Cannot Connvert.\n" + ex.Message);
                        tg.RollBack();
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Cannot Connvert.\n" + ex.Message);
            }
            return shapeContainer;
        }

        private static DirectShapeInfo ConvertBrep(Document doc, RhinoObjectInfo rhinoInfo)
        {
            DirectShapeInfo shapeInfo = new DirectShapeInfo();
            try
            {
                BRepBuilder brepBuilder = null;
                Rhino.Geometry.Brep rhinoBrep = rhinoInfo.Geometry as Rhino.Geometry.Brep;
                if(rhinoBrep.IsSolid)
                {
                    brepBuilder = new BRepBuilder(BRepType.Solid);
                }
                else if(rhinoBrep.IsSurface)
                {
                    brepBuilder = new BRepBuilder(BRepType.OpenShell);
                }

                foreach (Rhino.Geometry.BrepFace brepFace in rhinoBrep.Faces)
                {

                    BRepBuilderGeometryId nurbSplineFaceId = BRepBuilderGeometryId.InvalidGeometryId();
                    BRepBuilderGeometryId loopId = BRepBuilderGeometryId.InvalidGeometryId();

                    bool reverse = brepFace.OrientationIsReversed;
                    if(brepFace.ObjectType == Rhino.DocObjects.ObjectType.Surface)
                    {
                        Rhino.Geometry.NurbsSurface nurbsSurface = brepFace.ToNurbsSurface();

                        Rhino.Geometry.Collections.NurbsSurfacePointList points = nurbsSurface.Points;
                        int dirU = points.CountU;
                        int dirV = points.CountV;
                        int degreeU = nurbsSurface.Degree(0);
                        int degreeV = nurbsSurface.Degree(1);

                        // knots
                        Rhino.Geometry.Collections.NurbsSurfaceKnotList knotsU = nurbsSurface.KnotsU;
                        Rhino.Geometry.Collections.NurbsSurfaceKnotList knotsV = nurbsSurface.KnotsV;

                        List<XYZ> controlPoints = new List<XYZ>();
                        XYZ[][] rvtPoints = new XYZ[dirU][];
                        double[][] rvtWeights = new double[dirU][];
                        for (int u = 0; u < dirU; u++)
                        {
                            rvtPoints[u] = new XYZ[dirV];
                            rvtWeights[u] = new double[dirV];
                            for (int v = 0; v < dirV; v++)
                            {
                                // point coordinates at u, v
                                Rhino.Geometry.Point3d pt = points.GetControlPoint(u, v).Location;

                                XYZ xyz = new XYZ(pt.X, pt.Y, pt.Z);
                                rvtPoints[u][v] = xyz;
                                controlPoints.Add(xyz);
                                // weights at u, v
                                rvtWeights[u][v] = points.GetControlPoint(u, v).Weight;
                            }
                        }

                        // knots U
                        List<double> rvt_knotsU = new List<double>();
                        rvt_knotsU.Add(knotsU[0]);
                        for (int i = 0; i < knotsU.Count; i++)
                        {
                            rvt_knotsU.Add(knotsU[i]);
                        }
                        rvt_knotsU.Add(knotsU[knotsU.Count - 1]);

                        // knots V
                        List<double> rvt_knotsV = new List<double>();
                        rvt_knotsV.Add(knotsV[0]);
                        for (int i = 0; i < knotsV.Count; i++)
                        {
                            rvt_knotsV.Add(knotsV[i]);
                        }
                        rvt_knotsV.Add(knotsV[knotsV.Count - 1]);

                        BRepBuilderSurfaceGeometry brepSurface = BRepBuilderSurfaceGeometry.CreateNURBSSurface(degreeU, degreeV, rvt_knotsU, rvt_knotsV, controlPoints, reverse, null);
                        nurbSplineFaceId = brepBuilder.AddFace(brepSurface, reverse);
                        loopId = brepBuilder.AddLoop(nurbSplineFaceId);
                    }

                    foreach (Rhino.Geometry.BrepLoop loop in brepFace.Loops)
                    {
                        Rhino.Geometry.Curve curve = loop.To3dCurve();
                        Rhino.Geometry.NurbsCurve nurbsCurve = curve.ToNurbsCurve();

                        int degree = nurbsCurve.Degree;
                        List<XYZ> controlPoints = new List<XYZ>();
                        List<double> weights = new List<double>();
                        List<double> knots = new List<double>();

                        for(int i=0;i<nurbsCurve.Points.Count; i++)
                        {
                            Rhino.Geometry.ControlPoint ctrlPoint = nurbsCurve.Points[i];
                            Rhino.Geometry.Point3d point = ctrlPoint.Location;
                            XYZ xyz = new XYZ(point.X, point.Y, point.Z);

                            controlPoints.Add(xyz);
                            weights.Add(ctrlPoint.Weight);
                        }

                        knots.Add(nurbsCurve.Knots[0]);
                        for(int i = 0; i<nurbsCurve.Knots.Count; i++)
                        {
                            double knot = nurbsCurve.Knots[i];
                            knots.Add(knot);
                        }
                        knots.Add(nurbsCurve.Knots[nurbsCurve.Knots.Count - 1]);



                        Curve rvtCurve = NurbSpline.CreateCurve(degree, knots, controlPoints, weights);
                        BRepBuilderEdgeGeometry edgeGeo = BRepBuilderEdgeGeometry.Create(rvtCurve);
                        BRepBuilderGeometryId edgeId = brepBuilder.AddEdge(edgeGeo);
                        brepBuilder.AddCoEdge(loopId, edgeId, false);
                        
                    }

                    brepBuilder.FinishLoop(loopId);
                    brepBuilder.FinishFace(nurbSplineFaceId);
                }

                brepBuilder.Finish();


                DirectShape shape = DirectShape.CreateElement(doc, new ElementId((int)BuiltInCategory.OST_GenericModel));
                shape.ApplicationId = "RhinoBrep";
                shape.ApplicationDataId = rhinoInfo.ObjectId.ToString();

                if(null!=shape)
                {
                    shape.SetShape(brepBuilder);
                }

                shapeInfo.DirectShapeId = shape.Id;
                shapeInfo.RhinoObjectId = rhinoInfo.ObjectId;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Cannot Conver To Brep.\n" + ex.Message);
            }
            return shapeInfo;
        }



    }
}
