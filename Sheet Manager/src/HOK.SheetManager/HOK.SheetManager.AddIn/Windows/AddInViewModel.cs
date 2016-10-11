using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.SheetManager.AddIn.Classes;
using HOK.SheetManager.AddIn.Updaters;
using HOK.SheetManager.AddIn.Utils;
using HOK.SheetManager.Classes;
using HOK.SheetManager.Database;
using HOK.SheetManager.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HOK.SheetManager.AddIn.Windows
{
    public class AddInViewModel : INotifyPropertyChanged
    {
        private ExternalEvent m_event;
        private SheetManagerHandler m_handler;
        private Document currentDoc = null;

        private SheetManagerConfiguration config = new SheetManagerConfiguration();
        private Guid linkedProjectId = Guid.Empty; 
        private string dbFile = "";
        private LinkedProject currentProject = new LinkedProject();
        private RevitSheetData rvtSheetData = new RevitSheetData();
        private RevitSheet selectedSheet = null;
        private List<RevitItemMapper> selectedItemMappers = new List<RevitItemMapper>();

        private bool databaseOpened = false;
        private bool autoUpdate = false;
        private string statusText = "Ready";

        private RelayCommand connectDBCommand;
        private RelayCommand updateSheetCommand;
        private RelayCommand updateRevisionCommand;
        private RelayCommand updateRevisionOnSheetCommand;
        private RelayCommand placeViewCommand;
        private RelayCommand importViewCommand;
        private RelayCommand renumberSheetCommand;
        private RelayCommand renameViewCommand;
        private RelayCommand settingCommand;
        private RelayCommand helpCommand;
        private RelayCommand checkAllCommand;
        private RelayCommand uncheckAllCommand;

        public ExternalEvent ExtEvent { get { return m_event; } set { m_event = value; NotifyPropertyChanged("ExtEvent"); } }
        public SheetManagerHandler Handler { get { return m_handler; } set { m_handler = value; NotifyPropertyChanged("Handler"); } }

        public SheetManagerConfiguration Configuration { get { return config; } set { config = value; NotifyPropertyChanged("Configuration"); } }
        public Guid LinkedProjectId { get { return linkedProjectId; } set { linkedProjectId = value; NotifyPropertyChanged("LinkedProjectId"); } }
        public string DBFile { get { return dbFile; } set { dbFile = value; NotifyPropertyChanged("DatabaseOpened"); } }
        public LinkedProject CurrentProject { get { return currentProject; } set { currentProject = value; NotifyPropertyChanged("CurrentProject"); } }
        public RevitSheetData RvtSheetData { get { return rvtSheetData; } set { rvtSheetData = value; NotifyPropertyChanged("RvtSheetData"); } }
        public RevitSheet SelectedSheet { get { return selectedSheet; } set { selectedSheet = value; NotifyPropertyChanged("SelectedSheet"); } }
        public bool DatabaseOpened { get { return databaseOpened; } set { databaseOpened = value; NotifyPropertyChanged("DatabaseOpened"); } }
        public bool AutoUpdate { get { return autoUpdate; } set { autoUpdate = value; NotifyPropertyChanged("AutoUpdate"); } }
        public string StatusText { get { return statusText; } set { statusText = value; NotifyPropertyChanged("StatusText"); } }

        public ICommand ConnectDBCommand { get { return connectDBCommand; } }
        public ICommand UpdateSheetCommand { get { return updateSheetCommand; } }
        public ICommand UpdateRevisionCommand { get { return updateRevisionCommand; } }
        public ICommand UpdateRevisionOnSheetCommand { get { return updateRevisionOnSheetCommand; } }
        public ICommand PlaceViewCommand { get { return placeViewCommand; } }
        public ICommand ImportViewCommand { get { return importViewCommand; } }
        public ICommand RenumberSheetCommand { get { return renumberSheetCommand; } }
        public ICommand RenameViewCommand { get { return renameViewCommand; } }
        public ICommand SettingCommand { get { return settingCommand; } }
        public ICommand HelpCommand { get { return helpCommand; } }
        public ICommand CheckAllCommand { get { return checkAllCommand; } }
        public ICommand UncheckAllCommand { get { return uncheckAllCommand; } }

        public AddInViewModel(SheetManagerConfiguration configuration)
        {
            config = configuration;
            linkedProjectId = config.ModelId;
            dbFile = config.DatabaseFile;
            this.AutoUpdate = config.AutoUpdate;

            RegisterCommands();
        }

        private void RegisterCommands()
        {
            connectDBCommand = new RelayCommand(param => this.ConnectDBExecuted(param));
            updateSheetCommand = new RelayCommand(param => this.UpdateSheetExectued(param));
            updateRevisionCommand = new RelayCommand(param => this.UpdateRevisionExecuted(param));
            updateRevisionOnSheetCommand = new RelayCommand(param => this.UpdateRevisionOnSheetExecuted(param));
            placeViewCommand = new RelayCommand(param => this.PlaceViewExecuted(param));
            importViewCommand = new RelayCommand(param => this.ImportViewExecuted(param));
            renumberSheetCommand = new RelayCommand(param => this.RenumberSheetExecuted(param));
            renameViewCommand = new RelayCommand(param => this.RenameViewExecuted(param));
            settingCommand = new RelayCommand(param => this.SettingExecuted(param));
            helpCommand = new RelayCommand(param => this.HelpExecuted(param));
            checkAllCommand = new RelayCommand(param => this.CheckAllExectued(param));
            uncheckAllCommand = new RelayCommand(param => this.UncheckAllExecuted(param));
        }

        public void ConnectDBExecuted(object param)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Open a Sheet Database File";
                openFileDialog.DefaultExt = ".sqlite";
                openFileDialog.Filter = "SQLITE File (.sqlite)|*.sqlite";
                if ((bool)openFileDialog.ShowDialog())
                {
                    bool openedDB = OpenDatabase(openFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public bool OpenDatabase(string file)
        {
            bool opened = false;
            try
            {
                if (File.Exists(file))
                {
                    m_handler.ViewModel = this;
                    m_handler.Request.Make(RequestId.None); //in order to refresh document
                    m_event.Raise();

                    currentDoc = m_handler.CurrentDocument;

                    this.DBFile = file;
                    this.Configuration.DatabaseFile = file;
                    this.RvtSheetData = SheetDataReader.ReadSheetDatabase(file, rvtSheetData);

                    bool writerOpened = SheetDataWriter.OpenDatabase(file);

                    UpdateProjectInfo(currentDoc);

                    LinkStatusChecker.CheckLinkStatus(currentDoc, currentProject.Id, ref rvtSheetData, config.AutoUpdate);
                    this.RvtSheetData.SelectedDisciplineIndex = 0;
                    this.DatabaseOpened = true;
                    this.StatusText = file;

                    ProgressManager.databaseFilePath = file;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return opened;
        }

        private void UpdateProjectInfo(Document doc)
        {
            try
            {
                bool existProject = false;
                var foundProjects = from project in rvtSheetData.LinkedProjects where project.Id == config.ModelId || project.FilePath == config.CentralPath select project;
                if (foundProjects.Count() > 0)
                {
                    currentProject = foundProjects.First();
                    config.ModelId = currentProject.Id;
                    existProject = true;
                }
                else
                {
                    currentProject.Id = config.ModelId;
                }

                currentProject.FilePath = config.CentralPath;
                currentProject.ProjectNumber = doc.ProjectInformation.Number;
                currentProject.ProjectName = doc.ProjectInformation.Name;
                currentProject.LinkedBy = Environment.UserName;
                currentProject.LinkedDate = DateTime.Now;

                if (existProject)
                {
                    bool updated = SheetDataWriter.ChangeLinkedProject(currentProject, CommandType.UPDATE);
                }
                else
                {
                    bool updated = SheetDataWriter.ChangeLinkedProject(currentProject, CommandType.INSERT);
                }

                m_handler.ViewModel = this;
                m_handler.Request.Make(RequestId.StoreConfiguration);
                m_event.Raise();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void UpdateSheetExectued(object param)
        {
            try
            {
                if (SheetDataWriter.dbFile != dbFile) { SheetDataWriter.OpenDatabase(dbFile); }

                m_handler.ViewModel = this;
                m_handler.Request.Make(RequestId.UpdateSheet);
                m_event.Raise();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public bool UpdateSheets(Document doc)
        {
            bool updated = false;
            try
            {
                currentDoc = doc;
                int disciplineIndex = rvtSheetData.SelectedDisciplineIndex;
                if (disciplineIndex == -1) return false;

                Discipline selectedDiscipline = rvtSheetData.Disciplines[disciplineIndex];
                var sheetFound = from sheet in rvtSheetData.Sheets where sheet.DisciplineObj.Id == selectedDiscipline.Id && sheet.LinkStatus.IsSelected select sheet;
                if (sheetFound.Count() > 0)
                {
                    if (config.TitleblockId != ElementId.InvalidElementId)
                    {
                        ProgressManager.InitializeProgress("Creating or Updating Sheets...", sheetFound.Count());

                        List<MessageInfo> messages = new List<MessageInfo>();
                        using (TransactionGroup tg = new TransactionGroup(doc))
                        {
                            tg.Start("Update Sheets");
                            try
                            {
                                foreach (RevitSheet rvtSheet in sheetFound)
                                {
                                    ProgressManager.StepForward();
                                    using (Transaction trans = new Transaction(doc))
                                    {
                                        trans.Start("Update Sheet");
                                        try
                                        {
                                            bool updatedSheet = UpdateSheet(doc, rvtSheet, ref messages);
                                            if (!updatedSheet) { updated = updatedSheet; }
                                            trans.Commit();
                                        }
                                        catch (Exception ex)
                                        {
                                            trans.RollBack();
                                            MessageInfo msgInfo = new MessageInfo(rvtSheet, ex.Message);
                                            messages.Add(msgInfo);
                                        }
                                    }
                                }
                                tg.Assimilate();
                            }
                            catch (Exception ex)
                            {
                                tg.RollBack();
                                MessageBox.Show("Failed to update sheets.\n" + ex.Message, "Update Sheets", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }

                        if (messages.Count > 0)
                        {
                            MessageWindow mWindow = new MessageWindow();
                            mWindow.Messages = messages;
                            mWindow.ShowDialog();
                        }
                        ProgressManager.FinalizeProgress();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update sheets.\n"+ex.Message, "Update Sheets", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return updated;
        }

        private bool UpdateSheet(Document doc, RevitSheet rvtSheet, ref List<MessageInfo> messages)
        {
            bool sheetUpdated = false;
            try
            {
                ViewSheet viewSheet = null;
                bool sheetCreated = false;
                if (rvtSheet.LinkStatus.IsLinked)
                {
                    if (rvtSheet.LinkStatus.Modified)
                    {
                        viewSheet = doc.GetElement(rvtSheet.LinkStatus.CurrentLinkedId) as ViewSheet;
                    }
                }
                else
                {
                    if (config.IsPlaceholder)
                    {
                        viewSheet = ViewSheet.CreatePlaceholder(doc);
                    }
                    else
                    {
                        viewSheet = ViewSheet.Create(doc, config.TitleblockId);
                    }
                    sheetCreated = true;
                }

                if (null != viewSheet)
                {
                    bool updatedParam = UpdateSheetParameters(ref viewSheet, rvtSheet, ref messages);

                    if (updatedParam)
                    {
                        int sheetIndex = rvtSheetData.Sheets.IndexOf(rvtSheet);
                        this.RvtSheetData.Sheets[sheetIndex].LinkStatus.IsSelected = false;
                        this.RvtSheetData.Sheets[sheetIndex].LinkStatus.IsLinked = true;
                        this.RvtSheetData.Sheets[sheetIndex].LinkStatus.Modified = false;
                        this.RvtSheetData.Sheets[sheetIndex].LinkStatus.CurrentLinkedId = viewSheet.UniqueId;
                        this.RvtSheetData.Sheets[sheetIndex].LinkStatus.LinkedElementId = viewSheet.Id.IntegerValue;

                        this.RvtSheetData.Sheets[sheetIndex].LinkStatus.ToolTip = "Linked Sheet ElementId: " + viewSheet.Id.IntegerValue;

                        if (sheetCreated)
                        {
                            LinkedSheet linkedSheet = new LinkedSheet(Guid.NewGuid(), rvtSheet.Id, new LinkedProject(config.ModelId), viewSheet.UniqueId, false);
                            this.RvtSheetData.Sheets[sheetIndex].LinkedSheets.Add(linkedSheet);
                            bool dbUpdated = SheetDataWriter.ChangeLinkedSheet(linkedSheet, CommandType.INSERT);
                        }
                    }
                }

                sheetUpdated = true;
            }
            catch (Exception ex)
            {
                MessageInfo mInfo = new MessageInfo(rvtSheet, ex.Message);
                messages.Add(mInfo);
            }

            return sheetUpdated;
        }

        private bool UpdateSheetParameters(ref ViewSheet viewSheet, RevitSheet rvtSheet, ref List<MessageInfo> messages)
        {
            bool updated = false;
            try
            {
                viewSheet.SheetNumber = rvtSheet.Number;
                viewSheet.ViewName = rvtSheet.Name;

                foreach (SheetParameterValue paramValue in rvtSheet.SheetParameters.Values)
                {
                    string paramName = paramValue.Parameter.ParameterName;
                    Parameter param = viewSheet.LookupParameter(paramName);
                    if (null != param)
                    {
                        switch (param.StorageType)
                        {
                            case StorageType.Double:
                                double paramValueDbl;
                                if (double.TryParse(paramValue.ParameterValue, out paramValueDbl))
                                {
                                    param.Set(paramValueDbl);
                                }
                                break;
                            case StorageType.ElementId:
                                int paramValueElementId;
                                if (int.TryParse(paramValue.ParameterValue, out paramValueElementId))
                                {
                                    param.Set(new ElementId(paramValueElementId));
                                }
                                break;
                            case StorageType.Integer:
                                int paramvalueInt;
                                if (int.TryParse(paramValue.ParameterValue, out paramvalueInt))
                                {
                                    param.Set(paramvalueInt);
                                }

                                break;
                            case StorageType.String:
                                param.Set(paramValue.ParameterValue);
                                break;
                        }
                    }
                }
                updated = true;
            }
            catch (Exception ex)
            {
                MessageInfo mInfo = new MessageInfo(rvtSheet, ex.Message);
                messages.Add(mInfo);
                string message = ex.Message;
            }
            return updated;
        }

        public void UpdateRevisionExecuted(object param)
        {
            try
            {
                if (SheetDataWriter.dbFile != dbFile) { SheetDataWriter.OpenDatabase(dbFile); }

                m_handler.ViewModel = this;

                RevisionWindow revisionWindow = new RevisionWindow(config.ModelId, m_handler, m_event);
                revisionWindow.Owner = AppCommand.thisApp.mainWindow;
                revisionWindow.DataContext = rvtSheetData;
                revisionWindow.Show();

                /*
                if ((bool)revisionWindow.ShowDialog())
                {
                    m_handler.Request.Make(RequestId.UpdateRevision);
                    m_event.Raise();
                }*/
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update revisions.\n" + ex.Message, "Update Revisions", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public bool UpdateRevisions(Document doc)
        {
            bool updated = false;
            using (TransactionGroup tg = new TransactionGroup(doc))
            {
                tg.Start("Update Revisions");
                try
                {
                    List<MessageInfo> messages = new List<MessageInfo>();
                    for (int i = 0; i < rvtSheetData.Revisions.Count; i++)
                    {
                        if (rvtSheetData.Revisions[i].LinkStatus.IsSelected)
                        {
                            bool revisionCreated = false;
                            RevitRevision rvtRevision = rvtSheetData.Revisions[i];

                            using (Transaction trans = new Transaction(doc))
                            {
                                trans.Start("Update Revision");
                                try
                                {
                                    Revision revision = null;
                                    if (rvtRevision.LinkStatus.IsLinked)
                                    {
                                        if (rvtRevision.LinkStatus.Modified)
                                        {
                                            revision = doc.GetElement(rvtRevision.LinkStatus.CurrentLinkedId) as Revision;
                                        }
                                    }
                                    else
                                    {
                                        revision = Revision.Create(doc);
                                        revisionCreated = true;
                                    }

                                    if (null != revision)
                                    {
                                        revision.Description = rvtRevision.Description;
                                        revision.IssuedTo = rvtRevision.IssuedTo;
                                        revision.IssuedBy = rvtRevision.IssuedBy;
                                        revision.RevisionDate = rvtRevision.Date;

                                        if (revisionCreated)
                                        {
                                            string revisionNumber = "";
                                            try { revisionNumber = revision.RevisionNumber; }
                                            catch { revisionNumber = ""; }

                                            NumberType numType = (NumberType)Enum.Parse(typeof(NumberType), revision.NumberType.ToString());

                                            LinkedRevision linkedRevision = new LinkedRevision(Guid.NewGuid(), rvtRevision.Id, revision.SequenceNumber, revisionNumber, numType, linkedProjectId, revision.UniqueId, false);
                                            this.RvtSheetData.Revisions[i].LinkedRevisions.Add(linkedRevision);
                                            bool updatedDB = SheetDataWriter.ChangeLinkedRevision(linkedRevision, CommandType.INSERT);
                                        }

                                        this.RvtSheetData.Revisions[i].LinkStatus.IsSelected = false;
                                        this.RvtSheetData.Revisions[i].LinkStatus.IsLinked = true;
                                        this.RvtSheetData.Revisions[i].LinkStatus.Modified = false;
                                        this.RvtSheetData.Revisions[i].LinkStatus.CurrentLinkedId = revision.UniqueId;
                                        this.RvtSheetData.Revisions[i].LinkStatus.LinkedElementId = revision.Id.IntegerValue;
                                        this.RvtSheetData.Revisions[i].LinkStatus.ToolTip = "Linked Revision ElementId: " + revision.Id.IntegerValue;
                                    }
                                    trans.Commit();
                                }
                                catch (Exception ex)
                                {
                                    trans.RollBack();
                                    MessageInfo msgInfo = new MessageInfo(rvtRevision.Id, rvtRevision.Description, ex.Message);
                                    messages.Add(msgInfo);
                                }
                            }
                        }
                    }

                    if (messages.Count > 0)
                    {
                        MessageWindow mWindow = new MessageWindow();
                        mWindow.Messages = messages;
                        mWindow.ShowDialog();
                    }
                    tg.Assimilate();
                }
                catch (Exception ex)
                {
                    tg.RollBack();
                    MessageBox.Show("Failed to update revisions.\n" + ex.Message, "Update Revisions", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            return updated;
        }

        public void UpdateRevisionOnSheetExecuted(object param)
        {
            try
            {
                m_handler.ViewModel = this;
                m_handler.Request.Make(RequestId.UpdateRevisionOnSheet);
                m_event.Raise();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public bool UpdateRevisionOnSheets(Document doc)
        {
            bool updated = false;
            try
            {
                int disciplineIndex = rvtSheetData.SelectedDisciplineIndex;
                if (disciplineIndex == -1) return false;

                Discipline selectedDiscipline = rvtSheetData.Disciplines[disciplineIndex];
                var sheetFound = from sheet in rvtSheetData.Sheets
                                 where sheet.DisciplineObj.Id == selectedDiscipline.Id && sheet.LinkStatus.IsSelected && sheet.LinkStatus.IsLinked
                                 select sheet;
                if (sheetFound.Count() > 0)
                {
                    ProgressManager.InitializeProgress("Updating Revisions on Sheet...", sheetFound.Count());

                    using (TransactionGroup tg = new TransactionGroup(doc))
                    {
                        tg.Start("Update Revision On Sheets");
                        try
                        {
                            List<MessageInfo> messages = new List<MessageInfo>();
                            foreach (RevitSheet rvtSheet in sheetFound)
                            {
                                using (Transaction trans = new Transaction(doc))
                                {
                                    trans.Start("Update ROS");
                                    try
                                    {
                                        ProgressManager.StepForward();
                                        bool updatedRevisionOnSheet = UpdateRevisionOnSheet(doc, rvtSheet, ref messages);
                                        if (!updatedRevisionOnSheet) { updated = updatedRevisionOnSheet; }
                                        trans.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        trans.RollBack();
                                        MessageInfo msgInfo = new MessageInfo(rvtSheet, ex.Message);
                                        messages.Add(msgInfo);
                                    }
                                }
                            }

                            if (messages.Count > 0)
                            {
                                MessageWindow mWindow = new MessageWindow();
                                mWindow.Messages = messages;
                                mWindow.ShowDialog();
                            }
                            tg.Assimilate();
                        }
                        catch (Exception ex)
                        {
                            tg.RollBack();
                            MessageBox.Show("Failed to update Revision On Sheets.\n" + ex.Message, "Update Revision On Sheets", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    ProgressManager.FinalizeProgress();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update revisions on sheets.\n" + ex.Message, "Update Revision On Sheets", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return updated;
        }

        private bool UpdateRevisionOnSheet(Document doc, RevitSheet rvtSheet, ref List<MessageInfo> messages)
        {
            bool rosUpdated = false;
            try
            {
                int sheetIndex = rvtSheetData.Sheets.IndexOf(rvtSheet);

                ViewSheet viewSheet = doc.GetElement(rvtSheet.LinkStatus.CurrentLinkedId) as ViewSheet;
                if (null != viewSheet)
                {
                    List<ElementId> revisionIds = new List<ElementId>();
                    var revisionToInclude = from ros in rvtSheet.SheetRevisions.Values where ros.Include && ros.RvtRevision.LinkStatus.IsLinked select ros.RvtRevision;
                    if (revisionToInclude.Count() > 0)
                    {
                        foreach (RevitRevision rvtRevision in revisionToInclude)
                        {
                            int revisionId = rvtRevision.LinkStatus.LinkedElementId;
                            ElementId elementId = new ElementId(revisionId);
                            if (elementId != ElementId.InvalidElementId && !revisionIds.Contains(elementId))
                            {
                                revisionIds.Add(elementId);

                                this.RvtSheetData.Sheets[sheetIndex].SheetRevisions[rvtRevision.Id].LinkStatus.IsLinked = true;
                            }
                        }
                    }

                    if (revisionIds.Count > 0)
                    {
                        viewSheet.SetAdditionalRevisionIds(revisionIds);
                        rosUpdated = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageInfo mInfo = new MessageInfo(rvtSheet, ex.Message);
                messages.Add(mInfo);
            }
            
            return rosUpdated;
        }

        public void PlaceViewExecuted(object param)
        {
            try
            {
                m_handler.ViewModel = this;
                m_handler.Request.Make(RequestId.PlaceView);
                m_event.Raise();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public bool PlaceViews(Document doc)
        {
            bool placed = false;
            try
            {
                int disciplineIndex = rvtSheetData.SelectedDisciplineIndex;
                if (disciplineIndex == -1) return false;

                Discipline selectedDiscipline = rvtSheetData.Disciplines[disciplineIndex];
                var sheetFound = from sheet in rvtSheetData.Sheets where sheet.DisciplineObj.Id == selectedDiscipline.Id && sheet.LinkStatus.IsSelected select sheet;
                if (sheetFound.Count() > 0)
                {
                    ProgressManager.InitializeProgress("Placing views on sheets...", sheetFound.Count());

                    using (TransactionGroup tg = new TransactionGroup(doc))
                    {
                        tg.Start("Place Views");
                        try
                        {
                            List<MessageInfo> messages = new List<MessageInfo>();
                            foreach (RevitSheet rvtSheet in sheetFound)
                            {
                                using (Transaction trans = new Transaction(doc))
                                {
                                    trans.Start("Place View");
                                    try
                                    {
                                        ProgressManager.StepForward();
                                        bool placedView = PlaceView(doc, rvtSheet, ref messages);
                                        if (!placedView) { placed = placedView; }
                                        trans.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        trans.RollBack();
                                        MessageInfo msgInfo = new MessageInfo(rvtSheet, ex.Message);
                                        messages.Add(msgInfo);
                                    }
                                }
                            }

                            if (messages.Count > 0)
                            {
                                MessageWindow mWindow = new MessageWindow();
                                mWindow.Messages = messages;
                                mWindow.ShowDialog();
                            }
                            tg.Commit();
                        }
                        catch (Exception ex)
                        {
                            tg.RollBack();
                            MessageBox.Show("Failed to place views.\n" + ex.Message, "Place Views", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    
                    ProgressManager.FinalizeProgress();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to place views on the selcted sheets.\n" + ex.Message, "Place Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return placed;
        }

        private bool PlaceView(Document doc, RevitSheet rvtSheet, ref List<MessageInfo> messages)
        {
            bool placed = false;
            try
            {
                if (!rvtSheet.LinkStatus.IsLinked) { return placed; }

                ViewSheet viewSheet = doc.GetElement(rvtSheet.LinkStatus.CurrentLinkedId) as ViewSheet;
                if (null != viewSheet)
                {
                    var viewFound = from view in rvtSheetData.Views 
                                    where view.Sheet.Id == rvtSheet.Id && view.LinkStatus.LinkedElementId != -1 
                                    select view;
                    if (viewFound.Count() > 0)
                    {
                        List<ElementId> placedViewIds = viewSheet.GetAllPlacedViews().ToList();

                        List<ElementId> placedViewPortIds = viewSheet.GetAllViewports().ToList();
                        List<Viewport> viewports = new List<Viewport>();
                        foreach (ElementId eId in placedViewPortIds)
                        {
                            Viewport vp = doc.GetElement(eId) as Viewport;
                            viewports.Add(vp);
                        }

                        foreach (RevitView rvtView in viewFound)
                        {
                            int viewIndex = rvtSheetData.Views.IndexOf(rvtView);

                            View view = doc.GetElement(rvtView.LinkStatus.CurrentLinkedId) as View;
                            if (null != view)
                            {
                                try
                                {
                                    if (placedViewIds.Contains(view.Id))
                                    {
                                        //delete existing viewport
                                        var viewportFound = from vp in viewports where vp.ViewId == view.Id select vp;
                                        if (viewportFound.Count() > 0)
                                        {
                                            viewSheet.DeleteViewport(viewportFound.First());
                                        }
                                    }

                                    //create Viewport
                                    if (Viewport.CanAddViewToSheet(doc, viewSheet.Id, view.Id))
                                    {
                                        Viewport createdVP = Viewport.Create(doc, viewSheet.Id, view.Id, XYZ.Zero);
                                        if (null != createdVP)
                                        {
                                            XYZ centerPoint = createdVP.GetBoxCenter();
                                            Outline outline = createdVP.GetBoxOutline();
                                            XYZ diffToMove = new XYZ(rvtView.LocationU + outline.MaximumPoint.X, rvtView.LocationV + outline.MaximumPoint.Y, 0);

                                            ElementTransformUtils.MoveElement(doc, createdVP.Id, diffToMove);
                                            this.RvtSheetData.Views[viewIndex].LinkStatus.IsLinked = true;
                                        }
                                    }
                                    else if (view.ViewType == ViewType.Schedule)
                                    {

                                        ScheduleSheetInstance createdInstance = ScheduleSheetInstance.Create(doc, viewSheet.Id, view.Id, XYZ.Zero);
                                        if (null != createdInstance)
                                        {
                                            XYZ centerPoint = createdInstance.Point;
                                            BoundingBoxXYZ bbBox = createdInstance.get_BoundingBox(viewSheet);
                                            XYZ diffToMove = new XYZ(rvtView.LocationU - bbBox.Min.X, rvtView.LocationV - bbBox.Min.Y, 0);

                                            ElementTransformUtils.MoveElement(doc, createdInstance.Id, diffToMove);
                                            this.RvtSheetData.Views[viewIndex].LinkStatus.IsLinked = true;
                                        }
                                    }
                                    else
                                    {
                                        MessageInfo mInfo = new MessageInfo(rvtView.Id, rvtView.Name, "View cannot be added in the sheet");
                                        messages.Add(mInfo);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageInfo mInfo = new MessageInfo(rvtView.Id, rvtView.Name, ex.Message);
                                    messages.Add(mInfo);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return placed;
        }

        public void ImportViewExecuted(object param)
        {
            try
            {
                m_handler.ViewModel = this;
                m_handler.Request.Make(RequestId.ImportView);
                m_event.Raise();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public bool ImportViews(Document doc)
        {
            bool imported = false;
            try
            {
                //collect viewports
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<Viewport> viewports = collector.OfCategory(BuiltInCategory.OST_Viewports).ToElements().Cast<Viewport>().ToList();

                foreach (Viewport vp in viewports)
                {
                    RevitView rvtView = null;
                    RevitSheet rvtSheet = new RevitSheet();

                    View view = doc.GetElement(vp.ViewId) as View;
                    if (null != view)
                    {
                        ViewSheet viewSheet = doc.GetElement(vp.SheetId) as ViewSheet;
                        if (null != viewSheet)
                        {
                            var sheetFound = from sheet in rvtSheetData.Sheets where sheet.LinkStatus.CurrentLinkedId == viewSheet.UniqueId select sheet;
                            if (sheetFound.Count() > 0)
                            {
                                rvtSheet = sheetFound.First();
                            }
                        }

                        XYZ centerPoint = vp.GetBoxCenter();
                        Outline outline = vp.GetBoxOutline();
                        XYZ minPoint = outline.MinimumPoint;
                        var viewFound = from rv in rvtSheetData.Views where rv.Name == view.ViewName select rv;
                        if (viewFound.Count() > 0)
                        {
                            rvtView = viewFound.First();
                            if (centerPoint.X != rvtView.LocationU || centerPoint.Y != rvtView.LocationV)
                            {
                                int viewIndex = rvtSheetData.Views.IndexOf(rvtView);
                                this.RvtSheetData.Views[viewIndex].LocationU = Math.Round(minPoint.X, 2);
                                this.RvtSheetData.Views[viewIndex].LocationV = Math.Round(minPoint.Y, 2);
                                this.rvtSheetData.Views[viewIndex].LinkStatus.Modified = true;

                                bool updated = SheetDataWriter.ChangeViewItem(rvtSheetData.Views[viewIndex], CommandType.UPDATE);
                            }
                        }
                        else
                        {
                            RevitViewType viewType = new RevitViewType();
                            var viewTypeFound = from vType in rvtSheetData.ViewTypes where vType.ViewType.ToString() == view.ViewType.ToString() select vType;
                            if (viewTypeFound.Count() > 0)
                            {
                                viewType = viewTypeFound.First();
                            }
                            rvtView = new RevitView(Guid.NewGuid(), view.ViewName, rvtSheet, viewType, Math.Round(minPoint.X, 2), Math.Round(minPoint.Y, 2));
                            this.RvtSheetData.Views.Add(rvtView);

                            bool inserted = SheetDataWriter.ChangeViewItem(rvtView, CommandType.INSERT);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to import views.\n" + ex.Message, "Import Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return imported;
        }

        public void RenumberSheetExecuted(object param)
        {
            try
            {
                selectedItemMappers = new List<RevitItemMapper>();

                ReplaceWindow replaceWindow = new ReplaceWindow(MappingType.Sheet, "Sheet Number");
                replaceWindow.DataContext = rvtSheetData;
                if ((bool)replaceWindow.ShowDialog())
                {
                    selectedItemMappers = replaceWindow.SelectedItems;

                    if (SheetDataWriter.dbFile != dbFile) { SheetDataWriter.OpenDatabase(dbFile); }

                    m_handler.ViewModel = this;
                    m_handler.Request.Make(RequestId.RenumberSheet);
                    m_event.Raise();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public bool RenumberSheets(Document doc)
        {
            bool replaced = false;
            ProgressManager.InitializeProgress("Renumber Sheets...", selectedItemMappers.Count);
            List<MessageInfo> messages = new List<MessageInfo>();

            using (TransactionGroup tg = new TransactionGroup(doc))
            {
                tg.Start("Renumber Sheets");
                try
                {
                    foreach (RevitItemMapper itemMap in selectedItemMappers)
                    {
                        ProgressManager.StepForward();

                        using (Transaction trans = new Transaction(doc))
                        {
                            trans.Start("Renumber Sheet");
                            try
                            {
                                var sheetNumberFound = from sheet in rvtSheetData.Sheets
                                                       where sheet.Number == itemMap.SourceValue && sheet.LinkStatus.IsLinked
                                                       select sheet;
                                if (sheetNumberFound.Count() > 0)
                                {
                                    RevitSheet rvtSheet = sheetNumberFound.First();
                                    int sheetIndex = rvtSheetData.Sheets.IndexOf(rvtSheet);

                                    this.RvtSheetData.Sheets[sheetIndex].Number = itemMap.TargetValue;
                                    bool dbUpdated = SheetDataWriter.ChangeSheetItem(rvtSheet.Id.ToString(), "Sheet_Number", itemMap.TargetValue);

                                    ViewSheet viewSheet = doc.GetElement(rvtSheet.LinkStatus.CurrentLinkedId) as ViewSheet;
                                    if (null != viewSheet)
                                    {
                                        try
                                        {
                                            viewSheet.SheetNumber = itemMap.TargetValue;
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageInfo mInfo = new MessageInfo(rvtSheet, ex.Message);
                                            messages.Add(mInfo);
                                        }
                                    }
                                }
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                MessageInfo mInfo = new MessageInfo(itemMap.ItemId, itemMap.SourceValue, ex.Message);
                                messages.Add(mInfo);
                            }
                        }
                       
                    }
                    tg.Assimilate();
                }
                catch (Exception ex)
                {
                    tg.RollBack();
                    MessageBox.Show("Failed to renumber sheets.\n" + ex.Message, "Renumber Sheets", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            if (messages.Count > 0)
            {
                MessageWindow mWindow = new MessageWindow();
                mWindow.Messages = messages;
                mWindow.ShowDialog();
            }
            else
            {
                replaced = true;
            }
            ProgressManager.FinalizeProgress();
            return replaced;
        }

        public void RenameViewExecuted(object param)
        {
            try
            {
                selectedItemMappers = new List<RevitItemMapper>();

                ReplaceWindow replaceWindow = new ReplaceWindow(MappingType.View, "View Name");
                replaceWindow.DataContext = rvtSheetData;
                if ((bool)replaceWindow.ShowDialog())
                {
                    selectedItemMappers = replaceWindow.SelectedItems;

                    m_handler.ViewModel = this;
                    m_handler.Request.Make(RequestId.RenameView);
                    m_event.Raise();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public bool RenameViews(Document doc)
        {
            bool renamed = false;
            try
            {
                ProgressManager.InitializeProgress("Rename Views...", selectedItemMappers.Count);
                List<MessageInfo> messages = new List<MessageInfo>();

                using (TransactionGroup tg = new TransactionGroup(doc))
                {
                    tg.Start("Rename Views");
                    try
                    {
                        foreach (RevitItemMapper itemMap in selectedItemMappers)
                        {
                            ProgressManager.StepForward();
                            var viewNameFound = from view in rvtSheetData.Views
                                                where view.Name == itemMap.SourceValue && view.LinkStatus.IsLinked
                                                select view;
                            if (viewNameFound.Count() > 0)
                            {
                                RevitView rvtView = viewNameFound.First();
                                using (Transaction trans = new Transaction(doc))
                                {
                                    trans.Start("Rename View");
                                    try
                                    {
                                        int viewIndex = rvtSheetData.Views.IndexOf(rvtView);
                                        this.RvtSheetData.Views[viewIndex].Name = itemMap.TargetValue;

                                        View view = doc.GetElement(rvtView.LinkStatus.CurrentLinkedId) as View;
                                        if (null != view)
                                        {
                                            view.Name = itemMap.TargetValue;
                                        }
                                        trans.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageInfo mInfo = new MessageInfo(rvtView.Id, rvtView.Name, ex.Message);
                                        messages.Add(mInfo);
                                        trans.RollBack();
                                    }
                                }
                            }
                        }
                        tg.Assimilate();
                    }
                    catch (Exception ex)
                    {
                        tg.RollBack();
                        MessageBox.Show("Failed to rename views.\n" + ex.Message, "Rename Views", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                
                if (messages.Count > 0)
                {
                    MessageWindow mWindow = new MessageWindow();
                    mWindow.Messages = messages;
                    mWindow.ShowDialog();
                }
                else
                {
                    renamed = true;
                }
                ProgressManager.FinalizeProgress();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to rename views.\n" + ex.Message, "Rename Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return renamed;
        }

        public void SettingExecuted(object param)
        {
            try
            {
                SettingWindow settingWindow = new SettingWindow(m_handler.CurrentDocument, config);
                if ((bool)settingWindow.ShowDialog())
                {
                    config = settingWindow.Configuration;
                    m_handler.ViewModel = this;
                    m_handler.Request.Make(RequestId.StoreConfiguration);
                    m_event.Raise();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void StoreConfiguration(Document doc)
        {
            try
            {
                string centralPath = RevitUtil.GetCentralFilePath(doc);
                if (AppCommand.thisApp.configDictionary.ContainsKey(centralPath))
                {
                    AppCommand.thisApp.configDictionary.Remove(centralPath);
                }

                AppCommand.thisApp.configDictionary.Add(centralPath, config);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to store tool's configuration.\n" + ex.Message, "Store Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void HelpExecuted(object param)
        {
            try
            {
                string helpFile = @"V:\RVT-Data\HOK Program\Documentation\SheetManagerTools_Instruction.pdf";
                if (File.Exists(helpFile))
                {
                    Process.Start(helpFile);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void CheckAllExectued(object param)
        {
            try
            {
                int disciplineIndex = rvtSheetData.SelectedDisciplineIndex;
                if (disciplineIndex == -1) return;

                Discipline selectedDiscipline = rvtSheetData.Disciplines[disciplineIndex];
                var sheetFound = from sheet in rvtSheetData.Sheets where sheet.DisciplineObj.Id == selectedDiscipline.Id select sheet;
                if (sheetFound.Count() > 0)
                {
                    foreach (RevitSheet rvtSheet in sheetFound)
                    {
                        int index = rvtSheetData.Sheets.IndexOf(rvtSheet);
                        this.RvtSheetData.Sheets[index].LinkStatus.IsSelected = true;
                    }
                }

                //invoke itemssource converter to invoke datagrid sheet
                //this.RvtSheetData.SelectedDisciplineIndex = -1;
                //this.RvtSheetData.SelectedDisciplineIndex = disciplineIndex;

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void UncheckAllExecuted(object param)
        {
            try
            {
                int disciplineIndex = rvtSheetData.SelectedDisciplineIndex;
                if (disciplineIndex == -1) return;

                Discipline selectedDiscipline = rvtSheetData.Disciplines[disciplineIndex];
                var sheetFound = from sheet in rvtSheetData.Sheets where sheet.DisciplineObj.Id == selectedDiscipline.Id select sheet;
                if (sheetFound.Count() > 0)
                {
                    foreach (RevitSheet rvtSheet in sheetFound)
                    {
                        int index = rvtSheetData.Sheets.IndexOf(rvtSheet);
                        this.RvtSheetData.Sheets[index].LinkStatus.IsSelected = false;
                    }
                }

                //this.RvtSheetData.SelectedDisciplineIndex = -1;
                //this.RvtSheetData.SelectedDisciplineIndex = disciplineIndex;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
