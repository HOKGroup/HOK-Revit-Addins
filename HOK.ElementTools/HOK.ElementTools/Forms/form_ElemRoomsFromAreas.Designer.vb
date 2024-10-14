<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class form_ElemRoomsFromAreas
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(form_ElemRoomsFromAreas))
        Me.radioButtonPlaced = New System.Windows.Forms.RadioButton()
        Me.groupBox1 = New System.Windows.Forms.GroupBox()
        Me.checkBoxListReverse = New System.Windows.Forms.CheckBox()
        Me.groupBoxPlaced = New System.Windows.Forms.GroupBox()
        Me.radioButtonBoth = New System.Windows.Forms.RadioButton()
        Me.radioButtonNotPlaced = New System.Windows.Forms.RadioButton()
        Me.textBoxPad2 = New System.Windows.Forms.TextBox()
        Me.textBoxPad1 = New System.Windows.Forms.TextBox()
        Me.textBoxParameterList1 = New System.Windows.Forms.TextBox()
        Me.label7 = New System.Windows.Forms.Label()
        Me.textBoxParameterList2 = New System.Windows.Forms.TextBox()
        Me.label8 = New System.Windows.Forms.Label()
        Me.checkBoxPad1 = New System.Windows.Forms.CheckBox()
        Me.checkBoxPad2 = New System.Windows.Forms.CheckBox()
        Me.groupBox2 = New System.Windows.Forms.GroupBox()
        Me.buttonCreate = New System.Windows.Forms.Button()
        Me.label4 = New System.Windows.Forms.Label()
        Me.buttonClose = New System.Windows.Forms.Button()
        Me.listBoxAreas = New System.Windows.Forms.ListBox()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.ToolStripProgressBar1 = New System.Windows.Forms.ToolStripProgressBar()
        Me.ToolStripStatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.groupBox1.SuspendLayout()
        Me.groupBoxPlaced.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'radioButtonPlaced
        '
        Me.radioButtonPlaced.AutoSize = True
        Me.radioButtonPlaced.Location = New System.Drawing.Point(11, 15)
        Me.radioButtonPlaced.Name = "radioButtonPlaced"
        Me.radioButtonPlaced.Size = New System.Drawing.Size(58, 17)
        Me.radioButtonPlaced.TabIndex = 0
        Me.radioButtonPlaced.TabStop = True
        Me.radioButtonPlaced.Text = "Placed"
        Me.radioButtonPlaced.UseVisualStyleBackColor = True
        '
        'groupBox1
        '
        Me.groupBox1.Controls.Add(Me.checkBoxListReverse)
        Me.groupBox1.Controls.Add(Me.groupBoxPlaced)
        Me.groupBox1.Controls.Add(Me.textBoxPad2)
        Me.groupBox1.Controls.Add(Me.textBoxPad1)
        Me.groupBox1.Controls.Add(Me.textBoxParameterList1)
        Me.groupBox1.Controls.Add(Me.label7)
        Me.groupBox1.Controls.Add(Me.textBoxParameterList2)
        Me.groupBox1.Controls.Add(Me.label8)
        Me.groupBox1.Controls.Add(Me.checkBoxPad1)
        Me.groupBox1.Controls.Add(Me.checkBoxPad2)
        Me.groupBox1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBox1.Location = New System.Drawing.Point(326, 18)
        Me.groupBox1.Name = "groupBox1"
        Me.groupBox1.Size = New System.Drawing.Size(178, 289)
        Me.groupBox1.TabIndex = 57
        Me.groupBox1.TabStop = False
        Me.groupBox1.Text = "List Selection Options"
        '
        'checkBoxListReverse
        '
        Me.checkBoxListReverse.AutoSize = True
        Me.checkBoxListReverse.Location = New System.Drawing.Point(15, 254)
        Me.checkBoxListReverse.Name = "checkBoxListReverse"
        Me.checkBoxListReverse.Size = New System.Drawing.Size(114, 17)
        Me.checkBoxListReverse.TabIndex = 49
        Me.checkBoxListReverse.Text = "Reverse List Order"
        Me.checkBoxListReverse.UseVisualStyleBackColor = True
        '
        'groupBoxPlaced
        '
        Me.groupBoxPlaced.Controls.Add(Me.radioButtonBoth)
        Me.groupBoxPlaced.Controls.Add(Me.radioButtonNotPlaced)
        Me.groupBoxPlaced.Controls.Add(Me.radioButtonPlaced)
        Me.groupBoxPlaced.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBoxPlaced.Location = New System.Drawing.Point(14, 19)
        Me.groupBoxPlaced.Name = "groupBoxPlaced"
        Me.groupBoxPlaced.Size = New System.Drawing.Size(150, 82)
        Me.groupBoxPlaced.TabIndex = 50
        Me.groupBoxPlaced.TabStop = False
        Me.groupBoxPlaced.Text = "Include"
        '
        'radioButtonBoth
        '
        Me.radioButtonBoth.AutoSize = True
        Me.radioButtonBoth.Location = New System.Drawing.Point(11, 56)
        Me.radioButtonBoth.Name = "radioButtonBoth"
        Me.radioButtonBoth.Size = New System.Drawing.Size(47, 17)
        Me.radioButtonBoth.TabIndex = 2
        Me.radioButtonBoth.TabStop = True
        Me.radioButtonBoth.Text = "Both"
        Me.radioButtonBoth.UseVisualStyleBackColor = True
        '
        'radioButtonNotPlaced
        '
        Me.radioButtonNotPlaced.AutoSize = True
        Me.radioButtonNotPlaced.Location = New System.Drawing.Point(11, 35)
        Me.radioButtonNotPlaced.Name = "radioButtonNotPlaced"
        Me.radioButtonNotPlaced.Size = New System.Drawing.Size(78, 17)
        Me.radioButtonNotPlaced.TabIndex = 1
        Me.radioButtonNotPlaced.TabStop = True
        Me.radioButtonNotPlaced.Text = "Not Placed"
        Me.radioButtonNotPlaced.UseVisualStyleBackColor = True
        '
        'textBoxPad2
        '
        Me.textBoxPad2.Location = New System.Drawing.Point(134, 215)
        Me.textBoxPad2.Name = "textBoxPad2"
        Me.textBoxPad2.Size = New System.Drawing.Size(31, 20)
        Me.textBoxPad2.TabIndex = 48
        '
        'textBoxPad1
        '
        Me.textBoxPad1.Location = New System.Drawing.Point(134, 147)
        Me.textBoxPad1.Name = "textBoxPad1"
        Me.textBoxPad1.Size = New System.Drawing.Size(31, 20)
        Me.textBoxPad1.TabIndex = 47
        '
        'textBoxParameterList1
        '
        Me.textBoxParameterList1.Location = New System.Drawing.Point(15, 125)
        Me.textBoxParameterList1.Name = "textBoxParameterList1"
        Me.textBoxParameterList1.Size = New System.Drawing.Size(151, 20)
        Me.textBoxParameterList1.TabIndex = 39
        '
        'label7
        '
        Me.label7.AutoSize = True
        Me.label7.Location = New System.Drawing.Point(15, 110)
        Me.label7.Name = "label7"
        Me.label7.Size = New System.Drawing.Size(86, 13)
        Me.label7.TabIndex = 40
        Me.label7.Text = "List Parameter 1:"
        '
        'textBoxParameterList2
        '
        Me.textBoxParameterList2.Location = New System.Drawing.Point(15, 193)
        Me.textBoxParameterList2.Name = "textBoxParameterList2"
        Me.textBoxParameterList2.Size = New System.Drawing.Size(151, 20)
        Me.textBoxParameterList2.TabIndex = 41
        '
        'label8
        '
        Me.label8.AutoSize = True
        Me.label8.Location = New System.Drawing.Point(12, 178)
        Me.label8.Name = "label8"
        Me.label8.Size = New System.Drawing.Size(86, 13)
        Me.label8.TabIndex = 42
        Me.label8.Text = "List Parameter 2:"
        '
        'checkBoxPad1
        '
        Me.checkBoxPad1.AutoSize = True
        Me.checkBoxPad1.Location = New System.Drawing.Point(15, 150)
        Me.checkBoxPad1.Name = "checkBoxPad1"
        Me.checkBoxPad1.Size = New System.Drawing.Size(121, 17)
        Me.checkBoxPad1.TabIndex = 45
        Me.checkBoxPad1.Text = "Pad Value w/ Zeros"
        Me.checkBoxPad1.UseVisualStyleBackColor = True
        '
        'checkBoxPad2
        '
        Me.checkBoxPad2.AutoSize = True
        Me.checkBoxPad2.Location = New System.Drawing.Point(15, 218)
        Me.checkBoxPad2.Name = "checkBoxPad2"
        Me.checkBoxPad2.Size = New System.Drawing.Size(121, 17)
        Me.checkBoxPad2.TabIndex = 46
        Me.checkBoxPad2.Text = "Pad Value w/ Zeros"
        Me.checkBoxPad2.UseVisualStyleBackColor = True
        '
        'groupBox2
        '
        Me.groupBox2.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBox2.Location = New System.Drawing.Point(326, 350)
        Me.groupBox2.Name = "groupBox2"
        Me.groupBox2.Size = New System.Drawing.Size(178, 81)
        Me.groupBox2.TabIndex = 58
        Me.groupBox2.TabStop = False
        Me.groupBox2.Text = "Processing Options"
        '
        'buttonCreate
        '
        Me.buttonCreate.Location = New System.Drawing.Point(13, 448)
        Me.buttonCreate.Name = "buttonCreate"
        Me.buttonCreate.Size = New System.Drawing.Size(178, 29)
        Me.buttonCreate.TabIndex = 56
        Me.buttonCreate.Text = "Create Rooms"
        Me.buttonCreate.UseVisualStyleBackColor = True
        '
        'label4
        '
        Me.label4.AutoSize = True
        Me.label4.Location = New System.Drawing.Point(12, 9)
        Me.label4.Name = "label4"
        Me.label4.Size = New System.Drawing.Size(162, 13)
        Me.label4.TabIndex = 55
        Me.label4.Text = "Select Areas to Place as Rooms:"
        '
        'buttonClose
        '
        Me.buttonClose.Location = New System.Drawing.Point(326, 448)
        Me.buttonClose.Name = "buttonClose"
        Me.buttonClose.Size = New System.Drawing.Size(178, 29)
        Me.buttonClose.TabIndex = 53
        Me.buttonClose.Text = "Close"
        Me.buttonClose.UseVisualStyleBackColor = True
        '
        'listBoxAreas
        '
        Me.listBoxAreas.FormattingEnabled = True
        Me.listBoxAreas.Location = New System.Drawing.Point(13, 37)
        Me.listBoxAreas.Name = "listBoxAreas"
        Me.listBoxAreas.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.listBoxAreas.Size = New System.Drawing.Size(300, 394)
        Me.listBoxAreas.TabIndex = 54
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripProgressBar1, Me.ToolStripStatusLabel1})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 490)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(513, 22)
        Me.StatusStrip1.TabIndex = 59
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'ToolStripProgressBar1
        '
        Me.ToolStripProgressBar1.Name = "ToolStripProgressBar1"
        Me.ToolStripProgressBar1.Size = New System.Drawing.Size(150, 16)
        Me.ToolStripProgressBar1.Visible = False
        '
        'ToolStripStatusLabel1
        '
        Me.ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        Me.ToolStripStatusLabel1.Size = New System.Drawing.Size(39, 17)
        Me.ToolStripStatusLabel1.Text = "Ready"
        '
        'form_ElemRoomsFromAreas
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(513, 512)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.groupBox1)
        Me.Controls.Add(Me.groupBox2)
        Me.Controls.Add(Me.buttonCreate)
        Me.Controls.Add(Me.label4)
        Me.Controls.Add(Me.buttonClose)
        Me.Controls.Add(Me.listBoxAreas)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(529, 550)
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(529, 550)
        Me.Name = "form_ElemRoomsFromAreas"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Rooms From Areas"
        Me.TopMost = True
        Me.groupBox1.ResumeLayout(False)
        Me.groupBox1.PerformLayout()
        Me.groupBoxPlaced.ResumeLayout(False)
        Me.groupBoxPlaced.PerformLayout()
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents radioButtonPlaced As System.Windows.Forms.RadioButton
    Private WithEvents groupBox1 As System.Windows.Forms.GroupBox
    Private WithEvents checkBoxListReverse As System.Windows.Forms.CheckBox
    Private WithEvents groupBoxPlaced As System.Windows.Forms.GroupBox
    Private WithEvents radioButtonBoth As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonNotPlaced As System.Windows.Forms.RadioButton
    Private WithEvents textBoxPad2 As System.Windows.Forms.TextBox
    Private WithEvents textBoxPad1 As System.Windows.Forms.TextBox
    Private WithEvents textBoxParameterList1 As System.Windows.Forms.TextBox
    Private WithEvents label7 As System.Windows.Forms.Label
    Private WithEvents textBoxParameterList2 As System.Windows.Forms.TextBox
    Private WithEvents label8 As System.Windows.Forms.Label
    Private WithEvents checkBoxPad1 As System.Windows.Forms.CheckBox
    Private WithEvents checkBoxPad2 As System.Windows.Forms.CheckBox
    Private WithEvents groupBox2 As System.Windows.Forms.GroupBox
    Private WithEvents buttonCreate As System.Windows.Forms.Button
    Private WithEvents label4 As System.Windows.Forms.Label
    Private WithEvents buttonClose As System.Windows.Forms.Button
    Private WithEvents listBoxAreas As System.Windows.Forms.ListBox
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents ToolStripProgressBar1 As System.Windows.Forms.ToolStripProgressBar
    Friend WithEvents ToolStripStatusLabel1 As System.Windows.Forms.ToolStripStatusLabel
End Class
