using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using Revit.IFC.Export.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SmartBCF.AddIn.Util
{
    public class RevitLinkProperties
    {
        private RevitLinkInstance m_instance = null;
        private ElementId instanceId = ElementId.InvalidElementId;
        private string instanceName = "";
        private Transform transformValue = Transform.Identity;
        private Document linkedDocument = null;
        private string linkDocTitle = "";
        private string ifcProjectGuid = "";
        private bool isLinked = true;

        public RevitLinkInstance Instance { get { return m_instance; } set { m_instance = value; } }
        public ElementId InstanceId { get { return instanceId; } set { instanceId = value; } }
        public string InstanceName { get { return instanceName; } set { instanceName = value; } }
        public Transform TransformValue { get { return transformValue; } set { transformValue = value; } }
        public Document LinkedDocument { get { return linkedDocument; } set { linkedDocument = value; } }
        public string IfcProjectGuid { get { return ifcProjectGuid; } set { ifcProjectGuid = value; } }
        public bool IsLinked { get { return isLinked; } set { isLinked = value; } }

        public RevitLinkProperties(RevitLinkInstance instance)
        {
            m_instance = instance;
            instanceId = instance.Id;
            isLinked = true;
            CollectLinkInstanceInfo();
        }

        public RevitLinkProperties(Document doc)
        {
            isLinked = false;
            linkedDocument = doc;
#if RELEASE2022
            ifcProjectGuid = GUIDUtil.CreateProjectLevelGUID(linkedDocument, GUIDUtil.ProjectLevelGUIDType.Project);
#else
            ifcProjectGuid = ExporterIFCUtils.CreateProjectLevelGUID(linkedDocument, IFCProjectLevelGUIDType.Project);
#endif

        }

        public void CollectLinkInstanceInfo()
        {
            try
            {
                Parameter param = m_instance.get_Parameter(BuiltInParameter.RVT_LINK_INSTANCE_NAME);
                if (null != param)
                {
                    instanceName = param.AsString();
                }

                if (null != m_instance.GetTotalTransform())
                {
                    transformValue = m_instance.GetTotalTransform();
                }

                linkedDocument = m_instance.GetLinkDocument();
                if (null != linkedDocument)
                {
                    linkDocTitle = linkedDocument.Title;
                }

                if (null != linkedDocument)
                {
#if RELEASE2022
                    ifcProjectGuid = GUIDUtil.CreateProjectLevelGUID(linkedDocument, GUIDUtil.ProjectLevelGUIDType.Project);
#else
                    ifcProjectGuid = ExporterIFCUtils.CreateProjectLevelGUID(linkedDocument, IFCProjectLevelGUIDType.Project);
#endif
                }
                
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

    }
}
