using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.MissionControl.Core.Schemas;

namespace HOK.MissionControl.Tools.Communicator.Messaging
{
    public class TaskUpdatedMessage
    {
        public FamilyTask Task { get; set; }
    }

    //public class FamilyUpdatedMessage
    //{
    //    public FamilyItem Family { get; set; }
    //}

    public class TaskDeletedMessage
    {
        public HashSet<string> DeletedIds { get; set; }
    }

    public class TaskAddedMessage
    {
        public int FamilyId { get; set; }
        public FamilyStat FamilyStat { get; set; }
    }
}
