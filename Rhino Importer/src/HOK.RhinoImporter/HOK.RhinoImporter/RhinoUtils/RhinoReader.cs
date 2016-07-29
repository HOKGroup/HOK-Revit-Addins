using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.RhinoImporter.RhinoUtils
{
    public static class RhinoReader
    {
        public static RhinoGeometryContainer ReadRhino(string path)
        {
            RhinoGeometryContainer geoContainer = new RhinoGeometryContainer();
            try
            {
                File3dm rhinoFile = File3dm.Read(path);
                geoContainer.FilePath = path;

                File3dmObjectTable objects = rhinoFile.Objects;
                foreach (File3dmObject fileObj in objects)
                {
                    Guid objId = fileObj.Attributes.ObjectId;
                    string name = fileObj.Name;
                    GeometryBase geometry = fileObj.Geometry;

                    RhinoObjectInfo objInfo = new RhinoObjectInfo(objId, name, geometry);

                    CollectGeometryInfo(objInfo, ref geoContainer);
                }
            }
            catch(Exception ex)
            {
                string message = ex.Message;
            }
            return geoContainer;
        }

        private static void CollectGeometryInfo(RhinoObjectInfo objInfo, ref RhinoGeometryContainer container)
        {
            try
            {
                ObjectType geoType = objInfo.Geometry.ObjectType;
                switch (geoType)
                {
                    case ObjectType.Point:
                        Point point = objInfo.Geometry as Point;
                        if(null!=point)
                        {
                            container.Points.Add(objInfo);
                        }
                        break;
                    case ObjectType.Curve:
                        Curve curve = objInfo.Geometry as Curve;
                        if(null!=curve)
                        {
                            container.Curves.Add(objInfo);
                        }
                        break;
                    case ObjectType.Surface:
                        Surface surface = objInfo.Geometry as Surface;
                        if(null!=surface)
                        {
                            container.Surfaces.Add(objInfo);
                        }
                        break;
                    case ObjectType.Brep:
                        Brep brep = objInfo.Geometry as Brep;
                        if(null!=brep)
                        {
                            container.Breps.Add(objInfo);
                        }
                        break;
                    case ObjectType.Extrusion:
                        Extrusion extrusion = objInfo.Geometry as Extrusion;
                        if (null != extrusion)
                        {
                            container.Extrusions.Add(objInfo);
                        }
                        break;
                    case ObjectType.Mesh:
                        Mesh mesh = objInfo.Geometry as Mesh;
                        if (null != mesh)
                        {
                            container.Meshes.Add(objInfo);
                        }
                        break;
                }

         
            }
            catch(Exception ex)
            {
                string message = ex.Message;
            }
        }
    }
}
