namespace HOK.RoomsToMass.ParameterAssigner
{
    partial class Form_ElementFilter
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_ElementFilter));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.listViewCategory = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.checkBoxEmpty2 = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbValue2 = new System.Windows.Forms.ComboBox();
            this.cmbOperation2 = new System.Windows.Forms.ComboBox();
            this.cmbParamName2 = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxValue2 = new System.Windows.Forms.TextBox();
            this.radioButtonNone = new System.Windows.Forms.RadioButton();
            this.radioButtonOr = new System.Windows.Forms.RadioButton();
            this.radioButtonAnd = new System.Windows.Forms.RadioButton();
            this.checkBoxEmpty1 = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbOperation1 = new System.Windows.Forms.ComboBox();
            this.cmbParamName1 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbValue1 = new System.Windows.Forms.ComboBox();
            this.textBoxValue1 = new System.Windows.Forms.TextBox();
            this.buttonNone = new System.Windows.Forms.Button();
            this.buttonInvert = new System.Windows.Forms.Button();
            this.buttonAll = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonFilter = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAdvance = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.splitContainer1);
            this.groupBox1.Controls.Add(this.buttonNone);
            this.groupBox1.Controls.Add(this.buttonInvert);
            this.groupBox1.Controls.Add(this.buttonAll);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(10, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(7, 30, 7, 40);
            this.groupBox1.Size = new System.Drawing.Size(564, 412);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filters";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(7, 43);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox3);
            this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.splitContainer1.Size = new System.Drawing.Size(550, 329);
            this.splitContainer1.SplitterDistance = 257;
            this.splitContainer1.TabIndex = 5;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.listViewCategory);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(7);
            this.groupBox2.Size = new System.Drawing.Size(257, 329);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Categories";
            // 
            // listViewCategory
            // 
            this.listViewCategory.CheckBoxes = true;
            this.listViewCategory.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listViewCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewCategory.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewCategory.Location = new System.Drawing.Point(7, 20);
            this.listViewCategory.Name = "listViewCategory";
            this.listViewCategory.Size = new System.Drawing.Size(243, 302);
            this.listViewCategory.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewCategory.TabIndex = 0;
            this.listViewCategory.UseCompatibleStateImageBehavior = false;
            this.listViewCategory.View = System.Windows.Forms.View.Details;
            this.listViewCategory.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewCategory_ItemChecked);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Element Categories";
            this.columnHeader1.Width = 230;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.groupBox4);
            this.groupBox3.Controls.Add(this.checkBoxEmpty1);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.cmbOperation1);
            this.groupBox3.Controls.Add(this.cmbParamName1);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.cmbValue1);
            this.groupBox3.Controls.Add(this.textBoxValue1);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(5, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(284, 329);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Parameter Rules";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.checkBoxEmpty2);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.cmbValue2);
            this.groupBox4.Controls.Add(this.cmbOperation2);
            this.groupBox4.Controls.Add(this.cmbParamName2);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.textBoxValue2);
            this.groupBox4.Controls.Add(this.radioButtonNone);
            this.groupBox4.Controls.Add(this.radioButtonOr);
            this.groupBox4.Controls.Add(this.radioButtonAnd);
            this.groupBox4.Location = new System.Drawing.Point(6, 154);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(261, 168);
            this.groupBox4.TabIndex = 21;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Add Parameters";
            // 
            // checkBoxEmpty2
            // 
            this.checkBoxEmpty2.AutoSize = true;
            this.checkBoxEmpty2.Enabled = false;
            this.checkBoxEmpty2.Location = new System.Drawing.Point(82, 141);
            this.checkBoxEmpty2.Name = "checkBoxEmpty2";
            this.checkBoxEmpty2.Size = new System.Drawing.Size(106, 17);
            this.checkBoxEmpty2.TabIndex = 28;
            this.checkBoxEmpty2.Text = "Find empty value";
            this.checkBoxEmpty2.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 90);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 27;
            this.label5.Text = "Operator:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 117);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(45, 13);
            this.label6.TabIndex = 26;
            this.label6.Text = "Values: ";
            // 
            // cmbValue2
            // 
            this.cmbValue2.Enabled = false;
            this.cmbValue2.FormattingEnabled = true;
            this.cmbValue2.Location = new System.Drawing.Point(82, 114);
            this.cmbValue2.Name = "cmbValue2";
            this.cmbValue2.Size = new System.Drawing.Size(170, 21);
            this.cmbValue2.Sorted = true;
            this.cmbValue2.TabIndex = 24;
            // 
            // cmbOperation2
            // 
            this.cmbOperation2.Enabled = false;
            this.cmbOperation2.FormattingEnabled = true;
            this.cmbOperation2.Location = new System.Drawing.Point(82, 87);
            this.cmbOperation2.Name = "cmbOperation2";
            this.cmbOperation2.Size = new System.Drawing.Size(170, 21);
            this.cmbOperation2.Sorted = true;
            this.cmbOperation2.TabIndex = 23;
            // 
            // cmbParamName2
            // 
            this.cmbParamName2.Enabled = false;
            this.cmbParamName2.FormattingEnabled = true;
            this.cmbParamName2.Location = new System.Drawing.Point(82, 60);
            this.cmbParamName2.Name = "cmbParamName2";
            this.cmbParamName2.Size = new System.Drawing.Size(170, 21);
            this.cmbParamName2.TabIndex = 22;
            this.cmbParamName2.SelectedIndexChanged += new System.EventHandler(this.cmbParamName2_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 63);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(67, 13);
            this.label7.TabIndex = 21;
            this.label7.Text = "Parameter 2:";
            // 
            // textBoxValue2
            // 
            this.textBoxValue2.Location = new System.Drawing.Point(82, 114);
            this.textBoxValue2.Name = "textBoxValue2";
            this.textBoxValue2.Size = new System.Drawing.Size(167, 20);
            this.textBoxValue2.TabIndex = 25;
            // 
            // radioButtonNone
            // 
            this.radioButtonNone.AutoSize = true;
            this.radioButtonNone.Checked = true;
            this.radioButtonNone.Location = new System.Drawing.Point(13, 19);
            this.radioButtonNone.Name = "radioButtonNone";
            this.radioButtonNone.Size = new System.Drawing.Size(51, 17);
            this.radioButtonNone.TabIndex = 20;
            this.radioButtonNone.TabStop = true;
            this.radioButtonNone.Text = "None";
            this.radioButtonNone.UseVisualStyleBackColor = true;
            this.radioButtonNone.CheckedChanged += new System.EventHandler(this.radioButtonNone_CheckedChanged);
            // 
            // radioButtonOr
            // 
            this.radioButtonOr.AutoSize = true;
            this.radioButtonOr.Location = new System.Drawing.Point(120, 19);
            this.radioButtonOr.Name = "radioButtonOr";
            this.radioButtonOr.Size = new System.Drawing.Size(36, 17);
            this.radioButtonOr.TabIndex = 6;
            this.radioButtonOr.Text = "Or";
            this.radioButtonOr.UseVisualStyleBackColor = true;
            // 
            // radioButtonAnd
            // 
            this.radioButtonAnd.AutoSize = true;
            this.radioButtonAnd.Location = new System.Drawing.Point(70, 19);
            this.radioButtonAnd.Name = "radioButtonAnd";
            this.radioButtonAnd.Size = new System.Drawing.Size(44, 17);
            this.radioButtonAnd.TabIndex = 5;
            this.radioButtonAnd.Text = "And";
            this.radioButtonAnd.UseVisualStyleBackColor = true;
            // 
            // checkBoxEmpty1
            // 
            this.checkBoxEmpty1.AutoSize = true;
            this.checkBoxEmpty1.Location = new System.Drawing.Point(88, 115);
            this.checkBoxEmpty1.Name = "checkBoxEmpty1";
            this.checkBoxEmpty1.Size = new System.Drawing.Size(106, 17);
            this.checkBoxEmpty1.TabIndex = 18;
            this.checkBoxEmpty1.Text = "Find empty value";
            this.checkBoxEmpty1.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 64);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Operator:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 91);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Values: ";
            // 
            // cmbOperation1
            // 
            this.cmbOperation1.FormattingEnabled = true;
            this.cmbOperation1.Location = new System.Drawing.Point(88, 61);
            this.cmbOperation1.Name = "cmbOperation1";
            this.cmbOperation1.Size = new System.Drawing.Size(170, 21);
            this.cmbOperation1.Sorted = true;
            this.cmbOperation1.TabIndex = 3;
            this.cmbOperation1.SelectedIndexChanged += new System.EventHandler(this.cmbOperation1_SelectedIndexChanged);
            // 
            // cmbParamName1
            // 
            this.cmbParamName1.FormattingEnabled = true;
            this.cmbParamName1.Location = new System.Drawing.Point(88, 34);
            this.cmbParamName1.Name = "cmbParamName1";
            this.cmbParamName1.Size = new System.Drawing.Size(170, 21);
            this.cmbParamName1.TabIndex = 1;
            this.cmbParamName1.SelectedIndexChanged += new System.EventHandler(this.cmbParamName1_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Parameter 1:";
            // 
            // cmbValue1
            // 
            this.cmbValue1.FormattingEnabled = true;
            this.cmbValue1.Location = new System.Drawing.Point(88, 88);
            this.cmbValue1.Name = "cmbValue1";
            this.cmbValue1.Size = new System.Drawing.Size(170, 21);
            this.cmbValue1.Sorted = true;
            this.cmbValue1.TabIndex = 4;
            // 
            // textBoxValue1
            // 
            this.textBoxValue1.Location = new System.Drawing.Point(88, 89);
            this.textBoxValue1.Name = "textBoxValue1";
            this.textBoxValue1.Size = new System.Drawing.Size(170, 20);
            this.textBoxValue1.TabIndex = 7;
            // 
            // buttonNone
            // 
            this.buttonNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonNone.Image = ((System.Drawing.Image)(resources.GetObject("buttonNone.Image")));
            this.buttonNone.Location = new System.Drawing.Point(40, 378);
            this.buttonNone.Name = "buttonNone";
            this.buttonNone.Size = new System.Drawing.Size(24, 24);
            this.buttonNone.TabIndex = 4;
            this.buttonNone.UseVisualStyleBackColor = true;
            this.buttonNone.Click += new System.EventHandler(this.buttonNone_Click);
            // 
            // buttonInvert
            // 
            this.buttonInvert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonInvert.Image = ((System.Drawing.Image)(resources.GetObject("buttonInvert.Image")));
            this.buttonInvert.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonInvert.Location = new System.Drawing.Point(70, 379);
            this.buttonInvert.Name = "buttonInvert";
            this.buttonInvert.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.buttonInvert.Size = new System.Drawing.Size(69, 23);
            this.buttonInvert.TabIndex = 3;
            this.buttonInvert.Text = "Invert";
            this.buttonInvert.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonInvert.UseVisualStyleBackColor = true;
            this.buttonInvert.Click += new System.EventHandler(this.buttonInvert_Click);
            // 
            // buttonAll
            // 
            this.buttonAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAll.Image = ((System.Drawing.Image)(resources.GetObject("buttonAll.Image")));
            this.buttonAll.Location = new System.Drawing.Point(10, 378);
            this.buttonAll.Name = "buttonAll";
            this.buttonAll.Size = new System.Drawing.Size(24, 24);
            this.buttonAll.TabIndex = 2;
            this.buttonAll.UseVisualStyleBackColor = true;
            this.buttonAll.Click += new System.EventHandler(this.buttonAll_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 21);
            this.label1.Margin = new System.Windows.Forms.Padding(10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(213, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select categories to be included in the filter.\r\n";
            // 
            // buttonFilter
            // 
            this.buttonFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFilter.Image = ((System.Drawing.Image)(resources.GetObject("buttonFilter.Image")));
            this.buttonFilter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonFilter.Location = new System.Drawing.Point(488, 428);
            this.buttonFilter.Name = "buttonFilter";
            this.buttonFilter.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.buttonFilter.Size = new System.Drawing.Size(80, 27);
            this.buttonFilter.TabIndex = 1;
            this.buttonFilter.Text = "Filter";
            this.buttonFilter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonFilter.UseVisualStyleBackColor = true;
            this.buttonFilter.Click += new System.EventHandler(this.buttonFilter_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonCancel.Location = new System.Drawing.Point(402, 428);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.buttonCancel.Size = new System.Drawing.Size(80, 27);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonAdvance
            // 
            this.buttonAdvance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAdvance.Location = new System.Drawing.Point(17, 430);
            this.buttonAdvance.Name = "buttonAdvance";
            this.buttonAdvance.Size = new System.Drawing.Size(77, 23);
            this.buttonAdvance.TabIndex = 3;
            this.buttonAdvance.Text = ">> Advance";
            this.buttonAdvance.UseVisualStyleBackColor = true;
            this.buttonAdvance.Click += new System.EventHandler(this.buttonAdvance_Click);
            // 
            // Form_ElementFilter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 462);
            this.Controls.Add(this.buttonAdvance);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonFilter);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form_ElementFilter";
            this.Padding = new System.Windows.Forms.Padding(10, 10, 10, 40);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Element Filter";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonFilter;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonInvert;
        private System.Windows.Forms.Button buttonAll;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView listViewCategory;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button buttonNone;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox cmbOperation1;
        private System.Windows.Forms.ComboBox cmbParamName1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonAdvance;
        private System.Windows.Forms.RadioButton radioButtonOr;
        private System.Windows.Forms.RadioButton radioButtonAnd;
        private System.Windows.Forms.ComboBox cmbValue1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxValue1;
        private System.Windows.Forms.CheckBox checkBoxEmpty1;
        private System.Windows.Forms.RadioButton radioButtonNone;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox checkBoxEmpty2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbValue2;
        private System.Windows.Forms.ComboBox cmbOperation2;
        private System.Windows.Forms.ComboBox cmbParamName2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxValue2;
    }
}