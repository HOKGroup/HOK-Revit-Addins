namespace HOK.RoomsToMass.ParameterAssigner
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.bttnCancel = new System.Windows.Forms.Button();
            this.bttnApply = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.bttnCheckAll = new System.Windows.Forms.Button();
            this.bttnCheckNone = new System.Windows.Forms.Button();
            this.dataGridViewParam = new System.Windows.Forms.DataGridView();
            this.ColumnSelection = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColumnMass = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnElement = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewParam)).BeginInit();
            this.SuspendLayout();
            // 
            // bttnCancel
            // 
            this.bttnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnCancel.Location = new System.Drawing.Point(223, 378);
            this.bttnCancel.Name = "bttnCancel";
            this.bttnCancel.Size = new System.Drawing.Size(75, 23);
            this.bttnCancel.TabIndex = 5;
            this.bttnCancel.Text = "Cancel";
            this.bttnCancel.UseVisualStyleBackColor = true;
            this.bttnCancel.Click += new System.EventHandler(this.bttnCancel_Click);
            // 
            // bttnApply
            // 
            this.bttnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnApply.Location = new System.Drawing.Point(304, 378);
            this.bttnApply.Name = "bttnApply";
            this.bttnApply.Size = new System.Drawing.Size(75, 23);
            this.bttnApply.TabIndex = 4;
            this.bttnApply.Text = "Apply";
            this.bttnApply.UseVisualStyleBackColor = true;
            this.bttnApply.Click += new System.EventHandler(this.bttnApply_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.bttnCheckAll);
            this.groupBox1.Controls.Add(this.bttnCheckNone);
            this.groupBox1.Controls.Add(this.dataGridViewParam);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(10, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(10, 30, 10, 35);
            this.groupBox1.Size = new System.Drawing.Size(379, 362);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Parameters ";
            // 
            // bttnCheckAll
            // 
            this.bttnCheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bttnCheckAll.Image = ((System.Drawing.Image)(resources.GetObject("bttnCheckAll.Image")));
            this.bttnCheckAll.Location = new System.Drawing.Point(39, 331);
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
            this.bttnCheckNone.Location = new System.Drawing.Point(10, 331);
            this.bttnCheckNone.Name = "bttnCheckNone";
            this.bttnCheckNone.Size = new System.Drawing.Size(23, 23);
            this.bttnCheckNone.TabIndex = 4;
            this.bttnCheckNone.UseVisualStyleBackColor = true;
            this.bttnCheckNone.Click += new System.EventHandler(this.bttnCheckNone_Click);
            // 
            // dataGridViewParam
            // 
            this.dataGridViewParam.AllowUserToAddRows = false;
            this.dataGridViewParam.AllowUserToDeleteRows = false;
            this.dataGridViewParam.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.dataGridViewParam.ColumnHeadersHeight = 30;
            this.dataGridViewParam.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnSelection,
            this.ColumnMass,
            this.ColumnElement});
            this.dataGridViewParam.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewParam.Location = new System.Drawing.Point(10, 43);
            this.dataGridViewParam.Name = "dataGridViewParam";
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.Padding = new System.Windows.Forms.Padding(5);
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewParam.RowHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewParam.RowHeadersVisible = false;
            this.dataGridViewParam.RowHeadersWidth = 50;
            this.dataGridViewParam.Size = new System.Drawing.Size(359, 284);
            this.dataGridViewParam.TabIndex = 2;
            // 
            // ColumnSelection
            // 
            this.ColumnSelection.HeaderText = "Selection";
            this.ColumnSelection.Name = "ColumnSelection";
            this.ColumnSelection.Width = 55;
            // 
            // ColumnMass
            // 
            this.ColumnMass.HeaderText = "Mass Parameter";
            this.ColumnMass.Name = "ColumnMass";
            this.ColumnMass.ReadOnly = true;
            this.ColumnMass.Width = 150;
            // 
            // ColumnElement
            // 
            this.ColumnElement.HeaderText = "Element Parameter";
            this.ColumnElement.Name = "ColumnElement";
            this.ColumnElement.Width = 150;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 24);
            this.label1.Margin = new System.Windows.Forms.Padding(3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(214, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Propagate parameters from the following list.";
            // 
            // Form_Parameters
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(399, 412);
            this.Controls.Add(this.bttnCancel);
            this.Controls.Add(this.bttnApply);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(415, 450);
            this.Name = "Form_Parameters";
            this.Padding = new System.Windows.Forms.Padding(10, 10, 10, 40);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mass Parameters";
            this.Load += new System.EventHandler(this.Form_Parameters_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewParam)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bttnCancel;
        private System.Windows.Forms.Button bttnApply;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dataGridViewParam;
        private System.Windows.Forms.Button bttnCheckAll;
        private System.Windows.Forms.Button bttnCheckNone;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnSelection;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnMass;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnElement;
    }
}