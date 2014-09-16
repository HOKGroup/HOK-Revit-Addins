
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

namespace HOK.ModelManager.ReplicateViews
{
    public class ProjectViewManager
    {
        private UIApplication m_app;
        private bool verifiedUser = false;
        private GoogleDocsUtil googleDocsUtil = null;
        private Dictionary<string/*docName*/, ModelInfo> modelInfoDictionary = new Dictionary<string, ModelInfo>();
        private Dictionary<string/*docName*/, Dictionary<int/*viewId*/, ViewProperties>> viewDictionary = new Dictionary<string, Dictionary<int, ViewProperties>>();
        private List<string> viewTypeNames = new List<string>();
        private List<string> sheetNumbers = new List<string>();
        private List<ViewMapClass> viewMapList = new List<ViewMapClass>();
        private List<PreviewMap> previewMapList = new List<PreviewMap>();
        private Dictionary<string/*docTitle*/, GoogleDocInfo> googleDocDictionary = new Dictionary<string, GoogleDocInfo>();

        public Dictionary<string, ModelInfo> ModelInfoDictionary { get { return modelInfoDictionary; } set { modelInfoDictionary = value; } }
        public Dictionary<string, Dictionary<int, ViewProperties>> ViewDictionary { get { return viewDictionary; } set { viewDictionary = value; } }
        public bool VerifiedUser { get { return verifiedUser; } set { verifiedUser = value; } }

