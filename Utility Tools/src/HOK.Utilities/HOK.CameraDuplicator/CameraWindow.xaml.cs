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
        private ViewType selectedViewType = ViewType.ThreeD;
        private ViewFamily selectedViewFamily = ViewFamily.ThreeDimensional;

        private Dictionary<string/*guid*/, ModelInfo> modelDictionary = new Dictionary<string, ModelInfo>();
        private ViewConfiguration viewConfig = new ViewConfiguration();

        private List<MissingItem> missingItems = new List<MissingItem>();
        private int[] parametersToSkip = new int[] { (int)BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP/*scope box*/, (int)BuiltInParameter.VIEW_NAME, (int)BuiltInParameter.VIEW_TEMPLATE, (int)BuiltInParameter.VIEW_PHASE };

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);

        public CameraWindow(UIApplication uiapp)
        {
            m_app = uiapp;

            modelDictionary = GetModelInfo();
            InitializeComponent();

            this.Title = "View Mover v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            SetViewTypeList();

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

        private void SetViewTypeList()
        {
            try
            {
                List<ViewTypeInfo> viewTypeinfoList = new List<ViewTypeInfo>();

                ViewTypeInfo vtInfo = new ViewTypeInfo("3D : Camera View", ViewType.ThreeD, ViewFamily.ThreeDimensional);
                viewTypeinfoList.Add(vtInfo);

                vtInfo = new ViewTypeInfo("2D : Floor Plan", ViewType.FloorPlan, ViewFamily.FloorPlan);
                viewTypeinfoList.Add(vtInfo);

                vtInfo = new ViewTypeInfo("2D : Ceiling Plan", ViewType.CeilingPlan, ViewFamily.CeilingPlan);
                viewTypeinfoList.Add(vtInfo);

                vtInfo = new ViewTypeInfo("2D : Area Plan", ViewType.AreaPlan, ViewFamily.AreaPlan);
                viewTypeinfoList.Add(vtInfo);

                vtInfo = new ViewTypeInfo("2D : Structural Plan", ViewType.EngineeringPlan, ViewFamily.StructuralPlan);
                viewTypeinfoList.Add(vtInfo);

                viewTypeinfoList = viewTypeinfoList.OrderBy(o => o.ViewTypeName).ToList();

                comboBoxViewType.ItemsSource = null;
                comboBoxViewType.ItemsSource = viewTypeinfoList;
                comboBoxViewType.DisplayMemberPath = "ViewTypeName";
                comboBoxViewType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set the list of view type.\n"+ex.Message, "Set View Type List", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private Dictionary<string, ModelInfo> GetModelInfo()
        {
            Dictionary<string, ModelInfo> infoDictionary = new Dictionary<string, ModelInfo>();
            foreach (Document doc in m_app.Application.Documents)
            {
                if (doc.IsFamilyDocument) { continue; }
                if (doc.IsLinked) { continue; }
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
                List<CameraViewInfo> sourceViews = sModelInfo.CameraViews.Values.ToList();
                List<CameraViewInfo> recipientViews = rModelInfo.CameraViews.Values.ToList();
                foreach (CameraViewInfo cvi in recipientViews)
                {
                    var foundMap = from sv in sourceViews where sv.ViewName == cvi.ViewName select sv;
                    if (foundMap.Count() > 0)
                    {
                        CameraViewInfo foundInfo = foundMap.First(); //sourceView

                        CameraViewInfo sourceInfo = new CameraViewInfo(foundInfo);
                        sourceInfo.LinkedViewId = cvi.ViewId;
                        sourceInfo.Linked = true;
                        sModelInfo.CameraViews.Remove(sourceInfo.ViewId);
                        sModelInfo.CameraViews.Add(sourceInfo.ViewId, sourceInfo);

                        CameraViewInfo recipientInfo = new CameraViewInfo(cvi);
                        recipientInfo.LinkedViewId = sourceInfo.ViewId;
                        recipientInfo.Linked = true;
                        rModelInfo.CameraViews.Remove(recipientInfo.ViewId);
                        rModelInfo.CameraViews.Add(recipientInfo.ViewId, recipientInfo);

                    }
                }

                dataGridSource.ItemsSource = null;
                sourceViews = sModelInfo.CameraViews.Values.OrderBy(o => o.ViewName).ToList();
                dataGridSource.ItemsSource = sourceViews;

                dataGridRecipient.ItemsSource = null;
                recipientViews = rModelInfo.CameraViews.Values.OrderBy(o => o.ViewName).ToList();
                dataGridRecipient.ItemsSource = recipientViews;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display the list of camera view.\n"+ex.Message, "Display Camera View", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DisplayPlanView(ModelInfo sModelInfo, ModelInfo rModelInfo, ViewType selectedType)
        {
            try
            {
                var sViews = from sView in sModelInfo.PlanViews.Values where sView.PlanViewType == selectedType select sView;
                var rViews = from rView in rModelInfo.PlanViews.Values where rView.PlanViewType == selectedType select rView;
                if (sViews.Count() > 0 && rViews.Count() > 0)
                {
                    List<PlanViewInfo> sourceViews = sViews.ToList();
                    List<PlanViewInfo> recipientViews = rViews.ToList();
                    foreach (PlanViewInfo pvi in recipientViews)
                    {
                        var foundMap = from rv in sourceViews where rv.ViewName == pvi.ViewName select rv;
                        if (foundMap.Count() > 0)
                        {
                            PlanViewInfo foundInfo = foundMap.First();

                            PlanViewInfo sourceInfo = new PlanViewInfo(foundInfo);
                            sourceInfo.LinkedViewId = pvi.ViewId;
                            sourceInfo.Linked = true;
                            sModelInfo.PlanViews.Remove(sourceInfo.ViewId);
                            sModelInfo.PlanViews.Add(sourceInfo.ViewId, sourceInfo);

                            PlanViewInfo recipientInfo = new PlanViewInfo(pvi);
                            recipientInfo.LinkedViewId = sourceInfo.ViewId;
                            recipientInfo.Linked = true;
                            rModelInfo.PlanViews.Remove(recipientInfo.ViewId);
                            rModelInfo.PlanViews.Add(recipientInfo.ViewId, recipientInfo);
                           
                        }
                    }
                }

                dataGridSource.ItemsSource = null;
                sViews = from sView in sModelInfo.PlanViews.Values where sView.PlanViewType == selectedType select sView;
                if (sViews.Count() > 0)
                {
                    List<PlanViewInfo> sourceViews = sViews.OrderBy(o => o.ViewName).ToList();
                    dataGridSource.ItemsSource = sourceViews;
                }

                dataGridRecipient.ItemsSource = null;
                rViews = from rView in rModelInfo.PlanViews.Values where rView.PlanViewType == selectedType select rView;
                if (rViews.Count() > 0)
                {
                    List<PlanViewInfo> recipientViews = rViews.OrderBy(o => o.ViewName).ToList();
                    dataGridRecipient.ItemsSource = recipientViews;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display plan views.\n"+ex.Message, "Display Plan Views", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    if (selectedViewType == ViewType.ThreeD)
                    {
                        DisplayCameraView(sModelInfo, rModelInfo);
                    }
                    else
                    {
                        DisplayPlanView(sModelInfo, rModelInfo, selectedViewType);
                    }
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
                    if (selectedViewType == ViewType.ThreeD)
                    {
                        DisplayCameraView(sModelInfo, rModelInfo);
                    }
                    else
                    {
                        DisplayPlanView(sModelInfo, rModelInfo, selectedViewType);
                    }
                    //map info will be stored in the recipient document
                    viewConfig = ViewConfigDataStorageUtil.GetViewConfiguration(rModelInfo.ModelDoc);
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
                    statusLable.Text = "Duplicating Views ..";
                    progressBar.Visibility = System.Windows.Visibility.Visible;

                    ModelInfo sourceModelInfo = (ModelInfo)comboBoxSource.SelectedItem;
                    ModelInfo recipientModelInfo = (ModelInfo)comboBoxRecipient.SelectedItem;
                    if (selectedViewType == ViewType.ThreeD)
                    {
                        DuplicateCameraViews(sourceModelInfo, recipientModelInfo);
                    }
                    else
                    {
                        DuplicatePlanViews(sourceModelInfo, recipientModelInfo);
                    }

                    progressBar.Visibility = System.Windows.Visibility.Hidden;
                    statusLable.Text = "Ready.";
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to duplicate camera views.\n"+ex.Message, "Duplicate Camera Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool DuplicateCameraViews(ModelInfo sModel, ModelInfo rModel)
        {
            bool duplicated = false;
            try
            {
                List<CameraViewInfo> cameraViews = (List<CameraViewInfo>)dataGridSource.ItemsSource;
                ViewFamilyType viewFamilyType = GetViewFamilyType(rModel.ModelDoc, selectedViewFamily);
                
                using (TransactionGroup tg = new TransactionGroup(rModel.ModelDoc))
                {
                    tg.Start("Duplicate Camera Views");
                    try
                    {
                        var selectedViews = from cView in cameraViews where cView.IsSelected && !cView.Linked select cView;
                        if (selectedViews.Count() > 0)
                        {
                            missingItems = new List<MissingItem>();
                            progressBar.Value = 0;
                            progressBar.Maximum = selectedViews.Count();

                            UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
                            double count = 0;
                            foreach (CameraViewInfo viewInfo in selectedViews)
                            {
                                CameraViewInfo createdViewInfo = null;
                                bool duplicatedView = DuplicateCameraView(sModel, rModel, viewInfo, viewFamilyType, out createdViewInfo);
                                if (duplicatedView && null != createdViewInfo)
                                {
                                    rModel.CameraViews.Add(createdViewInfo.ViewId, createdViewInfo);

                                    CameraViewInfo sourceViewInfo = new CameraViewInfo(viewInfo);
                                    sourceViewInfo.Linked = true;
                                    sourceViewInfo.LinkedViewId = createdViewInfo.ViewId;
                                    sourceViewInfo.IsSelected = false;
                                    sModel.CameraViews.Remove(sourceViewInfo.ViewId);
                                    sModel.CameraViews.Add(sourceViewInfo.ViewId, sourceViewInfo);

                                }
                                count++;
                                Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, count });
                            }

                            if (missingItems.Count > 0)
                            {
                                NotificationWindow notificationWindow = new NotificationWindow(missingItems);
                                notificationWindow.Show();
                            }
                        }
                        
                        tg.Assimilate();
                    }
                    catch (Exception ex)
                    {
                        tg.RollBack();
                        MessageBox.Show("Failed to duplicate camera views.\n" + ex.Message, "Duplicate Camera Views", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                modelDictionary.Remove(rModel.ModelId);
                modelDictionary.Add(rModel.ModelId, rModel);
                modelDictionary.Remove(sModel.ModelId);
                modelDictionary.Add(sModel.ModelId, sModel);
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
            catch (Exception ex)
            {
                MessageBox.Show("Failed to duplicate camera views.\n"+ex.Message, "Duplicate Camera Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return duplicated;
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
                            createdView.Name = cameraInfo.ViewName;
                        }
                        createdView.SetOrientation(cameraInfo.Orientation);
                        createdView.CropBoxActive = cameraInfo.IsCropBoxOn;
                        createdView.CropBox = cameraInfo.CropBox;
                        createdView.IsSectionBoxActive = cameraInfo.IsSectionBoxOn;
                        createdView.SetSectionBox(cameraInfo.SectionBox);
                        createdView.DisplayStyle = cameraInfo.Display;
                        //createdView.SetRenderingSettings(cameraInfo.Rendering);

                        foreach (string paramName in cameraInfo.ViewParameters.Keys)
                        {
                            Parameter sourceParam = cameraInfo.ViewParameters[paramName];
                            Parameter recipientParam = createdView.LookupParameter(paramName);
                            if (parametersToSkip.Contains(sourceParam.Id.IntegerValue)) { continue; }

                            if (null != recipientParam && sourceParam.HasValue)
                            {
                                if (!recipientParam.IsReadOnly)
                                {
                                    switch (sourceParam.StorageType)
                                    {
                                        case StorageType.Double:
                                            try { recipientParam.Set(sourceParam.AsDouble()); }
                                            catch { }
                                            break;
                                        case StorageType.ElementId:
                                            /*
                                            try { recipientParam.Set(sourceParam.AsElementId()); }
                                            catch { }
                                             */
                                            break;
                                        case StorageType.Integer:
                                            try { recipientParam.Set(sourceParam.AsInteger()); }
                                            catch { }
                                            break;
                                        case StorageType.String:
                                            try { recipientParam.Set(sourceParam.AsString()); }
                                            catch { }
                                            break;
                                    }
                                }
                            }
                        }

                        if (cameraInfo.ViewTemplateId != ElementId.InvalidElementId)
                        {
                            ElementId templateId = GetLinkedItem(sModel, rModel, MapType.ViewTemplate, cameraInfo.ViewTemplateId);
                            if (templateId != ElementId.InvalidElementId && createdView.IsValidViewTemplate(templateId))
                            {
                                createdView.ViewTemplateId = templateId;
                            }
                            else
                            {
                                MissingItem missingItem = new MissingItem(cameraInfo.ViewName, "View Template", "");
                                missingItems.Add(missingItem);
                            }
                        }

                        if (cameraInfo.PhaseId != ElementId.InvalidElementId)
                        {
                            ElementId phaseId = GetLinkedItem(sModel, rModel, MapType.Phase, cameraInfo.PhaseId);
                            if (phaseId != ElementId.InvalidElementId)
                            {
                                Parameter param = createdView.get_Parameter(BuiltInParameter.VIEW_PHASE);
                                if (null != param)
                                {
                                    param.Set(phaseId);
                                }
                            }
                            else
                            {
                                MissingItem missingItem = new MissingItem(cameraInfo.ViewName, "Phase", "");
                                missingItems.Add(missingItem);
                            }
                        }

                        if (viewConfig.ApplyWorksetVisibility && cameraInfo.WorksetVisibilities.Count > 0)
                        {
                            bool worksetFound = true;
                            foreach (WorksetId wsId in cameraInfo.WorksetVisibilities.Keys)
                            {
                                WorksetVisibility wsVisibility = cameraInfo.WorksetVisibilities[wsId];
                                WorksetId worksetId = GetLinkedWorkset(sModel, rModel, wsId);
                                if (worksetId != WorksetId.InvalidWorksetId)
                                {
                                    createdView.SetWorksetVisibility(worksetId, wsVisibility);
                                }
                                else { worksetFound = false; }
                            }
                            if (!worksetFound)
                            {
                                MissingItem missingItem = new MissingItem(cameraInfo.ViewName, "Workset", "");
                                missingItems.Add(missingItem);
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

        private bool DuplicatePlanViews(ModelInfo sModel, ModelInfo rModel)
        {
            bool duplicated = false;
            try
            {
                List<PlanViewInfo> planViews = (List<PlanViewInfo>)dataGridSource.ItemsSource;
                ViewFamilyType viewFamilyType = GetViewFamilyType(rModel.ModelDoc, selectedViewFamily);
                
                using (TransactionGroup tg = new TransactionGroup(rModel.ModelDoc))
                {
                    tg.Start("Duplicate Plan Views");
                    try
                    {
                        var selectedViews = from pView in planViews where pView.IsSelected  && !pView.Linked select pView;
                        if (selectedViews.Count() > 0)
                        {
                            missingItems = new List<MissingItem>();
                            progressBar.Value = 0;
                            progressBar.Maximum = selectedViews.Count();

                            UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
                            
                            double count = 0;
                            foreach (PlanViewInfo viewInfo in selectedViews)
                            {
                                PlanViewInfo createdViewInfo = null;
                                bool duplicatedView = DuplicatePlanView(sModel, rModel, viewInfo, viewFamilyType, out createdViewInfo);
                                if (duplicatedView && null != createdViewInfo)
                                {
                                    rModel.PlanViews.Add(createdViewInfo.ViewId, createdViewInfo);

                                    PlanViewInfo sourceViewInfo = new PlanViewInfo(viewInfo);
                                    sourceViewInfo.Linked = true;
                                    sourceViewInfo.LinkedViewId = createdViewInfo.ViewId;
                                    sourceViewInfo.IsSelected = false;
                                    sModel.PlanViews.Remove(sourceViewInfo.ViewId);
                                    sModel.PlanViews.Add(sourceViewInfo.ViewId, sourceViewInfo);
                                }
                                count++;
                                Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, count });
                            }

                            if (missingItems.Count > 0)
                            {
                                NotificationWindow notificationWindow = new NotificationWindow(missingItems);
                                notificationWindow.Show();
                            }
                        }
                        
                        tg.Assimilate();
                        
                    }
                    catch (Exception ex)
                    {
                        tg.RollBack();
                        MessageBox.Show("Failed to duplicate camera views.\n" + ex.Message, "Duplicate Camera Views", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                modelDictionary.Remove(rModel.ModelId);
                modelDictionary.Add(rModel.ModelId, rModel);
                modelDictionary.Remove(sModel.ModelId);
                modelDictionary.Add(sModel.ModelId, sModel);
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
            catch (Exception ex)
            {
                MessageBox.Show("Failed to duplicate plan views.\n"+ex.Message, "Duplicate Plan Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return duplicated;
        }

        private bool DuplicatePlanView(ModelInfo sModel, ModelInfo rModel, PlanViewInfo planInfo, ViewFamilyType vFamilyType, out PlanViewInfo createdPlanInfo)
        {
            bool duplicated = false;
            createdPlanInfo = null;
            try
            {
                Document sourceDoc = sModel.ModelDoc;
                Document recipientDoc = rModel.ModelDoc;

                using (Transaction trans = new Transaction(recipientDoc))
                {
                    trans.Start("Create Plan View");
                    try
                    {
                        ViewPlan createdView = null;
                        ElementId levelId = GetLinkedItem(sModel, rModel, MapType.Level, planInfo.LevelId);
                        if (levelId != ElementId.InvalidElementId)
                        {
                            if (planInfo.PlanViewType == ViewType.AreaPlan)
                            {
                                ElementId areaSchemeId = GetLinkedItem(sModel, rModel, MapType.AreaScheme, planInfo.AreaSchemeId);
                                if (areaSchemeId != ElementId.InvalidElementId)
                                {
                                    createdView = ViewPlan.CreateAreaPlan(recipientDoc, areaSchemeId, levelId);
                                }
                                else
                                {
                                    MissingItem missingItem = new MissingItem(planInfo.ViewName, "Area Scheme", "");
                                    missingItems.Add(missingItem);
                                }
                            }
                            else
                            {
                                createdView = ViewPlan.Create(recipientDoc, vFamilyType.Id, levelId);
                            }

                            if (null != createdView)
                            {
                                if (CanHaveViewName(rModel, planInfo.ViewName))
                                {
                                    createdView.Name = planInfo.ViewName;
                                }
                                createdView.CropBoxActive = planInfo.IsCropBoxOn;
                                createdView.CropBox = planInfo.CropBox;
                                createdView.DisplayStyle = planInfo.Display;

                                foreach (string paramName in planInfo.ViewParameters.Keys)
                                {
                                    Parameter sourceParam = planInfo.ViewParameters[paramName];
                                    Parameter recipientParam = createdView.LookupParameter(paramName);
                                    if (parametersToSkip.Contains(sourceParam.Id.IntegerValue)) { continue; }

                                    if (null != recipientParam && sourceParam.HasValue)
                                    {
                                        if (!recipientParam.IsReadOnly)
                                        {
                                            switch (sourceParam.StorageType)
                                            {
                                                case StorageType.Double:
                                                    try { recipientParam.Set(sourceParam.AsDouble()); }
                                                    catch { }
                                                    break;
                                                case StorageType.ElementId:
                                                    /*try { recipientParam.Set(sourceParam.AsElementId()); }
                                                    catch { }*/
                                                    break;
                                                case StorageType.Integer:
                                                    try { recipientParam.Set(sourceParam.AsInteger()); }
                                                    catch { }
                                                    break;
                                                case StorageType.String:
                                                    try { recipientParam.Set(sourceParam.AsString()); }
                                                    catch { }
                                                    break;
                                            }
                                        }
                                    }
                                }

                                if (planInfo.ViewTemplateId != ElementId.InvalidElementId)
                                {
                                    ElementId templateId = GetLinkedItem(sModel, rModel, MapType.ViewTemplate, planInfo.ViewTemplateId);
                                    if (templateId != ElementId.InvalidElementId && createdView.IsValidViewTemplate(templateId))
                                    {
                                        createdView.ViewTemplateId = templateId;
                                    }
                                    else
                                    {
                                        MissingItem missingItem = new MissingItem(planInfo.ViewName, "View Template", "");
                                        missingItems.Add(missingItem);
                                    }
                                }

                                if (planInfo.ScopeBoxId != ElementId.InvalidElementId)
                                {
                                    ElementId scopeboxId = GetLinkedItem(sModel, rModel, MapType.ScopeBox, planInfo.ScopeBoxId);
                                    if (scopeboxId != ElementId.InvalidElementId)
                                    {
                                        Parameter param = createdView.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP);
                                        if (null != param)
                                        {
                                            param.Set(scopeboxId);
                                        }
                                    }
                                    else
                                    {
                                        MissingItem missingItem = new MissingItem(planInfo.ViewName, "Scope Box", "");
                                        missingItems.Add(missingItem);
                                    }
                                }

                                if (planInfo.PhaseId != ElementId.InvalidElementId)
                                {
                                    ElementId phaseId = GetLinkedItem(sModel, rModel, MapType.Phase, planInfo.PhaseId);
                                    if (phaseId != ElementId.InvalidElementId)
                                    {
                                        Parameter param = createdView.get_Parameter(BuiltInParameter.VIEW_PHASE);
                                        if (null != param)
                                        {
                                            param.Set(phaseId);
                                        }
                                    }
                                    else
                                    {
                                        MissingItem missingItem = new MissingItem(planInfo.ViewName, "Phase", "");
                                        missingItems.Add(missingItem);
                                    }
                                }

                                if (viewConfig.ApplyWorksetVisibility && planInfo.WorksetVisibilities.Count > 0)
                                {
                                    bool worksetFound = true;
                                    foreach (WorksetId wsId in planInfo.WorksetVisibilities.Keys)
                                    {
                                        WorksetVisibility wsVisibility = planInfo.WorksetVisibilities[wsId];
                                        WorksetId worksetId = GetLinkedWorkset(sModel, rModel, wsId);
                                        if (worksetId != WorksetId.InvalidWorksetId)
                                        {
                                            createdView.SetWorksetVisibility(worksetId, wsVisibility);
                                        }
                                        else { worksetFound = false; }
                                    }
                                    if (!worksetFound)
                                    {
                                        MissingItem missingItem = new MissingItem(planInfo.ViewName, "Workset", "");
                                        missingItems.Add(missingItem);
                                    }
                                }

                                createdPlanInfo = new PlanViewInfo(createdView);
                                createdPlanInfo.LinkedViewId = planInfo.ViewId;
                                createdPlanInfo.Linked = true;
                                duplicated = true;
                            }
                        }
                        else
                        {
                            MissingItem missingItem = new MissingItem(planInfo.ViewName, "Level", planInfo.LevelName);
                            missingItems.Add(missingItem);
                        }

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
                MessageBox.Show(planInfo.ViewName +": Failed to duplicate a plan view.\n"+ex.Message, "Duplicate Plan View", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return duplicated;
        }

        private ViewFamilyType GetViewFamilyType(Document doc, ViewFamily viewFamily)
        {
            ViewFamilyType vFamilyType = null;
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            List<ViewFamilyType> viewFamilyTypes = collector.OfClass(typeof(ViewFamilyType)).ToElements().Cast<ViewFamilyType>().ToList();
            var vTypes = from vType in viewFamilyTypes where vType.ViewFamily == viewFamily select vType;
            if (vTypes.Count() > 0)
            {
                vFamilyType = vTypes.First();
            }
            return vFamilyType;
        }

        private ElementId GetLinkedItem(ModelInfo sModel, ModelInfo rModel, MapType mapType, ElementId sourceItemId)
        {
            ElementId linkedId = ElementId.InvalidElementId;
            var mapItems = from map in viewConfig.MapItems
                           where map.MapItemType == mapType
                               && map.SourceModelId == sModel.ModelId && map.RecipientModelId == rModel.ModelId
                               && map.SourceItemId == sourceItemId.IntegerValue
                           select map;
            if (mapItems.Count() > 0)
            {
                linkedId = new ElementId(mapItems.First().RecipientItemId);
            }
            return linkedId;
        }

        private WorksetId GetLinkedWorkset(ModelInfo sModel, ModelInfo rModel, WorksetId sourceWorksetId)
        {
            WorksetId linkedWorksetId = WorksetId.InvalidWorksetId;
            var mapItems = from map in viewConfig.MapItems
                           where map.MapItemType == MapType.Workset
                               && map.SourceModelId == sModel.ModelId && map.RecipientModelId == rModel.ModelId
                               && map.SourceItemId == sourceWorksetId.IntegerValue
                           select map;
            if (mapItems.Count() > 0)
            {
                linkedWorksetId = new WorksetId(mapItems.First().RecipientItemId);
            }
            return linkedWorksetId;
        }

        private bool CanHaveViewName(ModelInfo rModelInfo, string ViewName)
        {
            bool allow = true;
            try
            {
                if (selectedViewType == ViewType.ThreeD)
                {
                    var views = from view in rModelInfo.CameraViews.Values where view.ViewName == ViewName select view;
                    if (views.Count() > 0)
                    {
                        //the same name of view already exist
                        allow = false;
                    }
                }
                else
                {
                    var views = from view in rModelInfo.PlanViews.Values where view.ViewName == ViewName && view.PlanViewType == selectedViewType select view;
                    if (views.Count() > 0)
                    {
                        allow = false;
                    }
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
                if (selectedViewType == ViewType.ThreeD)
                {
                    List<CameraViewInfo> cameraViews = (List<CameraViewInfo>)dataGridSource.ItemsSource;
                    for (int i = cameraViews.Count - 1; i > -1; i--)
                    {
                        CameraViewInfo cameraView = cameraViews[i];
                        cameraView.IsSelected = true;
                        cameraViews.RemoveAt(i);
                        cameraViews.Insert(i, cameraView);
                    }

                    dataGridSource.ItemsSource = null;
                    dataGridSource.ItemsSource = cameraViews;
                }
                else
                {
                    List<PlanViewInfo> planViews = (List<PlanViewInfo>)dataGridSource.ItemsSource;
                    for (int i = planViews.Count - 1; i > -1; i--)
                    {
                        PlanViewInfo planView = planViews[i];
                        planView.IsSelected = true;
                        planViews.RemoveAt(i);
                        planViews.Insert(i, planView);
                    }

                    dataGridSource.ItemsSource = null;
                    dataGridSource.ItemsSource = planViews;
                }
                
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
                if (selectedViewType == ViewType.ThreeD)
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
                else
                {
                    List<PlanViewInfo> planViews = (List<PlanViewInfo>)dataGridSource.ItemsSource;
                    for (int i = planViews.Count - 1; i > -1; i--)
                    {
                        PlanViewInfo planView = planViews[i];
                        planView.IsSelected = false;
                        planViews.RemoveAt(i);
                        planViews.Insert(i, planView);
                    }

                    dataGridSource.ItemsSource = null;
                    dataGridSource.ItemsSource = planViews;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to uncheck all items.\n" + ex.Message, "Uncheck All", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void comboBoxViewType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null != comboBoxViewType.SelectedItem)
            {
                ViewTypeInfo selectedInfo = (ViewTypeInfo)comboBoxViewType.SelectedItem;
                selectedViewType = selectedInfo.ViewTypeEnum;
                selectedViewFamily = selectedInfo.ViewFamilyEnum;

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
                        if (selectedViewType == ViewType.ThreeD)
                        {
                            DisplayCameraView(sModelInfo, rModelInfo);
                        }
                        else
                        {
                            DisplayPlanView(sModelInfo, rModelInfo, selectedViewType);
                        }
                    }
                }
            }
        }

        private void buttonConfiguration_Click(object sender, RoutedEventArgs e)
        {
            try
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
                        ViewConfigurationWindow configWindow = new ViewConfigurationWindow(sModelInfo, rModelInfo, viewConfig);
                        if (configWindow.ShowDialog() == true)
                        {
                            viewConfig = configWindow.ViewConfig;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display configuration settings.\n"+ex.Message, "Configuration ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
    }
}
