using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace HOK.ViewAnalysis
{
    public class LinkedInstanceData
    {
        private RevitLinkInstance m_instance = null;
        private int instanceId = -1;
        private Document linkedDocument = null;
        private string documentTitle = "";
        private Autodesk.Revit.DB.Transform transformValue = null;

        public RevitLinkInstance Instance { get { return m_instance; } set { m_instance = value; } }
        public int InstanceId { get { return instanceId; } set { instanceId = value; } }
        public Document LinkedDocument { get { return linkedDocument; } set { linkedDocument = value; } }
        public string DocumentTitle { get { return documentTitle; } set { documentTitle = value; } }
        public Autodesk.Revit.DB.Transform TransformValue { get { return transformValue; } set { transformValue = value; } }

        public LinkedInstanceData(RevitLinkInstance instance)
        {
            m_instance = instance;
            instanceId = instance.Id.IntegerValue;
#if RELEASE2013
            linkedDocument = instance.Document;
#elif RELEASE2014 || RELEASE2015
            linkedDocument = instance.GetLinkDocument();
#endif
            documentTitle = linkedDocument.Title;
            transformValue = instance.GetTotalTransform();
            
        }
    }

    
}
