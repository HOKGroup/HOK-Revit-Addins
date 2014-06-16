namespace HOK.RoomsToMass.ToMass
{
    partial class Form_RoomMass
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_RoomMass));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.bttnCreate = new System.Windows.Forms.Button();
            this.bttnCancel = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBoxHeight = new System.Windows.Forms.TextBox();
            this.checkBoxHeight = new System.Windows.Forms.CheckBox();
            this.dataGridViewRoom = new System.Windows.Forms.DataGridView();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuCheckElement = new System.Windows.Forms.ToolStripMenuItem();
            this.menuUncheckElement = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuViewElement = new System.Windows.Forms.ToolStripMenuItem();
            this.bttnCheckAll = new System.Windows.Forms.Button();
            this.bttnCheckNone = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.richTextBoxParameters = new System.Windows.Forms.RichTextBox();
            this.bttnParameter = new System.Windows.Forms.Button();
            this.colorDialogDepartment = new System.Windows.Forms.ColorDialog();
            this.ColumnSelection = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColumnNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnDepartment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnLevel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnPhase = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnDesignOption = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewRoom)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // bttnCreate
            // 
            this.bttnCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnCreate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnCreate.Image = ((System.Drawing.Image)(resources.GetObject("bttnCreate.Image")));
            this.bttnCreate.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnCreate.Location = new System.Drawing.Point(738, 10);
            this.bttnCreate.Margin = new System.Windows.Forms.Padding(5, 10, 10, 10);
            this.bttnCreate.Name = "bttnCreate";
            this.bttnCreate.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.bttnCreate.Size = new System.Drawing.Size(115, 28);
            this.bttnCreate.TabIndex = 0;
            this.bttnCreate.Text = "Update Mass";
            this.bttnCreate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bttnCreate.UseVisualStyleBackColor = true;
            this.bttnCreate.Click += new System.EventHandler(this.bttnCreate_Click);
            // 
            // bttnCancel
            // 
            this.bttnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnCancel.Location = new System.Drawing.Point(653, 10);
            this.bttnCancel.Margin = new System.Windows.Forms.Padding(10, 10, 5, 10);
            this.bttnCancel.Name = "bttnCancel";
            this.bttnCancel.Size = new System.Drawing.Size(75, 28);
            this.bttnCancel.TabIndex = 2;
            this.bttnCancel.Text = "Cancel";
            this.bttnCancel.UseVisualStyleBackColor = true;
            this.bttnCancel.Click += new System.EventHandler(this.bttnCancel_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar,
            this.statusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 540);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(865, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar
            // 
            this.toolStripProgressBar.Name = "toolStripProgressBar";
            this.toolStripProgressBar.Size = new System.Drawing.Size(150, 16);
            this.toolStripProgressBar.Step = 1;
            this.toolStripProgressBar.Visible = false;
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(39, 17);
            this.statusLabel.Text = "Ready";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.bttnCreate);
            this.splitContainer1.Panel2.Controls.Add(this.bttnCancel);
            this.splitContainer1.Size = new System.Drawing.Size(865, 540);
            this.splitContainer1.SplitterDistance = 487;
            this.splitContainer1.TabIndex = 4;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.groupBox1);
            this.splitContainer2.Panel1.Padding = new System.Windows.Forms.Padding(10, 10, 10, 5);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer2.Panel2.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.splitContainer2.Size = new System.Drawing.Size(865, 487);
            this.splitContainer2.SplitterDistance = 420;
            this.splitContainer2.TabIndex = 6;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBoxHeight);
            this.groupBox1.Controls.Add(this.checkBoxHeight);
            this.groupBox1.Controls.Add(this.dataGridViewRoom);
            this.groupBox1.Controls.Add(this.bttnCheckAll);
            this.groupBox1.Controls.Add(this.bttnCheckNone);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.groupBox1.Location = new System.Drawing.Point(10, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(10, 40, 10, 35);
            this.groupBox1.Size = new System.Drawing.Size(845, 405);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Placed Rooms";
            // 
            // textBoxHeight
            // 
            this.textBoxHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxHeight.Location = new System.Drawing.Point(761, 20);
            this.textBoxHeight.Name = "textBoxHeight";
            this.textBoxHeight.Size = new System.Drawing.Size(70, 20);
            this.textBoxHeight.TabIndex = 12;
            this.textBoxHeight.Text = "10";
            // 
            // checkBoxHeight
            // 
            this.checkBoxHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxHeight.AutoSize = true;
            this.checkBoxHeight.ForeColor = System.Drawing.SystemColors.WindowText;
            this.checkBoxHeight.Location = new System.Drawing.Point(658, 22);
            this.checkBoxHeight.Name = "checkBoxHeight";
            this.checkBoxHeight.Size = new System.Drawing.Size(97, 17);
            this.checkBoxHeight.TabIndex = 11;
            this.checkBoxHeight.Text = "Default Height:";
            this.checkBoxHeight.UseVisualStyleBackColor = true;
            this.checkBoxHeight.CheckedChanged += new System.EventHandler(this.checkBoxHeight_CheckedChanged);
            // 
            // dataGridViewRoom
            // 
            this.dataGridViewRoom.AllowUserToAddRows = false;
            this.dataGridViewRoom.AllowUserToDeleteRows = false;
            this.dataGridViewRoom.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewRoom.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dataGridViewRoom.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridViewRoom.ColumnHeadersHeight = 30;
            this.dataGridViewRoom.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnSelection,
            this.ColumnNumber,
            this.ColumnName,
            this.ColumnDepartment,
            this.ColumnLevel,
            this.ColumnPhase,
            this.ColumnDesignOption});
            this.dataGridViewRoom.ContextMenuStrip = this.contextMenuStrip;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowFrame;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewRoom.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewRoom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewRoom.Location = new System.Drawing.Point(10, 53);
            this.dataGridViewRoom.Name = "dataGridViewRoom";
            this.dataGridViewRoom.RowHeadersVisible = false;
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.dataGridViewRoom.RowsDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewRoom.RowTemplate.Height = 24;
            this.dataGridViewRoom.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewRoom.Size = new System.Drawing.Size(825, 317);
            this.dataGridViewRoom.TabIndex = 4;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuCheckElement,
            this.menuUncheckElement,
            this.toolStripSeparator1,
            this.menuViewElement});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(208, 76);
            // 
            // menuCheckElement
            // 
            this.menuCheckElement.Image = ((System.Drawing.Image)(resources.GetObject("menuCheckElement.Image")));
            this.menuCheckElement.Name = "menuCheckElement";
            this.menuCheckElement.Size = new System.Drawing.Size(207, 22);
            this.menuCheckElement.Text = "Check Selected Rooms";
            this.menuCheckElement.Click += new System.EventHandler(this.menuCheckElement_Click);
            // 
            // menuUncheckElement
            // 
            this.menuUncheckElement.Image = ((System.Drawing.Image)(resources.GetObject("menuUncheckElement.Image")));
            this.menuUncheckElement.Name = "menuUncheckElement";
            this.menuUncheckElement.Size = new System.Drawing.Size(207, 22);
            this.menuUncheckElement.Text = "Uncheck Selected Rooms";
            this.menuUncheckElement.Click += new System.EventHandler(this.menuUncheckElement_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(204, 6);
            // 
            // menuViewElement
            // 
            this.menuViewElement.Image = ((System.Drawing.Image)(resources.GetObject("menuViewElement.Image")));
            this.menuViewElement.Name = "menuViewElement";
            this.menuViewElement.Size = new System.Drawing.Size(207, 22);
            this.menuViewElement.Text = "View Element";
            this.menuViewElement.Click += new System.EventHandler(this.menuViewElement_Click);
            // 
            // bttnCheckAll
            // 
            this.bttnCheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bttnCheckAll.Image = ((System.Drawing.Image)(resources.GetObject("bttnCheckAll.Image")));
            this.bttnCheckAll.Location = new System.Drawing.Point(39, 376);
            this.bttnCheckAll.Name = "bttnCheckAll";
            this.bttnCheckAll.Size = new System.Drawing.Size(23, 23);
            this.bttnCheckAll.TabIndex = 3;
            this.bttnCheckAll.UseVisualStyleBackColor = true;
            this.bttnCheckAll.Click += new System.EventHandler(this.bttnCheckAll_Click);
            // 
            // bttnCheckNone
            // 
            this.bttnCheckNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bttnCheckNone.Image = ((System.Drawing.Image)(resources.GetObject("bttnCheckNone.Image")));
            this.bttnCheckNone.Location = new System.Drawing.Point(10, 376);
            this.bttnCheckNone.Name = "bttnCheckNone";
            this.bttnCheckNone.Size = new System.Drawing.Size(23, 23);
            this.bttnCheckNone.TabIndex = 2;
            this.bttnCheckNone.UseVisualStyleBackColor = true;
            this.bttnCheckNone.Click += new System.EventHandler(this.bttnCheckNone_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.label1.Location = new System.Drawing.Point(12, 23);
            this.label1.Margin = new System.Windows.Forms.Padding(5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(216, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Please Check Rooms to Generate 3D Mass.";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.richTextBoxParameters);
            this.groupBox2.Controls.Add(this.bttnParameter);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.groupBox2.Location = new System.Drawing.Point(10, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(845, 63);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Parameters to be Exported";
            // 
            // richTextBoxParameters
            // 
            this.richTextBoxParameters.BackColor = System.Drawing.SystemColors.Control;
            this.richTextBoxParameters.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxParameters.ForeColor = System.Drawing.Color.DarkRed;
            this.richTextBoxParameters.Location = new System.Drawing.Point(184, 23);
            this.richTextBoxParameters.Name = "richTextBoxParameters";
            this.richTextBoxParameters.Size = new System.Drawing.Size(544, 34);
            this.richTextBoxParameters.TabIndex = 4;
            this.richTextBoxParameters.Text = "Select shared parameters.";
            // 
            // bttnParameter
            // 
            this.bttnParameter.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bttnParameter.Image = ((System.Drawing.Image)(resources.GetObject("bttnParameter.Image")));
            this.bttnParameter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnParameter.Location = new System.Drawing.Point(15, 27);
            this.bttnParameter.Margin = new System.Windows.Forms.Padding(5);
            this.bttnParameter.Name = "bttnParameter";
            this.bttnParameter.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.bttnParameter.Size = new System.Drawing.Size(151, 28);
            this.bttnParameter.TabIndex = 3;
            this.bttnParameter.Text = "Shared Parameters . . ";
            this.bttnParameter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bttnParameter.UseVisualStyleBackColor = true;
            this.bttnParameter.Click += new System.EventHandler(this.bttnParameter_Click);
            // 
            // colorDialogDepartment
            // 
            this.colorDialogDepartment.AnyColor = true;
            this.colorDialogDepartment.Color = System.Drawing.Color.White;
            this.colorDialogDepartment.ShowHelp = true;
            // 
            // ColumnSelection
            // 
            this.ColumnSelection.FillWeight = 45F;
            this.ColumnSelection.HeaderText = "Selection";
            this.ColumnSelection.Name = "ColumnSelection";
            // 
            // ColumnNumber
            // 
            this.ColumnNumber.FillWeight = 87.05584F;
            this.ColumnNumber.HeaderText = "Number";
            this.ColumnNumber.Name = "ColumnNumber";
            this.ColumnNumber.ReadOnly = true;
            // 
            // ColumnName
            // 
            this.ColumnName.FillWeight = 87.05584F;
            this.ColumnName.HeaderText = "Name";
            this.ColumnName.Name = "ColumnName";
            this.ColumnName.ReadOnly = true;
            // 
            // ColumnDepartment
            // 
            this.ColumnDepartment.FillWeight = 87.05584F;
            this.ColumnDepartment.HeaderText = "Department";
            this.ColumnDepartment.Name = "ColumnDepartment";
            this.ColumnDepartment.ReadOnly = true;
            // 
            // ColumnLevel
            // 
            this.ColumnLevel.FillWeight = 87.05584F;
            this.ColumnLevel.HeaderText = "Level";
            this.ColumnLevel.Name = "ColumnLevel";
            this.ColumnLevel.ReadOnly = true;
            // 
            // ColumnPhase
            // 
            this.ColumnPhase.FillWeight = 87.05584F;
            this.ColumnPhase.HeaderText = "Phase";
            this.ColumnPhase.Name = "ColumnPhase";
            this.ColumnPhase.ReadOnly = true;
            // 
            // ColumnDesignOption
            // 
            this.ColumnDesignOption.FillWeight = 87.05584F;
            this.ColumnDesignOption.HeaderText = "Design Option";
            this.ColumnDesignOption.Name = "ColumnDesignOption";
            this.ColumnDesignOption.ReadOnly = true;
            // 
            // Form_RoomMass
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(865, 562);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(1000, 700);
            this.MinimumSize = new System.Drawing.Size(774, 549);
            this.Name = "Form_RoomMass";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Create Extruded Mass from Room";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewRoom)).EndInit();
            this.contextMenuStrip.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bttnCreate;
        private System.Windows.Forms.Button bttnCancel;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button bttnCheckAll;
        private System.Windows.Forms.Button bttnCheckNone;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem menuViewElement;
        private System.Windows.Forms.ColorDialog colorDialogDepartment;
        private System.Windows.Forms.DataGridView dataGridViewRoom;
        private System.Windows.Forms.ToolStripMenuItem menuCheckElement;
        private System.Windows.Forms.ToolStripMenuItem menuUncheckElement;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Button bttnParameter;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RichTextBox richTextBoxParameters;
        private System.Windows.Forms.TextBox textBoxHeight;
        private System.Windows.Forms.CheckBox checkBoxHeight;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnSelection;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnNumber;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnDepartment;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnLevel;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPhase;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnDesignOption;
    }
}