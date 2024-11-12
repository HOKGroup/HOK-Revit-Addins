<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class form_ElemTagViews
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(form_ElemTagViews))
        groupBox2 = New System.Windows.Forms.GroupBox()
        checkBoxStripSuffix = New System.Windows.Forms.CheckBox()
        textBoxPrefixViewSource = New System.Windows.Forms.TextBox()
        label3 = New System.Windows.Forms.Label()
        textBoxParameterViewName = New System.Windows.Forms.TextBox()
        label2 = New System.Windows.Forms.Label()
        groupBox1 = New System.Windows.Forms.GroupBox()
        label5 = New System.Windows.Forms.Label()
        textBoxRestrictPrefixValue = New System.Windows.Forms.TextBox()
        checkBoxListReverse = New System.Windows.Forms.CheckBox()
        checkBoxRestrictPrefix = New System.Windows.Forms.CheckBox()
        checkBoxIncludeExisting = New System.Windows.Forms.CheckBox()
        buttonSelect = New System.Windows.Forms.Button()
        folderBrowserDialogImages = New System.Windows.Forms.FolderBrowserDialog()
        buttonAddTags = New System.Windows.Forms.Button()
        listBoxViews = New System.Windows.Forms.ListBox()
        label4 = New System.Windows.Forms.Label()
        textBoxRoomTag = New System.Windows.Forms.TextBox()
        label1 = New System.Windows.Forms.Label()
        buttonClose = New System.Windows.Forms.Button()
        StatusStrip1 = New System.Windows.Forms.StatusStrip()
        ToolStripProgressBar1 = New System.Windows.Forms.ToolStripProgressBar()
        ToolStripStatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel()
        groupBox2.SuspendLayout()
        groupBox1.SuspendLayout()
        StatusStrip1.SuspendLayout()
        SuspendLayout()
        ' 
        ' groupBox2
        ' 
        groupBox2.Controls.Add(checkBoxStripSuffix)
        groupBox2.Controls.Add(textBoxPrefixViewSource)
        groupBox2.Controls.Add(label3)
        groupBox2.Controls.Add(textBoxParameterViewName)
        groupBox2.Controls.Add(label2)
        groupBox2.ForeColor = Drawing.SystemColors.ControlText
        groupBox2.Location = New System.Drawing.Point(379, 280)
        groupBox2.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        groupBox2.Name = "groupBox2"
        groupBox2.Padding = New System.Windows.Forms.Padding(4, 3, 4, 3)
        groupBox2.Size = New System.Drawing.Size(208, 186)
        groupBox2.TabIndex = 47
        groupBox2.TabStop = False
        groupBox2.Text = "Processing Options"
        ' 
        ' checkBoxStripSuffix
        ' 
        checkBoxStripSuffix.AutoSize = True
        checkBoxStripSuffix.Location = New System.Drawing.Point(10, 148)
        checkBoxStripSuffix.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        checkBoxStripSuffix.Name = "checkBoxStripSuffix"
        checkBoxStripSuffix.Size = New System.Drawing.Size(113, 19)
        checkBoxStripSuffix.TabIndex = 38
        checkBoxStripSuffix.Text = "Strip Suffix (-2D)"
        checkBoxStripSuffix.UseVisualStyleBackColor = True
        ' 
        ' textBoxPrefixViewSource
        ' 
        textBoxPrefixViewSource.Location = New System.Drawing.Point(10, 107)
        textBoxPrefixViewSource.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxPrefixViewSource.Name = "textBoxPrefixViewSource"
        textBoxPrefixViewSource.Size = New System.Drawing.Size(187, 23)
        textBoxPrefixViewSource.TabIndex = 37
        ' 
        ' label3
        ' 
        label3.AutoSize = True
        label3.Location = New System.Drawing.Point(7, 88)
        label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label3.Name = "label3"
        label3.Size = New System.Drawing.Size(103, 15)
        label3.TabIndex = 0
        label3.Text = "View Name Prefix:"
        ' 
        ' textBoxParameterViewName
        ' 
        textBoxParameterViewName.Location = New System.Drawing.Point(10, 48)
        textBoxParameterViewName.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxParameterViewName.Name = "textBoxParameterViewName"
        textBoxParameterViewName.Size = New System.Drawing.Size(187, 23)
        textBoxParameterViewName.TabIndex = 35
        ' 
        ' label2
        ' 
        label2.AutoSize = True
        label2.Location = New System.Drawing.Point(10, 31)
        label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label2.Name = "label2"
        label2.Size = New System.Drawing.Size(167, 15)
        label2.TabIndex = 36
        label2.Text = "Room Parameter - View Name"
        ' 
        ' groupBox1
        ' 
        groupBox1.Controls.Add(label5)
        groupBox1.Controls.Add(textBoxRestrictPrefixValue)
        groupBox1.Controls.Add(checkBoxListReverse)
        groupBox1.Controls.Add(checkBoxRestrictPrefix)
        groupBox1.Controls.Add(checkBoxIncludeExisting)
        groupBox1.ForeColor = Drawing.SystemColors.ControlText
        groupBox1.Location = New System.Drawing.Point(379, 80)
        groupBox1.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        groupBox1.Name = "groupBox1"
        groupBox1.Padding = New System.Windows.Forms.Padding(4, 3, 4, 3)
        groupBox1.Size = New System.Drawing.Size(208, 194)
        groupBox1.TabIndex = 46
        groupBox1.TabStop = False
        groupBox1.Text = "List Selection Options"
        ' 
        ' label5
        ' 
        label5.AutoSize = True
        label5.Location = New System.Drawing.Point(12, 148)
        label5.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label5.Name = "label5"
        label5.Size = New System.Drawing.Size(135, 30)
        label5.TabIndex = 32
        label5.Text = "Note:  Only 2D views are" & vbCrLf & "supported at this time."
        ' 
        ' textBoxRestrictPrefixValue
        ' 
        textBoxRestrictPrefixValue.Location = New System.Drawing.Point(14, 76)
        textBoxRestrictPrefixValue.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxRestrictPrefixValue.Name = "textBoxRestrictPrefixValue"
        textBoxRestrictPrefixValue.Size = New System.Drawing.Size(184, 23)
        textBoxRestrictPrefixValue.TabIndex = 28
        ' 
        ' checkBoxListReverse
        ' 
        checkBoxListReverse.AutoSize = True
        checkBoxListReverse.Location = New System.Drawing.Point(14, 115)
        checkBoxListReverse.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        checkBoxListReverse.Name = "checkBoxListReverse"
        checkBoxListReverse.Size = New System.Drawing.Size(120, 19)
        checkBoxListReverse.TabIndex = 24
        checkBoxListReverse.Text = "Reverse List Order"
        checkBoxListReverse.UseVisualStyleBackColor = True
        ' 
        ' checkBoxRestrictPrefix
        ' 
        checkBoxRestrictPrefix.AutoSize = True
        checkBoxRestrictPrefix.Location = New System.Drawing.Point(14, 55)
        checkBoxRestrictPrefix.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        checkBoxRestrictPrefix.Name = "checkBoxRestrictPrefix"
        checkBoxRestrictPrefix.Size = New System.Drawing.Size(172, 19)
        checkBoxRestrictPrefix.TabIndex = 29
        checkBoxRestrictPrefix.Text = "Restrict to Names w/ Prefix:"
        checkBoxRestrictPrefix.UseVisualStyleBackColor = True
        ' 
        ' checkBoxIncludeExisting
        ' 
        checkBoxIncludeExisting.AutoSize = True
        checkBoxIncludeExisting.Location = New System.Drawing.Point(14, 22)
        checkBoxIncludeExisting.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        checkBoxIncludeExisting.Name = "checkBoxIncludeExisting"
        checkBoxIncludeExisting.Size = New System.Drawing.Size(128, 19)
        checkBoxIncludeExisting.TabIndex = 31
        checkBoxIncludeExisting.Text = "List Already Tagged"
        checkBoxIncludeExisting.UseVisualStyleBackColor = True
        ' 
        ' buttonSelect
        ' 
        buttonSelect.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25F, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        buttonSelect.Location = New System.Drawing.Point(553, 24)
        buttonSelect.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        buttonSelect.Name = "buttonSelect"
        buttonSelect.Size = New System.Drawing.Size(34, 30)
        buttonSelect.TabIndex = 45
        buttonSelect.Text = "..."
        buttonSelect.UseVisualStyleBackColor = True
        ' 
        ' buttonAddTags
        ' 
        buttonAddTags.Location = New System.Drawing.Point(14, 513)
        buttonAddTags.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        buttonAddTags.Name = "buttonAddTags"
        buttonAddTags.Size = New System.Drawing.Size(208, 33)
        buttonAddTags.TabIndex = 42
        buttonAddTags.Text = "Add Tags"
        buttonAddTags.UseVisualStyleBackColor = True
        ' 
        ' listBoxViews
        ' 
        listBoxViews.FormattingEnabled = True
        listBoxViews.ItemHeight = 15
        listBoxViews.Location = New System.Drawing.Point(14, 87)
        listBoxViews.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        listBoxViews.Name = "listBoxViews"
        listBoxViews.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        listBoxViews.Size = New System.Drawing.Size(349, 409)
        listBoxViews.TabIndex = 40
        ' 
        ' label4
        ' 
        label4.AutoSize = True
        label4.Location = New System.Drawing.Point(14, 68)
        label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label4.Name = "label4"
        label4.Size = New System.Drawing.Size(184, 15)
        label4.TabIndex = 41
        label4.Text = "Select Views in Which to Add Tag:"
        ' 
        ' textBoxRoomTag
        ' 
        textBoxRoomTag.Location = New System.Drawing.Point(14, 29)
        textBoxRoomTag.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxRoomTag.Name = "textBoxRoomTag"
        textBoxRoomTag.Size = New System.Drawing.Size(531, 23)
        textBoxRoomTag.TabIndex = 43
        ' 
        ' label1
        ' 
        label1.AutoSize = True
        label1.Location = New System.Drawing.Point(14, 10)
        label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label1.Name = "label1"
        label1.Size = New System.Drawing.Size(156, 15)
        label1.TabIndex = 44
        label1.Text = "Room Tag to Add to Rooms:"
        ' 
        ' buttonClose
        ' 
        buttonClose.Location = New System.Drawing.Point(379, 513)
        buttonClose.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        buttonClose.Name = "buttonClose"
        buttonClose.Size = New System.Drawing.Size(208, 33)
        buttonClose.TabIndex = 39
        buttonClose.Text = "Close"
        buttonClose.UseVisualStyleBackColor = True
        ' 
        ' StatusStrip1
        ' 
        StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {ToolStripProgressBar1, ToolStripStatusLabel1})
        StatusStrip1.Location = New System.Drawing.Point(0, 562)
        StatusStrip1.Name = "StatusStrip1"
        StatusStrip1.Padding = New System.Windows.Forms.Padding(1, 0, 16, 0)
        StatusStrip1.Size = New System.Drawing.Size(600, 22)
        StatusStrip1.TabIndex = 48
        StatusStrip1.Text = "StatusStrip1"
        ' 
        ' ToolStripProgressBar1
        ' 
        ToolStripProgressBar1.Name = "ToolStripProgressBar1"
        ToolStripProgressBar1.Size = New System.Drawing.Size(175, 18)
        ToolStripProgressBar1.Visible = False
        ' 
        ' ToolStripStatusLabel1
        ' 
        ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        ToolStripStatusLabel1.Size = New System.Drawing.Size(39, 17)
        ToolStripStatusLabel1.Text = "Ready"
        ' 
        ' form_ElemTagViews
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        ClientSize = New System.Drawing.Size(600, 584)
        Controls.Add(StatusStrip1)
        Controls.Add(groupBox2)
        Controls.Add(groupBox1)
        Controls.Add(buttonSelect)
        Controls.Add(buttonAddTags)
        Controls.Add(listBoxViews)
        Controls.Add(label4)
        Controls.Add(textBoxRoomTag)
        Controls.Add(label1)
        Controls.Add(buttonClose)
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        MaximizeBox = False
        MaximumSize = New System.Drawing.Size(616, 800)
        MinimizeBox = False
        MinimumSize = New System.Drawing.Size(616, 623)
        Name = "form_ElemTagViews"
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "Tag Views"
        TopMost = True
        groupBox2.ResumeLayout(False)
        groupBox2.PerformLayout()
        groupBox1.ResumeLayout(False)
        groupBox1.PerformLayout()
        StatusStrip1.ResumeLayout(False)
        StatusStrip1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()

    End Sub
    Private WithEvents groupBox2 As System.Windows.Forms.GroupBox
    Private WithEvents checkBoxStripSuffix As System.Windows.Forms.CheckBox
    Private WithEvents textBoxPrefixViewSource As System.Windows.Forms.TextBox
    Private WithEvents label3 As System.Windows.Forms.Label
    Private WithEvents textBoxParameterViewName As System.Windows.Forms.TextBox
    Private WithEvents label2 As System.Windows.Forms.Label
    Private WithEvents groupBox1 As System.Windows.Forms.GroupBox
    Private WithEvents label5 As System.Windows.Forms.Label
    Private WithEvents textBoxRestrictPrefixValue As System.Windows.Forms.TextBox
    Private WithEvents checkBoxListReverse As System.Windows.Forms.CheckBox
    Private WithEvents checkBoxRestrictPrefix As System.Windows.Forms.CheckBox
    Private WithEvents checkBoxIncludeExisting As System.Windows.Forms.CheckBox
    Private WithEvents buttonSelect As System.Windows.Forms.Button
    Private WithEvents folderBrowserDialogImages As System.Windows.Forms.FolderBrowserDialog
    Private WithEvents buttonAddTags As System.Windows.Forms.Button
    Private WithEvents listBoxViews As System.Windows.Forms.ListBox
    Private WithEvents label4 As System.Windows.Forms.Label
    Private WithEvents textBoxRoomTag As System.Windows.Forms.TextBox
    Private WithEvents label1 As System.Windows.Forms.Label
    Private WithEvents buttonClose As System.Windows.Forms.Button
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents ToolStripProgressBar1 As System.Windows.Forms.ToolStripProgressBar
    Friend WithEvents ToolStripStatusLabel1 As System.Windows.Forms.ToolStripStatusLabel
End Class
