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
        radioButtonPlaced = New System.Windows.Forms.RadioButton()
        groupBox1 = New System.Windows.Forms.GroupBox()
        checkBoxListReverse = New System.Windows.Forms.CheckBox()
        groupBoxPlaced = New System.Windows.Forms.GroupBox()
        radioButtonBoth = New System.Windows.Forms.RadioButton()
        radioButtonNotPlaced = New System.Windows.Forms.RadioButton()
        textBoxPad2 = New System.Windows.Forms.TextBox()
        textBoxPad1 = New System.Windows.Forms.TextBox()
        textBoxParameterList1 = New System.Windows.Forms.TextBox()
        label7 = New System.Windows.Forms.Label()
        textBoxParameterList2 = New System.Windows.Forms.TextBox()
        label8 = New System.Windows.Forms.Label()
        checkBoxPad1 = New System.Windows.Forms.CheckBox()
        checkBoxPad2 = New System.Windows.Forms.CheckBox()
        groupBox2 = New System.Windows.Forms.GroupBox()
        buttonCreate = New System.Windows.Forms.Button()
        label4 = New System.Windows.Forms.Label()
        buttonClose = New System.Windows.Forms.Button()
        listBoxAreas = New System.Windows.Forms.ListBox()
        StatusStrip1 = New System.Windows.Forms.StatusStrip()
        ToolStripProgressBar1 = New System.Windows.Forms.ToolStripProgressBar()
        ToolStripStatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel()
        groupBox1.SuspendLayout()
        groupBoxPlaced.SuspendLayout()
        StatusStrip1.SuspendLayout()
        SuspendLayout()
        ' 
        ' radioButtonPlaced
        ' 
        radioButtonPlaced.AutoSize = True
        radioButtonPlaced.Location = New System.Drawing.Point(13, 17)
        radioButtonPlaced.Margin = New System.Windows.Forms.Padding(4)
        radioButtonPlaced.Name = "radioButtonPlaced"
        radioButtonPlaced.Size = New System.Drawing.Size(60, 19)
        radioButtonPlaced.TabIndex = 0
        radioButtonPlaced.TabStop = True
        radioButtonPlaced.Text = "Placed"
        radioButtonPlaced.UseVisualStyleBackColor = True
        ' 
        ' groupBox1
        ' 
        groupBox1.Controls.Add(checkBoxListReverse)
        groupBox1.Controls.Add(groupBoxPlaced)
        groupBox1.Controls.Add(textBoxPad2)
        groupBox1.Controls.Add(textBoxPad1)
        groupBox1.Controls.Add(textBoxParameterList1)
        groupBox1.Controls.Add(label7)
        groupBox1.Controls.Add(textBoxParameterList2)
        groupBox1.Controls.Add(label8)
        groupBox1.Controls.Add(checkBoxPad1)
        groupBox1.Controls.Add(checkBoxPad2)
        groupBox1.ForeColor = Drawing.SystemColors.ControlText
        groupBox1.Location = New System.Drawing.Point(380, 21)
        groupBox1.Margin = New System.Windows.Forms.Padding(4)
        groupBox1.Name = "groupBox1"
        groupBox1.Padding = New System.Windows.Forms.Padding(4)
        groupBox1.Size = New System.Drawing.Size(208, 334)
        groupBox1.TabIndex = 57
        groupBox1.TabStop = False
        groupBox1.Text = "List Selection Options"
        ' 
        ' checkBoxListReverse
        ' 
        checkBoxListReverse.AutoSize = True
        checkBoxListReverse.Location = New System.Drawing.Point(18, 293)
        checkBoxListReverse.Margin = New System.Windows.Forms.Padding(4)
        checkBoxListReverse.Name = "checkBoxListReverse"
        checkBoxListReverse.Size = New System.Drawing.Size(120, 19)
        checkBoxListReverse.TabIndex = 49
        checkBoxListReverse.Text = "Reverse List Order"
        checkBoxListReverse.UseVisualStyleBackColor = True
        ' 
        ' groupBoxPlaced
        ' 
        groupBoxPlaced.Controls.Add(radioButtonBoth)
        groupBoxPlaced.Controls.Add(radioButtonNotPlaced)
        groupBoxPlaced.Controls.Add(radioButtonPlaced)
        groupBoxPlaced.ForeColor = Drawing.SystemColors.ControlText
        groupBoxPlaced.Location = New System.Drawing.Point(16, 22)
        groupBoxPlaced.Margin = New System.Windows.Forms.Padding(4)
        groupBoxPlaced.Name = "groupBoxPlaced"
        groupBoxPlaced.Padding = New System.Windows.Forms.Padding(4)
        groupBoxPlaced.Size = New System.Drawing.Size(175, 95)
        groupBoxPlaced.TabIndex = 50
        groupBoxPlaced.TabStop = False
        groupBoxPlaced.Text = "Include"
        ' 
        ' radioButtonBoth
        ' 
        radioButtonBoth.AutoSize = True
        radioButtonBoth.Location = New System.Drawing.Point(13, 65)
        radioButtonBoth.Margin = New System.Windows.Forms.Padding(4)
        radioButtonBoth.Name = "radioButtonBoth"
        radioButtonBoth.Size = New System.Drawing.Size(50, 19)
        radioButtonBoth.TabIndex = 2
        radioButtonBoth.TabStop = True
        radioButtonBoth.Text = "Both"
        radioButtonBoth.UseVisualStyleBackColor = True
        ' 
        ' radioButtonNotPlaced
        ' 
        radioButtonNotPlaced.AutoSize = True
        radioButtonNotPlaced.Location = New System.Drawing.Point(13, 40)
        radioButtonNotPlaced.Margin = New System.Windows.Forms.Padding(4)
        radioButtonNotPlaced.Name = "radioButtonNotPlaced"
        radioButtonNotPlaced.Size = New System.Drawing.Size(83, 19)
        radioButtonNotPlaced.TabIndex = 1
        radioButtonNotPlaced.TabStop = True
        radioButtonNotPlaced.Text = "Not Placed"
        radioButtonNotPlaced.UseVisualStyleBackColor = True
        ' 
        ' textBoxPad2
        ' 
        textBoxPad2.Location = New System.Drawing.Point(156, 248)
        textBoxPad2.Margin = New System.Windows.Forms.Padding(4)
        textBoxPad2.Name = "textBoxPad2"
        textBoxPad2.Size = New System.Drawing.Size(36, 23)
        textBoxPad2.TabIndex = 48
        ' 
        ' textBoxPad1
        ' 
        textBoxPad1.Location = New System.Drawing.Point(156, 170)
        textBoxPad1.Margin = New System.Windows.Forms.Padding(4)
        textBoxPad1.Name = "textBoxPad1"
        textBoxPad1.Size = New System.Drawing.Size(36, 23)
        textBoxPad1.TabIndex = 47
        ' 
        ' textBoxParameterList1
        ' 
        textBoxParameterList1.Location = New System.Drawing.Point(18, 144)
        textBoxParameterList1.Margin = New System.Windows.Forms.Padding(4)
        textBoxParameterList1.Name = "textBoxParameterList1"
        textBoxParameterList1.Size = New System.Drawing.Size(176, 23)
        textBoxParameterList1.TabIndex = 39
        ' 
        ' label7
        ' 
        label7.AutoSize = True
        label7.Location = New System.Drawing.Point(18, 127)
        label7.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label7.Name = "label7"
        label7.Size = New System.Drawing.Size(94, 15)
        label7.TabIndex = 40
        label7.Text = "List Parameter 1:"
        ' 
        ' textBoxParameterList2
        ' 
        textBoxParameterList2.Location = New System.Drawing.Point(18, 223)
        textBoxParameterList2.Margin = New System.Windows.Forms.Padding(4)
        textBoxParameterList2.Name = "textBoxParameterList2"
        textBoxParameterList2.Size = New System.Drawing.Size(176, 23)
        textBoxParameterList2.TabIndex = 41
        ' 
        ' label8
        ' 
        label8.AutoSize = True
        label8.Location = New System.Drawing.Point(14, 205)
        label8.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label8.Name = "label8"
        label8.Size = New System.Drawing.Size(94, 15)
        label8.TabIndex = 42
        label8.Text = "List Parameter 2:"
        ' 
        ' checkBoxPad1
        ' 
        checkBoxPad1.AutoSize = True
        checkBoxPad1.Location = New System.Drawing.Point(18, 173)
        checkBoxPad1.Margin = New System.Windows.Forms.Padding(4)
        checkBoxPad1.Name = "checkBoxPad1"
        checkBoxPad1.Size = New System.Drawing.Size(126, 19)
        checkBoxPad1.TabIndex = 45
        checkBoxPad1.Text = "Pad Value w/ Zeros"
        checkBoxPad1.UseVisualStyleBackColor = True
        ' 
        ' checkBoxPad2
        ' 
        checkBoxPad2.AutoSize = True
        checkBoxPad2.Location = New System.Drawing.Point(18, 251)
        checkBoxPad2.Margin = New System.Windows.Forms.Padding(4)
        checkBoxPad2.Name = "checkBoxPad2"
        checkBoxPad2.Size = New System.Drawing.Size(126, 19)
        checkBoxPad2.TabIndex = 46
        checkBoxPad2.Text = "Pad Value w/ Zeros"
        checkBoxPad2.UseVisualStyleBackColor = True
        ' 
        ' groupBox2
        ' 
        groupBox2.ForeColor = Drawing.SystemColors.ControlText
        groupBox2.Location = New System.Drawing.Point(380, 404)
        groupBox2.Margin = New System.Windows.Forms.Padding(4)
        groupBox2.Name = "groupBox2"
        groupBox2.Padding = New System.Windows.Forms.Padding(4)
        groupBox2.Size = New System.Drawing.Size(208, 94)
        groupBox2.TabIndex = 58
        groupBox2.TabStop = False
        groupBox2.Text = "Processing Options"
        ' 
        ' buttonCreate
        ' 
        buttonCreate.Location = New System.Drawing.Point(15, 517)
        buttonCreate.Margin = New System.Windows.Forms.Padding(4)
        buttonCreate.Name = "buttonCreate"
        buttonCreate.Size = New System.Drawing.Size(208, 34)
        buttonCreate.TabIndex = 56
        buttonCreate.Text = "Create Rooms"
        buttonCreate.UseVisualStyleBackColor = True
        ' 
        ' label4
        ' 
        label4.AutoSize = True
        label4.Location = New System.Drawing.Point(14, 10)
        label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label4.Name = "label4"
        label4.Size = New System.Drawing.Size(172, 15)
        label4.TabIndex = 55
        label4.Text = "Select Areas to Place as Rooms:"
        ' 
        ' buttonClose
        ' 
        buttonClose.Location = New System.Drawing.Point(372, 517)
        buttonClose.Margin = New System.Windows.Forms.Padding(4)
        buttonClose.Name = "buttonClose"
        buttonClose.Size = New System.Drawing.Size(208, 34)
        buttonClose.TabIndex = 53
        buttonClose.Text = "Close"
        buttonClose.UseVisualStyleBackColor = True
        ' 
        ' listBoxAreas
        ' 
        listBoxAreas.FormattingEnabled = True
        listBoxAreas.ItemHeight = 15
        listBoxAreas.Location = New System.Drawing.Point(15, 43)
        listBoxAreas.Margin = New System.Windows.Forms.Padding(4)
        listBoxAreas.Name = "listBoxAreas"
        listBoxAreas.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        listBoxAreas.Size = New System.Drawing.Size(350, 454)
        listBoxAreas.TabIndex = 54
        ' 
        ' StatusStrip1
        ' 
        StatusStrip1.ImageScalingSize = New System.Drawing.Size(36, 36)
        StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {ToolStripProgressBar1, ToolStripStatusLabel1})
        StatusStrip1.Location = New System.Drawing.Point(0, 584)
        StatusStrip1.Name = "StatusStrip1"
        StatusStrip1.Padding = New System.Windows.Forms.Padding(1, 0, 16, 0)
        StatusStrip1.Size = New System.Drawing.Size(590, 22)
        StatusStrip1.TabIndex = 59
        StatusStrip1.Text = "StatusStrip1"
        ' 
        ' ToolStripProgressBar1
        ' 
        ToolStripProgressBar1.Name = "ToolStripProgressBar1"
        ToolStripProgressBar1.Size = New System.Drawing.Size(175, 22)
        ToolStripProgressBar1.Visible = False
        ' 
        ' ToolStripStatusLabel1
        ' 
        ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        ToolStripStatusLabel1.Size = New System.Drawing.Size(39, 17)
        ToolStripStatusLabel1.Text = "Ready"
        ' 
        ' form_ElemRoomsFromAreas
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        ClientSize = New System.Drawing.Size(590, 606)
        Controls.Add(StatusStrip1)
        Controls.Add(groupBox1)
        Controls.Add(groupBox2)
        Controls.Add(buttonCreate)
        Controls.Add(label4)
        Controls.Add(buttonClose)
        Controls.Add(listBoxAreas)
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        Margin = New System.Windows.Forms.Padding(4)
        MaximizeBox = False
        MaximumSize = New System.Drawing.Size(606, 800)
        MinimizeBox = False
        MinimumSize = New System.Drawing.Size(606, 623)
        Name = "form_ElemRoomsFromAreas"
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "Rooms From Areas"
        TopMost = True
        groupBox1.ResumeLayout(False)
        groupBox1.PerformLayout()
        groupBoxPlaced.ResumeLayout(False)
        groupBoxPlaced.PerformLayout()
        StatusStrip1.ResumeLayout(False)
        StatusStrip1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()

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
