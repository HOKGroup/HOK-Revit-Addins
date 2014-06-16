using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Google.Documents;

namespace HOK.ModelManager.ReplicateViews
{
    public enum LinkStatus
    {
        Linked=0,
        NewItem=1,
        MissingFromSource=2,
        MissingFromRecipient=3,
        None=4
    }

    public class TreeViewModel:INotifyPropertyChanged
    {
        public string Name { get; private set; }
        public object Tag { get; private set; }
        public string ToolTip { get; private set; }
        public List<TreeViewModel> Children { get; private set; }
        public bool IsInitiallySelected { get; private set; }
        public bool? _isChecked = false;
        public TreeViewModel _parent;

        public Brush TextColor { get; set; }
        public LinkStatus Status { get; set; }
        public bool IsEnabled { get; set; }
        
        public TreeViewModel(string name)
        {
            Name = name;
            SetLinkStatus(LinkStatus.None);
            IsEnabled = true;
            Children = new List<TreeViewModel>();
        }

        public TreeViewModel(string name, LinkStatus linkStatus)
        {
            Name = name;
            SetLinkStatus(linkStatus);
            IsEnabled = true;
            Children = new List<TreeViewModel>();
        }

        public void SetLinkStatus(LinkStatus status)
        {
            Status = status;
            switch (Status)
            {
                case LinkStatus.Linked:
                    TextColor = new SolidColorBrush(Colors.Gray);
                    break;
                case LinkStatus.NewItem:
                    TextColor = new SolidColorBrush(Colors.Green);
                    break;
                case LinkStatus.MissingFromSource:
                    TextColor = new SolidColorBrush(Colors.Red);
                    break;
                case LinkStatus.MissingFromRecipient:
                    TextColor = new SolidColorBrush(Colors.Red);
                    break;
                default:
                    TextColor = new SolidColorBrush(Colors.Black);
                    break;
            }
        }
       
        public bool? IsChecked
        {
            get { return _isChecked; }
            set { SetIsChecked(value, true, true); }
        }

        public void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked) return;
            _isChecked = value;
            if (updateChildren && _isChecked.HasValue) { Children.ForEach(c=>c.SetIsChecked(_isChecked, true, false));}
            if (updateParent && _parent != null) { _parent.VerifyCheckedState(); }
            NotifyPropertyChanged("IsChecked");
        }

        public void VerifyCheckedState()
        {
            bool? state = null;

            for (int i = 0; i < Children.Count; ++i)
            {
                bool? current = Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }

            SetIsChecked(state, false, true);
        }

        void Initialize()
        {
            foreach (TreeViewModel child in Children)
            {
                child._parent = this;
                child.Initialize();
            }
        }

        public static List<TreeViewModel> SetTreeByViewType(string docName, Dictionary<int, ViewProperties> selectedViews, List<string> viewTypeNames)
        {
            List<TreeViewModel> treeView = new List<TreeViewModel>();
            try
            {
                TreeViewModel tv = new TreeViewModel("Views(" + docName + ")");
                tv.Tag = docName;
                treeView.Add(tv);

                foreach (string viewTypeName in viewTypeNames)
                {
                    var viewItems = from viewItem in selectedViews.Values where viewItem.ViewTypeName == viewTypeName select viewItem;
                    if (viewItems.Count() > 0)
                    {
                        TreeViewModel viewTypeItem = new TreeViewModel("Drafting Views (" + viewTypeName + ")");
                        viewTypeItem.Tag = viewTypeName;
                        tv.Children.Add(viewTypeItem);

                        foreach (ViewProperties vp in viewItems)
                        {
                            TreeViewModel viewItem = new TreeViewModel(vp.ViewName, vp.Status);
                            viewItem.Tag = vp.ViewId;
                            switch (vp.Status)
                            {
                                case LinkStatus.Linked:
                                    if (null != vp.LinkedView)
                                    {
                                        viewItem.ToolTip = "Linked Id: " + vp.LinkedView.ViewId.ToString() + " Name: " + vp.LinkedView.ViewName;
                                    }
                                    break;
                                case LinkStatus.MissingFromRecipient:
                                    viewItem.ToolTip = "Missing From Recipient";
                                    break;
                                case LinkStatus.MissingFromSource:
                                    viewItem.ToolTip = "Missing From Source";
                                    break;
                                case LinkStatus.None:
                                    viewItem.ToolTip = "New Item";
                                    break;
                            }
                           
                            viewTypeItem.Children.Add(viewItem);
                        }
                        viewTypeItem.Children.OrderBy(child => child.Name).ToList();
                    }
                }
                tv.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(docName+": Failed to initialize the treeview.\n"+ex.Message, "SetTreeByViewType", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return treeView;
        }

        public static List<TreeViewModel> SetTreeBySheet(string docName, Dictionary<int, ViewProperties> selectedViews, List<string> sheetNumbers)
        {
            List<TreeViewModel> treeView = new List<TreeViewModel>();
            try
            {
                TreeViewModel tv = new TreeViewModel("Sheets (" + docName + ")");
                tv.Tag = docName;
                treeView.Add(tv);

                foreach (string sheetNumber in sheetNumbers)
                {
                    var viewItems = from viewItem in selectedViews.Values where viewItem.SheetNumber == sheetNumber select viewItem;
                    if (viewItems.Count() > 0)
                    {
                        ViewProperties firstProperties = viewItems.First();
                        string sheetName = firstProperties.SheetName;

                        TreeViewModel sheetItem = new TreeViewModel(sheetNumber + " - " + sheetName);
                        sheetItem.Tag = sheetNumber;
                        tv.Children.Add(sheetItem);

                        foreach (ViewProperties vp in viewItems)
                        {
                            TreeViewModel viewItem = new TreeViewModel(vp.ViewName, vp.Status);
                            viewItem.Tag = vp.ViewId;
                            switch (vp.Status)
                            {
                                case LinkStatus.Linked:
                                    if (null != vp.LinkedView)
                                    {
                                        viewItem.ToolTip = "Linked Id: " + vp.LinkedView.ViewId.ToString() + " Name: " + vp.LinkedView.ViewName;
                                    }
                                    break;
                                case LinkStatus.MissingFromRecipient:
                                    viewItem.ToolTip = "Missing From Recipient";
                                    break;
                                case LinkStatus.MissingFromSource:
                                    viewItem.ToolTip = "Missing From Source";
                                    break;
                                case LinkStatus.None:
                                    viewItem.ToolTip = "New Item";
                                    break;
                            }
                            sheetItem.Children.Add(viewItem);
                        }
                        sheetItem.Children.OrderBy(child => child.Name).ToList();
                    }
                }
                tv.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(docName+": Failed to initialize the treview.\n"+ex.Message, "SetTreeBySheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return treeView;
        }

        public static List<string> GetTree()
        {
            List<string> selected = new List<string>();

            //select = recursive method to check each tree view item for selection (if required)

            return selected;

            //***********************************************************
            //From your window capture selected your treeview control like:   TreeViewModel root = (TreeViewModel)TreeViewControl.Items[0];
            //                                                                List<string> selected = new List<string>(TreeViewModel.GetTree());
            //***********************************************************
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
