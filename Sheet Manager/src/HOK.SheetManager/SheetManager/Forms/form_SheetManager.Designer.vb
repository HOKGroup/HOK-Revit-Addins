<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class form_SheetManager
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(form_SheetManager))
        Me.ListBoxTableSet = New System.Windows.Forms.ListBox()
        Me.TreeViewTitleBlocks = New System.Windows.Forms.TreeView()
        Me.openFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.checkBoxUpdateExisting = New System.Windows.Forms.CheckBox()
        Me.GroupBoxTitleblocks = New System.Windows.Forms.GroupBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.LabelFilePath = New System.Windows.Forms.Label()
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.TreeViewSheets = New System.Windows.Forms.TreeView()
        Me.GroupBoxSheets = New System.Windows.Forms.GroupBox()
        Me.ButtonConnectAccess = New System.Windows.Forms.Button()
        Me.buttonExportData = New System.Windows.Forms.Button()
        Me.buttonRenumberSheets = New System.Windows.Forms.Button()
        Me.buttonAddViewsToSheets = New System.Windows.Forms.Button()
        Me.buttonUpdateCreate = New System.Windows.Forms.Button()
        Me.buttonClose = New System.Windows.Forms.Button()
        Me.ButtonConnectExcel = New System.Windows.Forms.Button()
        Me.linkAbout = New System.Windows.Forms.LinkLabel()
        Me.linkHelp = New System.Windows.Forms.LinkLabel()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.iniPathLabel = New System.Windows.Forms.ToolStripStatusLabel()
        Me.GroupBoxTitleblocks.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBoxSheets.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'ListBoxTableSet
        '
        Me.ListBoxTableSet.FormattingEnabled = True
        Me.ListBoxTableSet.Location = New System.Drawing.Point(6, 19)
        Me.ListBoxTableSet.Name = "ListBoxTableSet"
        Me.ListBoxTableSet.Size = New System.Drawing.Size(320, 147)
        Me.ListBoxTableSet.TabIndex = 13
        '
        'TreeViewTitleBlocks
        '
        Me.TreeViewTitleBlocks.HideSelection = False
        Me.TreeViewTitleBlocks.Location = New System.Drawing.Point(6, 15)
        Me.TreeViewTitleBlocks.Name = "TreeViewTitleBlocks"
        Me.TreeViewTitleBlocks.Size = New System.Drawing.Size(320, 151)
        Me.TreeViewTitleBlocks.TabIndex = 27
        '
        'openFileDialog1
        '
        Me.openFileDialog1.FileName = "openFileDialog1"
        '
        'checkBoxUpdateExisting
        '
        Me.checkBoxUpdateExisting.Location = New System.Drawing.Point(691, 273)
        Me.checkBoxUpdateExisting.Name = "checkBoxUpdateExisting"
        Me.checkBoxUpdateExisting.Size = New System.Drawing.Size(142, 43)
        Me.checkBoxUpdateExisting.TabIndex = 32
        Me.checkBoxUpdateExisting.Text = "Update Existing Values"
        Me.checkBoxUpdateExisting.UseVisualStyleBackColor = True
        '
        'GroupBoxTitleblocks
        '
        Me.GroupBoxTitleblocks.Controls.Add(Me.TreeViewTitleBlocks)
        Me.GroupBoxTitleblocks.Location = New System.Drawing.Point(344, 258)
        Me.GroupBoxTitleblocks.Name = "GroupBoxTitleblocks"
        Me.GroupBoxTitleblocks.Size = New System.Drawing.Size(332, 177)
        Me.GroupBoxTitleblocks.TabIndex = 37
        Me.GroupBoxTitleblocks.TabStop = False
        Me.GroupBoxTitleblocks.Text = "Select a Titleblock for New Sheets"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.LabelFilePath)
        Me.GroupBox2.Location = New System.Drawing.Point(12, 12)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(830, 57)
        Me.GroupBox2.TabIndex = 38
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Sheet Manager Input/Output File Path"
        '
        'LabelFilePath
        '
        Me.LabelFilePath.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LabelFilePath.ForeColor = System.Drawing.Color.Blue
        Me.LabelFilePath.Location = New System.Drawing.Point(6, 15)
        Me.LabelFilePath.Name = "LabelFilePath"
        Me.LabelFilePath.Size = New System.Drawing.Size(815, 34)
        Me.LabelFilePath.TabIndex = 45
        Me.LabelFilePath.Text = "Connect to Access or Excel File to Continue"
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(12, 440)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(830, 60)
        Me.ProgressBar1.TabIndex = 46
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.ListBoxTableSet)
        Me.GroupBox1.Location = New System.Drawing.Point(344, 75)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(332, 177)
        Me.GroupBox1.TabIndex = 47
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Select Sheet Data Source Table or Worksheet"
        '
        'TreeViewSheets
        '
        Me.TreeViewSheets.HideSelection = False
        Me.TreeViewSheets.Location = New System.Drawing.Point(6, 19)
        Me.TreeViewSheets.Name = "TreeViewSheets"
        Me.TreeViewSheets.Size = New System.Drawing.Size(314, 330)
        Me.TreeViewSheets.TabIndex = 28
        '
        'GroupBoxSheets
        '
        Me.GroupBoxSheets.Controls.Add(Me.TreeViewSheets)
        Me.GroupBoxSheets.Location = New System.Drawing.Point(12, 75)
        Me.GroupBoxSheets.Name = "GroupBoxSheets"
        Me.GroupBoxSheets.Size = New System.Drawing.Size(326, 360)
        Me.GroupBoxSheets.TabIndex = 48
        Me.GroupBoxSheets.TabStop = False
        Me.GroupBoxSheets.Text = "Sheet Elements"
        '
        'ButtonConnectAccess
        '
        Me.ButtonConnectAccess.Image = Global.HOK.SheetManager.My.Resources.Resources.MsAccess_Med
        Me.ButtonConnectAccess.ImageAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.ButtonConnectAccess.Location = New System.Drawing.Point(682, 181)
        Me.ButtonConnectAccess.Name = "ButtonConnectAccess"
        Me.ButtonConnectAccess.Size = New System.Drawing.Size(160, 60)
        Me.ButtonConnectAccess.TabIndex = 43
        Me.ButtonConnectAccess.Text = "Connect Access"
        Me.ButtonConnectAccess.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.ButtonConnectAccess.UseVisualStyleBackColor = True
        '
        'buttonExportData
        '
        Me.buttonExportData.Image = Global.HOK.SheetManager.My.Resources.Resources._002
        Me.buttonExportData.ImageAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.buttonExportData.Location = New System.Drawing.Point(350, 441)
        Me.buttonExportData.Name = "buttonExportData"
        Me.buttonExportData.Size = New System.Drawing.Size(160, 60)
        Me.buttonExportData.TabIndex = 35
        Me.buttonExportData.Text = "Export Sheet  Data"
        Me.buttonExportData.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.buttonExportData.UseVisualStyleBackColor = True
        '
        'buttonRenumberSheets
        '
        Me.buttonRenumberSheets.Image = Global.HOK.SheetManager.My.Resources.Resources.Quantity
        Me.buttonRenumberSheets.ImageAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.buttonRenumberSheets.Location = New System.Drawing.Point(12, 441)
        Me.buttonRenumberSheets.Name = "buttonRenumberSheets"
        Me.buttonRenumberSheets.Size = New System.Drawing.Size(160, 60)
        Me.buttonRenumberSheets.TabIndex = 34
        Me.buttonRenumberSheets.Text = "Renumber Sheets"
        Me.buttonRenumberSheets.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.buttonRenumberSheets.UseVisualStyleBackColor = True
        '
        'buttonAddViewsToSheets
        '
        Me.buttonAddViewsToSheets.Image = Global.HOK.SheetManager.My.Resources.Resources.ExportOrOpenAs
        Me.buttonAddViewsToSheets.ImageAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.buttonAddViewsToSheets.Location = New System.Drawing.Point(178, 441)
        Me.buttonAddViewsToSheets.Name = "buttonAddViewsToSheets"
        Me.buttonAddViewsToSheets.Size = New System.Drawing.Size(160, 60)
        Me.buttonAddViewsToSheets.TabIndex = 36
        Me.buttonAddViewsToSheets.Text = "Add Views to Sheets"
        Me.buttonAddViewsToSheets.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.buttonAddViewsToSheets.UseVisualStyleBackColor = True
        '
        'buttonUpdateCreate
        '
        Me.buttonUpdateCreate.Image = Global.HOK.SheetManager.My.Resources.Resources.Copy_of_AdmTmpl_dll_I0004_04095
        Me.buttonUpdateCreate.ImageAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.buttonUpdateCreate.Location = New System.Drawing.Point(516, 441)
        Me.buttonUpdateCreate.Name = "buttonUpdateCreate"
        Me.buttonUpdateCreate.Size = New System.Drawing.Size(160, 60)
        Me.buttonUpdateCreate.TabIndex = 31
        Me.buttonUpdateCreate.Text = "Create/Update Sheets"
        Me.buttonUpdateCreate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.buttonUpdateCreate.UseVisualStyleBackColor = True
        '
        'buttonClose
        '
        Me.buttonClose.Image = Global.HOK.SheetManager.My.Resources.Resources.Close
        Me.buttonClose.ImageAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.buttonClose.Location = New System.Drawing.Point(682, 441)
        Me.buttonClose.Name = "buttonClose"
        Me.buttonClose.Size = New System.Drawing.Size(160, 60)
        Me.buttonClose.TabIndex = 30
        Me.buttonClose.Text = "Close"
        Me.buttonClose.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.buttonClose.UseVisualStyleBackColor = True
        '
        'ButtonConnectExcel
        '
        Me.ButtonConnectExcel.Image = Global.HOK.SheetManager.My.Resources.Resources.Excel2007Logo_Med
        Me.ButtonConnectExcel.ImageAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.ButtonConnectExcel.Location = New System.Drawing.Point(682, 94)
        Me.ButtonConnectExcel.Name = "ButtonConnectExcel"
        Me.ButtonConnectExcel.Size = New System.Drawing.Size(160, 60)
        Me.ButtonConnectExcel.TabIndex = 42
        Me.ButtonConnectExcel.Text = "Connect Excel"
        Me.ButtonConnectExcel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.ButtonConnectExcel.UseVisualStyleBackColor = True
        '
        'linkAbout
        '
        Me.linkAbout.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.linkAbout.AutoSize = True
        Me.linkAbout.Location = New System.Drawing.Point(63, 516)
        Me.linkAbout.Name = "linkAbout"
        Me.linkAbout.Size = New System.Drawing.Size(35, 13)
        Me.linkAbout.TabIndex = 50
        Me.linkAbout.TabStop = True
        Me.linkAbout.Text = "About"
        '
        'linkHelp
        '
        Me.linkHelp.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.linkHelp.AutoSize = True
        Me.linkHelp.Location = New System.Drawing.Point(18, 516)
        Me.linkHelp.Name = "linkHelp"
        Me.linkHelp.Size = New System.Drawing.Size(29, 13)
        Me.linkHelp.TabIndex = 49
        Me.linkHelp.TabStop = True
        Me.linkHelp.Text = "Help"
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.iniPathLabel})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 540)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(854, 22)
        Me.StatusStrip1.TabIndex = 51
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'iniPathLabel
        '
        Me.iniPathLabel.Name = "iniPathLabel"
        Me.iniPathLabel.Size = New System.Drawing.Size(0, 17)
        '
        'form_SheetManager
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(854, 562)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.linkAbout)
        Me.Controls.Add(Me.linkHelp)
        Me.Controls.Add(Me.GroupBoxSheets)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.ButtonConnectAccess)
        Me.Controls.Add(Me.buttonExportData)
        Me.Controls.Add(Me.buttonRenumberSheets)
        Me.Controls.Add(Me.buttonAddViewsToSheets)
        Me.Controls.Add(Me.buttonUpdateCreate)
        Me.Controls.Add(Me.buttonClose)
        Me.Controls.Add(Me.checkBoxUpdateExisting)
        Me.Controls.Add(Me.GroupBoxTitleblocks)
        Me.Controls.Add(Me.ButtonConnectExcel)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(870, 600)
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(870, 600)
        Me.Name = "form_SheetManager"
        Me.Text = "Sheet Management"
        Me.GroupBoxTitleblocks.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBoxSheets.ResumeLayout(False)
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents ListBoxTableSet As System.Windows.Forms.ListBox
    Private WithEvents buttonExportData As System.Windows.Forms.Button
    Private WithEvents buttonRenumberSheets As System.Windows.Forms.Button
    Private WithEvents buttonAddViewsToSheets As System.Windows.Forms.Button
    Private WithEvents buttonUpdateCreate As System.Windows.Forms.Button
    Friend WithEvents TreeViewTitleBlocks As System.Windows.Forms.TreeView
    Private WithEvents buttonClose As System.Windows.Forms.Button
    Private WithEvents openFileDialog1 As System.Windows.Forms.OpenFileDialog
    Private WithEvents checkBoxUpdateExisting As System.Windows.Forms.CheckBox
    Friend WithEvents GroupBoxTitleblocks As System.Windows.Forms.GroupBox
    Private WithEvents ButtonConnectExcel As System.Windows.Forms.Button
    Private WithEvents ButtonConnectAccess As System.Windows.Forms.Button
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents LabelFilePath As System.Windows.Forms.Label
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents TreeViewSheets As System.Windows.Forms.TreeView
    Friend WithEvents GroupBoxSheets As System.Windows.Forms.GroupBox
    Private WithEvents linkAbout As System.Windows.Forms.LinkLabel
    Private WithEvents linkHelp As System.Windows.Forms.LinkLabel
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents iniPathLabel As System.Windows.Forms.ToolStripStatusLabel
End Class
