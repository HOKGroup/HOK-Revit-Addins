using System.Collections.ObjectModel;
using Autodesk.Revit.DB;
using HOK.MissionControl.GroupsManager.Utilities;

namespace HOK.MissionControl.GroupsManager
{
    public class GroupsManagerModel
    {
        private Document Doc { get; set; }

        public GroupsManagerModel(Document doc)
        {
            Doc = doc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<GroupTypeWrapper> CollectGroups()
        {
            return new ObservableCollection<GroupTypeWrapper>();
        }
    }
}
