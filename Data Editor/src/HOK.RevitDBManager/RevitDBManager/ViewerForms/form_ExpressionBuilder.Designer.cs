namespace RevitDBManager.ViewerForms
{
    partial class form_ExpressionBuilder
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(form_ExpressionBuilder));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.linkCalCol = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.bttnOK = new System.Windows.Forms.Button();
            this.bttnUndo = new System.Windows.Forms.Button();
            this.richTexExpression = new System.Windows.Forms.RichTextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.radioButtonRoundDown = new System.Windows.Forms.RadioButton();
            this.radioButtonRoundUp = new System.Windows.Forms.RadioButton();
            this.radioButtonDecimal = new System.Windows.Forms.RadioButton();
            this.bttnRight = new System.Windows.Forms.Button();
            this.bttnLeft = new System.Windows.Forms.Button();
            this.textBoxDecimal = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.bttnSpace = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.bttnAddString = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.bttnPower = new System.Windows.Forms.Button();
            this.bttnDivide = new System.Windows.Forms.Button();
            this.bttnMultiply = new System.Windows.Forms.Button();
            this.bttnSubtract = new System.Windows.Forms.Button();
            this.bttnAdd = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.bttnAddConst = new System.Windows.Forms.Button();
            this.bttnAddField = new System.Windows.Forms.Button();
            this.textBoxConst = new System.Windows.Forms.TextBox();
            this.listBoxFields = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblPreview = new System.Windows.Forms.Label();
            this.lblCalField = new System.Windows.Forms.Label();
            this.bttnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.linkCalCol);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.bttnOK);
            this.groupBox1.Controls.Add(this.bttnUndo);
            this.groupBox1.Controls.Add(this.richTexExpression);
            this.groupBox1.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.groupBox1.Location = new System.Drawing.Point(10, 424);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(420, 168);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Expression";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label5.Location = new System.Drawing.Point(14, 74);
            this.label5.Margin = new System.Windows.Forms.Padding(3);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(298, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Math and Calculation [ Field1 ] + [ Field2 ] / Constant Number ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label4.Location = new System.Drawing.Point(13, 42);
            this.label4.Margin = new System.Windows.Forms.Padding(3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(115, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Example of expression:";
            // 
            // linkCalCol
            // 
            this.linkCalCol.AutoSize = true;
            this.linkCalCol.Location = new System.Drawing.Point(256, 20);
            this.linkCalCol.Name = "linkCalCol";
            this.linkCalCol.Size = new System.Drawing.Size(93, 13);
            this.linkCalCol.TabIndex = 6;
            this.linkCalCol.TabStop = true;
            this.linkCalCol.Text = "calculated column";
            this.linkCalCol.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkCalCol_LinkClicked);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label2.Location = new System.Drawing.Point(13, 58);
            this.label2.Margin = new System.Windows.Forms.Padding(3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(298, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "String Concatenation [ Field1 ] + [ Field2 ]  + \"Constant String\"";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label1.Location = new System.Drawing.Point(13, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(239, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Enter an Expression to calculate the value of the ";
            // 
            // bttnOK
            // 
            this.bttnOK.ForeColor = System.Drawing.SystemColors.ControlText;
            this.bttnOK.Image = ((System.Drawing.Image)(resources.GetObject("bttnOK.Image")));
            this.bttnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnOK.Location = new System.Drawing.Point(324, 100);
            this.bttnOK.Name = "bttnOK";
            this.bttnOK.Size = new System.Drawing.Size(72, 23);
            this.bttnOK.TabIndex = 3;
            this.bttnOK.Text = "OK";
            this.bttnOK.UseVisualStyleBackColor = true;
            this.bttnOK.Click += new System.EventHandler(this.bttnOK_Click);
            // 
            // bttnUndo
            // 
            this.bttnUndo.ForeColor = System.Drawing.SystemColors.ControlText;
            this.bttnUndo.Image = ((System.Drawing.Image)(resources.GetObject("bttnUndo.Image")));
            this.bttnUndo.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bttnUndo.Location = new System.Drawing.Point(324, 131);
            this.bttnUndo.Name = "bttnUndo";
            this.bttnUndo.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.bttnUndo.Size = new System.Drawing.Size(72, 23);
            this.bttnUndo.TabIndex = 2;
            this.bttnUndo.Text = "Undo";
            this.bttnUndo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bttnUndo.UseVisualStyleBackColor = true;
            this.bttnUndo.Click += new System.EventHandler(this.bttnUndo_Click);
            // 
            // richTexExpression
            // 
            this.richTexExpression.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.richTexExpression.Location = new System.Drawing.Point(13, 98);
            this.richTexExpression.Margin = new System.Windows.Forms.Padding(10);
            this.richTexExpression.Name = "richTexExpression";
            this.richTexExpression.ReadOnly = true;
            this.richTexExpression.Size = new System.Drawing.Size(298, 58);
            this.richTexExpression.TabIndex = 0;
            this.richTexExpression.Text = "";
            this.richTexExpression.TextChanged += new System.EventHandler(this.richTexExpression_TextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.groupBox4);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.bttnAddConst);
            this.groupBox2.Controls.Add(this.bttnAddField);
            this.groupBox2.Controls.Add(this.textBoxConst);
            this.groupBox2.Controls.Add(this.listBoxFields);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.groupBox2.Location = new System.Drawing.Point(12, 7);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(420, 347);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Expression Elements";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.radioButtonRoundDown);
            this.groupBox4.Controls.Add(this.radioButtonRoundUp);
            this.groupBox4.Controls.Add(this.radioButtonDecimal);
            this.groupBox4.Controls.Add(this.bttnRight);
            this.groupBox4.Controls.Add(this.bttnLeft);
            this.groupBox4.Controls.Add(this.textBoxDecimal);
            this.groupBox4.Controls.Add(this.label9);
            this.groupBox4.Controls.Add(this.bttnSpace);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.bttnAddString);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.bttnPower);
            this.groupBox4.Controls.Add(this.bttnDivide);
            this.groupBox4.Controls.Add(this.bttnMultiply);
            this.groupBox4.Controls.Add(this.bttnSubtract);
            this.groupBox4.Controls.Add(this.bttnAdd);
            this.groupBox4.Location = new System.Drawing.Point(190, 67);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox4.Size = new System.Drawing.Size(217, 272);
            this.groupBox4.TabIndex = 6;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Operators";
            // 
            // radioButtonRoundDown
            // 
            this.radioButtonRoundDown.AutoSize = true;
            this.radioButtonRoundDown.ForeColor = System.Drawing.SystemColors.MenuText;
            this.radioButtonRoundDown.Location = new System.Drawing.Point(105, 233);
            this.radioButtonRoundDown.Name = "radioButtonRoundDown";
            this.radioButtonRoundDown.Size = new System.Drawing.Size(88, 17);
            this.radioButtonRoundDown.TabIndex = 25;
            this.radioButtonRoundDown.TabStop = true;
            this.radioButtonRoundDown.Text = "Round Down";
            this.radioButtonRoundDown.UseVisualStyleBackColor = true;
            this.radioButtonRoundDown.CheckedChanged += new System.EventHandler(this.radioButtonRoundDown_CheckedChanged);
            // 
            // radioButtonRoundUp
            // 
            this.radioButtonRoundUp.AutoSize = true;
            this.radioButtonRoundUp.ForeColor = System.Drawing.SystemColors.MenuText;
            this.radioButtonRoundUp.Location = new System.Drawing.Point(25, 233);
            this.radioButtonRoundUp.Name = "radioButtonRoundUp";
            this.radioButtonRoundUp.Size = new System.Drawing.Size(74, 17);
            this.radioButtonRoundUp.TabIndex = 24;
            this.radioButtonRoundUp.TabStop = true;
            this.radioButtonRoundUp.Text = "Round Up";
            this.radioButtonRoundUp.UseVisualStyleBackColor = true;
            this.radioButtonRoundUp.CheckedChanged += new System.EventHandler(this.radioButtonRoundUp_CheckedChanged);
            // 
            // radioButtonDecimal
            // 
            this.radioButtonDecimal.AutoSize = true;
            this.radioButtonDecimal.ForeColor = System.Drawing.SystemColors.MenuText;
            this.radioButtonDecimal.Location = new System.Drawing.Point(25, 206);
            this.radioButtonDecimal.Name = "radioButtonDecimal";
            this.radioButtonDecimal.Size = new System.Drawing.Size(120, 17);
            this.radioButtonDecimal.TabIndex = 23;
            this.radioButtonDecimal.TabStop = true;
            this.radioButtonDecimal.Text = "Set Decimal Places:";
            this.radioButtonDecimal.UseVisualStyleBackColor = true;
            this.radioButtonDecimal.CheckedChanged += new System.EventHandler(this.radioButtonDecimal_CheckedChanged);
            // 
            // bttnRight
            // 
            this.bttnRight.Enabled = false;
            this.bttnRight.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnRight.ForeColor = System.Drawing.SystemColors.ControlText;
            this.bttnRight.Location = new System.Drawing.Point(55, 159);
            this.bttnRight.Margin = new System.Windows.Forms.Padding(5);
            this.bttnRight.Name = "bttnRight";
            this.bttnRight.Size = new System.Drawing.Size(30, 30);
            this.bttnRight.TabIndex = 20;
            this.bttnRight.Tag = ")";
            this.bttnRight.Text = ")";
            this.bttnRight.UseVisualStyleBackColor = true;
            this.bttnRight.Click += new System.EventHandler(this.bttnRight_Click);
            // 
            // bttnLeft
            // 
            this.bttnLeft.Enabled = false;
            this.bttnLeft.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnLeft.ForeColor = System.Drawing.SystemColors.ControlText;
            this.bttnLeft.Location = new System.Drawing.Point(15, 159);
            this.bttnLeft.Margin = new System.Windows.Forms.Padding(5);
            this.bttnLeft.Name = "bttnLeft";
            this.bttnLeft.Size = new System.Drawing.Size(30, 30);
            this.bttnLeft.TabIndex = 19;
            this.bttnLeft.Tag = "(";
            this.bttnLeft.Text = "(";
            this.bttnLeft.UseVisualStyleBackColor = true;
            this.bttnLeft.Click += new System.EventHandler(this.bttnLeft_Click);
            // 
            // textBoxDecimal
            // 
            this.textBoxDecimal.Enabled = false;
            this.textBoxDecimal.Location = new System.Drawing.Point(151, 205);
            this.textBoxDecimal.Name = "textBoxDecimal";
            this.textBoxDecimal.Size = new System.Drawing.Size(30, 20);
            this.textBoxDecimal.TabIndex = 17;
            this.textBoxDecimal.TextChanged += new System.EventHandler(this.textBoxDecimal_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label9.Location = new System.Drawing.Point(54, 208);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(0, 13);
            this.label9.TabIndex = 16;
            // 
            // bttnSpace
            // 
            this.bttnSpace.Enabled = false;
            this.bttnSpace.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnSpace.ForeColor = System.Drawing.SystemColors.ControlText;
            this.bttnSpace.Location = new System.Drawing.Point(55, 47);
            this.bttnSpace.Margin = new System.Windows.Forms.Padding(5);
            this.bttnSpace.Name = "bttnSpace";
            this.bttnSpace.Size = new System.Drawing.Size(51, 30);
            this.bttnSpace.TabIndex = 15;
            this.bttnSpace.Text = "Space";
            this.bttnSpace.UseVisualStyleBackColor = true;
            this.bttnSpace.Click += new System.EventHandler(this.bttnSpace_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label8.Location = new System.Drawing.Point(17, 28);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(82, 13);
            this.label8.TabIndex = 12;
            this.label8.Text = "String and Text:";
            // 
            // bttnAddString
            // 
            this.bttnAddString.Enabled = false;
            this.bttnAddString.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnAddString.ForeColor = System.Drawing.SystemColors.ControlText;
            this.bttnAddString.Location = new System.Drawing.Point(15, 47);
            this.bttnAddString.Margin = new System.Windows.Forms.Padding(5);
            this.bttnAddString.Name = "bttnAddString";
            this.bttnAddString.Size = new System.Drawing.Size(30, 30);
            this.bttnAddString.TabIndex = 10;
            this.bttnAddString.Tag = "+";
            this.bttnAddString.Text = "+";
            this.bttnAddString.UseVisualStyleBackColor = true;
            this.bttnAddString.Click += new System.EventHandler(this.bttnAddString_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label7.Location = new System.Drawing.Point(17, 100);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 13);
            this.label7.TabIndex = 9;
            this.label7.Text = "Arithmetic:";
            // 
            // bttnPower
            // 
            this.bttnPower.Enabled = false;
            this.bttnPower.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnPower.ForeColor = System.Drawing.SystemColors.ControlText;
            this.bttnPower.Location = new System.Drawing.Point(175, 119);
            this.bttnPower.Margin = new System.Windows.Forms.Padding(5);
            this.bttnPower.Name = "bttnPower";
            this.bttnPower.Size = new System.Drawing.Size(30, 30);
            this.bttnPower.TabIndex = 8;
            this.bttnPower.Tag = "^";
            this.bttnPower.Text = "^";
            this.bttnPower.UseVisualStyleBackColor = true;
            this.bttnPower.Click += new System.EventHandler(this.bttnPower_Click);
            // 
            // bttnDivide
            // 
            this.bttnDivide.Enabled = false;
            this.bttnDivide.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnDivide.ForeColor = System.Drawing.SystemColors.ControlText;
            this.bttnDivide.Location = new System.Drawing.Point(135, 119);
            this.bttnDivide.Margin = new System.Windows.Forms.Padding(5);
            this.bttnDivide.Name = "bttnDivide";
            this.bttnDivide.Size = new System.Drawing.Size(30, 30);
            this.bttnDivide.TabIndex = 3;
            this.bttnDivide.Tag = "/";
            this.bttnDivide.Text = "/";
            this.bttnDivide.UseVisualStyleBackColor = true;
            this.bttnDivide.Click += new System.EventHandler(this.bttnDivide_Click);
            // 
            // bttnMultiply
            // 
            this.bttnMultiply.Enabled = false;
            this.bttnMultiply.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnMultiply.ForeColor = System.Drawing.SystemColors.ControlText;
            this.bttnMultiply.Location = new System.Drawing.Point(95, 119);
            this.bttnMultiply.Margin = new System.Windows.Forms.Padding(5);
            this.bttnMultiply.Name = "bttnMultiply";
            this.bttnMultiply.Size = new System.Drawing.Size(30, 30);
            this.bttnMultiply.TabIndex = 2;
            this.bttnMultiply.Tag = "*";
            this.bttnMultiply.Text = "x";
            this.bttnMultiply.UseVisualStyleBackColor = true;
            this.bttnMultiply.Click += new System.EventHandler(this.bttnMultiply_Click);
            // 
            // bttnSubtract
            // 
            this.bttnSubtract.Enabled = false;
            this.bttnSubtract.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnSubtract.ForeColor = System.Drawing.SystemColors.ControlText;
            this.bttnSubtract.Location = new System.Drawing.Point(55, 119);
            this.bttnSubtract.Margin = new System.Windows.Forms.Padding(5);
            this.bttnSubtract.Name = "bttnSubtract";
            this.bttnSubtract.Size = new System.Drawing.Size(30, 30);
            this.bttnSubtract.TabIndex = 1;
            this.bttnSubtract.Tag = "-";
            this.bttnSubtract.Text = "-";
            this.bttnSubtract.UseVisualStyleBackColor = true;
            this.bttnSubtract.Click += new System.EventHandler(this.bttnSubtract_Click);
            // 
            // bttnAdd
            // 
            this.bttnAdd.Enabled = false;
            this.bttnAdd.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnAdd.ForeColor = System.Drawing.SystemColors.ControlText;
            this.bttnAdd.Location = new System.Drawing.Point(15, 119);
            this.bttnAdd.Margin = new System.Windows.Forms.Padding(5);
            this.bttnAdd.Name = "bttnAdd";
            this.bttnAdd.Size = new System.Drawing.Size(30, 30);
            this.bttnAdd.TabIndex = 0;
            this.bttnAdd.Tag = "+";
            this.bttnAdd.Text = "+";
            this.bttnAdd.UseVisualStyleBackColor = true;
            this.bttnAdd.Click += new System.EventHandler(this.bttnAdd_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label6.Location = new System.Drawing.Point(187, 19);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(52, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Constant:";
            // 
            // bttnAddConst
            // 
            this.bttnAddConst.ForeColor = System.Drawing.SystemColors.ControlText;
            this.bttnAddConst.Location = new System.Drawing.Point(355, 41);
            this.bttnAddConst.Name = "bttnAddConst";
            this.bttnAddConst.Size = new System.Drawing.Size(52, 23);
            this.bttnAddConst.TabIndex = 4;
            this.bttnAddConst.Text = "Add";
            this.bttnAddConst.UseVisualStyleBackColor = true;
            this.bttnAddConst.Click += new System.EventHandler(this.bttnAddConst_Click);
            // 
            // bttnAddField
            // 
            this.bttnAddField.ForeColor = System.Drawing.SystemColors.ControlText;
            this.bttnAddField.Location = new System.Drawing.Point(78, 294);
            this.bttnAddField.Name = "bttnAddField";
            this.bttnAddField.Size = new System.Drawing.Size(102, 23);
            this.bttnAddField.TabIndex = 3;
            this.bttnAddField.Text = "Add to Expression";
            this.bttnAddField.UseVisualStyleBackColor = true;
            this.bttnAddField.Click += new System.EventHandler(this.bttnAddField_Click);
            // 
            // textBoxConst
            // 
            this.textBoxConst.Location = new System.Drawing.Point(190, 41);
            this.textBoxConst.Name = "textBoxConst";
            this.textBoxConst.Size = new System.Drawing.Size(159, 20);
            this.textBoxConst.TabIndex = 2;
            // 
            // listBoxFields
            // 
            this.listBoxFields.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBoxFields.FormattingEnabled = true;
            this.listBoxFields.Location = new System.Drawing.Point(17, 37);
            this.listBoxFields.Margin = new System.Windows.Forms.Padding(5);
            this.listBoxFields.Name = "listBoxFields";
            this.listBoxFields.Size = new System.Drawing.Size(163, 249);
            this.listBoxFields.Sorted = true;
            this.listBoxFields.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label3.Location = new System.Drawing.Point(16, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Available Fields:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lblPreview);
            this.groupBox3.Controls.Add(this.lblCalField);
            this.groupBox3.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.groupBox3.Location = new System.Drawing.Point(10, 360);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(420, 58);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Preview of Result";
            // 
            // lblPreview
            // 
            this.lblPreview.AutoSize = true;
            this.lblPreview.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.lblPreview.Location = new System.Drawing.Point(187, 27);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Size = new System.Drawing.Size(33, 13);
            this.lblPreview.TabIndex = 1;
            this.lblPreview.Text = "value";
            // 
            // lblCalField
            // 
            this.lblCalField.AutoSize = true;
            this.lblCalField.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblCalField.Location = new System.Drawing.Point(16, 27);
            this.lblCalField.Name = "lblCalField";
            this.lblCalField.Size = new System.Drawing.Size(93, 13);
            this.lblCalField.TabIndex = 0;
            this.lblCalField.Text = "Calculated Fields: ";
            // 
            // bttnCancel
            // 
            this.bttnCancel.Location = new System.Drawing.Point(344, 600);
            this.bttnCancel.Margin = new System.Windows.Forms.Padding(5);
            this.bttnCancel.Name = "bttnCancel";
            this.bttnCancel.Size = new System.Drawing.Size(75, 23);
            this.bttnCancel.TabIndex = 3;
            this.bttnCancel.Text = "Cancel";
            this.bttnCancel.UseVisualStyleBackColor = true;
            this.bttnCancel.Click += new System.EventHandler(this.bttnCancel_Click);
            // 
            // form_ExpressionBuilder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ClientSize = new System.Drawing.Size(444, 637);
            this.Controls.Add(this.bttnCancel);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "form_ExpressionBuilder";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Expression Builder";
            this.Load += new System.EventHandler(this.form_ExpressionBuilder_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.LinkLabel linkCalCol;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bttnOK;
        private System.Windows.Forms.Button bttnUndo;
        private System.Windows.Forms.RichTextBox richTexExpression;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button bttnAddString;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button bttnPower;
        private System.Windows.Forms.Button bttnDivide;
        private System.Windows.Forms.Button bttnMultiply;
        private System.Windows.Forms.Button bttnSubtract;
        private System.Windows.Forms.Button bttnAdd;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button bttnAddConst;
        private System.Windows.Forms.Button bttnAddField;
        private System.Windows.Forms.TextBox textBoxConst;
        private System.Windows.Forms.ListBox listBoxFields;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lblPreview;
        private System.Windows.Forms.Label lblCalField;
        private System.Windows.Forms.Button bttnSpace;
        private System.Windows.Forms.Button bttnCancel;
        private System.Windows.Forms.TextBox textBoxDecimal;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button bttnRight;
        private System.Windows.Forms.Button bttnLeft;
        private System.Windows.Forms.RadioButton radioButtonDecimal;
        private System.Windows.Forms.RadioButton radioButtonRoundDown;
        private System.Windows.Forms.RadioButton radioButtonRoundUp;
    }
}