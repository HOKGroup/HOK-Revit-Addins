using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using HOK.BCFReader.GenericClasses;
using System.IO;
using Autodesk.Revit.UI.Selection;

namespace HOK.BCFReader.GenericForms
{
    public partial class CommandForm : System.Windows.Forms.Form
    {
        private UIApplication m_app;
        private Document m_doc;
        private string bcfFile;
        private Dictionary<string/*topicId*/, BCF> bcfFiles = new Dictionary<string, BCF>();
        private BCF selectedBCF = new BCF();
        private Comment selectedComment = new Comment();
        private int selStatusIndex = 0;
        private BCFReaderClass bcfReader;
        private INIDataManager iniDataManager;
        private Dictionary<string/*name*/, View3D> view3dDictionary = new Dictionary<string, View3D>();
        private ViewFamilyType view3dFamilyType=null;
        private string selectedView = "";
        private string[] bcfParameters = new string[] { "BCF_Name", "BCF_Date", "BCF_IssueNumber", "BCF_Topic", "BCF_Action", "BCF_Comment" };

        public CommandForm(UIApplication uiapp, string bcf, INIDataManager iniManager)
        {
            try
            {
                m_app = uiapp;
                m_doc = uiapp.ActiveUIDocument.Document;
                bcfFile = bcf;
                iniDataManager = iniManager;

                bcfReader = new BCFReaderClass(bcfFile, iniDataManager);
                bcfFiles = bcfReader.BcfFiles;

                InitializeComponent();
                this.Text = "HOK BCF Reader v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                CollectViews();
                DisplayIssues();
                if (listViewIssue.Items.Count > 0) { listViewIssue.Items[0].Selected = true; }
                if (listViewStatus.Items.Count > 0) { listViewStatus.Items[0].Selected = true; }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize CommandForm.\n"+ex.Message, "CommandForm", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectViews()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<Element> elements = collector.OfClass(typeof(View3D)).ToElements().ToList();

                foreach (Element e in elements)
                {
                    View3D view3D = e as View3D;
                    if (!view3dDictionary.ContainsKey(view3D.Name))
                    {
                        view3dDictionary.Add(view3D.Name, view3D);
                    }
                }

                collector = new FilteredElementCollector(m_doc);
                elements = collector.OfClass(typeof(ViewFamilyType)).ToElements().ToList();
                foreach (Element element in elements)
                {
                    ViewFamilyType viewfamilytype = element as ViewFamilyType;
                    if (viewfamilytype.ViewFamily == ViewFamily.ThreeDimensional)
                    {
                        view3dFamilyType = viewfamilytype;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect 3d view information.\n"+ex.Message, "CommandForm:CollectView", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DisplayIssues()
        {
            try
            {
                foreach (string topicId in bcfFiles.Keys)
                {
                    BCF bcf = bcfFiles[topicId];
                    ListViewItem item = new ListViewItem();
                    item.Text = bcf.IssueNumber;
                    item.SubItems.Add(bcf.MarkUp.MarkUpTopic.Title);
                    item.Tag = bcf;
                    listViewIssue.Items.Add(item);
                }
                listViewIssue.Sort();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display BCF Issues onto the command window.\n"+ex.Message, "CommandForm:DisplayIssues", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DisplayBcfData(int index)
        {
            try
            {
                ListViewItem item = listViewIssue.Items[index];
                if (null != item.Tag)
                {
                    BCF bcf = item.Tag as BCF;
                    selectedBCF = bcf;
                    //snapshot
                    pictureBox.Image = bcf.SnapShot;

                    //listViewModel
                    foreach (IfcFile ifc in bcf.MarkUp.Header)
                    {
                        ListViewItem modelItem = new ListViewItem();
                        modelItem.Text = ifc.FileName;
                        modelItem.SubItems.Add(ifc.Date.ToString());
                        modelItem.Tag = ifc;

                        listViewModel.Items.Add(modelItem);
                    }

                    //listViewStatus
                    foreach (Comment comment in bcf.MarkUp.Comments)
                    {
                        ListViewItem commentItem = new ListViewItem();
                        commentItem.Text = comment.Date.ToString();
                        commentItem.SubItems.Add(comment.VerbalStatus);
                        commentItem.SubItems.Add(comment.Author);
                        commentItem.SubItems.Add(comment.CommentString);
                        commentItem.SubItems.Add(comment.Action);
                        commentItem.Tag = comment;

                        listViewStatus.Items.Add(commentItem);
                    }

                    //listViewComponents
                    int i = 1;
                    int j = 1;
                    foreach (HOK.BCFReader.GenericClasses.Component component in bcf.ViewPoint.Components)
                    {
                        int eId = 0;
                        ListViewItem componentItem = new ListViewItem();
                        if (null != component.AuthoringToolId)
                        {
                            if (int.TryParse(component.AuthoringToolId, out eId))
                            {
                                ElementId elementId = new ElementId(eId);
                                Element element = m_doc.GetElement(elementId);
                                if (null != element)
                                {
                                    componentItem.Text = element.Name;
                                    componentItem.Tag = element;
                                }
                                else
                                {
                                    componentItem.Text = "Missing Component #" + i;
                                    componentItem.ForeColor = System.Drawing.Color.Gray;
                                    i++;
                                }
                            }
                            else
                            {
                                componentItem.Text = "Unknown Component #" + j;
                                componentItem.ForeColor = System.Drawing.Color.Gray;
                                j++;
                            }
                            componentItem.SubItems.Add(component.AuthoringToolId);
                        }
                        else
                        {
                            componentItem.Text = "Unknown Component #" + j;
                            componentItem.SubItems.Add(component.IfcGuid);
                            componentItem.ForeColor = System.Drawing.Color.Gray;
                            j++;
                        }
                        listViewComponents.Items.Add(componentItem);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to display BCF data onto the command window.\n" + ex.Message, "CommandForm:DisplayBcfData", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ClearAllContents()
        {
            try
            {
                pictureBox.Image = null;
                listViewModel.Items.Clear();
                listViewStatus.Items.Clear();
                listViewComponents.Items.Clear();
                richTextBoxComment.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display clear all items.\n" + ex.Message, "CommandForm:ClearAllContents", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void linkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void linkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }

        private void buttonPlus_Click(object sender, EventArgs e)
        {
            ImageForm imageForm = new ImageForm(pictureBox.Image);
            imageForm.Show();
        }

        private void listViewIssue_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewIssue.SelectedItems.Count > 0)
            {
                int i = listViewIssue.SelectedIndices[0];
                if (i > -1)
                {
                    ListViewItem item = listViewIssue.Items[i];
                    if (null != item.Tag)
                    {
                        BCF bcf = item.Tag as BCF;
                        selectedView = bcf.IssueNumber.ToString() + " - " + bcf.MarkUp.MarkUpTopic.Title;
                        selectedBCF = bcf;
                        selectedComment = new Comment();
                    }

                    ClearAllContents();
                    DisplayBcfData(i);
                }
            }
        }

        private void listViewStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                richTextBoxComment.Clear();
                StringBuilder strBuilder = new StringBuilder();

                if (listViewStatus.SelectedItems.Count > 0)
                {
                    selStatusIndex = listViewStatus.SelectedIndices[0];
                    if (selStatusIndex > -1)
                    {
                        ListViewItem item = listViewStatus.Items[selStatusIndex];
                        if (null != item.Tag)
                        {
                            Comment comment = item.Tag as Comment;
                            strBuilder.AppendLine("Author: " + comment.Author);
                            strBuilder.AppendLine("Date: " + comment.Date);
                            strBuilder.AppendLine("Status: " + comment.Status);
                            strBuilder.AppendLine("Verbal Status: " + comment.VerbalStatus);
                            strBuilder.AppendLine("");
                            strBuilder.AppendLine("Comment:");
                            strBuilder.AppendLine(comment.CommentString);

                            richTextBoxComment.Text = strBuilder.ToString();
                            selectedComment = comment;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display comments details.\n" + ex.Message, "CommandForm:listViewStatus_SelectedIndexChanged", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnNew_Click(object sender, EventArgs e)
        {
            try
            {
                Comment comment = new Comment();
                CommentForm commentForm = new CommentForm(comment);
                if (DialogResult.OK == commentForm.ShowDialog())
                {
                    comment = commentForm.CommentInfo;
                    comment.Topic = selectedBCF.MarkUp.MarkUpTopic;
                    comment.CommentGuid = Guid.NewGuid().ToString();
                    selectedBCF.MarkUp.Comments.Add(comment);

                    string topicId = selectedBCF.MarkUp.MarkUpTopic.TopicGuid;
                    if (bcfFiles.ContainsKey(topicId))
                    {
                        bcfFiles.Remove(topicId);
                        bcfFiles.Add(topicId, selectedBCF);
                    }

                    ListViewItem commentItem = new ListViewItem();
                    commentItem.Text = comment.Date.ToString();
                    commentItem.SubItems.Add(comment.VerbalStatus);
                    commentItem.SubItems.Add(comment.Author);
                    commentItem.SubItems.Add(comment.CommentString);
                    commentItem.SubItems.Add(comment.Action);
                    commentItem.Tag = comment;

                    listViewStatus.Items.Add(commentItem);
                    listViewStatus.Items[listViewStatus.Items.Count - 1].Selected = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a new comment item.\n" + ex.Message, "CommandForm:bttnNew_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                string authorName = "";
                ListViewItem item = listViewStatus.Items[selStatusIndex];
                if (null != item.Tag)
                {
                    Comment comment = item.Tag as Comment;
                    authorName = comment.Author;
                    if (authorName.ToUpper() == Environment.UserName.ToUpper())
                    {
                        CommentForm commentForm = new CommentForm(comment);
                        if (DialogResult.OK == commentForm.ShowDialog())
                        {
                            comment = commentForm.CommentInfo;
                            for (int i = 0; i < selectedBCF.MarkUp.Comments.Count; i++)
                            {
                                if (selectedBCF.MarkUp.Comments[i].CommentGuid == comment.CommentGuid)
                                {
                                    selectedBCF.MarkUp.Comments.RemoveAt(i);
                                    selectedBCF.MarkUp.Comments.Insert(i, comment);
                                }
                            }

                            string topicId = selectedBCF.MarkUp.MarkUpTopic.TopicGuid;
                            if (bcfFiles.ContainsKey(topicId))
                            {
                                bcfFiles.Remove(topicId);
                                bcfFiles.Add(topicId, selectedBCF);
                            }

                            ListViewItem commentItem = new ListViewItem();
                            commentItem.Text = comment.Date.ToString();
                            commentItem.SubItems.Add(comment.VerbalStatus);
                            commentItem.SubItems.Add(comment.Author);
                            commentItem.SubItems.Add(comment.CommentString);
                            commentItem.SubItems.Add(comment.Action);
                            commentItem.Tag = comment;
                            listViewStatus.Items.RemoveAt(selStatusIndex);
                            listViewStatus.Items.Insert(selStatusIndex, commentItem);
                            listViewStatus.Items[selStatusIndex].Selected = true;
                        }
                    }
                    else
                    {
                        MessageBox.Show("You're not allowed to edit this comment.\nThe comment has been created by another user. [" + authorName + "]", "Edit Comments", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to edit a comment item.\n" + ex.Message, "CommandForm:bttnEdit_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                string authorName = "";
                if (listViewStatus.Items.Count > 1)
                {
                    ListViewItem item = listViewStatus.Items[selStatusIndex];
                    if (null != item.Tag)
                    {
                        Comment comment = item.Tag as Comment;
                        authorName = comment.Author;
                        if (authorName.ToUpper() == Environment.UserName.ToUpper())
                        {
                            DialogResult dr = MessageBox.Show("Would you like to delete the comment?\n [" + comment.CommentString + "]", "Delete Comments:", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                            if (dr == DialogResult.OK)
                            {
                                for (int i = 0; i < selectedBCF.MarkUp.Comments.Count; i++)
                                {
                                    if (selectedBCF.MarkUp.Comments[i].CommentGuid == comment.CommentGuid)
                                    {
                                        selectedBCF.MarkUp.Comments.RemoveAt(i);
                                    }
                                }
                                string topicId = selectedBCF.MarkUp.MarkUpTopic.TopicGuid;
                                if (bcfFiles.ContainsKey(topicId))
                                {
                                    bcfFiles.Remove(topicId);
                                    bcfFiles.Add(topicId, selectedBCF);
                                }
                                listViewStatus.Items.RemoveAt(selStatusIndex);
                            }
                        }
                        else
                        {
                            MessageBox.Show("You're not allowed to delete this comment.\nThe comment has been created by another user. [" + authorName + "]", "Delete Comments", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Cannot delete the comment item.\n At least one comment item should exist.", "Warning:Comments", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete a comment item.\n" + ex.Message, "CommandForm:bttnDelete_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            try
            {
                string tempZip = bcfFile.Replace(".bcfzip", ".zip");
                if (File.Exists(tempZip))
                {
                    FileInfo fileInfo = new FileInfo(tempZip);
                    fileInfo.MoveTo(Path.ChangeExtension(tempZip, ".bcfzip"));
                }
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to change the file extension.\n" + ex.Message, "CommandForm:buttonClose_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                bcfReader.BcfFiles = bcfFiles;
                bcfReader.WriteMarkUp();
                bcfReader.ChangeToBcfzip();
                iniDataManager.BCFFiles = bcfFiles;
                iniDataManager.WriteINI();
                this.Close();
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save BCF file\n" + bcfFile + "\n" + ex.Message, "CommandForm:buttonSave_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonShow_Click(object sender, EventArgs e)
        {
            try
            {
                if (listViewComponents.SelectedItems.Count > 0)
                {
                    //Would you like to create a 3dview?
                    if (!view3dDictionary.ContainsKey(selectedView))
                    {
                        DialogResult dr = MessageBox.Show("Would you like to create a 3DView including all components for the selected issues?\n View Name:" + selectedView, "Create a 3DView", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dr == DialogResult.Yes)
                        {
                            if (Create3DView())
                            {
                                MessageBox.Show("[" + selectedView + "] was successfully created in 3DView.\n Please open this view to find the model elements.", "Created 3DView", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }

                    UIDocument uidoc = m_app.ActiveUIDocument;
                    List<ElementId> elementIds = new List<ElementId>();
#if RELEASE2015 || RELEASE2016
                    Selection selection = uidoc.Selection;
                    foreach (ListViewItem item in listViewComponents.SelectedItems)
                    {
                        if (null != item.Tag)
                        {
                            Element element = item.Tag as Element;
                            if (null != element)
                            {
                                ElementId elementId = element.Id;
                                elementIds.Add(elementId);
                            }
                        }
                    }
                    
                    
                    if (elementIds.Count > 0)
                    {
                        //uidoc.ActiveView.IsolateElementsTemporary(elementIds);
                        uidoc.ShowElements(elementIds);//+ make selection
                        selection.SetElementIds(elementIds); ;
                    }
#else
                    SelElementSet newSelection = SelElementSet.Create();

                    foreach (ListViewItem item in listViewComponents.SelectedItems)
                    {
                        if (null != item.Tag)
                        {
                            Element element = item.Tag as Element;
                            if (null != element)
                            {
                                ElementId elementId = element.Id;
                                elementIds.Add(elementId);
                                newSelection.Add(element);
                            }
                        }
                    }
                    if (elementIds.Count > 0)
                    {
                        //uidoc.ActiveView.IsolateElementsTemporary(elementIds);
                        uidoc.ShowElements(elementIds);//+ make selection
                        uidoc.Selection.Elements = newSelection;
                    }
#endif
                }
                else
                {
                    MessageBox.Show("Please select at least one component item to show elements in Revit UI.", "Select an Item.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to show Revit elements.\n" + ex.Message, "CommandForm:buttonShow_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool Create3DView()
        {
            bool result = false;
            try
            {
                BoundingBoxXYZ boundingBox = null;
                Dictionary<int, Element> elementDictionary = new Dictionary<int, Element>();
                foreach (ListViewItem item in listViewComponents.Items)
                {
                    if (null != item.Tag)
                    {
                        Element element = item.Tag as Element;
                        if (null != element.get_BoundingBox(null) && !elementDictionary.ContainsKey(element.Id.IntegerValue))
                        {
                            elementDictionary.Add(element.Id.IntegerValue, element);
                        }
                    }
                }

                boundingBox = elementDictionary[elementDictionary.Keys.First()].get_BoundingBox(null);
                
                double maxX = boundingBox.Max.X;
                double maxY = boundingBox.Max.Y;
                double maxZ = boundingBox.Max.Z;
                double minX = boundingBox.Min.X;
                double minY = boundingBox.Min.Y;
                double minZ = boundingBox.Min.Z;

                foreach (int eId in elementDictionary.Keys)
                {
                    Element element = elementDictionary[eId];
                    boundingBox = element.get_BoundingBox(null);
                    if (null != boundingBox)
                    {
                        if (boundingBox.Max.X > maxX) { maxX = boundingBox.Max.X; }
                        if (boundingBox.Max.Y > maxY) { maxY = boundingBox.Max.Y; }
                        if (boundingBox.Max.Z > maxZ) { maxZ = boundingBox.Max.Z; }
                        if (boundingBox.Min.X < minX) { minX = boundingBox.Min.X; }
                        if (boundingBox.Min.Y < minY) { minY = boundingBox.Min.Y; }
                        if (boundingBox.Min.Z < minZ) { minZ = boundingBox.Min.Z; }
                    }
                }

                XYZ xyzMax = new XYZ(maxX, maxY, maxZ);
                XYZ xyzMin = new XYZ(minX, minY, minZ);

                boundingBox.Max = xyzMax;
                boundingBox.Min = xyzMin;
                using (Transaction trans = new Transaction(m_doc))
                {
                    trans.Start("Create a 3DView");
                    View3D view3d = View3D.CreateIsometric(m_doc, view3dFamilyType.Id);
#if RELEASE2013
                    view3d.SectionBox = boundingBox;
#else
                    view3d.SetSectionBox(boundingBox);
                    view3d.GetSectionBox().Enabled = true;
#endif

                    view3d.Name = selectedView;

#if RELEASE2014||RELEASE2015 || RELEASE2016
                    SetElementTransparency(view3d, elementDictionary);
#endif

                    if (!view3dDictionary.ContainsKey(view3d.Name))
                    {
                        view3dDictionary.Add(view3d.Name, view3d);
                    }
                    trans.Commit();
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a 3DView named "+selectedView+"\n"+ex.Message, "CommandForm:Create3DView", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            return result;
        }

#if RELEASE2014||RELEASE2015 || RELEASE2016
        private void SetElementTransparency(View3D view3d, Dictionary<int, Element> elementDictionary)
        {
            try
            {

                this.toolStripProgressBar1.Visible = true;
                this.toolStripStatusLabel1.Text = "Set Transparency of Elements";
                Application.DoEvents();
                System.Threading.Thread.Sleep(1000);

                OverrideGraphicSettings settings = new OverrideGraphicSettings();
                settings.SetSurfaceTransparency(70);
                //settings.SetHalftone(true);
                
                BoundingBoxXYZ boundingBox=view3d.GetSectionBox();
                Outline outline=new Outline(boundingBox.Min, boundingBox.Max);
                BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline);
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                IList<Element> boundingBoxElements = collector.WherePasses(filter).ToElements();

                this.toolStripProgressBar1.Maximum = boundingBoxElements.Count;
                this.toolStripProgressBar1.Value = 0;
                this.toolStripProgressBar1.ProgressBar.Refresh();

                foreach (Element element in boundingBoxElements)
                {
                    if (elementDictionary.ContainsKey(element.Id.IntegerValue)) { continue; }
                    view3d.SetElementOverrides(element.Id, settings);
                    this.toolStripProgressBar1.PerformStep();
                }
                this.toolStripProgressBar1.Visible = false;
                this.toolStripStatusLabel1.Text = "Ready";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a 3DView named " + selectedView + "\n" + ex.Message, "CommandForm:Create3DView", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
#endif
        private void CommandForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                string tempZip = bcfFile.Replace(".bcfzip", ".zip");
                if (File.Exists(tempZip))
                {
                    FileInfo fileInfo = new FileInfo(tempZip);
                    fileInfo.MoveTo(Path.ChangeExtension(tempZip, ".bcfzip"));
                }

                string basefolder = bcfFile.Replace(".bcfzip", "");
                if (Directory.Exists(basefolder))
                {
                    Directory.Delete(basefolder, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to change file format.\n" + ex.Message, "CommandForm:CommandForm_FormClosed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnRename_Click(object sender, EventArgs e)
        {
            try
            {
                if (listViewIssue.SelectedIndices.Count > 0)
                {
                    int index = listViewIssue.SelectedIndices[0];
                    ListViewItem item = listViewIssue.Items[index];

                    if (null != item.Tag)
                    {
                        BCF bcf = item.Tag as BCF;
                        RenameForm renameForm = new RenameForm(bcf.IssueNumber);
                        if (renameForm.ShowDialog() == DialogResult.OK)
                        {
                            bcf.IssueNumber = renameForm.IssueNumber;
                            renameForm.Close();

                            string topicId = bcf.MarkUp.MarkUpTopic.TopicGuid;
                            if (bcfFiles.ContainsKey(topicId))
                            {
                                bcfFiles.Remove(topicId);
                                bcfFiles.Add(topicId, bcf);
                            }

                            listViewIssue.Items.RemoveAt(index);

                            ListViewItem newItem = new ListViewItem();
                            newItem.Text = bcf.IssueNumber;
                            newItem.SubItems.Add(bcf.MarkUp.MarkUpTopic.Title);
                            newItem.Tag = bcf;
                            listViewIssue.Items.Add(newItem);

                            listViewIssue.Sort();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to change the number of the issue.\n" + ex.Message, "CommandForm:bttnRename_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnParameter_Click(object sender, EventArgs e)
        {
            try
            {
                if (listViewComponents.SelectedItems.Count > 0)
                {
                    if (null == selectedBCF.IssueNumber)
                    {
                        MessageBox.Show("Please select an Issue and a comment on the lists.", "Missing Issues", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (null == selectedComment.CommentGuid)
                    {
                        MessageBox.Show("Please select a comment on the list.", "Missing Comments", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        StringBuilder strBuilder = new StringBuilder();
                        strBuilder.AppendLine("Would you like to write following data in shared parameters?\n\n");
                        strBuilder.AppendLine("[BCF_Name] : " + selectedComment.Author);
                        strBuilder.AppendLine("[BCF_Date] : " + selectedComment.Date.ToString());
                        strBuilder.AppendLine("[BCF_IssueNumber] : " + selectedBCF.IssueNumber);
                        strBuilder.AppendLine("[BCF_Topic] : " + selectedBCF.MarkUp.MarkUpTopic.Title);
                        strBuilder.AppendLine("[BCF_Action] : " + selectedComment.Action);
                        strBuilder.AppendLine("[BCF_Comment] : " + selectedComment.CommentString);
                        strBuilder.AppendLine("");

                        DialogResult dr;
                        dr = MessageBox.Show(strBuilder.ToString(), "Write to Shared Parameters", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                        if (dr == DialogResult.Yes)
                        {
                            bool result=true;
                            using (Transaction trans = new Transaction(m_doc))
                            {
                                trans.Start("Write Parameters");
                                foreach (ListViewItem item in listViewComponents.SelectedItems)
                                {
                                    if (null != item.Tag)
                                    {
                                        Element element = item.Tag as Element;
                                        foreach (string defName in bcfParameters)
                                        {
#if RELEASE2015 || RELEASE2016
                                            Parameter parameter = element.LookupParameter(defName);
#else
                                            Parameter parameter = element.get_Parameter(defName);
#endif

                                            if (null != parameter)
                                            {
                                                SetParameter(element, defName);
                                            }
                                            else
                                            {
                                                result = AddParameter(element);
                                                SetParameter(element, defName);
                                            }
                                        }
                                    }
                                }
                                trans.Commit();
                            }
                            if (result)
                            {
                                MessageBox.Show("Selected elements has been successfully updated with BCF shared parameters.", "BCF Sahred Parameters", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select at least one component item to show elements in Revit UI.", "Select an Item.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write shared parameters.\n" + ex.Message, "CommandForm:bttnParameter_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool AddParameter(Element element)
        {
            bool instanceBindOK = false;
            try
            {
                DefinitionFile definitionFile = m_app.Application.OpenSharedParameterFile();
                DefinitionGroup defGroup = definitionFile.Groups.get_Item("HOK BCF");
                if (null == defGroup)
                {
                    defGroup = definitionFile.Groups.Create("HOK BCF");
                }

                foreach (string defName in bcfParameters)
                {
#if RELEASE2015 || RELEASE2016
                    Parameter parameter = element.LookupParameter(defName);
#else
                    Parameter parameter = element.get_Parameter(defName);
#endif
                    if (null != parameter) { continue; }

                    Definition definition = defGroup.Definitions.get_Item(defName);
                    if (null == definition)
                    {
#if RELEASE2015 
                        ExternalDefinitonCreationOptions option = new ExternalDefinitonCreationOptions(defName, ParameterType.Text);
                        definition = defGroup.Definitions.Create(option);
#elif RELEASE2016
                        ExternalDefinitionCreationOptions option = new ExternalDefinitionCreationOptions(defName, ParameterType.Text);
                        definition = defGroup.Definitions.Create(option);
#else
                        definition = defGroup.Definitions.Create(defName, ParameterType.Text);
#endif
                    }

                    BindingMap bindingMap = m_app.ActiveUIDocument.Document.ParameterBindings;
                    InstanceBinding instanceBinding = bindingMap.get_Item(definition) as InstanceBinding;
                    if (null != instanceBinding)
                    {
                        instanceBinding.Categories.Insert(element.Category);
                        instanceBindOK = bindingMap.ReInsert(definition, instanceBinding);
                    }
                    else
                    {
                        CategorySet categories = m_app.Application.Create.NewCategorySet();
                        categories.Insert(element.Category);
                        instanceBinding = m_app.Application.Create.NewInstanceBinding(categories);
                        instanceBindOK = bindingMap.Insert(definition, instanceBinding, BuiltInParameterGroup.PG_TEXT);
                        
                    }
                    if (!instanceBindOK) { break; }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add project parameters for element "+element.Name+"\n"+ex.Message, "CommandForm:AddParameter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                instanceBindOK = false;
            }
            return instanceBindOK;
        }

        private void SetParameter(Element element, string defName)
        {
            try
            {
#if RELEASE2015 || RELEASE2016
                Parameter parameter = element.LookupParameter(defName);
#else
                Parameter parameter = element.get_Parameter(defName);
#endif
                if (null != parameter)
                {
                    switch (defName)
                    {
                        case "BCF_Name":
                            if (null != selectedComment.Author) { parameter.Set(selectedComment.Author); }
                            break;
                        case "BCF_Date":
                            if (null != selectedComment.Date) { parameter.Set(selectedComment.Date.ToString()); }
                            break;
                        case "BCF_IssueNumber":
                            if (null != selectedBCF.IssueNumber) { parameter.Set(selectedBCF.IssueNumber); }
                            break;
                        case "BCF_Topic":
                            if (null != selectedBCF.MarkUp.MarkUpTopic.Title) { parameter.Set(selectedBCF.MarkUp.MarkUpTopic.Title); }
                            break;
                        case "BCF_Action":
                            if (null != selectedComment.Action) { parameter.Set(selectedComment.Action); }
                            break;
                        case "BCF_Comment":
                            if (null != selectedComment.CommentString) { parameter.Set(selectedComment.CommentString); }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set parameter values for " + defName + "\n" + ex.Message, "CommandForm:SetParameter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

    }
}
