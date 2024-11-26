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
            groupBox1 = new System.Windows.Forms.GroupBox();
            checkBoxRoom = new System.Windows.Forms.CheckBox();
            checkBoxMaintain = new System.Windows.Forms.CheckBox();
            checkBoxDelete = new System.Windows.Forms.CheckBox();
            comboBoxTo = new System.Windows.Forms.ComboBox();
            label2 = new System.Windows.Forms.Label();
            comboBoxFrom = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            buttonOK = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            splitContainer2 = new System.Windows.Forms.SplitContainer();
            groupBox2 = new System.Windows.Forms.GroupBox();
            labelElement = new System.Windows.Forms.Label();
            labelCategory = new System.Windows.Forms.Label();
            labelSelElements = new System.Windows.Forms.Label();
            buttonCategory = new System.Windows.Forms.Button();
            radioButtonCategory = new System.Windows.Forms.RadioButton();
            radioButtonSelected = new System.Windows.Forms.RadioButton();
            radioButtonAll = new System.Windows.Forms.RadioButton();
            groupBox1.SuspendLayout();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(checkBoxRoom);
            groupBox1.Controls.Add(checkBoxMaintain);
            groupBox1.Controls.Add(checkBoxDelete);
            groupBox1.Controls.Add(comboBoxTo);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(comboBoxFrom);
            groupBox1.Controls.Add(label1);
            groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBox1.Location = new System.Drawing.Point(12, 14);
            groupBox1.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(8, 9, 8, 9);
            groupBox1.Size = new System.Drawing.Size(1020, 601);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Level Rehost Options";
            // 
            // checkBoxRoom
            // 
            checkBoxRoom.AutoSize = true;
            checkBoxRoom.Location = new System.Drawing.Point(82, 467);
            checkBoxRoom.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            checkBoxRoom.Name = "checkBoxRoom";
            checkBoxRoom.Size = new System.Drawing.Size(420, 41);
            checkBoxRoom.TabIndex = 6;
            checkBoxRoom.Text = "Include cut and paste of Rooms";
            checkBoxRoom.UseVisualStyleBackColor = true;
            // 
            // checkBoxMaintain
            // 
            checkBoxMaintain.AutoSize = true;
            checkBoxMaintain.Checked = true;
            checkBoxMaintain.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxMaintain.Location = new System.Drawing.Point(82, 327);
            checkBoxMaintain.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            checkBoxMaintain.Name = "checkBoxMaintain";
            checkBoxMaintain.Size = new System.Drawing.Size(613, 41);
            checkBoxMaintain.TabIndex = 5;
            checkBoxMaintain.Text = "Maintain physical location by modifying offsets.";
            checkBoxMaintain.UseVisualStyleBackColor = true;
            // 
            // checkBoxDelete
            // 
            checkBoxDelete.AutoSize = true;
            checkBoxDelete.Location = new System.Drawing.Point(82, 398);
            checkBoxDelete.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            checkBoxDelete.Name = "checkBoxDelete";
            checkBoxDelete.Size = new System.Drawing.Size(616, 41);
            checkBoxDelete.TabIndex = 4;
            checkBoxDelete.Text = "Delete empty levels after rehosting all elements.";
            checkBoxDelete.UseVisualStyleBackColor = true;
            // 
            // comboBoxTo
            // 
            comboBoxTo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            comboBoxTo.FormattingEnabled = true;
            comboBoxTo.Location = new System.Drawing.Point(422, 205);
            comboBoxTo.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            comboBoxTo.Name = "comboBoxTo";
            comboBoxTo.Size = new System.Drawing.Size(484, 45);
            comboBoxTo.Sorted = true;
            comboBoxTo.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            label2.Location = new System.Drawing.Point(52, 211);
            label2.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(282, 29);
            label2.TabIndex = 2;
            label2.Text = "Element Migration To:";
            // 
            // comboBoxFrom
            // 
            comboBoxFrom.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            comboBoxFrom.FormattingEnabled = true;
            comboBoxFrom.Location = new System.Drawing.Point(422, 80);
            comboBoxFrom.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            comboBoxFrom.Name = "comboBoxFrom";
            comboBoxFrom.Size = new System.Drawing.Size(484, 45);
            comboBoxFrom.Sorted = true;
            comboBoxFrom.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            label1.Location = new System.Drawing.Point(52, 88);
            label1.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(322, 29);
            label1.TabIndex = 0;
            label1.Text = "Element Migration From: ";
            // 
            // buttonOK
            // 
            buttonOK.Location = new System.Drawing.Point(1102, 60);
            buttonOK.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(188, 71);
            buttonOK.TabIndex = 1;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += buttonOK_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new System.Drawing.Point(1305, 60);
            buttonCancel.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(188, 71);
            buttonCancel.TabIndex = 2;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new System.Drawing.Size(36, 36);
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripProgressBar, toolStripStatusLabel });
            statusStrip1.Location = new System.Drawing.Point(0, 783);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 35, 0);
            statusStrip1.Size = new System.Drawing.Size(1522, 48);
            statusStrip1.TabIndex = 3;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar
            // 
            toolStripProgressBar.Name = "toolStripProgressBar";
            toolStripProgressBar.Size = new System.Drawing.Size(375, 34);
            toolStripProgressBar.Step = 1;
            toolStripProgressBar.Visible = false;
            // 
            // toolStripStatusLabel
            // 
            toolStripStatusLabel.Name = "toolStripStatusLabel";
            toolStripStatusLabel.Size = new System.Drawing.Size(115, 37);
            toolStripStatusLabel.Text = "Ready. . ";
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(buttonOK);
            splitContainer1.Panel2.Controls.Add(buttonCancel);
            splitContainer1.Size = new System.Drawing.Size(1522, 783);
            splitContainer1.SplitterDistance = 629;
            splitContainer1.SplitterWidth = 9;
            splitContainer1.TabIndex = 4;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitContainer2.Location = new System.Drawing.Point(0, 0);
            splitContainer2.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(groupBox2);
            splitContainer2.Panel1.Padding = new System.Windows.Forms.Padding(12, 14, 0, 14);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(groupBox1);
            splitContainer2.Panel2.Padding = new System.Windows.Forms.Padding(12, 14, 12, 14);
            splitContainer2.Size = new System.Drawing.Size(1522, 629);
            splitContainer2.SplitterDistance = 470;
            splitContainer2.SplitterWidth = 8;
            splitContainer2.TabIndex = 1;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(labelElement);
            groupBox2.Controls.Add(labelCategory);
            groupBox2.Controls.Add(labelSelElements);
            groupBox2.Controls.Add(buttonCategory);
            groupBox2.Controls.Add(radioButtonCategory);
            groupBox2.Controls.Add(radioButtonSelected);
            groupBox2.Controls.Add(radioButtonAll);
            groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBox2.Location = new System.Drawing.Point(12, 14);
            groupBox2.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(5, 6, 5, 6);
            groupBox2.Size = new System.Drawing.Size(458, 601);
            groupBox2.TabIndex = 0;
            groupBox2.TabStop = false;
            groupBox2.Text = "Element Selection";
            // 
            // labelElement
            // 
            labelElement.AutoSize = true;
            labelElement.ForeColor = System.Drawing.SystemColors.GrayText;
            labelElement.Location = new System.Drawing.Point(75, 529);
            labelElement.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            labelElement.Name = "labelElement";
            labelElement.Size = new System.Drawing.Size(0, 37);
            labelElement.TabIndex = 6;
            // 
            // labelCategory
            // 
            labelCategory.AutoSize = true;
            labelCategory.ForeColor = System.Drawing.SystemColors.GrayText;
            labelCategory.Location = new System.Drawing.Point(75, 478);
            labelCategory.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            labelCategory.Name = "labelCategory";
            labelCategory.Size = new System.Drawing.Size(273, 37);
            labelCategory.TabIndex = 5;
            labelCategory.Text = "0 Categories Selected";
            // 
            // labelSelElements
            // 
            labelSelElements.AutoSize = true;
            labelSelElements.ForeColor = System.Drawing.SystemColors.GrayText;
            labelSelElements.Location = new System.Drawing.Point(75, 259);
            labelSelElements.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            labelSelElements.Name = "labelSelElements";
            labelSelElements.Size = new System.Drawing.Size(253, 37);
            labelSelElements.TabIndex = 4;
            labelSelElements.Text = "0 Elements Selected";
            // 
            // buttonCategory
            // 
            buttonCategory.Enabled = false;
            buttonCategory.Location = new System.Drawing.Point(80, 390);
            buttonCategory.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            buttonCategory.Name = "buttonCategory";
            buttonCategory.Size = new System.Drawing.Size(258, 57);
            buttonCategory.TabIndex = 3;
            buttonCategory.Text = "Select Categories";
            buttonCategory.UseVisualStyleBackColor = true;
            buttonCategory.Click += buttonCategory_Click;
            // 
            // radioButtonCategory
            // 
            radioButtonCategory.AutoSize = true;
            radioButtonCategory.Location = new System.Drawing.Point(45, 327);
            radioButtonCategory.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            radioButtonCategory.Name = "radioButtonCategory";
            radioButtonCategory.Size = new System.Drawing.Size(282, 41);
            radioButtonCategory.TabIndex = 2;
            radioButtonCategory.Text = "Selected Categories";
            radioButtonCategory.UseVisualStyleBackColor = true;
            radioButtonCategory.CheckedChanged += radioButtonCategory_CheckedChanged;
            // 
            // radioButtonSelected
            // 
            radioButtonSelected.AutoSize = true;
            radioButtonSelected.Location = new System.Drawing.Point(45, 205);
            radioButtonSelected.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            radioButtonSelected.Name = "radioButtonSelected";
            radioButtonSelected.Size = new System.Drawing.Size(262, 41);
            radioButtonSelected.TabIndex = 1;
            radioButtonSelected.Text = "Selected Elements";
            radioButtonSelected.UseVisualStyleBackColor = true;
            radioButtonSelected.CheckedChanged += radioButtonSelected_CheckedChanged;
            // 
            // radioButtonAll
            // 
            radioButtonAll.AutoSize = true;
            radioButtonAll.Checked = true;
            radioButtonAll.Location = new System.Drawing.Point(45, 88);
            radioButtonAll.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            radioButtonAll.Name = "radioButtonAll";
            radioButtonAll.Size = new System.Drawing.Size(193, 41);
            radioButtonAll.TabIndex = 0;
            radioButtonAll.TabStop = true;
            radioButtonAll.Text = "All Elements";
            radioButtonAll.UseVisualStyleBackColor = true;
            // 
            // LevelManagerForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(15F, 37F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1522, 831);
            Controls.Add(splitContainer1);
            Controls.Add(statusStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            MaximumSize = new System.Drawing.Size(1550, 910);
            MinimumSize = new System.Drawing.Size(1520, 793);
            Name = "LevelManagerForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Level Manager";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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