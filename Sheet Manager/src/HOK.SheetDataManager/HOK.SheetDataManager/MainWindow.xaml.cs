using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;

namespace HOK.SheetDataManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UIApplication m_app = null;
        private Document m_doc = null;

        private SheetManagerConfiguration config = new SheetManagerConfiguration();
        private LinkedProject currentProject = new LinkedProject();
        private string databaseFile = "";
        private DatabaseManager dbManager = null;
        private RevitSheetData sheetData = null;
        private RevitSheet selectedSheet = null;

        private Dictionary<string/*uniqueId*/, SheetInfo> existingSheets = new Dictionary<string, SheetInfo>();
        private Dictionary<string/*uniqueId*/, RevisionInfo> existingRevisions = new Dictionary<string, RevisionInfo>();
        private Dictionary<string/*uniqueId*/, ViewInfo> existingViews = new Dictionary<string, ViewInfo>();

        private string[] hokSheetParameters = new string[] { "Sheet Number", "Sheet Name", "Volume Number", "Sorted Discipline", "Drawing Type" };
        private string[] hokRevisionParameters = new string[] { "Revision Sequence", "Revision Number", "Revision Description", "Revision Date", "Issued to", "Issued by" };

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
        private UpdateProgressBarDelegate updatePbDelegate;

        public MainWindow(UIApplication uiapp)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;

            config = DataStorageUtil.GetConfiguration(m_doc);
            databaseFile = config.DatabaseFile;
            
            CollectRevitElements();

            InitializeComponent();

            updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);

            if (!string.IsNullOrEmpty(databaseFile) && File.Exists(databaseFile))
            {
                dbManager = new DatabaseManager(databaseFile);
                sheetData = dbManager.ReadDatabase();
                //write linked project info
                UpdateProjectInfo();
               
                CreateMaps();
                DisplaySheetData();
                statusLable.Text = databaseFile;
                buttonSheet.IsEnabled = true;
                buttonRevision.IsEnabled = true;
                buttonRenumber.IsEnabled = true;
                buttonAddView.IsEnabled = true;
                buttonRenameView.IsEnabled = true;
                buttonUpdateRevision.IsEnabled = true;

            }
            toggleImage.Tag = (config.AutoUpdate) ? "On" : "Off";
        }

        private void CollectRevitElements()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<ViewSheet> sheets = collector.OfClass(typeof(ViewSheet)).WhereElementIsNotElementType().ToElements().Cast<ViewSheet>().ToList();
                foreach (ViewSheet sheet in sheets)
                {
                    if (sheet.IsTemplate) { continue; }
                    if (!existingSheets.ContainsKey(sheet.UniqueId))
                    {
                        SheetInfo sheetInfo = new SheetInfo(sheet);
                        existingSheets.Add(sheetInfo.SheetUniqueId, sheetInfo);
                    }

                    if (sheet.GetAllViewports().Count > 0)
                    {
                        foreach (ElementId viewportId in sheet.GetAllViewports())
                        {
                            Viewport vp = m_doc.GetElement(viewportId) as Viewport;
                            if (null != vp)
                            {
                                View placedView = m_doc.GetElement(vp.ViewId) as View;
                                if (null != placedView)
                                {
                                    ViewInfo viewInfo = new ViewInfo(placedView, sheet.Id, vp.Id);
                                }
                            }
                            
                        }
                    }
                }

                collector = new FilteredElementCollector(m_doc);
                List<Revision> revisions = collector.OfClass(typeof(Revision)).WhereElementIsNotElementType().ToElements().Cast<Revision>().ToList();
                foreach (Revision revision in revisions)
                {
                    if (!existingRevisions.ContainsKey(revision.UniqueId))
                    {
                        RevisionInfo revInfo = new RevisionInfo(revision);
                        existingRevisions.Add(revInfo.RevisionUniqueId, revInfo);
                    }
                }

                collector = new FilteredElementCollector(m_doc);
                List<View> views = collector.OfCategory(BuiltInCategory.OST_Views).WhereElementIsNotElementType().ToElements().Cast<View>().ToList();
                foreach (View view in views)
                {
                    if (view.IsTemplate) { continue; }
                    if (!existingViews.ContainsKey(view.UniqueId))
                    {
                        ViewInfo viewInfo = new ViewInfo(view);
                        existingViews.Add(viewInfo.ViewUniqueId, viewInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect Revit elements.\n"+ex.Message, "Collect Revit Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateProjectInfo()
        {
            try
            {
                var foundProjects = from project in sheetData.LinkedProjects.Values where project.FilePath == config.CentralPath select project;
                if (sheetData.LinkedProjects.ContainsKey(config.ModelId))
                {
                    currentProject = sheetData.LinkedProjects[config.ModelId];
                }
                else if (foundProjects.Count() > 0)
                {
                    currentProject = foundProjects.First();
                    config.ModelId = currentProject.Id;
                }
                else
                {
                    currentProject.Id = config.ModelId;
                    currentProject.FilePath = config.CentralPath;
                    currentProject.ProjectNumber = m_doc.ProjectInformation.Number;
                    currentProject.ProjectName = m_doc.ProjectInformation.Name;
                    currentProject.LinkedBy = Environment.UserName;
                    currentProject.LinkedDate = DateTime.Now;

                    bool inserted = dbManager.InsertProjectInfo(currentProject);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update project information.\n" + ex.Message, "Update Project Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CreateMaps()
        {
            //find matching items between database and Revit project
            try
            {
                if (null != sheetData)
                {
                    using (Transaction trans = new Transaction(m_doc))
                    {
                        trans.Start("Create Map");
                        try
                        {
                            List<string> existingSheetIds = existingSheets.Keys.ToList();
                            foreach (string sheetUniqueId in existingSheetIds)
                            {
                                SheetInfo sheetInfo = existingSheets[sheetUniqueId];
                                RevitSheet rSheet = null;

                                var linkFound = from link in sheetData.LinkedSheets.Values where link.LinkedElementId == sheetInfo.SheetUniqueId && link.LinkProject.Id == currentProject.Id select link;
                                if (linkFound.Count() > 0)
                                {
                                    LinkedSheet linkedSheet = linkFound.First();
                                    if (sheetData.Sheets.ContainsKey(linkedSheet.SheetId))
                                    {
                                        rSheet = sheetData.Sheets[linkedSheet.SheetId];
                                        rSheet.Linked = true;
                                        rSheet.CurrentLinkedId = sheetUniqueId;

                                        if (rSheet.Name != sheetInfo.SheetName || rSheet.VolumeNumber != sheetInfo.VolumeNumber || rSheet.DrawingType != sheetInfo.DrawingType || rSheet.SortedDiscipline != sheetInfo.SortedDiscipline)
                                        {
                                            rSheet.Modified = true;
                                            rSheet.ToolTip = "Discrepancies found between the sheet item in the database and sheet parameters";
                                        }
                                        else
                                        {
                                            rSheet.ToolTip = "Linked Sheet ElementId: "+sheetInfo.SheetId.IntegerValue;
                                        }
                                        sheetInfo.LinkedSheetItem = linkedSheet;
                                    }
                                }
                                else
                                {
                                    //link not found: insert sheet item into the database
                                    LinkedSheet linkedSheet = new LinkedSheet(Guid.NewGuid(), Guid.NewGuid(), currentProject, sheetInfo.SheetUniqueId, true);
                                    sheetInfo.LinkedSheetItem = linkedSheet;

                                    rSheet = new RevitSheet(linkedSheet.SheetId);
                                    rSheet.CopySheetInfo(sheetInfo);
                                    rSheet.Linked = true;
                                    rSheet.CurrentLinkedId = sheetUniqueId;
                                    rSheet.Modified = false;

                                    rSheet.ToolTip = "Linked Sheet ElementId: " + sheetInfo.SheetId.IntegerValue;

                                    bool inserted = dbManager.InsertSheetInfo(sheetInfo);
                                }

                                if (null != rSheet)
                                {
                                    sheetData.Sheets.Remove(rSheet.Id);
                                    sheetData.Sheets.Add(rSheet.Id, rSheet);
                                }

                                existingSheets.Remove(sheetInfo.SheetUniqueId);
                                existingSheets.Add(sheetInfo.SheetUniqueId, sheetInfo);
                            }

                            List<string> existingRevisionIds = existingRevisions.Keys.ToList();
                            foreach (string revisionUniqueId in existingRevisionIds)
                            {
                                RevisionInfo revisionInfo = existingRevisions[revisionUniqueId];
                                RevitRevision rRevision = null;

                                var linkFound = from link in sheetData.LinkedRevisions.Values where link.LinkedElementId == revisionInfo.RevisionUniqueId && link.LinkProject.Id == currentProject.Id select link;
                                if (linkFound.Count() > 0)
                                {
                                    LinkedRevision linkedRevision = linkFound.First();
                                    if (sheetData.Revisions.ContainsKey(linkedRevision.RevisionId))
                                    {
                                        rRevision = sheetData.Revisions[linkedRevision.RevisionId];
                                        rRevision.Linked = true;
                                        rRevision.CurrentLinkedId = revisionUniqueId;

                                        if (rRevision.Description != revisionInfo.RevisionDescription || rRevision.Date != revisionInfo.Date || rRevision.IssuedBy != revisionInfo.IssuedBy || rRevision.IssuedTo != revisionInfo.IssuedTo)
                                        {
                                            rRevision.Modified = true;
                                            rRevision.ToolTip = "Discrepancies found between the revision item in the database and revision parameters";
                                        }
                                        else
                                        {
                                            rRevision.ToolTip = "Linked Revision ElementId: "+ revisionInfo.RevisionId.IntegerValue;
                                        }
                                        revisionInfo.LinkedRevisionItem = linkedRevision;
                                    }
                                }
                                else
                                {
                                    //link not found: insert revision item into the database
                                    LinkedRevision linkedRevision = new LinkedRevision(Guid.NewGuid(), Guid.NewGuid(), revisionInfo.RevisionSequence, revisionInfo.RevisionNumber, revisionInfo.NumberType
                                , currentProject, revisionInfo.RevisionUniqueId, true);
                                    revisionInfo.LinkedRevisionItem = linkedRevision;

                                    rRevision = new RevitRevision(linkedRevision.RevisionId);
                                    rRevision.CopyRevisionInfo(revisionInfo);
                                    rRevision.Linked = true;
                                    rRevision.CurrentLinkedId = revisionUniqueId;
                                    rRevision.Modified = false;

                                    rRevision.ToolTip = "Linked Revision ElementId: " + revisionInfo.RevisionId.IntegerValue;

                                    bool inserted = dbManager.InsertRevisionInfo(revisionInfo);

                                }

                                if (null != rRevision)
                                {
                                    sheetData.Revisions.Remove(rRevision.Id);
                                    sheetData.Revisions.Add(rRevision.Id, rRevision);
                                }

                                existingRevisions.Remove(revisionInfo.RevisionUniqueId);
                                existingRevisions.Add(revisionInfo.RevisionUniqueId, revisionInfo);
                            }

                            List<string> existingViewsIds = existingViews.Keys.ToList();
                            foreach (string viewUniqueId in existingViewsIds)
                            {
                                ViewInfo viewInfo = existingViews[viewUniqueId];

                                var views = from view in sheetData.Views.Values where view.Name == viewInfo.ViewName && view.ViewType.ViewType.ToString() == viewInfo.ViewInfoType.ToString() select view;
                                if (views.Count() > 0)
                                {
                                    List<RevitView> viewList = views.ToList();
                                    foreach (RevitView view in viewList)
                                    {
                                        RevitView updatedView = view;
                                        updatedView.Linked = true;
                                        updatedView.LinkedUniqueId = viewInfo.ViewUniqueId;

                                        updatedView.ToolTip = "Linked View ElementId: " + viewInfo.ViewId.IntegerValue;
                                        viewInfo.LinkedDBId = updatedView.Id;

                                        sheetData.Views.Remove(updatedView.Id);
                                        sheetData.Views.Add(updatedView.Id, updatedView);

                                        existingViews.Remove(viewInfo.ViewUniqueId);
                                        existingViews.Add(viewInfo.ViewUniqueId, viewInfo);
                                    }

                                }
                            }
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to create maps.\n" + ex.Message, "Create Maps", MessageBoxButton.OK, MessageBoxImage.Warning);
                            trans.RollBack();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create maps.\n" + ex.Message, "Create Maps", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DisplaySheetData()
        {
            try
            {
                if (null != sheetData)
                {
                    List<Discipline> disciplines = sheetData.Disciplines.Values.OrderBy(o => o.Name).ToList();
                    comboBoxDiscipline.ItemsSource = null;
                    comboBoxDiscipline.ItemsSource = disciplines;
                    comboBoxDiscipline.DisplayMemberPath = "Name";
                    comboBoxDiscipline.SelectedIndex = 0;

                    List<RevitViewType> viewTypes = sheetData.ViewTypes.Values.OrderBy(o => o.Name).ToList();
                    dataGridComboBoxViewType.ItemsSource = null;
                    dataGridComboBoxViewType.ItemsSource = viewTypes;
                    dataGridComboBoxViewType.DisplayMemberPath = "Name";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display Sheet data.\n"+ex.Message, "Display Sheet Data", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #region commands
        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Open a Sheet Database File";
                openFileDialog.DefaultExt = ".sqlite";
                openFileDialog.Filter = "SQLITE File (.sqlite)|*.sqlite";
                if ((bool)openFileDialog.ShowDialog())
                {
                    databaseFile = openFileDialog.FileName;
                    config.DatabaseFile = databaseFile;
                    bool stored = DataStorageUtil.StoreConfiguration(m_doc, config);

                    dbManager = new DatabaseManager(databaseFile);
                    sheetData = dbManager.ReadDatabase();
                    UpdateProjectInfo();
                    CreateMaps();

                    DisplaySheetData();
                    statusLable.Text = databaseFile;
                    buttonSheet.IsEnabled = true;
                    buttonRevision.IsEnabled = true;
                    buttonRenumber.IsEnabled = true;
                    buttonAddView.IsEnabled = true;
                    buttonRenameView.IsEnabled = true;
                    buttonUpdateRevision.IsEnabled = true;

                    App.thisApp.UnregisterSheetUpdater(m_doc);
                    if (config.AutoUpdate)
                    {
                        App.thisApp.RegisterSheetUpdater(m_doc, config);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open the databas file.\n"+ex.Message, "Open Database File", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonSheet_Click(object sender, RoutedEventArgs e)
        {
            SheetUpdater.isSheetManagerOn = true;
            try
            {
                List<RevitSheet> sheetItems = (List<RevitSheet>)dataGridSheets.ItemsSource;
                var selectedSheets = from sheet in sheetItems where sheet.IsSelected select sheet;
                if (selectedSheets.Count() > 0)
                {
                    if (config.TitleblockId != ElementId.InvalidElementId)
                    {
                        ElementId titleBlockId = config.TitleblockId;
                        bool isPlaceholder = config.IsPlaceholder;

                        statusLable.Text = "Creating / Updating Sheets";
                        List<RevitSheet> selectedSheetItems = selectedSheets.ToList();
                        progressBar.Visibility = System.Windows.Visibility.Visible;
                        progressBar.Value = 0;
                        progressBar.Maximum = selectedSheetItems.Count;
                        double value = 0;

                        using (TransactionGroup tg = new TransactionGroup(m_doc))
                        {
                            tg.Start("Update sheets");
                            try
                            {
                                Dictionary<Guid, MessageInfo> messages = new Dictionary<Guid, MessageInfo>();
                                int count = 0;
                                foreach (RevitSheet sheet in selectedSheetItems)
                                {
                                    value++;
                                    System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });
                                    using (Transaction trans = new Transaction(m_doc))
                                    {
                                        trans.Start("Update Sheet");
                                        try
                                        {
                                            ViewSheet viewSheet = null;
                                            if (sheet.Linked)
                                            {
                                                if (sheet.Modified)
                                                {
                                                    if (null != m_doc.GetElement(sheet.CurrentLinkedId))
                                                    {
                                                        viewSheet = m_doc.GetElement(sheet.CurrentLinkedId) as ViewSheet;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (isPlaceholder)
                                                {
                                                    viewSheet = ViewSheet.CreatePlaceholder(m_doc);
                                                }
                                                else
                                                {
                                                    viewSheet = ViewSheet.Create(m_doc, titleBlockId);
                                                }
                                            }

                                            if (null != viewSheet)
                                            {
                                                bool updatedParams = UpdateSheetParameters(viewSheet, sheet);
                                                RevitSheet updatedSheet = sheet;
                                                updatedSheet.Modified = false;
                                                updatedSheet.IsSelected = false;
                                                updatedSheet.Linked = true;
                                                updatedSheet.CurrentLinkedId = viewSheet.UniqueId;

                                                updatedSheet.ToolTip = "Linked Sheet ElementId: " + viewSheet.Id.IntegerValue;

                                                SheetInfo sheetInfo = new SheetInfo(viewSheet);
                                                LinkedSheet linkedSheet = dbManager.GetLinkedSheet(sheetInfo.SheetUniqueId, currentProject.Id);
                                                if (null == linkedSheet)
                                                {
                                                    linkedSheet = new LinkedSheet(Guid.NewGuid(), updatedSheet.Id, currentProject, viewSheet.UniqueId, false);
                                                    sheetData.LinkedSheets.Add(linkedSheet.Id, linkedSheet);

                                                    bool updatedLink = dbManager.UpdateLinkedSheet(linkedSheet);
                                                }
                                                sheetInfo.LinkedSheetItem = linkedSheet;

                                                if (existingSheets.ContainsKey(sheetInfo.SheetUniqueId))
                                                {
                                                    existingSheets.Remove(sheetInfo.SheetUniqueId);
                                                }
                                                existingSheets.Add(sheetInfo.SheetUniqueId, sheetInfo);

                                                if (sheetData.Sheets.ContainsKey(updatedSheet.Id))
                                                {
                                                    sheetData.Sheets.Remove(updatedSheet.Id);
                                                }
                                                sheetData.Sheets.Add(updatedSheet.Id, updatedSheet);
                                            }
                                            trans.Commit();
                                            count++;
                                        }
                                        catch (Exception ex)
                                        {
                                            if (!messages.ContainsKey(sheet.Id))
                                            {
                                                MessageInfo mInfo = new MessageInfo(sheet.Id, sheet, ex.Message);
                                                messages.Add(sheet.Id, mInfo);
                                            }
                                            trans.RollBack();
                                        }
                                    }
                                }
                                if (messages.Count > 0)
                                {
                                    CommandMessageBox messageWindow = new CommandMessageBox(messages, "Sheet Managers - Sheet Creation");
                                    messageWindow.Owner = this;
                                    if (messageWindow.ShowDialog() == true)
                                    {
                                        messageWindow.Close();
                                    }
                                }
                                else if(count > 0)
                                {
                                    MessageBox.Show( count+" sheet itmes are successfully created in the current project!", "Sheets Created", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                tg.Assimilate();
                            }
                            catch (Exception ex)
                            {
                                tg.RollBack();
                                MessageBox.Show("Failed to update sheets.\n" + ex.Message, "Update Sheets", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        progressBar.Visibility = System.Windows.Visibility.Hidden;
                        statusLable.Text = "Ready.";

                        DisplaySheets();
                    }
                    else
                    {
                        MessageBox.Show("Title Block is missing.\n Please select a titl block from the Sheet Settings.", "Title Blcok Missing", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Please select sheet items to create or update sheets.", "Select Sheet Items", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create or update Sheet elements.\n" + ex.Message, "Create/Update Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                SheetUpdater.isSheetManagerOn = false;
            }
        }

        private bool UpdateSheetParameters(ViewSheet sheet, RevitSheet sheetInfo)
        {
            bool updated = false;
            if (!string.IsNullOrEmpty(sheetInfo.Name))
            {
                Parameter param = sheet.get_Parameter(BuiltInParameter.SHEET_NAME);
                if (null != param)
                {
                    try
                    {
                        param.Set(sheetInfo.Name);
                    }
                    catch (Exception ex) { string message = ex.Message; }
                }
            }
            if (!string.IsNullOrEmpty(sheetInfo.Number))
            {
                Parameter param = sheet.get_Parameter(BuiltInParameter.SHEET_NUMBER);
                if (null != param)
                {
                    try
                    {
                        param.Set(sheetInfo.Number);
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }
                }
            }
            if (!string.IsNullOrEmpty(sheetInfo.VolumeNumber))
            {
                Parameter param = sheet.LookupParameter("Volume Number");
                if (null != param)
                {
                    try
                    {
                        param.Set(sheetInfo.VolumeNumber);
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }
                }
            }
            if (!string.IsNullOrEmpty(sheetInfo.DrawingType))
            {
                Parameter param = sheet.LookupParameter("Drawing Type");
                if (null != param)
                {
                    try
                    {
                        param.Set(sheetInfo.DrawingType);
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }
                }
            }
            if (!string.IsNullOrEmpty(sheetInfo.SortedDiscipline))
            {
                Parameter param = sheet.LookupParameter("Sorted Discipline");
                if (null != param)
                {
                    try
                    {
                        param.Set(sheetInfo.SortedDiscipline);
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }
                }
            }
            updated = true;
            return updated;
        }

        private void buttonRevision_Click(object sender, RoutedEventArgs e)
        {
            SheetUpdater.isSheetManagerOn = true;
            using (TransactionGroup tg = new TransactionGroup(m_doc))
            {
                tg.Start("Update Revisions");
                try
                {
                    List<RevitSheet> sheetItems = (List<RevitSheet>)dataGridSheets.ItemsSource;
                    var linkedSheets = from item in sheetItems where item.IsSelected && item.Linked select item;
                    if (linkedSheets.Count() > 0)
                    {
                        statusLable.Text = "Updating Revisions...";

                        progressBar.Visibility = System.Windows.Visibility.Visible;
                        progressBar.Value = 0;
                        progressBar.Maximum = linkedSheets.Count();
                        double value = 0;

                        Dictionary<Guid, MessageInfo> messages = new Dictionary<Guid, MessageInfo>();
                        int count = 0;
                        foreach (RevitSheet rSheet in linkedSheets)
                        {
                            value++;
                            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });
                            if (!string.IsNullOrEmpty(rSheet.CurrentLinkedId))
                            {
                                ViewSheet sheet = m_doc.GetElement(rSheet.CurrentLinkedId) as ViewSheet;
                                if (null != sheet)
                                {
                                    var matrixFound = from map in sheetData.RevisionMatrix.Values where map.SheetId == rSheet.Id select map;
                                    if (matrixFound.Count() > 0)
                                    {
                                        List<ElementId> revisionIds = new List<ElementId>();
                                        foreach (RevisionOnSheet ros in matrixFound)
                                        {
                                            if (sheetData.Revisions.ContainsKey(ros.RevisionId))
                                            {
                                                RevitRevision rRevision = sheetData.Revisions[ros.RevisionId];
                                                if (!string.IsNullOrEmpty(rRevision.CurrentLinkedId))
                                                {
                                                    if (existingRevisions.ContainsKey(rRevision.CurrentLinkedId))
                                                    {
                                                        RevisionInfo revInfo = existingRevisions[rRevision.CurrentLinkedId];
                                                        revisionIds.Add(revInfo.RevisionId);
                                                    }
                                                }
                                            }
                                        }

                                        using (Transaction trans = new Transaction(m_doc))
                                        {
                                            trans.Start("Set Revision");
                                            try
                                            {
                                                sheet.SetAdditionalRevisionIds(revisionIds);
                                                trans.Commit();
                                                count++;
                                            }
                                            catch (Exception ex)
                                            {
                                                if (!messages.ContainsKey(rSheet.Id))
                                                {
                                                    MessageInfo mInfo = new MessageInfo(rSheet.Id, rSheet, ex.Message);
                                                    messages.Add(rSheet.Id, mInfo);
                                                }
                                                trans.RollBack();
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (messages.Count > 0)
                        {
                            CommandMessageBox messageWindow = new CommandMessageBox(messages, "Sheet Managers - Revisions on Sheets");
                            messageWindow.Owner = this;
                            if (messageWindow.ShowDialog() == true)
                            {
                                messageWindow.Close();
                            }
                        }
                        else if(count > 0)
                        {
                            MessageBox.Show(count +" sheet items are successfully updated with revisions!", "Revisions Updated on Sheets", MessageBoxButton.OK, MessageBoxImage.Information);
                            DisplayRevisions();
                            SelectAllSheetItem(false);
                        }
                    }
                    statusLable.Text = "Ready.";
                    progressBar.Visibility = System.Windows.Visibility.Hidden;
                    tg.Assimilate();
                }
                catch (Exception ex)
                {
                    tg.RollBack();
                    MessageBox.Show("Failed to edit revision on sheets.\n" + ex.Message, "Update Revisions", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            SheetUpdater.isSheetManagerOn = false;
        }

        private void buttonRenumber_Click(object sender, RoutedEventArgs e)
        {
            if (sheetData.ReplaceItems.Count > 0)
            {
                var sheetReplaceItems = from item in sheetData.ReplaceItems.Values where item.ItemType == ReplaceType.Sheet && item.ParameterName == "Sheet Number" select item;
                if (sheetReplaceItems.Count() > 0)
                {
                    List<RevitSheet> sheetItems = (List<RevitSheet>)dataGridSheets.ItemsSource;
                    var selectedSheets = from sheet in sheetItems where sheet.IsSelected && sheet.Linked && !string.IsNullOrEmpty(sheet.CurrentLinkedId) select sheet;
                    if (selectedSheets.Count() > 0)
                    {
                        List<RevitSheet> selectedSheetItems = selectedSheets.ToList();

                        statusLable.Text = "Renumbering Sheets...";

                        progressBar.Visibility = System.Windows.Visibility.Visible;
                        progressBar.Value = 0;
                        progressBar.Maximum = selectedSheetItems.Count;
                        double value = 0;

                        Dictionary<Guid, MessageInfo> messages = new Dictionary<Guid, MessageInfo>();
                        int count = 0;

                        using (TransactionGroup tg = new TransactionGroup(m_doc))
                        {
                            tg.Start("Start Renumbering Sheets");
                            try
                            {
                                foreach (RevitSheet sheet in selectedSheetItems)
                                {
                                    value++;
                                    System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });

                                    RevitSheet rSheet = sheetData.Sheets[sheet.Id];
                                    using (Transaction trans = new Transaction(m_doc))
                                    {
                                        trans.Start("Renumber Sheet");
                                        try
                                        {
                                            string oldValue = sheet.Number;
                                            var itemFound = from item in sheetReplaceItems where item.SourceValue == oldValue select item;
                                            if (itemFound.Count() > 0)
                                            {
                                                ReplaceItem replaceItem = itemFound.First();
                                                ViewSheet viewSheet = m_doc.GetElement(sheet.CurrentLinkedId) as ViewSheet;
                                                if (null != viewSheet)
                                                {
                                                     Parameter param = viewSheet.LookupParameter("Sheet Number");
                                                     if (null != param)
                                                     {
                                                         param.Set(replaceItem.TargetValue);
                                                     }

                                                     if (existingSheets.ContainsKey(sheet.CurrentLinkedId))
                                                     {
                                                         SheetInfo sheetInfo = existingSheets[sheet.CurrentLinkedId];
                                                         sheetInfo.SheetNumber = replaceItem.TargetValue;
                                                         existingSheets.Remove(sheetInfo.SheetUniqueId);
                                                         existingSheets.Add(sheetInfo.SheetUniqueId, sheetInfo);
                                                     }

                                                     bool updated = dbManager.UpdateSheetInfo(sheet.Id, "Sheet Number", replaceItem.TargetValue);
                                                     rSheet.Number = replaceItem.TargetValue;
                                                }
                                            }
                                            trans.Commit();
                                            count++;
                                        }
                                        catch (Exception ex)
                                        {
                                            if (!messages.ContainsKey(sheet.Id))
                                            {
                                                MessageInfo mInfo = new MessageInfo(sheet.Id, sheet, ex.Message);
                                                messages.Add(sheet.Id, mInfo);
                                            }
                                            trans.RollBack();
                                        }
                                    }

                                    if (sheetData.Sheets.ContainsKey(rSheet.Id))
                                    {
                                        rSheet.IsSelected = false;
                                        sheetData.Sheets.Remove(rSheet.Id);
                                        sheetData.Sheets.Add(rSheet.Id, rSheet);
                                    }
                                }
                                if (messages.Count > 0)
                                {
                                    CommandMessageBox messageWindow = new CommandMessageBox(messages, "Sheet Managers - Renumber Sheets");
                                    messageWindow.Owner = this;
                                    if (messageWindow.ShowDialog() == true)
                                    {
                                        messageWindow.Close();
                                    }
                                }
                                else if(count >0)
                                {
                                    MessageBox.Show(count + " sheet itmes are successfully renumbered in the current project!", "Sheets Created", MessageBoxButton.OK, MessageBoxImage.Information);
                                }

                                tg.Assimilate();
                                int disciplineIndex = comboBoxDiscipline.SelectedIndex;
                                DisplaySheetData();
                                comboBoxDiscipline.SelectedIndex = disciplineIndex;
                            }
                            catch (Exception ex)
                            {
                                tg.RollBack();
                                MessageBox.Show("Failed to renumber sheets\n" + ex.Message, "Renumber Sheets", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }

                        statusLable.Text = "Ready.";
                        progressBar.Visibility = System.Windows.Visibility.Hidden; 
                    }
                }
            }
        }

        private void buttonSheetSetting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SheetSettingWindow settingWindow = new SheetSettingWindow(m_doc, config);
                settingWindow.Owner = this;
                if (settingWindow.ShowDialog() == true)
                {
                    config = settingWindow.Configuration;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open sheet settings window.\n"+ex.Message, "Open Sheet Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonAddView_Click(object sender, RoutedEventArgs e)
        {

            SheetUpdater.isSheetManagerOn = true;
            using (TransactionGroup tg = new TransactionGroup(m_doc))
            {
                tg.Start("Place Views");
                try
                {
                    List<RevitSheet> sheetItems = (List<RevitSheet>)dataGridSheets.ItemsSource;
                    var linkedSheets = from item in sheetItems where item.IsSelected && item.Linked && !string.IsNullOrEmpty(item.CurrentLinkedId) select item;
                    if (linkedSheets.Count() > 0)
                    {
                        statusLable.Text = "Placing Views...";

                        progressBar.Visibility = System.Windows.Visibility.Visible;
                        progressBar.Value = 0;
                        progressBar.Maximum = linkedSheets.Count();
                        double value = 0;

                        Dictionary<Guid, MessageInfo> messages = new Dictionary<Guid, MessageInfo>();
                        int count = 0;
                        foreach (RevitSheet rSheet in linkedSheets)
                        {
                            value++;
                            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });

                            ViewSheet sheet = m_doc.GetElement(rSheet.CurrentLinkedId) as ViewSheet;
                            if (null != sheet)
                            {
                                var views = from view in sheetData.Views.Values where view.Sheet.Id == rSheet.Id select view;
                                if (views.Count() > 0)
                                {
                                    foreach (RevitView view in views)
                                    {
                                        if (string.IsNullOrEmpty(view.LinkedUniqueId)) { continue; }
                                        else if (existingViews.ContainsKey(view.LinkedUniqueId)) // delete existing viewports if they are different from the sheets to be placed on
                                        {
                                            ViewInfo vInfo = existingViews[view.LinkedUniqueId];
                                            if (vInfo.IsOnSheet)
                                            {
                                                if (vInfo.PlacedSheetId != sheet.Id)
                                                {
                                                    using (Transaction trans = new Transaction(m_doc))
                                                    {
                                                        trans.Start("Delete Viewport");
                                                        try
                                                        {
                                                            m_doc.Delete(vInfo.ViewportId);
                                                            trans.Commit();
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            trans.RollBack();
                                                            string message = ex.Message;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        using (Transaction trans = new Transaction(m_doc))
                                        {
                                            trans.Start("Place View");
                                            try
                                            {
                                                XYZ viewPoint = XYZ.Zero;
                                                ViewInfo vInfo = existingViews[view.LinkedUniqueId];
                                                if (Viewport.CanAddViewToSheet(m_doc, sheet.Id, vInfo.ViewId))
                                                {
                                                    Viewport createdVP = Viewport.Create(m_doc, sheet.Id, vInfo.ViewId, viewPoint);
                                                    if (null != createdVP)
                                                    {
                                                        XYZ centerPoint = createdVP.GetBoxCenter();
                                                        Outline outline = createdVP.GetBoxOutline();
                                                        XYZ diffToMove = new XYZ(view.LocationU + outline.MaximumPoint.X, view.LocationV + outline.MaximumPoint.Y, 0);

                                                        ElementTransformUtils.MoveElement(m_doc, createdVP.Id, diffToMove);
                                                    }

                                                    trans.Commit();
                                                    count++;
                                                }
                                                else if (view.ViewType.ViewType == ViewTypeEnum.Schedule)
                                                {
                                                    ScheduleSheetInstance createdInstance = ScheduleSheetInstance.Create(m_doc, sheet.Id, vInfo.ViewId, viewPoint);
                                                    if (null != createdInstance)
                                                    {
                                                        XYZ centerPoint = createdInstance.Point;
                                                        BoundingBoxXYZ bbBox = createdInstance.get_BoundingBox(sheet);
                                                        XYZ diffToMove = new XYZ(view.LocationU - bbBox.Min.X, view.LocationV - bbBox.Min.Y, 0);

                                                        ElementTransformUtils.MoveElement(m_doc, createdInstance.Id, diffToMove);

                                                    }
                                                    
                                                    trans.Commit();
                                                    count++;
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                if (!messages.ContainsKey(rSheet.Id))
                                                {
                                                    MessageInfo mInfo = new MessageInfo(rSheet.Id, rSheet, ex.Message);
                                                    messages.Add(rSheet.Id, mInfo);
                                                }
                                                trans.RollBack();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (messages.Count > 0)
                        {
                            CommandMessageBox messageWindow = new CommandMessageBox(messages, "Sheet Managers - Place Views");
                            messageWindow.Owner = this;
                            if (messageWindow.ShowDialog() == true)
                            {
                                messageWindow.Close();
                            }
                        }
                        else if(count > 0)
                        {
                            MessageBox.Show(count +" view items are successfully placed on sheets.", "Views Placed", MessageBoxButton.OK, MessageBoxImage.Information);
                            DisplayViewItems();
                            SelectAllSheetItem(false);
                        }
                        statusLable.Text = "Ready.";
                        progressBar.Visibility = System.Windows.Visibility.Hidden;
                    }
                    tg.Assimilate();
                }
                catch (Exception ex)
                {
                    tg.RollBack();
                    MessageBox.Show("Failed to add views on sheets.\n" + ex.Message, "Add Views on Sheets", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            SheetUpdater.isSheetManagerOn = false;
        }

        private void buttonRenameView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sheetData.ReplaceItems.Count > 0)
                {
                    statusLable.Text = "Renaming Views...";

                    progressBar.Visibility = System.Windows.Visibility.Visible;
                    progressBar.Value = 0;
                    progressBar.Maximum = existingViews.Count;
                    double value = 0;

                    var sheetReplaceItems = from item in sheetData.ReplaceItems.Values where item.ItemType == ReplaceType.View && item.ParameterName == "View Name" select item;
                    using (TransactionGroup tg = new TransactionGroup(m_doc))
                    {
                        tg.Start("Start Renaming Views");
                        try
                        {
                            List<string> existingViewIds = existingViews.Keys.ToList();
                            Dictionary<Guid, MessageInfo> messages = new Dictionary<Guid, MessageInfo>();
                            int count = 0;
                            foreach (string viewUniqueId in existingViewIds)
                            {
                                value++;
                                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });

                                ViewInfo viewInfo = existingViews[viewUniqueId];
                                string oldValue = viewInfo.ViewName;
                                var itemFound = from item in sheetReplaceItems where item.SourceValue == oldValue select item;
                                if (itemFound.Count() > 0)
                                {
                                    ReplaceItem replaceItem = itemFound.First();
                                    View view = m_doc.GetElement(viewUniqueId) as View;
                                    if (null != view)
                                    {
                                        using (Transaction trans = new Transaction(m_doc))
                                        {
                                            trans.Start("Rename View");
                                            try
                                            {
                                                Parameter param = view.LookupParameter("View Name");
                                                if (null != param)
                                                {
                                                    param.Set(replaceItem.TargetValue);
                                                    viewInfo.ViewName = replaceItem.TargetValue;

                                                    existingViews.Remove(viewUniqueId);
                                                    existingViews.Add(viewUniqueId, viewInfo);

                                                    //omit: update DB for views
                                                }
                                                trans.Commit();
                                                count++;
                                            }
                                            catch (Exception ex)
                                            {
                                                if (!messages.ContainsKey(replaceItem.ItemId))
                                                {
                                                    MessageInfo mInfo = new MessageInfo(replaceItem.ItemId, replaceItem.SourceValue, ex.Message);
                                                    messages.Add(replaceItem.ItemId, mInfo);
                                                }
                                                trans.RollBack();
                                                string message = ex.Message;
                                            }
                                        }
                                    }
                                }
                            }
                            if (messages.Count > 0)
                            {
                                CommandMessageBox messageWindow = new CommandMessageBox(messages, "Sheet Managers - Renumber Sheets");
                                messageWindow.Owner = this;
                                if (messageWindow.ShowDialog() == true)
                                {
                                    messageWindow.Close();
                                }
                            }
                            else
                            {
                                MessageBox.Show(count + " views are successfully renumbered!", "Views Renamed", MessageBoxButton.OK, MessageBoxImage.Information);
                            }

                            tg.Assimilate();

                            int disciplineIndex = comboBoxDiscipline.SelectedIndex;
                            DisplaySheetData();
                            comboBoxDiscipline.SelectedIndex = disciplineIndex;
                        }
                        catch (Exception ex)
                        {
                            tg.RollBack();
                            MessageBox.Show("Failed to rename views\n" + ex.Message, "Rename View", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    statusLable.Text = "Ready";
                    progressBar.Visibility = System.Windows.Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to rename views.\n"+ex.Message, "Rename Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonUpdateRevision_Click(object sender, RoutedEventArgs e)
        {
            SheetUpdater.isSheetManagerOn = true;
            RevisionUpdater.isSheetManagerOn = true;
            try
            {
                RevisionWindow revWindow = new RevisionWindow(m_app, dbManager, sheetData, existingRevisions, currentProject);
                revWindow.Owner = this;
                if (revWindow.ShowDialog() == true)
                {
                    sheetData = revWindow.SheetData;
                    existingRevisions = revWindow.ExistingRevisions;

                    int disciplineIndex = comboBoxDiscipline.SelectedIndex;
                    DisplaySheetData();
                    comboBoxDiscipline.SelectedIndex = disciplineIndex;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update revisions.\n" + ex.Message, "Update Revisions", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                SheetUpdater.isSheetManagerOn = false;
                RevisionUpdater.isSheetManagerOn = false;
            }
        }

        private void buttonUpdater_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (config.AutoUpdate)
                {
                    MessageBoxResult result = MessageBox.Show("Would you like to disable the updating feature of writing changes on linked database?", "Updater Off", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        //change to updater off
                        config.AutoUpdate = false;
                        App.thisApp.UnregisterSheetUpdater(m_doc);
                        bool stored = DataStorageUtil.StoreConfiguration(m_doc, config);
                        toggleImage.Tag = "Off";
                    }
                }
                else
                {
                    //change to updater on
                    config.AutoUpdate = true;
                    App.thisApp.RegisterSheetUpdater(m_doc, config);
                    bool stored = DataStorageUtil.StoreConfiguration(m_doc, config);
                    toggleImage.Tag = "On";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set the configuration for the Sheet Manager.\n" + ex.Message, "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        #endregion

        private void comboBoxDiscipline_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DisplaySheets();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply the change of discipline.\n"+ex.Message, "Discipline Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DisplaySheets()
        {
            try
            {
                if (null != comboBoxDiscipline.SelectedItem && null != sheetData)
                {
                    Discipline selectedDiscipline = comboBoxDiscipline.SelectedItem as Discipline;
                    dataGridSheets.ItemsSource = null;
                    dataGridView.ItemsSource = null;
                    dataGridRevisions.ItemsSource = null;

                    var collectedSheets = from sheet in sheetData.Sheets.Values where sheet.DisciplineObj.Id == selectedDiscipline.Id select sheet;
                    if (collectedSheets.Count() > 0)
                    {
                        List<RevitSheet> sheetList = collectedSheets.OrderBy(o => o.Number).ToList();
                        dataGridSheets.ItemsSource = sheetList;
                        dataGridSheets.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display sheet items.\n" + ex.Message, "Display Sheet Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void dataGridSheets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (null != dataGridSheets.SelectedItem)
                {
                    selectedSheet = dataGridSheets.SelectedItem as RevitSheet;
                    dataGridView.ItemsSource = null;
                    dataGridRevisions.ItemsSource = null;

                    if (null != selectedSheet)
                    {
                        DisplayViewItems();
                        DisplayRevisions();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select a sheet item.\n" + ex.Message, "Select a Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DisplayViewItems()
        {
            try
            {
                dataGridView.ItemsSource = null;
                var views = from view in sheetData.Views.Values where null != view.Sheet select view;
                var collectedViews = from view in views where view.Sheet.Id == selectedSheet.Id select view;
                if (collectedViews.Count() > 0)
                {
                    List<RevitView> viewList = new List<RevitView>();
                    if (selectedSheet.Linked && !string.IsNullOrEmpty(selectedSheet.CurrentLinkedId))
                    {
                        ViewSheet viewSheet = m_doc.GetElement(selectedSheet.CurrentLinkedId) as ViewSheet;
                        if (null != viewSheet)
                        {
                            List<ElementId> placedViewIds = viewSheet.GetAllPlacedViews().ToList();
                            foreach (RevitView view in collectedViews)
                            {
                                RevitView updatedView = view;
                                if (updatedView.Linked)
                                {
                                    if (existingViews.ContainsKey(updatedView.LinkedUniqueId))
                                    {
                                        ViewInfo vInfo = existingViews[updatedView.LinkedUniqueId];
                                        if (placedViewIds.Contains(vInfo.ViewId))
                                        {
                                            updatedView.UnPlaced = false;
                                            updatedView.ToolTip = "Placed on Sheet.";
                                        }
                                    }
                                    else
                                    {
                                        updatedView.UnPlaced = true;
                                        updatedView.ToolTip = "Not Placed.";
                                    }
                                }
                                else
                                {
                                    updatedView.UnPlaced = false;
                                    updatedView.ToolTip = "Not Linked.";
                                }
                                viewList.Add(updatedView);
                            }
                        }
                    }
                    else
                    {
                        foreach (RevitView view in collectedViews)
                        {
                            RevitView updatedView = view;
                            if (updatedView.Linked)
                            {
                                updatedView.UnPlaced = true;
                                updatedView.ToolTip = "Not Placed.";
                            }
                            else
                            {
                                updatedView.UnPlaced = false;
                                updatedView.ToolTip = "Not Linked.";
                            }
                            viewList.Add(updatedView);
                        }
                    }

                    viewList = viewList.OrderBy(o => o.Name).ToList();
                    dataGridView.ItemsSource = viewList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display view items.\n" + ex.Message, "Display View Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DisplayRevisions()
        {
            try
            {
                dataGridRevisions.ItemsSource = null;
                var maps = from map in sheetData.RevisionMatrix.Values where map.SheetId == selectedSheet.Id select map;
                if (maps.Count() > 0)
                {
                    List<RevitRevision> revisionList = new List<RevitRevision>();
                    List<RevisionOnSheet> revisionOnSheets = maps.ToList();
                    if (selectedSheet.Linked && !string.IsNullOrEmpty(selectedSheet.CurrentLinkedId))
                    {
                        ViewSheet sheet = m_doc.GetElement(selectedSheet.CurrentLinkedId) as ViewSheet;
                        if (null != sheet)
                        {
                            List<ElementId> revisionIds = sheet.GetAllRevisionIds().ToList();
                            foreach (RevisionOnSheet ros in revisionOnSheets)
                            {
                                if (sheetData.Revisions.ContainsKey(ros.RevisionId))
                                {
                                    RevitRevision rvtRevision = sheetData.Revisions[ros.RevisionId];
                                    if (rvtRevision.Linked && existingRevisions.ContainsKey(rvtRevision.CurrentLinkedId))
                                    {
                                        RevisionInfo revInfo = existingRevisions[rvtRevision.CurrentLinkedId];
                                        if (revisionIds.Contains(revInfo.RevisionId))
                                        {
                                            rvtRevision.NotIncluded = false;
                                            rvtRevision.ToolTip = "Included in Sheet";
                                        }
                                        else
                                        {
                                            rvtRevision.NotIncluded = true;
                                            rvtRevision.ToolTip = "Not Included";
                                        }
                                    }
                                    else
                                    {
                                        rvtRevision.NotIncluded = false;
                                        rvtRevision.ToolTip = "Not Linked";
                                    }
                                    revisionList.Add(rvtRevision);
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (RevisionOnSheet ros in revisionOnSheets)
                        {
                            if (sheetData.Revisions.ContainsKey(ros.RevisionId))
                            {
                                RevitRevision rvtRevision = sheetData.Revisions[ros.RevisionId];
                                if (rvtRevision.Linked)
                                {
                                    rvtRevision.NotIncluded = true;
                                    rvtRevision.ToolTip = "Not Included";
                                }
                                else
                                {
                                    rvtRevision.NotIncluded = false;
                                    rvtRevision.ToolTip = "Not Linked";
                                }
                                revisionList.Add(rvtRevision);
                            }
                        }
                    }

                    dataGridRevisions.ItemsSource = revisionList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display revision items.\n" + ex.Message, "Display Revision Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCheckAll_Click(object sender, RoutedEventArgs e)
        {
            SelectAllSheetItem(true);
        }

        private void buttonCheckNone_Click(object sender, RoutedEventArgs e)
        {
            SelectAllSheetItem(false);
        }

        private void SelectAllSheetItem(bool selected)
        {
            try
            {
                if (null != dataGridSheets.ItemsSource)
                {
                    List<RevitSheet> sheets = (List<RevitSheet>)dataGridSheets.ItemsSource;
                    List<RevitSheet> updatedSheets = new List<RevitSheet>();
                    foreach (RevitSheet sheet in sheets)
                    {
                        RevitSheet rs = sheet;
                        rs.IsSelected = selected;
                        updatedSheets.Add(rs);
                    }
                    dataGridSheets.ItemsSource = null;
                    dataGridSheets.ItemsSource = updatedSheets;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select all sheet items.\n" + ex.Message, "Select All Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }

    public class SheetInfo
    {
        private ElementId sheetId = ElementId.InvalidElementId;
        private string sheetUniqueId = "";
        private string sheetNumber = "";
        private string sheetName = "";
        private bool isPlaceHolder = false;
        private string volumeNumber = "";
        private string drawingType = "";
        private string sortedDiscipline = "";
        private LinkedSheet linkedSheetItem = new LinkedSheet();

        public ElementId SheetId { get { return sheetId; } set { sheetId = value; } }
        public string SheetUniqueId { get { return sheetUniqueId; } set { sheetUniqueId = value; } }
        public string SheetNumber { get { return sheetNumber; } set { sheetNumber = value; } }
        public string SheetName { get { return sheetName; } set { sheetName = value; } }
        public bool IsPlaceHolder { get { return isPlaceHolder; } set { isPlaceHolder = value; } }
        public string VolumeNumber { get { return volumeNumber; } set { volumeNumber = value; } }
        public string DrawingType { get { return drawingType; } set { drawingType = value; } }
        public string SortedDiscipline { get { return sortedDiscipline; } set { sortedDiscipline = value; } }
        public LinkedSheet LinkedSheetItem { get { return linkedSheetItem; } set { linkedSheetItem = value; } }

        public SheetInfo(ViewSheet sheet)
        {
            sheetId = sheet.Id;
            sheetUniqueId = sheet.UniqueId;
            sheetNumber = sheet.SheetNumber;
            sheetName = sheet.ViewName;
            isPlaceHolder = sheet.IsPlaceholder;

            Parameter param = sheet.LookupParameter("Volume Number");
            if (null != param)
            {
                volumeNumber = param.AsString();
            }
            param = sheet.LookupParameter("Drawing Type");
            if (null != param)
            {
                drawingType = param.AsString();
            }
            param = sheet.LookupParameter("Sorted Discipline");
            if (null != param)
            {
                sortedDiscipline = param.AsString();
            }
        }

    }

    public class RevisionInfo
    {
        private ElementId revisionId = ElementId.InvalidElementId;
        private string revisionUniqueId = "";
        private int revisionSequence = 0;
        private string revisionNumber = "";
        private string revisionDescription = "";
        private string issuedBy = "";
        private string issuedTo = "";
        private RevisionNumberType numberType = RevisionNumberType.None;
        private string date = "";
        private LinkedRevision linkedRevisionItem = new LinkedRevision();

        public ElementId RevisionId { get { return revisionId; } set { revisionId = value; } }
        public string RevisionUniqueId { get { return revisionUniqueId; } set { revisionUniqueId = value; } }
        public int RevisionSequence { get { return revisionSequence; } set { revisionSequence = value; } }
        public string RevisionNumber { get { return revisionNumber; } set { revisionNumber = value; } }
        public string RevisionDescription { get { return revisionDescription; } set { revisionDescription = value; } }
        public string IssuedBy { get { return issuedBy; } set { issuedBy = value; } }
        public string IssuedTo { get { return issuedTo; } set { issuedTo = value; } }
        public RevisionNumberType NumberType { get { return numberType; } set { numberType = value; } }
        public string Date { get { return date; } set { date = value; } }
        public LinkedRevision LinkedRevisionItem { get { return linkedRevisionItem; } set { linkedRevisionItem = value; } }

        public RevisionInfo()
        {

        }

        public RevisionInfo(Revision revision)
        {
            revisionId = revision.Id;
            revisionUniqueId = revision.UniqueId;
            revisionSequence = revision.SequenceNumber;
            revisionNumber = revision.RevisionNumber;
            revisionDescription = revision.Description;
            issuedBy = revision.IssuedBy;
            issuedTo = revision.IssuedTo;
            numberType = revision.NumberType;
            date = revision.RevisionDate;

        }
    }

    public class ViewInfo
    {
        private ElementId viewId = ElementId.InvalidElementId;
        private string viewUniqueId = "";
        private string viewName = "";
        private ViewType viewInfoType = ViewType.Undefined;
        private bool isOnSheet = false;
        private ElementId placedSheetId = ElementId.InvalidElementId;
        private ElementId viewportId = ElementId.InvalidElementId;
        private Guid linkedDBId = Guid.Empty;

        public ElementId ViewId { get { return viewId; } set { viewId = value; } }
        public string ViewUniqueId { get { return viewUniqueId; } set { viewUniqueId = value; } }
        public string ViewName { get { return viewName; } set { viewName = value; } }
        public ViewType ViewInfoType { get { return viewInfoType; } set { viewInfoType = value; } }
        public bool IsOnSheet { get { return isOnSheet; } set { isOnSheet = value; } }
        public ElementId PlacedSheetId { get { return placedSheetId; } set { placedSheetId = value; } }
        public ElementId ViewportId { get { return viewportId; } set { viewportId = value; } }
        public Guid LinkedDBId { get { return linkedDBId; } set { linkedDBId = value; } }

        public ViewInfo(View view)
        {
            viewId = view.Id;
            viewUniqueId = view.UniqueId;
            viewName = view.Name;
            viewInfoType = view.ViewType;
        }

        public ViewInfo(View view, ElementId sheetId, ElementId vpId)
        {
            viewId = view.Id;
            viewUniqueId = view.UniqueId;
            viewName = view.Name;
            viewInfoType = view.ViewType;
            isOnSheet = true;
            placedSheetId = sheetId;
            viewportId = vpId;
        }
        
    }
}
