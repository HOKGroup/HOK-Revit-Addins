using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SmartBCF.BCFDBWriter
{
    public enum BCFTables
    {
        //FileInfo
        BCFFileInfo = 0,

        //project
        ProjectExtension = 1, Project = 2,

        //version
        Version = 3,

        //markup
        Topic = 4, Labels = 5, HeaderFile = 6, BimSnippet = 7, DocumentReferences = 8, RelatedTopics = 9, Viewpoints = 10, Comment = 11,

        //visinfo
        Point = 12, Direction = 13, RevitExtensions = 14, Components = 15, Bitmaps = 16, ClippingPlane = 17, Lines = 18, OrthogonalCamera = 19, PerspectiveCamera = 20
    }
}
