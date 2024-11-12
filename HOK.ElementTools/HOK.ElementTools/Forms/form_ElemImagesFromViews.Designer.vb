<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class form_ElemImagesFromViews
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(form_ElemImagesFromViews))
        groupBox1 = New System.Windows.Forms.GroupBox()
        checkBoxRestrictPrefix = New System.Windows.Forms.CheckBox()
        textBoxRestrictPrefixValue = New System.Windows.Forms.TextBox()
        checkBoxIncludeExisting = New System.Windows.Forms.CheckBox()
        checkBoxListReverse = New System.Windows.Forms.CheckBox()
        buttonBrowse = New System.Windows.Forms.Button()
        folderBrowserDialogImages = New System.Windows.Forms.FolderBrowserDialog()
        textBoxFolderPath = New System.Windows.Forms.TextBox()
        buttonCreate = New System.Windows.Forms.Button()
        listBoxViews = New System.Windows.Forms.ListBox()
        buttonClose = New System.Windows.Forms.Button()
        label4 = New System.Windows.Forms.Label()
        label1 = New System.Windows.Forms.Label()
        StatusStrip1 = New System.Windows.Forms.StatusStrip()
        ToolStripProgressBar1 = New System.Windows.Forms.ToolStripProgressBar()
        ToolStripStatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel()
        groupBox1.SuspendLayout()
        StatusStrip1.SuspendLayout()
        SuspendLayout()
        ' 
        ' groupBox1
        ' 
        groupBox1.Controls.Add(checkBoxRestrictPrefix)
        groupBox1.Controls.Add(textBoxRestrictPrefixValue)
        groupBox1.Controls.Add(checkBoxIncludeExisting)
        groupBox1.Controls.Add(checkBoxListReverse)
        groupBox1.ForeColor = Drawing.SystemColors.ControlText
        groupBox1.Location = New System.Drawing.Point(382, 80)
        groupBox1.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        groupBox1.Name = "groupBox1"
        groupBox1.Padding = New System.Windows.Forms.Padding(4, 3, 4, 3)
        groupBox1.Size = New System.Drawing.Size(206, 179)
        groupBox1.TabIndex = 42
        groupBox1.TabStop = False
        groupBox1.Text = "List Selection Options"
        ' 
        ' checkBoxRestrictPrefix
        ' 
        checkBoxRestrictPrefix.AutoSize = True
        checkBoxRestrictPrefix.Location = New System.Drawing.Point(13, 74)
        checkBoxRestrictPrefix.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        checkBoxRestrictPrefix.Name = "checkBoxRestrictPrefix"
        checkBoxRestrictPrefix.Size = New System.Drawing.Size(172, 19)
        checkBoxRestrictPrefix.TabIndex = 29
        checkBoxRestrictPrefix.Text = "Restrict to Names w/ Prefix:"
        checkBoxRestrictPrefix.UseVisualStyleBackColor = True
        ' 
        ' textBoxRestrictPrefixValue
        ' 
        textBoxRestrictPrefixValue.Location = New System.Drawing.Point(13, 97)
        textBoxRestrictPrefixValue.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxRestrictPrefixValue.Name = "textBoxRestrictPrefixValue"
        textBoxRestrictPrefixValue.Size = New System.Drawing.Size(184, 23)
        textBoxRestrictPrefixValue.TabIndex = 28
        ' 
        ' checkBoxIncludeExisting
        ' 
        checkBoxIncludeExisting.AutoSize = True
        checkBoxIncludeExisting.Location = New System.Drawing.Point(13, 22)
        checkBoxIncludeExisting.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        checkBoxIncludeExisting.Name = "checkBoxIncludeExisting"
        checkBoxIncludeExisting.Size = New System.Drawing.Size(155, 19)
        checkBoxIncludeExisting.TabIndex = 31
        checkBoxIncludeExisting.Text = "List and Replace Existing"
        checkBoxIncludeExisting.UseVisualStyleBackColor = True
        ' 
        ' checkBoxListReverse
        ' 
        checkBoxListReverse.AutoSize = True
        checkBoxListReverse.Location = New System.Drawing.Point(13, 152)
        checkBoxListReverse.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        checkBoxListReverse.Name = "checkBoxListReverse"
        checkBoxListReverse.Size = New System.Drawing.Size(120, 19)
        checkBoxListReverse.TabIndex = 24
        checkBoxListReverse.Text = "Reverse List Order"
        checkBoxListReverse.UseVisualStyleBackColor = True
        ' 
        ' buttonBrowse
        ' 
        buttonBrowse.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25F, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        buttonBrowse.Location = New System.Drawing.Point(556, 25)
        buttonBrowse.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        buttonBrowse.Name = "buttonBrowse"
        buttonBrowse.Size = New System.Drawing.Size(34, 30)
        buttonBrowse.TabIndex = 41
        buttonBrowse.Text = "..."
        buttonBrowse.UseVisualStyleBackColor = True
        ' 
        ' textBoxFolderPath
        ' 
        textBoxFolderPath.Location = New System.Drawing.Point(16, 29)
        textBoxFolderPath.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxFolderPath.Name = "textBoxFolderPath"
        textBoxFolderPath.Size = New System.Drawing.Size(532, 23)
        textBoxFolderPath.TabIndex = 39
        ' 
        ' buttonCreate
        ' 
        buttonCreate.Location = New System.Drawing.Point(16, 513)
        buttonCreate.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        buttonCreate.Name = "buttonCreate"
        buttonCreate.Size = New System.Drawing.Size(208, 33)
        buttonCreate.TabIndex = 38
        buttonCreate.Text = "Create Images"
        buttonCreate.UseVisualStyleBackColor = True
        ' 
        ' listBoxViews
        ' 
        listBoxViews.FormattingEnabled = True
        listBoxViews.ItemHeight = 15
        listBoxViews.Location = New System.Drawing.Point(16, 87)
        listBoxViews.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        listBoxViews.Name = "listBoxViews"
        listBoxViews.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        listBoxViews.Size = New System.Drawing.Size(349, 409)
        listBoxViews.TabIndex = 36
        ' 
        ' buttonClose
        ' 
        buttonClose.Location = New System.Drawing.Point(382, 513)
        buttonClose.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        buttonClose.Name = "buttonClose"
        buttonClose.Size = New System.Drawing.Size(208, 33)
        buttonClose.TabIndex = 35
        buttonClose.Text = "Close"
        buttonClose.UseVisualStyleBackColor = True
        ' 
        ' label4
        ' 
        label4.AutoSize = True
        label4.Location = New System.Drawing.Point(16, 68)
        label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label4.Name = "label4"
        label4.Size = New System.Drawing.Size(234, 15)
        label4.TabIndex = 37
        label4.Text = "Select Views For Which to Create an Image:"
        ' 
        ' label1
        ' 
        label1.AutoSize = True
        label1.Location = New System.Drawing.Point(14, 10)
        label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label1.Name = "label1"
        label1.Size = New System.Drawing.Size(179, 15)
        label1.TabIndex = 40
        label1.Text = "Folder In Which to Place Images:"
        ' 
        ' StatusStrip1
        ' 
        StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {ToolStripProgressBar1, ToolStripStatusLabel1})
        StatusStrip1.Location = New System.Drawing.Point(0, 739)
        StatusStrip1.Name = "StatusStrip1"
        StatusStrip1.Padding = New System.Windows.Forms.Padding(1, 0, 16, 0)
        StatusStrip1.Size = New System.Drawing.Size(601, 22)
        StatusStrip1.TabIndex = 43
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
        ' form_ElemImagesFromViews
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        ClientSize = New System.Drawing.Size(601, 761)
        Controls.Add(StatusStrip1)
        Controls.Add(groupBox1)
        Controls.Add(buttonBrowse)
        Controls.Add(textBoxFolderPath)
        Controls.Add(buttonCreate)
        Controls.Add(listBoxViews)
        Controls.Add(buttonClose)
        Controls.Add(label4)
        Controls.Add(label1)
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New System.Drawing.Size(617, 800)
        Name = "form_ElemImagesFromViews"
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "Export Images From Views"
        TopMost = True
        groupBox1.ResumeLayout(False)
        groupBox1.PerformLayout()
        StatusStrip1.ResumeLayout(False)
        StatusStrip1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()

    End Sub
    Private WithEvents groupBox1 As System.Windows.Forms.GroupBox
    Private WithEvents checkBoxRestrictPrefix As System.Windows.Forms.CheckBox
    Private WithEvents textBoxRestrictPrefixValue As System.Windows.Forms.TextBox
    Private WithEvents checkBoxIncludeExisting As System.Windows.Forms.CheckBox
    Private WithEvents checkBoxListReverse As System.Windows.Forms.CheckBox
    Private WithEvents buttonBrowse As System.Windows.Forms.Button
    Private WithEvents folderBrowserDialogImages As System.Windows.Forms.FolderBrowserDialog
    Private WithEvents textBoxFolderPath As System.Windows.Forms.TextBox
    Private WithEvents buttonCreate As System.Windows.Forms.Button
    Private WithEvents listBoxViews As System.Windows.Forms.ListBox
    Private WithEvents buttonClose As System.Windows.Forms.Button
    Private WithEvents label4 As System.Windows.Forms.Label
    Private WithEvents label1 As System.Windows.Forms.Label
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents ToolStripProgressBar1 As System.Windows.Forms.ToolStripProgressBar
    Friend WithEvents ToolStripStatusLabel1 As System.Windows.Forms.ToolStripStatusLabel
End Class
