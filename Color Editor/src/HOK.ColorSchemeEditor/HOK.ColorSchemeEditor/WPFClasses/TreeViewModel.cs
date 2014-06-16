using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Autodesk.Revit.DB;
using System.Windows;

namespace HOK.ColorSchemeEditor.WPFClasses
{
    public class TreeViewModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public object Tag { get; set; }
        public List<TreeViewModel> Children { get; set; }
        public bool IsInitiallySelected { get; set; }
        public bool? _isChecked = false;
        public TreeViewModel _parent;
       
        public TreeViewModel(CategoryInfo catInfo)
        {
            Name = catInfo.Name;
            Tag = catInfo;
            Children = new List<TreeViewModel>();
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
            if (updateChildren && _isChecked.HasValue) { Children.ForEach(c => c.SetIsChecked(_isChecked, true, false)); }
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

        public static List<TreeViewModel> SetTreeView(List<CategoryInfo> catInfoList, List<string> selectedCategories)
        {
            List<TreeViewModel> treeView = new List<TreeViewModel>();
            try
            {
                foreach (CategoryInfo catInfo in catInfoList)
                {
                    TreeViewModel categoryItem = new TreeViewModel(catInfo);
                    if (selectedCategories.Contains(catInfo.Name)) { categoryItem.IsChecked = true; }

                    if (catInfo.SubCategories.Count > 0)
                    {
                        foreach (CategoryInfo subInfo in catInfo.SubCategories)
                        {
                            TreeViewModel subCategoryItem = new TreeViewModel(subInfo);
                            if (selectedCategories.Contains(subInfo.Name)) { subCategoryItem.IsChecked = true; }

                            categoryItem.Children.Add(subCategoryItem);
                        }
                        categoryItem.Children.OrderBy(child => child.Name).ToList();
                    }
                    treeView.Add(categoryItem);
                }
                treeView.OrderBy(parent => parent.Name).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize the treeview.\n" + ex.Message, "Set TreeView By Categories", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return treeView;
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
