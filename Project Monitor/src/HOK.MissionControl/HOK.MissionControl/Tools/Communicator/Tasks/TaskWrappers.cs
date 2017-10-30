using System.ComponentModel;
using HOK.MissionControl.Core.Schemas;

namespace HOK.MissionControl.Tools.Communicator.Tasks
{
    public abstract class TaskWrapper : INotifyPropertyChanged
    {
        private object _element;
        public virtual object Element
        {
            get { return _element; }
            set { _element = value; RaisePropertyChanged("Element"); }
        }

        private object _task;
        public virtual object Task
        {
            get { return _task; }
            set { _task = value; RaisePropertyChanged("Task"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
    }

    public class SheetTaskWrapper : TaskWrapper
    {
        public SheetTaskWrapper(SheetTask task)
        {
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
    }
}
