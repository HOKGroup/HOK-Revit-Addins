using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Schemas.Sheets;

namespace HOK.MissionControl.Tools.Communicator.Tasks
{
    public abstract class TaskWrapper
    {
        public virtual object Element { get; set; }
        public virtual object Task { get; set; }
    }

    public class SheetTaskWrapper : TaskWrapper
    {
        public SheetTaskWrapper(SheetItem element, SheetTask task)
        {
            Element = element;
            Task = task;
        }

        public override bool Equals(object obj)
        {
            var item = obj as SheetTaskWrapper;
            return item != null && Task.Equals(item.Task);
        }

        public override int GetHashCode()
        {
            return Task.GetHashCode();
        }
    }

    public class FamilyTaskWrapper : TaskWrapper
    {
        public FamilyTaskWrapper(FamilyItem element, FamilyTask task)
        {
            Element = element;
            Task = task;
        }

        public override bool Equals(object obj)
        {
            var item = obj as FamilyTaskWrapper;
            return item != null && Task.Equals(item.Task);
        }

        public override int GetHashCode()
        {
            return Task.GetHashCode();
        }
    }
}
