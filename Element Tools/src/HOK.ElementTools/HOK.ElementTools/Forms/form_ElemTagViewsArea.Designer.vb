<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class form_ElemTagViewsArea
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(form_ElemTagViewsArea))
        Me.groupBox2 = New System.Windows.Forms.GroupBox()
        Me.checkBoxStripSuffix = New System.Windows.Forms.CheckBox()
        Me.textBoxPrefixViewSource = New System.Windows.Forms.TextBox()
        Me.label3 = New System.Windows.Forms.Label()
        Me.textBoxParameterViewName = New System.Windows.Forms.TextBox()
        Me.label2 = New System.Windows.Forms.Label()
        Me.groupBox1 = New System.Windows.Forms.GroupBox()
        Me.label5 = New System.Windows.Forms.Label()
        Me.textBoxRestrictPrefixValue = New System.Windows.Forms.TextBox()
        Me.checkBoxListReverse = New System.Windows.Forms.CheckBox()
        Me.checkBoxRestrictPrefix = New System.Windows.Forms.CheckBox()
        Me.checkBoxIncludeExisting = New System.Windows.Forms.CheckBox()
        Me.buttonSelect = New System.Windows.Forms.Button()
        Me.buttonAddTags = New System.Windows.Forms.Button()
        Me.listBoxViews = New System.Windows.Forms.ListBox()
        Me.label4 = New System.Windows.Forms.Label()
        Me.textBoxAreaTag = New System.Windows.Forms.TextBox()
        Me.label1 = New System.Windows.Forms.Label()
        Me.buttonClose = New System.Windows.Forms.Button()
        Me.folderBrowserDialogImages = New System.Windows.Forms.FolderBrowserDialog()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.ToolStripProgressBar1 = New System.Windows.Forms.ToolStripProgressBar()
        Me.ToolStripStatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.groupBox2.SuspendLayout()
        Me.groupBox1.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'groupBox2
        '
        Me.groupBox2.Controls.Add(Me.checkBoxStripSuffix)
        Me.groupBox2.Controls.Add(Me.textBoxPrefixViewSource)
        Me.groupBox2.Controls.Add(Me.label3)
        Me.groupBox2.Controls.Add(Me.textBoxParameterViewName)
        Me.groupBox2.Controls.Add(Me.label2)
        Me.groupBox2.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBox2.Location = New System.Drawing.Point(325, 245)
        Me.groupBox2.Name = "groupBox2"
        Me.groupBox2.Size = New System.Drawing.Size(178, 161)
        Me.groupBox2.TabIndex = 56
        Me.groupBox2.TabStop = False
        Me.groupBox2.Text = "Processing Options"
        '
        'checkBoxStripSuffix
        '
        Me.checkBoxStripSuffix.AutoSize = True
        Me.checkBoxStripSuffix.Location = New System.Drawing.Point(9, 128)
        Me.checkBoxStripSuffix.Name = "checkBoxStripSuffix"
        Me.checkBoxStripSuffix.Size = New System.Drawing.Size(102, 17)
        Me.checkBoxStripSuffix.TabIndex = 38
        Me.checkBoxStripSuffix.Text = "Strip Suffix (-2D)"
        Me.checkBoxStripSuffix.UseVisualStyleBackColor = True
        '
        'textBoxPrefixViewSource
        '
        Me.textBoxPrefixViewSource.Location = New System.Drawing.Point(9, 93)
        Me.textBoxPrefixViewSource.Name = "textBoxPrefixViewSource"
        Me.textBoxPrefixViewSource.Size = New System.Drawing.Size(161, 20)
        Me.textBoxPrefixViewSource.TabIndex = 37
        '
        'label3
        '
        Me.label3.AutoSize = True
        Me.label3.Location = New System.Drawing.Point(6, 76)
        Me.label3.Name = "label3"
        Me.label3.Size = New System.Drawing.Size(93, 13)
        Me.label3.TabIndex = 0
        Me.label3.Text = "View Name Prefix:"
        '
        'textBoxParameterViewName
        '
        Me.textBoxParameterViewName.Location = New System.Drawing.Point(9, 42)
        Me.textBoxParameterViewName.Name = "textBoxParameterViewName"
        Me.textBoxParameterViewName.Size = New System.Drawing.Size(161, 20)
        Me.textBoxParameterViewName.TabIndex = 35
        '
        'label2
        '
        Me.label2.AutoSize = True
        Me.label2.Location = New System.Drawing.Point(9, 27)
        Me.label2.Name = "label2"
        Me.label2.Size = New System.Drawing.Size(143, 13)
        Me.label2.TabIndex = 36
        Me.label2.Text = "Area Parameter - View Name"
        '
        'groupBox1
        '
        Me.groupBox1.Controls.Add(Me.label5)
        Me.groupBox1.Controls.Add(Me.textBoxRestrictPrefixValue)
        Me.groupBox1.Controls.Add(Me.checkBoxListReverse)
        Me.groupBox1.Controls.Add(Me.checkBoxRestrictPrefix)
        Me.groupBox1.Controls.Add(Me.checkBoxIncludeExisting)
        Me.groupBox1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBox1.Location = New System.Drawing.Point(325, 71)
        Me.groupBox1.Name = "groupBox1"
        Me.groupBox1.Size = New System.Drawing.Size(178, 168)
        Me.groupBox1.TabIndex = 55
        Me.groupBox1.TabStop = False
        Me.groupBox1.Text = "List Selection Options"
        '
        'label5
        '
        Me.label5.AutoSize = True
        Me.label5.Location = New System.Drawing.Point(10, 128)
        Me.label5.Name = "label5"
        Me.label5.Size = New System.Drawing.Size(125, 26)
        Me.label5.TabIndex = 32
        Me.label5.Text = "Note:  Only 2D views are" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "supported at this time."
        '
        'textBoxRestrictPrefixValue
        '
        Me.textBoxRestrictPrefixValue.Location = New System.Drawing.Point(12, 66)
        Me.textBoxRestrictPrefixValue.Name = "textBoxRestrictPrefixValue"
        Me.textBoxRestrictPrefixValue.Size = New System.Drawing.Size(158, 20)
        Me.textBoxRestrictPrefixValue.TabIndex = 28
        '
        'checkBoxListReverse
        '
        Me.checkBoxListReverse.AutoSize = True
        Me.checkBoxListReverse.Location = New System.Drawing.Point(12, 100)
        Me.checkBoxListReverse.Name = "checkBoxListReverse"
        Me.checkBoxListReverse.Size = New System.Drawing.Size(114, 17)
        Me.checkBoxListReverse.TabIndex = 24
        Me.checkBoxListReverse.Text = "Reverse List Order"
        Me.checkBoxListReverse.UseVisualStyleBackColor = True
        '
        'checkBoxRestrictPrefix
        '
        Me.checkBoxRestrictPrefix.AutoSize = True
        Me.checkBoxRestrictPrefix.Location = New System.Drawing.Point(12, 48)
        Me.checkBoxRestrictPrefix.Name = "checkBoxRestrictPrefix"
        Me.checkBoxRestrictPrefix.Size = New System.Drawing.Size(158, 17)
        Me.checkBoxRestrictPrefix.TabIndex = 29
        Me.checkBoxRestrictPrefix.Text = "Restrict to Names w/ Prefix:"
        Me.checkBoxRestrictPrefix.UseVisualStyleBackColor = True
        '
        'checkBoxIncludeExisting
        '
        Me.checkBoxIncludeExisting.AutoSize = True
        Me.checkBoxIncludeExisting.Location = New System.Drawing.Point(12, 19)
        Me.checkBoxIncludeExisting.Name = "checkBoxIncludeExisting"
        Me.checkBoxIncludeExisting.Size = New System.Drawing.Size(120, 17)
        Me.checkBoxIncludeExisting.TabIndex = 31
        Me.checkBoxIncludeExisting.Text = "List Already Tagged"
        Me.checkBoxIncludeExisting.UseVisualStyleBackColor = True
        '
        'buttonSelect
        '
        Me.buttonSelect.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.buttonSelect.Location = New System.Drawing.Point(474, 23)
        Me.buttonSelect.Name = "buttonSelect"
        Me.buttonSelect.Size = New System.Drawing.Size(29, 26)
        Me.buttonSelect.TabIndex = 54
        Me.buttonSelect.Text = "..."
        Me.buttonSelect.UseVisualStyleBackColor = True
        '
        'buttonAddTags
        '
        Me.buttonAddTags.Location = New System.Drawing.Point(12, 447)
        Me.buttonAddTags.Name = "buttonAddTags"
        Me.buttonAddTags.Size = New System.Drawing.Size(178, 29)
        Me.buttonAddTags.TabIndex = 51
        Me.buttonAddTags.Text = "Add Tags"
        Me.buttonAddTags.UseVisualStyleBackColor = True
        '
        'listBoxViews
        '
        Me.listBoxViews.FormattingEnabled = True
        Me.listBoxViews.Location = New System.Drawing.Point(12, 77)
        Me.listBoxViews.Name = "listBoxViews"
        Me.listBoxViews.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.listBoxViews.Size = New System.Drawing.Size(300, 355)
        Me.listBoxViews.TabIndex = 49
        '
        'label4
        '
        Me.label4.AutoSize = True
        Me.label4.Location = New System.Drawing.Point(12, 61)
        Me.label4.Name = "label4"
        Me.label4.Size = New System.Drawing.Size(172, 13)
        Me.label4.TabIndex = 50
        Me.label4.Text = "Select Views in Which to Add Tag:"
        '
        'textBoxAreaTag
        '
        Me.textBoxAreaTag.Location = New System.Drawing.Point(12, 27)
        Me.textBoxAreaTag.Name = "textBoxAreaTag"
        Me.textBoxAreaTag.Size = New System.Drawing.Size(456, 20)
        Me.textBoxAreaTag.TabIndex = 52
        '
        'label1
        '
        Me.label1.AutoSize = True
        Me.label1.Location = New System.Drawing.Point(12, 11)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(130, 13)
        Me.label1.TabIndex = 53
        Me.label1.Text = "Area Tag to Add to Areas:"
        '
        'buttonClose
        '
        Me.buttonClose.Location = New System.Drawing.Point(325, 447)
        Me.buttonClose.Name = "buttonClose"
        Me.buttonClose.Size = New System.Drawing.Size(178, 29)
        Me.buttonClose.TabIndex = 48
        Me.buttonClose.Text = "Close"
        Me.buttonClose.UseVisualStyleBackColor = True
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripProgressBar1, Me.ToolStripStatusLabel1})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 485)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(514, 22)
        Me.StatusStrip1.TabIndex = 57
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
        'form_ElemTagViewsArea
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(514, 507)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.groupBox2)
        Me.Controls.Add(Me.groupBox1)
        Me.Controls.Add(Me.buttonSelect)
        Me.Controls.Add(Me.buttonAddTags)
        Me.Controls.Add(Me.listBoxViews)
        Me.Controls.Add(Me.label4)
        Me.Controls.Add(Me.textBoxAreaTag)
        Me.Controls.Add(Me.label1)
        Me.Controls.Add(Me.buttonClose)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximumSize = New System.Drawing.Size(530, 545)
        Me.MinimumSize = New System.Drawing.Size(530, 524)
        Me.Name = "form_ElemTagViewsArea"
        Me.Text = "Tag Views"
        Me.groupBox2.ResumeLayout(False)
        Me.groupBox2.PerformLayout()
        Me.groupBox1.ResumeLayout(False)
        Me.groupBox1.PerformLayout()
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

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
    Private WithEvents buttonAddTags As System.Windows.Forms.Button
    Private WithEvents listBoxViews As System.Windows.Forms.ListBox
    Private WithEvents label4 As System.Windows.Forms.Label
    Private WithEvents textBoxAreaTag As System.Windows.Forms.TextBox
    Private WithEvents label1 As System.Windows.Forms.Label
    Private WithEvents buttonClose As System.Windows.Forms.Button
    Private WithEvents folderBrowserDialogImages As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents ToolStripProgressBar1 As System.Windows.Forms.ToolStripProgressBar
    Friend WithEvents ToolStripStatusLabel1 As System.Windows.Forms.ToolStripStatusLabel
End Class
