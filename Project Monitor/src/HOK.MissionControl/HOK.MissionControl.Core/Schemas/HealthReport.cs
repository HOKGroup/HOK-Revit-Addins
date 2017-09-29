using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HOK.MissionControl.Core.Schemas
{
    /// <summary>
    /// Event Object stored for each OnOpened/OnSynched events.
    /// </summary>
    public class WorksetEvent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string user { get; set; } = "";
        public double opened { get; set; } = 0;
        public double closed { get; set; } = 0;
        public DateTime createdOn { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Individual Workset object stored inside WorksetItem.
    /// </summary>
    public class Item
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string name { get; set; }
        public double count { get; set; }
    }

    /// <summary>
    /// Workset contained stored for each OnClosed event.
    /// </summary>
    public class WorksetItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public List<Item> worksets { get; set; }
    }

    /// <summary>
    /// View Stats Data Collection
    /// </summary>
    public class ViewStat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public int totalViews { get; set; }
        public int totalSheets { get; set; }
        public int totalSchedules { get; set; }
        public int viewsOnSheet { get; set; }
        public int viewsOnSheetWithTemplate { get; set; }
        public int schedulesOnSheet { get; set; }
        public int unclippedViews { get; set; }
        public DateTime createdOn { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Imported DWG File info.
    /// </summary>
    public class DwgFileInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string name { get; set; }
        public int elementId { get; set; }
        public int instances { get; set; }
        public bool isViewSpecific { get; set; }
        public bool isLinked { get; set; }
    }

    /// <summary>
    /// Link Stats Data Collection.
    /// </summary>
    public class LinkStat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public int totalImportedDwg { get; set; }
        public List<DwgFileInfo> importedDwgFiles { get; set; }
        public int unusedLinkedImages { get; set; }
        public int totalDwgStyles { get; set; }
        public int totalImportedStyles { get; set; }
        public int totalLinkedModels { get; set; }
        public int totalLinkedDwg { get; set; }
        public DateTime createdOn { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Task object added to Family.
    /// </summary>
    [DataContract]
    public class FamilyTask
    {
        //private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [DataMember(Name = "_id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string name { get; set; }

        [DataMember(Name = "assignedTo")]
        public string assignedTo { get; set; }

        [DataMember(Name = "message")]
        public string message { get; set; }

        [DataMember(Name = "submittedBy")]
        public string submittedBy { get; set; }

        [DataMember(Name = "completedBy")]
        public string completedBy { get; set; }

        //[IgnoreDataMember]
        [DataMember(Name = "submittedOn")]
        public DateTime? submittedOn { get; set; }

        //[DataMember(Name = "submittedOn")]
        //private long submittedOnTicks {
        //    get { return (long) (submittedOn - unixEpoch).TotalMilliseconds; }
        //    set { submittedOn = unixEpoch.AddMilliseconds(value); }
        //}

        //[IgnoreDataMember]
        [DataMember(Name = "completedOn")]
        public DateTime? completedOn { get; set; }

        //[DataMember(Name = "completedOn")]
        //private long completedOnTicks
        //{
        //    get
        //    {
        //        if (completedOn.HasValue)
        //        {
        //            return (long)(completedOn.Value - unixEpoch).TotalMilliseconds;
        //        }
        //        // completedOn is null, return something reasonable
        //        return long.MinValue;
        //    }
        //    //set
        //    //{
        //    //    if (value != long.MinValue)
        //    //    {
        //    //        completedOn = unixEpoch.AddMilliseconds(value);
        //    //    }
        //    //    else
        //    //    {
        //    //        completedOn = null;
        //    //    }
        //    //}
        //}
    }

    /// <summary>
    /// Individual Family object data.
    /// </summary>
    public class FamilyItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [DataMember(Name = "_id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string name { get; set; }

        [DataMember(Name = "isNameVerified")]
        public bool isNameVerified { get; set; }

        [DataMember(Name = "size")]
        public string size { get; set; }

        [DataMember(Name = "isSizeVerified")]
        public bool isSizeVerified { get; set; }

        [DataMember(Name = "sizeValue")]
        public long sizeValue { get; set; }

        [DataMember(Name = "instances")]
        public int instances { get; set; }

        [DataMember(Name = "isInstancesVerified")]
        public bool isInstancesVerified { get; set; }

        [DataMember(Name = "elementId")]
        public int elementId { get; set; }

        [DataMember(Name = "refPlaneCount")]
        public int refPlaneCount { get; set; }

        [DataMember(Name = "isRefPlaneCountVerified")]
        public bool isRefPlaneCountVerified { get; set; }

        [DataMember(Name = "arrayCount")]
        public int arrayCount { get; set; }

        [DataMember(Name = "isArrayCountVerified")]
        public bool isArrayCountVerified { get; set; }

        [DataMember(Name = "voidCount")]
        public int voidCount { get; set; }

        [DataMember(Name = "isVoidCountVerified")]
        public bool isVoidCountVerified { get; set; }

        [DataMember(Name = "nestedFamilyCount")]
        public int nestedFamilyCount { get; set; }

        [DataMember(Name = "isNestedFamilyCountVerified")]
        public bool isNestedFamilyCountVerified { get; set; }

        [DataMember(Name = "parametersCount")]
        public int parametersCount { get; set; }

        [DataMember(Name = "isParametersCountVerified")]
        public bool isParametersCountVerified { get; set; }

        [DataMember(Name = "isFailingChecks")]
        public bool isFailingChecks { get; set; }

        [DataMember(Name = "isDeleted")]
        public bool isDeleted { get; set; }

        [DataMember(Name = "tasks")]
        public List<FamilyTask> tasks { get; set; } = new List<FamilyTask>();
    }

    /// <summary>
    /// Summarized Family stats for Dashbord generation.
    /// </summary>
    public class FamilyStat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [DataMember(Name = "_id")]
        public string Id { get; set; }

        [DataMember(Name = "centralPath")]
        public string centralPath { get; set; }

        [DataMember(Name = "totalFamilies")]
        public int totalFamilies { get; set; }

        [DataMember(Name = "unusedFamilies")]
        public int unusedFamilies { get; set; }

        [DataMember(Name = "oversizedFamilies")]
        public int oversizedFamilies { get; set; }

        [DataMember(Name = "inPlaceFamilies")]
        public int inPlaceFamilies { get; set; }

        [DataMember(Name = "createdBy")]
        public string createdBy { get; set; }

        [DataMember(Name = "createdOn")]
        public DateTime? createdOn { get; set; }

        [DataMember(Name = "families")]
        public List<FamilyItem> families { get; set; } = new List<FamilyItem>();
    }

    /// <summary>
    /// Model size data, model session event data.
    /// </summary>
    public class EventData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public long value { get; set; }
        public DateTime createdOn { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Session monitoring data.
    /// </summary>
    public class SessionInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string user { get; set; }
        public DateTime? from { get; set; }
        public DateTime? to { get; set; }
        public List<DateTime> synched { get; set; } = new List<DateTime>();
        public DateTime createdOn { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Main MongoDB Collection for Health Report Data.
    /// </summary>
    public class HealthReportData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string centralPath { get; set; }
        public List<WorksetEvent> onOpened { get; set; } = new List<WorksetEvent>();
        public List<WorksetEvent> onSynched { get; set; } = new List<WorksetEvent>();
        public List<WorksetItem> itemCount { get; set; } = new List<WorksetItem>();
        public List<ViewStat> viewStats { get; set; } = new List<ViewStat>();
        public List<LinkStat> linkStats { get; set; } = new List<LinkStat>();
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string familyStats { get; set; }
        public List<EventData> openTimes { get; set; } = new List<EventData>();
        public List<EventData> synchTimes { get; set; } = new List<EventData>();
        public List<EventData> modelSizes { get; set; } = new List<EventData>();
        public List<SessionInfo> sessionLogs { get; set; } = new List<SessionInfo>();
    }
}