        public ProjectViewManager(UIApplication uiapp)
        {
            m_app = uiapp;
            googleDocsUtil = new GoogleDocsUtil();
            if (googleDocsUtil.GoogleDocActivated)
            {
                verifiedUser = VerifyUserInfo();
                if (verifiedUser)
                {
                    GetDraftingViewInfo();
                    viewTypeNames.Sort();
                    sheetNumbers.Sort();
                    sheetNumbers.Add("None");

                    if (modelInfoDictionary.Count > 0)
                    {
                        foreach (ModelInfo modelInfo in modelInfoDictionary.Values)
                        {
                            GoogleDocInfo googleDocInfo = googleDocsUtil.GetGoogleDocInfo(modelInfo, ModelManagerMode.ProjectReplication);
                            if (null != googleDocInfo)
                            {
                                if (!googleDocDictionary.ContainsKey(googleDocInfo.DocTitle))
                                {
                                    googleDocDictionary.Add(googleDocInfo.DocTitle, googleDocInfo);
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool VerifyUserInfo()
        {
            bool verified = true;
            try
            {
                Document activeDoc = m_app.ActiveUIDocument.Document;
                ModelInfo modelInfo = new ModelInfo(activeDoc);
                if (!modelInfo.HOKStandard)
                {
                    Google.Documents.Document googleFolder = googleDocsUtil.GetGoogleFolder(modelInfo, ModelManagerMode.ProjectReplication, false);
                    if (null == googleFolder) { verified = false; }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to verify the user info.\n"+ex.Message, "Verify User Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                verified = false;
            }
            return verified;
        }

        private void GetDraftingViewInfo()
        {
            try
            {
                foreach (Document doc in m_app.Application.Documents)
                {
                    if (!string.IsNullOrEmpty(doc.Title) && !doc.IsFamilyDocument)
                    {
                        if (!modelInfoDictionary.ContainsKey(doc.Title))
                        {
                            ModelInfo modelInfo = new ModelInfo(doc);
                            modelInfoDictionary.Add(doc.Title, modelInfo);

                            FilteredElementCollector collector = new FilteredElementCollector(doc);
                            List<ViewDrafting> viewDraftingList = collector.OfClass(typeof(ViewDrafting)).WhereElementIsNotElementType().ToElements().Cast<ViewDrafting>().ToList();
                            Dictionary<int, ViewProperties> collectedViews = new Dictionary<int, ViewProperties>();
                            foreach (ViewDrafting view in viewDraftingList)
                            {
                                if (view.ViewType != ViewType.DraftingView) { continue; }
                                if (!collectedViews.ContainsKey(view.Id.IntegerValue))
                                {
                                    ViewProperties vp = new ViewProperties(doc, view);
                                    collectedViews.Add(vp.ViewId, vp);
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
                            viewDictionary.Add(doc.Title, collectedViews);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get documents information.\n"+ex.Message, "GetDocumentInfo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void RefreshTreeView(string sourceDoc, string recipientDoc, TreeView sourceView, TreeView recipientView, TreeViewSortBy sortBy, bool createLinks)
        {
            try
            {
                ViewMapClass viewMap = GetViewMap(sourceDoc, recipientDoc, createLinks);
                if (null != viewMap)
                {
                    sourceView.ItemsSource = null;
                    recipientView.ItemsSource = null;

                    if (sortBy == TreeViewSortBy.Sheet)
                    {
                        sourceView.ItemsSource = TreeViewModel.SetTreeBySheet(sourceDoc, viewMap.SourceViews, sheetNumbers);
                        recipientView.ItemsSource = TreeViewModel.SetTreeBySheet(recipientDoc, viewMap.RecipientViews, sheetNumbers);
                    }
                    else if (sortBy == TreeViewSortBy.ViewType)
                    {
                        sourceView.ItemsSource = TreeViewModel.SetTreeByViewType(sourceDoc, viewMap.SourceViews, viewTypeNames);
                        recipientView.ItemsSource = TreeViewModel.SetTreeByViewType(recipientDoc, viewMap.RecipientViews, viewTypeNames);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to refresh the tree view.\nSource Model:"+sourceDoc+"\nRecipient Model:"+recipientDoc+"\n" + ex.Message, "Refresh Tree View", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public ViewMapClass GetViewMap(string sourceDoc, string recipientDoc, bool createLinks)
        {
            ViewMapClass viewMap = null;
            try
            {
                Dictionary<int, ViewProperties> sourceViews = viewDictionary[sourceDoc];
                Dictionary<int, ViewProperties> recipientViews = viewDictionary[recipientDoc];
                List<LinkInfo> linkInfo = new List<LinkInfo>();
                if (googleDocDictionary.ContainsKey(recipientDoc))
                {
                    linkInfo = googleDocDictionary[recipientDoc].LinkInfoList;
                }
                ModelInfo sInfo = modelInfoDictionary[sourceDoc];
                ModelInfo rInfo = modelInfoDictionary[recipientDoc];

                if (createLinks)
                {
                    List<LinkInfo> linksCreated = CreateLinksByNames(sourceViews, recipientViews, sInfo, rInfo, linkInfo);
                    if (linksCreated.Count > 0)
                    {
                        linkInfo.AddRange(linksCreated);
                    }
                }

                ViewMapClass vmc = new ViewMapClass(sInfo, rInfo, sourceViews, recipientViews, linkInfo);
                viewMapList.Add(vmc);
                viewMap = vmc;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return viewMap;
        }

        private List<LinkInfo> CreateLinksByNames(Dictionary<int, ViewProperties> sourceViews, Dictionary<int, ViewProperties> recipientViews, ModelInfo sInfo, ModelInfo rInfo, List<LinkInfo> googleLinks)
        {
            List<LinkInfo> linkInfoList = new List<LinkInfo>();
            try
            {
                foreach (int sourceItemId in sourceViews.Keys)
                {
                    var Ids = from link in googleLinks where link.SourceItemId == sourceItemId select link;
                    if (Ids.Count() > 0)
                    {
                        continue;
                    }
                    else
                    {
                        //create links
                        ViewProperties sView=sourceViews[sourceItemId];
                        string viewName=sView.ViewName;
                        var views = from view in recipientViews.Values where view.ViewName == viewName select view;
                        if (views.Count() > 0)
                        {
                            ViewProperties rView=views.First();

                            LinkInfo li = new LinkInfo();
                            li.ItemType = LinkItemType.DraftingView;
                            li.SourceModelName = sInfo.DocTitle;
                            li.SourceModelPath = sInfo.DocCentralPath;
                            li.DestModelName = rInfo.DocTitle;
                            li.DestModelPath = rInfo.DocCentralPath;

                            li.SourceItemId = sView.ViewId;
                            li.SourceItemName = sView.ViewName;
                            li.DestItemId = rView.ViewId;
                            li.DestItemName = rView.ViewName;

                            li.LinkModified = DateTime.Now.ToString("G");
                            li.LinkModifiedBy = Environment.UserName;

                            linkInfoList.Add(li);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create links by view names.\n"+ex.Message, "Create Links By Names", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return linkInfoList;
        }

        public void RefreshTreeViewBySelection(string sourceDoc, string recipientDoc, TreeView sourceView, TreeView recipientView, TreeViewSortBy sortBy, string filterString, bool createLinks)
        {
            try
            {
                ViewMapClass viewMap = GetViewMap(sourceDoc, recipientDoc, createLinks);
                if (null != viewMap)
                {
                    List<string> strList = new List<string>();
                    strList.Add(filterString);

                    recipientView.ItemsSource = null;

                    if (sortBy == TreeViewSortBy.Sheet)
                    {
                        recipientView.ItemsSource = TreeViewModel.SetTreeBySheet(recipientDoc, viewMap.RecipientViews, strList);
                    }
                    else if (sortBy == TreeViewSortBy.ViewType)
                    {
                        recipientView.ItemsSource = TreeViewModel.SetTreeByViewType(recipientDoc, viewMap.RecipientViews, strList);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show( "Failed to refresh the tree view by "+filterString +"\n"+ ex.Message, "Refresh Tree View By Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
   
        public bool UpdateDraftingViews(string sourceDoc, string recipientDoc, TreeView treeViewSource, TreeView treeViewRecipient, bool createSheet, bool createLinks,TextBlock statusLable, ProgressBar progressBar)
        {
            bool result = false;
            try
            {
                previewMapList.Clear();

                ViewMapClass vmc = GetViewMap(sourceDoc, recipientDoc, createLinks);
                Dictionary<int, ViewProperties> sourceViews = vmc.SourceViews;
                Dictionary<int, ViewProperties> recipientViews = vmc.RecipientViews;

                List<PreviewMap> previewList = new List<PreviewMap>();
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
                                    if (sourceViews.ContainsKey(viewId))
                                    {
                                        ViewProperties viewSource = sourceViews[viewId];
                                        ViewProperties viewRecipient = null;
                                        LinkInfo linkInfo = new LinkInfo();
                                        if (null != viewSource.LinkedView)
                                        {
                                            viewRecipient = viewSource.LinkedView;
                                            var linkInfoList = from info in vmc.LinkInfoList where info.SourceItemId == viewId && info.DestItemId == viewRecipient.ViewId select info;
                                            if (linkInfoList.Count() > 0)
                                            {
                                                linkInfo = linkInfoList.First();
                                            }
                                        }
                                        
                                        PreviewMap preview = new PreviewMap();
                                        preview.SourceModelInfo = modelInfoDictionary[sourceDoc];
                                        preview.RecipientModelInfo = modelInfoDictionary[recipientDoc];
                                        preview.SourceViewProperties = viewSource;
                                        preview.RecipientViewProperties = viewRecipient;
                                        preview.ViewLinkInfo = linkInfo;
                                        preview.IsEnabled = true;

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
                        List<LinkInfo> listLinkInfo = new List<LinkInfo>();
                        foreach (PreviewMap preview in previewMapList)
                        {
                            listLinkInfo.Add(preview.ViewLinkInfo);
                        }

                        progressBar.Visibility = System.Windows.Visibility.Visible;
                        statusLable.Visibility = System.Windows.Visibility.Visible;

                        bool updatedGoogleDoc = UpdateGoogleDoc(recipientDoc, listLinkInfo);
                        bool updatedDictionary = UpdateRecipientViewDictionary(recipientDoc, previewMapList);
                            
                        progressBar.Visibility = System.Windows.Visibility.Hidden;
                        statusLable.Visibility = System.Windows.Visibility.Hidden;
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update drafting views.\n" + ex.Message, "Update Drafting Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        public bool UpdateGoogleDoc(string recipientDoc, List<LinkInfo> listLinkInfo)
        {
            bool result = false;
            try
            {
                GoogleDocInfo googleDocInfo = null;

                if (googleDocDictionary.ContainsKey(recipientDoc))
                {
                    googleDocInfo = googleDocDictionary[recipientDoc];
                }
                else
                {
                    googleDocInfo = googleDocsUtil.CreateSpreadsheet(modelInfoDictionary[recipientDoc], ModelManagerMode.ProjectReplication);
                }
                bool updated=googleDocsUtil.UpdateLinkInfo(googleDocInfo.SheetEntry, listLinkInfo);
                if (updated)
                {
                    listLinkInfo = googleDocsUtil.GetLinkInfo(googleDocInfo.SheetEntry);
                    googleDocInfo.LinkInfoList = listLinkInfo;
                    if (googleDocDictionary.ContainsKey(recipientDoc)) { googleDocDictionary.Remove(recipientDoc); }
                    googleDocDictionary.Add(recipientDoc, googleDocInfo);
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update google doc information.\n"+ex.Message, "Update Google Doc", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        public bool UpdateGoogleDoc(string recipientDoc, List<LinkInfo> updatingLinks, List<LinkInfo> deletingLinks)
        {
            bool result = false;
            try
            {
                GoogleDocInfo googleDocInfo = null;

                if (googleDocDictionary.ContainsKey(recipientDoc))
                {
                    googleDocInfo = googleDocDictionary[recipientDoc];
                }
                else
                {
                    googleDocInfo = googleDocsUtil.CreateSpreadsheet(modelInfoDictionary[recipientDoc], ModelManagerMode.ProjectReplication);
                }
                bool deleted = googleDocsUtil.DeleteLinkInfo(googleDocInfo.SheetEntry, deletingLinks);
                bool updated = googleDocsUtil.UpdateLinkInfo(googleDocInfo.SheetEntry, updatingLinks);
                if (updated && deleted)
                {
                    updatingLinks = googleDocsUtil.GetLinkInfo(googleDocInfo.SheetEntry);
                    googleDocInfo.LinkInfoList = updatingLinks;
                    if (googleDocDictionary.ContainsKey(recipientDoc)) { googleDocDictionary.Remove(recipientDoc); }
                    googleDocDictionary.Add(recipientDoc, googleDocInfo);
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(recipientDoc + ": Failed to update google doc.\n" + ex.Message, "Update Google Doc", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private bool UpdateRecipientViewDictionary(string recipientDoc, List<PreviewMap> previewMapList)
        {
            bool result = false;
            try
            {
                //to reload destination image file path
                if (viewDictionary.ContainsKey(recipientDoc))
                {
                    Dictionary<int, ViewProperties> views = viewDictionary[recipientDoc];
                    foreach (PreviewMap preview in previewMapList)
                    {
                        if (views.ContainsKey(preview.RecipientViewProperties.ViewId))
                        {
                            views.Remove(preview.RecipientViewProperties.ViewId);
                        }
                        views.Add(preview.RecipientViewProperties.ViewId, preview.RecipientViewProperties);
                    }
                    viewDictionary.Remove(recipientDoc);
                    viewDictionary.Add(recipientDoc, views);
                }

                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update view dictionary.\n" + ex.Message, "Update Recipient View Dictionary", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        public bool UpdateViewDictionary(ViewMapClass viewMap)
        {
            bool result = false;
            try
            {
                string sourceDoc = viewMap.SourceInfo.DocTitle;
                if (viewDictionary.ContainsKey(sourceDoc))
                {
                    Dictionary<int, ViewProperties> sourceViews = viewMap.SourceViews;
                    viewDictionary.Remove(sourceDoc);
                    viewDictionary.Add(sourceDoc, sourceViews);
                }

                string recipientDoc = viewMap.RecipientInfo.DocTitle;
                if (viewDictionary.ContainsKey(recipientDoc))
                {
                    Dictionary<int, ViewProperties> recipientViews = viewMap.RecipientViews;
                    viewDictionary.Remove(recipientDoc);
                    viewDictionary.Add(recipientDoc, recipientViews);
                }

                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update view dictionary.\n" + ex.Message, "Update View Dictionary", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }
    }

}
