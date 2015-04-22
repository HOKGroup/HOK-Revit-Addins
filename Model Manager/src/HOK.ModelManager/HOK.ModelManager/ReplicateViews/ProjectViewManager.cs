
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using HOK.ModelManager.GoogleDocs;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using System;
using System.Windows;
using System.Threading;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace HOK.ModelManager.ReplicateViews
{
    public class ProjectViewManager
    {
        private UIApplication m_app;
        private bool verifiedUser = false;
        private Dictionary<string/*docId*/, ModelInfo> modelInfoDictionary = new Dictionary<string, ModelInfo>();

        private List<string> viewTypeNames = new List<string>();
        private List<string> sheetNumbers = new List<string>();

        public Dictionary<string, ModelInfo> ModelInfoDictionary { get { return modelInfoDictionary; } set { modelInfoDictionary = value; } }
       
        public bool VerifiedUser { get { return verifiedUser; } set { verifiedUser = value; } }

        public ProjectViewManager(UIApplication uiapp)
        {
            m_app = uiapp;

            GetModelInfo();
            viewTypeNames.Sort();
            sheetNumbers.Sort();
            sheetNumbers.Add("None");

        }

       
        private void GetModelInfo()
        {
            try
            {
                foreach (Document doc in m_app.Application.Documents)
                {
                    if (!string.IsNullOrEmpty(doc.Title) && !doc.IsFamilyDocument)
                    {
                        ModelInfo modelInfo = new ModelInfo(doc);
                        if (!modelInfoDictionary.ContainsKey(modelInfo.RevitDocumentID))
                        {
                            modelInfo.GoogleSheetID = GoogleDriveUtil.GetGoogleSheetId(modelInfo);
                            if (!string.IsNullOrEmpty(modelInfo.GoogleSheetID))
                            {
                                verifiedUser = true;
                                modelInfo.LinkInfoCollection = GoogleSheetUtil.GetLinkInfo(modelInfo.GoogleSheetID);
                            }
                            modelInfo.ViewDictionary = GetDraftingViewInfo(doc);
                            modelInfoDictionary.Add(modelInfo.RevitDocumentID, modelInfo);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get the model information.\n"+ex.Message, "Get Model Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private Dictionary<int, ViewProperties> GetDraftingViewInfo(Document doc)
        {
            Dictionary<int, ViewProperties> viewDictionary = new Dictionary<int, ViewProperties>();
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<ViewDrafting> viewDraftingList = collector.OfClass(typeof(ViewDrafting)).WhereElementIsNotElementType().ToElements().Cast<ViewDrafting>().ToList();
               
                foreach (ViewDrafting view in viewDraftingList)
                {
                    if (view.ViewType != ViewType.DraftingView) { continue; }
                    if (!viewDictionary.ContainsKey(view.Id.IntegerValue))
                    {
                        ViewProperties vp = new ViewProperties(doc, view);
                        viewDictionary.Add(vp.ViewId, vp);
                        if (!viewTypeNames.Contains(vp.ViewTypeName))
                        {
                            viewTypeNames.Add(vp.ViewTypeName);
                        }
                        if (vp.IsOnSheet && !string.IsNullOrEmpty(vp.SheetNumber))
                        {
                            if (!sheetNumbers.Contains(vp.SheetNumber))
                            {
                                sheetNumbers.Add(vp.SheetNumber);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get the information of drafting views.\n"+ex.Message, "Get Drafting Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return viewDictionary;
        }

        public void RefreshTreeView(ModelInfo sourceModel, ModelInfo recipientModel, TreeView sourceView, TreeView recipientView, TreeViewSortBy sortBy, bool createLinks)
        {
            try
            {
                ViewMapClass viewMap = new ViewMapClass(sourceModel, recipientModel, createLinks);
                if (null != viewMap)
                {
                    sourceView.ItemsSource = null;
                    recipientView.ItemsSource = null;

                    if (sortBy == TreeViewSortBy.Sheet)
                    {
                        sourceView.ItemsSource = TreeViewModel.SetTreeBySheet(sourceModel.DocTitle, viewMap.SourceViews, sheetNumbers);
                        recipientView.ItemsSource = TreeViewModel.SetTreeBySheet(recipientModel.DocTitle, viewMap.RecipientViews, sheetNumbers);
                    }
                    else if (sortBy == TreeViewSortBy.ViewType)
                    {
                        sourceView.ItemsSource = TreeViewModel.SetTreeByViewType(sourceModel.DocTitle, viewMap.SourceViews, viewTypeNames);
                        recipientView.ItemsSource = TreeViewModel.SetTreeByViewType(recipientModel.DocTitle, viewMap.RecipientViews, viewTypeNames);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to refresh the tree view.\nSource Model:"+sourceModel.DocTitle+"\nRecipient Model:"+recipientModel.DocTitle+"\n" + ex.Message, "Refresh Tree View", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        public void RefreshTreeViewBySelection(ModelInfo sourceModel, ModelInfo recipientModel, TreeView sourceView, TreeView recipientView, TreeViewSortBy sortBy, string filterString, bool createLinks)
        {
            try
            {
                ViewMapClass viewMap = new ViewMapClass(sourceModel, recipientModel, createLinks);
                if (null != viewMap)
                {
                    List<string> strList = new List<string>();
                    strList.Add(filterString);

                    recipientView.ItemsSource = null;

                    if (sortBy == TreeViewSortBy.Sheet)
                    {
                        recipientView.ItemsSource = TreeViewModel.SetTreeBySheet(recipientModel.DocTitle, viewMap.RecipientViews, strList);
                    }
                    else if (sortBy == TreeViewSortBy.ViewType)
                    {
                        recipientView.ItemsSource = TreeViewModel.SetTreeByViewType(recipientModel.DocTitle, viewMap.RecipientViews, strList);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show( "Failed to refresh the tree view by "+filterString +"\n"+ ex.Message, "Refresh Tree View By Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
   
        public bool UpdateDraftingViews(ModelInfo sourceInfo, ModelInfo recipientInfo, TreeView treeViewSource, TreeView treeViewRecipient, bool createSheet, bool createLinks,TextBlock statusLable, ProgressBar progressBar)
        {
            bool result = false;
            try
            {
                List<int> sourceViewIds = new List<int>();
                List<PreviewMap> previewMapList = new List<PreviewMap>();

                ViewMapClass vmc = new ViewMapClass(sourceInfo, recipientInfo, createLinks);
                Dictionary<int, ViewProperties> sourceViews = vmc.SourceViews;
                Dictionary<int, ViewProperties> recipientViews = vmc.RecipientViews;

                List<TreeViewModel> treeviewModels = treeViewSource.ItemsSource as List<TreeViewModel>;
                foreach (TreeViewModel rootNode in treeviewModels) 
                {
                    foreach(TreeViewModel secondNode in rootNode.Children)
                    {
                        foreach(TreeViewModel viewNode in secondNode.Children)
                        {
                            if (viewNode.IsChecked == true)
                            {
                                int viewId = 0;
                                if (int.TryParse(viewNode.Tag.ToString(), out viewId))
                                {
                                    if (sourceViews.ContainsKey(viewId) && !sourceViewIds.Contains(viewId))
                                    {
                                        ViewProperties viewSource = sourceViews[viewId];
                                        if (viewSource.DependantViews.Count > 0)
                                        {
                                            foreach (int dependentId in viewSource.DependantViews.Keys)
                                            {
                                                if (!sourceViewIds.Contains(dependentId))
                                                {
                                                    ViewProperties dependentSource = viewSource.DependantViews[dependentId];
                                                    ViewProperties dependentRecipient = null;
                                                    LinkInfo dependantlinkInfo = new LinkInfo();
                                                    if (null != dependentSource.LinkedView)
                                                    {
                                                        dependentRecipient = dependentSource.LinkedView;
                                                        var linkInfoList = from info in vmc.LinkInfoList where info.SourceItemId == dependentId && info.DestItemId == dependentRecipient.ViewId select info;
                                                        if (linkInfoList.Count() > 0)
                                                        {
                                                            dependantlinkInfo = linkInfoList.First();
                                                        }
                                                    }

                                                    PreviewMap dependantView = new PreviewMap();
                                                    dependantView.SourceModelInfo = sourceInfo;
                                                    dependantView.RecipientModelInfo = recipientInfo;
                                                    dependantView.SourceViewProperties = dependentSource;
                                                    dependantView.RecipientViewProperties = dependentRecipient;
                                                    dependantView.ViewLinkInfo = dependantlinkInfo;
                                                    dependantView.IsEnabled = true;

                                                    sourceViewIds.Add(dependentId);
                                                    previewMapList.Add(dependantView);
                                                }
                                            }
                                        }
                                        
                                        ViewProperties viewRecipient = null;
                                        LinkInfo linkInfo = new LinkInfo();
                                        if (null != viewSource.LinkedView)
                                        {
                                            viewRecipient = viewSource.LinkedView;
                                            var linkInfoList = from info in vmc.LinkInfoList where info.SourceItemId == viewId && info.DestItemId == viewRecipient.ViewId && info.ItemStatus != LinkItemStatus.Deleted select info;
                                            if (linkInfoList.Count() > 0)
                                            {
                                                linkInfo = linkInfoList.First();
                                            }
                                        }
                                        
                                        PreviewMap preview = new PreviewMap();
                                        preview.SourceModelInfo = sourceInfo;
                                        preview.RecipientModelInfo = recipientInfo;
                                        preview.SourceViewProperties = viewSource;
                                        preview.RecipientViewProperties = viewRecipient;
                                        preview.ViewLinkInfo = linkInfo;
                                        preview.IsEnabled = true;

                                        sourceViewIds.Add(viewId);
                                        previewMapList.Add(preview);
                                    }
                                }
                            }
                        }
                    }
                }

                if (previewMapList.Count > 0)
                {
                    PreviewWindow pWindow = new PreviewWindow(previewMapList, createSheet);
                    if (true == pWindow.ShowDialog())
                    {
                        previewMapList = pWindow.PreviewMapList;

                        progressBar.Visibility = System.Windows.Visibility.Visible;
                        statusLable.Visibility = System.Windows.Visibility.Visible;

                        bool updatedDictionary = UpdateRecipientViewDictionary(recipientInfo, previewMapList);
                        bool updatedGoogleSheet = GoogleSheetUtil.WriteLinkIfo(recipientInfo.GoogleSheetID, recipientInfo.LinkInfoCollection);
                        bool updatedLinkStatus = UpdateLinkStatus(recipientInfo);

                        progressBar.Visibility = System.Windows.Visibility.Hidden;
                        statusLable.Visibility = System.Windows.Visibility.Hidden;
                    }
                }
                else
                {
                    MessageBox.Show("Please select at least one source item to duplicate views.", "Select a Source Item", MessageBoxButton.OK, MessageBoxImage.Information);
                    result = false;
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update drafting views.\n" + ex.Message, "Update Drafting Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private bool UpdateRecipientViewDictionary(ModelInfo recipientInfo, List<PreviewMap> mapList)
        {
            bool updated = false;
            try
            {
                //update link info
                ObservableCollection<LinkInfo> linkInfoCollection = recipientInfo.LinkInfoCollection;
                Dictionary<int, ViewProperties> viewDictionary = recipientInfo.ViewDictionary;
                foreach (PreviewMap map in mapList)
                {
                    LinkInfo info = map.ViewLinkInfo;
                    if (info.DestItemId == -1) { continue; } //skipped items
                    var foundLink = linkInfoCollection.Where(x => x.SourceModelId == info.SourceModelId && x.SourceItemId == info.SourceItemId);
                    if (foundLink.Count() > 0)
                    {
                        int index  = linkInfoCollection.IndexOf(foundLink.First());
                        if (linkInfoCollection[index].ItemStatus == LinkItemStatus.None) //existing in the spreadsheet
                        {
                            linkInfoCollection[index] = info;
                            linkInfoCollection[index].ItemStatus = LinkItemStatus.Updated;
                        }
                    }
                    else if(info.ItemStatus == LinkItemStatus.None)
                    {
                        info.ItemStatus = LinkItemStatus.Added;
                        linkInfoCollection.Add(info);
                    }

                    ViewProperties vp = map.RecipientViewProperties;
                    if (null != vp)
                    {
                        if (viewDictionary.ContainsKey(vp.ViewId))
                        {
                            viewDictionary.Remove(vp.ViewId);
                        }
                        viewDictionary.Add(vp.ViewId, vp);
                    }
                }

                //update dictionary
                recipientInfo.LinkInfoCollection = linkInfoCollection;
                recipientInfo.ViewDictionary = viewDictionary;

                if (modelInfoDictionary.ContainsKey(recipientInfo.RevitDocumentID))
                {
                    modelInfoDictionary.Remove(recipientInfo.RevitDocumentID);
                }

                modelInfoDictionary.Add(recipientInfo.RevitDocumentID, recipientInfo);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update the recipient model info.\n"+ex.Message, "Update Recipient Model Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                updated = false;    
            }
            return updated;
        }

        public bool FixViewDictionary(ViewMapClass viewMapClass)
        {
            bool result = false;
            try
            {
                ModelInfo recipientInfo = viewMapClass.RecipientInfo;
                ObservableCollection<LinkInfo> linkInfoCollection = recipientInfo.LinkInfoCollection;
                List<LinkInfo> fixedLInkInfo = viewMapClass.LinkInfoList;

                //deleted links
                var linkToDelete = from info in fixedLInkInfo where info.ItemStatus == LinkItemStatus.Deleted select info;
                if (linkToDelete.Count() > 0)
                {
                    foreach (LinkInfo linkInfo in linkToDelete)
                    {
                        int index = linkInfoCollection.IndexOf(linkInfoCollection.Where(x => x.SourceModelId == linkInfo.SourceModelId && x.SourceItemId == linkInfo.SourceItemId && x.DestItemId == linkInfo.DestItemId).FirstOrDefault());
                        if (index > -1)
                        {
                            linkInfoCollection[index].ItemStatus = linkInfo.ItemStatus;
                        }
                    }
                }

                //added links
                 var linkToAdd = from info in fixedLInkInfo where info.ItemStatus == LinkItemStatus.Added select info;
                 if (linkToAdd.Count() > 0)
                 {
                     foreach (LinkInfo linkInfo in linkToAdd)
                     {
                         int index = linkInfoCollection.IndexOf(linkInfoCollection.Where(x => x.SourceModelId == linkInfo.SourceModelId && x.SourceItemId == linkInfo.SourceItemId && x.DestItemId == linkInfo.DestItemId).FirstOrDefault());
                         if (index > -1)
                         {
                             linkInfoCollection[index].ItemStatus = linkInfo.ItemStatus;
                         }
                         else
                         {
                             linkInfoCollection.Add(linkInfo);
                         }

                     }
                 }

                 if(modelInfoDictionary.ContainsKey(recipientInfo.RevitDocumentID))
                 {
                     modelInfoDictionary.Remove(recipientInfo.RevitDocumentID);
                 }

                 recipientInfo.LinkInfoCollection = linkInfoCollection;
                 modelInfoDictionary.Add(recipientInfo.RevitDocumentID, recipientInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to fix the link information.\n"+ex.Message, "Fix Link Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        public bool UpdateLinkStatus(ModelInfo recipientInfo)
        {
            bool updated = false;
            try
            {
                //after updating google spreadsheet,restoring back to the initial state
                ObservableCollection<LinkInfo> linkInfoCollection = recipientInfo.LinkInfoCollection;
                for (int i = linkInfoCollection.Count - 1; i > -1; i--)
                {
                    if (linkInfoCollection[i].ItemStatus == LinkItemStatus.Deleted)
                    {
                        linkInfoCollection.RemoveAt(i);
                    }
                    else
                    {
                        linkInfoCollection[i].ItemStatus = LinkItemStatus.None;
                    }
                }

                if (modelInfoDictionary.ContainsKey(recipientInfo.RevitDocumentID))
                {
                    modelInfoDictionary.Remove(recipientInfo.RevitDocumentID);
                }
                modelInfoDictionary.Add(recipientInfo.RevitDocumentID, recipientInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update the status of linked items.\n"+ex.Message, "Update Link Status", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return updated;
        }

    }

}
