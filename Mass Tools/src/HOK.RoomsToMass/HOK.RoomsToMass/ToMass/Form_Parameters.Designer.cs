namespace HOK.RoomsToMass.ToMass
{
    partial class Form_Parameters
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Parameters));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.bttnDelete = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.listViewMassParameter = new System.Windows.Forms.ListView();
            this.columnMassParameter = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.bttnAdd = new System.Windows.Forms.Button();
            this.labelParameter = new System.Windows.Forms.Label();
            this.listViewMainParameter = new System.Windows.Forms.ListView();
            this.columnParameter = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.bttnCreate = new System.Windows.Forms.Button();
            this.bttnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.bttnDelete);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.listViewMassParameter);
            this.groupBox1.Controls.Add(this.bttnAdd);
            this.groupBox1.Controls.Add(this.labelParameter);
            this.groupBox1.Controls.Add(this.listViewMainParameter);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.groupBox1.Location = new System.Drawing.Point(10, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox1.Size = new System.Drawing.Size(494, 382);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Parameters Map";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(453, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Select a parameter and click the \"Add\" button to create a shared parameter in the" +
                " Mass family.";
            // 
            // bttnDelete
            // 
            this.bttnDelete.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bttnDelete.Image = ((System.Drawing.Image)(resources.GetObject("bttnDelete.Image")));
            this.bttnDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnDelete.Location = new System.Drawing.Point(211, 238);
            this.bttnDelete.Margin = new System.Windows.Forms.Padding(5);
            this.bttnDelete.Name = "bttnDelete";
            this.bttnDelete.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
            this.bttnDelete.Size = new System.Drawing.Size(80, 23);
            this.bttnDelete.TabIndex = 5;
            this.bttnDelete.Text = "Delete";
            this.bttnDelete.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bttnDelete.UseVisualStyleBackColor = true;
            this.bttnDelete.Click += new System.EventHandler(this.bttnDelete_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.label2.Location = new System.Drawing.Point(314, 55);
            this.label2.Margin = new System.Windows.Forms.Padding(3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Mass Parameters:";
            // 
            // listViewMassParameter
            // 
            this.listViewMassParameter.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnMassParameter});
            this.listViewMassParameter.FullRowSelect = true;
            this.listViewMassParameter.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewMassParameter.Location = new System.Drawing.Point(317, 76);
            this.listViewMassParameter.Margin = new System.Windows.Forms.Padding(5);
            this.listViewMassParameter.Name = "listViewMassParameter";
            this.listViewMassParameter.Size = new System.Drawing.Size(156, 287);
            this.listViewMassParameter.TabIndex = 3;
            this.listViewMassParameter.UseCompatibleStateImageBehavior = false;
            this.listViewMassParameter.View = System.Windows.Forms.View.Details;
            // 
            // columnMassParameter
            // 
            this.columnMassParameter.Text = "Mass Parameters";
            this.columnMassParameter.Width = 149;
            // 
            // bttnAdd
            // 
            this.bttnAdd.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bttnAdd.Image = ((System.Drawing.Image)(resources.GetObject("bttnAdd.Image")));
            this.bttnAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bttnAdd.Location = new System.Drawing.Point(211, 145);
            this.bttnAdd.Margin = new System.Windows.Forms.Padding(5);
            this.bttnAdd.Name = "bttnAdd";
            this.bttnAdd.Padding = new System.Windows.Forms.Padding(15, 0, 15, 0);
            this.bttnAdd.Size = new System.Drawing.Size(80, 23);
            this.bttnAdd.TabIndex = 2;
            this.bttnAdd.Text = "Add";
            this.bttnAdd.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnAdd.UseVisualStyleBackColor = true;
            this.bttnAdd.Click += new System.EventHandler(this.bttnAdd_Click);
            // 
            // labelParameter
            // 
            this.labelParameter.AutoSize = true;
            this.labelParameter.ForeColor = System.Drawing.SystemColors.WindowText;
            this.labelParameter.Location = new System.Drawing.Point(20, 55);
            this.labelParameter.Margin = new System.Windows.Forms.Padding(3);
            this.labelParameter.Name = "labelParameter";
            this.labelParameter.Size = new System.Drawing.Size(94, 13);
            this.labelParameter.TabIndex = 1;
            this.labelParameter.Text = "Room Parameters:";
            // 
            // listViewMainParameter
            // 
            this.listViewMainParameter.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnParameter});
            this.listViewMainParameter.FullRowSelect = true;
            this.listViewMainParameter.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewMainParameter.Location = new System.Drawing.Point(21, 76);
            this.listViewMainParameter.Margin = new System.Windows.Forms.Padding(5);
            this.listViewMainParameter.Name = "listViewMainParameter";
            this.listViewMainParameter.Size = new System.Drawing.Size(156, 287);
            this.listViewMainParameter.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewMainParameter.TabIndex = 0;
            this.listViewMainParameter.UseCompatibleStateImageBehavior = false;
            this.listViewMainParameter.View = System.Windows.Forms.View.Details;
            // 
            // columnParameter
            // 
            this.columnParameter.Text = "ColumnParameter";
            this.columnParameter.Width = 147;
            // 
            // bttnCreate
            // 
            this.bttnCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnCreate.Location = new System.Drawing.Point(424, 400);
            this.bttnCreate.Margin = new System.Windows.Forms.Padding(5);
            this.bttnCreate.Name = "bttnCreate";
            this.bttnCreate.Size = new System.Drawing.Size(75, 28);
            this.bttnCreate.TabIndex = 1;
            this.bttnCreate.Text = "Apply";
            this.bttnCreate.UseVisualStyleBackColor = true;
            this.bttnCreate.Click += new System.EventHandler(this.bttnCreate_Click);
            // 
            // bttnCancel
            // 
            this.bttnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnCancel.Location = new System.Drawing.Point(339, 400);
            this.bttnCancel.Margin = new System.Windows.Forms.Padding(5);
            this.bttnCancel.Name = "bttnCancel";
            this.bttnCancel.Size = new System.Drawing.Size(75, 28);
            this.bttnCancel.TabIndex = 2;
            this.bttnCancel.Text = "Cancel";
            this.bttnCancel.UseVisualStyleBackColor = true;
            this.bttnCancel.Click += new System.EventHandler(this.bttnCancel_Click);
            // 
            // Form_Parameters
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 442);
            this.Controls.Add(this.bttnCancel);
            this.Controls.Add(this.bttnCreate);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(530, 480);
            this.MinimumSize = new System.Drawing.Size(530, 480);
            this.Name = "Form_Parameters";
            this.Padding = new System.Windows.Forms.Padding(10, 10, 10, 50);
            this.Text = "Shared Parameters";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button bttnCreate;
        private System.Windows.Forms.Button bttnCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView listViewMassParameter;
        private System.Windows.Forms.Button bttnAdd;
        private System.Windows.Forms.Label labelParameter;
        private System.Windows.Forms.ListView listViewMainParameter;
        private System.Windows.Forms.Button bttnDelete;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ColumnHeader columnParameter;
        private System.Windows.Forms.ColumnHeader columnMassParameter;
    }
}