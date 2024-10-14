<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class form_ParamMenu
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(form_ParamMenu))
        Me.buttonScheduleKey = New System.Windows.Forms.Button()
        Me.buttonStringCalculate = New System.Windows.Forms.Button()
        Me.buttonWriteToExcel = New System.Windows.Forms.Button()
        Me.buttonReloadSettings = New System.Windows.Forms.Button()
        Me.buttonMathCalculate = New System.Windows.Forms.Button()
        Me.buttonElementRollUp = New System.Windows.Forms.Button()
        Me.buttonElementParent = New System.Windows.Forms.Button()
        Me.buttonClose = New System.Windows.Forms.Button()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.ButtonSuperCalc = New System.Windows.Forms.Button()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.GroupBox4.SuspendLayout()
        Me.SuspendLayout()
        '
        'buttonScheduleKey
        '
        Me.buttonScheduleKey.Location = New System.Drawing.Point(6, 52)
        Me.buttonScheduleKey.Name = "buttonScheduleKey"
        Me.buttonScheduleKey.Size = New System.Drawing.Size(280, 27)
        Me.buttonScheduleKey.TabIndex = 31
        Me.buttonScheduleKey.Text = "Schedule Key Manager"
        Me.buttonScheduleKey.UseVisualStyleBackColor = True
        '
        'buttonStringCalculate
        '
        Me.buttonStringCalculate.Location = New System.Drawing.Point(6, 19)
        Me.buttonStringCalculate.Name = "buttonStringCalculate"
        Me.buttonStringCalculate.Size = New System.Drawing.Size(280, 27)
        Me.buttonStringCalculate.TabIndex = 29
        Me.buttonStringCalculate.Text = "Calculation and Concatenation"
        Me.buttonStringCalculate.UseVisualStyleBackColor = True
        '
        'buttonWriteToExcel
        '
        Me.buttonWriteToExcel.Location = New System.Drawing.Point(6, 19)
        Me.buttonWriteToExcel.Name = "buttonWriteToExcel"
        Me.buttonWriteToExcel.Size = New System.Drawing.Size(280, 27)
        Me.buttonWriteToExcel.TabIndex = 28
        Me.buttonWriteToExcel.Text = "Write to Excel"
        Me.buttonWriteToExcel.UseVisualStyleBackColor = True
        '
        'buttonReloadSettings
        '
        Me.buttonReloadSettings.Location = New System.Drawing.Point(18, 369)
        Me.buttonReloadSettings.Name = "buttonReloadSettings"
        Me.buttonReloadSettings.Size = New System.Drawing.Size(280, 27)
        Me.buttonReloadSettings.TabIndex = 25
        Me.buttonReloadSettings.Text = "Reload Default Settings"
        Me.buttonReloadSettings.UseVisualStyleBackColor = True
        '
        'buttonMathCalculate
        '
        Me.buttonMathCalculate.Location = New System.Drawing.Point(6, 19)
        Me.buttonMathCalculate.Name = "buttonMathCalculate"
        Me.buttonMathCalculate.Size = New System.Drawing.Size(280, 27)
        Me.buttonMathCalculate.TabIndex = 26
        Me.buttonMathCalculate.Text = "Calculation"
        Me.buttonMathCalculate.UseVisualStyleBackColor = True
        '
        'buttonElementRollUp
        '
        Me.buttonElementRollUp.Location = New System.Drawing.Point(6, 52)
        Me.buttonElementRollUp.Name = "buttonElementRollUp"
        Me.buttonElementRollUp.Size = New System.Drawing.Size(280, 27)
        Me.buttonElementRollUp.TabIndex = 30
        Me.buttonElementRollUp.Text = "Roll Up Value to Parent"
        Me.buttonElementRollUp.UseVisualStyleBackColor = True
        '
        'buttonElementParent
        '
        Me.buttonElementParent.Location = New System.Drawing.Point(6, 19)
        Me.buttonElementParent.Name = "buttonElementParent"
        Me.buttonElementParent.Size = New System.Drawing.Size(280, 27)
        Me.buttonElementParent.TabIndex = 27
        Me.buttonElementParent.Text = "Define Parent-Child Relationship"
        Me.buttonElementParent.UseVisualStyleBackColor = True
        '
        'buttonClose
        '
        Me.buttonClose.Location = New System.Drawing.Point(18, 424)
        Me.buttonClose.Name = "buttonClose"
        Me.buttonClose.Size = New System.Drawing.Size(280, 27)
        Me.buttonClose.TabIndex = 24
        Me.buttonClose.Text = "Close"
        Me.buttonClose.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.buttonStringCalculate)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 12)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(292, 62)
        Me.GroupBox1.TabIndex = 32
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Strings and Text"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.buttonMathCalculate)
        Me.GroupBox2.Location = New System.Drawing.Point(12, 80)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(292, 62)
        Me.GroupBox2.TabIndex = 33
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Math and Calculations"
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.buttonElementParent)
        Me.GroupBox3.Controls.Add(Me.buttonElementRollUp)
        Me.GroupBox3.Location = New System.Drawing.Point(12, 148)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(292, 97)
        Me.GroupBox3.TabIndex = 34
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Elements and Symbols"
        '
        'GroupBox4
        '
        Me.GroupBox4.Controls.Add(Me.buttonWriteToExcel)
        Me.GroupBox4.Controls.Add(Me.buttonScheduleKey)
        Me.GroupBox4.Location = New System.Drawing.Point(12, 251)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.Size = New System.Drawing.Size(292, 96)
        Me.GroupBox4.TabIndex = 35
        Me.GroupBox4.TabStop = False
        Me.GroupBox4.Text = "External Data (Excel and Access)"
        '
        'ButtonSuperCalc
        '
        Me.ButtonSuperCalc.Location = New System.Drawing.Point(349, 99)
        Me.ButtonSuperCalc.Name = "ButtonSuperCalc"
        Me.ButtonSuperCalc.Size = New System.Drawing.Size(280, 27)
        Me.ButtonSuperCalc.TabIndex = 36
        Me.ButtonSuperCalc.Text = "*NEW Super Calculation"
        Me.ButtonSuperCalc.UseVisualStyleBackColor = True
        '
        'form_ParamMenu
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(318, 464)
        Me.Controls.Add(Me.ButtonSuperCalc)
        Me.Controls.Add(Me.GroupBox4)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.buttonReloadSettings)
        Me.Controls.Add(Me.buttonClose)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(678, 502)
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(334, 502)
        Me.Name = "form_ParamMenu"
        Me.Text = "Parameter Tools"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox4.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents buttonScheduleKey As System.Windows.Forms.Button
    Private WithEvents buttonStringCalculate As System.Windows.Forms.Button
    Private WithEvents buttonWriteToExcel As System.Windows.Forms.Button
    Private WithEvents buttonReloadSettings As System.Windows.Forms.Button
    Private WithEvents buttonMathCalculate As System.Windows.Forms.Button
    Private WithEvents buttonElementRollUp As System.Windows.Forms.Button
    Private WithEvents buttonElementParent As System.Windows.Forms.Button
    Private WithEvents buttonClose As System.Windows.Forms.Button
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Private WithEvents ButtonSuperCalc As System.Windows.Forms.Button
End Class
