using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HOK.RhinoImporter.RhinoUtils;
using HOK.RhinoImporter.DirectShapeUtils;

namespace HOK.RhinoImporter
{
    [Transaction(TransactionMode.Manual)]
    public class Command :IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            m_app = commandData.Application;
            m_doc = m_app.ActiveUIDocument.Document;
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Rhino 3-D Model (*.3dm)|*.3dm";
                dialog.Title = "Select a Rhino Model";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    RhinoGeometryContainer container = RhinoReader.ReadRhino(dialog.FileName);
                    DirectShapeContainer shapeContainer = DirectShapeConverter.ConverToDirectShape(m_doc, container);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Cannot start command.\n" + ex.Message);
            }
            
            //ProcessMultipleSAT();
            return Result.Succeeded;
        }

        public void ProcessMultipleSAT()
        {
            List<ElementId> importIds = new List<ElementId>();

            // create and set new SAT options
            SATImportOptions satOptions = new SATImportOptions();
            satOptions.Placement = ImportPlacement.Origin;
            satOptions.ColorMode = ImportColorMode.BlackAndWhite;
            satOptions.Unit = ImportUnit.Millimeter;

            using (Transaction trans = new Transaction(m_doc, "UpdateSAT"))
            {
                trans.Start();

                List<GeometryObject> geoObjList = new List<GeometryObject>();
                DirectShape ds = null;
                ElementId currentId;

                try
                {
                    currentId = m_doc.Import(@"B:\Rhino\OpenNURBS\v5_example_file.sat", satOptions, m_doc.ActiveView);
                    importIds.Add(currentId);
                }
                catch (Exception)
                {
                    currentId = ElementId.InvalidElementId;
                }
                // extract geometry from import instance
                ImportInstance ii = m_doc.GetElement(currentId) as ImportInstance;
                Options gOptions = new Options();
                gOptions.ComputeReferences = true;
                GeometryElement geoElement = ii.get_Geometry(gOptions);

                // get solids from geometry element
                List<GeometryObject> tempGeoList = FindElementGeometry(geoElement);
                foreach (GeometryObject go in tempGeoList)
                {
                    geoObjList.Add(go);
                }


                ds = DirectShape.CreateElement(m_doc, new ElementId((int)BuiltInCategory.OST_GenericModel));
                ds.SetShape(geoObjList);

                // set the Direct Shape options
                DirectShapeOptions dsOptions = ds.GetOptions();
                dsOptions.ReferencingOption = DirectShapeReferencingOption.Referenceable;
                ds.SetOptions(dsOptions);

                trans.Commit();

                trans.Start("Delete Elements");
                // clean up imported solids
                m_doc.Delete(importIds);
                trans.Commit();
            }
        }


        private List<GeometryObject> FindElementGeometry(GeometryElement geoElement)
        {
            List<GeometryObject> geoObjects = new List<GeometryObject>();
            try
            {
                var solidGeometries = from geoObj in geoElement
                                      where geoObj.GetType() == typeof(Solid) && (geoObj as Solid).Volume != 0
                                      select geoObj;
                if (solidGeometries.Count() > 0)
                {
                    geoObjects.AddRange(solidGeometries);
                }
                var meshGeometries = from geoObj in geoElement where geoObj.GetType() == typeof(Mesh) select geoObj;
                if (meshGeometries.Count() > 0)
                {
                    geoObjects.AddRange(meshGeometries);
                }

                var geoInstances = from geoObj in geoElement where geoObj.GetType() == typeof(GeometryInstance) select geoObj as GeometryInstance;
                if (geoInstances.Count() > 0)
                {
                    foreach (GeometryInstance geoInst in geoInstances)
                    {
                        GeometryElement geoElem2 = geoInst.GetSymbolGeometry(geoInst.Transform);
                        geoObjects.AddRange(FindElementGeometry(geoElem2));
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return geoObjects;
        }
    }
}
