<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class form_ParamRollUp
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(form_ParamRollUp))
        Me.groupBox2 = New System.Windows.Forms.GroupBox()
        Me.textBoxParamSource = New System.Windows.Forms.TextBox()
        Me.label6 = New System.Windows.Forms.Label()
        Me.textBoxParamChildKey = New System.Windows.Forms.TextBox()
        Me.comboBoxCatChild = New System.Windows.Forms.ComboBox()
        Me.label2 = New System.Windows.Forms.Label()
        Me.label4 = New System.Windows.Forms.Label()
        Me.groupBox1 = New System.Windows.Forms.GroupBox()
        Me.textBoxParamRollUp = New System.Windows.Forms.TextBox()
        Me.textBoxParamParentKey = New System.Windows.Forms.TextBox()
        Me.comboBoxCatParent = New System.Windows.Forms.ComboBox()
        Me.label1 = New System.Windows.Forms.Label()
        Me.label5 = New System.Windows.Forms.Label()
        Me.label3 = New System.Windows.Forms.Label()
        Me.buttonProcess = New System.Windows.Forms.Button()
        Me.buttonClose = New System.Windows.Forms.Button()
        Me.groupBox2.SuspendLayout()
        Me.groupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'groupBox2
        '
        Me.groupBox2.Controls.Add(Me.textBoxParamSource)
        Me.groupBox2.Controls.Add(Me.label6)
        Me.groupBox2.Controls.Add(Me.textBoxParamChildKey)
        Me.groupBox2.Controls.Add(Me.comboBoxCatChild)
        Me.groupBox2.Controls.Add(Me.label2)
        Me.groupBox2.Controls.Add(Me.label4)
        Me.groupBox2.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBox2.Location = New System.Drawing.Point(323, 12)
        Me.groupBox2.Name = "groupBox2"
        Me.groupBox2.Size = New System.Drawing.Size(301, 219)
        Me.groupBox2.TabIndex = 17
        Me.groupBox2.TabStop = False
        Me.groupBox2.Text = "Child Element Settings"
        '
        'textBoxParamSource
        '
        Me.textBoxParamSource.Location = New System.Drawing.Point(14, 163)
        Me.textBoxParamSource.Name = "textBoxParamSource"
        Me.textBoxParamSource.Size = New System.Drawing.Size(275, 20)
        Me.textBoxParamSource.TabIndex = 6
        '
        'label6
        '
        Me.label6.AutoSize = True
        Me.label6.Location = New System.Drawing.Point(11, 147)
        Me.label6.Name = "label6"
        Me.label6.Size = New System.Drawing.Size(153, 13)
        Me.label6.TabIndex = 7
        Me.label6.Text = "Source Value Parameter Name"
        '
        'textBoxParamChildKey
        '
        Me.textBoxParamChildKey.Location = New System.Drawing.Point(14, 105)
        Me.textBoxParamChildKey.Name = "textBoxParamChildKey"
        Me.textBoxParamChildKey.Size = New System.Drawing.Size(275, 20)
        Me.textBoxParamChildKey.TabIndex = 4
        '
        'comboBoxCatChild
        '
        Me.comboBoxCatChild.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.comboBoxCatChild.FormattingEnabled = True
        Me.comboBoxCatChild.Location = New System.Drawing.Point(14, 49)
        Me.comboBoxCatChild.Name = "comboBoxCatChild"
        Me.comboBoxCatChild.Size = New System.Drawing.Size(275, 21)
        Me.comboBoxCatChild.TabIndex = 2
        '
        'label2
        '
        Me.label2.AutoSize = True
        Me.label2.Location = New System.Drawing.Point(11, 33)
        Me.label2.Name = "label2"
        Me.label2.Size = New System.Drawing.Size(72, 13)
        Me.label2.TabIndex = 3
        Me.label2.Text = "Element Type"
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
        Me.groupBox1.Controls.Add(Me.textBoxParamRollUp)
        Me.groupBox1.Controls.Add(Me.textBoxParamParentKey)
        Me.groupBox1.Controls.Add(Me.comboBoxCatParent)
        Me.groupBox1.Controls.Add(Me.label1)
        Me.groupBox1.Controls.Add(Me.label5)
        Me.groupBox1.Controls.Add(Me.label3)
        Me.groupBox1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBox1.Location = New System.Drawing.Point(12, 12)
        Me.groupBox1.Name = "groupBox1"
        Me.groupBox1.Size = New System.Drawing.Size(305, 219)
        Me.groupBox1.TabIndex = 16
        Me.groupBox1.TabStop = False
        Me.groupBox1.Text = "Parent Element Settings"
        '
        'textBoxParamRollUp
        '
        Me.textBoxParamRollUp.Location = New System.Drawing.Point(17, 163)
        Me.textBoxParamRollUp.Name = "textBoxParamRollUp"
        Me.textBoxParamRollUp.Size = New System.Drawing.Size(275, 20)
        Me.textBoxParamRollUp.TabIndex = 4
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
        Me.label1.Size = New System.Drawing.Size(72, 13)
        Me.label1.TabIndex = 1
        Me.label1.Text = "Element Type"
        '
        'label5
        '
        Me.label5.AutoSize = True
        Me.label5.Location = New System.Drawing.Point(14, 147)
        Me.label5.Name = "label5"
        Me.label5.Size = New System.Drawing.Size(154, 13)
        Me.label5.TabIndex = 5
        Me.label5.Text = "Roll Up Value Parameter Name"
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
        Me.buttonProcess.Location = New System.Drawing.Point(337, 237)
        Me.buttonProcess.Name = "buttonProcess"
        Me.buttonProcess.Size = New System.Drawing.Size(114, 31)
        Me.buttonProcess.TabIndex = 19
        Me.buttonProcess.Text = "Process Elements"
        Me.buttonProcess.UseVisualStyleBackColor = True
        '
        'buttonClose
        '
        Me.buttonClose.Location = New System.Drawing.Point(498, 237)
        Me.buttonClose.Name = "buttonClose"
        Me.buttonClose.Size = New System.Drawing.Size(114, 31)
        Me.buttonClose.TabIndex = 18
        Me.buttonClose.Text = "Close"
        Me.buttonClose.UseVisualStyleBackColor = True
        '
        'form_ParamRollUp
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(637, 281)
        Me.Controls.Add(Me.buttonProcess)
        Me.Controls.Add(Me.buttonClose)
        Me.Controls.Add(Me.groupBox2)
        Me.Controls.Add(Me.groupBox1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(653, 319)
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(653, 319)
        Me.Name = "form_ParamRollUp"
        Me.Text = "Roll Up Element Values"
        Me.groupBox2.ResumeLayout(False)
        Me.groupBox2.PerformLayout()
        Me.groupBox1.ResumeLayout(False)
        Me.groupBox1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents groupBox2 As System.Windows.Forms.GroupBox
    Private WithEvents textBoxParamSource As System.Windows.Forms.TextBox
    Private WithEvents label6 As System.Windows.Forms.Label
    Private WithEvents textBoxParamChildKey As System.Windows.Forms.TextBox
    Private WithEvents comboBoxCatChild As System.Windows.Forms.ComboBox
    Private WithEvents label2 As System.Windows.Forms.Label
    Private WithEvents label4 As System.Windows.Forms.Label
    Private WithEvents groupBox1 As System.Windows.Forms.GroupBox
    Private WithEvents textBoxParamRollUp As System.Windows.Forms.TextBox
    Private WithEvents textBoxParamParentKey As System.Windows.Forms.TextBox
    Private WithEvents comboBoxCatParent As System.Windows.Forms.ComboBox
    Private WithEvents label1 As System.Windows.Forms.Label
    Private WithEvents label5 As System.Windows.Forms.Label
    Private WithEvents label3 As System.Windows.Forms.Label
    Private WithEvents buttonProcess As System.Windows.Forms.Button
    Private WithEvents buttonClose As System.Windows.Forms.Button
End Class
