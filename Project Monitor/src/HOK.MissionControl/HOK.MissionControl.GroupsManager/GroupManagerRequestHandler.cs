#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using GalaSoft.MvvmLight.Messaging;
using HOK.Core.Utilities;
using HOK.Core.WpfUtilities;
using HOK.MissionControl.GroupsManager.Utilities;

#endregion

namespace HOK.MissionControl.GroupsManager
{
    public class GroupManagerRequestHandler : IExternalEventHandler
    {
        public GroupManagerRequest Request { get; set; } = new GroupManagerRequest();
        public object Arg1 { get; set; }

        public string GetName()
        {
            return "Task External Event";
        }

        public void Execute(UIApplication app)
        {
            try
            {
                switch (Request.Take())
                {
                    case GroupRequestType.None:
                    {
                        return;
                    }
                    case GroupRequestType.Delete:
                        Delete(app);
                        break;
                    case GroupRequestType.Ungroup:
                        Ungroup(app);
                        break;
                    case GroupRequestType.FindView:
                        FindView(app);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        private void FindView(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;
            var gt = (GroupTypeWrapper) Arg1;
            if (!gt.Instances.Any()) return;

            var gi = (Group) doc.GetElement(gt.Instances.First());
            View view;
            if (gi.OwnerViewId != ElementId.InvalidElementId)
            {
                // detail group
                view = doc.GetElement(gi.OwnerViewId) as View;
            }
            else if (gi.LevelId != ElementId.InvalidElementId)
            {
                // model group
                // (Konrad) It's possible that Group was placed on a level with offset
                // we need to find the level/view closest to the actual height
                var levelId = gi.get_Parameter(BuiltInParameter.GROUP_LEVEL).AsElementId();
                var offset = gi.get_Parameter(BuiltInParameter.GROUP_OFFSET_FROM_LEVEL).AsDouble();
                var levels = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .WhereElementIsNotElementType()
                    .Cast<Level>()
                    .ToDictionary(x => x.Id, x => x.Elevation);
                var trueHeight = ((Level) doc.GetElement(levelId)).Elevation + offset;
                var closest =
                    levels.Values.Aggregate((x, y) => Math.Abs(x - trueHeight) < Math.Abs(y - trueHeight) ? x : y);
                var closestLevel = levels.FirstOrDefault(x => Math.Abs(x.Value - closest) < 0.01).Key;
                view = new FilteredElementCollector(doc)
                    .OfClass(typeof(View))
                    .WhereElementIsNotElementType()
                    .Cast<View>()
                    .FirstOrDefault(x => x.GenLevel?.Id == closestLevel);
            }
            else
            {
                // unknown
                return;
            }

            if (view == null) return;

            app.ActiveUIDocument.ActiveView = view;
            app.ActiveUIDocument.ShowElements(gi);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public void Delete(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;
            var groups = (List<GroupTypeWrapper>)Arg1;
            var deleted = new List<GroupTypeWrapper>();

            using (var trans = new Transaction(doc, "Delete Groups"))
            {
                trans.Start();
                StatusBarManager.InitializeProgress("Deleting Groups...", groups.Count);

                foreach (var g in groups)
                {
                    StatusBarManager.StepForward();
                    try
                    {
                        doc.Delete(g.Id);
                        deleted.Add(g);
                    }
                    catch (Exception e)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                    }
                }

                StatusBarManager.FinalizeProgress();
                trans.Commit();
            }

            Messenger.Default.Send(new GroupsDeleted { Groups = deleted });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public void Ungroup(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;
            var groups = (List<GroupTypeWrapper>) Arg1;
            var deleted = new List<GroupTypeWrapper>();

            using (var trans = new Transaction(doc, "Ungroup Groups"))
            {
                trans.Start();
                StatusBarManager.InitializeProgress("Ungrouping Groups...", groups.Count);

                foreach (var g in groups)
                {
                    StatusBarManager.StepForward();
                    var ungrouped = 0;
                    foreach (var id in g.Instances)
                    {
                        var instance = (Group)doc.GetElement(id);
                        if (instance == null) continue;

                        try
                        {
                            instance.UngroupMembers();
                            ungrouped++;
                        }
                        catch (Exception e)
                        {
                            Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                        }
                    }

                    if (ungrouped != g.Instances.Count) continue;

                    try
                    {
                        doc.Delete(g.Id);
                        deleted.Add(g);
                    }
                    catch (Exception e)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                    }
                }

                StatusBarManager.FinalizeProgress();
                trans.Commit();
            }

            Messenger.Default.Send(new GroupsDeleted { Groups = deleted });
        }
    }

    public class GroupManagerRequest
    {
        private int _request = (int)GroupRequestType.None;

        public GroupRequestType Take()
        {
            return (GroupRequestType)Interlocked.Exchange(ref _request, (int)GroupRequestType.None);
        }

        public void Make(GroupRequestType request)
        {
            Interlocked.Exchange(ref _request, (int)request);
        }
    }

    public enum GroupRequestType
    {
        None,
        Delete,
        Ungroup,
        FindView
    }
}
