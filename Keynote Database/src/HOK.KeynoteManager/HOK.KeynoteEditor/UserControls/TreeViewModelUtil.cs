using HOK.Keynote.ClassModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.KeynoteEditor.UserControls
{
    public static class TreeViewModelUtil
    {
        public static ObservableCollection<TreeViewModel> SetTree(List<KeynoteInfo> keynoteInfoList)
        {
            ObservableCollection<TreeViewModel> treeView = new ObservableCollection<TreeViewModel>();
            try
            {
                var topNodeFound = from keynote in keynoteInfoList where string.IsNullOrEmpty(keynote.parentKey) select keynote;
                if (topNodeFound.Count() > 0)
                {
                    List<KeynoteInfo> topNodes = topNodeFound.ToList();
                    topNodes = topNodes.OrderBy(o => o.key).ToList();

                    foreach (KeynoteInfo kInfo in topNodes)
                    {
                        TreeViewModel tvm = new TreeViewModel(kInfo);
                        tvm.ChildrenItems = FindChildrenItems(tvm, keynoteInfoList);
                        treeView.Add(tvm);
                        tvm.Initialize();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize the treview.\n" + ex.Message, "SetTree", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return treeView;
        }

        private static ObservableCollection<TreeViewModel> FindChildrenItems(TreeViewModel tvm, List<KeynoteInfo> keynoteList)
        {
            ObservableCollection<TreeViewModel> childrenItems = new ObservableCollection<TreeViewModel>();
            try
            {
                var childFound = from keynote in keynoteList where keynote.parentKey == tvm.KeynoteItem.key select keynote;
                if (childFound.Count() > 0)
                {
                    List<KeynoteInfo> childList = childFound.OrderBy(o => o.key).ToList();
                    foreach (KeynoteInfo keynote in childList)
                    {
                        TreeViewModel childTvm = new TreeViewModel(keynote);
                        childTvm.ChildrenItems = FindChildrenItems(childTvm, keynoteList);
                        childrenItems.Add(childTvm);
                        childTvm.Initialize();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find children items.\n" + ex.Message, "Find Children Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return childrenItems;
        }
    }
}
