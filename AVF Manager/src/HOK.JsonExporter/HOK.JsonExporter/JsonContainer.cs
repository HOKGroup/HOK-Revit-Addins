using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Dynamic;

namespace HOK.JsonExporter
{

    //three.js object class
    [DataContract]
    public class JsonContainer
    {
        [DataContract]
        public class JsMaterial
        {
            [DataMember]
            public string uuid { get; set; }
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public string type { get; set; } // MeshPhongMaterial
            [DataMember]
            public int color { get; set; } // 16777215
            [DataMember]
            public int ambient { get; set; } //16777215
            [DataMember]
            public int emissive { get; set; } // 1
            [DataMember]
            public int specular { get; set; } //1118481
            [DataMember]
            public int shininess { get; set; } // 30
            [DataMember]
            public double opacity { get; set; } // 1
            [DataMember]
            public bool transparent { get; set; } // false
            [DataMember]
            public bool wireframe { get; set; } // false

        }

        [DataContract]
        public class JsGeometryData
        {
            // populate data object properties
            //jason.data.vertices = new object[mesh.Vertices.Count * 3];
            //jason.data.normals = new object[0];
            //jason.data.uvs = new object[0];
            //jason.data.faces = new object[mesh.Faces.Count * 4];
            //jason.data.scale = 1;
            //jason.data.visible = true;
            //jason.data.castShadow = true;
            //jason.data.receiveShadow = false;
            //jason.data.doubleSided = true;

            [DataMember]
            public List<int> vertices { get; set; } // millimetres
            // "morphTargets": []
            [DataMember]
            public List<double> normals { get; set; }
            // "colors": []
            [DataMember]
            public List<double> uvs { get; set; }
            [DataMember]
            public List<int> faces { get; set; } // indices into Vertices + Materials
            [DataMember]
            public double scale { get; set; }
            [DataMember]
            public bool visible { get; set; }
            [DataMember]
            public bool castShadow { get; set; }
            [DataMember]
            public bool receiveShadow { get; set; }
            [DataMember]
            public bool doubleSided { get; set; }
            [DataMember]
            public List<JsMaterial> materials { get; set; }
        }

        [DataContract]
        public class JsGeometry
        {
            [DataMember]
            public string uuid { get; set; }
            [DataMember]
            public string type { get; set; } // "Geometry"
            [DataMember]
            public JsGeometryData data { get; set; }
            //[DataMember] public double scale { get; set; }
            //[DataMember]
            //public List<JsMaterial> materials { get; set; }
        }

        [DataContract]
        public class JsObject
        {
            [DataMember]
            public string uuid { get; set; }
            [DataMember]
            public string name { get; set; } // BIM <document name>
            [DataMember]
            public string type { get; set; } // Object3D
            [DataMember]
            public double[] matrix { get; set; } // [1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1]
            [DataMember]
            public List<JsObject> children { get; set; }

            // The following are only on the children:

            [DataMember]
            public string geometry { get; set; }
            [DataMember]
            public string material { get; set; }

            //[DataMember] public List<double> position { get; set; }
            //[DataMember] public List<double> rotation { get; set; }
            //[DataMember] public List<double> quaternion { get; set; }
            //[DataMember] public List<double> scale { get; set; }
            //[DataMember] public bool visible { get; set; }
            //[DataMember] public bool castShadow { get; set; }
            //[DataMember] public bool receiveShadow { get; set; }
            //[DataMember] public bool doubleSided { get; set; }

            [DataMember]
            public Dictionary<string, string> userData { get; set; }
        }

        // https://github.com/mrdoob/three.js/wiki/JSON-Model-format-3

        // for the faces, we will use
        // triangle with material
        // 00 00 00 10 = 2
        // 2, [vertex_index, vertex_index, vertex_index], [material_index]     // e.g.:
        //
        //2, 0,1,2, 0

        public class Metadata
        {
            [DataMember]
            public string type { get; set; } //  "Object"
            [DataMember]
            public double version { get; set; } // 4.3
            [DataMember]
            public string generator { get; set; } //  "RvtVa3c Revit vA3C exporter"
        }

        [DataMember]
        public Metadata metadata { get; set; }

        [DataMember(Name = "object")]
        public JsObject obj { get; set; }

        [DataMember]
        public List<JsGeometry> geometries;
        [DataMember]
        public List<JsMaterial> materials;

    }


}
