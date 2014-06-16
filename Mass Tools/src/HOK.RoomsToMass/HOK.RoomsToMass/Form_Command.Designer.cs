namespace HOK.RoomsToMass
{
    partial class Form_Command
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Command));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxFloors = new System.Windows.Forms.CheckBox();
            this.checkBoxAreas = new System.Windows.Forms.CheckBox();
            this.checkBoxRooms = new System.Windows.Forms.CheckBox();
            this.bttnFloor = new System.Windows.Forms.Button();
            this.bttnArea = new System.Windows.Forms.Button();
            this.bttnRoom = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.bttnOK = new System.Windows.Forms.Button();
            this.bttnCancel = new System.Windows.Forms.Button();
            this.linkHelp = new System.Windows.Forms.LinkLabel();
            this.linkAbout = new System.Windows.Forms.LinkLabel();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxFloors);
            this.groupBox1.Controls.Add(this.checkBoxAreas);
            this.groupBox1.Controls.Add(this.checkBoxRooms);
            this.groupBox1.Controls.Add(this.bttnFloor);
            this.groupBox1.Controls.Add(this.bttnArea);
            this.groupBox1.Controls.Add(this.bttnRoom);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.groupBox1.Location = new System.Drawing.Point(10, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(339, 212);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Geometry Source";
            // 
            // checkBoxFloors
            // 
            this.checkBoxFloors.AutoSize = true;
            this.checkBoxFloors.ForeColor = System.Drawing.SystemColors.WindowText;
            this.checkBoxFloors.Location = new System.Drawing.Point(221, 166);
            this.checkBoxFloors.Name = "checkBoxFloors";
            this.checkBoxFloors.Size = new System.Drawing.Size(99, 17);
            this.checkBoxFloors.TabIndex = 6;
            this.checkBoxFloors.Text = "Selected Floors";
            this.checkBoxFloors.UseVisualStyleBackColor = true;
            // 
            // checkBoxAreas
            // 
            this.checkBoxAreas.AutoSize = true;
            this.checkBoxAreas.ForeColor = System.Drawing.SystemColors.WindowText;
            this.checkBoxAreas.Location = new System.Drawing.Point(221, 114);
            this.checkBoxAreas.Name = "checkBoxAreas";
            this.checkBoxAreas.Size = new System.Drawing.Size(98, 17);
            this.checkBoxAreas.TabIndex = 5;
            this.checkBoxAreas.Text = "Selected Areas";
            this.checkBoxAreas.UseVisualStyleBackColor = true;
            // 
            // checkBoxRooms
            // 
            this.checkBoxRooms.AutoSize = true;
            this.checkBoxRooms.ForeColor = System.Drawing.SystemColors.WindowText;
            this.checkBoxRooms.Location = new System.Drawing.Point(221, 65);
            this.checkBoxRooms.Name = "checkBoxRooms";
            this.checkBoxRooms.Size = new System.Drawing.Size(104, 17);
            this.checkBoxRooms.TabIndex = 4;
            this.checkBoxRooms.Text = "Selected Rooms";
            this.checkBoxRooms.UseVisualStyleBackColor = true;
            // 
            // bttnFloor
            // 
            this.bttnFloor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnFloor.ForeColor = System.Drawing.SystemColors.ControlText;
            this.bttnFloor.Location = new System.Drawing.Point(16, 160);
            this.bttnFloor.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.bttnFloor.Name = "bttnFloor";
            this.bttnFloor.Size = new System.Drawing.Size(182, 25);
            this.bttnFloor.TabIndex = 3;
            this.bttnFloor.Text = "Floors";
            this.bttnFloor.UseVisualStyleBackColor = true;
            this.bttnFloor.Click += new System.EventHandler(this.bttnFloor_Click);
            // 
            // bttnArea
            // 
            this.bttnArea.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnArea.ForeColor = System.Drawing.SystemColors.ControlText;
            this.bttnArea.Location = new System.Drawing.Point(16, 108);
            this.bttnArea.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.bttnArea.Name = "bttnArea";
            this.bttnArea.Size = new System.Drawing.Size(182, 25);
            this.bttnArea.TabIndex = 2;
            this.bttnArea.Text = "Areas";
            this.bttnArea.UseVisualStyleBackColor = true;
            this.bttnArea.Click += new System.EventHandler(this.bttnArea_Click);
            // 
            // bttnRoom
            // 
            this.bttnRoom.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnRoom.ForeColor = System.Drawing.SystemColors.ControlText;
            this.bttnRoom.Location = new System.Drawing.Point(16, 59);
            this.bttnRoom.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.bttnRoom.Name = "bttnRoom";
            this.bttnRoom.Size = new System.Drawing.Size(182, 25);
            this.bttnRoom.TabIndex = 1;
            this.bttnRoom.Text = "Rooms";
            this.bttnRoom.UseVisualStyleBackColor = true;
            this.bttnRoom.Click += new System.EventHandler(this.bttnRoom_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label1.Location = new System.Drawing.Point(13, 26);
            this.label1.Margin = new System.Windows.Forms.Padding(10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Create 3D Mass from:";
            // 
            // bttnOK
            // 
            this.bttnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnOK.Location = new System.Drawing.Point(271, 228);
            this.bttnOK.Name = "bttnOK";
            this.bttnOK.Size = new System.Drawing.Size(75, 23);
            this.bttnOK.TabIndex = 1;
            this.bttnOK.Text = "OK";
            this.bttnOK.UseVisualStyleBackColor = true;
            // 
            // bttnCancel
            // 
            this.bttnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnCancel.Location = new System.Drawing.Point(190, 228);
            this.bttnCancel.Name = "bttnCancel";
            this.bttnCancel.Size = new System.Drawing.Size(75, 23);
            this.bttnCancel.TabIndex = 2;
            this.bttnCancel.Text = "Cancel";
            this.bttnCancel.UseVisualStyleBackColor = true;
            this.bttnCancel.Click += new System.EventHandler(this.bttnCancel_Click);
            // 
            // linkHelp
            // 
            this.linkHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkHelp.AutoSize = true;
            this.linkHelp.Location = new System.Drawing.Point(23, 233);
            this.linkHelp.Name = "linkHelp";
            this.linkHelp.Size = new System.Drawing.Size(29, 13);
            this.linkHelp.TabIndex = 3;
            this.linkHelp.TabStop = true;
            this.linkHelp.Text = "Help";
            this.linkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkHelp_LinkClicked);
            // 
            // linkAbout
            // 
            this.linkAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkAbout.AutoSize = true;
            this.linkAbout.Location = new System.Drawing.Point(69, 233);
            this.linkAbout.Name = "linkAbout";
            this.linkAbout.Size = new System.Drawing.Size(35, 13);
            this.linkAbout.TabIndex = 4;
            this.linkAbout.TabStop = true;
            this.linkAbout.Text = "About";
            this.linkAbout.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkAbout_LinkClicked);
            // 
            // Form_Command
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(359, 262);
            this.Controls.Add(this.linkAbout);
            this.Controls.Add(this.linkHelp);
            this.Controls.Add(this.bttnCancel);
            this.Controls.Add(this.bttnOK);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(375, 300);
            this.MinimumSize = new System.Drawing.Size(375, 300);
            this.Name = "Form_Command";
            this.Padding = new System.Windows.Forms.Padding(10, 10, 10, 40);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Create Extruded Mass";
            this.Load += new System.EventHandler(this.Form_Command_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button bttnOK;
        private System.Windows.Forms.Button bttnCancel;
        private System.Windows.Forms.Button bttnFloor;
        private System.Windows.Forms.Button bttnArea;
        private System.Windows.Forms.Button bttnRoom;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxFloors;
        private System.Windows.Forms.CheckBox checkBoxAreas;
        private System.Windows.Forms.CheckBox checkBoxRooms;
        private System.Windows.Forms.LinkLabel linkHelp;
        private System.Windows.Forms.LinkLabel linkAbout;
    }
}