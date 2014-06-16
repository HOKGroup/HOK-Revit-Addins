namespace HOK.AVFManager.GenericForms
{
    partial class CommandForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CommandForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.bttnInterior = new System.Windows.Forms.Button();
            this.bttnArchitecture = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.bttnUrban = new System.Windows.Forms.Button();
            this.bttnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.bttnInterior);
            this.groupBox1.Controls.Add(this.bttnArchitecture);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.bttnUrban);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.groupBox1.Location = new System.Drawing.Point(10, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(214, 177);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Category of Analysis";
            // 
            // bttnInterior
            // 
            this.bttnInterior.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnInterior.ForeColor = System.Drawing.SystemColors.ControlText;
            this.bttnInterior.Location = new System.Drawing.Point(13, 129);
            this.bttnInterior.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.bttnInterior.Name = "bttnInterior";
            this.bttnInterior.Size = new System.Drawing.Size(188, 30);
            this.bttnInterior.TabIndex = 3;
            this.bttnInterior.Text = "Interior Design";
            this.bttnInterior.UseVisualStyleBackColor = true;
            this.bttnInterior.Click += new System.EventHandler(this.bttnInterior_Click);
            // 
            // bttnArchitecture
            // 
            this.bttnArchitecture.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnArchitecture.ForeColor = System.Drawing.SystemColors.ControlText;
            this.bttnArchitecture.Location = new System.Drawing.Point(13, 89);
            this.bttnArchitecture.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.bttnArchitecture.Name = "bttnArchitecture";
            this.bttnArchitecture.Size = new System.Drawing.Size(188, 30);
            this.bttnArchitecture.TabIndex = 2;
            this.bttnArchitecture.Text = "Architecture";
            this.bttnArchitecture.UseVisualStyleBackColor = true;
            this.bttnArchitecture.Click += new System.EventHandler(this.bttnArchitecture_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.label1.Location = new System.Drawing.Point(20, 26);
            this.label1.Margin = new System.Windows.Forms.Padding(5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(143, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Define the type of the model.";
            // 
            // bttnUrban
            // 
            this.bttnUrban.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnUrban.ForeColor = System.Drawing.SystemColors.ControlText;
            this.bttnUrban.Location = new System.Drawing.Point(13, 49);
            this.bttnUrban.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.bttnUrban.Name = "bttnUrban";
            this.bttnUrban.Size = new System.Drawing.Size(188, 30);
            this.bttnUrban.TabIndex = 0;
            this.bttnUrban.Text = "Urban Planning";
            this.bttnUrban.UseVisualStyleBackColor = true;
            this.bttnUrban.Click += new System.EventHandler(this.bttnUrban_Click);
            // 
            // bttnCancel
            // 
            this.bttnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnCancel.Location = new System.Drawing.Point(146, 193);
            this.bttnCancel.Name = "bttnCancel";
            this.bttnCancel.Size = new System.Drawing.Size(75, 23);
            this.bttnCancel.TabIndex = 1;
            this.bttnCancel.Text = "Cancel";
            this.bttnCancel.UseVisualStyleBackColor = true;
            this.bttnCancel.Click += new System.EventHandler(this.bttnCancel_Click);
            // 
            // CommandForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(234, 227);
            this.Controls.Add(this.bttnCancel);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(250, 265);
            this.MinimumSize = new System.Drawing.Size(250, 265);
            this.Name = "CommandForm";
            this.Padding = new System.Windows.Forms.Padding(10, 10, 10, 40);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Model Type";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button bttnInterior;
        private System.Windows.Forms.Button bttnArchitecture;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bttnUrban;
        private System.Windows.Forms.Button bttnCancel;
    }
}