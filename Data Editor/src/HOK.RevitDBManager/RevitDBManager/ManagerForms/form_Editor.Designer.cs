namespace RevitDBManager.Forms
{
    partial class form_Editor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(form_Editor));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.bttnExisting = new System.Windows.Forms.Button();
            this.bttnChange = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBoxDbPath = new System.Windows.Forms.TextBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabCategory = new System.Windows.Forms.TabPage();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.bttnCancelCat = new System.Windows.Forms.Button();
            this.bttnSyncCat = new System.Windows.Forms.Button();
            this.bttnCategoryFilt = new System.Windows.Forms.Button();
            this.txtBoxCategory = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.listViewCategory = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label2 = new System.Windows.Forms.Label();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.bttnCancelFamily = new System.Windows.Forms.Button();
            this.bttnSyncType = new System.Windows.Forms.Button();
            this.bttnCheckNone = new System.Windows.Forms.Button();
            this.bttnCheckAll = new System.Windows.Forms.Button();
            this.bttnTypeFilt = new System.Windows.Forms.Button();
            this.txtBoxType = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.treeViewFamily = new System.Windows.Forms.TreeView();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.bttnSelectionFilt = new System.Windows.Forms.Button();
            this.txtBoxSelection = new System.Windows.Forms.TextBox();
            this.bttnDetailView = new System.Windows.Forms.Button();
            this.bttnIconView = new System.Windows.Forms.Button();
            this.bttnRemove = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.bttnAdd = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.listViewSelection = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imgListThumbnail = new System.Windows.Forms.ImageList(this.components);
            this.tabParameter = new System.Windows.Forms.TabPage();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.dataGridFamily = new System.Windows.Forms.DataGridView();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CategoryColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.splitContainer6 = new System.Windows.Forms.SplitContainer();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.checkBoxExcludeInst = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.lblFormat = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.lblParamType = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.lblGroup = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.lblDUT = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.radioBttnInst = new System.Windows.Forms.RadioButton();
            this.radioBttnType = new System.Windows.Forms.RadioButton();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.bttnRevit = new System.Windows.Forms.Button();
            this.bttnLock = new System.Windows.Forms.Button();
            this.dataGridParameter = new System.Windows.Forms.DataGridView();
            this.contextMenuLock = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.lockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.revitLockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textBox7 = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tabReference = new System.Windows.Forms.TabPage();
            this.splitContainer7 = new System.Windows.Forms.SplitContainer();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.lblExtDB = new System.Windows.Forms.Label();
            this.bttnBrowse = new System.Windows.Forms.Button();
            this.textBox9 = new System.Windows.Forms.TextBox();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.bttnAddRelationship = new System.Windows.Forms.Button();
            this.bttnEditParam = new System.Windows.Forms.Button();
            this.bttnDelParam = new System.Windows.Forms.Button();
            this.dataGridExt = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.textBox10 = new System.Windows.Forms.TextBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.linkAbout = new System.Windows.Forms.LinkLabel();
            this.linkHelp = new System.Windows.Forms.LinkLabel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.bttnBack = new System.Windows.Forms.Button();
            this.bttnNext = new System.Windows.Forms.Button();
            this.bttnCancel = new System.Windows.Forms.Button();
            this.bttnCreate = new System.Windows.Forms.Button();
            this.imageListLock = new System.Windows.Forms.ImageList(this.components);
            this.saveFileDialogDB = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.Column7 = new System.Windows.Forms.DataGridViewButtonColumn();
            this.Column8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column9 = new System.Windows.Forms.DataGridViewImageColumn();
            this.Column10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabCategory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tabParameter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridFamily)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer6)).BeginInit();
            this.splitContainer6.Panel1.SuspendLayout();
            this.splitContainer6.Panel2.SuspendLayout();
            this.splitContainer6.SuspendLayout();
            this.groupBox10.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridParameter)).BeginInit();
            this.contextMenuLock.SuspendLayout();
            this.tabReference.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer7)).BeginInit();
            this.splitContainer7.Panel1.SuspendLayout();
            this.splitContainer7.Panel2.SuspendLayout();
            this.splitContainer7.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBox9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridExt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.MenuBar;
            this.splitContainer1.Panel1.Controls.Add(this.bttnExisting);
            this.splitContainer1.Panel1.Controls.Add(this.bttnChange);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.txtBoxDbPath);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(984, 692);
            this.splitContainer1.SplitterDistance = 44;
            this.splitContainer1.TabIndex = 0;
            // 
            // bttnExisting
            // 
            this.bttnExisting.BackColor = System.Drawing.Color.Transparent;
            this.bttnExisting.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.bttnExisting.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnExisting.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.bttnExisting.Image = ((System.Drawing.Image)(resources.GetObject("bttnExisting.Image")));
            this.bttnExisting.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnExisting.Location = new System.Drawing.Point(791, 9);
            this.bttnExisting.Margin = new System.Windows.Forms.Padding(5);
            this.bttnExisting.Name = "bttnExisting";
            this.bttnExisting.Size = new System.Drawing.Size(94, 27);
            this.bttnExisting.TabIndex = 6;
            this.bttnExisting.Text = "From Existing";
            this.bttnExisting.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bttnExisting.UseVisualStyleBackColor = false;
            this.bttnExisting.Click += new System.EventHandler(this.bttnExisting_Click);
            // 
            // bttnChange
            // 
            this.bttnChange.BackColor = System.Drawing.Color.Transparent;
            this.bttnChange.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.bttnChange.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnChange.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.bttnChange.Image = ((System.Drawing.Image)(resources.GetObject("bttnChange.Image")));
            this.bttnChange.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnChange.Location = new System.Drawing.Point(687, 9);
            this.bttnChange.Margin = new System.Windows.Forms.Padding(5);
            this.bttnChange.Name = "bttnChange";
            this.bttnChange.Size = new System.Drawing.Size(94, 27);
            this.bttnChange.TabIndex = 5;
            this.bttnChange.Text = "Create New";
            this.bttnChange.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bttnChange.UseVisualStyleBackColor = false;
            this.bttnChange.Click += new System.EventHandler(this.bttnChange_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label1.Location = new System.Drawing.Point(13, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Linked Database:";
            // 
            // txtBoxDbPath
            // 
            this.txtBoxDbPath.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.txtBoxDbPath.Enabled = false;
            this.txtBoxDbPath.Location = new System.Drawing.Point(110, 14);
            this.txtBoxDbPath.Name = "txtBoxDbPath";
            this.txtBoxDbPath.ReadOnly = true;
            this.txtBoxDbPath.Size = new System.Drawing.Size(568, 20);
            this.txtBoxDbPath.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.BackColor = System.Drawing.Color.Transparent;
            this.splitContainer2.Panel2.Controls.Add(this.pictureBox2);
            this.splitContainer2.Panel2.Controls.Add(this.linkAbout);
            this.splitContainer2.Panel2.Controls.Add(this.linkHelp);
            this.splitContainer2.Panel2.Controls.Add(this.statusStrip1);
            this.splitContainer2.Panel2.Controls.Add(this.bttnBack);
            this.splitContainer2.Panel2.Controls.Add(this.bttnNext);
            this.splitContainer2.Panel2.Controls.Add(this.bttnCancel);
            this.splitContainer2.Panel2.Controls.Add(this.bttnCreate);
            this.splitContainer2.Panel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.splitContainer2.Size = new System.Drawing.Size(984, 644);
            this.splitContainer2.SplitterDistance = 562;
            this.splitContainer2.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabCategory);
            this.tabControl1.Controls.Add(this.tabParameter);
            this.tabControl1.Controls.Add(this.tabReference);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(984, 562);
            this.tabControl1.TabIndex = 1;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabCategory
            // 
            this.tabCategory.BackColor = System.Drawing.Color.Transparent;
            this.tabCategory.Controls.Add(this.splitContainer3);
            this.tabCategory.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabCategory.Location = new System.Drawing.Point(4, 24);
            this.tabCategory.Name = "tabCategory";
            this.tabCategory.Padding = new System.Windows.Forms.Padding(3);
            this.tabCategory.Size = new System.Drawing.Size(976, 534);
            this.tabCategory.TabIndex = 0;
            this.tabCategory.Text = "       Category       ";
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(3, 3);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.bttnCancelCat);
            this.splitContainer3.Panel1.Controls.Add(this.bttnSyncCat);
            this.splitContainer3.Panel1.Controls.Add(this.bttnCategoryFilt);
            this.splitContainer3.Panel1.Controls.Add(this.txtBoxCategory);
            this.splitContainer3.Panel1.Controls.Add(this.groupBox1);
            this.splitContainer3.Panel1.Controls.Add(this.label2);
            this.splitContainer3.Panel1.Padding = new System.Windows.Forms.Padding(5, 40, 5, 30);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer4);
            this.splitContainer3.Size = new System.Drawing.Size(970, 528);
            this.splitContainer3.SplitterDistance = 226;
            this.splitContainer3.TabIndex = 0;
            // 
            // bttnCancelCat
            // 
            this.bttnCancelCat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnCancelCat.BackColor = System.Drawing.Color.Transparent;
            this.bttnCancelCat.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.bttnCancelCat.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnCancelCat.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.bttnCancelCat.Image = ((System.Drawing.Image)(resources.GetObject("bttnCancelCat.Image")));
            this.bttnCancelCat.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnCancelCat.Location = new System.Drawing.Point(18, 500);
            this.bttnCancelCat.Name = "bttnCancelCat";
            this.bttnCancelCat.Size = new System.Drawing.Size(65, 25);
            this.bttnCancelCat.TabIndex = 8;
            this.bttnCancelCat.Text = "Cancel";
            this.bttnCancelCat.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bttnCancelCat.UseVisualStyleBackColor = false;
            this.bttnCancelCat.Click += new System.EventHandler(this.bttnCancelCat_Click);
            // 
            // bttnSyncCat
            // 
            this.bttnSyncCat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnSyncCat.BackColor = System.Drawing.Color.Transparent;
            this.bttnSyncCat.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.bttnSyncCat.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnSyncCat.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.bttnSyncCat.Image = ((System.Drawing.Image)(resources.GetObject("bttnSyncCat.Image")));
            this.bttnSyncCat.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnSyncCat.Location = new System.Drawing.Point(89, 500);
            this.bttnSyncCat.Name = "bttnSyncCat";
            this.bttnSyncCat.Size = new System.Drawing.Size(129, 25);
            this.bttnSyncCat.TabIndex = 7;
            this.bttnSyncCat.Text = "Auto Sync Category";
            this.bttnSyncCat.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.bttnSyncCat.UseVisualStyleBackColor = false;
            this.bttnSyncCat.Click += new System.EventHandler(this.bttnSyncCat_Click);
            // 
            // bttnCategoryFilt
            // 
            this.bttnCategoryFilt.BackColor = System.Drawing.Color.Transparent;
            this.bttnCategoryFilt.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.bttnCategoryFilt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnCategoryFilt.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.bttnCategoryFilt.Image = ((System.Drawing.Image)(resources.GetObject("bttnCategoryFilt.Image")));
            this.bttnCategoryFilt.Location = new System.Drawing.Point(134, 21);
            this.bttnCategoryFilt.Name = "bttnCategoryFilt";
            this.bttnCategoryFilt.Size = new System.Drawing.Size(25, 25);
            this.bttnCategoryFilt.TabIndex = 6;
            this.bttnCategoryFilt.UseVisualStyleBackColor = false;
            // 
            // txtBoxCategory
            // 
            this.txtBoxCategory.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtBoxCategory.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtBoxCategory.Location = new System.Drawing.Point(10, 24);
            this.txtBoxCategory.Name = "txtBoxCategory";
            this.txtBoxCategory.Size = new System.Drawing.Size(118, 20);
            this.txtBoxCategory.TabIndex = 3;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBox2);
            this.groupBox1.Controls.Add(this.listViewCategory);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.White;
            this.groupBox1.Location = new System.Drawing.Point(5, 40);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(5, 20, 5, 5);
            this.groupBox1.Size = new System.Drawing.Size(216, 458);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox2.BackColor = System.Drawing.Color.RoyalBlue;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.ForeColor = System.Drawing.Color.White;
            this.textBox2.Location = new System.Drawing.Point(5, 12);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(204, 15);
            this.textBox2.TabIndex = 3;
            this.textBox2.Text = "Category";
            this.textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // listViewCategory
            // 
            this.listViewCategory.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewCategory.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4});
            this.listViewCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewCategory.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewCategory.FullRowSelect = true;
            this.listViewCategory.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewCategory.Location = new System.Drawing.Point(5, 33);
            this.listViewCategory.MultiSelect = false;
            this.listViewCategory.Name = "listViewCategory";
            this.listViewCategory.Size = new System.Drawing.Size(206, 420);
            this.listViewCategory.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewCategory.TabIndex = 0;
            this.listViewCategory.UseCompatibleStateImageBehavior = false;
            this.listViewCategory.View = System.Windows.Forms.View.Details;
            this.listViewCategory.SelectedIndexChanged += new System.EventHandler(this.listViewCategory_SelectedIndexChanged);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Width = 201;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label2.Location = new System.Drawing.Point(7, 5);
            this.label2.Margin = new System.Windows.Forms.Padding(5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(124, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Step1: Select a category";
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.bttnCancelFamily);
            this.splitContainer4.Panel1.Controls.Add(this.bttnSyncType);
            this.splitContainer4.Panel1.Controls.Add(this.bttnCheckNone);
            this.splitContainer4.Panel1.Controls.Add(this.bttnCheckAll);
            this.splitContainer4.Panel1.Controls.Add(this.bttnTypeFilt);
            this.splitContainer4.Panel1.Controls.Add(this.txtBoxType);
            this.splitContainer4.Panel1.Controls.Add(this.label3);
            this.splitContainer4.Panel1.Controls.Add(this.groupBox2);
            this.splitContainer4.Panel1.Padding = new System.Windows.Forms.Padding(5, 40, 5, 30);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.splitContainer4.Panel2.Controls.Add(this.bttnSelectionFilt);
            this.splitContainer4.Panel2.Controls.Add(this.txtBoxSelection);
            this.splitContainer4.Panel2.Controls.Add(this.bttnDetailView);
            this.splitContainer4.Panel2.Controls.Add(this.bttnIconView);
            this.splitContainer4.Panel2.Controls.Add(this.bttnRemove);
            this.splitContainer4.Panel2.Controls.Add(this.label4);
            this.splitContainer4.Panel2.Controls.Add(this.bttnAdd);
            this.splitContainer4.Panel2.Controls.Add(this.groupBox3);
            this.splitContainer4.Panel2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.splitContainer4.Panel2.Padding = new System.Windows.Forms.Padding(65, 40, 5, 5);
            this.splitContainer4.Size = new System.Drawing.Size(740, 528);
            this.splitContainer4.SplitterDistance = 266;
            this.splitContainer4.TabIndex = 0;
            // 
            // bttnCancelFamily
            // 
            this.bttnCancelFamily.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnCancelFamily.BackColor = System.Drawing.Color.Transparent;
            this.bttnCancelFamily.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.bttnCancelFamily.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnCancelFamily.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.bttnCancelFamily.Image = ((System.Drawing.Image)(resources.GetObject("bttnCancelFamily.Image")));
            this.bttnCancelFamily.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnCancelFamily.Location = new System.Drawing.Point(63, 500);
            this.bttnCancelFamily.Name = "bttnCancelFamily";
            this.bttnCancelFamily.Size = new System.Drawing.Size(65, 25);
            this.bttnCancelFamily.TabIndex = 12;
            this.bttnCancelFamily.Text = "Cancel";
            this.bttnCancelFamily.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bttnCancelFamily.UseVisualStyleBackColor = false;
            this.bttnCancelFamily.Click += new System.EventHandler(this.bttnCancelFamily_Click);
            // 
            // bttnSyncType
            // 
            this.bttnSyncType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnSyncType.BackColor = System.Drawing.Color.Transparent;
            this.bttnSyncType.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.bttnSyncType.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnSyncType.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.bttnSyncType.Image = ((System.Drawing.Image)(resources.GetObject("bttnSyncType.Image")));
            this.bttnSyncType.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnSyncType.Location = new System.Drawing.Point(134, 500);
            this.bttnSyncType.Name = "bttnSyncType";
            this.bttnSyncType.Size = new System.Drawing.Size(124, 25);
            this.bttnSyncType.TabIndex = 11;
            this.bttnSyncType.Text = "Auto Sync Family";
            this.bttnSyncType.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.bttnSyncType.UseVisualStyleBackColor = false;
            this.bttnSyncType.Click += new System.EventHandler(this.bttnSyncType_Click);
            // 
            // bttnCheckNone
            // 
            this.bttnCheckNone.BackColor = System.Drawing.Color.Transparent;
            this.bttnCheckNone.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.bttnCheckNone.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnCheckNone.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.bttnCheckNone.Image = ((System.Drawing.Image)(resources.GetObject("bttnCheckNone.Image")));
            this.bttnCheckNone.Location = new System.Drawing.Point(239, 22);
            this.bttnCheckNone.Name = "bttnCheckNone";
            this.bttnCheckNone.Size = new System.Drawing.Size(23, 23);
            this.bttnCheckNone.TabIndex = 10;
            this.bttnCheckNone.UseVisualStyleBackColor = false;
            this.bttnCheckNone.Click += new System.EventHandler(this.bttnCheckNone_Click);
            // 
            // bttnCheckAll
            // 
            this.bttnCheckAll.BackColor = System.Drawing.Color.Transparent;
            this.bttnCheckAll.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.bttnCheckAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnCheckAll.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.bttnCheckAll.Image = ((System.Drawing.Image)(resources.GetObject("bttnCheckAll.Image")));
            this.bttnCheckAll.Location = new System.Drawing.Point(210, 22);
            this.bttnCheckAll.Name = "bttnCheckAll";
            this.bttnCheckAll.Size = new System.Drawing.Size(23, 23);
            this.bttnCheckAll.TabIndex = 9;
            this.bttnCheckAll.UseVisualStyleBackColor = false;
            this.bttnCheckAll.Click += new System.EventHandler(this.bttnCheckAll_Click);
            // 
            // bttnTypeFilt
            // 
            this.bttnTypeFilt.BackColor = System.Drawing.Color.Transparent;
            this.bttnTypeFilt.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.bttnTypeFilt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnTypeFilt.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.bttnTypeFilt.Image = ((System.Drawing.Image)(resources.GetObject("bttnTypeFilt.Image")));
            this.bttnTypeFilt.Location = new System.Drawing.Point(134, 21);
            this.bttnTypeFilt.Name = "bttnTypeFilt";
            this.bttnTypeFilt.Size = new System.Drawing.Size(25, 25);
            this.bttnTypeFilt.TabIndex = 8;
            this.bttnTypeFilt.UseVisualStyleBackColor = false;
            // 
            // txtBoxType
            // 
            this.txtBoxType.Location = new System.Drawing.Point(10, 24);
            this.txtBoxType.Name = "txtBoxType";
            this.txtBoxType.Size = new System.Drawing.Size(118, 20);
            this.txtBoxType.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label3.Location = new System.Drawing.Point(10, 5);
            this.label3.Margin = new System.Windows.Forms.Padding(5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(167, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Step2: Check multiple family types";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.treeViewFamily);
            this.groupBox2.Controls.Add(this.textBox3);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.ForeColor = System.Drawing.Color.White;
            this.groupBox2.Location = new System.Drawing.Point(5, 40);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(5, 20, 5, 5);
            this.groupBox2.Size = new System.Drawing.Size(256, 458);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            // 
            // treeViewFamily
            // 
            this.treeViewFamily.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeViewFamily.CheckBoxes = true;
            this.treeViewFamily.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewFamily.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeViewFamily.Location = new System.Drawing.Point(5, 33);
            this.treeViewFamily.Name = "treeViewFamily";
            this.treeViewFamily.Size = new System.Drawing.Size(246, 420);
            this.treeViewFamily.TabIndex = 5;
            this.treeViewFamily.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeViewFamily_AfterCheck);
            // 
            // textBox3
            // 
            this.textBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox3.BackColor = System.Drawing.Color.CornflowerBlue;
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox3.ForeColor = System.Drawing.Color.White;
            this.textBox3.Location = new System.Drawing.Point(5, 12);
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.Size = new System.Drawing.Size(246, 15);
            this.textBox3.TabIndex = 4;
            this.textBox3.Text = "Family Type";
            this.textBox3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // bttnSelectionFilt
            // 
            this.bttnSelectionFilt.BackColor = System.Drawing.Color.Transparent;
            this.bttnSelectionFilt.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.bttnSelectionFilt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnSelectionFilt.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.bttnSelectionFilt.Image = ((System.Drawing.Image)(resources.GetObject("bttnSelectionFilt.Image")));
            this.bttnSelectionFilt.Location = new System.Drawing.Point(191, 21);
            this.bttnSelectionFilt.Name = "bttnSelectionFilt";
            this.bttnSelectionFilt.Size = new System.Drawing.Size(25, 25);
            this.bttnSelectionFilt.TabIndex = 12;
            this.bttnSelectionFilt.UseVisualStyleBackColor = false;
            this.bttnSelectionFilt.Click += new System.EventHandler(this.bttnSelectionFilt_Click);
            // 
            // txtBoxSelection
            // 
            this.txtBoxSelection.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtBoxSelection.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtBoxSelection.Location = new System.Drawing.Point(67, 24);
            this.txtBoxSelection.Name = "txtBoxSelection";
            this.txtBoxSelection.Size = new System.Drawing.Size(118, 20);
            this.txtBoxSelection.TabIndex = 11;
            this.txtBoxSelection.Enter += new System.EventHandler(this.txtBoxSelection_Enter);
            // 
            // bttnDetailView
            // 
            this.bttnDetailView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnDetailView.BackColor = System.Drawing.Color.Transparent;
            this.bttnDetailView.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.bttnDetailView.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnDetailView.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.bttnDetailView.Image = ((System.Drawing.Image)(resources.GetObject("bttnDetailView.Image")));
            this.bttnDetailView.Location = new System.Drawing.Point(433, 19);
            this.bttnDetailView.Name = "bttnDetailView";
            this.bttnDetailView.Size = new System.Drawing.Size(25, 25);
            this.bttnDetailView.TabIndex = 12;
            this.bttnDetailView.UseVisualStyleBackColor = false;
            this.bttnDetailView.Click += new System.EventHandler(this.bttnDetailView_Click);
            // 
            // bttnIconView
            // 
            this.bttnIconView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnIconView.BackColor = System.Drawing.Color.Transparent;
            this.bttnIconView.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.bttnIconView.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnIconView.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.bttnIconView.Image = ((System.Drawing.Image)(resources.GetObject("bttnIconView.Image")));
            this.bttnIconView.Location = new System.Drawing.Point(402, 19);
            this.bttnIconView.Name = "bttnIconView";
            this.bttnIconView.Size = new System.Drawing.Size(25, 25);
            this.bttnIconView.TabIndex = 11;
            this.bttnIconView.UseVisualStyleBackColor = false;
            this.bttnIconView.Click += new System.EventHandler(this.bttnIconView_Click);
            // 
            // bttnRemove
            // 
            this.bttnRemove.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.bttnRemove.FlatAppearance.BorderSize = 0;
            this.bttnRemove.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.bttnRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bttnRemove.Image = ((System.Drawing.Image)(resources.GetObject("bttnRemove.Image")));
            this.bttnRemove.Location = new System.Drawing.Point(12, 297);
            this.bttnRemove.Name = "bttnRemove";
            this.bttnRemove.Size = new System.Drawing.Size(40, 40);
            this.bttnRemove.TabIndex = 5;
            this.bttnRemove.UseVisualStyleBackColor = true;
            this.bttnRemove.Click += new System.EventHandler(this.bttnRemove_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label4.Location = new System.Drawing.Point(67, 5);
            this.label4.Margin = new System.Windows.Forms.Padding(5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(222, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Step3: Add/Remove family types for selection";
            // 
            // bttnAdd
            // 
            this.bttnAdd.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.bttnAdd.FlatAppearance.BorderSize = 0;
            this.bttnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bttnAdd.Image = ((System.Drawing.Image)(resources.GetObject("bttnAdd.Image")));
            this.bttnAdd.Location = new System.Drawing.Point(12, 156);
            this.bttnAdd.Name = "bttnAdd";
            this.bttnAdd.Size = new System.Drawing.Size(40, 40);
            this.bttnAdd.TabIndex = 2;
            this.bttnAdd.UseVisualStyleBackColor = true;
            this.bttnAdd.Click += new System.EventHandler(this.bttnAdd_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.textBox4);
            this.groupBox3.Controls.Add(this.listViewSelection);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.ForeColor = System.Drawing.Color.White;
            this.groupBox3.Location = new System.Drawing.Point(65, 40);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(5, 20, 5, 5);
            this.groupBox3.Size = new System.Drawing.Size(400, 483);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            // 
            // textBox4
            // 
            this.textBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox4.BackColor = System.Drawing.Color.DarkRed;
            this.textBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox4.ForeColor = System.Drawing.Color.White;
            this.textBox4.Location = new System.Drawing.Point(5, 12);
            this.textBox4.Multiline = true;
            this.textBox4.Name = "textBox4";
            this.textBox4.ReadOnly = true;
            this.textBox4.Size = new System.Drawing.Size(390, 15);
            this.textBox4.TabIndex = 5;
            this.textBox4.Text = "Family Type Selection";
            this.textBox4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // listViewSelection
            // 
            this.listViewSelection.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewSelection.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listViewSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewSelection.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewSelection.ForeColor = System.Drawing.SystemColors.WindowText;
            this.listViewSelection.FullRowSelect = true;
            this.listViewSelection.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewSelection.LargeImageList = this.imgListThumbnail;
            this.listViewSelection.Location = new System.Drawing.Point(5, 33);
            this.listViewSelection.Name = "listViewSelection";
            this.listViewSelection.Size = new System.Drawing.Size(390, 445);
            this.listViewSelection.SmallImageList = this.imgListThumbnail;
            this.listViewSelection.TabIndex = 1;
            this.listViewSelection.UseCompatibleStateImageBehavior = false;
            this.listViewSelection.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Family Type Name";
            this.columnHeader1.Width = 170;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Family Name";
            this.columnHeader2.Width = 115;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Category Name";
            this.columnHeader3.Width = 105;
            // 
            // imgListThumbnail
            // 
            this.imgListThumbnail.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgListThumbnail.ImageStream")));
            this.imgListThumbnail.TransparentColor = System.Drawing.Color.Transparent;
            this.imgListThumbnail.Images.SetKeyName(0, "room.png");
            this.imgListThumbnail.Images.SetKeyName(1, "space.png");
            this.imgListThumbnail.Images.SetKeyName(2, "area.png");
            // 
            // tabParameter
            // 
            this.tabParameter.BackColor = System.Drawing.Color.Transparent;
            this.tabParameter.Controls.Add(this.splitContainer5);
            this.tabParameter.Location = new System.Drawing.Point(4, 24);
            this.tabParameter.Name = "tabParameter";
            this.tabParameter.Padding = new System.Windows.Forms.Padding(3);
            this.tabParameter.Size = new System.Drawing.Size(976, 534);
            this.tabParameter.TabIndex = 1;
            this.tabParameter.Text = "       Parameters         ";
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.Location = new System.Drawing.Point(3, 3);
            this.splitContainer5.Name = "splitContainer5";
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.groupBox4);
            this.splitContainer5.Panel1.Controls.Add(this.button2);
            this.splitContainer5.Panel1.Controls.Add(this.textBox1);
            this.splitContainer5.Panel1.Controls.Add(this.label5);
            this.splitContainer5.Panel1.Padding = new System.Windows.Forms.Padding(5, 50, 5, 5);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.splitContainer6);
            this.splitContainer5.Size = new System.Drawing.Size(970, 528);
            this.splitContainer5.SplitterDistance = 250;
            this.splitContainer5.TabIndex = 0;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.dataGridFamily);
            this.groupBox4.Controls.Add(this.textBox6);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox4.ForeColor = System.Drawing.SystemColors.WindowText;
            this.groupBox4.Location = new System.Drawing.Point(5, 50);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(5, 20, 5, 5);
            this.groupBox4.Size = new System.Drawing.Size(240, 473);
            this.groupBox4.TabIndex = 10;
            this.groupBox4.TabStop = false;
            // 
            // dataGridFamily
            // 
            this.dataGridFamily.AllowUserToAddRows = false;
            this.dataGridFamily.AllowUserToDeleteRows = false;
            this.dataGridFamily.AllowUserToResizeRows = false;
            this.dataGridFamily.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dataGridFamily.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridFamily.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridFamily.ColumnHeadersVisible = false;
            this.dataGridFamily.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column5,
            this.CategoryColumn});
            this.dataGridFamily.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridFamily.GridColor = System.Drawing.Color.LightGray;
            this.dataGridFamily.Location = new System.Drawing.Point(5, 33);
            this.dataGridFamily.Name = "dataGridFamily";
            this.dataGridFamily.ReadOnly = true;
            this.dataGridFamily.RowHeadersVisible = false;
            this.dataGridFamily.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridFamily.Size = new System.Drawing.Size(230, 435);
            this.dataGridFamily.TabIndex = 4;
            this.dataGridFamily.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridFamily_CellClick);
            // 
            // Column5
            // 
            this.Column5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column5.HeaderText = "CategoryAndFamily";
            this.Column5.Name = "Column5";
            this.Column5.ReadOnly = true;
            // 
            // CategoryColumn
            // 
            this.CategoryColumn.HeaderText = "CategoryName";
            this.CategoryColumn.Name = "CategoryColumn";
            this.CategoryColumn.ReadOnly = true;
            this.CategoryColumn.Visible = false;
            // 
            // textBox6
            // 
            this.textBox6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox6.BackColor = System.Drawing.Color.RoyalBlue;
            this.textBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox6.ForeColor = System.Drawing.Color.White;
            this.textBox6.Location = new System.Drawing.Point(5, 12);
            this.textBox6.Multiline = true;
            this.textBox6.Name = "textBox6";
            this.textBox6.ReadOnly = true;
            this.textBox6.Size = new System.Drawing.Size(228, 15);
            this.textBox6.TabIndex = 3;
            this.textBox6.Text = "Families";
            this.textBox6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.Transparent;
            this.button2.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.button2.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));
            this.button2.Location = new System.Drawing.Point(132, 25);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(25, 25);
            this.button2.TabIndex = 9;
            this.button2.UseVisualStyleBackColor = false;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(8, 26);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(118, 21);
            this.textBox1.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label5.Location = new System.Drawing.Point(7, 5);
            this.label5.Margin = new System.Windows.Forms.Padding(5);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(211, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Step1: Select a family to display parameters";
            // 
            // splitContainer6
            // 
            this.splitContainer6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer6.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer6.Location = new System.Drawing.Point(0, 0);
            this.splitContainer6.Name = "splitContainer6";
            // 
            // splitContainer6.Panel1
            // 
            this.splitContainer6.Panel1.Controls.Add(this.label9);
            this.splitContainer6.Panel1.Controls.Add(this.groupBox10);
            this.splitContainer6.Panel1.Controls.Add(this.label7);
            this.splitContainer6.Panel1.Controls.Add(this.groupBox7);
            this.splitContainer6.Panel1.Controls.Add(this.groupBox6);
            // 
            // splitContainer6.Panel2
            // 
            this.splitContainer6.Panel2.Controls.Add(this.groupBox5);
            this.splitContainer6.Panel2.Controls.Add(this.label6);
            this.splitContainer6.Panel2.Padding = new System.Windows.Forms.Padding(0, 28, 0, 0);
            this.splitContainer6.Size = new System.Drawing.Size(716, 528);
            this.splitContainer6.SplitterDistance = 234;
            this.splitContainer6.TabIndex = 14;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label9.Location = new System.Drawing.Point(6, 84);
            this.label9.Margin = new System.Windows.Forms.Padding(5);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(161, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "Step 3: Select a Parameter Type";
            // 
            // groupBox10
            // 
            this.groupBox10.Controls.Add(this.checkBoxExcludeInst);
            this.groupBox10.Location = new System.Drawing.Point(3, 26);
            this.groupBox10.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(224, 51);
            this.groupBox10.TabIndex = 15;
            this.groupBox10.TabStop = false;
            // 
            // checkBoxExcludeInst
            // 
            this.checkBoxExcludeInst.AutoSize = true;
            this.checkBoxExcludeInst.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.checkBoxExcludeInst.Location = new System.Drawing.Point(28, 20);
            this.checkBoxExcludeInst.Name = "checkBoxExcludeInst";
            this.checkBoxExcludeInst.Size = new System.Drawing.Size(148, 19);
            this.checkBoxExcludeInst.TabIndex = 0;
            this.checkBoxExcludeInst.Text = "Exclude Instance Data";
            this.checkBoxExcludeInst.UseVisualStyleBackColor = true;
            this.checkBoxExcludeInst.CheckedChanged += new System.EventHandler(this.checkBoxExcludeInst_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label7.Location = new System.Drawing.Point(6, 8);
            this.label7.Margin = new System.Windows.Forms.Padding(5);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(192, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Step 2: Exclude/Include Instance Data";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.lblFormat);
            this.groupBox7.Controls.Add(this.label14);
            this.groupBox7.Controls.Add(this.lblParamType);
            this.groupBox7.Controls.Add(this.label17);
            this.groupBox7.Controls.Add(this.lblType);
            this.groupBox7.Controls.Add(this.lblGroup);
            this.groupBox7.Controls.Add(this.lblName);
            this.groupBox7.Controls.Add(this.label13);
            this.groupBox7.Controls.Add(this.lblDUT);
            this.groupBox7.Controls.Add(this.label11);
            this.groupBox7.Controls.Add(this.label10);
            this.groupBox7.Controls.Add(this.label8);
            this.groupBox7.Controls.Add(this.textBox8);
            this.groupBox7.Location = new System.Drawing.Point(3, 184);
            this.groupBox7.Margin = new System.Windows.Forms.Padding(5, 3, 5, 5);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.groupBox7.Size = new System.Drawing.Size(226, 339);
            this.groupBox7.TabIndex = 13;
            this.groupBox7.TabStop = false;
            // 
            // lblFormat
            // 
            this.lblFormat.AutoSize = true;
            this.lblFormat.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFormat.ForeColor = System.Drawing.Color.SteelBlue;
            this.lblFormat.Location = new System.Drawing.Point(25, 312);
            this.lblFormat.Margin = new System.Windows.Forms.Padding(5, 3, 5, 5);
            this.lblFormat.Name = "lblFormat";
            this.lblFormat.Size = new System.Drawing.Size(90, 13);
            this.lblFormat.TabIndex = 32;
            this.lblFormat.Text = "Parameter Format";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label14.Location = new System.Drawing.Point(8, 291);
            this.label14.Margin = new System.Windows.Forms.Padding(5);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(93, 13);
            this.label14.TabIndex = 31;
            this.label14.Text = "Parameter Format:";
            // 
            // lblParamType
            // 
            this.lblParamType.AutoSize = true;
            this.lblParamType.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblParamType.ForeColor = System.Drawing.Color.SteelBlue;
            this.lblParamType.Location = new System.Drawing.Point(25, 259);
            this.lblParamType.Margin = new System.Windows.Forms.Padding(5, 3, 5, 5);
            this.lblParamType.Name = "lblParamType";
            this.lblParamType.Size = new System.Drawing.Size(82, 13);
            this.lblParamType.TabIndex = 30;
            this.lblParamType.Text = "Parameter Type";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label17.Location = new System.Drawing.Point(8, 238);
            this.label17.Margin = new System.Windows.Forms.Padding(5);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(85, 13);
            this.label17.TabIndex = 29;
            this.label17.Text = "Parameter Type:";
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblType.ForeColor = System.Drawing.Color.SteelBlue;
            this.lblType.Location = new System.Drawing.Point(27, 159);
            this.lblType.Margin = new System.Windows.Forms.Padding(5, 3, 5, 5);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(94, 13);
            this.lblType.TabIndex = 28;
            this.lblType.Text = "Type of Parameter";
            // 
            // lblGroup
            // 
            this.lblGroup.AutoSize = true;
            this.lblGroup.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGroup.ForeColor = System.Drawing.Color.SteelBlue;
            this.lblGroup.Location = new System.Drawing.Point(25, 108);
            this.lblGroup.Margin = new System.Windows.Forms.Padding(5, 3, 5, 5);
            this.lblGroup.Name = "lblGroup";
            this.lblGroup.Size = new System.Drawing.Size(87, 13);
            this.lblGroup.TabIndex = 27;
            this.lblGroup.Text = "Parameter Group";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.ForeColor = System.Drawing.Color.SteelBlue;
            this.lblName.Location = new System.Drawing.Point(25, 58);
            this.lblName.Margin = new System.Windows.Forms.Padding(5, 3, 5, 5);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(86, 13);
            this.lblName.TabIndex = 26;
            this.lblName.Text = "Parameter Name";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label13.Location = new System.Drawing.Point(10, 138);
            this.label13.Margin = new System.Windows.Forms.Padding(5);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(97, 13);
            this.label13.TabIndex = 25;
            this.label13.Text = "Type of Parameter:";
            // 
            // lblDUT
            // 
            this.lblDUT.AutoSize = true;
            this.lblDUT.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDUT.ForeColor = System.Drawing.Color.SteelBlue;
            this.lblDUT.Location = new System.Drawing.Point(25, 211);
            this.lblDUT.Margin = new System.Windows.Forms.Padding(5);
            this.lblDUT.Name = "lblDUT";
            this.lblDUT.Size = new System.Drawing.Size(90, 13);
            this.lblDUT.TabIndex = 22;
            this.lblDUT.Text = "Display Unit Type";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label11.Location = new System.Drawing.Point(8, 188);
            this.label11.Margin = new System.Windows.Forms.Padding(5);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(89, 13);
            this.label11.TabIndex = 21;
            this.label11.Text = "Display Unit type:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label10.Location = new System.Drawing.Point(10, 87);
            this.label10.Margin = new System.Windows.Forms.Padding(5);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(122, 13);
            this.label10.TabIndex = 17;
            this.label10.Text = "Group Parameter Under:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label8.Location = new System.Drawing.Point(14, 37);
            this.label8.Margin = new System.Windows.Forms.Padding(5);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(89, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Parameter Name:";
            // 
            // textBox8
            // 
            this.textBox8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox8.BackColor = System.Drawing.Color.LightSteelBlue;
            this.textBox8.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox8.ForeColor = System.Drawing.Color.White;
            this.textBox8.Location = new System.Drawing.Point(6, 14);
            this.textBox8.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.textBox8.Multiline = true;
            this.textBox8.Name = "textBox8";
            this.textBox8.ReadOnly = true;
            this.textBox8.Size = new System.Drawing.Size(214, 15);
            this.textBox8.TabIndex = 4;
            this.textBox8.Text = "Parameter Properties";
            this.textBox8.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.radioBttnInst);
            this.groupBox6.Controls.Add(this.radioBttnType);
            this.groupBox6.Location = new System.Drawing.Point(3, 101);
            this.groupBox6.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(224, 84);
            this.groupBox6.TabIndex = 12;
            this.groupBox6.TabStop = false;
            // 
            // radioBttnInst
            // 
            this.radioBttnInst.AutoSize = true;
            this.radioBttnInst.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.radioBttnInst.Location = new System.Drawing.Point(28, 51);
            this.radioBttnInst.Margin = new System.Windows.Forms.Padding(5);
            this.radioBttnInst.Name = "radioBttnInst";
            this.radioBttnInst.Size = new System.Drawing.Size(138, 19);
            this.radioBttnInst.TabIndex = 1;
            this.radioBttnInst.Text = "Instance Parameters";
            this.radioBttnInst.UseVisualStyleBackColor = true;
            // 
            // radioBttnType
            // 
            this.radioBttnType.AutoSize = true;
            this.radioBttnType.Checked = true;
            this.radioBttnType.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.radioBttnType.Location = new System.Drawing.Point(28, 22);
            this.radioBttnType.Margin = new System.Windows.Forms.Padding(5);
            this.radioBttnType.Name = "radioBttnType";
            this.radioBttnType.Size = new System.Drawing.Size(118, 19);
            this.radioBttnType.TabIndex = 0;
            this.radioBttnType.TabStop = true;
            this.radioBttnType.Text = "Type Parameters";
            this.radioBttnType.UseVisualStyleBackColor = true;
            this.radioBttnType.CheckedChanged += new System.EventHandler(this.radioBttnType_CheckedChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.bttnRevit);
            this.groupBox5.Controls.Add(this.bttnLock);
            this.groupBox5.Controls.Add(this.dataGridParameter);
            this.groupBox5.Controls.Add(this.textBox7);
            this.groupBox5.Controls.Add(this.label12);
            this.groupBox5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox5.ForeColor = System.Drawing.SystemColors.WindowText;
            this.groupBox5.Location = new System.Drawing.Point(0, 28);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(5, 60, 5, 5);
            this.groupBox5.Size = new System.Drawing.Size(478, 500);
            this.groupBox5.TabIndex = 11;
            this.groupBox5.TabStop = false;
            // 
            // bttnRevit
            // 
            this.bttnRevit.Image = ((System.Drawing.Image)(resources.GetObject("bttnRevit.Image")));
            this.bttnRevit.Location = new System.Drawing.Point(111, 35);
            this.bttnRevit.Name = "bttnRevit";
            this.bttnRevit.Size = new System.Drawing.Size(32, 32);
            this.bttnRevit.TabIndex = 13;
            this.bttnRevit.UseVisualStyleBackColor = true;
            this.bttnRevit.Click += new System.EventHandler(this.bttnRevit_Click);
            // 
            // bttnLock
            // 
            this.bttnLock.Image = ((System.Drawing.Image)(resources.GetObject("bttnLock.Image")));
            this.bttnLock.Location = new System.Drawing.Point(73, 34);
            this.bttnLock.Name = "bttnLock";
            this.bttnLock.Size = new System.Drawing.Size(32, 32);
            this.bttnLock.TabIndex = 12;
            this.bttnLock.UseVisualStyleBackColor = true;
            this.bttnLock.Click += new System.EventHandler(this.bttnLock_Click);
            // 
            // dataGridParameter
            // 
            this.dataGridParameter.AllowUserToAddRows = false;
            this.dataGridParameter.AllowUserToResizeRows = false;
            this.dataGridParameter.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridParameter.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dataGridParameter.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridParameter.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridParameter.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridParameter.ColumnHeadersHeight = 25;
            this.dataGridParameter.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column7,
            this.Column8,
            this.Column9,
            this.Column10});
            this.dataGridParameter.ContextMenuStrip = this.contextMenuLock;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridParameter.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridParameter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridParameter.GridColor = System.Drawing.SystemColors.ControlLight;
            this.dataGridParameter.Location = new System.Drawing.Point(5, 73);
            this.dataGridParameter.Name = "dataGridParameter";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridParameter.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridParameter.RowHeadersVisible = false;
            this.dataGridParameter.RowTemplate.Height = 25;
            this.dataGridParameter.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridParameter.Size = new System.Drawing.Size(468, 422);
            this.dataGridParameter.TabIndex = 7;
            this.dataGridParameter.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridParameter_CellClick);
            this.dataGridParameter.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridParameter_CellContentClick);
            this.dataGridParameter.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridParameter_CellMouseDown);
            this.dataGridParameter.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dataGridParameter_CellPainting);
            this.dataGridParameter.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dataGridParameter_DataError);
            // 
            // contextMenuLock
            // 
            this.contextMenuLock.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lockToolStripMenuItem,
            this.revitLockToolStripMenuItem});
            this.contextMenuLock.Name = "contextMenuLock";
            this.contextMenuLock.Size = new System.Drawing.Size(129, 48);
            // 
            // lockToolStripMenuItem
            // 
            this.lockToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("lockToolStripMenuItem.Image")));
            this.lockToolStripMenuItem.Name = "lockToolStripMenuItem";
            this.lockToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.lockToolStripMenuItem.Text = "Read Only";
            this.lockToolStripMenuItem.Click += new System.EventHandler(this.lockToolStripMenuItem_Click);
            // 
            // revitLockToolStripMenuItem
            // 
            this.revitLockToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("revitLockToolStripMenuItem.Image")));
            this.revitLockToolStripMenuItem.Name = "revitLockToolStripMenuItem";
            this.revitLockToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.revitLockToolStripMenuItem.Text = "Editable";
            this.revitLockToolStripMenuItem.Click += new System.EventHandler(this.revitLockToolStripMenuItem_Click);
            // 
            // textBox7
            // 
            this.textBox7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox7.BackColor = System.Drawing.Color.MediumAquamarine;
            this.textBox7.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox7.ForeColor = System.Drawing.Color.White;
            this.textBox7.Location = new System.Drawing.Point(5, 13);
            this.textBox7.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
            this.textBox7.Multiline = true;
            this.textBox7.Name = "textBox7";
            this.textBox7.ReadOnly = true;
            this.textBox7.Size = new System.Drawing.Size(468, 17);
            this.textBox7.TabIndex = 3;
            this.textBox7.Text = "Parameters";
            this.textBox7.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(8, 44);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(65, 13);
            this.label12.TabIndex = 8;
            this.label12.Text = "Read From: ";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label6.Location = new System.Drawing.Point(6, 8);
            this.label6.Margin = new System.Windows.Forms.Padding(5);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(228, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Step 4: Check/Uncheck parameters for viewer";
            // 
            // tabReference
            // 
            this.tabReference.BackColor = System.Drawing.Color.Transparent;
            this.tabReference.Controls.Add(this.splitContainer7);
            this.tabReference.Location = new System.Drawing.Point(4, 24);
            this.tabReference.Name = "tabReference";
            this.tabReference.Padding = new System.Windows.Forms.Padding(3);
            this.tabReference.Size = new System.Drawing.Size(976, 534);
            this.tabReference.TabIndex = 2;
            this.tabReference.Text = "     Reference Link    ";
            // 
            // splitContainer7
            // 
            this.splitContainer7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer7.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer7.Location = new System.Drawing.Point(3, 3);
            this.splitContainer7.Name = "splitContainer7";
            this.splitContainer7.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer7.Panel1
            // 
            this.splitContainer7.Panel1.Controls.Add(this.groupBox8);
            this.splitContainer7.Panel1.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            // 
            // splitContainer7.Panel2
            // 
            this.splitContainer7.Panel2.Controls.Add(this.groupBox9);
            this.splitContainer7.Panel2.Padding = new System.Windows.Forms.Padding(10, 0, 10, 10);
            this.splitContainer7.Size = new System.Drawing.Size(970, 528);
            this.splitContainer7.SplitterDistance = 92;
            this.splitContainer7.TabIndex = 17;
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.lblExtDB);
            this.groupBox8.Controls.Add(this.bttnBrowse);
            this.groupBox8.Controls.Add(this.textBox9);
            this.groupBox8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox8.Location = new System.Drawing.Point(10, 0);
            this.groupBox8.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Padding = new System.Windows.Forms.Padding(5, 0, 5, 5);
            this.groupBox8.Size = new System.Drawing.Size(950, 92);
            this.groupBox8.TabIndex = 17;
            this.groupBox8.TabStop = false;
            // 
            // lblExtDB
            // 
            this.lblExtDB.AutoSize = true;
            this.lblExtDB.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblExtDB.Location = new System.Drawing.Point(73, 53);
            this.lblExtDB.Margin = new System.Windows.Forms.Padding(3);
            this.lblExtDB.Name = "lblExtDB";
            this.lblExtDB.Size = new System.Drawing.Size(111, 15);
            this.lblExtDB.TabIndex = 8;
            this.lblExtDB.Text = "External Database:";
            // 
            // bttnBrowse
            // 
            this.bttnBrowse.Image = ((System.Drawing.Image)(resources.GetObject("bttnBrowse.Image")));
            this.bttnBrowse.Location = new System.Drawing.Point(8, 35);
            this.bttnBrowse.Name = "bttnBrowse";
            this.bttnBrowse.Size = new System.Drawing.Size(50, 50);
            this.bttnBrowse.TabIndex = 7;
            this.bttnBrowse.UseVisualStyleBackColor = true;
            this.bttnBrowse.Click += new System.EventHandler(this.bttnBrowse_Click);
            // 
            // textBox9
            // 
            this.textBox9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox9.BackColor = System.Drawing.Color.LightSteelBlue;
            this.textBox9.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox9.ForeColor = System.Drawing.Color.White;
            this.textBox9.Location = new System.Drawing.Point(8, 14);
            this.textBox9.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.textBox9.Multiline = true;
            this.textBox9.Name = "textBox9";
            this.textBox9.ReadOnly = true;
            this.textBox9.Size = new System.Drawing.Size(934, 15);
            this.textBox9.TabIndex = 4;
            this.textBox9.TabStop = false;
            this.textBox9.Text = " External Database Browser";
            // 
            // groupBox9
            // 
            this.groupBox9.Controls.Add(this.bttnAddRelationship);
            this.groupBox9.Controls.Add(this.bttnEditParam);
            this.groupBox9.Controls.Add(this.bttnDelParam);
            this.groupBox9.Controls.Add(this.dataGridExt);
            this.groupBox9.Controls.Add(this.textBox10);
            this.groupBox9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox9.Location = new System.Drawing.Point(10, 0);
            this.groupBox9.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Padding = new System.Windows.Forms.Padding(5, 60, 5, 5);
            this.groupBox9.Size = new System.Drawing.Size(950, 422);
            this.groupBox9.TabIndex = 16;
            this.groupBox9.TabStop = false;
            // 
            // bttnAddRelationship
            // 
            this.bttnAddRelationship.Image = ((System.Drawing.Image)(resources.GetObject("bttnAddRelationship.Image")));
            this.bttnAddRelationship.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnAddRelationship.Location = new System.Drawing.Point(10, 39);
            this.bttnAddRelationship.Margin = new System.Windows.Forms.Padding(5);
            this.bttnAddRelationship.Name = "bttnAddRelationship";
            this.bttnAddRelationship.Size = new System.Drawing.Size(224, 25);
            this.bttnAddRelationship.TabIndex = 13;
            this.bttnAddRelationship.Text = "Add Parameter-Field Relationship";
            this.bttnAddRelationship.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bttnAddRelationship.UseVisualStyleBackColor = true;
            this.bttnAddRelationship.Click += new System.EventHandler(this.bttnAddRelationship_Click);
            // 
            // bttnEditParam
            // 
            this.bttnEditParam.Image = ((System.Drawing.Image)(resources.GetObject("bttnEditParam.Image")));
            this.bttnEditParam.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnEditParam.Location = new System.Drawing.Point(244, 39);
            this.bttnEditParam.Margin = new System.Windows.Forms.Padding(5);
            this.bttnEditParam.Name = "bttnEditParam";
            this.bttnEditParam.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.bttnEditParam.Size = new System.Drawing.Size(75, 25);
            this.bttnEditParam.TabIndex = 11;
            this.bttnEditParam.Text = "Edit";
            this.bttnEditParam.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bttnEditParam.UseVisualStyleBackColor = true;
            this.bttnEditParam.Click += new System.EventHandler(this.bttnEditParam_Click);
            // 
            // bttnDelParam
            // 
            this.bttnDelParam.Image = ((System.Drawing.Image)(resources.GetObject("bttnDelParam.Image")));
            this.bttnDelParam.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnDelParam.Location = new System.Drawing.Point(329, 39);
            this.bttnDelParam.Margin = new System.Windows.Forms.Padding(5);
            this.bttnDelParam.Name = "bttnDelParam";
            this.bttnDelParam.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.bttnDelParam.Size = new System.Drawing.Size(85, 25);
            this.bttnDelParam.TabIndex = 10;
            this.bttnDelParam.Text = "Delete";
            this.bttnDelParam.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bttnDelParam.UseVisualStyleBackColor = true;
            this.bttnDelParam.Click += new System.EventHandler(this.bttnDelParam_Click);
            // 
            // dataGridExt
            // 
            this.dataGridExt.AllowUserToAddRows = false;
            this.dataGridExt.AllowUserToDeleteRows = false;
            this.dataGridExt.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridExt.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
            this.dataGridExt.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dataGridExt.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridExt.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridExt.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridExt.ColumnHeadersHeight = 25;
            this.dataGridExt.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4});
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridExt.DefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridExt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridExt.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnKeystroke;
            this.dataGridExt.GridColor = System.Drawing.Color.LightGray;
            this.dataGridExt.Location = new System.Drawing.Point(5, 74);
            this.dataGridExt.Name = "dataGridExt";
            this.dataGridExt.ReadOnly = true;
            this.dataGridExt.RowHeadersVisible = false;
            this.dataGridExt.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridExt.Size = new System.Drawing.Size(940, 343);
            this.dataGridExt.TabIndex = 8;
            // 
            // Column1
            // 
            this.Column1.FillWeight = 15F;
            this.Column1.HeaderText = "Category Name";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // Column2
            // 
            this.Column2.FillWeight = 15F;
            this.Column2.HeaderText = "Controlling Parameter";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            // 
            // Column3
            // 
            this.Column3.FillWeight = 30F;
            this.Column3.HeaderText = "Updating Parameters";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            // 
            // Column4
            // 
            this.Column4.FillWeight = 40F;
            this.Column4.HeaderText = "Linked Database";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            // 
            // textBox10
            // 
            this.textBox10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox10.BackColor = System.Drawing.Color.CornflowerBlue;
            this.textBox10.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox10.ForeColor = System.Drawing.Color.White;
            this.textBox10.Location = new System.Drawing.Point(8, 16);
            this.textBox10.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.textBox10.Multiline = true;
            this.textBox10.Name = "textBox10";
            this.textBox10.ReadOnly = true;
            this.textBox10.Size = new System.Drawing.Size(934, 15);
            this.textBox10.TabIndex = 4;
            this.textBox10.Text = "  Linked Parameters";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(25, 23);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(16, 16);
            this.pictureBox2.TabIndex = 29;
            this.pictureBox2.TabStop = false;
            // 
            // linkAbout
            // 
            this.linkAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkAbout.AutoSize = true;
            this.linkAbout.Location = new System.Drawing.Point(86, 26);
            this.linkAbout.Name = "linkAbout";
            this.linkAbout.Size = new System.Drawing.Size(35, 13);
            this.linkAbout.TabIndex = 12;
            this.linkAbout.TabStop = true;
            this.linkAbout.Text = "About";
            this.linkAbout.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkAbout_LinkClicked);
            // 
            // linkHelp
            // 
            this.linkHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkHelp.AutoSize = true;
            this.linkHelp.Location = new System.Drawing.Point(40, 26);
            this.linkHelp.Name = "linkHelp";
            this.linkHelp.Size = new System.Drawing.Size(29, 13);
            this.linkHelp.TabIndex = 11;
            this.linkHelp.TabStop = true;
            this.linkHelp.Text = "Help";
            this.linkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkHelp_LinkClicked);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.progressBar,
            this.statusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 56);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(984, 22);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // progressBar
            // 
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(120, 16);
            this.progressBar.Step = 1;
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(118, 17);
            this.statusLabel.Text = "toolStripStatusLabel1";
            // 
            // bttnBack
            // 
            this.bttnBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnBack.BackColor = System.Drawing.Color.Transparent;
            this.bttnBack.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.bttnBack.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnBack.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.bttnBack.Image = ((System.Drawing.Image)(resources.GetObject("bttnBack.Image")));
            this.bttnBack.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnBack.Location = new System.Drawing.Point(695, 16);
            this.bttnBack.Margin = new System.Windows.Forms.Padding(5, 5, 10, 5);
            this.bttnBack.Name = "bttnBack";
            this.bttnBack.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.bttnBack.Size = new System.Drawing.Size(84, 30);
            this.bttnBack.TabIndex = 9;
            this.bttnBack.Text = "Back";
            this.bttnBack.UseVisualStyleBackColor = false;
            this.bttnBack.Click += new System.EventHandler(this.bttnBack_Click);
            // 
            // bttnNext
            // 
            this.bttnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnNext.BackColor = System.Drawing.Color.Transparent;
            this.bttnNext.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.bttnNext.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnNext.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.bttnNext.Image = ((System.Drawing.Image)(resources.GetObject("bttnNext.Image")));
            this.bttnNext.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnNext.Location = new System.Drawing.Point(789, 16);
            this.bttnNext.Margin = new System.Windows.Forms.Padding(5, 5, 10, 5);
            this.bttnNext.Name = "bttnNext";
            this.bttnNext.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.bttnNext.Size = new System.Drawing.Size(84, 30);
            this.bttnNext.TabIndex = 8;
            this.bttnNext.Text = "Next";
            this.bttnNext.UseVisualStyleBackColor = false;
            this.bttnNext.Click += new System.EventHandler(this.bttnNext_Click);
            // 
            // bttnCancel
            // 
            this.bttnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnCancel.BackColor = System.Drawing.Color.Transparent;
            this.bttnCancel.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.bttnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnCancel.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.bttnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnCancel.Location = new System.Drawing.Point(601, 16);
            this.bttnCancel.Margin = new System.Windows.Forms.Padding(5, 5, 10, 5);
            this.bttnCancel.Name = "bttnCancel";
            this.bttnCancel.Size = new System.Drawing.Size(84, 30);
            this.bttnCancel.TabIndex = 7;
            this.bttnCancel.Text = "Cancel";
            this.bttnCancel.UseVisualStyleBackColor = false;
            this.bttnCancel.Click += new System.EventHandler(this.bttnCancel_Click);
            // 
            // bttnCreate
            // 
            this.bttnCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnCreate.BackColor = System.Drawing.Color.Transparent;
            this.bttnCreate.Enabled = false;
            this.bttnCreate.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.bttnCreate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnCreate.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.bttnCreate.Image = ((System.Drawing.Image)(resources.GetObject("bttnCreate.Image")));
            this.bttnCreate.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnCreate.Location = new System.Drawing.Point(883, 16);
            this.bttnCreate.Margin = new System.Windows.Forms.Padding(5, 5, 10, 5);
            this.bttnCreate.Name = "bttnCreate";
            this.bttnCreate.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.bttnCreate.Size = new System.Drawing.Size(84, 30);
            this.bttnCreate.TabIndex = 6;
            this.bttnCreate.Text = "Apply";
            this.bttnCreate.UseVisualStyleBackColor = false;
            this.bttnCreate.Click += new System.EventHandler(this.bttnCreate_Click);
            // 
            // imageListLock
            // 
            this.imageListLock.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListLock.ImageStream")));
            this.imageListLock.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListLock.Images.SetKeyName(0, "none.bmp");
            this.imageListLock.Images.SetKeyName(1, "lock.png");
            this.imageListLock.Images.SetKeyName(2, "editable.png");
            this.imageListLock.Images.SetKeyName(3, "readonly.png");
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            // 
            // Column7
            // 
            this.Column7.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Column7.FillWeight = 76.14214F;
            this.Column7.Frozen = true;
            this.Column7.HeaderText = "Visibility";
            this.Column7.Name = "Column7";
            this.Column7.Width = 70;
            // 
            // Column8
            // 
            this.Column8.FillWeight = 147.7157F;
            this.Column8.HeaderText = "Parameter Name";
            this.Column8.Name = "Column8";
            this.Column8.ReadOnly = true;
            // 
            // Column9
            // 
            this.Column9.FillWeight = 76.14214F;
            this.Column9.HeaderText = "Editable";
            this.Column9.Name = "Column9";
            // 
            // Column10
            // 
            this.Column10.HeaderText = "ParamId";
            this.Column10.Name = "Column10";
            this.Column10.ReadOnly = true;
            this.Column10.Visible = false;
            // 
            // form_Editor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(984, 692);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(1000, 730);
            this.Name = "form_Editor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Create New Revit Data Collection";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.form_Editor_FormClosed);
            this.Load += new System.EventHandler(this.form_Editor_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabCategory.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel1.PerformLayout();
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel1.PerformLayout();
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tabParameter.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel1.PerformLayout();
            this.splitContainer5.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
            this.splitContainer5.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridFamily)).EndInit();
            this.splitContainer6.Panel1.ResumeLayout(false);
            this.splitContainer6.Panel1.PerformLayout();
            this.splitContainer6.Panel2.ResumeLayout(false);
            this.splitContainer6.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer6)).EndInit();
            this.splitContainer6.ResumeLayout(false);
            this.groupBox10.ResumeLayout(false);
            this.groupBox10.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridParameter)).EndInit();
            this.contextMenuLock.ResumeLayout(false);
            this.tabReference.ResumeLayout(false);
            this.splitContainer7.Panel1.ResumeLayout(false);
            this.splitContainer7.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer7)).EndInit();
            this.splitContainer7.ResumeLayout(false);
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.groupBox9.ResumeLayout(false);
            this.groupBox9.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridExt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBoxDbPath;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListView listViewCategory;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ListView listViewSelection;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Button bttnChange;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button bttnAdd;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button bttnCancel;
        private System.Windows.Forms.Button bttnCreate;
        private System.Windows.Forms.ImageList imgListThumbnail;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Button bttnRemove;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Button bttnCategoryFilt;
        private System.Windows.Forms.TextBox txtBoxCategory;
        private System.Windows.Forms.Button bttnTypeFilt;
        private System.Windows.Forms.TextBox txtBoxType;
        private System.Windows.Forms.TreeView treeViewFamily;
        private System.Windows.Forms.Button bttnCheckNone;
        private System.Windows.Forms.Button bttnCheckAll;
        private System.Windows.Forms.Button bttnDetailView;
        private System.Windows.Forms.Button bttnIconView;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Button bttnSelectionFilt;
        private System.Windows.Forms.TextBox txtBoxSelection;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabCategory;
        private System.Windows.Forms.TabPage tabParameter;
        private System.Windows.Forms.Button bttnNext;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.DataGridView dataGridFamily;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox textBox7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.RadioButton radioBttnInst;
        private System.Windows.Forms.RadioButton radioBttnType;
        private System.Windows.Forms.Button bttnBack;
        private System.Windows.Forms.Button bttnSyncCat;
        private System.Windows.Forms.Button bttnSyncType;
        private System.Windows.Forms.SplitContainer splitContainer6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblDUT;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBox8;
        private System.Windows.Forms.Button bttnRevit;
        private System.Windows.Forms.Button bttnLock;
        private System.Windows.Forms.DataGridView dataGridParameter;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ImageList imageListLock;
        private System.Windows.Forms.ContextMenuStrip contextMenuLock;
        private System.Windows.Forms.ToolStripMenuItem lockToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem revitLockToolStripMenuItem;
        private System.Windows.Forms.Button bttnCancelCat;
        private System.Windows.Forms.Button bttnCancelFamily;
        private System.Windows.Forms.TabPage tabReference;
        private System.Windows.Forms.SaveFileDialog saveFileDialogDB;
        private System.Windows.Forms.SplitContainer splitContainer7;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.TextBox textBox9;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.TextBox textBox10;
        private System.Windows.Forms.Label lblExtDB;
        private System.Windows.Forms.Button bttnBrowse;
        private System.Windows.Forms.DataGridView dataGridExt;
        private System.Windows.Forms.Button bttnEditParam;
        private System.Windows.Forms.Button bttnDelParam;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Label lblParamType;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Label lblGroup;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label lblFormat;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.CheckBox checkBoxExcludeInst;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.DataGridViewTextBoxColumn CategoryColumn;
        private System.Windows.Forms.Button bttnAddRelationship;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.Button bttnExisting;
        private System.Windows.Forms.LinkLabel linkAbout;
        private System.Windows.Forms.LinkLabel linkHelp;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.DataGridViewButtonColumn Column7;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column8;
        private System.Windows.Forms.DataGridViewImageColumn Column9;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column10;
    }
}