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
        buttonSelect = New System.Windows.Forms.Button()
        folderBrowserDialogImages = New System.Windows.Forms.FolderBrowserDialog()
        textBoxTitleblock = New System.Windows.Forms.TextBox()
        label1 = New System.Windows.Forms.Label()
        checkBoxIncludeExisting = New System.Windows.Forms.CheckBox()
        buttonClose = New System.Windows.Forms.Button()
        checkBoxRestrictPrefix = New System.Windows.Forms.CheckBox()
        textBoxRestrictPrefixValue = New System.Windows.Forms.TextBox()
        groupBox1 = New System.Windows.Forms.GroupBox()
        checkBoxListReverse = New System.Windows.Forms.CheckBox()
        buttonCreate = New System.Windows.Forms.Button()
        listBoxViews = New System.Windows.Forms.ListBox()
        label4 = New System.Windows.Forms.Label()
        StatusStrip1 = New System.Windows.Forms.StatusStrip()
        ToolStripProgressBar1 = New System.Windows.Forms.ToolStripProgressBar()
        ToolStripStatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel()
        groupBox1.SuspendLayout()
        StatusStrip1.SuspendLayout()
        SuspendLayout()
        ' 
        ' buttonSelect
        ' 
        buttonSelect.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25F, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        buttonSelect.Location = New System.Drawing.Point(553, 24)
        buttonSelect.Margin = New System.Windows.Forms.Padding(4)
        buttonSelect.Name = "buttonSelect"
        buttonSelect.Size = New System.Drawing.Size(34, 30)
        buttonSelect.TabIndex = 43
        buttonSelect.Text = "..."
        buttonSelect.UseVisualStyleBackColor = True
        ' 
        ' textBoxTitleblock
        ' 
        textBoxTitleblock.Location = New System.Drawing.Point(14, 29)
        textBoxTitleblock.Margin = New System.Windows.Forms.Padding(4)
        textBoxTitleblock.Name = "textBoxTitleblock"
        textBoxTitleblock.Size = New System.Drawing.Size(532, 23)
        textBoxTitleblock.TabIndex = 41
        ' 
        ' label1
        ' 
        label1.AutoSize = True
        label1.Location = New System.Drawing.Point(14, 10)
        label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label1.Name = "label1"
        label1.Size = New System.Drawing.Size(179, 15)
        label1.TabIndex = 42
        label1.Text = "Titleblock to Use for New Sheets:"
        ' 
        ' checkBoxIncludeExisting
        ' 
        checkBoxIncludeExisting.AutoSize = True
        checkBoxIncludeExisting.Location = New System.Drawing.Point(13, 25)
        checkBoxIncludeExisting.Margin = New System.Windows.Forms.Padding(4)
        checkBoxIncludeExisting.Name = "checkBoxIncludeExisting"
        checkBoxIncludeExisting.Size = New System.Drawing.Size(155, 19)
        checkBoxIncludeExisting.TabIndex = 31
        checkBoxIncludeExisting.Text = "List and Replace Existing"
        checkBoxIncludeExisting.UseVisualStyleBackColor = True
        ' 
        ' buttonClose
        ' 
        buttonClose.Location = New System.Drawing.Point(370, 514)
        buttonClose.Margin = New System.Windows.Forms.Padding(4)
        buttonClose.Name = "buttonClose"
        buttonClose.Size = New System.Drawing.Size(208, 34)
        buttonClose.TabIndex = 37
        buttonClose.Text = "Close"
        buttonClose.UseVisualStyleBackColor = True
        ' 
        ' checkBoxRestrictPrefix
        ' 
        checkBoxRestrictPrefix.AutoSize = True
        checkBoxRestrictPrefix.Location = New System.Drawing.Point(13, 65)
        checkBoxRestrictPrefix.Margin = New System.Windows.Forms.Padding(4)
        checkBoxRestrictPrefix.Name = "checkBoxRestrictPrefix"
        checkBoxRestrictPrefix.Size = New System.Drawing.Size(172, 19)
        checkBoxRestrictPrefix.TabIndex = 29
        checkBoxRestrictPrefix.Text = "Restrict to Names w/ Prefix:"
        checkBoxRestrictPrefix.UseVisualStyleBackColor = True
        ' 
        ' textBoxRestrictPrefixValue
        ' 
        textBoxRestrictPrefixValue.Location = New System.Drawing.Point(13, 85)
        textBoxRestrictPrefixValue.Margin = New System.Windows.Forms.Padding(4)
        textBoxRestrictPrefixValue.Name = "textBoxRestrictPrefixValue"
        textBoxRestrictPrefixValue.Size = New System.Drawing.Size(176, 23)
        textBoxRestrictPrefixValue.TabIndex = 28
        ' 
        ' groupBox1
        ' 
        groupBox1.Controls.Add(checkBoxIncludeExisting)
        groupBox1.Controls.Add(checkBoxRestrictPrefix)
        groupBox1.Controls.Add(textBoxRestrictPrefixValue)
        groupBox1.Controls.Add(checkBoxListReverse)
        groupBox1.ForeColor = Drawing.SystemColors.ControlText
        groupBox1.Location = New System.Drawing.Point(379, 80)
        groupBox1.Margin = New System.Windows.Forms.Padding(4)
        groupBox1.Name = "groupBox1"
        groupBox1.Padding = New System.Windows.Forms.Padding(4)
        groupBox1.Size = New System.Drawing.Size(208, 172)
        groupBox1.TabIndex = 44
        groupBox1.TabStop = False
        groupBox1.Text = "List Selection Options"
        ' 
        ' checkBoxListReverse
        ' 
        checkBoxListReverse.AutoSize = True
        checkBoxListReverse.Location = New System.Drawing.Point(13, 131)
        checkBoxListReverse.Margin = New System.Windows.Forms.Padding(4)
        checkBoxListReverse.Name = "checkBoxListReverse"
        checkBoxListReverse.Size = New System.Drawing.Size(120, 19)
        checkBoxListReverse.TabIndex = 24
        checkBoxListReverse.Text = "Reverse List Order"
        checkBoxListReverse.UseVisualStyleBackColor = True
        ' 
        ' buttonCreate
        ' 
        buttonCreate.Location = New System.Drawing.Point(14, 514)
        buttonCreate.Margin = New System.Windows.Forms.Padding(4)
        buttonCreate.Name = "buttonCreate"
        buttonCreate.Size = New System.Drawing.Size(208, 34)
        buttonCreate.TabIndex = 40
        buttonCreate.Text = "Create Sheets"
        buttonCreate.UseVisualStyleBackColor = True
        ' 
        ' listBoxViews
        ' 
        listBoxViews.FormattingEnabled = True
        listBoxViews.ItemHeight = 15
        listBoxViews.Location = New System.Drawing.Point(14, 86)
        listBoxViews.Margin = New System.Windows.Forms.Padding(4)
        listBoxViews.Name = "listBoxViews"
        listBoxViews.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        listBoxViews.Size = New System.Drawing.Size(350, 409)
        listBoxViews.TabIndex = 38
        ' 
        ' label4
        ' 
        label4.AutoSize = True
        label4.Location = New System.Drawing.Point(14, 68)
        label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label4.Name = "label4"
        label4.Size = New System.Drawing.Size(234, 15)
        label4.TabIndex = 39
        label4.Text = "Select Views For Which to Create an Image:"
        ' 
        ' StatusStrip1
        ' 
        StatusStrip1.ImageScalingSize = New System.Drawing.Size(36, 36)
        StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {ToolStripProgressBar1, ToolStripStatusLabel1})
        StatusStrip1.Location = New System.Drawing.Point(0, 559)
        StatusStrip1.Name = "StatusStrip1"
        StatusStrip1.Padding = New System.Windows.Forms.Padding(1, 0, 16, 0)
        StatusStrip1.Size = New System.Drawing.Size(593, 22)
        StatusStrip1.TabIndex = 45
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
        ' form_ElemSheetsFromViews
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        ClientSize = New System.Drawing.Size(593, 581)
        Controls.Add(StatusStrip1)
        Controls.Add(buttonSelect)
        Controls.Add(textBoxTitleblock)
        Controls.Add(label1)
        Controls.Add(buttonClose)
        Controls.Add(groupBox1)
        Controls.Add(buttonCreate)
        Controls.Add(listBoxViews)
        Controls.Add(label4)
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        Margin = New System.Windows.Forms.Padding(4)
        MaximizeBox = False
        MaximumSize = New System.Drawing.Size(609, 800)
        MinimizeBox = False
        MinimumSize = New System.Drawing.Size(609, 620)
        Name = "form_ElemSheetsFromViews"
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "Sheets From Views"
        TopMost = True
        groupBox1.ResumeLayout(False)
        groupBox1.PerformLayout()
        StatusStrip1.ResumeLayout(False)
        StatusStrip1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()

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
