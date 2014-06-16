namespace HOK.BCFReader.GenericForms
{
    partial class TrialMakerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TrialMakerForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.lblText = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelExpried = new System.Windows.Forms.Label();
            this.labelTopic = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBoxLicense = new System.Windows.Forms.TextBox();
            this.bttnActivate = new System.Windows.Forms.Button();
            this.textBoxId3 = new System.Windows.Forms.TextBox();
            this.textBoxId2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxId1 = new System.Windows.Forms.TextBox();
            this.textBoxCompany = new System.Windows.Forms.TextBox();
            this.labelCompany = new System.Windows.Forms.Label();
            this.textBoxProduct = new System.Windows.Forms.TextBox();
            this.labelProduct = new System.Windows.Forms.Label();
            this.bttnCancel = new System.Windows.Forms.Button();
            this.bttnRun = new System.Windows.Forms.Button();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.linkEmail = new System.Windows.Forms.LinkLabel();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.lblText);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(494, 43);
            this.panel1.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(405, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "BCF Reader";
            // 
            // lblText
            // 
            this.lblText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblText.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblText.Location = new System.Drawing.Point(177, 9);
            this.lblText.Margin = new System.Windows.Forms.Padding(3);
            this.lblText.Name = "lblText";
            this.lblText.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblText.Size = new System.Drawing.Size(296, 18);
            this.lblText.TabIndex = 6;
            this.lblText.Text = "HOK Revit Add-in Tools 2013";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(10, 8);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(32, 32);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // labelExpried
            // 
            this.labelExpried.AutoEllipsis = true;
            this.labelExpried.AutoSize = true;
            this.labelExpried.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelExpried.Location = new System.Drawing.Point(7, 77);
            this.labelExpried.MaximumSize = new System.Drawing.Size(470, 50);
            this.labelExpried.Name = "labelExpried";
            this.labelExpried.Size = new System.Drawing.Size(192, 13);
            this.labelExpried.TabIndex = 14;
            this.labelExpried.Text = "The activation will be expired in ";
            this.labelExpried.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelTopic
            // 
            this.labelTopic.AutoSize = true;
            this.labelTopic.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTopic.Location = new System.Drawing.Point(7, 55);
            this.labelTopic.Name = "labelTopic";
            this.labelTopic.Size = new System.Drawing.Size(275, 13);
            this.labelTopic.TabIndex = 11;
            this.labelTopic.Text = "This copy of HOK BCF Reader is not activated.";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.pictureBox2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.textBoxId3);
            this.groupBox1.Controls.Add(this.textBoxId2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBoxId1);
            this.groupBox1.Controls.Add(this.textBoxCompany);
            this.groupBox1.Controls.Add(this.labelCompany);
            this.groupBox1.Controls.Add(this.textBoxProduct);
            this.groupBox1.Controls.Add(this.labelProduct);
            this.groupBox1.Location = new System.Drawing.Point(10, 102);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(473, 271);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Product Information";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(338, 99);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(121, 91);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 10;
            this.pictureBox2.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(315, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Please enter the following information to activate this BCF Reader";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.textBoxLicense);
            this.groupBox2.Controls.Add(this.bttnActivate);
            this.groupBox2.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.groupBox2.Location = new System.Drawing.Point(6, 196);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(461, 64);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Activation Code";
            // 
            // textBoxLicense
            // 
            this.textBoxLicense.Location = new System.Drawing.Point(21, 24);
            this.textBoxLicense.MaxLength = 29;
            this.textBoxLicense.Name = "textBoxLicense";
            this.textBoxLicense.Size = new System.Drawing.Size(310, 20);
            this.textBoxLicense.TabIndex = 6;
            // 
            // bttnActivate
            // 
            this.bttnActivate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnActivate.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bttnActivate.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnActivate.Location = new System.Drawing.Point(363, 19);
            this.bttnActivate.Name = "bttnActivate";
            this.bttnActivate.Size = new System.Drawing.Size(83, 29);
            this.bttnActivate.TabIndex = 7;
            this.bttnActivate.Text = "Activate";
            this.bttnActivate.UseVisualStyleBackColor = true;
            this.bttnActivate.Click += new System.EventHandler(this.bttnActivate_Click);
            // 
            // textBoxId3
            // 
            this.textBoxId3.Location = new System.Drawing.Point(184, 144);
            this.textBoxId3.MaxLength = 1;
            this.textBoxId3.Name = "textBoxId3";
            this.textBoxId3.Size = new System.Drawing.Size(19, 20);
            this.textBoxId3.TabIndex = 5;
            // 
            // textBoxId2
            // 
            this.textBoxId2.Location = new System.Drawing.Point(159, 144);
            this.textBoxId2.MaxLength = 1;
            this.textBoxId2.Name = "textBoxId2";
            this.textBoxId2.Size = new System.Drawing.Size(19, 20);
            this.textBoxId2.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(78, 147);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Identifier:";
            // 
            // textBoxId1
            // 
            this.textBoxId1.Location = new System.Drawing.Point(134, 144);
            this.textBoxId1.MaxLength = 1;
            this.textBoxId1.Name = "textBoxId1";
            this.textBoxId1.Size = new System.Drawing.Size(19, 20);
            this.textBoxId1.TabIndex = 3;
            // 
            // textBoxCompany
            // 
            this.textBoxCompany.Location = new System.Drawing.Point(134, 102);
            this.textBoxCompany.Name = "textBoxCompany";
            this.textBoxCompany.Size = new System.Drawing.Size(191, 20);
            this.textBoxCompany.TabIndex = 2;
            // 
            // labelCompany
            // 
            this.labelCompany.AutoSize = true;
            this.labelCompany.Location = new System.Drawing.Point(40, 105);
            this.labelCompany.Name = "labelCompany";
            this.labelCompany.Size = new System.Drawing.Size(88, 13);
            this.labelCompany.TabIndex = 0;
            this.labelCompany.Text = "Company Name: ";
            // 
            // textBoxProduct
            // 
            this.textBoxProduct.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxProduct.Location = new System.Drawing.Point(134, 61);
            this.textBoxProduct.Name = "textBoxProduct";
            this.textBoxProduct.ReadOnly = true;
            this.textBoxProduct.Size = new System.Drawing.Size(191, 20);
            this.textBoxProduct.TabIndex = 1;
            this.textBoxProduct.Text = "HOK BCF Reader";
            // 
            // labelProduct
            // 
            this.labelProduct.AutoSize = true;
            this.labelProduct.Location = new System.Drawing.Point(47, 64);
            this.labelProduct.Name = "labelProduct";
            this.labelProduct.Size = new System.Drawing.Size(81, 13);
            this.labelProduct.TabIndex = 0;
            this.labelProduct.Text = "Product Name: ";
            // 
            // bttnCancel
            // 
            this.bttnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnCancel.Location = new System.Drawing.Point(409, 383);
            this.bttnCancel.Name = "bttnCancel";
            this.bttnCancel.Size = new System.Drawing.Size(75, 25);
            this.bttnCancel.TabIndex = 9;
            this.bttnCancel.Text = "Cancel";
            this.bttnCancel.UseVisualStyleBackColor = true;
            this.bttnCancel.Click += new System.EventHandler(this.bttnCancel_Click);
            // 
            // bttnRun
            // 
            this.bttnRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnRun.Location = new System.Drawing.Point(298, 383);
            this.bttnRun.Name = "bttnRun";
            this.bttnRun.Size = new System.Drawing.Size(105, 25);
            this.bttnRun.TabIndex = 8;
            this.bttnRun.Text = "Continue to Run";
            this.bttnRun.UseVisualStyleBackColor = true;
            this.bttnRun.Click += new System.EventHandler(this.bttnRun_Click);
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox3.Image")));
            this.pictureBox3.Location = new System.Drawing.Point(10, 383);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(18, 18);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox3.TabIndex = 15;
            this.pictureBox3.TabStop = false;
            // 
            // linkEmail
            // 
            this.linkEmail.AutoSize = true;
            this.linkEmail.Location = new System.Drawing.Point(35, 387);
            this.linkEmail.Name = "linkEmail";
            this.linkEmail.Size = new System.Drawing.Size(125, 13);
            this.linkEmail.TabIndex = 16;
            this.linkEmail.TabStop = true;
            this.linkEmail.Text = "Activation Code Request";
            this.linkEmail.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkEmail_LinkClicked);
            // 
            // TrialMakerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(494, 417);
            this.Controls.Add(this.linkEmail);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.labelExpried);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.bttnRun);
            this.Controls.Add(this.labelTopic);
            this.Controls.Add(this.bttnCancel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(510, 455);
            this.MinimumSize = new System.Drawing.Size(510, 455);
            this.Name = "TrialMakerForm";
            this.Padding = new System.Windows.Forms.Padding(0, 0, 0, 40);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Activation Wizard";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblText;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button bttnCancel;
        private System.Windows.Forms.Button bttnRun;
        private System.Windows.Forms.Label labelTopic;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBoxLicense;
        private System.Windows.Forms.Button bttnActivate;
        private System.Windows.Forms.TextBox textBoxId3;
        private System.Windows.Forms.TextBox textBoxId2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxId1;
        private System.Windows.Forms.TextBox textBoxCompany;
        private System.Windows.Forms.Label labelCompany;
        private System.Windows.Forms.TextBox textBoxProduct;
        private System.Windows.Forms.Label labelProduct;
        private System.Windows.Forms.Label labelExpried;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.LinkLabel linkEmail;
    }
}