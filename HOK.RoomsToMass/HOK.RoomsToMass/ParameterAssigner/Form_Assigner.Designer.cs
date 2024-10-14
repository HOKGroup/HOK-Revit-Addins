namespace HOK.RoomsToMass.ParameterAssigner
{
    partial class Form_Assigner
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Assigner));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButtonAll = new System.Windows.Forms.RadioButton();
            this.radioButtonLink = new System.Windows.Forms.RadioButton();
            this.radioButtonHost = new System.Windows.Forms.RadioButton();
            this.buttonSetting = new System.Windows.Forms.Button();
            this.checkBoxFilter = new System.Windows.Forms.CheckBox();
            this.bttnParameter = new System.Windows.Forms.Button();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.dataGridViewMass = new System.Windows.Forms.DataGridView();
            this.ColumnSelection = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColumnId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnLinked = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnView = new System.Windows.Forms.DataGridViewButtonColumn();
            this.dataGridViewParameter = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bttnFilter = new System.Windows.Forms.Button();
            this.bttnCheckAll = new System.Windows.Forms.Button();
            this.bttnCheckNone = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.bttnSplit = new System.Windows.Forms.Button();
            this.linkHelp = new System.Windows.Forms.LinkLabel();
            this.bttnAssign = new System.Windows.Forms.Button();
            this.bttnCancel = new System.Windows.Forms.Button();
            this.checkBoxEmptyParam = new System.Windows.Forms.CheckBox();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMass)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewParameter)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar,
            this.toolStripStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 610);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(844, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar
            // 
            this.toolStripProgressBar.Name = "toolStripProgressBar";
            this.toolStripProgressBar.Size = new System.Drawing.Size(200, 16);
            this.toolStripProgressBar.Step = 1;
            this.toolStripProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(39, 17);
            this.toolStripStatusLabel.Text = "Ready";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(10);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.bttnSplit);
            this.splitContainer1.Panel2.Controls.Add(this.linkHelp);
            this.splitContainer1.Panel2.Controls.Add(this.bttnAssign);
            this.splitContainer1.Panel2.Controls.Add(this.bttnCancel);
            this.splitContainer1.Size = new System.Drawing.Size(844, 610);
            this.splitContainer1.SplitterDistance = 558;
            this.splitContainer1.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxEmptyParam);
            this.groupBox1.Controls.Add(this.radioButtonAll);
            this.groupBox1.Controls.Add(this.radioButtonLink);
            this.groupBox1.Controls.Add(this.radioButtonHost);
            this.groupBox1.Controls.Add(this.buttonSetting);
            this.groupBox1.Controls.Add(this.checkBoxFilter);
            this.groupBox1.Controls.Add(this.bttnParameter);
            this.groupBox1.Controls.Add(this.splitContainer2);
            this.groupBox1.Controls.Add(this.bttnFilter);
            this.groupBox1.Controls.Add(this.bttnCheckAll);
            this.groupBox1.Controls.Add(this.bttnCheckNone);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.groupBox1.Location = new System.Drawing.Point(10, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(10, 100, 10, 45);
            this.groupBox1.Size = new System.Drawing.Size(824, 538);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Placed Mass";
            // 
            // radioButtonAll
            // 
            this.radioButtonAll.AutoSize = true;
            this.radioButtonAll.Checked = true;
            this.radioButtonAll.ForeColor = System.Drawing.SystemColors.WindowText;
            this.radioButtonAll.Location = new System.Drawing.Point(18, 48);
            this.radioButtonAll.Margin = new System.Windows.Forms.Padding(5);
            this.radioButtonAll.Name = "radioButtonAll";
            this.radioButtonAll.Size = new System.Drawing.Size(73, 17);
            this.radioButtonAll.TabIndex = 12;
            this.radioButtonAll.TabStop = true;
            this.radioButtonAll.Text = "Display All";
            this.radioButtonAll.UseVisualStyleBackColor = true;
            this.radioButtonAll.CheckedChanged += new System.EventHandler(this.radioButtonAll_CheckedChanged);
            // 
            // radioButtonLink
            // 
            this.radioButtonLink.AutoSize = true;
            this.radioButtonLink.ForeColor = System.Drawing.SystemColors.WindowText;
            this.radioButtonLink.Location = new System.Drawing.Point(97, 48);
            this.radioButtonLink.Margin = new System.Windows.Forms.Padding(5);
            this.radioButtonLink.Name = "radioButtonLink";
            this.radioButtonLink.Size = new System.Drawing.Size(105, 17);
            this.radioButtonLink.TabIndex = 13;
            this.radioButtonLink.Text = "Only Linked Files";
            this.radioButtonLink.UseVisualStyleBackColor = true;
            this.radioButtonLink.CheckedChanged += new System.EventHandler(this.radioButtonLink_CheckedChanged);
            // 
            // radioButtonHost
            // 
            this.radioButtonHost.AutoSize = true;
            this.radioButtonHost.ForeColor = System.Drawing.SystemColors.WindowText;
            this.radioButtonHost.Location = new System.Drawing.Point(208, 48);
            this.radioButtonHost.Margin = new System.Windows.Forms.Padding(5);
            this.radioButtonHost.Name = "radioButtonHost";
            this.radioButtonHost.Size = new System.Drawing.Size(107, 17);
            this.radioButtonHost.TabIndex = 14;
            this.radioButtonHost.Text = "Only Host Project";
            this.radioButtonHost.UseVisualStyleBackColor = true;
            this.radioButtonHost.CheckedChanged += new System.EventHandler(this.radioButtonHost_CheckedChanged);
            // 
            // buttonSetting
            // 
            this.buttonSetting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSetting.ForeColor = System.Drawing.SystemColors.MenuText;
            this.buttonSetting.Image = ((System.Drawing.Image)(resources.GetObject("buttonSetting.Image")));
            this.buttonSetting.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonSetting.Location = new System.Drawing.Point(733, 499);
            this.buttonSetting.Name = "buttonSetting";
            this.buttonSetting.Padding = new System.Windows.Forms.Padding(3, 0, 2, 0);
            this.buttonSetting.Size = new System.Drawing.Size(78, 26);
            this.buttonSetting.TabIndex = 16;
            this.buttonSetting.Text = "Settings";
            this.buttonSetting.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonSetting.UseVisualStyleBackColor = true;
            this.buttonSetting.Click += new System.EventHandler(this.buttonSetting_Click);
            // 
            // checkBoxFilter
            // 
            this.checkBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxFilter.AutoSize = true;
            this.checkBoxFilter.ForeColor = System.Drawing.SystemColors.WindowText;
            this.checkBoxFilter.Location = new System.Drawing.Point(123, 505);
            this.checkBoxFilter.Name = "checkBoxFilter";
            this.checkBoxFilter.Size = new System.Drawing.Size(89, 17);
            this.checkBoxFilter.TabIndex = 15;
            this.checkBoxFilter.Text = "Enable Filters";
            this.checkBoxFilter.UseVisualStyleBackColor = true;
            this.checkBoxFilter.CheckedChanged += new System.EventHandler(this.checkBoxFilter_CheckedChanged);
            // 
            // bttnParameter
            // 
            this.bttnParameter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnParameter.ForeColor = System.Drawing.SystemColors.MenuText;
            this.bttnParameter.Image = ((System.Drawing.Image)(resources.GetObject("bttnParameter.Image")));
            this.bttnParameter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnParameter.Location = new System.Drawing.Point(687, 79);
            this.bttnParameter.Name = "bttnParameter";
            this.bttnParameter.Padding = new System.Windows.Forms.Padding(3, 0, 2, 0);
            this.bttnParameter.Size = new System.Drawing.Size(124, 26);
            this.bttnParameter.TabIndex = 10;
            this.bttnParameter.Text = "Select Parameters";
            this.bttnParameter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bttnParameter.UseVisualStyleBackColor = true;
            this.bttnParameter.Click += new System.EventHandler(this.bttnParameter_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(10, 113);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.dataGridViewMass);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.dataGridViewParameter);
            this.splitContainer2.Size = new System.Drawing.Size(804, 380);
            this.splitContainer2.SplitterDistance = 594;
            this.splitContainer2.TabIndex = 9;
            // 
            // dataGridViewMass
            // 
            this.dataGridViewMass.AllowUserToAddRows = false;
            this.dataGridViewMass.AllowUserToDeleteRows = false;
            this.dataGridViewMass.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewMass.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dataGridViewMass.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridViewMass.ColumnHeadersHeight = 25;
            this.dataGridViewMass.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnSelection,
            this.ColumnId,
            this.ColumnName,
            this.ColumnLinked,
            this.ColumnView});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowFrame;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewMass.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewMass.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewMass.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewMass.MultiSelect = false;
            this.dataGridViewMass.Name = "dataGridViewMass";
            this.dataGridViewMass.RowHeadersVisible = false;
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.dataGridViewMass.RowsDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewMass.RowTemplate.Height = 24;
            this.dataGridViewMass.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewMass.Size = new System.Drawing.Size(594, 380);
            this.dataGridViewMass.TabIndex = 5;
            this.dataGridViewMass.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewMass_CellClick);
            this.dataGridViewMass.Scroll += new System.Windows.Forms.ScrollEventHandler(this.dataGridViewMass_Scroll);
            this.dataGridViewMass.SelectionChanged += new System.EventHandler(this.dataGridViewMass_SelectionChanged);
            // 
            // ColumnSelection
            // 
            this.ColumnSelection.FillWeight = 35F;
            this.ColumnSelection.HeaderText = "Selection";
            this.ColumnSelection.Name = "ColumnSelection";
            // 
            // ColumnId
            // 
            this.ColumnId.FillWeight = 50F;
            this.ColumnId.HeaderText = "Mass Id";
            this.ColumnId.Name = "ColumnId";
            this.ColumnId.ReadOnly = true;
            // 
            // ColumnName
            // 
            this.ColumnName.FillWeight = 60F;
            this.ColumnName.HeaderText = "Name";
            this.ColumnName.Name = "ColumnName";
            this.ColumnName.ReadOnly = true;
            // 
            // ColumnLinked
            // 
            this.ColumnLinked.FillWeight = 120F;
            this.ColumnLinked.HeaderText = "Linked File";
            this.ColumnLinked.Name = "ColumnLinked";
            this.ColumnLinked.ReadOnly = true;
            // 
            // ColumnView
            // 
            this.ColumnView.FillWeight = 60F;
            this.ColumnView.HeaderText = "View";
            this.ColumnView.Name = "ColumnView";
            // 
            // dataGridViewParameter
            // 
            this.dataGridViewParameter.AllowUserToAddRows = false;
            this.dataGridViewParameter.AllowUserToDeleteRows = false;
            this.dataGridViewParameter.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dataGridViewParameter.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridViewParameter.ColumnHeadersHeight = 25;
            this.dataGridViewParameter.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowFrame;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewParameter.DefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridViewParameter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewParameter.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewParameter.MultiSelect = false;
            this.dataGridViewParameter.Name = "dataGridViewParameter";
            this.dataGridViewParameter.RowHeadersVisible = false;
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            this.dataGridViewParameter.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridViewParameter.RowTemplate.Height = 24;
            this.dataGridViewParameter.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewParameter.Size = new System.Drawing.Size(206, 380);
            this.dataGridViewParameter.TabIndex = 6;
            this.dataGridViewParameter.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewParameter_CellContentClick);
            this.dataGridViewParameter.Scroll += new System.Windows.Forms.ScrollEventHandler(this.dataGridViewParameter_Scroll);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.FillWeight = 50F;
            this.dataGridViewTextBoxColumn1.HeaderText = "Parameter1";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.FillWeight = 60F;
            this.dataGridViewTextBoxColumn2.HeaderText = "Parameter2";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // bttnFilter
            // 
            this.bttnFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bttnFilter.ForeColor = System.Drawing.SystemColors.MenuText;
            this.bttnFilter.Image = ((System.Drawing.Image)(resources.GetObject("bttnFilter.Image")));
            this.bttnFilter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnFilter.Location = new System.Drawing.Point(10, 499);
            this.bttnFilter.Name = "bttnFilter";
            this.bttnFilter.Padding = new System.Windows.Forms.Padding(3, 0, 2, 0);
            this.bttnFilter.Size = new System.Drawing.Size(107, 26);
            this.bttnFilter.TabIndex = 8;
            this.bttnFilter.Text = "Element Filter";
            this.bttnFilter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bttnFilter.UseVisualStyleBackColor = true;
            this.bttnFilter.Click += new System.EventHandler(this.bttnFilter_Click);
            // 
            // bttnCheckAll
            // 
            this.bttnCheckAll.Image = ((System.Drawing.Image)(resources.GetObject("bttnCheckAll.Image")));
            this.bttnCheckAll.Location = new System.Drawing.Point(43, 81);
            this.bttnCheckAll.Name = "bttnCheckAll";
            this.bttnCheckAll.Size = new System.Drawing.Size(23, 23);
            this.bttnCheckAll.TabIndex = 3;
            this.bttnCheckAll.UseVisualStyleBackColor = true;
            this.bttnCheckAll.Click += new System.EventHandler(this.bttnCheckAll_Click);
            // 
            // bttnCheckNone
            // 
            this.bttnCheckNone.Image = ((System.Drawing.Image)(resources.GetObject("bttnCheckNone.Image")));
            this.bttnCheckNone.Location = new System.Drawing.Point(14, 81);
            this.bttnCheckNone.Name = "bttnCheckNone";
            this.bttnCheckNone.Size = new System.Drawing.Size(23, 23);
            this.bttnCheckNone.TabIndex = 2;
            this.bttnCheckNone.UseVisualStyleBackColor = true;
            this.bttnCheckNone.Click += new System.EventHandler(this.bttnCheckNone_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.label1.Location = new System.Drawing.Point(15, 25);
            this.label1.Margin = new System.Windows.Forms.Padding(5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(274, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Propagate parameters from the following mass instances.";
            // 
            // bttnSplit
            // 
            this.bttnSplit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnSplit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnSplit.Image = ((System.Drawing.Image)(resources.GetObject("bttnSplit.Image")));
            this.bttnSplit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnSplit.Location = new System.Drawing.Point(543, 8);
            this.bttnSplit.Margin = new System.Windows.Forms.Padding(5, 10, 5, 10);
            this.bttnSplit.Name = "bttnSplit";
            this.bttnSplit.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.bttnSplit.Size = new System.Drawing.Size(122, 30);
            this.bttnSplit.TabIndex = 6;
            this.bttnSplit.Text = "Split Elements";
            this.bttnSplit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bttnSplit.UseVisualStyleBackColor = true;
            this.bttnSplit.Click += new System.EventHandler(this.bttnSplit_Click);
            // 
            // linkHelp
            // 
            this.linkHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkHelp.AutoSize = true;
            this.linkHelp.Location = new System.Drawing.Point(25, 18);
            this.linkHelp.Name = "linkHelp";
            this.linkHelp.Size = new System.Drawing.Size(29, 13);
            this.linkHelp.TabIndex = 5;
            this.linkHelp.TabStop = true;
            this.linkHelp.Text = "Help";
            this.linkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkHelp_LinkClicked);
            // 
            // bttnAssign
            // 
            this.bttnAssign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnAssign.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnAssign.Image = ((System.Drawing.Image)(resources.GetObject("bttnAssign.Image")));
            this.bttnAssign.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnAssign.Location = new System.Drawing.Point(675, 8);
            this.bttnAssign.Margin = new System.Windows.Forms.Padding(5, 10, 5, 10);
            this.bttnAssign.Name = "bttnAssign";
            this.bttnAssign.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.bttnAssign.Size = new System.Drawing.Size(149, 30);
            this.bttnAssign.TabIndex = 3;
            this.bttnAssign.Text = "Assign Parameters";
            this.bttnAssign.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bttnAssign.UseVisualStyleBackColor = true;
            this.bttnAssign.Click += new System.EventHandler(this.bttnAssign_Click);
            // 
            // bttnCancel
            // 
            this.bttnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnCancel.Location = new System.Drawing.Point(458, 8);
            this.bttnCancel.Margin = new System.Windows.Forms.Padding(5, 10, 5, 10);
            this.bttnCancel.Name = "bttnCancel";
            this.bttnCancel.Size = new System.Drawing.Size(75, 30);
            this.bttnCancel.TabIndex = 4;
            this.bttnCancel.Text = "Cancel";
            this.bttnCancel.UseVisualStyleBackColor = true;
            this.bttnCancel.Click += new System.EventHandler(this.bttnCancel_Click);
            // 
            // checkBoxEmptyParam
            // 
            this.checkBoxEmptyParam.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxEmptyParam.AutoSize = true;
            this.checkBoxEmptyParam.ForeColor = System.Drawing.SystemColors.WindowText;
            this.checkBoxEmptyParam.Location = new System.Drawing.Point(214, 505);
            this.checkBoxEmptyParam.Name = "checkBoxEmptyParam";
            this.checkBoxEmptyParam.Size = new System.Drawing.Size(176, 17);
            this.checkBoxEmptyParam.TabIndex = 17;
            this.checkBoxEmptyParam.Text = "Process Empty Parameters Only";
            this.checkBoxEmptyParam.UseVisualStyleBackColor = true;
            // 
            // Form_Assigner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(844, 632);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form_Assigner";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mass Commands";
            this.Load += new System.EventHandler(this.Form_Assigner_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMass)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewParameter)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button bttnCheckAll;
        private System.Windows.Forms.Button bttnCheckNone;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bttnAssign;
        private System.Windows.Forms.Button bttnCancel;
        private System.Windows.Forms.Button bttnFilter;
        private System.Windows.Forms.LinkLabel linkHelp;
        private System.Windows.Forms.Button bttnParameter;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataGridView dataGridViewMass;
        private System.Windows.Forms.DataGridView dataGridViewParameter;
        private System.Windows.Forms.RadioButton radioButtonHost;
        private System.Windows.Forms.RadioButton radioButtonLink;
        private System.Windows.Forms.RadioButton radioButtonAll;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnSelection;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnId;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnLinked;
        private System.Windows.Forms.DataGridViewButtonColumn ColumnView;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.CheckBox checkBoxFilter;
        private System.Windows.Forms.Button bttnSplit;
        private System.Windows.Forms.Button buttonSetting;
        private System.Windows.Forms.CheckBox checkBoxEmptyParam;
    }
}