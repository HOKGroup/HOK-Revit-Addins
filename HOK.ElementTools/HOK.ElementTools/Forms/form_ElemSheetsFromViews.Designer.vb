<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class form_ElemSheetsFromViews
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(form_ElemSheetsFromViews))
        Me.buttonSelect = New System.Windows.Forms.Button()
        Me.folderBrowserDialogImages = New System.Windows.Forms.FolderBrowserDialog()
        Me.textBoxTitleblock = New System.Windows.Forms.TextBox()
        Me.label1 = New System.Windows.Forms.Label()
        Me.checkBoxIncludeExisting = New System.Windows.Forms.CheckBox()
        Me.buttonClose = New System.Windows.Forms.Button()
        Me.checkBoxRestrictPrefix = New System.Windows.Forms.CheckBox()
        Me.textBoxRestrictPrefixValue = New System.Windows.Forms.TextBox()
        Me.groupBox1 = New System.Windows.Forms.GroupBox()
        Me.checkBoxListReverse = New System.Windows.Forms.CheckBox()
        Me.buttonCreate = New System.Windows.Forms.Button()
        Me.listBoxViews = New System.Windows.Forms.ListBox()
        Me.label4 = New System.Windows.Forms.Label()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.ToolStripProgressBar1 = New System.Windows.Forms.ToolStripProgressBar()
        Me.ToolStripStatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.groupBox1.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'buttonSelect
        '
        Me.buttonSelect.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.buttonSelect.Location = New System.Drawing.Point(474, 21)
        Me.buttonSelect.Name = "buttonSelect"
        Me.buttonSelect.Size = New System.Drawing.Size(29, 26)
        Me.buttonSelect.TabIndex = 43
        Me.buttonSelect.Text = "..."
        Me.buttonSelect.UseVisualStyleBackColor = True
        '
        'textBoxTitleblock
        '
        Me.textBoxTitleblock.Location = New System.Drawing.Point(12, 25)
        Me.textBoxTitleblock.Name = "textBoxTitleblock"
        Me.textBoxTitleblock.Size = New System.Drawing.Size(456, 20)
        Me.textBoxTitleblock.TabIndex = 41
        '
        'label1
        '
        Me.label1.AutoSize = True
        Me.label1.Location = New System.Drawing.Point(12, 9)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(166, 13)
        Me.label1.TabIndex = 42
        Me.label1.Text = "Titleblock to Use for New Sheets:"
        '
        'checkBoxIncludeExisting
        '
        Me.checkBoxIncludeExisting.AutoSize = True
        Me.checkBoxIncludeExisting.Location = New System.Drawing.Point(11, 22)
        Me.checkBoxIncludeExisting.Name = "checkBoxIncludeExisting"
        Me.checkBoxIncludeExisting.Size = New System.Drawing.Size(145, 17)
        Me.checkBoxIncludeExisting.TabIndex = 31
        Me.checkBoxIncludeExisting.Text = "List and Replace Existing"
        Me.checkBoxIncludeExisting.UseVisualStyleBackColor = True
        '
        'buttonClose
        '
        Me.buttonClose.Location = New System.Drawing.Point(325, 445)
        Me.buttonClose.Name = "buttonClose"
        Me.buttonClose.Size = New System.Drawing.Size(178, 29)
        Me.buttonClose.TabIndex = 37
        Me.buttonClose.Text = "Close"
        Me.buttonClose.UseVisualStyleBackColor = True
        '
        'checkBoxRestrictPrefix
        '
        Me.checkBoxRestrictPrefix.AutoSize = True
        Me.checkBoxRestrictPrefix.Location = New System.Drawing.Point(11, 56)
        Me.checkBoxRestrictPrefix.Name = "checkBoxRestrictPrefix"
        Me.checkBoxRestrictPrefix.Size = New System.Drawing.Size(158, 17)
        Me.checkBoxRestrictPrefix.TabIndex = 29
        Me.checkBoxRestrictPrefix.Text = "Restrict to Names w/ Prefix:"
        Me.checkBoxRestrictPrefix.UseVisualStyleBackColor = True
        '
        'textBoxRestrictPrefixValue
        '
        Me.textBoxRestrictPrefixValue.Location = New System.Drawing.Point(11, 74)
        Me.textBoxRestrictPrefixValue.Name = "textBoxRestrictPrefixValue"
        Me.textBoxRestrictPrefixValue.Size = New System.Drawing.Size(151, 20)
        Me.textBoxRestrictPrefixValue.TabIndex = 28
        '
        'groupBox1
        '
        Me.groupBox1.Controls.Add(Me.checkBoxIncludeExisting)
        Me.groupBox1.Controls.Add(Me.checkBoxRestrictPrefix)
        Me.groupBox1.Controls.Add(Me.textBoxRestrictPrefixValue)
        Me.groupBox1.Controls.Add(Me.checkBoxListReverse)
        Me.groupBox1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBox1.Location = New System.Drawing.Point(325, 69)
        Me.groupBox1.Name = "groupBox1"
        Me.groupBox1.Size = New System.Drawing.Size(178, 149)
        Me.groupBox1.TabIndex = 44
        Me.groupBox1.TabStop = False
        Me.groupBox1.Text = "List Selection Options"
        '
        'checkBoxListReverse
        '
        Me.checkBoxListReverse.AutoSize = True
        Me.checkBoxListReverse.Location = New System.Drawing.Point(11, 114)
        Me.checkBoxListReverse.Name = "checkBoxListReverse"
        Me.checkBoxListReverse.Size = New System.Drawing.Size(114, 17)
        Me.checkBoxListReverse.TabIndex = 24
        Me.checkBoxListReverse.Text = "Reverse List Order"
        Me.checkBoxListReverse.UseVisualStyleBackColor = True
        '
        'buttonCreate
        '
        Me.buttonCreate.Location = New System.Drawing.Point(12, 445)
        Me.buttonCreate.Name = "buttonCreate"
        Me.buttonCreate.Size = New System.Drawing.Size(178, 29)
        Me.buttonCreate.TabIndex = 40
        Me.buttonCreate.Text = "Create Sheets"
        Me.buttonCreate.UseVisualStyleBackColor = True
        '
        'listBoxViews
        '
        Me.listBoxViews.FormattingEnabled = True
        Me.listBoxViews.Location = New System.Drawing.Point(12, 75)
        Me.listBoxViews.Name = "listBoxViews"
        Me.listBoxViews.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.listBoxViews.Size = New System.Drawing.Size(300, 355)
        Me.listBoxViews.TabIndex = 38
        '
        'label4
        '
        Me.label4.AutoSize = True
        Me.label4.Location = New System.Drawing.Point(12, 59)
        Me.label4.Name = "label4"
        Me.label4.Size = New System.Drawing.Size(216, 13)
        Me.label4.TabIndex = 39
        Me.label4.Text = "Select Views For Which to Create an Image:"
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripProgressBar1, Me.ToolStripStatusLabel1})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 485)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(515, 22)
        Me.StatusStrip1.TabIndex = 45
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
        'form_ElemSheetsFromViews
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(515, 507)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.buttonSelect)
        Me.Controls.Add(Me.textBoxTitleblock)
        Me.Controls.Add(Me.label1)
        Me.Controls.Add(Me.buttonClose)
        Me.Controls.Add(Me.groupBox1)
        Me.Controls.Add(Me.buttonCreate)
        Me.Controls.Add(Me.listBoxViews)
        Me.Controls.Add(Me.label4)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(531, 545)
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(531, 524)
        Me.Name = "form_ElemSheetsFromViews"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Sheets From Views"
        Me.TopMost = True
        Me.groupBox1.ResumeLayout(False)
        Me.groupBox1.PerformLayout()
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents buttonSelect As System.Windows.Forms.Button
    Private WithEvents folderBrowserDialogImages As System.Windows.Forms.FolderBrowserDialog
    Private WithEvents textBoxTitleblock As System.Windows.Forms.TextBox
    Private WithEvents label1 As System.Windows.Forms.Label
    Private WithEvents checkBoxIncludeExisting As System.Windows.Forms.CheckBox
    Private WithEvents buttonClose As System.Windows.Forms.Button
    Private WithEvents checkBoxRestrictPrefix As System.Windows.Forms.CheckBox
    Private WithEvents textBoxRestrictPrefixValue As System.Windows.Forms.TextBox
    Private WithEvents groupBox1 As System.Windows.Forms.GroupBox
    Private WithEvents checkBoxListReverse As System.Windows.Forms.CheckBox
    Private WithEvents buttonCreate As System.Windows.Forms.Button
    Private WithEvents listBoxViews As System.Windows.Forms.ListBox
    Private WithEvents label4 As System.Windows.Forms.Label
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents ToolStripProgressBar1 As System.Windows.Forms.ToolStripProgressBar
    Friend WithEvents ToolStripStatusLabel1 As System.Windows.Forms.ToolStripStatusLabel
End Class
