using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Newtonsoft.Json;

namespace HOK.JsonExporter
{

    public class JsonExporter
    {
        private Document m_doc;
        private string jsFileName;
        private Reference refFace;
        private Face m_face;
        private string refString; //stable representation
        private JsonContainer container;
        private Random random = new Random();

        public JsonExporter(Document doc, string fileName, Reference referenceface)
        {
            try
            {
                m_doc = doc;
                jsFileName = fileName;
                refFace = referenceface;
                refString = refFace.ConvertToStableRepresentation(m_doc);
                m_face = doc.GetElement(refFace).GetGeometryObjectFromReference(refFace) as Face;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to export to JSON.\n"+ex.Message, "JSON Exporter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public bool ExportJSON()
        {
            bool exported = false;
            try
            {
                List<Face> subFaces = CreateSubDividedFaces(true, 5, 0, 0);//by distance
                if (subFaces.Count > 0)
                {
                    bool result = WriteJson(subFaces);
                    if (result)
                    {
                        JsonSerializerSettings settings = new JsonSerializerSettings();
                        settings.NullValueHandling = NullValueHandling.Ignore;
                        Formatting formatting = Formatting.Indented;
                        string strJson = JsonConvert.SerializeObject(container, formatting, settings);

                        File.WriteAllText(jsFileName, strJson);
                        if (File.Exists(jsFileName))
                        {
                            exported = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to Export JSON.\n" + ex.Message, "Export to JSON", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return exported;
        }

        private List<Face> CreateSubDividedFaces(bool byDistance, double distanceVal, int numSplitU, int numSplitV)
        {
            List<Face> subFaces = new List<Face>();
            using (TransactionGroup tg = new TransactionGroup(m_doc))
            {
                tg.Start("Create Faces");
                BoundingBoxUV bb = m_face.GetBoundingBox();
                double minU = bb.Min.U;
                double minV = bb.Min.V;
                int numU = numSplitU;
                int numV = numSplitV;
                double spacingU = 0;
                double spacingV = 0;
                if (byDistance)
                {
                    numU = (int)((bb.Max.U - bb.Min.U) / distanceVal);
                    numV = (int)((bb.Max.V - bb.Min.V) / distanceVal);
                }

                spacingU = (bb.Max.U - bb.Min.U) / numU;
                spacingV = (bb.Max.V - bb.Min.V) / numV;

                for (int i = 0; i < numU; i++)
                {
                    for (int j = 0; j < numV; j++)
                    {
                        using (Transaction trans = new Transaction(m_doc))
                        {
                            trans.Start("Create Face");
                            try
                            {

                                UV uv1 = new UV(minU + (i * spacingU), minV + (j * spacingV));
                                XYZ point1 = m_face.Evaluate(uv1);

                                UV uv2 = new UV(minU + ((i + 1) * spacingU), minV + (j * spacingV));
                                XYZ point2 = m_face.Evaluate(uv2);

                                UV uv3 = new UV(minU + ((i + 1) * spacingU), minV + ((j + 1) * spacingV));
                                XYZ point3 = m_face.Evaluate(uv3);

                                UV uv4 = new UV(minU + (i * spacingU), minV + ((j + 1) * spacingV));
                                XYZ point4 = m_face.Evaluate(uv4);


                                List<Curve> curves = new List<Curve>();
                                Line line1 = Line.CreateBound(point1, point2);
                                curves.Add(line1);
                                Line line2 = Line.CreateBound(point2, point3);
                                curves.Add(line2);
                                Line line3 = Line.CreateBound(point3, point4);
                                curves.Add(line3);
                                Line line4 = Line.CreateBound(point4, point1);
                                curves.Add(line4);

                                CurveLoop curveLoop = CurveLoop.Create(curves);
                                List<CurveLoop> curveLoopList = new List<CurveLoop>();
                                curveLoopList.Add(curveLoop);

                                Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(curveLoopList, new XYZ(0, 0, -1), 1);
                                trans.Commit();

                                //find top face from the solid
                                Face topFace = null;
                                if (null != solid)
                                {
                                    foreach (Face face in solid.Faces)
                                    {
                                        XYZ normal = face.ComputeNormal(new UV(0, 0));
                                        if (normal.Z > 0)
                                        {
                                            if (Math.Abs(normal.Z) > Math.Abs(normal.X) && Math.Abs(normal.Z) > Math.Abs(normal.Y))
                                            {
                                                topFace = face; break;
                                            }
                                        }
                                    }
                                }
                                if (null != topFace)
                                {
                                    subFaces.Add(topFace);
                                }
                                
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                string message = ex.Message;
                            }
                        }
                    }
                }
                tg.Assimilate();
            }
            return subFaces;
        }

        private bool WriteJson(List<Face> faces)
        {
            bool result = false;
            try
            {
                container = new JsonContainer();
                container.metadata = new JsonContainer.Metadata();
                container.metadata.type = "Object";
                container.metadata.version = 4.3;
                container.metadata.generator = "Json Exporter for AVF";

                container.materials = new List<JsonContainer.JsMaterial>();
                container.geometries = new List<JsonContainer.JsGeometry>();

                container.obj = new JsonContainer.JsObject();
                container.obj.uuid = m_doc.ActiveView.UniqueId;
                container.obj.type = "Scene";
                container.obj.matrix = new double[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
                container.obj.children = new List<JsonContainer.JsObject>();

                using (TransactionGroup tg = new TransactionGroup(m_doc))
                {
                    tg.Start("Write Json");
                    foreach (Face face in faces)
                    {
                        using (Transaction trans = new Transaction(m_doc))
                        {
                            trans.Start("Write Face Json");
                            try
                            {
                                WriteFaceJson(face);
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                string message = ex.Message;
                                trans.RollBack();
                            }
                        }
                    }
                    tg.Assimilate();
                }
                

                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write the default settings.\n"+ex.Message, "Write Default Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        private void WriteFaceJson(Face face)
        {
            try
            {
                List<int> fIndices = new List<int>();
                List<int> fVertices = new List<int>();
                List<double> fNormals = new List<double>();
                XYZ centerPoint = null;
                if (GetFaceGeometry(face, out fIndices, out fVertices, out fNormals, out centerPoint))
                {
                    Color randomColor = new Color((byte)random.Next(0, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255));
                    int colorVal = ConverterUtil.ColorToInt(randomColor);

                    JsonContainer.JsMaterial jsMaterial = new JsonContainer.JsMaterial();
                    jsMaterial.uuid = Guid.NewGuid().ToString();
                    jsMaterial.type = "MeshPhongMaterial";
                    jsMaterial.name = "AVF Material";
                    jsMaterial.color = colorVal;
                    jsMaterial.ambient = colorVal;
                    jsMaterial.emissive = 0;
                    jsMaterial.specular = colorVal;
                    jsMaterial.shininess = 1;
                    jsMaterial.opacity = 1;
                    jsMaterial.transparent = false;
                    jsMaterial.wireframe = false;

                    container.materials.Add(jsMaterial);

                    JsonContainer.JsGeometry jsGeometry = new JsonContainer.JsGeometry();
                    jsGeometry.uuid = Guid.NewGuid().ToString();
                    jsGeometry.type = "Geometry";
                    jsGeometry.data = new JsonContainer.JsGeometryData();
                    jsGeometry.data.faces = new List<int>();
                    jsGeometry.data.vertices = new List<int>();
                    jsGeometry.data.normals = new List<double>();
                    jsGeometry.data.uvs = new List<double>();
                    jsGeometry.data.visible = true;
                    jsGeometry.data.castShadow = true;
                    jsGeometry.data.receiveShadow = false;
                    jsGeometry.data.doubleSided = true;
                    jsGeometry.data.scale = 1;
                    jsGeometry.data.faces = fIndices;
                    jsGeometry.data.vertices = fVertices;
                    jsGeometry.data.normals = fNormals;

                    container.geometries.Add(jsGeometry);

                    JsonContainer.JsObject avfElement = new JsonContainer.JsObject();
                    avfElement.uuid = Guid.NewGuid().ToString();
                    avfElement.matrix = new double[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
                    avfElement.type = "RevitElement";
                    avfElement.material = jsMaterial.uuid;
                    avfElement.userData = new Dictionary<string, string>();
                    avfElement.userData.Add("AVFReference", refString);
                    avfElement.userData.Add("PointX", ConverterUtil.FootToMm(centerPoint.X).ToString());
                    avfElement.userData.Add("PointY", ConverterUtil.FootToMm(centerPoint.Y).ToString());
                    avfElement.userData.Add("PointZ", ConverterUtil.FootToMm(centerPoint.Z).ToString());
                    //avfElement.userData.Add("AnalysisValue", "");
                    avfElement.children = new List<JsonContainer.JsObject>();

                    JsonContainer.JsObject meshObj = new JsonContainer.JsObject();
                    meshObj.name = "AVF Mesh";
                    meshObj.geometry = jsGeometry.uuid;
                    meshObj.matrix = new double[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
                    meshObj.type = "Mesh";
                    meshObj.uuid = Guid.NewGuid().ToString();
                    meshObj.material = jsMaterial.uuid;

                    avfElement.children.Add(meshObj);
                    container.obj.children.Add(avfElement);
                }
            }
            catch (Exception ex)
            {
                string message = "Cannot write face data: "+ex.Message;
            }
        }

        private bool GetFaceGeometry(Face face, out List<int> faceIndices, out List<int> faceVertices, out List<double> faceNormals, out XYZ centerPoint)
        {
            bool result = false;
            faceIndices = new List<int>();
            faceVertices = new List<int>();
            faceNormals = new List<double>();
            centerPoint = new XYZ();
            try
            {
                BoundingBoxUV bb = face.GetBoundingBox();
                UV midUV = new UV((bb.Max.U - bb.Min.U) / 2, (bb.Max.V - bb.Min.V) / 2);
                centerPoint = face.Evaluate(midUV);

                Mesh mesh = m_face.Triangulate();
                int nTriangles = mesh.NumTriangles;
                IList<XYZ> vertices = mesh.Vertices;
                int nVertices = vertices.Count;
                List<int> vertexCoordsMm = new List<int>(3 * nVertices);

                foreach (XYZ v in vertices)
                {
                    vertexCoordsMm.Add(ConverterUtil.FootToMm(v.X));
                    vertexCoordsMm.Add(ConverterUtil.FootToMm(v.Y));
                    vertexCoordsMm.Add(ConverterUtil.FootToMm(v.Z));
                }

                int[] triangleIndices = new int[3];
                XYZ[] triangleCorners = new XYZ[3];

                for (int i = 0; i < nTriangles; ++i)
                {
                    faceIndices.Add(2); //triangle with material

                    MeshTriangle triangle = mesh.get_Triangle(i);

                    for (int j = 0; j < 3; ++j)
                    {
                        int k = (int)triangle.get_Index(j);
                        triangleIndices[j] = k;
                        triangleCorners[j] = vertices[k];
                    }

                    // Calculate constant triangle facet normal.

                    XYZ v = triangleCorners[1]
                      - triangleCorners[0];
                    XYZ w = triangleCorners[2]
                      - triangleCorners[0];
                    XYZ triangleNormal = v
                      .CrossProduct(w)
                      .Normalize();

                    for (int j = 0; j < 3; ++j)
                    {
                        int nFaceVertices = faceVertices.Count;

                        faceIndices.Add(nFaceVertices / 3);

                        int i3 = triangleIndices[j] * 3;

                        // Rotate the X, Y and Z directions, 
                        // since the Z direction points upward 
                        // in Revit as opposed to sideways or
                        // outwards or forwards in WebGL.

                        faceVertices.Add(vertexCoordsMm[i3 + 1]);
                        faceVertices.Add(vertexCoordsMm[i3 + 2]);
                        faceVertices.Add(vertexCoordsMm[i3]);

                        UV uv = m_face.Project(
                             triangleCorners[j]).UVPoint;

                        XYZ normal = m_face.ComputeNormal(uv);

                        faceNormals.Add(normal.Y);
                        faceNormals.Add(normal.Z);
                        faceNormals.Add(normal.X);
                    }

                    faceIndices.Add(0);
                }
                result = true;
            }
            catch (Exception ex)
            {
                string message = "Cannot get face geometry: "+ex.Message;
            }
            return result;
        }


    }
}
