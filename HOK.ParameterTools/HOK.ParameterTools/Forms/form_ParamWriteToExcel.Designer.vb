<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class form_ParamWriteToExcel
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(form_ParamWriteToExcel))
        Me.groupBoxElementsToProcess = New System.Windows.Forms.GroupBox()
        Me.radioButtonProcessSelection = New System.Windows.Forms.RadioButton()
        Me.radioButtonProcessAll = New System.Windows.Forms.RadioButton()
        Me.LabelCategory = New System.Windows.Forms.Label()
        Me.buttonClose = New System.Windows.Forms.Button()
        Me.buttonWrite = New System.Windows.Forms.Button()
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.CheckBoxInstance = New System.Windows.Forms.CheckBox()
        Me.CheckBoxType = New System.Windows.Forms.CheckBox()
        Me.ComboBoxCategory = New System.Windows.Forms.ComboBox()
        Me.GroupBoxParamScope = New System.Windows.Forms.GroupBox()
        Me.groupBoxElementsToProcess.SuspendLayout()
        Me.GroupBoxParamScope.SuspendLayout()
        Me.SuspendLayout()
        '
        'groupBoxElementsToProcess
        '
        Me.groupBoxElementsToProcess.Controls.Add(Me.radioButtonProcessSelection)
        Me.groupBoxElementsToProcess.Controls.Add(Me.radioButtonProcessAll)
        Me.groupBoxElementsToProcess.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBoxElementsToProcess.Location = New System.Drawing.Point(14, 54)
        Me.groupBoxElementsToProcess.Name = "groupBoxElementsToProcess"
        Me.groupBoxElementsToProcess.Size = New System.Drawing.Size(160, 120)
        Me.groupBoxElementsToProcess.TabIndex = 22
        Me.groupBoxElementsToProcess.TabStop = False
        Me.groupBoxElementsToProcess.Text = "Elements to Process"
        '
        'radioButtonProcessSelection
        '
        Me.radioButtonProcessSelection.AutoSize = True
        Me.radioButtonProcessSelection.Location = New System.Drawing.Point(14, 32)
        Me.radioButtonProcessSelection.Name = "radioButtonProcessSelection"
        Me.radioButtonProcessSelection.Size = New System.Drawing.Size(125, 17)
        Me.radioButtonProcessSelection.TabIndex = 1
        Me.radioButtonProcessSelection.Text = "Pre-selection in Revit"
        Me.radioButtonProcessSelection.UseVisualStyleBackColor = True
        '
        'radioButtonProcessAll
        '
        Me.radioButtonProcessAll.AutoSize = True
        Me.radioButtonProcessAll.Checked = True
        Me.radioButtonProcessAll.Location = New System.Drawing.Point(14, 72)
        Me.radioButtonProcessAll.Name = "radioButtonProcessAll"
        Me.radioButtonProcessAll.Size = New System.Drawing.Size(125, 17)
        Me.radioButtonProcessAll.TabIndex = 0
        Me.radioButtonProcessAll.TabStop = True
        Me.radioButtonProcessAll.Text = "All Elements in Model"
        Me.radioButtonProcessAll.UseVisualStyleBackColor = True
        '
        'LabelCategory
        '
        Me.LabelCategory.AutoSize = True
        Me.LabelCategory.Location = New System.Drawing.Point(12, 9)
        Me.LabelCategory.Name = "LabelCategory"
        Me.LabelCategory.Size = New System.Drawing.Size(90, 13)
        Me.LabelCategory.TabIndex = 21
        Me.LabelCategory.Text = "Element Category"
        '
        'buttonClose
        '
        Me.buttonClose.Location = New System.Drawing.Point(181, 182)
        Me.buttonClose.Name = "buttonClose"
        Me.buttonClose.Size = New System.Drawing.Size(160, 31)
        Me.buttonClose.TabIndex = 16
        Me.buttonClose.Text = "Close"
        Me.buttonClose.UseVisualStyleBackColor = True
        '
        'buttonWrite
        '
        Me.buttonWrite.Location = New System.Drawing.Point(15, 182)
        Me.buttonWrite.Name = "buttonWrite"
        Me.buttonWrite.Size = New System.Drawing.Size(159, 31)
        Me.buttonWrite.TabIndex = 17
        Me.buttonWrite.Text = "Write Data"
        Me.buttonWrite.UseVisualStyleBackColor = True
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(12, 182)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(329, 30)
        Me.ProgressBar1.TabIndex = 23
        '
        'CheckBoxInstance
        '
        Me.CheckBoxInstance.AutoSize = True
        Me.CheckBoxInstance.Location = New System.Drawing.Point(17, 33)
        Me.CheckBoxInstance.Name = "CheckBoxInstance"
        Me.CheckBoxInstance.Size = New System.Drawing.Size(123, 17)
        Me.CheckBoxInstance.TabIndex = 24
        Me.CheckBoxInstance.Text = "Instance Parameters"
        Me.CheckBoxInstance.UseVisualStyleBackColor = True
        '
        'CheckBoxType
        '
        Me.CheckBoxType.AutoSize = True
        Me.CheckBoxType.Location = New System.Drawing.Point(17, 73)
        Me.CheckBoxType.Name = "CheckBoxType"
        Me.CheckBoxType.Size = New System.Drawing.Size(106, 17)
        Me.CheckBoxType.TabIndex = 25
        Me.CheckBoxType.Text = "Type Parameters"
        Me.CheckBoxType.UseVisualStyleBackColor = True
        '
        'ComboBoxCategory
        '
        Me.ComboBoxCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBoxCategory.FormattingEnabled = True
        Me.ComboBoxCategory.Location = New System.Drawing.Point(14, 27)
        Me.ComboBoxCategory.Name = "ComboBoxCategory"
        Me.ComboBoxCategory.Size = New System.Drawing.Size(327, 21)
        Me.ComboBoxCategory.TabIndex = 26
        '
        'GroupBoxParamScope
        '
        Me.GroupBoxParamScope.Controls.Add(Me.CheckBoxInstance)
        Me.GroupBoxParamScope.Controls.Add(Me.CheckBoxType)
        Me.GroupBoxParamScope.Location = New System.Drawing.Point(181, 54)
        Me.GroupBoxParamScope.Name = "GroupBoxParamScope"
        Me.GroupBoxParamScope.Size = New System.Drawing.Size(160, 120)
        Me.GroupBoxParamScope.TabIndex = 27
        Me.GroupBoxParamScope.TabStop = False
        Me.GroupBoxParamScope.Text = "Parameters to Include"
        '
        'form_ParamWriteToExcel
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(359, 227)
        Me.Controls.Add(Me.GroupBoxParamScope)
        Me.Controls.Add(Me.ComboBoxCategory)
        Me.Controls.Add(Me.groupBoxElementsToProcess)
        Me.Controls.Add(Me.LabelCategory)
        Me.Controls.Add(Me.buttonClose)
        Me.Controls.Add(Me.buttonWrite)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(375, 265)
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(375, 265)
        Me.Name = "form_ParamWriteToExcel"
        Me.Text = "Write to Excel"
        Me.groupBoxElementsToProcess.ResumeLayout(False)
        Me.groupBoxElementsToProcess.PerformLayout()
        Me.GroupBoxParamScope.ResumeLayout(False)
        Me.GroupBoxParamScope.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents groupBoxElementsToProcess As System.Windows.Forms.GroupBox
    Private WithEvents radioButtonProcessSelection As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonProcessAll As System.Windows.Forms.RadioButton
    Private WithEvents LabelCategory As System.Windows.Forms.Label
    Private WithEvents buttonClose As System.Windows.Forms.Button
    Private WithEvents buttonWrite As System.Windows.Forms.Button
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar
    Friend WithEvents CheckBoxInstance As System.Windows.Forms.CheckBox
    Friend WithEvents CheckBoxType As System.Windows.Forms.CheckBox
    Friend WithEvents ComboBoxCategory As System.Windows.Forms.ComboBox
    Friend WithEvents GroupBoxParamScope As System.Windows.Forms.GroupBox
End Class
