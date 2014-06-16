<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class form_ParamCalculate
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(form_ParamCalculate))
        Me.radioButtonDivABDivC = New System.Windows.Forms.RadioButton()
        Me.radioButtonDivABMultC = New System.Windows.Forms.RadioButton()
        Me.radioButtonMultABC = New System.Windows.Forms.RadioButton()
        Me.radioButtonMultABDivC = New System.Windows.Forms.RadioButton()
        Me.radioButtonSubABDivC = New System.Windows.Forms.RadioButton()
        Me.radioButtonAddABDivC = New System.Windows.Forms.RadioButton()
        Me.radioButtonSubABC = New System.Windows.Forms.RadioButton()
        Me.radioButtonAddABSubC = New System.Windows.Forms.RadioButton()
        Me.radioButtonAddABC = New System.Windows.Forms.RadioButton()
        Me.textBoxConstantC = New System.Windows.Forms.TextBox()
        Me.radioButtonSumAGroupBy = New System.Windows.Forms.RadioButton()
        Me.radioButtonSumA = New System.Windows.Forms.RadioButton()
        Me.textBoxConstantB = New System.Windows.Forms.TextBox()
        Me.textBoxConstantA = New System.Windows.Forms.TextBox()
        Me.groupBox2 = New System.Windows.Forms.GroupBox()
        Me.textBoxFormula3 = New System.Windows.Forms.TextBox()
        Me.textBoxFormula2 = New System.Windows.Forms.TextBox()
        Me.checkBoxFormulaInclude3 = New System.Windows.Forms.CheckBox()
        Me.checkBoxFormulaInclude2 = New System.Windows.Forms.CheckBox()
        Me.checkBoxFormulaInclude1 = New System.Windows.Forms.CheckBox()
        Me.label16 = New System.Windows.Forms.Label()
        Me.label15 = New System.Windows.Forms.Label()
        Me.buttonFormulaEdit3 = New System.Windows.Forms.Button()
        Me.buttonFormulaEdit2 = New System.Windows.Forms.Button()
        Me.buttonFormulaEdit1 = New System.Windows.Forms.Button()
        Me.buttonFormulaSave3 = New System.Windows.Forms.Button()
        Me.buttonFormulaSave2 = New System.Windows.Forms.Button()
        Me.label8 = New System.Windows.Forms.Label()
        Me.label14 = New System.Windows.Forms.Label()
        Me.label11 = New System.Windows.Forms.Label()
        Me.label12 = New System.Windows.Forms.Label()
        Me.label13 = New System.Windows.Forms.Label()
        Me.buttonFormulaSave1 = New System.Windows.Forms.Button()
        Me.textBoxFormula1 = New System.Windows.Forms.TextBox()
        Me.radioButtonSubABDivB = New System.Windows.Forms.RadioButton()
        Me.textBoxGroupBy = New System.Windows.Forms.TextBox()
        Me.radioButtonSubABDivA = New System.Windows.Forms.RadioButton()
        Me.radioButtonDivAB = New System.Windows.Forms.RadioButton()
        Me.groupBoxFunction = New System.Windows.Forms.GroupBox()
        Me.radioButtonMultAB = New System.Windows.Forms.RadioButton()
        Me.radioButtonSubAB = New System.Windows.Forms.RadioButton()
        Me.radioButtonAddAB = New System.Windows.Forms.RadioButton()
        Me.textBoxParameterC = New System.Windows.Forms.TextBox()
        Me.buttonProcess = New System.Windows.Forms.Button()
        Me.buttonClose = New System.Windows.Forms.Button()
        Me.groupBoxElementsToProcess = New System.Windows.Forms.GroupBox()
        Me.radioButtonProcessSelection = New System.Windows.Forms.RadioButton()
        Me.radioButtonProcessAll = New System.Windows.Forms.RadioButton()
        Me.label1 = New System.Windows.Forms.Label()
        Me.comboBoxElementType = New System.Windows.Forms.ComboBox()
        Me.textBoxParameterB = New System.Windows.Forms.TextBox()
        Me.textBoxTarget = New System.Windows.Forms.TextBox()
        Me.textBoxParameterA = New System.Windows.Forms.TextBox()
        Me.label2 = New System.Windows.Forms.Label()
        Me.label9 = New System.Windows.Forms.Label()
        Me.label6 = New System.Windows.Forms.Label()
        Me.label7 = New System.Windows.Forms.Label()
        Me.label3 = New System.Windows.Forms.Label()
        Me.label10 = New System.Windows.Forms.Label()
        Me.label4 = New System.Windows.Forms.Label()
        Me.label5 = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.Label18 = New System.Windows.Forms.Label()
        Me.Label17 = New System.Windows.Forms.Label()
        Me.groupBox2.SuspendLayout()
        Me.groupBoxFunction.SuspendLayout()
        Me.groupBoxElementsToProcess.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'radioButtonDivABDivC
        '
        Me.radioButtonDivABDivC.AutoSize = True
        Me.radioButtonDivABDivC.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.radioButtonDivABDivC.Location = New System.Drawing.Point(329, 134)
        Me.radioButtonDivABDivC.Name = "radioButtonDivABDivC"
        Me.radioButtonDivABDivC.Size = New System.Drawing.Size(74, 17)
        Me.radioButtonDivABDivC.TabIndex = 17
        Me.radioButtonDivABDivC.TabStop = True
        Me.radioButtonDivABDivC.Text = "(A / B) / C"
        Me.radioButtonDivABDivC.UseVisualStyleBackColor = True
        '
        'radioButtonDivABMultC
        '
        Me.radioButtonDivABMultC.AutoSize = True
        Me.radioButtonDivABMultC.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.radioButtonDivABMultC.Location = New System.Drawing.Point(329, 111)
        Me.radioButtonDivABMultC.Name = "radioButtonDivABMultC"
        Me.radioButtonDivABMultC.Size = New System.Drawing.Size(73, 17)
        Me.radioButtonDivABMultC.TabIndex = 16
        Me.radioButtonDivABMultC.TabStop = True
        Me.radioButtonDivABMultC.Text = "(A / B) * C"
        Me.radioButtonDivABMultC.UseVisualStyleBackColor = True
        '
        'radioButtonMultABC
        '
        Me.radioButtonMultABC.AutoSize = True
        Me.radioButtonMultABC.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.radioButtonMultABC.Location = New System.Drawing.Point(224, 112)
        Me.radioButtonMultABC.Name = "radioButtonMultABC"
        Me.radioButtonMultABC.Size = New System.Drawing.Size(66, 17)
        Me.radioButtonMultABC.TabIndex = 15
        Me.radioButtonMultABC.TabStop = True
        Me.radioButtonMultABC.Text = "A * B * C"
        Me.radioButtonMultABC.UseVisualStyleBackColor = True
        '
        'radioButtonMultABDivC
        '
        Me.radioButtonMultABDivC.AutoSize = True
        Me.radioButtonMultABDivC.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.radioButtonMultABDivC.Location = New System.Drawing.Point(330, 76)
        Me.radioButtonMultABDivC.Name = "radioButtonMultABDivC"
        Me.radioButtonMultABDivC.Size = New System.Drawing.Size(73, 17)
        Me.radioButtonMultABDivC.TabIndex = 14
        Me.radioButtonMultABDivC.TabStop = True
        Me.radioButtonMultABDivC.Text = "(A * B) / C"
        Me.radioButtonMultABDivC.UseVisualStyleBackColor = True
        '
        'radioButtonSubABDivC
        '
        Me.radioButtonSubABDivC.AutoSize = True
        Me.radioButtonSubABDivC.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.radioButtonSubABDivC.Location = New System.Drawing.Point(330, 53)
        Me.radioButtonSubABDivC.Name = "radioButtonSubABDivC"
        Me.radioButtonSubABDivC.Size = New System.Drawing.Size(72, 17)
        Me.radioButtonSubABDivC.TabIndex = 13
        Me.radioButtonSubABDivC.TabStop = True
        Me.radioButtonSubABDivC.Text = "(A - B) / C"
        Me.radioButtonSubABDivC.UseVisualStyleBackColor = True
        '
        'radioButtonAddABDivC
        '
        Me.radioButtonAddABDivC.AutoSize = True
        Me.radioButtonAddABDivC.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.radioButtonAddABDivC.Location = New System.Drawing.Point(330, 30)
        Me.radioButtonAddABDivC.Name = "radioButtonAddABDivC"
        Me.radioButtonAddABDivC.Size = New System.Drawing.Size(75, 17)
        Me.radioButtonAddABDivC.TabIndex = 12
        Me.radioButtonAddABDivC.TabStop = True
        Me.radioButtonAddABDivC.Text = "(A + B) / C"
        Me.radioButtonAddABDivC.UseVisualStyleBackColor = True
        '
        'radioButtonSubABC
        '
        Me.radioButtonSubABC.AutoSize = True
        Me.radioButtonSubABC.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.radioButtonSubABC.Location = New System.Drawing.Point(224, 74)
        Me.radioButtonSubABC.Name = "radioButtonSubABC"
        Me.radioButtonSubABC.Size = New System.Drawing.Size(70, 17)
        Me.radioButtonSubABC.TabIndex = 11
        Me.radioButtonSubABC.TabStop = True
        Me.radioButtonSubABC.Text = "(A - B) - C"
        Me.radioButtonSubABC.UseVisualStyleBackColor = True
        '
        'radioButtonAddABSubC
        '
        Me.radioButtonAddABSubC.AutoSize = True
        Me.radioButtonAddABSubC.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.radioButtonAddABSubC.Location = New System.Drawing.Point(224, 51)
        Me.radioButtonAddABSubC.Name = "radioButtonAddABSubC"
        Me.radioButtonAddABSubC.Size = New System.Drawing.Size(73, 17)
        Me.radioButtonAddABSubC.TabIndex = 10
        Me.radioButtonAddABSubC.TabStop = True
        Me.radioButtonAddABSubC.Text = "(A + B) - C"
        Me.radioButtonAddABSubC.UseVisualStyleBackColor = True
        '
        'radioButtonAddABC
        '
        Me.radioButtonAddABC.AutoSize = True
        Me.radioButtonAddABC.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.radioButtonAddABC.Location = New System.Drawing.Point(224, 28)
        Me.radioButtonAddABC.Name = "radioButtonAddABC"
        Me.radioButtonAddABC.Size = New System.Drawing.Size(70, 17)
        Me.radioButtonAddABC.TabIndex = 9
        Me.radioButtonAddABC.TabStop = True
        Me.radioButtonAddABC.Text = "A + B + C"
        Me.radioButtonAddABC.UseVisualStyleBackColor = True
        '
        'textBoxConstantC
        '
        Me.textBoxConstantC.Location = New System.Drawing.Point(231, 93)
        Me.textBoxConstantC.Name = "textBoxConstantC"
        Me.textBoxConstantC.Size = New System.Drawing.Size(128, 20)
        Me.textBoxConstantC.TabIndex = 71
        '
        'radioButtonSumAGroupBy
        '
        Me.radioButtonSumAGroupBy.AutoSize = True
        Me.radioButtonSumAGroupBy.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.radioButtonSumAGroupBy.Location = New System.Drawing.Point(108, 149)
        Me.radioButtonSumAGroupBy.Name = "radioButtonSumAGroupBy"
        Me.radioButtonSumAGroupBy.Size = New System.Drawing.Size(160, 17)
        Me.radioButtonSumAGroupBy.TabIndex = 8
        Me.radioButtonSumAGroupBy.TabStop = True
        Me.radioButtonSumAGroupBy.Text = "Sum (A) Group By Parameter"
        Me.radioButtonSumAGroupBy.UseVisualStyleBackColor = True
        '
        'radioButtonSumA
        '
        Me.radioButtonSumA.AutoSize = True
        Me.radioButtonSumA.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.radioButtonSumA.Location = New System.Drawing.Point(13, 149)
        Me.radioButtonSumA.Name = "radioButtonSumA"
        Me.radioButtonSumA.Size = New System.Drawing.Size(62, 17)
        Me.radioButtonSumA.TabIndex = 7
        Me.radioButtonSumA.TabStop = True
        Me.radioButtonSumA.Text = "Sum (A)"
        Me.radioButtonSumA.UseVisualStyleBackColor = True
        '
        'textBoxConstantB
        '
        Me.textBoxConstantB.Location = New System.Drawing.Point(231, 68)
        Me.textBoxConstantB.Name = "textBoxConstantB"
        Me.textBoxConstantB.Size = New System.Drawing.Size(128, 20)
        Me.textBoxConstantB.TabIndex = 70
        '
        'textBoxConstantA
        '
        Me.textBoxConstantA.Location = New System.Drawing.Point(231, 42)
        Me.textBoxConstantA.Name = "textBoxConstantA"
        Me.textBoxConstantA.Size = New System.Drawing.Size(128, 20)
        Me.textBoxConstantA.TabIndex = 69
        '
        'groupBox2
        '
        Me.groupBox2.Controls.Add(Me.textBoxFormula3)
        Me.groupBox2.Controls.Add(Me.textBoxFormula2)
        Me.groupBox2.Controls.Add(Me.checkBoxFormulaInclude3)
        Me.groupBox2.Controls.Add(Me.checkBoxFormulaInclude2)
        Me.groupBox2.Controls.Add(Me.checkBoxFormulaInclude1)
        Me.groupBox2.Controls.Add(Me.label16)
        Me.groupBox2.Controls.Add(Me.label15)
        Me.groupBox2.Controls.Add(Me.buttonFormulaEdit3)
        Me.groupBox2.Controls.Add(Me.buttonFormulaEdit2)
        Me.groupBox2.Controls.Add(Me.buttonFormulaEdit1)
        Me.groupBox2.Controls.Add(Me.buttonFormulaSave3)
        Me.groupBox2.Controls.Add(Me.buttonFormulaSave2)
        Me.groupBox2.Controls.Add(Me.label8)
        Me.groupBox2.Controls.Add(Me.label14)
        Me.groupBox2.Controls.Add(Me.label11)
        Me.groupBox2.Controls.Add(Me.label12)
        Me.groupBox2.Controls.Add(Me.label13)
        Me.groupBox2.Controls.Add(Me.buttonFormulaSave1)
        Me.groupBox2.Controls.Add(Me.textBoxFormula1)
        Me.groupBox2.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBox2.Location = New System.Drawing.Point(12, 352)
        Me.groupBox2.Name = "groupBox2"
        Me.groupBox2.Size = New System.Drawing.Size(807, 132)
        Me.groupBox2.TabIndex = 72
        Me.groupBox2.TabStop = False
        Me.groupBox2.Text = "Formulas to Process"
        '
        'textBoxFormula3
        '
        Me.textBoxFormula3.Location = New System.Drawing.Point(176, 92)
        Me.textBoxFormula3.Name = "textBoxFormula3"
        Me.textBoxFormula3.Size = New System.Drawing.Size(625, 20)
        Me.textBoxFormula3.TabIndex = 58
        '
        'textBoxFormula2
        '
        Me.textBoxFormula2.Location = New System.Drawing.Point(176, 67)
        Me.textBoxFormula2.Name = "textBoxFormula2"
        Me.textBoxFormula2.Size = New System.Drawing.Size(625, 20)
        Me.textBoxFormula2.TabIndex = 57
        '
        'checkBoxFormulaInclude3
        '
        Me.checkBoxFormulaInclude3.AutoSize = True
        Me.checkBoxFormulaInclude3.Location = New System.Drawing.Point(143, 95)
        Me.checkBoxFormulaInclude3.Name = "checkBoxFormulaInclude3"
        Me.checkBoxFormulaInclude3.Size = New System.Drawing.Size(15, 14)
        Me.checkBoxFormulaInclude3.TabIndex = 56
        Me.checkBoxFormulaInclude3.UseVisualStyleBackColor = True
        '
        'checkBoxFormulaInclude2
        '
        Me.checkBoxFormulaInclude2.AutoSize = True
        Me.checkBoxFormulaInclude2.Location = New System.Drawing.Point(143, 70)
        Me.checkBoxFormulaInclude2.Name = "checkBoxFormulaInclude2"
        Me.checkBoxFormulaInclude2.Size = New System.Drawing.Size(15, 14)
        Me.checkBoxFormulaInclude2.TabIndex = 55
        Me.checkBoxFormulaInclude2.UseVisualStyleBackColor = True
        '
        'checkBoxFormulaInclude1
        '
        Me.checkBoxFormulaInclude1.AutoSize = True
        Me.checkBoxFormulaInclude1.Location = New System.Drawing.Point(143, 45)
        Me.checkBoxFormulaInclude1.Name = "checkBoxFormulaInclude1"
        Me.checkBoxFormulaInclude1.Size = New System.Drawing.Size(15, 14)
        Me.checkBoxFormulaInclude1.TabIndex = 54
        Me.checkBoxFormulaInclude1.UseVisualStyleBackColor = True
        '
        'label16
        '
        Me.label16.AutoSize = True
        Me.label16.Location = New System.Drawing.Point(132, 24)
        Me.label16.Name = "label16"
        Me.label16.Size = New System.Drawing.Size(42, 13)
        Me.label16.TabIndex = 53
        Me.label16.Text = "Include"
        '
        'label15
        '
        Me.label15.AutoSize = True
        Me.label15.Location = New System.Drawing.Point(5, 24)
        Me.label15.Name = "label15"
        Me.label15.Size = New System.Drawing.Size(33, 13)
        Me.label15.TabIndex = 48
        Me.label15.Text = "Order"
        '
        'buttonFormulaEdit3
        '
        Me.buttonFormulaEdit3.Location = New System.Drawing.Point(94, 90)
        Me.buttonFormulaEdit3.Name = "buttonFormulaEdit3"
        Me.buttonFormulaEdit3.Size = New System.Drawing.Size(24, 22)
        Me.buttonFormulaEdit3.TabIndex = 52
        Me.buttonFormulaEdit3.UseVisualStyleBackColor = True
        '
        'buttonFormulaEdit2
        '
        Me.buttonFormulaEdit2.Location = New System.Drawing.Point(94, 65)
        Me.buttonFormulaEdit2.Name = "buttonFormulaEdit2"
        Me.buttonFormulaEdit2.Size = New System.Drawing.Size(24, 22)
        Me.buttonFormulaEdit2.TabIndex = 51
        Me.buttonFormulaEdit2.UseVisualStyleBackColor = True
        '
        'buttonFormulaEdit1
        '
        Me.buttonFormulaEdit1.Location = New System.Drawing.Point(94, 41)
        Me.buttonFormulaEdit1.Name = "buttonFormulaEdit1"
        Me.buttonFormulaEdit1.Size = New System.Drawing.Size(24, 22)
        Me.buttonFormulaEdit1.TabIndex = 50
        Me.buttonFormulaEdit1.UseVisualStyleBackColor = True
        '
        'buttonFormulaSave3
        '
        Me.buttonFormulaSave3.Location = New System.Drawing.Point(49, 90)
        Me.buttonFormulaSave3.Name = "buttonFormulaSave3"
        Me.buttonFormulaSave3.Size = New System.Drawing.Size(24, 22)
        Me.buttonFormulaSave3.TabIndex = 49
        Me.buttonFormulaSave3.UseVisualStyleBackColor = True
        '
        'buttonFormulaSave2
        '
        Me.buttonFormulaSave2.Location = New System.Drawing.Point(49, 65)
        Me.buttonFormulaSave2.Name = "buttonFormulaSave2"
        Me.buttonFormulaSave2.Size = New System.Drawing.Size(24, 22)
        Me.buttonFormulaSave2.TabIndex = 48
        Me.buttonFormulaSave2.UseVisualStyleBackColor = True
        '
        'label8
        '
        Me.label8.AutoSize = True
        Me.label8.Location = New System.Drawing.Point(45, 24)
        Me.label8.Name = "label8"
        Me.label8.Size = New System.Drawing.Size(32, 13)
        Me.label8.TabIndex = 43
        Me.label8.Text = "Save"
        '
        'label14
        '
        Me.label14.AutoSize = True
        Me.label14.Location = New System.Drawing.Point(93, 24)
        Me.label14.Name = "label14"
        Me.label14.Size = New System.Drawing.Size(25, 13)
        Me.label14.TabIndex = 47
        Me.label14.Text = "Edit"
        '
        'label11
        '
        Me.label11.AutoSize = True
        Me.label11.Location = New System.Drawing.Point(12, 95)
        Me.label11.Name = "label11"
        Me.label11.Size = New System.Drawing.Size(16, 13)
        Me.label11.TabIndex = 46
        Me.label11.Text = "3:" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10)
        '
        'label12
        '
        Me.label12.AutoSize = True
        Me.label12.Location = New System.Drawing.Point(12, 70)
        Me.label12.Name = "label12"
        Me.label12.Size = New System.Drawing.Size(16, 13)
        Me.label12.TabIndex = 45
        Me.label12.Text = "2:" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10)
        '
        'label13
        '
        Me.label13.AutoSize = True
        Me.label13.Location = New System.Drawing.Point(12, 44)
        Me.label13.Name = "label13"
        Me.label13.Size = New System.Drawing.Size(16, 13)
        Me.label13.TabIndex = 44
        Me.label13.Text = "1:"
        '
        'buttonFormulaSave1
        '
        Me.buttonFormulaSave1.Location = New System.Drawing.Point(49, 41)
        Me.buttonFormulaSave1.Name = "buttonFormulaSave1"
        Me.buttonFormulaSave1.Size = New System.Drawing.Size(24, 22)
        Me.buttonFormulaSave1.TabIndex = 42
        Me.buttonFormulaSave1.UseVisualStyleBackColor = True
        '
        'textBoxFormula1
        '
        Me.textBoxFormula1.Location = New System.Drawing.Point(176, 41)
        Me.textBoxFormula1.Name = "textBoxFormula1"
        Me.textBoxFormula1.Size = New System.Drawing.Size(625, 20)
        Me.textBoxFormula1.TabIndex = 41
        '
        'radioButtonSubABDivB
        '
        Me.radioButtonSubABDivB.AutoSize = True
        Me.radioButtonSubABDivB.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.radioButtonSubABDivB.Location = New System.Drawing.Point(108, 53)
        Me.radioButtonSubABDivB.Name = "radioButtonSubABDivB"
        Me.radioButtonSubABDivB.Size = New System.Drawing.Size(72, 17)
        Me.radioButtonSubABDivB.TabIndex = 5
        Me.radioButtonSubABDivB.TabStop = True
        Me.radioButtonSubABDivB.Text = "(A - B) / B"
        Me.radioButtonSubABDivB.UseVisualStyleBackColor = True
        '
        'textBoxGroupBy
        '
        Me.textBoxGroupBy.Location = New System.Drawing.Point(14, 140)
        Me.textBoxGroupBy.Name = "textBoxGroupBy"
        Me.textBoxGroupBy.Size = New System.Drawing.Size(345, 20)
        Me.textBoxGroupBy.TabIndex = 64
        '
        'radioButtonSubABDivA
        '
        Me.radioButtonSubABDivA.AutoSize = True
        Me.radioButtonSubABDivA.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.radioButtonSubABDivA.Location = New System.Drawing.Point(108, 30)
        Me.radioButtonSubABDivA.Name = "radioButtonSubABDivA"
        Me.radioButtonSubABDivA.Size = New System.Drawing.Size(72, 17)
        Me.radioButtonSubABDivA.TabIndex = 4
        Me.radioButtonSubABDivA.TabStop = True
        Me.radioButtonSubABDivA.Text = "(A - B) / A"
        Me.radioButtonSubABDivA.UseVisualStyleBackColor = True
        '
        'radioButtonDivAB
        '
        Me.radioButtonDivAB.AutoSize = True
        Me.radioButtonDivAB.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.radioButtonDivAB.Location = New System.Drawing.Point(13, 111)
        Me.radioButtonDivAB.Name = "radioButtonDivAB"
        Me.radioButtonDivAB.Size = New System.Drawing.Size(50, 17)
        Me.radioButtonDivAB.TabIndex = 3
        Me.radioButtonDivAB.TabStop = True
        Me.radioButtonDivAB.Text = "A / B"
        Me.radioButtonDivAB.UseVisualStyleBackColor = True
        '
        'groupBoxFunction
        '
        Me.groupBoxFunction.Controls.Add(Me.radioButtonDivABDivC)
        Me.groupBoxFunction.Controls.Add(Me.radioButtonDivABMultC)
        Me.groupBoxFunction.Controls.Add(Me.radioButtonMultABC)
        Me.groupBoxFunction.Controls.Add(Me.radioButtonMultABDivC)
        Me.groupBoxFunction.Controls.Add(Me.radioButtonSubABDivC)
        Me.groupBoxFunction.Controls.Add(Me.radioButtonAddABDivC)
        Me.groupBoxFunction.Controls.Add(Me.radioButtonSubABC)
        Me.groupBoxFunction.Controls.Add(Me.radioButtonAddABSubC)
        Me.groupBoxFunction.Controls.Add(Me.radioButtonAddABC)
        Me.groupBoxFunction.Controls.Add(Me.radioButtonSumAGroupBy)
        Me.groupBoxFunction.Controls.Add(Me.radioButtonSumA)
        Me.groupBoxFunction.Controls.Add(Me.radioButtonSubABDivB)
        Me.groupBoxFunction.Controls.Add(Me.radioButtonSubABDivA)
        Me.groupBoxFunction.Controls.Add(Me.radioButtonDivAB)
        Me.groupBoxFunction.Controls.Add(Me.radioButtonMultAB)
        Me.groupBoxFunction.Controls.Add(Me.radioButtonSubAB)
        Me.groupBoxFunction.Controls.Add(Me.radioButtonAddAB)
        Me.groupBoxFunction.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBoxFunction.Location = New System.Drawing.Point(397, 119)
        Me.groupBoxFunction.Name = "groupBoxFunction"
        Me.groupBoxFunction.Size = New System.Drawing.Size(422, 227)
        Me.groupBoxFunction.TabIndex = 61
        Me.groupBoxFunction.TabStop = False
        Me.groupBoxFunction.Text = "Function"
        '
        'radioButtonMultAB
        '
        Me.radioButtonMultAB.AutoSize = True
        Me.radioButtonMultAB.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.radioButtonMultAB.Location = New System.Drawing.Point(13, 88)
        Me.radioButtonMultAB.Name = "radioButtonMultAB"
        Me.radioButtonMultAB.Size = New System.Drawing.Size(49, 17)
        Me.radioButtonMultAB.TabIndex = 2
        Me.radioButtonMultAB.TabStop = True
        Me.radioButtonMultAB.Text = "A * B"
        Me.radioButtonMultAB.UseVisualStyleBackColor = True
        '
        'radioButtonSubAB
        '
        Me.radioButtonSubAB.AutoSize = True
        Me.radioButtonSubAB.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.radioButtonSubAB.Location = New System.Drawing.Point(13, 51)
        Me.radioButtonSubAB.Name = "radioButtonSubAB"
        Me.radioButtonSubAB.Size = New System.Drawing.Size(48, 17)
        Me.radioButtonSubAB.TabIndex = 1
        Me.radioButtonSubAB.TabStop = True
        Me.radioButtonSubAB.Text = "A - B"
        Me.radioButtonSubAB.UseVisualStyleBackColor = True
        '
        'radioButtonAddAB
        '
        Me.radioButtonAddAB.AutoSize = True
        Me.radioButtonAddAB.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.radioButtonAddAB.Location = New System.Drawing.Point(13, 28)
        Me.radioButtonAddAB.Name = "radioButtonAddAB"
        Me.radioButtonAddAB.Size = New System.Drawing.Size(51, 17)
        Me.radioButtonAddAB.TabIndex = 0
        Me.radioButtonAddAB.TabStop = True
        Me.radioButtonAddAB.Text = "A + B"
        Me.radioButtonAddAB.UseVisualStyleBackColor = True
        '
        'textBoxParameterC
        '
        Me.textBoxParameterC.Location = New System.Drawing.Point(30, 93)
        Me.textBoxParameterC.Name = "textBoxParameterC"
        Me.textBoxParameterC.Size = New System.Drawing.Size(174, 20)
        Me.textBoxParameterC.TabIndex = 62
        '
        'buttonProcess
        '
        Me.buttonProcess.Location = New System.Drawing.Point(534, 490)
        Me.buttonProcess.Name = "buttonProcess"
        Me.buttonProcess.Size = New System.Drawing.Size(140, 56)
        Me.buttonProcess.TabIndex = 54
        Me.buttonProcess.Text = "Process Elements"
        Me.buttonProcess.UseVisualStyleBackColor = True
        '
        'buttonClose
        '
        Me.buttonClose.Location = New System.Drawing.Point(680, 490)
        Me.buttonClose.Name = "buttonClose"
        Me.buttonClose.Size = New System.Drawing.Size(139, 56)
        Me.buttonClose.TabIndex = 53
        Me.buttonClose.Text = "Close"
        Me.buttonClose.UseVisualStyleBackColor = True
        '
        'groupBoxElementsToProcess
        '
        Me.groupBoxElementsToProcess.Controls.Add(Me.radioButtonProcessSelection)
        Me.groupBoxElementsToProcess.Controls.Add(Me.radioButtonProcessAll)
        Me.groupBoxElementsToProcess.Controls.Add(Me.label1)
        Me.groupBoxElementsToProcess.Controls.Add(Me.comboBoxElementType)
        Me.groupBoxElementsToProcess.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBoxElementsToProcess.Location = New System.Drawing.Point(12, 12)
        Me.groupBoxElementsToProcess.Name = "groupBoxElementsToProcess"
        Me.groupBoxElementsToProcess.Size = New System.Drawing.Size(807, 101)
        Me.groupBoxElementsToProcess.TabIndex = 52
        Me.groupBoxElementsToProcess.TabStop = False
        Me.groupBoxElementsToProcess.Text = "Elements to Process"
        '
        'radioButtonProcessSelection
        '
        Me.radioButtonProcessSelection.AutoSize = True
        Me.radioButtonProcessSelection.Location = New System.Drawing.Point(398, 43)
        Me.radioButtonProcessSelection.Name = "radioButtonProcessSelection"
        Me.radioButtonProcessSelection.Size = New System.Drawing.Size(113, 17)
        Me.radioButtonProcessSelection.TabIndex = 1
        Me.radioButtonProcessSelection.TabStop = True
        Me.radioButtonProcessSelection.Text = "Selected Elements"
        Me.radioButtonProcessSelection.UseVisualStyleBackColor = True
        '
        'radioButtonProcessAll
        '
        Me.radioButtonProcessAll.AutoSize = True
        Me.radioButtonProcessAll.Location = New System.Drawing.Point(609, 43)
        Me.radioButtonProcessAll.Name = "radioButtonProcessAll"
        Me.radioButtonProcessAll.Size = New System.Drawing.Size(125, 17)
        Me.radioButtonProcessAll.TabIndex = 0
        Me.radioButtonProcessAll.TabStop = True
        Me.radioButtonProcessAll.Text = "All Elements in Model"
        Me.radioButtonProcessAll.UseVisualStyleBackColor = True
        '
        'label1
        '
        Me.label1.AutoSize = True
        Me.label1.Location = New System.Drawing.Point(13, 23)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(49, 13)
        Me.label1.TabIndex = 51
        Me.label1.Text = "Category"
        '
        'comboBoxElementType
        '
        Me.comboBoxElementType.FormattingEnabled = True
        Me.comboBoxElementType.Location = New System.Drawing.Point(16, 43)
        Me.comboBoxElementType.Name = "comboBoxElementType"
        Me.comboBoxElementType.Size = New System.Drawing.Size(188, 21)
        Me.comboBoxElementType.TabIndex = 50
        '
        'textBoxParameterB
        '
        Me.textBoxParameterB.Location = New System.Drawing.Point(30, 68)
        Me.textBoxParameterB.Name = "textBoxParameterB"
        Me.textBoxParameterB.Size = New System.Drawing.Size(174, 20)
        Me.textBoxParameterB.TabIndex = 57
        '
        'textBoxTarget
        '
        Me.textBoxTarget.Location = New System.Drawing.Point(14, 186)
        Me.textBoxTarget.Name = "textBoxTarget"
        Me.textBoxTarget.Size = New System.Drawing.Size(345, 20)
        Me.textBoxTarget.TabIndex = 59
        '
        'textBoxParameterA
        '
        Me.textBoxParameterA.Location = New System.Drawing.Point(30, 42)
        Me.textBoxParameterA.Name = "textBoxParameterA"
        Me.textBoxParameterA.Size = New System.Drawing.Size(174, 20)
        Me.textBoxParameterA.TabIndex = 55
        '
        'label2
        '
        Me.label2.AutoSize = True
        Me.label2.Location = New System.Drawing.Point(13, 45)
        Me.label2.Name = "label2"
        Me.label2.Size = New System.Drawing.Size(17, 13)
        Me.label2.TabIndex = 56
        Me.label2.Text = "A:"
        '
        'label9
        '
        Me.label9.AutoSize = True
        Me.label9.Location = New System.Drawing.Point(209, 45)
        Me.label9.Name = "label9"
        Me.label9.Size = New System.Drawing.Size(16, 13)
        Me.label9.TabIndex = 67
        Me.label9.Text = "or"
        '
        'label6
        '
        Me.label6.AutoSize = True
        Me.label6.Location = New System.Drawing.Point(10, 124)
        Me.label6.Name = "label6"
        Me.label6.Size = New System.Drawing.Size(101, 13)
        Me.label6.TabIndex = 65
        Me.label6.Text = "Group by Parameter"
        '
        'label7
        '
        Me.label7.AutoSize = True
        Me.label7.Location = New System.Drawing.Point(13, 25)
        Me.label7.Name = "label7"
        Me.label7.Size = New System.Drawing.Size(129, 13)
        Me.label7.TabIndex = 66
        Me.label7.Text = "Source:  Parameter Name"
        '
        'label3
        '
        Me.label3.AutoSize = True
        Me.label3.Location = New System.Drawing.Point(13, 71)
        Me.label3.Name = "label3"
        Me.label3.Size = New System.Drawing.Size(17, 13)
        Me.label3.TabIndex = 58
        Me.label3.Text = "B:" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10)
        '
        'label10
        '
        Me.label10.AutoSize = True
        Me.label10.Location = New System.Drawing.Point(230, 25)
        Me.label10.Name = "label10"
        Me.label10.Size = New System.Drawing.Size(91, 13)
        Me.label10.TabIndex = 68
        Me.label10.Text = "Numeric Constant"
        '
        'label4
        '
        Me.label4.AutoSize = True
        Me.label4.Location = New System.Drawing.Point(10, 169)
        Me.label4.Name = "label4"
        Me.label4.Size = New System.Drawing.Size(89, 13)
        Me.label4.TabIndex = 60
        Me.label4.Text = "Target Parameter" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10)
        '
        'label5
        '
        Me.label5.AutoSize = True
        Me.label5.Location = New System.Drawing.Point(13, 96)
        Me.label5.Name = "label5"
        Me.label5.Size = New System.Drawing.Size(17, 13)
        Me.label5.TabIndex = 63
        Me.label5.Text = "C:" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10)
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.Label18)
        Me.GroupBox1.Controls.Add(Me.Label17)
        Me.GroupBox1.Controls.Add(Me.label7)
        Me.GroupBox1.Controls.Add(Me.textBoxConstantC)
        Me.GroupBox1.Controls.Add(Me.label5)
        Me.GroupBox1.Controls.Add(Me.textBoxConstantB)
        Me.GroupBox1.Controls.Add(Me.label4)
        Me.GroupBox1.Controls.Add(Me.textBoxConstantA)
        Me.GroupBox1.Controls.Add(Me.label10)
        Me.GroupBox1.Controls.Add(Me.label3)
        Me.GroupBox1.Controls.Add(Me.textBoxGroupBy)
        Me.GroupBox1.Controls.Add(Me.label6)
        Me.GroupBox1.Controls.Add(Me.label9)
        Me.GroupBox1.Controls.Add(Me.textBoxParameterC)
        Me.GroupBox1.Controls.Add(Me.label2)
        Me.GroupBox1.Controls.Add(Me.textBoxParameterA)
        Me.GroupBox1.Controls.Add(Me.textBoxTarget)
        Me.GroupBox1.Controls.Add(Me.textBoxParameterB)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 119)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(379, 227)
        Me.GroupBox1.TabIndex = 74
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Formula Editor"
        '
        'Label18
        '
        Me.Label18.AutoSize = True
        Me.Label18.Location = New System.Drawing.Point(209, 96)
        Me.Label18.Name = "Label18"
        Me.Label18.Size = New System.Drawing.Size(16, 13)
        Me.Label18.TabIndex = 73
        Me.Label18.Text = "or"
        '
        'Label17
        '
        Me.Label17.AutoSize = True
        Me.Label17.Location = New System.Drawing.Point(209, 71)
        Me.Label17.Name = "Label17"
        Me.Label17.Size = New System.Drawing.Size(16, 13)
        Me.Label17.TabIndex = 72
        Me.Label17.Text = "or"
        '
        'form_ParamCalculate
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(832, 557)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.groupBox2)
        Me.Controls.Add(Me.groupBoxFunction)
        Me.Controls.Add(Me.buttonProcess)
        Me.Controls.Add(Me.buttonClose)
        Me.Controls.Add(Me.groupBoxElementsToProcess)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(848, 595)
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(848, 595)
        Me.Name = "form_ParamCalculate"
        Me.Text = "Math Calculation"
        Me.groupBox2.ResumeLayout(False)
        Me.groupBox2.PerformLayout()
        Me.groupBoxFunction.ResumeLayout(False)
        Me.groupBoxFunction.PerformLayout()
        Me.groupBoxElementsToProcess.ResumeLayout(False)
        Me.groupBoxElementsToProcess.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents radioButtonDivABDivC As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonDivABMultC As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonMultABC As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonMultABDivC As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonSubABDivC As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonAddABDivC As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonSubABC As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonAddABSubC As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonAddABC As System.Windows.Forms.RadioButton
    Private WithEvents textBoxConstantC As System.Windows.Forms.TextBox
    Private WithEvents radioButtonSumAGroupBy As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonSumA As System.Windows.Forms.RadioButton
    Private WithEvents textBoxConstantB As System.Windows.Forms.TextBox
    Private WithEvents textBoxConstantA As System.Windows.Forms.TextBox
    Private WithEvents groupBox2 As System.Windows.Forms.GroupBox
    Private WithEvents textBoxFormula3 As System.Windows.Forms.TextBox
    Private WithEvents textBoxFormula2 As System.Windows.Forms.TextBox
    Private WithEvents checkBoxFormulaInclude3 As System.Windows.Forms.CheckBox
    Private WithEvents checkBoxFormulaInclude2 As System.Windows.Forms.CheckBox
    Private WithEvents checkBoxFormulaInclude1 As System.Windows.Forms.CheckBox
    Private WithEvents label16 As System.Windows.Forms.Label
    Private WithEvents label15 As System.Windows.Forms.Label
    Private WithEvents buttonFormulaEdit3 As System.Windows.Forms.Button
    Private WithEvents buttonFormulaEdit2 As System.Windows.Forms.Button
    Private WithEvents buttonFormulaEdit1 As System.Windows.Forms.Button
    Private WithEvents buttonFormulaSave3 As System.Windows.Forms.Button
    Private WithEvents buttonFormulaSave2 As System.Windows.Forms.Button
    Private WithEvents label8 As System.Windows.Forms.Label
    Private WithEvents label14 As System.Windows.Forms.Label
    Private WithEvents label11 As System.Windows.Forms.Label
    Private WithEvents label12 As System.Windows.Forms.Label
    Private WithEvents label13 As System.Windows.Forms.Label
    Private WithEvents buttonFormulaSave1 As System.Windows.Forms.Button
    Private WithEvents textBoxFormula1 As System.Windows.Forms.TextBox
    Private WithEvents radioButtonSubABDivB As System.Windows.Forms.RadioButton
    Private WithEvents textBoxGroupBy As System.Windows.Forms.TextBox
    Private WithEvents radioButtonSubABDivA As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonDivAB As System.Windows.Forms.RadioButton
    Private WithEvents groupBoxFunction As System.Windows.Forms.GroupBox
    Private WithEvents radioButtonMultAB As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonSubAB As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonAddAB As System.Windows.Forms.RadioButton
    Private WithEvents textBoxParameterC As System.Windows.Forms.TextBox
    Private WithEvents buttonProcess As System.Windows.Forms.Button
    Private WithEvents buttonClose As System.Windows.Forms.Button
    Private WithEvents groupBoxElementsToProcess As System.Windows.Forms.GroupBox
    Private WithEvents radioButtonProcessSelection As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonProcessAll As System.Windows.Forms.RadioButton
    Private WithEvents comboBoxElementType As System.Windows.Forms.ComboBox
    Private WithEvents label1 As System.Windows.Forms.Label
    Private WithEvents textBoxParameterB As System.Windows.Forms.TextBox
    Private WithEvents textBoxTarget As System.Windows.Forms.TextBox
    Private WithEvents textBoxParameterA As System.Windows.Forms.TextBox
    Private WithEvents label2 As System.Windows.Forms.Label
    Private WithEvents label9 As System.Windows.Forms.Label
    Private WithEvents label6 As System.Windows.Forms.Label
    Private WithEvents label7 As System.Windows.Forms.Label
    Private WithEvents label3 As System.Windows.Forms.Label
    Private WithEvents label10 As System.Windows.Forms.Label
    Private WithEvents label4 As System.Windows.Forms.Label
    Private WithEvents label5 As System.Windows.Forms.Label
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Private WithEvents Label18 As System.Windows.Forms.Label
    Private WithEvents Label17 As System.Windows.Forms.Label
End Class
