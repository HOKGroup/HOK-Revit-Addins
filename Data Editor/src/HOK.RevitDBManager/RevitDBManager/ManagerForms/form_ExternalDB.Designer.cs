namespace RevitDBManager.ManagerForms
{
    partial class form_ExternalDB
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(form_ExternalDB));
            this.imageListProject = new System.Windows.Forms.ImageList(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.listViewRevit = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.textBoxCategory = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.listViewExtDatabase = new System.Windows.Forms.ListView();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.comboBoxTable = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.bttnAddControl = new System.Windows.Forms.Button();
            this.bttnAddUpdate = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.textBoxField = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxParameter = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.listViewUpdate = new System.Windows.Forms.ListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.bttnDelete = new System.Windows.Forms.Button();
            this.bttnMoveUp = new System.Windows.Forms.Button();
            this.bttnMoveDown = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.bttnOK = new System.Windows.Forms.Button();
            this.bttnCancel = new System.Windows.Forms.Button();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.lblExtDB = new System.Windows.Forms.Label();
            this.textBox9 = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageListProject
            // 
            this.imageListProject.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListProject.ImageStream")));
            this.imageListProject.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListProject.Images.SetKeyName(0, "default.png");
            this.imageListProject.Images.SetKeyName(1, "services.png");
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.pictureBox1);
            this.groupBox1.Controls.Add(this.listViewRevit);
            this.groupBox1.Controls.Add(this.textBoxCategory);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBox2);
            this.groupBox1.Location = new System.Drawing.Point(14, 88);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(5, 0, 5, 5);
            this.groupBox1.Size = new System.Drawing.Size(220, 424);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label3.Location = new System.Drawing.Point(64, 401);
            this.label3.Margin = new System.Windows.Forms.Padding(5, 5, 0, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(118, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Project Parameters with";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(188, 398);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(18, 18);
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            // 
            // listViewRevit
            // 
            this.listViewRevit.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listViewRevit.FullRowSelect = true;
            this.listViewRevit.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewRevit.LargeImageList = this.imageListProject;
            this.listViewRevit.Location = new System.Drawing.Point(13, 62);
            this.listViewRevit.MultiSelect = false;
            this.listViewRevit.Name = "listViewRevit";
            this.listViewRevit.Size = new System.Drawing.Size(199, 330);
            this.listViewRevit.SmallImageList = this.imageListProject;
            this.listViewRevit.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewRevit.TabIndex = 7;
            this.listViewRevit.UseCompatibleStateImageBehavior = false;
            this.listViewRevit.View = System.Windows.Forms.View.Details;
            this.listViewRevit.SelectedIndexChanged += new System.EventHandler(this.listViewRevit_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Parameter Name";
            this.columnHeader1.Width = 192;
            // 
            // textBoxCategory
            // 
            this.textBoxCategory.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxCategory.Location = new System.Drawing.Point(70, 36);
            this.textBoxCategory.Name = "textBoxCategory";
            this.textBoxCategory.ReadOnly = true;
            this.textBoxCategory.Size = new System.Drawing.Size(142, 20);
            this.textBoxCategory.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 37);
            this.label1.Margin = new System.Windows.Forms.Padding(5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Category:";
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox2.BackColor = System.Drawing.Color.CornflowerBlue;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox2.ForeColor = System.Drawing.Color.White;
            this.textBox2.Location = new System.Drawing.Point(8, 14);
            this.textBox2.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(204, 15);
            this.textBox2.TabIndex = 4;
            this.textBox2.TabStop = false;
            this.textBox2.Text = "Revit Parameters";
            this.textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.listViewExtDatabase);
            this.groupBox2.Controls.Add(this.comboBoxTable);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.textBox1);
            this.groupBox2.Location = new System.Drawing.Point(244, 88);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(5, 0, 5, 5);
            this.groupBox2.Size = new System.Drawing.Size(220, 424);
            this.groupBox2.TabIndex = 20;
            this.groupBox2.TabStop = false;
            // 
            // listViewExtDatabase
            // 
            this.listViewExtDatabase.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3});
            this.listViewExtDatabase.FullRowSelect = true;
            this.listViewExtDatabase.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewExtDatabase.Location = new System.Drawing.Point(8, 63);
            this.listViewExtDatabase.MultiSelect = false;
            this.listViewExtDatabase.Name = "listViewExtDatabase";
            this.listViewExtDatabase.Size = new System.Drawing.Size(204, 329);
            this.listViewExtDatabase.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewExtDatabase.TabIndex = 8;
            this.listViewExtDatabase.UseCompatibleStateImageBehavior = false;
            this.listViewExtDatabase.View = System.Windows.Forms.View.Details;
            this.listViewExtDatabase.SelectedIndexChanged += new System.EventHandler(this.listViewExtDatabase_SelectedIndexChanged);
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Field Names";
            this.columnHeader3.Width = 199;
            // 
            // comboBoxTable
            // 
            this.comboBoxTable.FormattingEnabled = true;
            this.comboBoxTable.Location = new System.Drawing.Point(79, 36);
            this.comboBoxTable.Name = "comboBoxTable";
            this.comboBoxTable.Size = new System.Drawing.Size(133, 21);
            this.comboBoxTable.TabIndex = 7;
            this.comboBoxTable.SelectedIndexChanged += new System.EventHandler(this.comboBoxTable_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 37);
            this.label2.Margin = new System.Windows.Forms.Padding(5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Table Name: ";
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.BackColor = System.Drawing.Color.CornflowerBlue;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.ForeColor = System.Drawing.Color.White;
            this.textBox1.Location = new System.Drawing.Point(8, 14);
            this.textBox1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(204, 15);
            this.textBox1.TabIndex = 4;
            this.textBox1.TabStop = false;
            this.textBox1.Text = "External Database Fields";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // bttnAddControl
            // 
            this.bttnAddControl.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.bttnAddControl.FlatAppearance.BorderSize = 0;
            this.bttnAddControl.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bttnAddControl.Image = ((System.Drawing.Image)(resources.GetObject("bttnAddControl.Image")));
            this.bttnAddControl.Location = new System.Drawing.Point(475, 136);
            this.bttnAddControl.Name = "bttnAddControl";
            this.bttnAddControl.Size = new System.Drawing.Size(40, 40);
            this.bttnAddControl.TabIndex = 21;
            this.bttnAddControl.UseVisualStyleBackColor = true;
            this.bttnAddControl.Click += new System.EventHandler(this.bttnAddControl_Click);
            // 
            // bttnAddUpdate
            // 
            this.bttnAddUpdate.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.bttnAddUpdate.FlatAppearance.BorderSize = 0;
            this.bttnAddUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bttnAddUpdate.Image = ((System.Drawing.Image)(resources.GetObject("bttnAddUpdate.Image")));
            this.bttnAddUpdate.Location = new System.Drawing.Point(475, 273);
            this.bttnAddUpdate.Name = "bttnAddUpdate";
            this.bttnAddUpdate.Size = new System.Drawing.Size(40, 40);
            this.bttnAddUpdate.TabIndex = 22;
            this.bttnAddUpdate.UseVisualStyleBackColor = true;
            this.bttnAddUpdate.Click += new System.EventHandler(this.bttnAddUpdate_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.textBoxField);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.textBoxParameter);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.textBox4);
            this.groupBox3.Location = new System.Drawing.Point(523, 88);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(5, 0, 5, 5);
            this.groupBox3.Size = new System.Drawing.Size(313, 103);
            this.groupBox3.TabIndex = 23;
            this.groupBox3.TabStop = false;
            // 
            // textBoxField
            // 
            this.textBoxField.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxField.Location = new System.Drawing.Point(163, 62);
            this.textBoxField.Name = "textBoxField";
            this.textBoxField.ReadOnly = true;
            this.textBoxField.Size = new System.Drawing.Size(137, 20);
            this.textBoxField.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(160, 39);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(81, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Database Field:";
            // 
            // textBoxParameter
            // 
            this.textBoxParameter.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxParameter.Location = new System.Drawing.Point(20, 62);
            this.textBoxParameter.Name = "textBoxParameter";
            this.textBoxParameter.ReadOnly = true;
            this.textBoxParameter.Size = new System.Drawing.Size(137, 20);
            this.textBoxParameter.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Revit Parameter:";
            // 
            // textBox4
            // 
            this.textBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox4.BackColor = System.Drawing.Color.MediumAquamarine;
            this.textBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox4.ForeColor = System.Drawing.Color.White;
            this.textBox4.Location = new System.Drawing.Point(8, 14);
            this.textBox4.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.textBox4.Multiline = true;
            this.textBox4.Name = "textBox4";
            this.textBox4.ReadOnly = true;
            this.textBox4.Size = new System.Drawing.Size(297, 15);
            this.textBox4.TabIndex = 4;
            this.textBox4.TabStop = false;
            this.textBox4.Text = "Controlling Parameter and Field";
            this.textBox4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.listViewUpdate);
            this.groupBox4.Controls.Add(this.bttnDelete);
            this.groupBox4.Controls.Add(this.bttnMoveUp);
            this.groupBox4.Controls.Add(this.bttnMoveDown);
            this.groupBox4.Controls.Add(this.textBox3);
            this.groupBox4.Location = new System.Drawing.Point(523, 201);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(5, 0, 5, 5);
            this.groupBox4.Size = new System.Drawing.Size(313, 318);
            this.groupBox4.TabIndex = 24;
            this.groupBox4.TabStop = false;
            // 
            // listViewUpdate
            // 
            this.listViewUpdate.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2,
            this.columnHeader4});
            this.listViewUpdate.FullRowSelect = true;
            this.listViewUpdate.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewUpdate.Location = new System.Drawing.Point(8, 35);
            this.listViewUpdate.MultiSelect = false;
            this.listViewUpdate.Name = "listViewUpdate";
            this.listViewUpdate.Size = new System.Drawing.Size(297, 244);
            this.listViewUpdate.TabIndex = 13;
            this.listViewUpdate.UseCompatibleStateImageBehavior = false;
            this.listViewUpdate.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Parameter Name";
            this.columnHeader2.Width = 145;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Field Name";
            this.columnHeader4.Width = 148;
            // 
            // bttnDelete
            // 
            this.bttnDelete.Image = ((System.Drawing.Image)(resources.GetObject("bttnDelete.Image")));
            this.bttnDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnDelete.Location = new System.Drawing.Point(230, 287);
            this.bttnDelete.Name = "bttnDelete";
            this.bttnDelete.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.bttnDelete.Size = new System.Drawing.Size(75, 23);
            this.bttnDelete.TabIndex = 12;
            this.bttnDelete.Text = "Delete";
            this.bttnDelete.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bttnDelete.UseVisualStyleBackColor = true;
            this.bttnDelete.Click += new System.EventHandler(this.bttnDelete_Click);
            // 
            // bttnMoveUp
            // 
            this.bttnMoveUp.Image = ((System.Drawing.Image)(resources.GetObject("bttnMoveUp.Image")));
            this.bttnMoveUp.Location = new System.Drawing.Point(10, 287);
            this.bttnMoveUp.Name = "bttnMoveUp";
            this.bttnMoveUp.Size = new System.Drawing.Size(30, 23);
            this.bttnMoveUp.TabIndex = 11;
            this.bttnMoveUp.UseVisualStyleBackColor = true;
            this.bttnMoveUp.Click += new System.EventHandler(this.bttnMoveUp_Click);
            // 
            // bttnMoveDown
            // 
            this.bttnMoveDown.Image = ((System.Drawing.Image)(resources.GetObject("bttnMoveDown.Image")));
            this.bttnMoveDown.Location = new System.Drawing.Point(46, 287);
            this.bttnMoveDown.Name = "bttnMoveDown";
            this.bttnMoveDown.Size = new System.Drawing.Size(30, 23);
            this.bttnMoveDown.TabIndex = 10;
            this.bttnMoveDown.UseVisualStyleBackColor = true;
            this.bttnMoveDown.Click += new System.EventHandler(this.bttnMoveDown_Click);
            // 
            // textBox3
            // 
            this.textBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox3.BackColor = System.Drawing.Color.MediumAquamarine;
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox3.ForeColor = System.Drawing.Color.White;
            this.textBox3.Location = new System.Drawing.Point(8, 14);
            this.textBox3.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.Size = new System.Drawing.Size(297, 15);
            this.textBox3.TabIndex = 4;
            this.textBox3.TabStop = false;
            this.textBox3.Text = "Updating Parameters and Fields ";
            this.textBox3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // bttnOK
            // 
            this.bttnOK.Location = new System.Drawing.Point(753, 529);
            this.bttnOK.Margin = new System.Windows.Forms.Padding(5);
            this.bttnOK.Name = "bttnOK";
            this.bttnOK.Size = new System.Drawing.Size(75, 27);
            this.bttnOK.TabIndex = 25;
            this.bttnOK.Text = "OK";
            this.bttnOK.UseVisualStyleBackColor = true;
            this.bttnOK.Click += new System.EventHandler(this.bttnOK_Click);
            // 
            // bttnCancel
            // 
            this.bttnCancel.Location = new System.Drawing.Point(668, 529);
            this.bttnCancel.Margin = new System.Windows.Forms.Padding(5);
            this.bttnCancel.Name = "bttnCancel";
            this.bttnCancel.Size = new System.Drawing.Size(75, 27);
            this.bttnCancel.TabIndex = 26;
            this.bttnCancel.Text = "Cancel";
            this.bttnCancel.UseVisualStyleBackColor = true;
            this.bttnCancel.Click += new System.EventHandler(this.bttnCancel_Click);
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.lblExtDB);
            this.groupBox8.Controls.Add(this.textBox9);
            this.groupBox8.Location = new System.Drawing.Point(14, 8);
            this.groupBox8.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Padding = new System.Windows.Forms.Padding(5, 0, 5, 5);
            this.groupBox8.Size = new System.Drawing.Size(822, 70);
            this.groupBox8.TabIndex = 27;
            this.groupBox8.TabStop = false;
            // 
            // lblExtDB
            // 
            this.lblExtDB.AutoSize = true;
            this.lblExtDB.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblExtDB.Location = new System.Drawing.Point(10, 41);
            this.lblExtDB.Margin = new System.Windows.Forms.Padding(5);
            this.lblExtDB.Name = "lblExtDB";
            this.lblExtDB.Size = new System.Drawing.Size(97, 13);
            this.lblExtDB.TabIndex = 8;
            this.lblExtDB.Text = "External Database:";
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
            this.textBox9.Size = new System.Drawing.Size(806, 15);
            this.textBox9.TabIndex = 4;
            this.textBox9.TabStop = false;
            this.textBox9.Text = " External Database Browser";
            // 
            // form_ExternalDB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(849, 577);
            this.Controls.Add(this.groupBox8);
            this.Controls.Add(this.bttnCancel);
            this.Controls.Add(this.bttnOK);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.bttnAddUpdate);
            this.Controls.Add(this.bttnAddControl);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "form_ExternalDB";
            this.Text = "Linked External Database";
            this.Load += new System.EventHandler(this.form_ExternalDB_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imageListProject;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button bttnAddControl;
        private System.Windows.Forms.Button bttnAddUpdate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ListView listViewRevit;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.TextBox textBoxCategory;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView listViewExtDatabase;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ComboBox comboBoxTable;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Button bttnOK;
        private System.Windows.Forms.Button bttnCancel;
        private System.Windows.Forms.Button bttnDelete;
        private System.Windows.Forms.Button bttnMoveUp;
        private System.Windows.Forms.Button bttnMoveDown;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.Label lblExtDB;
        private System.Windows.Forms.TextBox textBox9;
        private System.Windows.Forms.TextBox textBoxField;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxParameter;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListView listViewUpdate;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader4;
    }
}