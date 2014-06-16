<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class form_ParameterCalculation
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(form_ParameterCalculation))
        Me.ButtonOk = New System.Windows.Forms.Button()
        Me.ButtonClose = New System.Windows.Forms.Button()
        Me.groupBoxElementsToProcess = New System.Windows.Forms.GroupBox()
        Me.radioButtonProcessSelection = New System.Windows.Forms.RadioButton()
        Me.radioButtonProcessAll = New System.Windows.Forms.RadioButton()
        Me.label1 = New System.Windows.Forms.Label()
        Me.comboBoxCategory = New System.Windows.Forms.ComboBox()
        Me.DataGridViewResults = New System.Windows.Forms.DataGridView()
        Me.DataGridViewParameters = New System.Windows.Forms.DataGridView()
        Me.TextBoxFormula = New System.Windows.Forms.TextBox()
        Me.LabelParametersAdded = New System.Windows.Forms.Label()
        Me.ButtonSaved1 = New System.Windows.Forms.Button()
        Me.ButtonSaved2 = New System.Windows.Forms.Button()
        Me.ButtonSaved3 = New System.Windows.Forms.Button()
        Me.ButtonSaved4 = New System.Windows.Forms.Button()
        Me.ButtonSaved5 = New System.Windows.Forms.Button()
        Me.LabelSavedItems = New System.Windows.Forms.Label()
        Me.ProgressBarMain = New System.Windows.Forms.ProgressBar()
        Me.ButtonHelp = New System.Windows.Forms.Button()
        Me.TextBoxResultsParameter = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.TextBoxGroupByParameter = New System.Windows.Forms.TextBox()
        Me.BottomToolStripPanel = New System.Windows.Forms.ToolStripPanel()
        Me.TopToolStripPanel = New System.Windows.Forms.ToolStripPanel()
        Me.RightToolStripPanel = New System.Windows.Forms.ToolStripPanel()
        Me.LeftToolStripPanel = New System.Windows.Forms.ToolStripPanel()
        Me.ContentPanel = New System.Windows.Forms.ToolStripContentPanel()
        Me.groupBoxElementsToProcess.SuspendLayout()
        CType(Me.DataGridViewResults, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.DataGridViewParameters, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'ButtonOk
        '
        Me.ButtonOk.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonOk.Location = New System.Drawing.Point(600, 444)
        Me.ButtonOk.Name = "ButtonOk"
        Me.ButtonOk.Size = New System.Drawing.Size(140, 56)
        Me.ButtonOk.TabIndex = 56
        Me.ButtonOk.Text = "Preview Results"
        Me.ButtonOk.UseVisualStyleBackColor = True
        '
        'ButtonClose
        '
        Me.ButtonClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonClose.Location = New System.Drawing.Point(746, 444)
        Me.ButtonClose.Name = "ButtonClose"
        Me.ButtonClose.Size = New System.Drawing.Size(139, 56)
        Me.ButtonClose.TabIndex = 55
        Me.ButtonClose.Text = "Close"
        Me.ButtonClose.UseVisualStyleBackColor = True
        '
        'groupBoxElementsToProcess
        '
        Me.groupBoxElementsToProcess.Controls.Add(Me.radioButtonProcessSelection)
        Me.groupBoxElementsToProcess.Controls.Add(Me.radioButtonProcessAll)
        Me.groupBoxElementsToProcess.Controls.Add(Me.label1)
        Me.groupBoxElementsToProcess.Controls.Add(Me.comboBoxCategory)
        Me.groupBoxElementsToProcess.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBoxElementsToProcess.Location = New System.Drawing.Point(12, 12)
        Me.groupBoxElementsToProcess.Name = "groupBoxElementsToProcess"
        Me.groupBoxElementsToProcess.Size = New System.Drawing.Size(582, 101)
        Me.groupBoxElementsToProcess.TabIndex = 57
        Me.groupBoxElementsToProcess.TabStop = False
        Me.groupBoxElementsToProcess.Text = "Elements to Process"
        '
        'radioButtonProcessSelection
        '
        Me.radioButtonProcessSelection.AutoSize = True
        Me.radioButtonProcessSelection.Location = New System.Drawing.Point(304, 61)
        Me.radioButtonProcessSelection.Name = "radioButtonProcessSelection"
        Me.radioButtonProcessSelection.Size = New System.Drawing.Size(205, 17)
        Me.radioButtonProcessSelection.TabIndex = 1
        Me.radioButtonProcessSelection.Text = "Selected Elements Matching Category"
        Me.radioButtonProcessSelection.UseVisualStyleBackColor = True
        '
        'radioButtonProcessAll
        '
        Me.radioButtonProcessAll.AutoSize = True
        Me.radioButtonProcessAll.Checked = True
        Me.radioButtonProcessAll.Location = New System.Drawing.Point(304, 23)
        Me.radioButtonProcessAll.Name = "radioButtonProcessAll"
        Me.radioButtonProcessAll.Size = New System.Drawing.Size(174, 17)
        Me.radioButtonProcessAll.TabIndex = 0
        Me.radioButtonProcessAll.TabStop = True
        Me.radioButtonProcessAll.Text = "All Elements Matching Category"
        Me.radioButtonProcessAll.UseVisualStyleBackColor = True
        '
        'label1
        '
        Me.label1.AutoSize = True
        Me.label1.Location = New System.Drawing.Point(14, 22)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(49, 13)
        Me.label1.TabIndex = 51
        Me.label1.Text = "Category"
        '
        'comboBoxCategory
        '
        Me.comboBoxCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.comboBoxCategory.FormattingEnabled = True
        Me.comboBoxCategory.Location = New System.Drawing.Point(16, 39)
        Me.comboBoxCategory.Name = "comboBoxCategory"
        Me.comboBoxCategory.Size = New System.Drawing.Size(250, 21)
        Me.comboBoxCategory.TabIndex = 50
        '
        'DataGridViewResults
        '
        Me.DataGridViewResults.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.DataGridViewResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridViewResults.Location = New System.Drawing.Point(12, 297)
        Me.DataGridViewResults.Name = "DataGridViewResults"
        Me.DataGridViewResults.Size = New System.Drawing.Size(873, 141)
        Me.DataGridViewResults.TabIndex = 58
        '
        'DataGridViewParameters
        '
        Me.DataGridViewParameters.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.DataGridViewParameters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridViewParameters.Location = New System.Drawing.Point(600, 35)
        Me.DataGridViewParameters.Name = "DataGridViewParameters"
        Me.DataGridViewParameters.Size = New System.Drawing.Size(285, 256)
        Me.DataGridViewParameters.TabIndex = 59
        '
        'TextBoxFormula
        '
        Me.TextBoxFormula.BackColor = System.Drawing.SystemColors.Info
        Me.TextBoxFormula.Location = New System.Drawing.Point(12, 119)
        Me.TextBoxFormula.Multiline = True
        Me.TextBoxFormula.Name = "TextBoxFormula"
        Me.TextBoxFormula.Size = New System.Drawing.Size(582, 119)
        Me.TextBoxFormula.TabIndex = 52
        Me.TextBoxFormula.Text = "<Enter Formula>"
        '
        'LabelParametersAdded
        '
        Me.LabelParametersAdded.AutoSize = True
        Me.LabelParametersAdded.Location = New System.Drawing.Point(598, 17)
        Me.LabelParametersAdded.Name = "LabelParametersAdded"
        Me.LabelParametersAdded.Size = New System.Drawing.Size(176, 13)
        Me.LabelParametersAdded.TabIndex = 52
        Me.LabelParametersAdded.Text = "Parameters Available for Calculation"
        '
        'ButtonSaved1
        '
        Me.ButtonSaved1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ButtonSaved1.Location = New System.Drawing.Point(15, 461)
        Me.ButtonSaved1.Name = "ButtonSaved1"
        Me.ButtonSaved1.Size = New System.Drawing.Size(29, 22)
        Me.ButtonSaved1.TabIndex = 64
        Me.ButtonSaved1.Text = "1"
        Me.ButtonSaved1.UseVisualStyleBackColor = True
        '
        'ButtonSaved2
        '
        Me.ButtonSaved2.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ButtonSaved2.Location = New System.Drawing.Point(50, 461)
        Me.ButtonSaved2.Name = "ButtonSaved2"
        Me.ButtonSaved2.Size = New System.Drawing.Size(29, 22)
        Me.ButtonSaved2.TabIndex = 65
        Me.ButtonSaved2.Text = "2"
        Me.ButtonSaved2.UseVisualStyleBackColor = True
        '
        'ButtonSaved3
        '
        Me.ButtonSaved3.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ButtonSaved3.Location = New System.Drawing.Point(85, 461)
        Me.ButtonSaved3.Name = "ButtonSaved3"
        Me.ButtonSaved3.Size = New System.Drawing.Size(29, 22)
        Me.ButtonSaved3.TabIndex = 66
        Me.ButtonSaved3.Text = "3"
        Me.ButtonSaved3.UseVisualStyleBackColor = True
        '
        'ButtonSaved4
        '
        Me.ButtonSaved4.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ButtonSaved4.Location = New System.Drawing.Point(120, 461)
        Me.ButtonSaved4.Name = "ButtonSaved4"
        Me.ButtonSaved4.Size = New System.Drawing.Size(29, 22)
        Me.ButtonSaved4.TabIndex = 67
        Me.ButtonSaved4.Text = "4"
        Me.ButtonSaved4.UseVisualStyleBackColor = True
        '
        'ButtonSaved5
        '
        Me.ButtonSaved5.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ButtonSaved5.Location = New System.Drawing.Point(155, 461)
        Me.ButtonSaved5.Name = "ButtonSaved5"
        Me.ButtonSaved5.Size = New System.Drawing.Size(29, 22)
        Me.ButtonSaved5.TabIndex = 68
        Me.ButtonSaved5.Text = "5"
        Me.ButtonSaved5.UseVisualStyleBackColor = True
        '
        'LabelSavedItems
        '
        Me.LabelSavedItems.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LabelSavedItems.AutoSize = True
        Me.LabelSavedItems.Location = New System.Drawing.Point(12, 443)
        Me.LabelSavedItems.Name = "LabelSavedItems"
        Me.LabelSavedItems.Size = New System.Drawing.Size(83, 13)
        Me.LabelSavedItems.TabIndex = 69
        Me.LabelSavedItems.Text = "Saved Formulas"
        '
        'ProgressBarMain
        '
        Me.ProgressBarMain.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ProgressBarMain.Location = New System.Drawing.Point(12, 443)
        Me.ProgressBarMain.Name = "ProgressBarMain"
        Me.ProgressBarMain.Size = New System.Drawing.Size(582, 56)
        Me.ProgressBarMain.TabIndex = 70
        '
        'ButtonHelp
        '
        Me.ButtonHelp.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ButtonHelp.Location = New System.Drawing.Point(316, 443)
        Me.ButtonHelp.Name = "ButtonHelp"
        Me.ButtonHelp.Size = New System.Drawing.Size(140, 56)
        Me.ButtonHelp.TabIndex = 71
        Me.ButtonHelp.Text = "Formula Help"
        Me.ButtonHelp.UseVisualStyleBackColor = True
        '
        'TextBoxResultsParameter
        '
        Me.TextBoxResultsParameter.BackColor = System.Drawing.Color.MistyRose
        Me.TextBoxResultsParameter.Location = New System.Drawing.Point(316, 271)
        Me.TextBoxResultsParameter.Name = "TextBoxResultsParameter"
        Me.TextBoxResultsParameter.Size = New System.Drawing.Size(250, 20)
        Me.TextBoxResultsParameter.TabIndex = 72
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(313, 252)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(136, 13)
        Me.Label3.TabIndex = 73
        Me.Label3.Text = "Send Results to Parameter:"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(26, 252)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(115, 13)
        Me.Label4.TabIndex = 75
        Me.Label4.Text = """Group By"" Parameter:"
        '
        'TextBoxGroupByParameter
        '
        Me.TextBoxGroupByParameter.BackColor = System.Drawing.SystemColors.Info
        Me.TextBoxGroupByParameter.Location = New System.Drawing.Point(29, 271)
        Me.TextBoxGroupByParameter.Name = "TextBoxGroupByParameter"
        Me.TextBoxGroupByParameter.Size = New System.Drawing.Size(249, 20)
        Me.TextBoxGroupByParameter.TabIndex = 74
        '
        'BottomToolStripPanel
        '
        Me.BottomToolStripPanel.Location = New System.Drawing.Point(0, 0)
        Me.BottomToolStripPanel.Name = "BottomToolStripPanel"
        Me.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal
        Me.BottomToolStripPanel.RowMargin = New System.Windows.Forms.Padding(3, 0, 0, 0)
        Me.BottomToolStripPanel.Size = New System.Drawing.Size(0, 0)
        '
        'TopToolStripPanel
        '
        Me.TopToolStripPanel.Location = New System.Drawing.Point(0, 0)
        Me.TopToolStripPanel.Name = "TopToolStripPanel"
        Me.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal
        Me.TopToolStripPanel.RowMargin = New System.Windows.Forms.Padding(3, 0, 0, 0)
        Me.TopToolStripPanel.Size = New System.Drawing.Size(0, 0)
        '
        'RightToolStripPanel
        '
        Me.RightToolStripPanel.Location = New System.Drawing.Point(0, 0)
        Me.RightToolStripPanel.Name = "RightToolStripPanel"
        Me.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal
        Me.RightToolStripPanel.RowMargin = New System.Windows.Forms.Padding(3, 0, 0, 0)
        Me.RightToolStripPanel.Size = New System.Drawing.Size(0, 0)
        '
        'LeftToolStripPanel
        '
        Me.LeftToolStripPanel.Location = New System.Drawing.Point(0, 0)
        Me.LeftToolStripPanel.Name = "LeftToolStripPanel"
        Me.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal
        Me.LeftToolStripPanel.RowMargin = New System.Windows.Forms.Padding(3, 0, 0, 0)
        Me.LeftToolStripPanel.Size = New System.Drawing.Size(0, 0)
        '
        'ContentPanel
        '
        Me.ContentPanel.Size = New System.Drawing.Size(150, 150)
        '
        'form_ParameterCalculation
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(897, 512)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.TextBoxGroupByParameter)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.TextBoxResultsParameter)
        Me.Controls.Add(Me.ButtonHelp)
        Me.Controls.Add(Me.LabelSavedItems)
        Me.Controls.Add(Me.ButtonSaved5)
        Me.Controls.Add(Me.ButtonSaved4)
        Me.Controls.Add(Me.ButtonSaved3)
        Me.Controls.Add(Me.ButtonSaved2)
        Me.Controls.Add(Me.ButtonSaved1)
        Me.Controls.Add(Me.LabelParametersAdded)
        Me.Controls.Add(Me.TextBoxFormula)
        Me.Controls.Add(Me.DataGridViewParameters)
        Me.Controls.Add(Me.DataGridViewResults)
        Me.Controls.Add(Me.groupBoxElementsToProcess)
        Me.Controls.Add(Me.ButtonOk)
        Me.Controls.Add(Me.ButtonClose)
        Me.Controls.Add(Me.ProgressBarMain)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(913, 550)
        Me.Name = "form_ParameterCalculation"
        Me.Text = "Parameter Calculation"
        Me.groupBoxElementsToProcess.ResumeLayout(False)
        Me.groupBoxElementsToProcess.PerformLayout()
        CType(Me.DataGridViewResults, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.DataGridViewParameters, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents ButtonOk As System.Windows.Forms.Button
    Private WithEvents ButtonClose As System.Windows.Forms.Button
    Private WithEvents groupBoxElementsToProcess As System.Windows.Forms.GroupBox
    Private WithEvents radioButtonProcessSelection As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonProcessAll As System.Windows.Forms.RadioButton
    Private WithEvents label1 As System.Windows.Forms.Label
    Private WithEvents comboBoxCategory As System.Windows.Forms.ComboBox
    Friend WithEvents DataGridViewResults As System.Windows.Forms.DataGridView
    Friend WithEvents DataGridViewParameters As System.Windows.Forms.DataGridView
    Friend WithEvents TextBoxFormula As System.Windows.Forms.TextBox
    Private WithEvents LabelParametersAdded As System.Windows.Forms.Label
    Private WithEvents ButtonSaved1 As System.Windows.Forms.Button
    Private WithEvents ButtonSaved2 As System.Windows.Forms.Button
    Private WithEvents ButtonSaved3 As System.Windows.Forms.Button
    Private WithEvents ButtonSaved4 As System.Windows.Forms.Button
    Private WithEvents ButtonSaved5 As System.Windows.Forms.Button
    Private WithEvents LabelSavedItems As System.Windows.Forms.Label
    Friend WithEvents ProgressBarMain As System.Windows.Forms.ProgressBar
    Private WithEvents ButtonHelp As System.Windows.Forms.Button
    Friend WithEvents TextBoxResultsParameter As System.Windows.Forms.TextBox
    Private WithEvents Label3 As System.Windows.Forms.Label
    Private WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents TextBoxGroupByParameter As System.Windows.Forms.TextBox
    Friend WithEvents BottomToolStripPanel As System.Windows.Forms.ToolStripPanel
    Friend WithEvents TopToolStripPanel As System.Windows.Forms.ToolStripPanel
    Friend WithEvents RightToolStripPanel As System.Windows.Forms.ToolStripPanel
    Friend WithEvents LeftToolStripPanel As System.Windows.Forms.ToolStripPanel
    Friend WithEvents ContentPanel As System.Windows.Forms.ToolStripContentPanel
End Class
