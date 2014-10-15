namespace HOK.AddIn_Installer_Internal
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelStatus = new System.Windows.Forms.Label();
            this.lblText = new System.Windows.Forms.Label();
            this.pictureBoxIcon = new System.Windows.Forms.PictureBox();
            this.groupBoxTools = new System.Windows.Forms.GroupBox();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.comboBoxTarget = new System.Windows.Forms.ComboBox();
            this.checkBoxBeta = new System.Windows.Forms.CheckBox();
            this.buttonAll = new System.Windows.Forms.Button();
            this.imageListCheck = new System.Windows.Forms.ImageList(this.components);
            this.listViewTools = new System.Windows.Forms.ListView();
            this.imageListIcons = new System.Windows.Forms.ImageList(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.listViewBeta = new System.Windows.Forms.ListView();
            this.buttonUninstall = new System.Windows.Forms.Button();
            this.buttonInstall = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.linkLabelHelp = new System.Windows.Forms.LinkLabel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.imageListDynamo = new System.Windows.Forms.ImageList(this.components);
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).BeginInit();
            this.groupBoxTools.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.panel1.Controls.Add(this.labelStatus);
            this.panel1.Controls.Add(this.lblText);
            this.panel1.Controls.Add(this.pictureBoxIcon);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(599, 63);
            this.panel1.TabIndex = 12;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(16, 33);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(68, 13);
            this.labelStatus.TabIndex = 7;
            this.labelStatus.Text = "File Manager";
            // 
            // lblText
            // 
            this.lblText.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblText.Location = new System.Drawing.Point(16, 12);
            this.lblText.Margin = new System.Windows.Forms.Padding(3);
            this.lblText.Name = "lblText";
            this.lblText.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblText.Size = new System.Drawing.Size(191, 18);
            this.lblText.TabIndex = 6;
            this.lblText.Text = "HOK Revit Add-Ins";
            // 
            // pictureBoxIcon
            // 
            this.pictureBoxIcon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxIcon.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxIcon.Image")));
            this.pictureBoxIcon.Location = new System.Drawing.Point(542, 12);
            this.pictureBoxIcon.Name = "pictureBoxIcon";
            this.pictureBoxIcon.Size = new System.Drawing.Size(45, 45);
            this.pictureBoxIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxIcon.TabIndex = 0;
            this.pictureBoxIcon.TabStop = false;
            // 
            // groupBoxTools
            // 
            this.groupBoxTools.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxTools.Controls.Add(this.splitContainerMain);
            this.groupBoxTools.Location = new System.Drawing.Point(19, 76);
            this.groupBoxTools.Margin = new System.Windows.Forms.Padding(10);
            this.groupBoxTools.Name = "groupBoxTools";
            this.groupBoxTools.Size = new System.Drawing.Size(561, 518);
            this.groupBoxTools.TabIndex = 13;
            this.groupBoxTools.TabStop = false;
            this.groupBoxTools.Text = "Revit Add-Ins";
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerMain.Location = new System.Drawing.Point(3, 16);
            this.splitContainerMain.Name = "splitContainerMain";
            this.splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.comboBoxTarget);
            this.splitContainerMain.Panel1.Controls.Add(this.checkBoxBeta);
            this.splitContainerMain.Panel1.Controls.Add(this.buttonAll);
            this.splitContainerMain.Panel1.Controls.Add(this.listViewTools);
            this.splitContainerMain.Panel1.Controls.Add(this.label2);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.groupBox1);
            this.splitContainerMain.Panel2.Padding = new System.Windows.Forms.Padding(5);
            this.splitContainerMain.Panel2Collapsed = true;
            this.splitContainerMain.Size = new System.Drawing.Size(555, 499);
            this.splitContainerMain.SplitterDistance = 454;
            this.splitContainerMain.TabIndex = 5;
            // 
            // comboBoxTarget
            // 
            this.comboBoxTarget.FormattingEnabled = true;
            this.comboBoxTarget.Location = new System.Drawing.Point(419, 21);
            this.comboBoxTarget.Name = "comboBoxTarget";
            this.comboBoxTarget.Size = new System.Drawing.Size(106, 21);
            this.comboBoxTarget.TabIndex = 6;
            this.comboBoxTarget.SelectedIndexChanged += new System.EventHandler(this.comboBoxTarget_SelectedIndexChanged);
            // 
            // checkBoxBeta
            // 
            this.checkBoxBeta.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxBeta.AutoSize = true;
            this.checkBoxBeta.Location = new System.Drawing.Point(21, 479);
            this.checkBoxBeta.Name = "checkBoxBeta";
            this.checkBoxBeta.Size = new System.Drawing.Size(113, 17);
            this.checkBoxBeta.TabIndex = 5;
            this.checkBoxBeta.Text = "Include BETA files";
            this.checkBoxBeta.UseVisualStyleBackColor = true;
            this.checkBoxBeta.CheckedChanged += new System.EventHandler(this.checkBoxBeta_CheckedChanged);
            // 
            // buttonAll
            // 
            this.buttonAll.ImageIndex = 0;
            this.buttonAll.ImageList = this.imageListCheck;
            this.buttonAll.Location = new System.Drawing.Point(15, 19);
            this.buttonAll.Name = "buttonAll";
            this.buttonAll.Size = new System.Drawing.Size(26, 26);
            this.buttonAll.TabIndex = 4;
            this.buttonAll.UseVisualStyleBackColor = true;
            this.buttonAll.Click += new System.EventHandler(this.buttonAll_Click);
            // 
            // imageListCheck
            // 
            this.imageListCheck.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListCheck.ImageStream")));
            this.imageListCheck.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListCheck.Images.SetKeyName(0, "checkbox.png");
            this.imageListCheck.Images.SetKeyName(1, "none.png");
            // 
            // listViewTools
            // 
            this.listViewTools.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewTools.CheckBoxes = true;
            this.listViewTools.FullRowSelect = true;
            this.listViewTools.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewTools.LargeImageList = this.imageListIcons;
            this.listViewTools.Location = new System.Drawing.Point(15, 52);
            this.listViewTools.Margin = new System.Windows.Forms.Padding(7, 7, 7, 5);
            this.listViewTools.Name = "listViewTools";
            this.listViewTools.Size = new System.Drawing.Size(521, 419);
            this.listViewTools.SmallImageList = this.imageListIcons;
            this.listViewTools.TabIndex = 3;
            this.listViewTools.UseCompatibleStateImageBehavior = false;
            this.listViewTools.View = System.Windows.Forms.View.Details;
            this.listViewTools.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewTools_ItemChecked);
            // 
            // imageListIcons
            // 
            this.imageListIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListIcons.ImageStream")));
            this.imageListIcons.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListIcons.Images.SetKeyName(0, "element.ico");
            this.imageListIcons.Images.SetKeyName(1, "parameter.ico");
            this.imageListIcons.Images.SetKeyName(2, "sheet.ico");
            this.imageListIcons.Images.SetKeyName(3, "comment.ico");
            this.imageListIcons.Images.SetKeyName(4, "cube.png");
            this.imageListIcons.Images.SetKeyName(5, "editor.ico");
            this.imageListIcons.Images.SetKeyName(6, "chart.ico");
            this.imageListIcons.Images.SetKeyName(7, "height.png");
            this.imageListIcons.Images.SetKeyName(8, "copy.png");
            this.imageListIcons.Images.SetKeyName(9, "color32.png");
            this.imageListIcons.Images.SetKeyName(10, "walker.png");
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(314, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Target Software :";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.listViewBeta);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(5, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(7);
            this.groupBox1.Size = new System.Drawing.Size(140, 36);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Beta Files";
            // 
            // listViewBeta
            // 
            this.listViewBeta.CheckBoxes = true;
            this.listViewBeta.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewBeta.FullRowSelect = true;
            this.listViewBeta.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewBeta.LargeImageList = this.imageListIcons;
            this.listViewBeta.Location = new System.Drawing.Point(7, 20);
            this.listViewBeta.Margin = new System.Windows.Forms.Padding(7, 7, 7, 5);
            this.listViewBeta.Name = "listViewBeta";
            this.listViewBeta.Size = new System.Drawing.Size(126, 9);
            this.listViewBeta.SmallImageList = this.imageListIcons;
            this.listViewBeta.TabIndex = 4;
            this.listViewBeta.UseCompatibleStateImageBehavior = false;
            this.listViewBeta.View = System.Windows.Forms.View.Details;
            this.listViewBeta.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewBeta_ItemChecked);
            // 
            // buttonUninstall
            // 
            this.buttonUninstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUninstall.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonUninstall.Image = ((System.Drawing.Image)(resources.GetObject("buttonUninstall.Image")));
            this.buttonUninstall.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonUninstall.Location = new System.Drawing.Point(400, 607);
            this.buttonUninstall.Name = "buttonUninstall";
            this.buttonUninstall.Padding = new System.Windows.Forms.Padding(6);
            this.buttonUninstall.Size = new System.Drawing.Size(93, 33);
            this.buttonUninstall.TabIndex = 15;
            this.buttonUninstall.Text = "Uninstall";
            this.buttonUninstall.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonUninstall.UseVisualStyleBackColor = true;
            this.buttonUninstall.Click += new System.EventHandler(this.buttonUninstall_Click);
            // 
            // buttonInstall
            // 
            this.buttonInstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInstall.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonInstall.Image = ((System.Drawing.Image)(resources.GetObject("buttonInstall.Image")));
            this.buttonInstall.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonInstall.Location = new System.Drawing.Point(260, 607);
            this.buttonInstall.Name = "buttonInstall";
            this.buttonInstall.Padding = new System.Windows.Forms.Padding(5);
            this.buttonInstall.Size = new System.Drawing.Size(134, 33);
            this.buttonInstall.TabIndex = 14;
            this.buttonInstall.Text = "Install / Update";
            this.buttonInstall.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonInstall.UseVisualStyleBackColor = true;
            this.buttonInstall.Click += new System.EventHandler(this.buttonInstall_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancel.Location = new System.Drawing.Point(499, 607);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Padding = new System.Windows.Forms.Padding(6);
            this.buttonCancel.Size = new System.Drawing.Size(81, 33);
            this.buttonCancel.TabIndex = 16;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // linkLabelHelp
            // 
            this.linkLabelHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelHelp.AutoSize = true;
            this.linkLabelHelp.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.linkLabelHelp.Location = new System.Drawing.Point(40, 615);
            this.linkLabelHelp.Margin = new System.Windows.Forms.Padding(0);
            this.linkLabelHelp.Name = "linkLabelHelp";
            this.linkLabelHelp.Padding = new System.Windows.Forms.Padding(3);
            this.linkLabelHelp.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.linkLabelHelp.Size = new System.Drawing.Size(78, 19);
            this.linkLabelHelp.TabIndex = 18;
            this.linkLabelHelp.TabStop = true;
            this.linkLabelHelp.Text = "eMail Support";
            this.linkLabelHelp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.linkLabelHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelHelp_LinkClicked);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(21, 618);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(18, 18);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 19;
            this.pictureBox2.TabStop = false;
            // 
            // imageListDynamo
            // 
            this.imageListDynamo.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListDynamo.ImageStream")));
            this.imageListDynamo.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListDynamo.Images.SetKeyName(0, "dynamo16.png");
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(599, 652);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.linkLabelHelp);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonUninstall);
            this.Controls.Add(this.buttonInstall);
            this.Controls.Add(this.groupBoxTools);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(615, 690);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "HOK Revit Add-Ins Installer";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).EndInit();
            this.groupBoxTools.ResumeLayout(false);
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel1.PerformLayout();
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label lblText;
        private System.Windows.Forms.PictureBox pictureBoxIcon;
        private System.Windows.Forms.GroupBox groupBoxTools;
        private System.Windows.Forms.Button buttonUninstall;
        private System.Windows.Forms.Button buttonInstall;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.LinkLabel linkLabelHelp;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.ImageList imageListIcons;
        private System.Windows.Forms.ImageList imageListCheck;
        private System.Windows.Forms.CheckBox checkBoxBeta;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.Button buttonAll;
        private System.Windows.Forms.ListView listViewTools;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListView listViewBeta;
        private System.Windows.Forms.ComboBox comboBoxTarget;
        private System.Windows.Forms.ImageList imageListDynamo;
    }
}

