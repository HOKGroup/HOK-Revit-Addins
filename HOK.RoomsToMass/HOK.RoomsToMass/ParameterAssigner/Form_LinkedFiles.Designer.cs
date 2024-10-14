namespace HOK.RoomsToMass.ParameterAssigner
{
    partial class Form_LinkedFiles
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_LinkedFiles));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.radioButtonAll = new System.Windows.Forms.RadioButton();
            this.radioButtonLink = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.radioButtonHost = new System.Windows.Forms.RadioButton();
            this.radioButtonSelectedMass = new System.Windows.Forms.RadioButton();
            this.labelSelectedMass = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.treeViewLinkedFile = new System.Windows.Forms.TreeView();
            this.bttnApply = new System.Windows.Forms.Button();
            this.bttnCancel = new System.Windows.Forms.Button();
            this.bttnCheckAll = new System.Windows.Forms.Button();
            this.bttnCheckNone = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.splitContainer1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(10, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox1.Size = new System.Drawing.Size(334, 492);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Source File";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(5, 18);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.radioButtonAll);
            this.splitContainer1.Panel1.Controls.Add(this.radioButtonLink);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.radioButtonHost);
            this.splitContainer1.Panel1.Controls.Add(this.radioButtonSelectedMass);
            this.splitContainer1.Panel1.Controls.Add(this.labelSelectedMass);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(5);
            this.splitContainer1.Size = new System.Drawing.Size(324, 469);
            this.splitContainer1.SplitterDistance = 158;
            this.splitContainer1.TabIndex = 8;
            // 
            // radioButtonAll
            // 
            this.radioButtonAll.AutoSize = true;
            this.radioButtonAll.Enabled = false;
            this.radioButtonAll.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonAll.Location = new System.Drawing.Point(25, 67);
            this.radioButtonAll.Margin = new System.Windows.Forms.Padding(5);
            this.radioButtonAll.Name = "radioButtonAll";
            this.radioButtonAll.Size = new System.Drawing.Size(83, 19);
            this.radioButtonAll.TabIndex = 5;
            this.radioButtonAll.Text = "Display All ";
            this.radioButtonAll.UseVisualStyleBackColor = true;
            this.radioButtonAll.CheckedChanged += new System.EventHandler(this.radioButtonAll_CheckedChanged);
            // 
            // radioButtonLink
            // 
            this.radioButtonLink.AutoSize = true;
            this.radioButtonLink.Enabled = false;
            this.radioButtonLink.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonLink.Location = new System.Drawing.Point(25, 125);
            this.radioButtonLink.Margin = new System.Windows.Forms.Padding(5);
            this.radioButtonLink.Name = "radioButtonLink";
            this.radioButtonLink.Size = new System.Drawing.Size(116, 19);
            this.radioButtonLink.TabIndex = 7;
            this.radioButtonLink.Text = "Only Linked Files";
            this.radioButtonLink.UseVisualStyleBackColor = true;
            this.radioButtonLink.CheckedChanged += new System.EventHandler(this.radioButtonLink_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 5);
            this.label1.Margin = new System.Windows.Forms.Padding(5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(301, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Propagate mass element parameters from the following source:";
            // 
            // radioButtonHost
            // 
            this.radioButtonHost.AutoSize = true;
            this.radioButtonHost.Checked = true;
            this.radioButtonHost.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonHost.Location = new System.Drawing.Point(25, 96);
            this.radioButtonHost.Margin = new System.Windows.Forms.Padding(5);
            this.radioButtonHost.Name = "radioButtonHost";
            this.radioButtonHost.Size = new System.Drawing.Size(120, 19);
            this.radioButtonHost.TabIndex = 6;
            this.radioButtonHost.TabStop = true;
            this.radioButtonHost.Text = "Only Host Project";
            this.radioButtonHost.UseVisualStyleBackColor = true;
            this.radioButtonHost.CheckedChanged += new System.EventHandler(this.radioButtonHost_CheckedChanged);
            // 
            // radioButtonSelectedMass
            // 
            this.radioButtonSelectedMass.AutoSize = true;
            this.radioButtonSelectedMass.Enabled = false;
            this.radioButtonSelectedMass.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonSelectedMass.Location = new System.Drawing.Point(25, 38);
            this.radioButtonSelectedMass.Margin = new System.Windows.Forms.Padding(5);
            this.radioButtonSelectedMass.Name = "radioButtonSelectedMass";
            this.radioButtonSelectedMass.Size = new System.Drawing.Size(137, 19);
            this.radioButtonSelectedMass.TabIndex = 3;
            this.radioButtonSelectedMass.Text = "Selected Mass (host)";
            this.radioButtonSelectedMass.UseVisualStyleBackColor = true;
            this.radioButtonSelectedMass.CheckedChanged += new System.EventHandler(this.radioButtonSelectedMass_CheckedChanged);
            // 
            // labelSelectedMass
            // 
            this.labelSelectedMass.AutoSize = true;
            this.labelSelectedMass.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.labelSelectedMass.Location = new System.Drawing.Point(170, 41);
            this.labelSelectedMass.Name = "labelSelectedMass";
            this.labelSelectedMass.Size = new System.Drawing.Size(57, 13);
            this.labelSelectedMass.TabIndex = 4;
            this.labelSelectedMass.Text = "Not Found";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.bttnCheckAll);
            this.groupBox2.Controls.Add(this.bttnCheckNone);
            this.groupBox2.Controls.Add(this.treeViewLinkedFile);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(5, 5);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(5, 5, 5, 30);
            this.groupBox2.Size = new System.Drawing.Size(314, 297);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Linked Files";
            // 
            // treeViewLinkedFile
            // 
            this.treeViewLinkedFile.CheckBoxes = true;
            this.treeViewLinkedFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewLinkedFile.Location = new System.Drawing.Point(5, 18);
            this.treeViewLinkedFile.Name = "treeViewLinkedFile";
            this.treeViewLinkedFile.Size = new System.Drawing.Size(304, 249);
            this.treeViewLinkedFile.TabIndex = 3;
            this.treeViewLinkedFile.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeViewLinkedFile_AfterCheck);
            // 
            // bttnApply
            // 
            this.bttnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnApply.Location = new System.Drawing.Point(266, 508);
            this.bttnApply.Name = "bttnApply";
            this.bttnApply.Size = new System.Drawing.Size(75, 23);
            this.bttnApply.TabIndex = 1;
            this.bttnApply.Text = "Apply";
            this.bttnApply.UseVisualStyleBackColor = true;
            this.bttnApply.Click += new System.EventHandler(this.bttnApply_Click);
            // 
            // bttnCancel
            // 
            this.bttnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnCancel.Location = new System.Drawing.Point(185, 508);
            this.bttnCancel.Name = "bttnCancel";
            this.bttnCancel.Size = new System.Drawing.Size(75, 23);
            this.bttnCancel.TabIndex = 2;
            this.bttnCancel.Text = "Cancel";
            this.bttnCancel.UseVisualStyleBackColor = true;
            this.bttnCancel.Click += new System.EventHandler(this.bttnCancel_Click);
            // 
            // bttnCheckAll
            // 
            this.bttnCheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bttnCheckAll.Image = ((System.Drawing.Image)(resources.GetObject("bttnCheckAll.Image")));
            this.bttnCheckAll.Location = new System.Drawing.Point(36, 271);
            this.bttnCheckAll.Name = "bttnCheckAll";
            this.bttnCheckAll.Size = new System.Drawing.Size(23, 23);
            this.bttnCheckAll.TabIndex = 5;
            this.bttnCheckAll.UseVisualStyleBackColor = true;
            this.bttnCheckAll.Click += new System.EventHandler(this.bttnCheckAll_Click);
            // 
            // bttnCheckNone
            // 
            this.bttnCheckNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bttnCheckNone.Image = ((System.Drawing.Image)(resources.GetObject("bttnCheckNone.Image")));
            this.bttnCheckNone.Location = new System.Drawing.Point(7, 271);
            this.bttnCheckNone.Name = "bttnCheckNone";
            this.bttnCheckNone.Size = new System.Drawing.Size(23, 23);
            this.bttnCheckNone.TabIndex = 4;
            this.bttnCheckNone.UseVisualStyleBackColor = true;
            this.bttnCheckNone.Click += new System.EventHandler(this.bttnCheckNone_Click);
            // 
            // Form_LinkedFiles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(354, 542);
            this.Controls.Add(this.bttnCancel);
            this.Controls.Add(this.bttnApply);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(350, 510);
            this.Name = "Form_LinkedFiles";
            this.Padding = new System.Windows.Forms.Padding(10, 10, 10, 40);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mass Source";
            this.groupBox1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button bttnApply;
        private System.Windows.Forms.Button bttnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radioButtonLink;
        private System.Windows.Forms.RadioButton radioButtonHost;
        private System.Windows.Forms.RadioButton radioButtonAll;
        private System.Windows.Forms.Label labelSelectedMass;
        private System.Windows.Forms.RadioButton radioButtonSelectedMass;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TreeView treeViewLinkedFile;
        private System.Windows.Forms.Button bttnCheckAll;
        private System.Windows.Forms.Button bttnCheckNone;
    }
}