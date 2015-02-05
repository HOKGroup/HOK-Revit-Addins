namespace HOK.WorksetView
{
    partial class ViewCreatorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewCreatorForm));
            this.groupBoxWorkset = new System.Windows.Forms.GroupBox();
            this.checkedListBoxSource = new System.Windows.Forms.CheckedListBox();
            this.labelSelect = new System.Windows.Forms.Label();
            this.buttonNone = new System.Windows.Forms.Button();
            this.buttonAll = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxLevel = new System.Windows.Forms.ComboBox();
            this.radioButtonPlan = new System.Windows.Forms.RadioButton();
            this.radioButton3D = new System.Windows.Forms.RadioButton();
            this.labelViewType = new System.Windows.Forms.Label();
            this.comboBoxViewBy = new System.Windows.Forms.ComboBox();
            this.labelViewBy = new System.Windows.Forms.Label();
            this.checkBoxOverwrite = new System.Windows.Forms.CheckBox();
            this.groupBoxWorkset.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxWorkset
            // 
            this.groupBoxWorkset.Controls.Add(this.checkedListBoxSource);
            this.groupBoxWorkset.Controls.Add(this.labelSelect);
            this.groupBoxWorkset.Controls.Add(this.buttonNone);
            this.groupBoxWorkset.Controls.Add(this.buttonAll);
            this.groupBoxWorkset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxWorkset.Location = new System.Drawing.Point(10, 10);
            this.groupBoxWorkset.Name = "groupBoxWorkset";
            this.groupBoxWorkset.Padding = new System.Windows.Forms.Padding(10, 30, 10, 40);
            this.groupBoxWorkset.Size = new System.Drawing.Size(284, 332);
            this.groupBoxWorkset.TabIndex = 0;
            this.groupBoxWorkset.TabStop = false;
            this.groupBoxWorkset.Text = "Source Items";
            // 
            // checkedListBoxSource
            // 
            this.checkedListBoxSource.CheckOnClick = true;
            this.checkedListBoxSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkedListBoxSource.FormattingEnabled = true;
            this.checkedListBoxSource.Location = new System.Drawing.Point(10, 43);
            this.checkedListBoxSource.Name = "checkedListBoxSource";
            this.checkedListBoxSource.Size = new System.Drawing.Size(264, 249);
            this.checkedListBoxSource.TabIndex = 4;
            // 
            // labelSelect
            // 
            this.labelSelect.AutoSize = true;
            this.labelSelect.Location = new System.Drawing.Point(13, 24);
            this.labelSelect.Margin = new System.Windows.Forms.Padding(3);
            this.labelSelect.Name = "labelSelect";
            this.labelSelect.Size = new System.Drawing.Size(142, 13);
            this.labelSelect.TabIndex = 1;
            this.labelSelect.Text = "Select items to create views.";
            // 
            // buttonNone
            // 
            this.buttonNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonNone.Location = new System.Drawing.Point(94, 298);
            this.buttonNone.Name = "buttonNone";
            this.buttonNone.Size = new System.Drawing.Size(75, 23);
            this.buttonNone.TabIndex = 3;
            this.buttonNone.Text = "Check None";
            this.buttonNone.UseVisualStyleBackColor = true;
            this.buttonNone.Click += new System.EventHandler(this.buttonNone_Click);
            // 
            // buttonAll
            // 
            this.buttonAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAll.Location = new System.Drawing.Point(13, 298);
            this.buttonAll.Name = "buttonAll";
            this.buttonAll.Size = new System.Drawing.Size(75, 23);
            this.buttonAll.TabIndex = 2;
            this.buttonAll.Text = "Check All";
            this.buttonAll.UseVisualStyleBackColor = true;
            this.buttonAll.Click += new System.EventHandler(this.buttonAll_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(217, 32);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(120, 32);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(93, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "Create Views";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar,
            this.toolStripStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 575);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(304, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar
            // 
            this.toolStripProgressBar.Name = "toolStripProgressBar";
            this.toolStripProgressBar.Size = new System.Drawing.Size(150, 16);
            this.toolStripProgressBar.Step = 1;
            this.toolStripProgressBar.Visible = false;
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
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.checkBoxOverwrite);
            this.splitContainer1.Panel2.Controls.Add(this.buttonCancel);
            this.splitContainer1.Panel2.Controls.Add(this.buttonOK);
            this.splitContainer1.Size = new System.Drawing.Size(304, 575);
            this.splitContainer1.SplitterDistance = 501;
            this.splitContainer1.TabIndex = 4;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.groupBox1);
            this.splitContainer2.Panel1.Padding = new System.Windows.Forms.Padding(10, 10, 10, 5);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.groupBoxWorkset);
            this.splitContainer2.Panel2.Padding = new System.Windows.Forms.Padding(10, 10, 10, 5);
            this.splitContainer2.Size = new System.Drawing.Size(304, 501);
            this.splitContainer2.SplitterDistance = 150;
            this.splitContainer2.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.comboBoxLevel);
            this.groupBox1.Controls.Add(this.radioButtonPlan);
            this.groupBox1.Controls.Add(this.radioButton3D);
            this.groupBox1.Controls.Add(this.labelViewType);
            this.groupBox1.Controls.Add(this.comboBoxViewBy);
            this.groupBox1.Controls.Add(this.labelViewBy);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(10, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(284, 135);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "View Source";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(53, 100);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Level: ";
            // 
            // comboBoxLevel
            // 
            this.comboBoxLevel.Enabled = false;
            this.comboBoxLevel.FormattingEnabled = true;
            this.comboBoxLevel.Location = new System.Drawing.Point(98, 97);
            this.comboBoxLevel.Name = "comboBoxLevel";
            this.comboBoxLevel.Size = new System.Drawing.Size(161, 21);
            this.comboBoxLevel.TabIndex = 5;
            // 
            // radioButtonPlan
            // 
            this.radioButtonPlan.AutoSize = true;
            this.radioButtonPlan.Location = new System.Drawing.Point(98, 77);
            this.radioButtonPlan.Name = "radioButtonPlan";
            this.radioButtonPlan.Size = new System.Drawing.Size(150, 17);
            this.radioButtonPlan.TabIndex = 4;
            this.radioButtonPlan.Text = "Plan Views (Select a level)";
            this.radioButtonPlan.UseVisualStyleBackColor = true;
            this.radioButtonPlan.CheckedChanged += new System.EventHandler(this.radioButtonPlan_CheckedChanged);
            // 
            // radioButton3D
            // 
            this.radioButton3D.AutoSize = true;
            this.radioButton3D.Checked = true;
            this.radioButton3D.Location = new System.Drawing.Point(98, 55);
            this.radioButton3D.Name = "radioButton3D";
            this.radioButton3D.Size = new System.Drawing.Size(70, 17);
            this.radioButton3D.TabIndex = 3;
            this.radioButton3D.TabStop = true;
            this.radioButton3D.Text = "3D Views";
            this.radioButton3D.UseVisualStyleBackColor = true;
            this.radioButton3D.CheckedChanged += new System.EventHandler(this.radioButton3D_CheckedChanged);
            // 
            // labelViewType
            // 
            this.labelViewType.AutoSize = true;
            this.labelViewType.Location = new System.Drawing.Point(29, 57);
            this.labelViewType.Name = "labelViewType";
            this.labelViewType.Size = new System.Drawing.Size(63, 13);
            this.labelViewType.TabIndex = 2;
            this.labelViewType.Text = "View Type: ";
            // 
            // comboBoxViewBy
            // 
            this.comboBoxViewBy.FormattingEnabled = true;
            this.comboBoxViewBy.Location = new System.Drawing.Point(98, 23);
            this.comboBoxViewBy.Name = "comboBoxViewBy";
            this.comboBoxViewBy.Size = new System.Drawing.Size(157, 21);
            this.comboBoxViewBy.TabIndex = 1;
            this.comboBoxViewBy.SelectedIndexChanged += new System.EventHandler(this.comboBoxViewBy_SelectedIndexChanged);
            // 
            // labelViewBy
            // 
            this.labelViewBy.AutoSize = true;
            this.labelViewBy.Location = new System.Drawing.Point(10, 26);
            this.labelViewBy.Name = "labelViewBy";
            this.labelViewBy.Size = new System.Drawing.Size(82, 13);
            this.labelViewBy.TabIndex = 0;
            this.labelViewBy.Text = "Create View By:";
            // 
            // checkBoxOverwrite
            // 
            this.checkBoxOverwrite.AutoSize = true;
            this.checkBoxOverwrite.Location = new System.Drawing.Point(17, 3);
            this.checkBoxOverwrite.Name = "checkBoxOverwrite";
            this.checkBoxOverwrite.Size = new System.Drawing.Size(139, 17);
            this.checkBoxOverwrite.TabIndex = 3;
            this.checkBoxOverwrite.Text = "Overwrite existing views";
            this.checkBoxOverwrite.UseVisualStyleBackColor = true;
            // 
            // ViewCreatorForm
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(304, 597);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(320, 600);
            this.Name = "ViewCreatorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "View Creator";
            this.groupBoxWorkset.ResumeLayout(false);
            this.groupBoxWorkset.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxWorkset;
        private System.Windows.Forms.Label labelSelect;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.CheckBox checkBoxOverwrite;
        private System.Windows.Forms.Button buttonNone;
        private System.Windows.Forms.Button buttonAll;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButtonPlan;
        private System.Windows.Forms.RadioButton radioButton3D;
        private System.Windows.Forms.Label labelViewType;
        private System.Windows.Forms.ComboBox comboBoxViewBy;
        private System.Windows.Forms.Label labelViewBy;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxLevel;
        private System.Windows.Forms.CheckedListBox checkedListBoxSource;
    }
}