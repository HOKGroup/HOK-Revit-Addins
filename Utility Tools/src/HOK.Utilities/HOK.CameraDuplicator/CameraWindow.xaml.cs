using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace HOK.CameraDuplicator
{
    /// <summary>
    /// Interaction logic for CameraWindow.xaml
    /// </summary>
    public partial class CameraWindow : Window
    {
        private UIApplication m_app = null;

        private Dictionary<string/*guid*/, ModelInfo> modelDictionary = new Dictionary<string, ModelInfo>();
        private List<CameraViewMap> cameraViewMapList = new List<CameraViewMap>();

        public CameraWindow(UIApplication uiapp)
        {
            m_app = uiapp;

            modelDictionary = GetModelInfo();
            InitializeComponent();

            List<ModelInfo> models = modelDictionary.Values.OrderBy(o => o.ModelName).ToList();
            comboBoxSource.ItemsSource = models;
            comboBoxSource.DisplayMemberPath = "ModelName";
            comboBoxRecipient.ItemsSource = models;
            comboBoxRecipient.DisplayMemberPath = "ModelName";

            if (models.Count > 1)
            {
                comboBoxSource.SelectedIndex = 0;
                comboBoxRecipient.SelectedIndex = 1;
            }
        }

        private Dictionary<string, ModelInfo> GetModelInfo()
        {
            Dictionary<string, ModelInfo> infoDictionary = new Dictionary<string, ModelInfo>();
            foreach (Document doc in m_app.Application.Documents)
            {
                if (doc.IsFamilyDocument) { continue; }
#if RELEASE204 ||RELEASE2015
                if (doc.IsLinked) { continue; }
#endif
                string modelId = ModelDataStorageUtil.GetModelId(doc);
                if (!infoDictionary.ContainsKey(modelId))
                {
                    ModelInfo info = new ModelInfo(doc, modelId);
                    infoDictionary.Add(modelId, info);
                }
            }
            return infoDictionary;
        }

        private void DisplayCameraView(ModelInfo sModelInfo, ModelInfo rModelInfo)
        {
            try
            {
                cameraViewMapList = CameraDataStorageUtil.GetCameraViewMap(rModelInfo.ModelDoc);

                foreach (CameraViewMap map in cameraViewMapList)
                {
                    if (sModelInfo.ModelId == map.SourceModelId && rModelInfo.ModelId == map.RecipientModelId)
                    {
                        if (sModelInfo.CameraViews.ContainsKey(map.SourceViewId) && rModelInfo.CameraViews.ContainsKey(map.RecipientViewId))
                        {
                            CameraViewInfo sViewInfo = new CameraViewInfo(sModelInfo.CameraViews[map.SourceViewId]);
                            sViewInfo.LinkedViewId = map.RecipientViewId;
                            sViewInfo.Linked = true;
                            sModelInfo.CameraViews.Remove(map.SourceViewId);
                            sModelInfo.CameraViews.Add(map.SourceViewId, sViewInfo);
                            
                            CameraViewInfo rViewInfo = new CameraViewInfo(rModelInfo.CameraViews[map.RecipientViewId]);
                            rViewInfo.LinkedViewId = map.SourceViewId;
                            rViewInfo.Linked = true;
                            rModelInfo.CameraViews.Remove(map.RecipientViewId);
                            rModelInfo.CameraViews.Add(map.RecipientViewId, rViewInfo);
                        }
                    }
                }

                List<CameraViewInfo> sViews = sModelInfo.CameraViews.Values.OrderBy(o => o.ViewName).ToList();
                dataGridSource.ItemsSource = null;
                dataGridSource.ItemsSource = sViews;

                List<CameraViewInfo> rViews = rModelInfo.CameraViews.Values.OrderBy(o => o.ViewName).ToList();
                dataGridRecipient.ItemsSource = null;
                dataGridRecipient.ItemsSource = rViews;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display the list of camera view.\n"+ex.Message, "Display Camera View", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void comboBoxSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null != comboBoxSource.SelectedItem && null != comboBoxRecipient.SelectedItem)
            {
                ModelInfo sModelInfo = (ModelInfo)comboBoxSource.SelectedItem;
                ModelInfo rModelInfo = (ModelInfo)comboBoxRecipient.SelectedItem;
                if (sModelInfo.ModelId == rModelInfo.ModelId)
                {
                    labelSameModel.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    labelSameModel.Visibility = System.Windows.Visibility.Hidden;
                    DisplayCameraView(sModelInfo, rModelInfo);
                }
                
            }
        }

        private void comboBoxRecipient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null != comboBoxSource.SelectedItem && null != comboBoxRecipient.SelectedItem)
            {
                ModelInfo sModelInfo = (ModelInfo)comboBoxSource.SelectedItem;
                ModelInfo rModelInfo = (ModelInfo)comboBoxRecipient.SelectedItem;
                if (sModelInfo.ModelId == rModelInfo.ModelId)
                {
                    labelSameModel.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    labelSameModel.Visibility = System.Windows.Visibility.Hidden;
                    DisplayCameraView(sModelInfo, rModelInfo);
                }

            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != comboBoxSource.SelectedItem && null != comboBoxRecipient.SelectedItem)
                {
                    ModelInfo sourceModelInfo = (ModelInfo)comboBoxSource.SelectedItem;
                    ModelInfo recipientModelInfo = (ModelInfo)comboBoxRecipient.SelectedItem;
                    List<CameraViewInfo> cameraViews = (List<CameraViewInfo>)dataGridSource.ItemsSource;
                    ViewFamilyType viewFamilyType = GetViewFamilyType(recipientModelInfo.ModelDoc);

                    int count = 0;
                    using (TransactionGroup tg = new TransactionGroup(recipientModelInfo.ModelDoc))
                    {
                        tg.Start("Duplicate Camera Views");
                        try
                        {
                            foreach (CameraViewInfo viewInfo in cameraViews)
                            {
                                if (viewInfo.IsSelected)
                                {
                                    CameraViewInfo createdViewInfo = null;
                                    bool duplicated = DuplicateCameraView(sourceModelInfo, recipientModelInfo, viewInfo, viewFamilyType, out createdViewInfo);
                                    if (duplicated && null != createdViewInfo) 
                                    { 
                                        recipientModelInfo.CameraViews.Add(createdViewInfo.ViewId, createdViewInfo);

                                        CameraViewInfo sourceViewInfo = new CameraViewInfo(viewInfo);
                                        sourceViewInfo.Linked = true;
                                        sourceViewInfo.LinkedViewId = createdViewInfo.ViewId;
                                        sourceModelInfo.CameraViews.Remove(sourceViewInfo.ViewId);
                                        sourceModelInfo.CameraViews.Add(sourceViewInfo.ViewId, sourceViewInfo);

                                        CameraViewMap viewMap = new CameraViewMap();
                                        viewMap.SourceModelId = sourceModelInfo.ModelId;
                                        viewMap.SourceViewId = sourceViewInfo.ViewId;
                                        viewMap.RecipientModelId = recipientModelInfo.ModelId;
                                        viewMap.RecipientViewId = createdViewInfo.ViewId;

                                        int index = cameraViewMapList.FindIndex(o => o.SourceModelId == sourceModelInfo.ModelId && o.SourceViewId == sourceViewInfo.ViewId);
                                        if (index > 0) { cameraViewMapList.RemoveAt(index); }
                                        cameraViewMapList.Add(viewMap);
                                        count++; 
                                    }
                                }
                            }
                            tg.Assimilate();
                        }
                        catch (Exception ex)
                        {
                            tg.RollBack();
                            MessageBox.Show("Failed to duplicate camera views.\n"+ex.Message, "Duplicate Camera Views", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    CameraDataStorageUtil.StoreCameraViewMap(recipientModelInfo.ModelDoc, cameraViewMapList);

                    modelDictionary.Remove(recipientModelInfo.ModelId);
                    modelDictionary.Add(recipientModelInfo.ModelId, recipientModelInfo);
                    modelDictionary.Remove(sourceModelInfo.ModelId);
                    modelDictionary.Add(sourceModelInfo.ModelId, sourceModelInfo);
                    int sourceSelectedIndex = comboBoxSource.SelectedIndex;
                    int recipientSelectedIndex = comboBoxRecipient.SelectedIndex;

                    List<ModelInfo> models = modelDictionary.Values.OrderBy(o => o.ModelName).ToList();
                    comboBoxSource.ItemsSource = null;
                    comboBoxSource.ItemsSource = models;
                    comboBoxSource.DisplayMemberPath = "ModelName";
                    comboBoxSource.SelectedIndex = sourceSelectedIndex;

                    comboBoxRecipient.ItemsSource = null;
                    comboBoxRecipient.ItemsSource = models;
                    comboBoxRecipient.DisplayMemberPath = "ModelName";
                    comboBoxRecipient.SelectedIndex = recipientSelectedIndex;

                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to duplicate camera views.\n"+ex.Message, "Duplicate Camera Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private ViewFamilyType GetViewFamilyType(Document doc)
        {
            ViewFamilyType vFamilyType = null;
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            List<ViewFamilyType> viewFamilyTypes = collector.OfClass(typeof(ViewFamilyType)).ToElements().Cast<ViewFamilyType>().ToList();
            var vTypes = from vType in viewFamilyTypes where vType.ViewFamily == ViewFamily.ThreeDimensional select vType;
            if (vTypes.Count() > 0)
            {
                vFamilyType = vTypes.First();
            }
            return vFamilyType;
        }

        private bool DuplicateCameraView(ModelInfo sModel, ModelInfo rModel, CameraViewInfo cameraInfo, ViewFamilyType vFamilyType, out CameraViewInfo createdViewInfo)
        {
            bool duplicated = false;
            createdViewInfo = null;
            try
            {
                Document sourceDoc = sModel.ModelDoc;
                Document recipientDoc = rModel.ModelDoc;

                using (Transaction trans = new Transaction(recipientDoc))
                {
                    trans.Start("Create Camera View");
                    try
                    {
                        View3D createdView = View3D.CreatePerspective(recipientDoc, vFamilyType.Id);
                        if (CanHaveViewName(rModel, cameraInfo.ViewName))
                        {
                            createdView.ViewName = cameraInfo.ViewName;
                        }
                        createdView.SetOrientation(cameraInfo.Orientation);
                        createdView.CropBoxActive = cameraInfo.IsCropBoxOn; 
                        createdView.CropBox = cameraInfo.CropBox;
#if RELEASE2013
                        createdView.SectionBox = cameraInfo.SectionBox;
#elif RELEASE2014 ||RELEASE2015
                        createdView.IsSectionBoxActive = cameraInfo.IsSectionBoxOn;
                        createdView.SetSectionBox(cameraInfo.SectionBox);
#endif

                        createdView.DisplayStyle = cameraInfo.Display;
                        //createdView.SetRenderingSettings(cameraInfo.Rendering);

                        foreach (string paramName in cameraInfo.ViewParameters.Keys)
                        {
                            Parameter sourceParam = cameraInfo.ViewParameters[paramName];
#if RELEASE2013||RELEASE2014
                            Parameter recipientParam = createdView.get_Parameter(paramName);
#elif RELEASE2015
                             Parameter recipientParam = createdView.LookupParameter(paramName);
#endif


                            if (null != recipientParam && sourceParam.HasValue)
                            {
                                if (!recipientParam.IsReadOnly)
                                {
                                    switch (sourceParam.StorageType)
                                    {
                                        case StorageType.Double:
                                            try 
                                            {
                                                recipientParam.Set(sourceParam.AsDouble());
                                            }
                                            catch { }
                                            break;
                                        case StorageType.ElementId:
                                            try
                                            {
                                                recipientParam.Set(sourceParam.AsElementId());
                                            }
                                            catch
                                            {
                                            }
                                            break;
                                        case StorageType.Integer:
                                            try
                                            {
                                                recipientParam.Set(sourceParam.AsInteger());
                                            }
                                            catch { }
                                            break;
                                        case StorageType.String:
                                            try
                                            {
                                                recipientParam.Set(sourceParam.AsString());
                                            }
                                            catch { }
                                            
                                            break;
                                    }
                                }
                            }
                        }

                        createdViewInfo = new CameraViewInfo(createdView);
                        createdViewInfo.LinkedViewId = cameraInfo.ViewId;
                        createdViewInfo.Linked = true;
                        duplicated = true;
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        string message = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to duplicate camera views.\n" + ex.Message, "Duplicate Camera Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return duplicated;
        }

        private bool CanHaveViewName(ModelInfo rModelInfo, string ViewName)
        {
            bool allow = true;
            try
            {
                var views = from view in rModelInfo.CameraViews.Values where view.ViewName == ViewName select view;
                if (views.Count() > 0)
                {
                    //the same name of view already exist
                    allow = false;
                }
                           
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check existing view names.\n"+ex.Message, "View Name Check", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return allow;
        }

        private void buttonAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<CameraViewInfo> cameraViews = (List<CameraViewInfo>)dataGridSource.ItemsSource;
                for (int i = cameraViews.Count - 1; i >-1; i--)
                {
                    CameraViewInfo cameraView = cameraViews[i];
                    cameraView.IsSelected = true;
                    cameraViews.RemoveAt(i);
                    cameraViews.Insert(i, cameraView);
                }

                dataGridSource.ItemsSource = null;
                dataGridSource.ItemsSource = cameraViews;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check all items.\n" + ex.Message, "Check All", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonNone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<CameraViewInfo> cameraViews = (List<CameraViewInfo>)dataGridSource.ItemsSource;
                for (int i = cameraViews.Count - 1; i > -1; i--)
                {
                    CameraViewInfo cameraView = cameraViews[i];
                    cameraView.IsSelected = false;
                    cameraViews.RemoveAt(i);
                    cameraViews.Insert(i, cameraView);
                }

                dataGridSource.ItemsSource = null;
                dataGridSource.ItemsSource = cameraViews;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to uncheck all items.\n" + ex.Message, "Uncheck All", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
    }
}
