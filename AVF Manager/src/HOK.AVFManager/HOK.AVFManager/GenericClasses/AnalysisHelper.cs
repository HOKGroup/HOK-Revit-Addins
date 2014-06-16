using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

namespace HOK.AVFManager.GenericClasses
{
    public static class AnalysisHelper
    {
        public static bool DetermineFaceDirection(Face face, DisplayingFaces displayingFace)
        {
            bool result = false;
            XYZ normal = ComputeNormalOfFace(face);

            switch (displayingFace)
            {
                case DisplayingFaces.Top:
                    if (normal.Z > 0)
                    {
                        if (Math.Abs(normal.Z) > Math.Abs(normal.X) && Math.Abs(normal.Z) > Math.Abs(normal.Y))
                        {
                            result = true;
                        }
                    }
                    break;
                case DisplayingFaces.Side:
                    if (normal.Z == 0)
                    {
                        result = true;
                    }
                    else if (Math.Abs(normal.Z) < Math.Abs(normal.X) && Math.Abs(normal.Z) < Math.Abs(normal.Y))
                    {
                        result = true;
                    }
                    break;
                case DisplayingFaces.Bottom:
                    if (normal.Z < 0)
                    {
                        if (Math.Abs(normal.Z) > Math.Abs(normal.X) && Math.Abs(normal.Z) > Math.Abs(normal.Y))
                        {
                            result = true;
                        }
                    }
                    break;
                case DisplayingFaces.All:
                    result = true;
                    break;
            }

            return result;
        }

        public static XYZ ComputeNormalOfFace(Face face)
        {
            XYZ normal;
            BoundingBoxUV bbuv = face.GetBoundingBox();
            UV faceCenter = (bbuv.Min + bbuv.Max) / 2;
            normal = face.ComputeNormal(faceCenter);
            return normal;
        }

        public static XYZ FindCentroid(Face face)
        {
            XYZ centroid;
            BoundingBoxUV bbuv = face.GetBoundingBox();
            UV faceCenter = (bbuv.Min + bbuv.Max) / 2;
            centroid = face.Evaluate(faceCenter);
            return centroid;
        }

    }
}
