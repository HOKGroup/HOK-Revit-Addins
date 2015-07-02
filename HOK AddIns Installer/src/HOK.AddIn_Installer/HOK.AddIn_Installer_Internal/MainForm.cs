using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Security.Permissions;
using System.Security;
using System.Security.AccessControl;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace HOK.AddIn_Installer_Internal
{
    public partial class MainForm : Form
    {
        private string masterDirectory;
        private string betaDirectory;
        private string installDirectory;
        private string programDirectory = "";
        private List<ColumnHeader> revitColumnHeaders = new List<ColumnHeader>();
        private List<ColumnHeader> dynamoColumnHeaders = new List<ColumnHeader>();
        private Dictionary<Tool2013, RevitToolProperties> tool2013Dictionary = new Dictionary<Tool2013, RevitToolProperties>();
        private Dictionary<Tool2014, RevitToolProperties> tool2014Dictionary = new Dictionary<Tool2014, RevitToolProperties>();
        private Dictionary<Tool2015, RevitToolProperties> tool2015Dictionary = new Dictionary<Tool2015, RevitToolProperties>();
        private Dictionary<Tool2016, RevitToolProperties> tool2016Dictionary = new Dictionary<Tool2016, RevitToolProperties>();
        private Dictionary<string, DynamoToolProperties> dynamoDictionary = new Dictionary<string, DynamoToolProperties>();
        private Dictionary<string/*toolName*/, string/*version*/> installedVersions2013 = new Dictionary<string, string>();
        private Dictionary<string/*toolName*/, string/*version*/> installedVersions2014 = new Dictionary<string, string>();
        private Dictionary<string/*toolName*/, string/*version*/> installedVersions2015 = new Dictionary<string, string>();
        private Dictionary<string/*toolName*/, string/*version*/> installedVersions2016 = new Dictionary<string, string>();
        private Dictionary<string/*toolName*/, DateTime> installedDateDynamo = new Dictionary<string, DateTime>();
        private ToolManager toolManager;
        private DeprecatedTools deprecatedTools;
        private string[] splitter = new string[] { "##" };

        public MainForm()
        {
            CreateDirectories(TargetSoftware.Revit_2013);
            CreateDirectories(TargetSoftware.Revit_2014);
            CreateDirectories(TargetSoftware.Revit_2015);
            CreateDirectories(TargetSoftware.Revit_2016);
            CreateDirectories(TargetSoftware.Dynamo);

            deprecatedTools = new DeprecatedTools();

            toolManager = new ToolManager();
            if (tool2013Dictionary.Count == 0) { tool2013Dictionary = toolManager.Get2013Dictionary(deprecatedTools.Deprecated2013); }
            if (tool2014Dictionary.Count == 0) { tool2014Dictionary = toolManager.Get2014Dictionary(deprecatedTools.Deprecated2014); }
            if (tool2015Dictionary.Count == 0) { tool2015Dictionary = toolManager.Get2015Dictionary(deprecatedTools.Deprecated2015); }
            if (tool2016Dictionary.Count == 0) { tool2016Dictionary = toolManager.Get2016Dictionary(deprecatedTools.Deprecated2016); }
            if (dynamoDictionary.Count == 0) { dynamoDictionary = toolManager.GetDynamoDictionary(deprecatedTools.DeprecatedDynamo); }
            
            InitializeComponent();
            this.Text = "HOK AddIns Installer v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            comboBoxTarget.DataSource = Enum.GetValues(typeof(TargetSoftware));
            comboBoxTarget.SelectedItem = TargetSoftware.Revit_2013;

        }

        private void Display2013Tools()
        {
            try
            {
                listViewTools.Items.Clear();
                listViewBeta.Items.Clear();

                CreateColumnHeaders(listViewTools, TargetSoftware.Revit_2013,false);
                CreateColumnHeaders(listViewBeta, TargetSoftware.Revit_2013,true);

                listViewTools.LargeImageList = imageListIcons;
                listViewTools.SmallImageList = imageListIcons;
                listViewBeta.LargeImageList = imageListIcons;
                listViewBeta.SmallImageList = imageListIcons;

                int needInstall = 0;
                foreach (Tool2013 tool in tool2013Dictionary.Keys)
                {
                    RevitToolProperties tp = tool2013Dictionary[tool];
                    if (tool == Tool2013.Default) { continue; }

                    if (!tp.BetaOnly)
                    {
                        ListViewItem item = new ListViewItem(tp.ToolName);
                        item.Name = tp.ToolName;
                        item.ImageIndex = tp.ImageIndex;
                        item.Tag = tool;
                        item.SubItems.Add("v." + tp.ReleaseVersionInfo.FileVersion);
                        item.SubItems.Add(tp.ReleaseDate);

                        if (null != tp.InstallVersionInfo)
                        {
                            item.SubItems.Add("v." + tp.InstallVersionInfo.FileVersion);
                            item.Checked = CompareVersions(tp.InstallVersionInfo, tp.ReleaseVersionInfo);
                        }
                        else
                        {
                            item.SubItems.Add("Not Installed");
                            item.Checked = true;
                        }
                        if (item.Checked) { needInstall++; }
                        listViewTools.Items.Add(item);
                    }

                    if (null!=tp.BetaVersionInfo)
                    {
                        AddBetaToolItem(tp);
                    }
                }

                if (needInstall > 0)
                {
                    labelStatus.Text = needInstall + " of 8 out of date";
                }
                else
                {
                    labelStatus.Text = "All files updated.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display tools for Revit 2013.\n" + ex.Message, "Display2013Tools", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        } 

        private void Display2014Tools()
        {
            try
            {
                listViewTools.Items.Clear();
                listViewBeta.Items.Clear();

                CreateColumnHeaders(listViewTools, TargetSoftware.Revit_2014, false);
                CreateColumnHeaders(listViewBeta, TargetSoftware.Revit_2014, true);
                
                listViewTools.LargeImageList = imageListIcons;
                listViewTools.SmallImageList = imageListIcons;
                listViewBeta.LargeImageList = imageListIcons;
                listViewBeta.SmallImageList = imageListIcons;

                int needInstall = 0;
                foreach (Tool2014 tool in tool2014Dictionary.Keys)
                {
                    RevitToolProperties tp = tool2014Dictionary[tool];
                    if (tool == Tool2014.Default) { continue; }

                    if (!tp.BetaOnly)
                    {
                        ListViewItem item = new ListViewItem(tp.ToolName);
                        item.Name = tp.ToolName;
                        item.ImageIndex = tp.ImageIndex;
                        item.Tag = tool;
                        item.SubItems.Add("v." + tp.ReleaseVersionInfo.FileVersion);
                        item.SubItems.Add(tp.ReleaseDate);

                        if (null != tp.InstallVersionInfo)
                        {
                            item.SubItems.Add("v." + tp.InstallVersionInfo.FileVersion);
                            item.Checked = CompareVersions(tp.InstallVersionInfo, tp.ReleaseVersionInfo);
                        }
                        else
                        {
                            item.SubItems.Add("Not Installed");
                            item.Checked = true;
                        }
                        if (item.Checked) { needInstall++; }
                        listViewTools.Items.Add(item);
                    }
                    
                    if (null!=tp.BetaVersionInfo)
                    {
                        AddBetaToolItem(tp);
                    }
                }

                if (needInstall > 0)
                {
                    labelStatus.Text = needInstall + " of 10 out of date";
                }
                else
                {
                    labelStatus.Text = "All files updated.";
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display tools for Revit 2014.\n" + ex.Message, "Display2014Tools", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Display2015Tools()
        {
            try
            {
                listViewTools.Items.Clear();
                listViewBeta.Items.Clear();

                CreateColumnHeaders(listViewTools, TargetSoftware.Revit_2015, false);
                CreateColumnHeaders(listViewBeta, TargetSoftware.Revit_2015, true);

                listViewTools.LargeImageList = imageListIcons;
                listViewTools.SmallImageList = imageListIcons;
                listViewBeta.LargeImageList = imageListIcons;
                listViewBeta.SmallImageList = imageListIcons;

                int needInstall = 0;
                foreach (Tool2015 tool in tool2015Dictionary.Keys)
                {
                    RevitToolProperties tp = tool2015Dictionary[tool];
                    if (tool == Tool2015.Default) { continue; }

                    if (!tp.BetaOnly)
                    {
                        ListViewItem item = new ListViewItem(tp.ToolName);
                        item.Name = tp.ToolName;
                        item.ImageIndex = tp.ImageIndex;
                        item.Tag = tool;
                        item.SubItems.Add("v." + tp.ReleaseVersionInfo.FileVersion);
                        item.SubItems.Add(tp.ReleaseDate);

                        if (null != tp.InstallVersionInfo)
                        {
                            item.SubItems.Add("v." + tp.InstallVersionInfo.FileVersion);
                            item.Checked = CompareVersions(tp.InstallVersionInfo, tp.ReleaseVersionInfo);
                        }
                        else
                        {
                            item.SubItems.Add("Not Installed");
                            item.Checked = true;
                        }
                        if (item.Checked) { needInstall++; }
                        listViewTools.Items.Add(item);
                    }

                    if (null != tp.BetaVersionInfo)
                    {
                        AddBetaToolItem(tp);
                    }
                }

                if (needInstall > 0)
                {
                    labelStatus.Text = needInstall + " of 10 out of date";
                }
                else
                {
                    labelStatus.Text = "All files updated.";
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display tools for Revit 2015.\n" + ex.Message, "Display2015Tools", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Display2016Tools()
        {
            try
            {
                listViewTools.Items.Clear();
                listViewBeta.Items.Clear();

                CreateColumnHeaders(listViewTools, TargetSoftware.Revit_2016, false);
                CreateColumnHeaders(listViewBeta, TargetSoftware.Revit_2016, true);

                listViewTools.LargeImageList = imageListIcons;
                listViewTools.SmallImageList = imageListIcons;
                listViewBeta.LargeImageList = imageListIcons;
                listViewBeta.SmallImageList = imageListIcons;

                int needInstall = 0;
                foreach (Tool2016 tool in tool2016Dictionary.Keys)
                {
                    RevitToolProperties tp = tool2016Dictionary[tool];

                    if (!tp.BetaOnly)
                    {
                        ListViewItem item = new ListViewItem(tp.ToolName);
                        item.Name = tp.ToolName;
                        item.ImageIndex = tp.ImageIndex;
                        item.Tag = tool;
                        item.SubItems.Add("v." + tp.ReleaseVersionInfo.FileVersion);
                        item.SubItems.Add(tp.ReleaseDate);

                        if (null != tp.InstallVersionInfo)
                        {
                            item.SubItems.Add("v." + tp.InstallVersionInfo.FileVersion);
                            item.Checked = CompareVersions(tp.InstallVersionInfo, tp.ReleaseVersionInfo);
                        }
                        else
                        {
                            item.SubItems.Add("Not Installed");
                            item.Checked = true;
                        }
                        if (item.Checked) { needInstall++; }
                        listViewTools.Items.Add(item);
                    }

                    if (null != tp.BetaVersionInfo)
                    {
                        AddBetaToolItem(tp);
                    }
                }

                if (needInstall > 0)
                {
                    labelStatus.Text = needInstall + " of 3 out of date";
                }
                else
                {
                    labelStatus.Text = "All files updated.";
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display tools for Revit 2015.\n" + ex.Message, "Display2015Tools", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DisplayDynamoTools()
        {
            try
            {
                listViewTools.Items.Clear();
                listViewBeta.Items.Clear();

                CreateColumnHeaders(listViewTools, TargetSoftware.Dynamo, false);
                CreateColumnHeaders(listViewBeta, TargetSoftware.Dynamo, true);

                listViewTools.LargeImageList = imageListDynamo;
                listViewTools.SmallImageList = imageListDynamo;
                listViewBeta.LargeImageList = imageListDynamo;
                listViewBeta.SmallImageList = imageListDynamo;

                int needInstall = 0;

                foreach (DynamoToolProperties dtp in dynamoDictionary.Values)
                {
                    if (null != dtp.ReleasedFileInfo)
                    {
                        ListViewItem item = new ListViewItem(dtp.ToolName);
                        item.Name = dtp.ToolName;
                        item.ImageIndex = 0;
                        item.Tag = dtp;
                        item.SubItems.Add(dtp.FileType);
                        item.SubItems.Add(dtp.ReleasedFileInfo.LastWriteTime.Date.ToString("d"));

                        if (null != dtp.InstalledFileInfo)
                        {
                            item.SubItems.Add(dtp.InstalledFileInfo.LastWriteTime.Date.ToString("d"));
                            item.Checked = CompareDate(dtp.InstalledFileInfo.LastWriteTime, dtp.ReleasedFileInfo.LastWriteTime);
                        }
                        else
                        {
                            item.SubItems.Add("Not Installed");
                            item.Checked = true;
                        }

                        if (item.Checked) { needInstall++; }
                        listViewTools.Items.Add(item);
                    }
                    if (null != dtp.BetaFileInfo)
                    {
                        ListViewItem item = new ListViewItem(dtp.ToolName);
                        item.Name = dtp.ToolName;
                        item.ImageIndex = 0;
                        item.Tag = dtp;
                        item.SubItems.Add(dtp.FileType);
                        item.SubItems.Add(dtp.BetaFileInfo.LastWriteTime.Date.ToString("d"));

                        if (null != dtp.InstalledFileInfo)
                        {
                            item.SubItems.Add(dtp.InstalledFileInfo.LastWriteTime.Date.ToString("d"));
                            item.Checked = CompareDate(dtp.InstalledFileInfo.LastWriteTime, dtp.BetaFileInfo.LastWriteTime);
                        }
                        else
                        {
                            item.SubItems.Add("Not Installed");
                            item.Checked = true;
                        }
                        listViewBeta.Items.Add(item);
                    }
                }

                if (needInstall > 0)
                {
                    labelStatus.Text = needInstall + " of " + dynamoDictionary.Count + " out of date";
                }
                else
                {
                    labelStatus.Text = "All files updated.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display dynamo definitions.\n"+ex.Message, "Display Dynamo Definitions", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CreateColumnHeaders(ListView listView, TargetSoftware target, bool beta)
        {
            try
            {
                List<ColumnHeader> headers = new List<ColumnHeader>();
                if (target == TargetSoftware.Revit_2013 || target == TargetSoftware.Revit_2014 || target==TargetSoftware.Revit_2015 || target == TargetSoftware.Revit_2016)
                {
                    if (revitColumnHeaders.Count == 0)
                    {
                        ColumnHeader columnheader = new ColumnHeader();
                        columnheader.TextAlign = HorizontalAlignment.Left;
                        columnheader.Name = "columnToolName";
                        columnheader.Text = "Tool Name";
                        columnheader.Width = 210;
                        revitColumnHeaders.Add(columnheader);

                        columnheader = new ColumnHeader();
                        columnheader.TextAlign = HorizontalAlignment.Left;
                        columnheader.Name = "columnReleasedVersion";
                        columnheader.Text = "Released Version";
                        columnheader.Width = 100;
                        revitColumnHeaders.Add(columnheader);

                        columnheader = new ColumnHeader();
                        columnheader.TextAlign = HorizontalAlignment.Left;
                        columnheader.Name = "columnReleasedDate";
                        columnheader.Text = "Released Date";
                        columnheader.Width = 100;
                        revitColumnHeaders.Add(columnheader);

                        columnheader = new ColumnHeader();
                        columnheader.TextAlign = HorizontalAlignment.Left;
                        columnheader.Name = "columnInstalledVersion";
                        columnheader.Text = "Installed Version";
                        columnheader.Width = 100;
                        revitColumnHeaders.Add(columnheader);
                    }
                    headers = revitColumnHeaders;
                }
                else if (target == TargetSoftware.Dynamo)
                {
                    if (dynamoColumnHeaders.Count == 0)
                    {
                        ColumnHeader columnheader = new ColumnHeader();
                        columnheader.TextAlign = HorizontalAlignment.Left;
                        columnheader.Name = "columnDefName";
                        columnheader.Text = "Definition Name";
                        columnheader.Width = 210;
                        dynamoColumnHeaders.Add(columnheader);

                        columnheader = new ColumnHeader();
                        columnheader.TextAlign = HorizontalAlignment.Left;
                        columnheader.Name = "columnFileType";
                        columnheader.Text = "File Type";
                        columnheader.Width = 100;
                        dynamoColumnHeaders.Add(columnheader);

                        columnheader = new ColumnHeader();
                        columnheader.TextAlign = HorizontalAlignment.Left;
                        columnheader.Name = "columnReleasedDate";
                        columnheader.Text = "Released Date";
                        columnheader.Width = 100;
                        dynamoColumnHeaders.Add(columnheader);
                       
                        columnheader = new ColumnHeader();
                        columnheader.TextAlign = HorizontalAlignment.Left;
                        columnheader.Name = "columnInstalledDate";
                        columnheader.Text = "Installed Date";
                        columnheader.Width = 100;
                        dynamoColumnHeaders.Add(columnheader);
                    }
                    headers = dynamoColumnHeaders;
                }

                listView.Columns.Clear();
                foreach (ColumnHeader header in headers)
                {
                    if (beta)
                    {
                        ColumnHeader colHeader = header.Clone() as ColumnHeader;
                        colHeader.Name = header.Name + "_beta";
                        listView.Columns.Add(colHeader);
                    }
                    else
                    {
                        listView.Columns.Add(header);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(listView.Name+": Failed to create column headers in the list view.\n"+ex.Message, "List View Column Headers", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool CompareVersions(FileVersionInfo installedVersionInfo, FileVersionInfo releasedVersionInfo)
        {
            bool outDated = false;
            try
            {
                Version installedVersion = new Version(installedVersionInfo.FileVersion);
                Version releasedVersion = new Version(releasedVersionInfo.FileVersion);
                if (installedVersion.CompareTo(releasedVersion) < 0)
                {
                    outDated = true;
                }
            
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to compare between the relesased and installed version info.\n"+ex.Message, "Compare Versions", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return outDated;
        }

        private bool CompareDate(DateTime installedDate, DateTime releasedDate)
        {
            bool outDated = false;
            try
            {
                
                if (DateTime.Compare(installedDate, releasedDate) < 0)
                {
                    outDated = true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to compare between the relesased and installed version info.\n" + ex.Message, "Compare Versions", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return outDated;
        }

        private void AddBetaToolItem(RevitToolProperties tp)
        {
            try
            {
                ListViewItem item = new ListViewItem(tp.ToolName);
                item.Name = tp.ToolName;
                item.ImageIndex = tp.ImageIndex;
                if (tp.TargetSoftWareEnum == TargetSoftware.Revit_2013) { item.Tag = tp.ToolEnum2013; }
                else if (tp.TargetSoftWareEnum == TargetSoftware.Revit_2014) { item.Tag = tp.ToolEnum2014; }
                else if (tp.TargetSoftWareEnum == TargetSoftware.Revit_2015) { item.Tag = tp.ToolEnum2015; }
                else if (tp.TargetSoftWareEnum == TargetSoftware.Revit_2016) { item.Tag = tp.ToolEnum2016; }
                item.SubItems.Add("v." + tp.BetaVersionInfo.FileVersion);
                item.SubItems.Add(tp.BetaDate);

                if (null==tp.InstallVersionInfo)
                {
                    item.SubItems.Add("Not Installed");
                }
                else
                {
                    item.SubItems.Add("v." + tp.InstallVersionInfo.FileVersion);
                    item.Checked = CompareVersions(tp.InstallVersionInfo, tp.BetaVersionInfo);
                }

                listViewBeta.Items.Add(item);
            }
            catch(Exception ex)
            {
                MessageBox.Show(tp.ToolName+": Failed to add an item for beta version.\n"+ex.Message, "MainForm:AddBetaToolItem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonInstall_Click(object sender, EventArgs e)
        {
            try
            {
                if (CountCheckedItems())
                {
                    TargetSoftware target = (TargetSoftware)comboBoxTarget.SelectedItem;
                    switch (target)
                    {
                        case TargetSoftware.Revit_2013:
                            Install2013Tool();
                            break;
                        case TargetSoftware.Revit_2014:
                            Install2014Tool();
                            break;
                        case TargetSoftware.Revit_2015:
                            Install2015Tool();
                            break;
                        case TargetSoftware.Revit_2016:
                            Install2016Tool();
                            break;
                        case TargetSoftware.Dynamo:
                            InstallDynamo();
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("Selection is empty. Please make a selection including at least one tool to be installed.", "Not Selected", MessageBoxButtons.OK, MessageBoxIcon.Information); 
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to install the selected tools.\n " + ex.Message, "MainForm:buttonInstall_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Install2013Tool()
        {
            try
            {
                if (CheckPrerequisite())
                {
                    TargetSoftware target = TargetSoftware.Revit_2013;

                    ProgressForm progressForm = new ProgressForm("Installing selected tools . . . ");
                    //DeleteOldDirectories(progressForm, target);
                    progressForm.Show();
                   
                    int count = 0;
                    
                    for (int i = 0; i < listViewTools.CheckedItems.Count; i++)
                    {
                        ListViewItem item = listViewTools.CheckedItems[i];
                        if (null != item.Tag)
                        {
                            Tool2013 tool = (Tool2013)item.Tag;
                            if (tool2013Dictionary.ContainsKey(tool))
                            {
                                RevitToolProperties tp = tool2013Dictionary[tool];
                                CopyFromStandard(tp, progressForm, target);

                                tp.InstallVersionInfo = tp.ReleaseVersionInfo;
                                tool2013Dictionary.Remove(tool);
                                tool2013Dictionary.Add(tool, tp);
                                item.SubItems[3].Text ="v."+ tp.InstallVersionInfo.FileVersion;
                            }
                        }
                        count++;
                    }

                    if (checkBoxBeta.Checked)
                    {
                        for (int i = 0; i < listViewBeta.CheckedItems.Count; i++)
                        {
                            ListViewItem item = listViewBeta.CheckedItems[i];
                            if (null != item.Tag)
                            {
                                Tool2013 tool = (Tool2013)item.Tag;
                                if (tool2013Dictionary.ContainsKey(tool))
                                {
                                    if (tool2013Dictionary.ContainsKey(tool))
                                    {
                                        RevitToolProperties tp = tool2013Dictionary[tool];
                                        CopyFromBeta(tp, progressForm, target);

                                        tp.InstallVersionInfo = tp.BetaVersionInfo;
                                        tool2013Dictionary.Remove(tool);
                                        tool2013Dictionary.Add(tool, tp);
                                        item.SubItems[3].Text = "v." + tp.InstallVersionInfo.FileVersion;
                                    }
                                }
                            }
                            count++;
                        }
                    }

                    //make copies of the default files first
                    RevitToolProperties defaultTP = tool2013Dictionary[Tool2013.Default];
                    if (checkBoxBeta.Checked) { CopyFromBeta(defaultTP, progressForm, target); }
                    else { CopyFromStandard(defaultTP, progressForm, target); }

                    progressForm.Close();

                    DialogResult dr = MessageBox.Show(count + " tools were successfully installed.\nThe new Addins will be available when Revit is restarted.\nWould you like to exit the installer?"
                        , "Installation Complete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.Yes)
                    {
                        this.Close();
                    }
                    else
                    {
                        Display2013Tools();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errors occured while installing 2013 tools.\n" + ex.Message, "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Install2014Tool()
        {
            try
            {
                if (CheckPrerequisite())
                {
                    TargetSoftware traget = TargetSoftware.Revit_2014;
                    ProgressForm progressForm = new ProgressForm("Installing selected tools . . . ");
                    //DeleteOldDirectories(progressForm, traget);
                    progressForm.Show();

                    int count = 0;

                    for (int i = 0; i < listViewTools.CheckedItems.Count; i++)
                    {
                        ListViewItem item = listViewTools.CheckedItems[i];
                        if (null != item.Tag)
                        {
                            Tool2014 tool = (Tool2014)item.Tag;
                            if (tool2014Dictionary.ContainsKey(tool))
                            {
                                RevitToolProperties tp = tool2014Dictionary[tool];
                                CopyFromStandard(tp, progressForm, traget);
                                
                                tp.InstallVersionInfo = tp.ReleaseVersionInfo;
                                tool2014Dictionary.Remove(tool);
                                tool2014Dictionary.Add(tool, tp);
                                item.SubItems[3].Text = "v."+tp.InstallVersionInfo.FileVersion;
                            }
                        }
                        count++;
                    }

                    if (checkBoxBeta.Checked)
                    {
                        for (int i = 0; i < listViewBeta.CheckedItems.Count; i++)
                        {
                            ListViewItem item = listViewBeta.CheckedItems[i];
                            if (null != item.Tag)
                            {
                                Tool2014 tool = (Tool2014)item.Tag;
                                if (tool2014Dictionary.ContainsKey(tool))
                                {
                                    if (tool2014Dictionary.ContainsKey(tool))
                                    {
                                        RevitToolProperties tp = tool2014Dictionary[tool];
                                        CopyFromBeta(tp, progressForm, traget);

                                        tp.InstallVersionInfo = tp.BetaVersionInfo;
                                        tool2014Dictionary.Remove(tool);
                                        tool2014Dictionary.Add(tool, tp);
                                        item.SubItems[3].Text = "v." + tp.InstallVersionInfo.FileVersion;
                                    }
                                }
                            }
                            count++;
                        }
                    }

                    //make copies of the default files first
                    RevitToolProperties defaultTP = tool2014Dictionary[Tool2014.Default];
                    if (checkBoxBeta.Checked ) { CopyFromBeta(defaultTP, progressForm, traget); }
                    else { CopyFromStandard(defaultTP, progressForm, traget); }
                    
                    progressForm.Close();

                    DialogResult dr = MessageBox.Show(count + " tools were successfully installed.\nThe new Addins will be available when Revit is restarted.\nWould you like to exit the installer?"
                        , "Installation Complete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.Yes)
                    {
                        this.Close();
                    }
                    else
                    {
                        Display2014Tools();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errors occured while installing 2014 tools.\n" + ex.Message, "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Install2015Tool()
        {
            try
            {
                if (CheckPrerequisite())
                {
                    TargetSoftware traget = TargetSoftware.Revit_2015;
                    ProgressForm progressForm = new ProgressForm("Installing selected tools . . . ");
                    //DeleteOldDirectories(progressForm, traget);
                    progressForm.Show();

                    int count = 0;

                    for (int i = 0; i < listViewTools.CheckedItems.Count; i++)
                    {
                        ListViewItem item = listViewTools.CheckedItems[i];
                        if (null != item.Tag)
                        {
                            Tool2015 tool = (Tool2015)item.Tag;
                            if (tool2015Dictionary.ContainsKey(tool))
                            {
                                RevitToolProperties tp = tool2015Dictionary[tool];
                                CopyFromStandard(tp, progressForm, traget);

                                tp.InstallVersionInfo = tp.ReleaseVersionInfo;
                                tool2015Dictionary.Remove(tool);
                                tool2015Dictionary.Add(tool, tp);
                                item.SubItems[3].Text = "v." + tp.InstallVersionInfo.FileVersion;
                            }
                        }
                        count++;
                    }

                    if (checkBoxBeta.Checked)
                    {
                        for (int i = 0; i < listViewBeta.CheckedItems.Count; i++)
                        {
                            ListViewItem item = listViewBeta.CheckedItems[i];
                            if (null != item.Tag)
                            {
                                Tool2015 tool = (Tool2015)item.Tag;
                                if (tool2015Dictionary.ContainsKey(tool))
                                {
                                    if (tool2015Dictionary.ContainsKey(tool))
                                    {
                                        RevitToolProperties tp = tool2015Dictionary[tool];
                                       
                                        CopyFromBeta(tp, progressForm, traget);
                                        tp.InstallVersionInfo = tp.BetaVersionInfo;
                                        tool2015Dictionary.Remove(tool);
                                        tool2015Dictionary.Add(tool, tp);
                                        item.SubItems[3].Text = "v." + tp.InstallVersionInfo.FileVersion;
                                    }
                                }
                            }
                            count++;
                        }
                    }

                    //make copies of the default files first
                    RevitToolProperties defaultTP = tool2015Dictionary[Tool2015.Default];
                    if (checkBoxBeta.Checked) { CopyFromBeta(defaultTP, progressForm, traget); }
                    else { CopyFromStandard(defaultTP, progressForm, traget); }

                    progressForm.Close();

                    DialogResult dr = MessageBox.Show(count + " tools were successfully installed.\nThe new Addins will be available when Revit is restarted.\nWould you like to exit the installer?"
                        , "Installation Complete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.Yes)
                    {
                        this.Close();
                    }
                    else
                    {
                        Display2015Tools();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errors occured while installing 2015 tools.\n" + ex.Message, "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Install2016Tool()
        {
            try
            {
                if (CheckPrerequisite())
                {
                    TargetSoftware traget = TargetSoftware.Revit_2016;
                    ProgressForm progressForm = new ProgressForm("Installing selected tools . . . ");
                    //DeleteOldDirectories(progressForm, traget);
                    progressForm.Show();

                    int count = 0;

                    for (int i = 0; i < listViewTools.CheckedItems.Count; i++)
                    {
                        ListViewItem item = listViewTools.CheckedItems[i];
                        if (null != item.Tag)
                        {
                            Tool2016 tool = (Tool2016)item.Tag;
                            if (tool2016Dictionary.ContainsKey(tool))
                            {
                                RevitToolProperties tp = tool2016Dictionary[tool];
                                CopyFromStandard(tp, progressForm, traget);

                                tp.InstallVersionInfo = tp.ReleaseVersionInfo;
                                tool2016Dictionary.Remove(tool);
                                tool2016Dictionary.Add(tool, tp);
                                item.SubItems[3].Text = "v." + tp.InstallVersionInfo.FileVersion;
                            }
                        }
                        count++;
                    }

                    if (checkBoxBeta.Checked)
                    {
                        for (int i = 0; i < listViewBeta.CheckedItems.Count; i++)
                        {
                            ListViewItem item = listViewBeta.CheckedItems[i];
                            if (null != item.Tag)
                            {
                                Tool2016 tool = (Tool2016)item.Tag;
                                if (tool2016Dictionary.ContainsKey(tool))
                                {
                                    if (tool2016Dictionary.ContainsKey(tool))
                                    {
                                        RevitToolProperties tp = tool2016Dictionary[tool];

                                        CopyFromBeta(tp, progressForm, traget);
                                        tp.InstallVersionInfo = tp.BetaVersionInfo;
                                        tool2016Dictionary.Remove(tool);
                                        tool2016Dictionary.Add(tool, tp);
                                        item.SubItems[3].Text = "v." + tp.InstallVersionInfo.FileVersion;
                                    }
                                }
                            }
                            count++;
                        }
                    }

                    progressForm.Close();

                    DialogResult dr = MessageBox.Show(count + " tools were successfully installed.\nThe new Addins will be available when Revit is restarted.\nWould you like to exit the installer?"
                        , "Installation Complete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.Yes)
                    {
                        this.Close();
                    }
                    else
                    {
                        Display2016Tools();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errors occured while installing 2015 tools.\n" + ex.Message, "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void InstallDynamo()
        {
            try
            {
                if (CheckPrerequisite())
                {
                    TargetSoftware target = TargetSoftware.Dynamo;
                    ProgressForm progressForm = new ProgressForm("Installing selected tools . . . ");
                    progressForm.SetMaximumValue(listViewTools.CheckedItems.Count+listViewBeta.CheckedItems.Count);

                    int count = 0;

                    for (int i = 0; i < listViewTools.CheckedItems.Count; i++)
                    {
                        ListViewItem item = listViewTools.CheckedItems[i];
                        if (null != item.Tag)
                        {
                            DynamoToolProperties tp = (DynamoToolProperties)item.Tag;
                            if (dynamoDictionary.ContainsKey(tp.ToolName))
                            {
                                tp.InstalledFileInfo=CopyFromStandard(tp, progressForm, target);
                                dynamoDictionary.Remove(tp.ToolName);
                                dynamoDictionary.Add(tp.ToolName, tp);

                                if (null != tp.InstalledFileInfo)
                                {
                                    item.SubItems[2].Text = "v." + tp.InstalledFileInfo.LastWriteTime.Date.ToString("d");
                                }
                            }
                        }
                        count++;
                    }

                    if (checkBoxBeta.Checked)
                    {
                        for (int i = 0; i < listViewBeta.CheckedItems.Count; i++)
                        {
                            ListViewItem item = listViewBeta.CheckedItems[i];
                            if (null != item.Tag)
                            {
                                DynamoToolProperties tp = (DynamoToolProperties)item.Tag;
                                if (dynamoDictionary.ContainsKey(tp.ToolName))
                                {
                                    tp.InstalledFileInfo = CopyFromBeta(tp, progressForm, target);
                                    dynamoDictionary.Remove(tp.ToolName);
                                    dynamoDictionary.Add(tp.ToolName, tp);

                                    if (null != tp.InstalledFileInfo)
                                    {
                                        item.SubItems[2].Text = "v." + tp.InstalledFileInfo.LastWriteTime.Date.ToString("d");
                                    }
                                }
                            }
                            count++;
                        }
                    }

                    progressForm.Close();

                    DialogResult dr = MessageBox.Show(count + " tools were successfully installed.\nThe new Addins will be available when Revit is restarted.\nWould you like to exit the installer?"
                        , "Installation Complete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.Yes)
                    {
                        this.Close();
                    }
                    else
                    {
                        DisplayDynamoTools();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errors occured while installing Dynamo definitions.\n" + ex.Message, "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeleteOldDirectories(ProgressForm progress, TargetSoftware target)
        {
            try
            {
                bool isWriteAccess = false;
                try
                {
                    AuthorizationRuleCollection collection = Directory.GetAccessControl(programDirectory).GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
                    foreach (FileSystemAccessRule rule in collection)
                    {
                        if (rule.AccessControlType == AccessControlType.Allow)
                        {
                            isWriteAccess = true;
                            break;
                        }
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    string message = ex.Message;
                    isWriteAccess = false;
                }

                if (isWriteAccess)
                {
                    int maxVal = CountInstallingFiles(target);
                    DirectoryInfo di = new DirectoryInfo(programDirectory);
                    List<RevitToolProperties> filesToMove = new List<RevitToolProperties>();
                    if (target == TargetSoftware.Revit_2013)
                    {
                        foreach (Tool2013 tool in tool2013Dictionary.Keys)
                        {
                            RevitToolProperties tp = tool2013Dictionary[tool];
                            FileInfo[] files = di.GetFiles(tp.DLLName, SearchOption.AllDirectories);
                            if (files.Length > 0)
                            {
                                filesToMove.Add(tp);
                                maxVal += tp.FilePaths.Count;
                            }
                        }
                    }
                    else if (target == TargetSoftware.Revit_2014)
                    {
                        foreach (Tool2014 tool in tool2014Dictionary.Keys)
                        {
                            RevitToolProperties tp = tool2014Dictionary[tool];
                            FileInfo[] files = di.GetFiles(tp.DLLName, SearchOption.AllDirectories);
                            if (files.Length > 0)
                            {
                                filesToMove.Add(tp);
                                maxVal += tp.FilePaths.Count;
                            }
                        }
                    }
                    else if (target == TargetSoftware.Revit_2015)
                    {
                        foreach (Tool2015 tool in tool2015Dictionary.Keys)
                        {
                            RevitToolProperties tp = tool2015Dictionary[tool];
                            FileInfo[] files = di.GetFiles(tp.DLLName, SearchOption.AllDirectories);
                            if (files.Length > 0)
                            {
                                filesToMove.Add(tp);
                                maxVal += tp.FilePaths.Count;
                            }
                        }
                    }
                    
                    progress.SetMaximumValue(maxVal);
                    progress.Show();
                    progress.Refresh();

                    if (filesToMove.Count > 0)
                    {
                        foreach (RevitToolProperties tp in filesToMove)
                        {
                            CopyFromStandard(tp, progress, target);
                        }
                    }
                    string dllDirectory = programDirectory + "/DLL";
                    if (Directory.Exists(dllDirectory))
                    {
                        try { Directory.Delete(dllDirectory, true); }
                        catch { }
                    }
                    string imgDirectory = programDirectory + "/Image";
                    if (Directory.Exists(imgDirectory))
                    {
                        try { Directory.Delete(imgDirectory, true); }
                        catch { }
                    }

                    if (File.Exists(programDirectory + "/HOK.RibbonTab.dll"))
                    {
                        try { File.Delete(programDirectory + "/HOK.RibbonTab.dll"); }
                        catch { }
                    }
                    if (File.Exists(programDirectory + "/HOK.RibbonTab.addin"))
                    {
                        try { File.Delete(programDirectory + "/HOK.RibbonTab.addin"); }
                        catch { }
                    }
                    if (File.Exists(programDirectory + "/HOK.Help.txt"))
                    {
                        try { File.Delete(programDirectory + "/HOK.Help.txt"); }
                        catch { }
                    }
                    if (File.Exists(programDirectory + "/HOK.Tooltip.txt"))
                    {
                        try { File.Delete(programDirectory + "/HOK.Tooltip.txt"); }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete files from old directories.\n" + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool CountCheckedItems()
        {
            bool found = false;
            try
            {
                if (listViewTools.CheckedItems.Count > 0)
                {
                    return true;
                }
                else if (checkBoxBeta.Checked)
                {
                    if (listViewBeta.CheckedItems.Count > 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find checked items.\n" + ex.Message, "Count Checked Items", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return found;
        }

        private bool CheckPrerequisite()
        {
            bool pass = true;
            try
            {
                Process[] prs = Process.GetProcesses();
                
                for (int i = 0; i < prs.Length; i++)
                {
                    if (prs[i].ProcessName == "Revit")
                    {
                        DialogResult dr = MessageBox.Show("The installer has detected that Revit is currently running.\n"
                        +"You can install/uninstall the addins, but an application restart is required before the new tools are available.\n"
                        +"Would you like to continue?"
                          , "Application in Use", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

                        if (dr == DialogResult.OK)
                        {
                            pass = true;
                        }
                        else
                        {
                            pass = false;
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check prerequisite conditions.\n"+ex.Message, "MainForm:CheckPrerequisite", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return pass;
        }

        private void CreateDirectories(TargetSoftware target)
        {
            try
            {
                string appDataDirectory = "";
                switch (target)
                {
                    case TargetSoftware.Revit_2013:
                        appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2013";
                        break;
                    case TargetSoftware.Revit_2014:
                        appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2014";
                        break;
                    case TargetSoftware.Revit_2015:
                        appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2015";
                        break;
                    case TargetSoftware.Revit_2016:
                        appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2016";
                        break;
                    case TargetSoftware.Dynamo:
                        appDataDirectory=@"C:\Autodesk\Dynamo\Core\definitions";
                        break;
                }
                
                if (Directory.Exists(appDataDirectory))
                {
                    if (target == TargetSoftware.Revit_2013 || target == TargetSoftware.Revit_2014 || target == TargetSoftware.Revit_2015)
                    {
                        if (!Directory.Exists(appDataDirectory + "\\HOK-Addin.bundle"))
                        {
                            Directory.CreateDirectory(appDataDirectory + "\\HOK-Addin.bundle");
                        }
                        if (!Directory.Exists(appDataDirectory + "\\HOK-Addin.bundle\\Contents"))
                        {
                            Directory.CreateDirectory(appDataDirectory + "\\HOK-Addin.bundle\\Contents");
                        }
                        if (!Directory.Exists(appDataDirectory + "\\HOK-Addin.bundle\\Contents\\Resources"))
                        {
                            Directory.CreateDirectory(appDataDirectory + "\\HOK-Addin.bundle\\Contents\\Resources");
                        }
                    }
                    else if (target == TargetSoftware.Revit_2016)
                    {
                        if (!Directory.Exists(appDataDirectory + "\\HOK-Addin.bundle"))
                        {
                            Directory.CreateDirectory(appDataDirectory + "\\HOK-Addin.bundle");
                        }
                        if (!Directory.Exists(appDataDirectory + "\\HOK-Addin.bundle\\Contents_Beta"))
                        {
                            Directory.CreateDirectory(appDataDirectory + "\\HOK-Addin.bundle\\Contents_Beta");
                        }
                        if (!Directory.Exists(appDataDirectory + "\\HOK-Addin.bundle\\Contents_Beta\\Resources"))
                        {
                            Directory.CreateDirectory(appDataDirectory + "\\HOK-Addin.bundle\\Contents_Beta\\Resources");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create prerequisite directories.\n" + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private int CountInstallingFiles(TargetSoftware target)
        {
            int count = 0;
            if (target == TargetSoftware.Revit_2013)
            {
                count = tool2013Dictionary[Tool2013.Default].FilePaths.Count;
                foreach (ListViewItem item in listViewTools.Items)
                {
                    if (item.Checked && null != item.Tag)
                    {
                        Tool2013 tool = (Tool2013)item.Tag;
                        count += tool2013Dictionary[tool].FilePaths.Count;
                    }
                }

                foreach (ListViewItem item in listViewBeta.Items)
                {
                    if (item.Checked && null != item.Tag)
                    {
                        Tool2013 tool = (Tool2013)item.Tag;
                        count += tool2013Dictionary[tool].FilePaths.Count;
                    }
                }
            }
            else if (target == TargetSoftware.Revit_2014)
            {
                count = tool2014Dictionary[Tool2014.Default].FilePaths.Count;
                foreach (ListViewItem item in listViewTools.Items)
                {
                    if (item.Checked && null != item.Tag)
                    {
                        Tool2014 tool = (Tool2014)item.Tag;
                        count += tool2014Dictionary[tool].FilePaths.Count;
                    }
                }

                foreach (ListViewItem item in listViewBeta.Items)
                {
                    if (item.Checked && null != item.Tag)
                    {
                        Tool2014 tool = (Tool2014)item.Tag;
                        count += tool2014Dictionary[tool].FilePaths.Count;
                    }
                }
            }
            else if (target == TargetSoftware.Revit_2015)
            {
                count = tool2015Dictionary[Tool2015.Default].FilePaths.Count;
                foreach (ListViewItem item in listViewTools.Items)
                {
                    if (item.Checked && null != item.Tag)
                    {
                        Tool2015 tool = (Tool2015)item.Tag;
                        count += tool2015Dictionary[tool].FilePaths.Count;
                    }
                }

                foreach (ListViewItem item in listViewBeta.Items)
                {
                    if (item.Checked && null != item.Tag)
                    {
                        Tool2015 tool = (Tool2015)item.Tag;
                        count += tool2015Dictionary[tool].FilePaths.Count;
                    }
                }
            }
            else if (target == TargetSoftware.Revit_2016)
            {
                
            }
            return count;
        }

        private void CopyFromStandard(RevitToolProperties tp, ProgressForm progressForm, TargetSoftware target)
        {
            try
            {
                List<string> fileNames = new List<string>();
                fileNames = tp.FilePaths;
                progressForm.SetValue(0);
                progressForm.SetMaximumValue(fileNames.Count);
                progressForm.Refresh();

                foreach (string path in fileNames)
                {
                    if(File.Exists(masterDirectory+path))
                    {
                        try { File.Copy(masterDirectory + path, installDirectory + path, true); }
                        catch { continue; }
                    }
                    progressForm.StepForward();
                }

                if (target == TargetSoftware.Revit_2013)
                {
                    if (installedVersions2013.ContainsKey(tp.ToolName))
                    {
                        installedVersions2013.Remove(tp.ToolName);
                    }
                    installedVersions2013.Add(tp.ToolName, tp.ReleaseVersionInfo.FileVersion);
                }
                else if (target == TargetSoftware.Revit_2014)
                {
                    if (installedVersions2014.ContainsKey(tp.ToolName))
                    {
                        installedVersions2014.Remove(tp.ToolName);
                    }
                    installedVersions2014.Add(tp.ToolName, tp.ReleaseVersionInfo.FileVersion);
                }
                else if (target == TargetSoftware.Revit_2015)
                {
                    if (installedVersions2015.ContainsKey(tp.ToolName))
                    {
                        installedVersions2015.Remove(tp.ToolName);
                    }
                    installedVersions2015.Add(tp.ToolName, tp.ReleaseVersionInfo.FileVersion);
                }
                else if (target == TargetSoftware.Revit_2016)
                {
                    if (installedVersions2016.ContainsKey(tp.ToolName))
                    {
                        installedVersions2016.Remove(tp.ToolName);
                    }
                    installedVersions2016.Add(tp.ToolName, tp.ReleaseVersionInfo.FileVersion);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to make copies from the master directory.\n Tool Name:"+tp.ToolName+"\n"+ex.Message, "MainForm:CopyFromMaster", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private FileInfo CopyFromStandard(DynamoToolProperties tp, ProgressForm progressForm, TargetSoftware target)
        {
            FileInfo installedFileInfo = null;
            try
            {
                string standardFilePath = Path.Combine(masterDirectory, tp.FileName);
                string installFilePath = Path.Combine(installDirectory, tp.FileName);
                if (File.Exists(standardFilePath))
                {
                    try { File.Copy(standardFilePath, installFilePath, true); }
                    catch { }
                }
                progressForm.StepForward();

                if (target == TargetSoftware.Dynamo)
                {
                    if (installedDateDynamo.ContainsKey(tp.ToolName))
                    {
                        installedDateDynamo.Remove(tp.ToolName);
                    }
                    installedDateDynamo.Add(tp.ToolName, tp.ReleasedFileInfo.LastWriteTime);
                }

                if (File.Exists(installFilePath))
                {
                    installedFileInfo = new FileInfo(installFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to make copies from the master directory.\n Tool Name:" + tp.ToolName + "\n" + ex.Message, "MainForm:CopyFromMaster", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return installedFileInfo;
        }

        private void CopyFromBeta(RevitToolProperties tp, ProgressForm progressForm, TargetSoftware target)
        {
            try
            {
                List<string> fileNames = new List<string>();
                fileNames = tp.FilePaths;
                progressForm.SetValue(0);
                progressForm.SetMaximumValue(fileNames.Count);
                progressForm.Refresh();

                foreach (string path in fileNames)
                {
                    string betaPath = betaDirectory + path;
                    string standardPath = masterDirectory + path;
                    string installPath = installDirectory + path;

                    if (File.Exists(betaPath))
                    {
                        try { File.Copy(betaPath, installPath, true); }
                        catch { continue; }
                    }
                    progressForm.StepForward();
                }

                if (null != tp.BetaVersionInfo)
                {
                    if (target == TargetSoftware.Revit_2013)
                    {
                        if (installedVersions2013.ContainsKey(tp.ToolName))
                        {
                            installedVersions2013.Remove(tp.ToolName);
                        }
                        installedVersions2013.Add(tp.ToolName, tp.BetaVersionInfo.FileVersion);
                    }
                    else if (target == TargetSoftware.Revit_2014)
                    {
                        if (installedVersions2014.ContainsKey(tp.ToolName))
                        {
                            installedVersions2014.Remove(tp.ToolName);
                        }
                        installedVersions2014.Add(tp.ToolName, tp.BetaVersionInfo.FileVersion);
                    }
                    else if (target == TargetSoftware.Revit_2015)
                    {
                        if (installedVersions2015.ContainsKey(tp.ToolName))
                        {
                            installedVersions2015.Remove(tp.ToolName);
                        }
                        installedVersions2015.Add(tp.ToolName, tp.BetaVersionInfo.FileVersion);
                    }
                    else if (target == TargetSoftware.Revit_2016)
                    {
                        if (installedVersions2016.ContainsKey(tp.ToolName))
                        {
                            installedVersions2016.Remove(tp.ToolName);
                        }
                        installedVersions2016.Add(tp.ToolName, tp.BetaVersionInfo.FileVersion);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to make copies from the beta directory.\n Tool Name:" + tp.ToolName + "\n" + ex.Message, "MainForm:CopyFromBeta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private FileInfo CopyFromBeta(DynamoToolProperties tp, ProgressForm progressForm, TargetSoftware target)
        {
            FileInfo installedFileInfo = null;
            try
            {
                string betaFilePath = Path.Combine(betaDirectory, tp.FileName);
                string installFilePath = Path.Combine(installDirectory, tp.FileName);
                if (File.Exists(betaFilePath))
                {
                    try { File.Copy(betaFilePath, installFilePath, true); }
                    catch { }
                }
                progressForm.StepForward();

                if (target == TargetSoftware.Dynamo)
                {
                    if (installedDateDynamo.ContainsKey(tp.ToolName))
                    {
                        installedDateDynamo.Remove(tp.ToolName);
                    }
                    installedDateDynamo.Add(tp.ToolName, tp.BetaFileInfo.LastWriteTime);
                }

                if (File.Exists(installFilePath))
                {
                    installedFileInfo = new FileInfo(installFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to make copies from the beta directory.\n Tool Name:" + tp.ToolName + "\n" + ex.Message, "MainForm:CopyFromBeta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return installedFileInfo;
        }

        private void buttonUninstall_Click(object sender, EventArgs e)
        {
            try
            {
                if (CountCheckedItems())
                {
                    if (CheckPrerequisite())
                    {
                        int count = 0;
                        TargetSoftware target = (TargetSoftware)comboBoxTarget.SelectedItem;

                        for (int i = 0; i < listViewTools.CheckedItems.Count; i++)
                        {
                            ListViewItem item = listViewTools.CheckedItems[i];
                            if (null != item.Tag)
                            {
                                switch (target)
                                {
                                    case TargetSoftware.Revit_2013:
                                        Tool2013 tool2013 = (Tool2013)item.Tag;
                                        if (tool2013Dictionary.ContainsKey(tool2013))
                                        {
                                            RevitToolProperties tp = tool2013Dictionary[tool2013];
                                            if (RemoveFile(tp, target)) { count++; }
                                        }
                                        break;
                                    case TargetSoftware.Revit_2014:
                                        Tool2014 tool2014 = (Tool2014)item.Tag;
                                        if (tool2014Dictionary.ContainsKey(tool2014))
                                        {
                                            RevitToolProperties tp = tool2014Dictionary[tool2014];
                                            if (RemoveFile(tp, target)) { count++; }
                                        }
                                        break;
                                    case TargetSoftware.Revit_2015:
                                        Tool2015 tool2015 = (Tool2015)item.Tag;
                                        if (tool2015Dictionary.ContainsKey(tool2015))
                                        {
                                            RevitToolProperties tp = tool2015Dictionary[tool2015];
                                            if (RemoveFile(tp, target)) { count++; }
                                        }
                                        break;
                                    case TargetSoftware.Revit_2016:
                                        Tool2016 tool2016 = (Tool2016)item.Tag;
                                        if (tool2016Dictionary.ContainsKey(tool2016))
                                        {
                                            RevitToolProperties tp = tool2016Dictionary[tool2016];
                                            if (RemoveFile(tp, target)) { count++; }
                                        }
                                        break;
                                    case TargetSoftware.Dynamo:
                                        DynamoToolProperties dtp = (DynamoToolProperties)item.Tag;
                                        if (dynamoDictionary.ContainsKey(dtp.ToolName))
                                        {
                                            if (RemoveFile(dtp, target)) { count++; }
                                        }
                                        break;
                                }
                            }
                        }
                        if (checkBoxBeta.Checked)
                        {
                            for (int i = 0; i < listViewBeta.CheckedItems.Count; i++)
                            {
                                ListViewItem item = listViewBeta.CheckedItems[i];
                                if (null != item.Tag)
                                {
                                    switch (target)
                                    {
                                        case TargetSoftware.Revit_2013:
                                            Tool2013 tool2013 = (Tool2013)item.Tag;
                                            if (tool2013Dictionary.ContainsKey(tool2013))
                                            {
                                                RevitToolProperties tp = tool2013Dictionary[tool2013];
                                                if (RemoveFile(tp, target)) { count++; }
                                            }
                                            break;
                                        case TargetSoftware.Revit_2014:
                                            Tool2014 tool2014 = (Tool2014)item.Tag;
                                            if (tool2014Dictionary.ContainsKey(tool2014))
                                            {
                                                RevitToolProperties tp = tool2014Dictionary[tool2014];
                                                if (RemoveFile(tp, target)) { count++; }
                                            }
                                            break;
                                        case TargetSoftware.Revit_2015:
                                            Tool2015 tool2015 = (Tool2015)item.Tag;
                                            if (tool2015Dictionary.ContainsKey(tool2015))
                                            {
                                                RevitToolProperties tp = tool2015Dictionary[tool2015];
                                                if (RemoveFile(tp, target)) { count++; }
                                            }
                                            break;
                                        case TargetSoftware.Revit_2016:
                                             Tool2016 tool2016 = (Tool2016)item.Tag;
                                            if (tool2016Dictionary.ContainsKey(tool2016))
                                            {
                                                RevitToolProperties tp = tool2016Dictionary[tool2016];
                                                if (RemoveFile(tp, target)) { count++; }
                                            }
                                            break;
                                        case TargetSoftware.Dynamo:
                                            DynamoToolProperties dtp = (DynamoToolProperties)item.Tag;
                                            if (dynamoDictionary.ContainsKey(dtp.ToolName))
                                            {
                                                if (RemoveFile(dtp, target)) { count++; }
                                            }
                                            break;
                                    }
                                }
                            }
                        }

                        if (count > 0)
                        {
                            DialogResult dr = MessageBox.Show(count + " tools were successfully uninstalled.", "Uninstallation Complete!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            if (dr == DialogResult.OK)
                            {
                                this.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to install the selected tools.\n " + ex.Message, "MainForm:buttonInstall_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool RemoveFile(RevitToolProperties tp, TargetSoftware target)
        {
            bool result = false;
            try
            {
                List<string> fileNames = new List<string>();
                fileNames = tp.FilePaths;
                foreach (string path in fileNames)
                {
                    if (File.Exists(installDirectory + path))
                    {
                        try { File.Delete(installDirectory + path); result = true; }
                        catch { }
                    }
                }

                if (target == TargetSoftware.Revit_2013)
                {
                    if (installedVersions2013.ContainsKey(tp.ToolName))
                    {
                        installedVersions2013.Remove(tp.ToolName);
                    }
                }
                else if (target == TargetSoftware.Revit_2014)
                {
                    if (installedVersions2014.ContainsKey(tp.ToolName))
                    {
                        installedVersions2014.Remove(tp.ToolName);
                    }
                }
                else if (target == TargetSoftware.Revit_2015)
                {
                    if (installedVersions2015.ContainsKey(tp.ToolName))
                    {
                        installedVersions2015.Remove(tp.ToolName);
                    }
                }
                else if (target == TargetSoftware.Revit_2016)
                {
                    if (installedVersions2016.ContainsKey(tp.ToolName))
                    {
                        installedVersions2016.Remove(tp.ToolName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to uninstall existing tools.\n"+ex.Message, "MainForm:RemoveFile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            return result;
        }

        private bool RemoveFile(DynamoToolProperties tp, TargetSoftware target)
        {
            bool result = false;
            try
            {
                string installedPath = Path.Combine(installDirectory, tp.FileName);
                if (File.Exists(installedPath))
                {
                    try { File.Delete(installedPath); result = true; }
                    catch { }
                }

                if (target == TargetSoftware.Dynamo)
                {
                    if (installedDateDynamo.ContainsKey(tp.ToolName))
                    {
                        installedDateDynamo.Remove(tp.ToolName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to uninstall existing tools.\n" + ex.Message, "MainForm:RemoveFile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            return result;
        }

        private void buttonAll_Click(object sender, EventArgs e)
        {
            if (buttonAll.ImageIndex == 0)
            {
                foreach (ListViewItem item in listViewTools.Items)
                {
                    item.Checked = false;
                }
                buttonAll.ImageIndex = 1;
            }
            else if (buttonAll.ImageIndex == 1)
            {
                foreach (ListViewItem item in listViewTools.Items)
                {
                    item.Checked = true;
                }
                buttonAll.ImageIndex = 0;
            }
        }

        private void checkBoxBeta_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxBeta.Checked)
            {
                splitContainerMain.Panel2Collapsed = false;
                int height = this.Size.Height + 150;
                this.Size = new Size(this.Size.Width, height);

                foreach (ListViewItem item in listViewTools.CheckedItems)
                {
                    if (listViewBeta.Items.ContainsKey(item.Name))
                    {
                        item.Checked = false;
                        listViewBeta.Items[item.Name].Checked = true;
                    }
                }
            }
            else
            {
                splitContainerMain.Panel2Collapsed = true;
                int height = this.Size.Height - 150;
                this.Size = new Size(this.Size.Width, height);

               
            }
        }

        private void listViewTools_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Checked && listViewBeta.Items.ContainsKey(e.Item.Name))
            {
                listViewBeta.Items[e.Item.Name].Checked = false;
            }
        }

        private void listViewBeta_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Checked && listViewTools.Items.ContainsKey(e.Item.Name))
            {
                listViewTools.Items[e.Item.Name].Checked = false;
            }
        }

        private void SelectRevit2013()
        {
            masterDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Addin Files\2013";
            betaDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Beta Files\2013";
            installDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2013";
            programDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Autodesk\REVIT\Addins\2013";

            if (Directory.Exists(masterDirectory))
            {
                Display2013Tools();
            }
            else
            {
                MessageBox.Show("Cannot access to the master directory for HOK AddIns Installer.\n You may not be in the HOK network connection.", "Directory Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SelectRevit2014()
        {
            masterDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Addin Files\2014";
            betaDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Beta Files\2014";
            installDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2014";
            programDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Autodesk\REVIT\Addins\2014";

            if (Directory.Exists(masterDirectory))
            {
                Display2014Tools();
            }
            else
            {
                MessageBox.Show("Cannot access to the master directory for HOK AddIns Installer.\n You may not be in the HOK network connection.", "Directory Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SelectRevit2015()
        {
            masterDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Addin Files\2015";
            betaDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Beta Files\2015";
            installDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2015";
            programDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Autodesk\REVIT\Addins\2015";

            if (Directory.Exists(masterDirectory))
            {
                Display2015Tools();
            }
            else
            {
                MessageBox.Show("Cannot access to the master directory for HOK AddIns Installer.\n You may not be in the HOK network connection.", "Directory Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SelectRevit2016()
        {
            masterDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Addin Files\2016";
            betaDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Beta Files\2016";
            installDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2016";
            programDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Autodesk\REVIT\Addins\2016";

            if (Directory.Exists(masterDirectory))
            {
                Display2016Tools();
            }
            else
            {
                MessageBox.Show("Cannot access to the master directory for HOK AddIns Installer.\n You may not be in the HOK network connection.", "Directory Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        } 

        private void SelectDynamo()
        {
            masterDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Dynamo\Standard Definitions";
            betaDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Dynamo\Beta Definitions";
            installDirectory = @"C:\Autodesk\Dynamo\Core\definitions";
            programDirectory = @"C:\Autodesk\Dynamo\Core\definitions";

            if (Directory.Exists(masterDirectory))
            {
                DisplayDynamoTools();
            }
            else
            {
                MessageBox.Show("Cannot access to the master directory for HOK AddIns Installer.\n You may not be in the HOK network connection.", "Directory Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void linkLabelHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Outlook.Application outlookApplication = new Outlook.Application();
            Outlook.NameSpace nameSpace = outlookApplication.GetNamespace("MAPI");
            Outlook.Folder folderInbox = (Outlook.Folder)nameSpace.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);
            Outlook.MailItem mailItem = (Outlook.MailItem)outlookApplication.CreateItem(Outlook.OlItemType.olMailItem);

            mailItem.Subject = "HOK Addins Installer Problem Report";
            mailItem.Body = "**** This email will go to the developer of the installer. ****\n" ;

            mailItem.Recipients.Add("jinsol.kim@hok.com");
            mailItem.Display(false);
        }

        private void comboBoxTarget_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                TargetSoftware target = (TargetSoftware)comboBoxTarget.SelectedItem;
                switch (target)
                {
                    case TargetSoftware.Revit_2013:
                       SelectRevit2013();
                        break;
                    case TargetSoftware.Revit_2014:
                        SelectRevit2014();
                        break;
                    case TargetSoftware.Revit_2015:
                        SelectRevit2015();
                        break;
                    case TargetSoftware.Revit_2016:
                        SelectRevit2016();
                        break;
                    case TargetSoftware.Dynamo:
                        SelectDynamo();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("The selected target software cannot be applied.\n"+ex.Message, "Target Software", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        
    }

}
