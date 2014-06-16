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
        Me.groupBox1 = New System.Windows.Forms.GroupBox()
        Me.checkBoxRestrictPrefix = New System.Windows.Forms.CheckBox()
        Me.textBoxRestrictPrefixValue = New System.Windows.Forms.TextBox()
        Me.checkBoxIncludeExisting = New System.Windows.Forms.CheckBox()
        Me.checkBoxListReverse = New System.Windows.Forms.CheckBox()
        Me.buttonBrowse = New System.Windows.Forms.Button()
        Me.folderBrowserDialogImages = New System.Windows.Forms.FolderBrowserDialog()
        Me.textBoxFolderPath = New System.Windows.Forms.TextBox()
        Me.buttonCreate = New System.Windows.Forms.Button()
        Me.listBoxViews = New System.Windows.Forms.ListBox()
        Me.buttonClose = New System.Windows.Forms.Button()
        Me.label4 = New System.Windows.Forms.Label()
        Me.label1 = New System.Windows.Forms.Label()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.ToolStripProgressBar1 = New System.Windows.Forms.ToolStripProgressBar()
        Me.ToolStripStatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.groupBox1.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'groupBox1
        '
        Me.groupBox1.Controls.Add(Me.checkBoxRestrictPrefix)
        Me.groupBox1.Controls.Add(Me.textBoxRestrictPrefixValue)
        Me.groupBox1.Controls.Add(Me.checkBoxIncludeExisting)
        Me.groupBox1.Controls.Add(Me.checkBoxListReverse)
        Me.groupBox1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBox1.Location = New System.Drawing.Point(327, 69)
        Me.groupBox1.Name = "groupBox1"
        Me.groupBox1.Size = New System.Drawing.Size(177, 155)
        Me.groupBox1.TabIndex = 42
        Me.groupBox1.TabStop = False
        Me.groupBox1.Text = "List Selection Options"
        '
        'checkBoxRestrictPrefix
        '
        Me.checkBoxRestrictPrefix.AutoSize = True
        Me.checkBoxRestrictPrefix.Location = New System.Drawing.Point(11, 64)
        Me.checkBoxRestrictPrefix.Name = "checkBoxRestrictPrefix"
        Me.checkBoxRestrictPrefix.Size = New System.Drawing.Size(158, 17)
        Me.checkBoxRestrictPrefix.TabIndex = 29
        Me.checkBoxRestrictPrefix.Text = "Restrict to Names w/ Prefix:"
        Me.checkBoxRestrictPrefix.UseVisualStyleBackColor = True
        '
        'textBoxRestrictPrefixValue
        '
        Me.textBoxRestrictPrefixValue.Location = New System.Drawing.Point(11, 84)
        Me.textBoxRestrictPrefixValue.Name = "textBoxRestrictPrefixValue"
        Me.textBoxRestrictPrefixValue.Size = New System.Drawing.Size(158, 20)
        Me.textBoxRestrictPrefixValue.TabIndex = 28
        '
        'checkBoxIncludeExisting
        '
        Me.checkBoxIncludeExisting.AutoSize = True
        Me.checkBoxIncludeExisting.Location = New System.Drawing.Point(11, 19)
        Me.checkBoxIncludeExisting.Name = "checkBoxIncludeExisting"
        Me.checkBoxIncludeExisting.Size = New System.Drawing.Size(145, 17)
        Me.checkBoxIncludeExisting.TabIndex = 31
        Me.checkBoxIncludeExisting.Text = "List and Replace Existing"
        Me.checkBoxIncludeExisting.UseVisualStyleBackColor = True
        '
        'checkBoxListReverse
        '
        Me.checkBoxListReverse.AutoSize = True
        Me.checkBoxListReverse.Location = New System.Drawing.Point(11, 132)
        Me.checkBoxListReverse.Name = "checkBoxListReverse"
        Me.checkBoxListReverse.Size = New System.Drawing.Size(114, 17)
        Me.checkBoxListReverse.TabIndex = 24
        Me.checkBoxListReverse.Text = "Reverse List Order"
        Me.checkBoxListReverse.UseVisualStyleBackColor = True
        '
        'buttonBrowse
        '
        Me.buttonBrowse.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.buttonBrowse.Location = New System.Drawing.Point(477, 22)
        Me.buttonBrowse.Name = "buttonBrowse"
        Me.buttonBrowse.Size = New System.Drawing.Size(29, 26)
        Me.buttonBrowse.TabIndex = 41
        Me.buttonBrowse.Text = "..."
        Me.buttonBrowse.UseVisualStyleBackColor = True
        '
        'textBoxFolderPath
        '
        Me.textBoxFolderPath.Location = New System.Drawing.Point(14, 25)
        Me.textBoxFolderPath.Name = "textBoxFolderPath"
        Me.textBoxFolderPath.Size = New System.Drawing.Size(457, 20)
        Me.textBoxFolderPath.TabIndex = 39
        '
        'buttonCreate
        '
        Me.buttonCreate.Location = New System.Drawing.Point(14, 445)
        Me.buttonCreate.Name = "buttonCreate"
        Me.buttonCreate.Size = New System.Drawing.Size(178, 29)
        Me.buttonCreate.TabIndex = 38
        Me.buttonCreate.Text = "Create Images"
        Me.buttonCreate.UseVisualStyleBackColor = True
        '
        'listBoxViews
        '
        Me.listBoxViews.FormattingEnabled = True
        Me.listBoxViews.Location = New System.Drawing.Point(14, 75)
        Me.listBoxViews.Name = "listBoxViews"
        Me.listBoxViews.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.listBoxViews.Size = New System.Drawing.Size(300, 355)
        Me.listBoxViews.TabIndex = 36
        '
        'buttonClose
        '
        Me.buttonClose.Location = New System.Drawing.Point(327, 445)
        Me.buttonClose.Name = "buttonClose"
        Me.buttonClose.Size = New System.Drawing.Size(178, 29)
        Me.buttonClose.TabIndex = 35
        Me.buttonClose.Text = "Close"
        Me.buttonClose.UseVisualStyleBackColor = True
        '
        'label4
        '
        Me.label4.AutoSize = True
        Me.label4.Location = New System.Drawing.Point(14, 59)
        Me.label4.Name = "label4"
        Me.label4.Size = New System.Drawing.Size(216, 13)
        Me.label4.TabIndex = 37
        Me.label4.Text = "Select Views For Which to Create an Image:"
        '
        'label1
        '
        Me.label1.AutoSize = True
        Me.label1.Location = New System.Drawing.Point(12, 9)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(164, 13)
        Me.label1.TabIndex = 40
        Me.label1.Text = "Folder In Which to Place Images:"
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripProgressBar1, Me.ToolStripStatusLabel1})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 482)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(515, 22)
        Me.StatusStrip1.TabIndex = 43
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
        'form_ElemImagesFromViews
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(515, 504)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.groupBox1)
        Me.Controls.Add(Me.buttonBrowse)
        Me.Controls.Add(Me.textBoxFolderPath)
        Me.Controls.Add(Me.buttonCreate)
        Me.Controls.Add(Me.listBoxViews)
        Me.Controls.Add(Me.buttonClose)
        Me.Controls.Add(Me.label4)
        Me.Controls.Add(Me.label1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(531, 522)
        Me.Name = "form_ElemImagesFromViews"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Export Images From Views"
        Me.TopMost = True
        Me.groupBox1.ResumeLayout(False)
        Me.groupBox1.PerformLayout()
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

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
