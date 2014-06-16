namespace HOK.AVFManager.GenericForms
{
    partial class AdvancedSettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdvancedSettingsForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageValues = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.textBoxDescription5 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBoxConfig5 = new System.Windows.Forms.ComboBox();
            this.textBoxDescription4 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxConfig4 = new System.Windows.Forms.ComboBox();
            this.textBoxDescription3 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxConfig3 = new System.Windows.Forms.ComboBox();
            this.textBoxDescription2 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxConfig2 = new System.Windows.Forms.ComboBox();
            this.textBoxDescription1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxConfig1 = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButtonMonths = new System.Windows.Forms.RadioButton();
            this.radioButtonDays = new System.Windows.Forms.RadioButton();
            this.radioButtonTime = new System.Windows.Forms.RadioButton();
            this.radioButtonParameters = new System.Windows.Forms.RadioButton();
            this.tabPageGrids = new System.Windows.Forms.TabPage();
            this.bttnCancel = new System.Windows.Forms.Button();
            this.bttnApply = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPageValues.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageValues);
            this.tabControl1.Controls.Add(this.tabPageGrids);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(5, 5);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(409, 454);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPageValues
            // 
            this.tabPageValues.Controls.Add(this.groupBox1);
            this.tabPageValues.Location = new System.Drawing.Point(4, 22);
            this.tabPageValues.Name = "tabPageValues";
            this.tabPageValues.Padding = new System.Windows.Forms.Padding(10);
            this.tabPageValues.Size = new System.Drawing.Size(401, 428);
            this.tabPageValues.TabIndex = 0;
            this.tabPageValues.Text = "  Multiple Values  ";
            this.tabPageValues.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.groupBox1.Location = new System.Drawing.Point(10, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(381, 408);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Analysis Configurations";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.textBoxDescription5);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.comboBoxConfig5);
            this.groupBox3.Controls.Add(this.textBoxDescription4);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.comboBoxConfig4);
            this.groupBox3.Controls.Add(this.textBoxDescription3);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.comboBoxConfig3);
            this.groupBox3.Controls.Add(this.textBoxDescription2);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.comboBoxConfig2);
            this.groupBox3.Controls.Add(this.textBoxDescription1);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.comboBoxConfig1);
            this.groupBox3.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.groupBox3.Location = new System.Drawing.Point(157, 19);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(7);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(214, 378);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Definitions and Description";
            // 
            // textBoxDescription5
            // 
            this.textBoxDescription5.Location = new System.Drawing.Point(21, 340);
            this.textBoxDescription5.Multiline = true;
            this.textBoxDescription5.Name = "textBoxDescription5";
            this.textBoxDescription5.Size = new System.Drawing.Size(183, 20);
            this.textBoxDescription5.TabIndex = 14;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.SystemColors.WindowText;
            this.label5.Location = new System.Drawing.Point(18, 311);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Data - 5:";
            // 
            // comboBoxConfig5
            // 
            this.comboBoxConfig5.FormattingEnabled = true;
            this.comboBoxConfig5.Location = new System.Drawing.Point(67, 309);
            this.comboBoxConfig5.Margin = new System.Windows.Forms.Padding(7);
            this.comboBoxConfig5.Name = "comboBoxConfig5";
            this.comboBoxConfig5.Size = new System.Drawing.Size(137, 21);
            this.comboBoxConfig5.TabIndex = 12;
            // 
            // textBoxDescription4
            // 
            this.textBoxDescription4.Location = new System.Drawing.Point(21, 267);
            this.textBoxDescription4.Multiline = true;
            this.textBoxDescription4.Name = "textBoxDescription4";
            this.textBoxDescription4.Size = new System.Drawing.Size(183, 20);
            this.textBoxDescription4.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.SystemColors.WindowText;
            this.label4.Location = new System.Drawing.Point(18, 238);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Data - 4:";
            // 
            // comboBoxConfig4
            // 
            this.comboBoxConfig4.FormattingEnabled = true;
            this.comboBoxConfig4.Location = new System.Drawing.Point(67, 236);
            this.comboBoxConfig4.Margin = new System.Windows.Forms.Padding(7);
            this.comboBoxConfig4.Name = "comboBoxConfig4";
            this.comboBoxConfig4.Size = new System.Drawing.Size(137, 21);
            this.comboBoxConfig4.TabIndex = 9;
            // 
            // textBoxDescription3
            // 
            this.textBoxDescription3.Location = new System.Drawing.Point(21, 195);
            this.textBoxDescription3.Multiline = true;
            this.textBoxDescription3.Name = "textBoxDescription3";
            this.textBoxDescription3.Size = new System.Drawing.Size(183, 20);
            this.textBoxDescription3.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.SystemColors.WindowText;
            this.label3.Location = new System.Drawing.Point(18, 166);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Data - 3:";
            // 
            // comboBoxConfig3
            // 
            this.comboBoxConfig3.FormattingEnabled = true;
            this.comboBoxConfig3.Location = new System.Drawing.Point(67, 164);
            this.comboBoxConfig3.Margin = new System.Windows.Forms.Padding(7);
            this.comboBoxConfig3.Name = "comboBoxConfig3";
            this.comboBoxConfig3.Size = new System.Drawing.Size(137, 21);
            this.comboBoxConfig3.TabIndex = 6;
            // 
            // textBoxDescription2
            // 
            this.textBoxDescription2.Location = new System.Drawing.Point(21, 127);
            this.textBoxDescription2.Multiline = true;
            this.textBoxDescription2.Name = "textBoxDescription2";
            this.textBoxDescription2.Size = new System.Drawing.Size(183, 20);
            this.textBoxDescription2.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.label2.Location = new System.Drawing.Point(18, 98);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Data - 2:";
            // 
            // comboBoxConfig2
            // 
            this.comboBoxConfig2.FormattingEnabled = true;
            this.comboBoxConfig2.Location = new System.Drawing.Point(67, 96);
            this.comboBoxConfig2.Margin = new System.Windows.Forms.Padding(7);
            this.comboBoxConfig2.Name = "comboBoxConfig2";
            this.comboBoxConfig2.Size = new System.Drawing.Size(137, 21);
            this.comboBoxConfig2.TabIndex = 3;
            // 
            // textBoxDescription1
            // 
            this.textBoxDescription1.Location = new System.Drawing.Point(21, 57);
            this.textBoxDescription1.Multiline = true;
            this.textBoxDescription1.Name = "textBoxDescription1";
            this.textBoxDescription1.Size = new System.Drawing.Size(183, 20);
            this.textBoxDescription1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.label1.Location = new System.Drawing.Point(18, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Data - 1:";
            // 
            // comboBoxConfig1
            // 
            this.comboBoxConfig1.FormattingEnabled = true;
            this.comboBoxConfig1.Location = new System.Drawing.Point(67, 26);
            this.comboBoxConfig1.Margin = new System.Windows.Forms.Padding(7);
            this.comboBoxConfig1.Name = "comboBoxConfig1";
            this.comboBoxConfig1.Size = new System.Drawing.Size(137, 21);
            this.comboBoxConfig1.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radioButtonMonths);
            this.groupBox2.Controls.Add(this.radioButtonDays);
            this.groupBox2.Controls.Add(this.radioButtonTime);
            this.groupBox2.Controls.Add(this.radioButtonParameters);
            this.groupBox2.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.groupBox2.Location = new System.Drawing.Point(10, 19);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(7);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(133, 179);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Data Source";
            // 
            // radioButtonMonths
            // 
            this.radioButtonMonths.AutoSize = true;
            this.radioButtonMonths.Enabled = false;
            this.radioButtonMonths.ForeColor = System.Drawing.SystemColors.WindowText;
            this.radioButtonMonths.Location = new System.Drawing.Point(23, 137);
            this.radioButtonMonths.Margin = new System.Windows.Forms.Padding(10);
            this.radioButtonMonths.Name = "radioButtonMonths";
            this.radioButtonMonths.Size = new System.Drawing.Size(75, 17);
            this.radioButtonMonths.TabIndex = 3;
            this.radioButtonMonths.TabStop = true;
            this.radioButtonMonths.Text = "By Months";
            this.radioButtonMonths.UseVisualStyleBackColor = true;
            // 
            // radioButtonDays
            // 
            this.radioButtonDays.AutoSize = true;
            this.radioButtonDays.Enabled = false;
            this.radioButtonDays.ForeColor = System.Drawing.SystemColors.WindowText;
            this.radioButtonDays.Location = new System.Drawing.Point(23, 100);
            this.radioButtonDays.Margin = new System.Windows.Forms.Padding(10);
            this.radioButtonDays.Name = "radioButtonDays";
            this.radioButtonDays.Size = new System.Drawing.Size(64, 17);
            this.radioButtonDays.TabIndex = 2;
            this.radioButtonDays.TabStop = true;
            this.radioButtonDays.Text = "By Days";
            this.radioButtonDays.UseVisualStyleBackColor = true;
            // 
            // radioButtonTime
            // 
            this.radioButtonTime.AutoSize = true;
            this.radioButtonTime.Enabled = false;
            this.radioButtonTime.ForeColor = System.Drawing.SystemColors.WindowText;
            this.radioButtonTime.Location = new System.Drawing.Point(23, 63);
            this.radioButtonTime.Margin = new System.Windows.Forms.Padding(10);
            this.radioButtonTime.Name = "radioButtonTime";
            this.radioButtonTime.Size = new System.Drawing.Size(63, 17);
            this.radioButtonTime.TabIndex = 1;
            this.radioButtonTime.TabStop = true;
            this.radioButtonTime.Text = "By Time";
            this.radioButtonTime.UseVisualStyleBackColor = true;
            // 
            // radioButtonParameters
            // 
            this.radioButtonParameters.AutoSize = true;
            this.radioButtonParameters.ForeColor = System.Drawing.SystemColors.WindowText;
            this.radioButtonParameters.Location = new System.Drawing.Point(23, 26);
            this.radioButtonParameters.Margin = new System.Windows.Forms.Padding(10);
            this.radioButtonParameters.Name = "radioButtonParameters";
            this.radioButtonParameters.Size = new System.Drawing.Size(93, 17);
            this.radioButtonParameters.TabIndex = 0;
            this.radioButtonParameters.TabStop = true;
            this.radioButtonParameters.Text = "By Parameters";
            this.radioButtonParameters.UseVisualStyleBackColor = true;
            this.radioButtonParameters.CheckedChanged += new System.EventHandler(this.radioButtonParameters_CheckedChanged);
            // 
            // tabPageGrids
            // 
            this.tabPageGrids.Location = new System.Drawing.Point(4, 22);
            this.tabPageGrids.Name = "tabPageGrids";
            this.tabPageGrids.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGrids.Size = new System.Drawing.Size(401, 428);
            this.tabPageGrids.TabIndex = 1;
            this.tabPageGrids.Text = "  Grids  ";
            this.tabPageGrids.UseVisualStyleBackColor = true;
            // 
            // bttnCancel
            // 
            this.bttnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnCancel.Location = new System.Drawing.Point(254, 465);
            this.bttnCancel.Name = "bttnCancel";
            this.bttnCancel.Size = new System.Drawing.Size(75, 23);
            this.bttnCancel.TabIndex = 1;
            this.bttnCancel.Text = "Cancel";
            this.bttnCancel.UseVisualStyleBackColor = true;
            this.bttnCancel.Click += new System.EventHandler(this.bttnCancel_Click);
            // 
            // bttnApply
            // 
            this.bttnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnApply.Location = new System.Drawing.Point(335, 465);
            this.bttnApply.Name = "bttnApply";
            this.bttnApply.Size = new System.Drawing.Size(75, 23);
            this.bttnApply.TabIndex = 2;
            this.bttnApply.Text = "Apply";
            this.bttnApply.UseVisualStyleBackColor = true;
            this.bttnApply.Click += new System.EventHandler(this.bttnApply_Click);
            // 
            // AdvancedSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(419, 499);
            this.Controls.Add(this.bttnApply);
            this.Controls.Add(this.bttnCancel);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AdvancedSettingsForm";
            this.Padding = new System.Windows.Forms.Padding(5, 5, 5, 40);
            this.Text = "Advanced Settings";
            this.tabControl1.ResumeLayout(false);
            this.tabPageValues.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageValues;
        private System.Windows.Forms.TabPage tabPageGrids;
        private System.Windows.Forms.Button bttnCancel;
        private System.Windows.Forms.Button bttnApply;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox textBoxDescription5;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBoxConfig5;
        private System.Windows.Forms.TextBox textBoxDescription4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxConfig4;
        private System.Windows.Forms.TextBox textBoxDescription3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxConfig3;
        private System.Windows.Forms.TextBox textBoxDescription2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxConfig2;
        private System.Windows.Forms.TextBox textBoxDescription1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxConfig1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButtonMonths;
        private System.Windows.Forms.RadioButton radioButtonDays;
        private System.Windows.Forms.RadioButton radioButtonTime;
        private System.Windows.Forms.RadioButton radioButtonParameters;
    }
}