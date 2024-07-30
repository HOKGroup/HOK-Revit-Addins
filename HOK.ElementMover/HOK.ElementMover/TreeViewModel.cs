using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace HOK.ElementMover
{
    public enum TreeViewNodeType
    {
        None,
        Root,
        Category,
        Family,
        FamilyType,
        ElementMapping
    }

    public class TreeViewElementModel : INotifyPropertyChanged
    {
        public string Name { get; private set; }
        public object Tag { get; private set; }
        public bool IsInitiallySelected { get; private set; }
        public bool Matched { get; private set; }
        public TreeViewElementModel ParentNode { get; private set; }
        public List<TreeViewElementModel> ChildrenNodes { get; private set; }
        public TreeViewNodeType NodeType { get; private set; }
        public string ToolTip { get; private set; }
        public Visibility ToolTipVisibility { get; private set; }
        public bool? isChecked = false;

        public TreeViewElementModel(string nodeName)
        {
            Name = nodeName;
            Tag = null;
            Matched = true;
            ChildrenNodes = new List<TreeViewElementModel>();
            ToolTip = "";
        }

        void Initialize()
        {
            foreach (var child in ChildrenNodes)
            {
                child.ParentNode = this;
                child.Initialize();
            }
        }

        public bool? IsChecked
        {
            get { return isChecked; }
            set { SetIsChecked(value, true, true); }
        }

        public void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == isChecked) return;
            isChecked = value;
            if (updateChildren && isChecked.HasValue) { ChildrenNodes.ForEach(c => c.SetIsChecked(isChecked, true, false)); }
            if (updateParent && ParentNode != null) { ParentNode.VerifyCheckedState(); }
            NotifyPropertyChanged("IsChecked");
        }

        public void VerifyCheckedState()
        {
            bool? state = null;

            for (var i = 0; i < ChildrenNodes.Count; ++i)
            {
                var current = ChildrenNodes[i].IsChecked;
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

        public static List<TreeViewElementModel> SetTreeView(LinkedInstanceProperties lip)
        {
            var treeView = new List<TreeViewElementModel>();
            try
            {
                var customCategories = new string[] { "Rooms", "Levels", "Grids", "Scope Boxes" };

                var categories = from linkedElement in lip.LinkedElements.Values select linkedElement.CategoryName;
                if (categories.Count() > 0)
                {
                    var categoryNames = categories.Distinct().ToList(); categoryNames.Sort();
                    foreach (var categoryName in categoryNames)
                    {
                        var categoryNode = new TreeViewElementModel(categoryName);
                        categoryNode.NodeType = TreeViewNodeType.Category;
                        categoryNode.ToolTipVisibility = Visibility.Hidden;
                        treeView.Add(categoryNode);

                        if (customCategories.Contains(categoryName))
                        {
                            var elements = from linkedElement in lip.LinkedElements.Values
                                        where linkedElement.CategoryName == categoryName
                                        select linkedElement;

                            var linkedElements = elements.OrderBy(o => o.LinkDisplayText).ToList();
                            foreach (var linkInfo in linkedElements)
                            {
                                var elementNode = new TreeViewElementModel(linkInfo.LinkDisplayText);
                                elementNode.Tag = linkInfo;
                                elementNode.NodeType = TreeViewNodeType.ElementMapping;
                                elementNode.Matched = linkInfo.Matched;
                                elementNode.ToolTip = linkInfo.ToolTipText;
                                elementNode.ToolTipVisibility = Visibility.Visible;

                                categoryNode.ChildrenNodes.Add(elementNode);
                            }
                            continue;
                        }

                        var families = from linkedElement in lip.LinkedElements.Values 
                                       where linkedElement.CategoryName == categoryName 
                                       select linkedElement.FamilyName;
                        if (families.Count() > 0)
                        {
                            var familyNames = families.Distinct().ToList(); familyNames.Sort();
                            foreach (var familyName in familyNames)
                            {
                                var familyNode = new TreeViewElementModel(familyName);
                                familyNode.NodeType = TreeViewNodeType.Family;
                                familyNode.ToolTipVisibility = Visibility.Hidden;
                                categoryNode.ChildrenNodes.Add(familyNode);

                                var familyTypes = from linkedElement in lip.LinkedElements.Values
                                                  where linkedElement.CategoryName == categoryName && linkedElement.FamilyName == familyName
                                                  select linkedElement.FamilyTypeName;
                                if (familyTypes.Count() > 0)
                                {
                                    var familyTypeNames = familyTypes.Distinct().ToList(); familyTypeNames.Sort();
                                    foreach (var familyTypeName in familyTypeNames)
                                    {
                                        var familyTypeNode = new TreeViewElementModel(familyTypeName);
                                        familyTypeNode.NodeType = TreeViewNodeType.FamilyType;
                                        familyTypeNode.ToolTipVisibility = Visibility.Hidden;
                                        familyNode.ChildrenNodes.Add(familyTypeNode);

                                        var mappings = from linkedElement in lip.LinkedElements.Values
                                                       where linkedElement.CategoryName == categoryName && linkedElement.FamilyName == familyName && linkedElement.FamilyTypeName == familyTypeName
                                                       select linkedElement;
                                        if (mappings.Count() > 0)
                                        {
                                            var linkedElements = mappings.OrderBy(o => o.LinkDisplayText).ToList();
                                            foreach (var linkInfo in linkedElements)
                                            {
                                                var elementNode = new TreeViewElementModel(linkInfo.LinkDisplayText);
                                                elementNode.Tag = linkInfo;
                                                elementNode.NodeType = TreeViewNodeType.ElementMapping;
                                                elementNode.Matched = linkInfo.Matched;
                                                elementNode.ToolTip = linkInfo.ToolTipText;
                                                elementNode.ToolTipVisibility = Visibility.Visible;

                                                familyTypeNode.ChildrenNodes.Add(elementNode);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        categoryNode.Initialize();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set the tree view.\n"+ex.Message, "Set Tree View", MessageBoxButton.OK, MessageBoxImage.Warning);
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

    public class TreeViewFamilyModel : INotifyPropertyChanged
    {
        public string Name { get; private set; }
        public object Tag { get; private set; }
        public bool IsInitiallySelected { get; private set; }
        public TreeViewFamilyModel ParentNode { get; private set; }
        public List<TreeViewFamilyModel> ChildrenNodes { get; private set; }
        public TreeViewNodeType NodeType { get; private set; }
        public bool? isChecked = false;

        public TreeViewFamilyModel(string nodeName)
        {
            Name = nodeName;
            Tag = null;
            ChildrenNodes = new List<TreeViewFamilyModel>();
        }

        public void Initialize()
        {
            foreach (var child in ChildrenNodes)
            {
                child.ParentNode = this;
                child.Initialize();
            }
        }

        public bool? IsChecked
        {
            get { return isChecked; }
            set { SetIsChecked(value, true, true); }
        }

        public void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == isChecked) return;
            isChecked = value;
            if (updateChildren && isChecked.HasValue) { ChildrenNodes.ForEach(c => c.SetIsChecked(isChecked, true, false)); }
            if (updateParent && ParentNode != null) { ParentNode.VerifyCheckedState(); }
            NotifyPropertyChanged("IsChecked");
        }

        public void VerifyCheckedState()
        {
            bool? state = null;

            for (var i = 0; i < ChildrenNodes.Count; ++i)
            {
                var current = ChildrenNodes[i].IsChecked;
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

        public static List<TreeViewFamilyModel> SetTreeView(LinkedInstanceProperties lip)
        {
            var treeView = new List<TreeViewFamilyModel>();
            try
            {
                var categories = from linkedFamily in lip.LinkedFamilies.Values select linkedFamily.CategoryName;
                if (categories.Count() > 0)
                {
                    var categoryNames = categories.Distinct().ToList(); categoryNames.Sort();
                    foreach (var categoryName in categoryNames)
                    {
                        var categoryNode = new TreeViewFamilyModel(categoryName);
                        categoryNode.NodeType = TreeViewNodeType.Category;
                        treeView.Add(categoryNode);

                        var families = from linkedFamily in lip.LinkedFamilies.Values
                                       where linkedFamily.CategoryName == categoryName
                                       select linkedFamily.SourceFamilyName;
                        if (families.Count() > 0)
                        {
                            var familyNames = families.Distinct().ToList(); familyNames.Sort();
                            foreach (var familyName in familyNames)
                            {
                                var linkedFamilies = from linkedFamily in lip.LinkedFamilies.Values
                                                  where linkedFamily.CategoryName == categoryName && linkedFamily.SourceFamilyName == familyName
                                                  select linkedFamily;

                                if (linkedFamilies.Count() > 0)
                                {
                                    var familyInfo = linkedFamilies.First();

                                    var familyNode = new TreeViewFamilyModel(familyInfo.SourceFamilyName + " : " + familyInfo.TargetFamilyName);
                                    familyNode.NodeType = TreeViewNodeType.Family;
                                    categoryNode.ChildrenNodes.Add(familyNode);

                                    var familyTypes = linkedFamilies.OrderBy(o => o.SourceTypeName).ToList();
                                    foreach (var linkedInfo in familyTypes)
                                    {
                                        var familyTypeNode = new TreeViewFamilyModel(linkedInfo.SourceTypeName + " : " + linkedInfo.TargetTypeName);
                                        familyTypeNode.NodeType = TreeViewNodeType.FamilyType;
                                        familyTypeNode.Tag = linkedInfo;
                                        familyNode.ChildrenNodes.Add(familyTypeNode);

                                    }
                                }
                            }
                        }
                        categoryNode.Initialize();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set the tree view.\n" + ex.Message, "Set Tree View", MessageBoxButton.OK, MessageBoxImage.Warning);
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
