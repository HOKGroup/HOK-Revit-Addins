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
        Me.ComboBoxFilter = New System.Windows.Forms.ComboBox()
        Me.LabelDisplay = New System.Windows.Forms.Label()
        Me.ButtonUncheck = New System.Windows.Forms.Button()
        Me.ButtonCheckAll = New System.Windows.Forms.Button()
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
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.RadioButtonPlaceholder = New System.Windows.Forms.RadioButton()
        Me.RadioButtonViewSheet = New System.Windows.Forms.RadioButton()
        Me.GroupBoxTitleblocks.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBoxSheets.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.SuspendLayout()
        '
        'ListBoxTableSet
        '
        Me.ListBoxTableSet.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ListBoxTableSet.FormattingEnabled = True
        Me.ListBoxTableSet.Location = New System.Drawing.Point(7, 20)
        Me.ListBoxTableSet.Name = "ListBoxTableSet"
        Me.ListBoxTableSet.Size = New System.Drawing.Size(252, 175)
        Me.ListBoxTableSet.TabIndex = 13
        '
        'TreeViewTitleBlocks
        '
        Me.TreeViewTitleBlocks.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TreeViewTitleBlocks.HideSelection = False
        Me.TreeViewTitleBlocks.Location = New System.Drawing.Point(7, 20)
        Me.TreeViewTitleBlocks.Name = "TreeViewTitleBlocks"
        Me.TreeViewTitleBlocks.Size = New System.Drawing.Size(252, 172)
        Me.TreeViewTitleBlocks.TabIndex = 27
        '
        'openFileDialog1
        '
        Me.openFileDialog1.FileName = "openFileDialog1"
        '
        'checkBoxUpdateExisting
        '
        Me.checkBoxUpdateExisting.Checked = True
        Me.checkBoxUpdateExisting.CheckState = System.Windows.Forms.CheckState.Checked
        Me.checkBoxUpdateExisting.Location = New System.Drawing.Point(691, 255)
        Me.checkBoxUpdateExisting.Name = "checkBoxUpdateExisting"
        Me.checkBoxUpdateExisting.Size = New System.Drawing.Size(142, 43)
        Me.checkBoxUpdateExisting.TabIndex = 32
        Me.checkBoxUpdateExisting.Text = "Update Existing Values"
        Me.checkBoxUpdateExisting.UseVisualStyleBackColor = True
        '
        'GroupBoxTitleblocks
        '
        Me.GroupBoxTitleblocks.Controls.Add(Me.TreeViewTitleBlocks)
        Me.GroupBoxTitleblocks.Location = New System.Drawing.Point(410, 283)
        Me.GroupBoxTitleblocks.Name = "GroupBoxTitleblocks"
        Me.GroupBoxTitleblocks.Padding = New System.Windows.Forms.Padding(7)
        Me.GroupBoxTitleblocks.Size = New System.Drawing.Size(266, 199)
        Me.GroupBoxTitleblocks.TabIndex = 37
        Me.GroupBoxTitleblocks.TabStop = False
        Me.GroupBoxTitleblocks.Text = "Select a Titleblock for New Sheets"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.LabelFilePath)
        Me.GroupBox2.Location = New System.Drawing.Point(12, 12)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(850, 57)
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
        Me.ProgressBar1.Location = New System.Drawing.Point(13, 488)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(830, 60)
        Me.ProgressBar1.TabIndex = 46
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.ListBoxTableSet)
        Me.GroupBox1.Location = New System.Drawing.Point(410, 75)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(7)
        Me.GroupBox1.Size = New System.Drawing.Size(266, 202)
        Me.GroupBox1.TabIndex = 47
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Select Sheet Data Source Table or Worksheet"
        '
        'TreeViewSheets
        '
        Me.TreeViewSheets.CheckBoxes = True
        Me.TreeViewSheets.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TreeViewSheets.HideSelection = False
        Me.TreeViewSheets.Location = New System.Drawing.Point(7, 48)
        Me.TreeViewSheets.Name = "TreeViewSheets"
        Me.TreeViewSheets.Size = New System.Drawing.Size(378, 352)
        Me.TreeViewSheets.TabIndex = 28
        '
        'GroupBoxSheets
        '
        Me.GroupBoxSheets.Controls.Add(Me.ComboBoxFilter)
        Me.GroupBoxSheets.Controls.Add(Me.LabelDisplay)
        Me.GroupBoxSheets.Controls.Add(Me.ButtonUncheck)
        Me.GroupBoxSheets.Controls.Add(Me.ButtonCheckAll)
        Me.GroupBoxSheets.Controls.Add(Me.TreeViewSheets)
        Me.GroupBoxSheets.Location = New System.Drawing.Point(12, 75)
        Me.GroupBoxSheets.Name = "GroupBoxSheets"
        Me.GroupBoxSheets.Padding = New System.Windows.Forms.Padding(7, 35, 7, 7)
        Me.GroupBoxSheets.Size = New System.Drawing.Size(392, 407)
        Me.GroupBoxSheets.TabIndex = 48
        Me.GroupBoxSheets.TabStop = False
        Me.GroupBoxSheets.Text = "Sheet Elements"
        '
        'ComboBoxFilter
        '
        Me.ComboBoxFilter.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ComboBoxFilter.FormattingEnabled = True
        Me.ComboBoxFilter.Items.AddRange(New Object() {"Select All Sheets", "New Sheets Only", "Existing Sheets Only"})
        Me.ComboBoxFilter.Location = New System.Drawing.Point(264, 22)
        Me.ComboBoxFilter.Name = "ComboBoxFilter"
        Me.ComboBoxFilter.Size = New System.Drawing.Size(121, 21)
        Me.ComboBoxFilter.TabIndex = 32
        '
        'LabelDisplay
        '
        Me.LabelDisplay.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LabelDisplay.AutoSize = True
        Me.LabelDisplay.Location = New System.Drawing.Point(162, 25)
        Me.LabelDisplay.Name = "LabelDisplay"
        Me.LabelDisplay.Size = New System.Drawing.Size(96, 13)
        Me.LabelDisplay.TabIndex = 31
        Me.LabelDisplay.Text = "Selection Options: "
        '
        'ButtonUncheck
        '
        Me.ButtonUncheck.Image = CType(resources.GetObject("ButtonUncheck.Image"), System.Drawing.Image)
        Me.ButtonUncheck.Location = New System.Drawing.Point(39, 20)
        Me.ButtonUncheck.Name = "ButtonUncheck"
        Me.ButtonUncheck.Size = New System.Drawing.Size(22, 22)
        Me.ButtonUncheck.TabIndex = 30
        Me.ButtonUncheck.UseVisualStyleBackColor = True
        '
        'ButtonCheckAll
        '
        Me.ButtonCheckAll.Image = CType(resources.GetObject("ButtonCheckAll.Image"), System.Drawing.Image)
        Me.ButtonCheckAll.Location = New System.Drawing.Point(13, 20)
        Me.ButtonCheckAll.Name = "ButtonCheckAll"
        Me.ButtonCheckAll.Size = New System.Drawing.Size(22, 22)
        Me.ButtonCheckAll.TabIndex = 29
        Me.ButtonCheckAll.UseVisualStyleBackColor = True
        '
        'ButtonConnectAccess
        '
        Me.ButtonConnectAccess.Image = Global.HOK.SheetManager.My.Resources.Resources.MsAccess_Med
        Me.ButtonConnectAccess.ImageAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.ButtonConnectAccess.Location = New System.Drawing.Point(691, 165)
        Me.ButtonConnectAccess.Name = "ButtonConnectAccess"
        Me.ButtonConnectAccess.Size = New System.Drawing.Size(171, 60)
        Me.ButtonConnectAccess.TabIndex = 43
        Me.ButtonConnectAccess.Text = "Connect Access"
        Me.ButtonConnectAccess.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.ButtonConnectAccess.UseVisualStyleBackColor = True
        '
        'buttonExportData
        '
        Me.buttonExportData.Image = Global.HOK.SheetManager.My.Resources.Resources._002
        Me.buttonExportData.ImageAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.buttonExportData.Location = New System.Drawing.Point(350, 488)
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
        Me.buttonRenumberSheets.Location = New System.Drawing.Point(12, 488)
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
        Me.buttonAddViewsToSheets.Location = New System.Drawing.Point(178, 488)
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
        Me.buttonUpdateCreate.Location = New System.Drawing.Point(516, 488)
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
        Me.buttonClose.Location = New System.Drawing.Point(682, 488)
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
        Me.ButtonConnectExcel.Location = New System.Drawing.Point(691, 86)
        Me.ButtonConnectExcel.Name = "ButtonConnectExcel"
        Me.ButtonConnectExcel.Size = New System.Drawing.Size(171, 60)
        Me.ButtonConnectExcel.TabIndex = 42
        Me.ButtonConnectExcel.Text = "Connect Excel"
        Me.ButtonConnectExcel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.ButtonConnectExcel.UseVisualStyleBackColor = True
        '
        'linkAbout
        '
        Me.linkAbout.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.linkAbout.AutoSize = True
        Me.linkAbout.Location = New System.Drawing.Point(63, 576)
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
        Me.linkHelp.Location = New System.Drawing.Point(18, 576)
        Me.linkHelp.Name = "linkHelp"
        Me.linkHelp.Size = New System.Drawing.Size(29, 13)
        Me.linkHelp.TabIndex = 49
        Me.linkHelp.TabStop = True
        Me.linkHelp.Text = "Help"
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.iniPathLabel})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 600)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(874, 22)
        Me.StatusStrip1.TabIndex = 51
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'iniPathLabel
        '
        Me.iniPathLabel.Name = "iniPathLabel"
        Me.iniPathLabel.Size = New System.Drawing.Size(0, 17)
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.RadioButtonPlaceholder)
        Me.GroupBox3.Controls.Add(Me.RadioButtonViewSheet)
        Me.GroupBox3.Location = New System.Drawing.Point(691, 322)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(171, 75)
        Me.GroupBox3.TabIndex = 52
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Sheet Type"
        '
        'RadioButtonPlaceholder
        '
        Me.RadioButtonPlaceholder.AutoSize = True
        Me.RadioButtonPlaceholder.Location = New System.Drawing.Point(6, 42)
        Me.RadioButtonPlaceholder.Name = "RadioButtonPlaceholder"
        Me.RadioButtonPlaceholder.Size = New System.Drawing.Size(151, 17)
        Me.RadioButtonPlaceholder.TabIndex = 1
        Me.RadioButtonPlaceholder.Text = "Create Placeholder Sheets"
        Me.RadioButtonPlaceholder.UseVisualStyleBackColor = True
        '
        'RadioButtonViewSheet
        '
        Me.RadioButtonViewSheet.AutoSize = True
        Me.RadioButtonViewSheet.Checked = True
        Me.RadioButtonViewSheet.Location = New System.Drawing.Point(6, 19)
        Me.RadioButtonViewSheet.Name = "RadioButtonViewSheet"
        Me.RadioButtonViewSheet.Size = New System.Drawing.Size(118, 17)
        Me.RadioButtonViewSheet.TabIndex = 0
        Me.RadioButtonViewSheet.TabStop = True
        Me.RadioButtonViewSheet.Text = "Create View Sheets"
        Me.RadioButtonViewSheet.UseVisualStyleBackColor = True
        '
        'form_SheetManager
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(874, 622)
        Me.Controls.Add(Me.GroupBox3)
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
        Me.MaximumSize = New System.Drawing.Size(890, 660)
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(890, 660)
        Me.Name = "form_SheetManager"
        Me.Text = "Sheet Management"
        Me.GroupBoxTitleblocks.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBoxSheets.ResumeLayout(False)
        Me.GroupBoxSheets.PerformLayout()
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
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
    Friend WithEvents ButtonUncheck As System.Windows.Forms.Button
    Friend WithEvents ButtonCheckAll As System.Windows.Forms.Button
    Friend WithEvents ComboBoxFilter As System.Windows.Forms.ComboBox
    Friend WithEvents LabelDisplay As System.Windows.Forms.Label
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents RadioButtonPlaceholder As System.Windows.Forms.RadioButton
    Friend WithEvents RadioButtonViewSheet As System.Windows.Forms.RadioButton
End Class
