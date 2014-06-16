namespace RevitDBManager.Forms
{
    partial class form_FileSelection
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(form_FileSelection));
            this.bttnCancel = new System.Windows.Forms.Button();
            this.bttnOK = new System.Windows.Forms.Button();
            this.defaultImageList = new System.Windows.Forms.ImageList(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.bttnAddComments = new System.Windows.Forms.Button();
            this.bttnActivate = new System.Windows.Forms.Button();
            this.listViewFileItems = new System.Windows.Forms.ListView();
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripFileInfo = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.activateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.linkAbout = new System.Windows.Forms.LinkLabel();
            this.linkHelp = new System.Windows.Forms.LinkLabel();
            this.groupBox1.SuspendLayout();
            this.contextMenuStripFileInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // bttnCancel
            // 
            this.bttnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnCancel.BackColor = System.Drawing.Color.Transparent;
            this.bttnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bttnCancel.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.bttnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnCancel.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.bttnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnCancel.Location = new System.Drawing.Point(869, 257);
            this.bttnCancel.Margin = new System.Windows.Forms.Padding(5);
            this.bttnCancel.Name = "bttnCancel";
            this.bttnCancel.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.bttnCancel.Size = new System.Drawing.Size(84, 30);
            this.bttnCancel.TabIndex = 11;
            this.bttnCancel.Text = "Cancel";
            this.bttnCancel.UseVisualStyleBackColor = false;
            this.bttnCancel.Click += new System.EventHandler(this.bttnCancel_Click);
            // 
            // bttnOK
            // 
            this.bttnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnOK.BackColor = System.Drawing.Color.Transparent;
            this.bttnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bttnOK.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLightLight;
            this.bttnOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnOK.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.bttnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnOK.Location = new System.Drawing.Point(775, 257);
            this.bttnOK.Margin = new System.Windows.Forms.Padding(5);
            this.bttnOK.Name = "bttnOK";
            this.bttnOK.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.bttnOK.Size = new System.Drawing.Size(84, 30);
            this.bttnOK.TabIndex = 10;
            this.bttnOK.Text = "OK";
            this.bttnOK.UseVisualStyleBackColor = false;
            this.bttnOK.Click += new System.EventHandler(this.bttnOK_Click);
            // 
            // defaultImageList
            // 
            this.defaultImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("defaultImageList.ImageStream")));
            this.defaultImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.defaultImageList.Images.SetKeyName(0, "default.png");
            this.defaultImageList.Images.SetKeyName(1, "emblem_default.png");
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.bttnAddComments);
            this.groupBox1.Controls.Add(this.bttnActivate);
            this.groupBox1.Controls.Add(this.listViewFileItems);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(11, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(7, 25, 7, 40);
            this.groupBox1.Size = new System.Drawing.Size(944, 237);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            // 
            // bttnAddComments
            // 
            this.bttnAddComments.Image = ((System.Drawing.Image)(resources.GetObject("bttnAddComments.Image")));
            this.bttnAddComments.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnAddComments.Location = new System.Drawing.Point(85, 203);
            this.bttnAddComments.Name = "bttnAddComments";
            this.bttnAddComments.Size = new System.Drawing.Size(110, 26);
            this.bttnAddComments.TabIndex = 3;
            this.bttnAddComments.Text = "Add Comments";
            this.bttnAddComments.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bttnAddComments.UseVisualStyleBackColor = true;
            this.bttnAddComments.Click += new System.EventHandler(this.bttnAddComments_Click);
            // 
            // bttnActivate
            // 
            this.bttnActivate.Image = ((System.Drawing.Image)(resources.GetObject("bttnActivate.Image")));
            this.bttnActivate.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnActivate.Location = new System.Drawing.Point(7, 203);
            this.bttnActivate.Name = "bttnActivate";
            this.bttnActivate.Size = new System.Drawing.Size(72, 26);
            this.bttnActivate.TabIndex = 2;
            this.bttnActivate.Text = "Activate";
            this.bttnActivate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bttnActivate.UseVisualStyleBackColor = true;
            this.bttnActivate.Click += new System.EventHandler(this.bttnActivate_Click);
            // 
            // listViewFileItems
            // 
            this.listViewFileItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listViewFileItems.ContextMenuStrip = this.contextMenuStripFileInfo;
            this.listViewFileItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewFileItems.FullRowSelect = true;
            this.listViewFileItems.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewFileItems.LabelEdit = true;
            this.listViewFileItems.Location = new System.Drawing.Point(7, 38);
            this.listViewFileItems.MultiSelect = false;
            this.listViewFileItems.Name = "listViewFileItems";
            this.listViewFileItems.Size = new System.Drawing.Size(930, 159);
            this.listViewFileItems.SmallImageList = this.defaultImageList;
            this.listViewFileItems.TabIndex = 1;
            this.listViewFileItems.UseCompatibleStateImageBehavior = false;
            this.listViewFileItems.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "File Name";
            this.columnHeader5.Width = 180;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Date modified";
            this.columnHeader1.Width = 140;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Modified by";
            this.columnHeader2.Width = 100;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Path";
            this.columnHeader3.Width = 350;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Comments";
            this.columnHeader4.Width = 214;
            // 
            // contextMenuStripFileInfo
            // 
            this.contextMenuStripFileInfo.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.activateToolStripMenuItem,
            this.toolStripSeparator1,
            this.propertiesToolStripMenuItem});
            this.contextMenuStripFileInfo.Name = "contextMenuStripFileInfo";
            this.contextMenuStripFileInfo.Size = new System.Drawing.Size(128, 54);
            // 
            // activateToolStripMenuItem
            // 
            this.activateToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("activateToolStripMenuItem.Image")));
            this.activateToolStripMenuItem.Name = "activateToolStripMenuItem";
            this.activateToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.activateToolStripMenuItem.Text = "Activate";
            this.activateToolStripMenuItem.Click += new System.EventHandler(this.activateToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(124, 6);
            // 
            // propertiesToolStripMenuItem
            // 
            this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
            this.propertiesToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.propertiesToolStripMenuItem.Text = "Properties";
            this.propertiesToolStripMenuItem.Click += new System.EventHandler(this.propertiesToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 16);
            this.label1.Margin = new System.Windows.Forms.Padding(3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(176, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Please select a database file below.";
            // 
            // linkAbout
            // 
            this.linkAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkAbout.AutoSize = true;
            this.linkAbout.Location = new System.Drawing.Point(70, 267);
            this.linkAbout.Name = "linkAbout";
            this.linkAbout.Size = new System.Drawing.Size(35, 13);
            this.linkAbout.TabIndex = 14;
            this.linkAbout.TabStop = true;
            this.linkAbout.Text = "About";
            this.linkAbout.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkAbout_LinkClicked);
            // 
            // linkHelp
            // 
            this.linkHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkHelp.AutoSize = true;
            this.linkHelp.Location = new System.Drawing.Point(24, 267);
            this.linkHelp.Name = "linkHelp";
            this.linkHelp.Size = new System.Drawing.Size(29, 13);
            this.linkHelp.TabIndex = 13;
            this.linkHelp.TabStop = true;
            this.linkHelp.Text = "Help";
            this.linkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkHelp_LinkClicked);
            // 
            // form_FileSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ClientSize = new System.Drawing.Size(967, 301);
            this.Controls.Add(this.linkAbout);
            this.Controls.Add(this.linkHelp);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.bttnCancel);
            this.Controls.Add(this.bttnOK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "form_FileSelection";
            this.Text = "Alternative Data Source";
            this.Load += new System.EventHandler(this.form_FileSelection_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.contextMenuStripFileInfo.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bttnCancel;
        private System.Windows.Forms.Button bttnOK;
        private System.Windows.Forms.ImageList defaultImageList;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button bttnActivate;
        private System.Windows.Forms.ListView listViewFileItems;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Button bttnAddComments;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripFileInfo;
        private System.Windows.Forms.ToolStripMenuItem activateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.LinkLabel linkAbout;
        private System.Windows.Forms.LinkLabel linkHelp;
    }
}