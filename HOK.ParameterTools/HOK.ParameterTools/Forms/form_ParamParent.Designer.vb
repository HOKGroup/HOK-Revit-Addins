<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class form_ParamParent
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(form_ParamParent))
        Me.groupBox2 = New System.Windows.Forms.GroupBox()
        Me.textBoxParamChildKey = New System.Windows.Forms.TextBox()
        Me.comboBoxCatChild = New System.Windows.Forms.ComboBox()
        Me.label2 = New System.Windows.Forms.Label()
        Me.label4 = New System.Windows.Forms.Label()
        Me.groupBox1 = New System.Windows.Forms.GroupBox()
        Me.textBoxParamParentKey = New System.Windows.Forms.TextBox()
        Me.comboBoxCatParent = New System.Windows.Forms.ComboBox()
        Me.label1 = New System.Windows.Forms.Label()
        Me.label3 = New System.Windows.Forms.Label()
        Me.buttonProcess = New System.Windows.Forms.Button()
        Me.buttonClose = New System.Windows.Forms.Button()
        Me.groupBox2.SuspendLayout()
        Me.groupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'groupBox2
        '
        Me.groupBox2.Controls.Add(Me.textBoxParamChildKey)
        Me.groupBox2.Controls.Add(Me.comboBoxCatChild)
        Me.groupBox2.Controls.Add(Me.label2)
        Me.groupBox2.Controls.Add(Me.label4)
        Me.groupBox2.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBox2.Location = New System.Drawing.Point(12, 174)
        Me.groupBox2.Name = "groupBox2"
        Me.groupBox2.Size = New System.Drawing.Size(305, 162)
        Me.groupBox2.TabIndex = 19
        Me.groupBox2.TabStop = False
        Me.groupBox2.Text = "Child"
        '
        'textBoxParamChildKey
        '
        Me.textBoxParamChildKey.Location = New System.Drawing.Point(14, 105)
        Me.textBoxParamChildKey.Name = "textBoxParamChildKey"
        Me.textBoxParamChildKey.Size = New System.Drawing.Size(278, 20)
        Me.textBoxParamChildKey.TabIndex = 4
        '
        'comboBoxCatChild
        '
        Me.comboBoxCatChild.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.comboBoxCatChild.FormattingEnabled = True
        Me.comboBoxCatChild.Location = New System.Drawing.Point(14, 49)
        Me.comboBoxCatChild.Name = "comboBoxCatChild"
        Me.comboBoxCatChild.Size = New System.Drawing.Size(278, 21)
        Me.comboBoxCatChild.TabIndex = 2
        '
        'label2
        '
        Me.label2.AutoSize = True
        Me.label2.Location = New System.Drawing.Point(11, 33)
        Me.label2.Name = "label2"
        Me.label2.Size = New System.Drawing.Size(90, 13)
        Me.label2.TabIndex = 3
        Me.label2.Text = "Element Category"
        '
        'label4
        '
        Me.label4.AutoSize = True
        Me.label4.Location = New System.Drawing.Point(11, 89)
        Me.label4.Name = "label4"
        Me.label4.Size = New System.Drawing.Size(107, 13)
        Me.label4.TabIndex = 5
        Me.label4.Text = "Key Parameter Name"
        '
        'groupBox1
        '
        Me.groupBox1.Controls.Add(Me.textBoxParamParentKey)
        Me.groupBox1.Controls.Add(Me.comboBoxCatParent)
        Me.groupBox1.Controls.Add(Me.label1)
        Me.groupBox1.Controls.Add(Me.label3)
        Me.groupBox1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBox1.Location = New System.Drawing.Point(12, 12)
        Me.groupBox1.Name = "groupBox1"
        Me.groupBox1.Size = New System.Drawing.Size(305, 156)
        Me.groupBox1.TabIndex = 18
        Me.groupBox1.TabStop = False
        Me.groupBox1.Text = "Parent"
        '
        'textBoxParamParentKey
        '
        Me.textBoxParamParentKey.Location = New System.Drawing.Point(17, 105)
        Me.textBoxParamParentKey.Name = "textBoxParamParentKey"
        Me.textBoxParamParentKey.Size = New System.Drawing.Size(275, 20)
        Me.textBoxParamParentKey.TabIndex = 2
        '
        'comboBoxCatParent
        '
        Me.comboBoxCatParent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.comboBoxCatParent.FormattingEnabled = True
        Me.comboBoxCatParent.Location = New System.Drawing.Point(17, 49)
        Me.comboBoxCatParent.Name = "comboBoxCatParent"
        Me.comboBoxCatParent.Size = New System.Drawing.Size(275, 21)
        Me.comboBoxCatParent.TabIndex = 0
        '
        'label1
        '
        Me.label1.AutoSize = True
        Me.label1.Location = New System.Drawing.Point(14, 33)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(90, 13)
        Me.label1.TabIndex = 1
        Me.label1.Text = "Element Category"
        '
        'label3
        '
        Me.label3.AutoSize = True
        Me.label3.Location = New System.Drawing.Point(14, 89)
        Me.label3.Name = "label3"
        Me.label3.Size = New System.Drawing.Size(107, 13)
        Me.label3.TabIndex = 3
        Me.label3.Text = "Key Parameter Name"
        '
        'buttonProcess
        '
        Me.buttonProcess.Location = New System.Drawing.Point(26, 342)
        Me.buttonProcess.Name = "buttonProcess"
        Me.buttonProcess.Size = New System.Drawing.Size(120, 31)
        Me.buttonProcess.TabIndex = 21
        Me.buttonProcess.Text = "Process Elements"
        Me.buttonProcess.UseVisualStyleBackColor = True
        '
        'buttonClose
        '
        Me.buttonClose.Location = New System.Drawing.Point(184, 342)
        Me.buttonClose.Name = "buttonClose"
        Me.buttonClose.Size = New System.Drawing.Size(120, 31)
        Me.buttonClose.TabIndex = 20
        Me.buttonClose.Text = "Close"
        Me.buttonClose.UseVisualStyleBackColor = True
        '
        'form_ParamParent
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(328, 386)
        Me.Controls.Add(Me.buttonProcess)
        Me.Controls.Add(Me.buttonClose)
        Me.Controls.Add(Me.groupBox2)
        Me.Controls.Add(Me.groupBox1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(344, 424)
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(344, 424)
        Me.Name = "form_ParamParent"
        Me.Text = "Define Parent-Child Relationship"
        Me.groupBox2.ResumeLayout(False)
        Me.groupBox2.PerformLayout()
        Me.groupBox1.ResumeLayout(False)
        Me.groupBox1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents groupBox2 As System.Windows.Forms.GroupBox
    Private WithEvents textBoxParamChildKey As System.Windows.Forms.TextBox
    Private WithEvents comboBoxCatChild As System.Windows.Forms.ComboBox
    Private WithEvents label2 As System.Windows.Forms.Label
    Private WithEvents label4 As System.Windows.Forms.Label
    Private WithEvents groupBox1 As System.Windows.Forms.GroupBox
    Private WithEvents textBoxParamParentKey As System.Windows.Forms.TextBox
    Private WithEvents comboBoxCatParent As System.Windows.Forms.ComboBox
    Private WithEvents label1 As System.Windows.Forms.Label
    Private WithEvents label3 As System.Windows.Forms.Label
    Private WithEvents buttonProcess As System.Windows.Forms.Button
    Private WithEvents buttonClose As System.Windows.Forms.Button
End Class
