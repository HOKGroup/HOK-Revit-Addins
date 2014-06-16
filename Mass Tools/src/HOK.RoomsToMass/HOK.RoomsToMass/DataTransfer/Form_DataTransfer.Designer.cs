namespace HOK.RoomsToMass.DataTransfer
{
    partial class Form_DataTransfer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_DataTransfer));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButtonFloor = new System.Windows.Forms.RadioButton();
            this.radioButtonArea = new System.Windows.Forms.RadioButton();
            this.radioButtonRoom = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.bttnToRoom = new System.Windows.Forms.Button();
            this.bttnToMass = new System.Windows.Forms.Button();
            this.bttnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.radioButtonFloor);
            this.groupBox1.Controls.Add(this.radioButtonArea);
            this.groupBox1.Controls.Add(this.radioButtonRoom);
            this.groupBox1.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(336, 64);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Select a Category:";
            // 
            // radioButtonFloor
            // 
            this.radioButtonFloor.AutoSize = true;
            this.radioButtonFloor.ForeColor = System.Drawing.SystemColors.WindowText;
            this.radioButtonFloor.Location = new System.Drawing.Point(246, 30);
            this.radioButtonFloor.Name = "radioButtonFloor";
            this.radioButtonFloor.Size = new System.Drawing.Size(53, 17);
            this.radioButtonFloor.TabIndex = 2;
            this.radioButtonFloor.TabStop = true;
            this.radioButtonFloor.Text = "Floors";
            this.radioButtonFloor.UseVisualStyleBackColor = true;
            // 
            // radioButtonArea
            // 
            this.radioButtonArea.AutoSize = true;
            this.radioButtonArea.ForeColor = System.Drawing.SystemColors.WindowText;
            this.radioButtonArea.Location = new System.Drawing.Point(132, 30);
            this.radioButtonArea.Name = "radioButtonArea";
            this.radioButtonArea.Size = new System.Drawing.Size(52, 17);
            this.radioButtonArea.TabIndex = 1;
            this.radioButtonArea.TabStop = true;
            this.radioButtonArea.Text = "Areas";
            this.radioButtonArea.UseVisualStyleBackColor = true;
            // 
            // radioButtonRoom
            // 
            this.radioButtonRoom.AutoSize = true;
            this.radioButtonRoom.Checked = true;
            this.radioButtonRoom.ForeColor = System.Drawing.SystemColors.WindowText;
            this.radioButtonRoom.Location = new System.Drawing.Point(20, 30);
            this.radioButtonRoom.Name = "radioButtonRoom";
            this.radioButtonRoom.Size = new System.Drawing.Size(58, 17);
            this.radioButtonRoom.TabIndex = 0;
            this.radioButtonRoom.TabStop = true;
            this.radioButtonRoom.Text = "Rooms";
            this.radioButtonRoom.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.bttnToRoom);
            this.groupBox2.Controls.Add(this.bttnToMass);
            this.groupBox2.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.groupBox2.Location = new System.Drawing.Point(12, 82);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(336, 140);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Commands";
            // 
            // bttnToRoom
            // 
            this.bttnToRoom.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bttnToRoom.Image = ((System.Drawing.Image)(resources.GetObject("bttnToRoom.Image")));
            this.bttnToRoom.Location = new System.Drawing.Point(37, 83);
            this.bttnToRoom.Margin = new System.Windows.Forms.Padding(5);
            this.bttnToRoom.Name = "bttnToRoom";
            this.bttnToRoom.Size = new System.Drawing.Size(262, 30);
            this.bttnToRoom.TabIndex = 1;
            this.bttnToRoom.Text = "            Masses                      Rooms/Areas/Floors";
            this.bttnToRoom.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnToRoom.UseVisualStyleBackColor = true;
            this.bttnToRoom.Click += new System.EventHandler(this.bttnToRoom_Click);
            // 
            // bttnToMass
            // 
            this.bttnToMass.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bttnToMass.Image = ((System.Drawing.Image)(resources.GetObject("bttnToMass.Image")));
            this.bttnToMass.Location = new System.Drawing.Point(37, 33);
            this.bttnToMass.Margin = new System.Windows.Forms.Padding(5);
            this.bttnToMass.Name = "bttnToMass";
            this.bttnToMass.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.bttnToMass.Size = new System.Drawing.Size(262, 30);
            this.bttnToMass.TabIndex = 0;
            this.bttnToMass.Text = "Rooms/Areas/Floors              Masses";
            this.bttnToMass.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnToMass.UseVisualStyleBackColor = true;
            this.bttnToMass.Click += new System.EventHandler(this.bttnToMass_Click);
            // 
            // bttnCancel
            // 
            this.bttnCancel.Location = new System.Drawing.Point(271, 232);
            this.bttnCancel.Margin = new System.Windows.Forms.Padding(5);
            this.bttnCancel.Name = "bttnCancel";
            this.bttnCancel.Size = new System.Drawing.Size(75, 23);
            this.bttnCancel.TabIndex = 2;
            this.bttnCancel.Text = "Cancel";
            this.bttnCancel.UseVisualStyleBackColor = true;
            this.bttnCancel.Click += new System.EventHandler(this.bttnCancel_Click);
            // 
            // Form_DataTransfer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(360, 269);
            this.Controls.Add(this.bttnCancel);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form_DataTransfer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Transfer Data";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButtonFloor;
        private System.Windows.Forms.RadioButton radioButtonArea;
        private System.Windows.Forms.RadioButton radioButtonRoom;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button bttnToRoom;
        private System.Windows.Forms.Button bttnToMass;
        private System.Windows.Forms.Button bttnCancel;
    }
}