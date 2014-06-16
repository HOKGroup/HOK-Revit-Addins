namespace HOK.BCFReader_SerialMaker
{
    partial class RecordForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RecordForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelTopic = new System.Windows.Forms.Label();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.textBoxKeyword = new System.Windows.Forms.TextBox();
            this.dataGridViewRecords = new System.Windows.Forms.DataGridView();
            this.buttonClose = new System.Windows.Forms.Button();
            this.ColumnCompany = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnIdentifier = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnLicense = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnModify = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewRecords)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.labelTopic);
            this.groupBox1.Controls.Add(this.buttonSearch);
            this.groupBox1.Controls.Add(this.textBoxKeyword);
            this.groupBox1.Controls.Add(this.dataGridViewRecords);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(7, 7);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(7, 30, 7, 7);
            this.groupBox1.Size = new System.Drawing.Size(728, 280);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Activation Codes";
            // 
            // labelTopic
            // 
            this.labelTopic.AutoSize = true;
            this.labelTopic.Location = new System.Drawing.Point(10, 19);
            this.labelTopic.Name = "labelTopic";
            this.labelTopic.Size = new System.Drawing.Size(380, 13);
            this.labelTopic.TabIndex = 3;
            this.labelTopic.Text = "Please review the generated activation codes to avoid providing duplicate one.";
            // 
            // buttonSearch
            // 
            this.buttonSearch.Image = ((System.Drawing.Image)(resources.GetObject("buttonSearch.Image")));
            this.buttonSearch.Location = new System.Drawing.Point(675, 13);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(26, 23);
            this.buttonSearch.TabIndex = 2;
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // textBoxKeyword
            // 
            this.textBoxKeyword.Location = new System.Drawing.Point(537, 16);
            this.textBoxKeyword.Name = "textBoxKeyword";
            this.textBoxKeyword.Size = new System.Drawing.Size(132, 20);
            this.textBoxKeyword.TabIndex = 1;
            // 
            // dataGridViewRecords
            // 
            this.dataGridViewRecords.AllowUserToAddRows = false;
            this.dataGridViewRecords.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewRecords.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dataGridViewRecords.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridViewRecords.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewRecords.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnCompany,
            this.ColumnIdentifier,
            this.ColumnLicense,
            this.ColumnDate,
            this.ColumnModify});
            this.dataGridViewRecords.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewRecords.Location = new System.Drawing.Point(7, 43);
            this.dataGridViewRecords.Name = "dataGridViewRecords";
            this.dataGridViewRecords.ReadOnly = true;
            this.dataGridViewRecords.RowHeadersVisible = false;
            this.dataGridViewRecords.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridViewRecords.Size = new System.Drawing.Size(714, 230);
            this.dataGridViewRecords.TabIndex = 0;
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.Location = new System.Drawing.Point(657, 293);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 1;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // ColumnCompany
            // 
            this.ColumnCompany.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnCompany.FillWeight = 120F;
            this.ColumnCompany.Frozen = true;
            this.ColumnCompany.HeaderText = "Company Names";
            this.ColumnCompany.Name = "ColumnCompany";
            this.ColumnCompany.ReadOnly = true;
            this.ColumnCompany.Width = 152;
            // 
            // ColumnIdentifier
            // 
            this.ColumnIdentifier.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnIdentifier.FillWeight = 70F;
            this.ColumnIdentifier.Frozen = true;
            this.ColumnIdentifier.HeaderText = "Identifier";
            this.ColumnIdentifier.Name = "ColumnIdentifier";
            this.ColumnIdentifier.ReadOnly = true;
            this.ColumnIdentifier.Width = 70;
            // 
            // ColumnLicense
            // 
            this.ColumnLicense.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnLicense.FillWeight = 250F;
            this.ColumnLicense.Frozen = true;
            this.ColumnLicense.HeaderText = "Activation Code";
            this.ColumnLicense.Name = "ColumnLicense";
            this.ColumnLicense.ReadOnly = true;
            this.ColumnLicense.Width = 254;
            // 
            // ColumnDate
            // 
            this.ColumnDate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnDate.FillWeight = 160F;
            this.ColumnDate.HeaderText = "Last Modified";
            this.ColumnDate.Name = "ColumnDate";
            this.ColumnDate.ReadOnly = true;
            this.ColumnDate.Width = 130;
            // 
            // ColumnModify
            // 
            this.ColumnModify.HeaderText = "Generated By";
            this.ColumnModify.Name = "ColumnModify";
            this.ColumnModify.ReadOnly = true;
            // 
            // RecordForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(742, 327);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RecordForm";
            this.Padding = new System.Windows.Forms.Padding(7, 7, 7, 40);
            this.Text = "Code Recorder";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewRecords)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.DataGridView dataGridViewRecords;
        private System.Windows.Forms.Label labelTopic;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.TextBox textBoxKeyword;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnCompany;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnIdentifier;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnLicense;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnModify;
    }
}