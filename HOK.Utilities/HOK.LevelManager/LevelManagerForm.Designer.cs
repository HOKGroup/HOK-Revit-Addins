namespace HOK.LevelManager
{
    partial class LevelManagerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LevelManagerForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxMaintain = new System.Windows.Forms.CheckBox();
            this.checkBoxDelete = new System.Windows.Forms.CheckBox();
            this.comboBoxTo = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxFrom = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.labelElement = new System.Windows.Forms.Label();
            this.labelCategory = new System.Windows.Forms.Label();
            this.labelSelElements = new System.Windows.Forms.Label();
            this.buttonCategory = new System.Windows.Forms.Button();
            this.radioButtonCategory = new System.Windows.Forms.RadioButton();
            this.radioButtonSelected = new System.Windows.Forms.RadioButton();
            this.radioButtonAll = new System.Windows.Forms.RadioButton();
            this.checkBoxRoom = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxRoom);
            this.groupBox1.Controls.Add(this.checkBoxMaintain);
            this.groupBox1.Controls.Add(this.checkBoxDelete);
            this.groupBox1.Controls.Add(this.comboBoxTo);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.comboBoxFrom);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(5, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(408, 205);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Level Rehost Options";
            // 
            // checkBoxMaintain
            // 
            this.checkBoxMaintain.AutoSize = true;
            this.checkBoxMaintain.Checked = true;
            this.checkBoxMaintain.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMaintain.Location = new System.Drawing.Point(33, 115);
            this.checkBoxMaintain.Name = "checkBoxMaintain";
            this.checkBoxMaintain.Size = new System.Drawing.Size(245, 17);
            this.checkBoxMaintain.TabIndex = 5;
            this.checkBoxMaintain.Text = "Maintain physical location by modifying offsets.";
            this.checkBoxMaintain.UseVisualStyleBackColor = true;
            // 
            // checkBoxDelete
            // 
            this.checkBoxDelete.AutoSize = true;
            this.checkBoxDelete.Location = new System.Drawing.Point(33, 140);
            this.checkBoxDelete.Name = "checkBoxDelete";
            this.checkBoxDelete.Size = new System.Drawing.Size(249, 17);
            this.checkBoxDelete.TabIndex = 4;
            this.checkBoxDelete.Text = "Delete empty levels after rehosting all elements.";
            this.checkBoxDelete.UseVisualStyleBackColor = true;
            // 
            // comboBoxTo
            // 
            this.comboBoxTo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxTo.FormattingEnabled = true;
            this.comboBoxTo.Location = new System.Drawing.Point(169, 72);
            this.comboBoxTo.Name = "comboBoxTo";
            this.comboBoxTo.Size = new System.Drawing.Size(196, 21);
            this.comboBoxTo.Sorted = true;
            this.comboBoxTo.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(21, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Element Migration To:";
            // 
            // comboBoxFrom
            // 
            this.comboBoxFrom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxFrom.FormattingEnabled = true;
            this.comboBoxFrom.Location = new System.Drawing.Point(169, 28);
            this.comboBoxFrom.Name = "comboBoxFrom";
            this.comboBoxFrom.Size = new System.Drawing.Size(196, 21);
            this.comboBoxFrom.Sorted = true;
            this.comboBoxFrom.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(21, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(147, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Element Migration From: ";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(441, 20);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 25);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(522, 20);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 25);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar,
            this.toolStripStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 270);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(609, 22);
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
            this.toolStripStatusLabel.Size = new System.Drawing.Size(51, 17);
            this.toolStripStatusLabel.Text = "Ready. . ";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.buttonOK);
            this.splitContainer1.Panel2.Controls.Add(this.buttonCancel);
            this.splitContainer1.Size = new System.Drawing.Size(609, 270);
            this.splitContainer1.SplitterDistance = 215;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 4;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.groupBox2);
            this.splitContainer2.Panel1.Padding = new System.Windows.Forms.Padding(5, 5, 0, 5);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer2.Panel2.Padding = new System.Windows.Forms.Padding(5);
            this.splitContainer2.Size = new System.Drawing.Size(609, 215);
            this.splitContainer2.SplitterDistance = 188;
            this.splitContainer2.SplitterWidth = 3;
            this.splitContainer2.TabIndex = 1;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.labelElement);
            this.groupBox2.Controls.Add(this.labelCategory);
            this.groupBox2.Controls.Add(this.labelSelElements);
            this.groupBox2.Controls.Add(this.buttonCategory);
            this.groupBox2.Controls.Add(this.radioButtonCategory);
            this.groupBox2.Controls.Add(this.radioButtonSelected);
            this.groupBox2.Controls.Add(this.radioButtonAll);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(5, 5);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(183, 205);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Element Selection";
            // 
            // labelElement
            // 
            this.labelElement.AutoSize = true;
            this.labelElement.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelElement.Location = new System.Drawing.Point(30, 186);
            this.labelElement.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelElement.Name = "labelElement";
            this.labelElement.Size = new System.Drawing.Size(0, 13);
            this.labelElement.TabIndex = 6;
            // 
            // labelCategory
            // 
            this.labelCategory.AutoSize = true;
            this.labelCategory.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelCategory.Location = new System.Drawing.Point(30, 168);
            this.labelCategory.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelCategory.Name = "labelCategory";
            this.labelCategory.Size = new System.Drawing.Size(111, 13);
            this.labelCategory.TabIndex = 5;
            this.labelCategory.Text = "0 Categories Selected";
            // 
            // labelSelElements
            // 
            this.labelSelElements.AutoSize = true;
            this.labelSelElements.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelSelElements.Location = new System.Drawing.Point(30, 91);
            this.labelSelElements.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelSelElements.Name = "labelSelElements";
            this.labelSelElements.Size = new System.Drawing.Size(104, 13);
            this.labelSelElements.TabIndex = 4;
            this.labelSelElements.Text = "0 Elements Selected";
            // 
            // buttonCategory
            // 
            this.buttonCategory.Enabled = false;
            this.buttonCategory.Location = new System.Drawing.Point(32, 137);
            this.buttonCategory.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCategory.Name = "buttonCategory";
            this.buttonCategory.Size = new System.Drawing.Size(103, 20);
            this.buttonCategory.TabIndex = 3;
            this.buttonCategory.Text = "Select Categories";
            this.buttonCategory.UseVisualStyleBackColor = true;
            this.buttonCategory.Click += new System.EventHandler(this.buttonCategory_Click);
            // 
            // radioButtonCategory
            // 
            this.radioButtonCategory.AutoSize = true;
            this.radioButtonCategory.Location = new System.Drawing.Point(18, 115);
            this.radioButtonCategory.Margin = new System.Windows.Forms.Padding(2);
            this.radioButtonCategory.Name = "radioButtonCategory";
            this.radioButtonCategory.Size = new System.Drawing.Size(120, 17);
            this.radioButtonCategory.TabIndex = 2;
            this.radioButtonCategory.Text = "Selected Categories";
            this.radioButtonCategory.UseVisualStyleBackColor = true;
            this.radioButtonCategory.CheckedChanged += new System.EventHandler(this.radioButtonCategory_CheckedChanged);
            // 
            // radioButtonSelected
            // 
            this.radioButtonSelected.AutoSize = true;
            this.radioButtonSelected.Location = new System.Drawing.Point(18, 72);
            this.radioButtonSelected.Margin = new System.Windows.Forms.Padding(2);
            this.radioButtonSelected.Name = "radioButtonSelected";
            this.radioButtonSelected.Size = new System.Drawing.Size(113, 17);
            this.radioButtonSelected.TabIndex = 1;
            this.radioButtonSelected.Text = "Selected Elements";
            this.radioButtonSelected.UseVisualStyleBackColor = true;
            this.radioButtonSelected.CheckedChanged += new System.EventHandler(this.radioButtonSelected_CheckedChanged);
            // 
            // radioButtonAll
            // 
            this.radioButtonAll.AutoSize = true;
            this.radioButtonAll.Checked = true;
            this.radioButtonAll.Location = new System.Drawing.Point(18, 31);
            this.radioButtonAll.Margin = new System.Windows.Forms.Padding(2);
            this.radioButtonAll.Name = "radioButtonAll";
            this.radioButtonAll.Size = new System.Drawing.Size(82, 17);
            this.radioButtonAll.TabIndex = 0;
            this.radioButtonAll.TabStop = true;
            this.radioButtonAll.Text = "All Elements";
            this.radioButtonAll.UseVisualStyleBackColor = true;
            // 
            // checkBoxRoom
            // 
            this.checkBoxRoom.AutoSize = true;
            this.checkBoxRoom.Location = new System.Drawing.Point(33, 164);
            this.checkBoxRoom.Name = "checkBoxRoom";
            this.checkBoxRoom.Size = new System.Drawing.Size(177, 17);
            this.checkBoxRoom.TabIndex = 6;
            this.checkBoxRoom.Text = "Include cut and paste of Rooms";
            this.checkBoxRoom.UseVisualStyleBackColor = true;
            // 
            // LevelManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(609, 292);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(625, 330);
            this.Name = "LevelManagerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Level Manager";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxMaintain;
        private System.Windows.Forms.CheckBox checkBoxDelete;
        private System.Windows.Forms.ComboBox comboBoxTo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxFrom;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonCategory;
        private System.Windows.Forms.RadioButton radioButtonCategory;
        private System.Windows.Forms.RadioButton radioButtonSelected;
        private System.Windows.Forms.RadioButton radioButtonAll;
        private System.Windows.Forms.Label labelElement;
        private System.Windows.Forms.Label labelCategory;
        private System.Windows.Forms.Label labelSelElements;
        private System.Windows.Forms.CheckBox checkBoxRoom;
    }
}