<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class form_Main
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(form_Main))
        Me.ListBoxTableSet = New System.Windows.Forms.ListBox()
        Me.buttonRenumber = New System.Windows.Forms.Button()
        Me.buttonAddViews = New System.Windows.Forms.Button()
        Me.buttonCreate = New System.Windows.Forms.Button()
        Me.TreeViewTitleBlocks = New System.Windows.Forms.TreeView()
        Me.buttonClose = New System.Windows.Forms.Button()
        Me.openFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.checkBoxUpdateExisting = New System.Windows.Forms.CheckBox()
        Me.listBoxTitleblocks = New System.Windows.Forms.ListBox()
        Me.LabelSheetGroupSelection = New System.Windows.Forms.Label()
        Me.GroupBoxTitleblocks = New System.Windows.Forms.GroupBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.LabelFilePath = New System.Windows.Forms.Label()
        Me.DataGridViewSheets = New System.Windows.Forms.DataGridView()
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.ButtonUseAccess = New System.Windows.Forms.Button()
        Me.buttonWriteData = New System.Windows.Forms.Button()
        Me.ButtonUseExcel = New System.Windows.Forms.Button()
        Me.GroupBoxTitleblocks.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        CType(Me.DataGridViewSheets, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'ListBoxTableSet
        '
        Me.ListBoxTableSet.FormattingEnabled = True
        Me.ListBoxTableSet.Location = New System.Drawing.Point(12, 164)
        Me.ListBoxTableSet.Name = "ListBoxTableSet"
        Me.ListBoxTableSet.Size = New System.Drawing.Size(160, 264)
        Me.ListBoxTableSet.TabIndex = 13
        '
        'buttonRenumber
        '
        Me.buttonRenumber.Location = New System.Drawing.Point(510, 434)
        Me.buttonRenumber.Name = "buttonRenumber"
        Me.buttonRenumber.Size = New System.Drawing.Size(160, 60)
        Me.buttonRenumber.TabIndex = 34
        Me.buttonRenumber.Text = "Renumber Sheets"
        Me.buttonRenumber.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.buttonRenumber.UseVisualStyleBackColor = True
        '
        'buttonAddViews
        '
        Me.buttonAddViews.Image = Global.HOK.SheetManager.My.Resources.Resources.ExportOrOpenAs
        Me.buttonAddViews.ImageAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.buttonAddViews.Location = New System.Drawing.Point(676, 434)
        Me.buttonAddViews.Name = "buttonAddViews"
        Me.buttonAddViews.Size = New System.Drawing.Size(160, 60)
        Me.buttonAddViews.TabIndex = 36
        Me.buttonAddViews.Text = "Add Views to Sheets"
        Me.buttonAddViews.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.buttonAddViews.UseVisualStyleBackColor = True
        '
        'buttonCreate
        '
        Me.buttonCreate.Image = Global.HOK.SheetManager.My.Resources.Resources.Copy_of_AdmTmpl_dll_I0004_04095
        Me.buttonCreate.ImageAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.buttonCreate.Location = New System.Drawing.Point(178, 434)
        Me.buttonCreate.Name = "buttonCreate"
        Me.buttonCreate.Size = New System.Drawing.Size(160, 60)
        Me.buttonCreate.TabIndex = 31
        Me.buttonCreate.Text = "Create/Update Sheets"
        Me.buttonCreate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.buttonCreate.UseVisualStyleBackColor = True
        '
        'TreeViewTitleBlocks
        '
        Me.TreeViewTitleBlocks.Location = New System.Drawing.Point(6, 68)
        Me.TreeViewTitleBlocks.Name = "TreeViewTitleBlocks"
        Me.TreeViewTitleBlocks.Size = New System.Drawing.Size(289, 52)
        Me.TreeViewTitleBlocks.TabIndex = 27
        '
        'buttonClose
        '
        Me.buttonClose.Location = New System.Drawing.Point(842, 434)
        Me.buttonClose.Name = "buttonClose"
        Me.buttonClose.Size = New System.Drawing.Size(160, 60)
        Me.buttonClose.TabIndex = 30
        Me.buttonClose.Text = "Close"
        Me.buttonClose.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.buttonClose.UseVisualStyleBackColor = True
        '
        'openFileDialog1
        '
        Me.openFileDialog1.FileName = "openFileDialog1"
        '
        'checkBoxUpdateExisting
        '
        Me.checkBoxUpdateExisting.Location = New System.Drawing.Point(12, 434)
        Me.checkBoxUpdateExisting.Name = "checkBoxUpdateExisting"
        Me.checkBoxUpdateExisting.Size = New System.Drawing.Size(142, 43)
        Me.checkBoxUpdateExisting.TabIndex = 32
        Me.checkBoxUpdateExisting.Text = "Update Existing Values"
        Me.checkBoxUpdateExisting.UseVisualStyleBackColor = True
        '
        'listBoxTitleblocks
        '
        Me.listBoxTitleblocks.FormattingEnabled = True
        Me.listBoxTitleblocks.Location = New System.Drawing.Point(6, 19)
        Me.listBoxTitleblocks.Name = "listBoxTitleblocks"
        Me.listBoxTitleblocks.Size = New System.Drawing.Size(289, 43)
        Me.listBoxTitleblocks.TabIndex = 29
        '
        'LabelSheetGroupSelection
        '
        Me.LabelSheetGroupSelection.AutoSize = True
        Me.LabelSheetGroupSelection.Location = New System.Drawing.Point(12, 148)
        Me.LabelSheetGroupSelection.Name = "LabelSheetGroupSelection"
        Me.LabelSheetGroupSelection.Size = New System.Drawing.Size(105, 13)
        Me.LabelSheetGroupSelection.TabIndex = 14
        Me.LabelSheetGroupSelection.Text = "Select Sheet Source"
        '
        'GroupBoxTitleblocks
        '
        Me.GroupBoxTitleblocks.Controls.Add(Me.listBoxTitleblocks)
        Me.GroupBoxTitleblocks.Controls.Add(Me.TreeViewTitleBlocks)
        Me.GroupBoxTitleblocks.Location = New System.Drawing.Point(701, 12)
        Me.GroupBoxTitleblocks.Name = "GroupBoxTitleblocks"
        Me.GroupBoxTitleblocks.Size = New System.Drawing.Size(301, 126)
        Me.GroupBoxTitleblocks.TabIndex = 37
        Me.GroupBoxTitleblocks.TabStop = False
        Me.GroupBoxTitleblocks.Text = "Select a Titleblock for New Sheets"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.LabelFilePath)
        Me.GroupBox2.Location = New System.Drawing.Point(178, 12)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(517, 126)
        Me.GroupBox2.TabIndex = 38
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Sheet Manager Input/Output File Path"
        '
        'LabelFilePath
        '
        Me.LabelFilePath.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LabelFilePath.Location = New System.Drawing.Point(6, 15)
        Me.LabelFilePath.Name = "LabelFilePath"
        Me.LabelFilePath.Size = New System.Drawing.Size(505, 108)
        Me.LabelFilePath.TabIndex = 45
        Me.LabelFilePath.Text = "Connect to either an Access or Excel File to Continue"
        Me.LabelFilePath.TextAlign = System.Drawing.ContentAlignment.BottomRight
        '
        'DataGridViewSheets
        '
        Me.DataGridViewSheets.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridViewSheets.Location = New System.Drawing.Point(178, 163)
        Me.DataGridViewSheets.Name = "DataGridViewSheets"
        Me.DataGridViewSheets.Size = New System.Drawing.Size(824, 265)
        Me.DataGridViewSheets.TabIndex = 44
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(178, 434)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(824, 60)
        Me.ProgressBar1.TabIndex = 46
        '
        'ButtonUseAccess
        '
        Me.ButtonUseAccess.Image = Global.HOK.SheetManager.My.Resources.Resources.MsAccess_Med
        Me.ButtonUseAccess.ImageAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.ButtonUseAccess.Location = New System.Drawing.Point(12, 78)
        Me.ButtonUseAccess.Name = "ButtonUseAccess"
        Me.ButtonUseAccess.Size = New System.Drawing.Size(160, 60)
        Me.ButtonUseAccess.TabIndex = 43
        Me.ButtonUseAccess.Text = "Connect Access"
        Me.ButtonUseAccess.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.ButtonUseAccess.UseVisualStyleBackColor = True
        '
        'buttonWriteData
        '
        Me.buttonWriteData.Image = Global.HOK.SheetManager.My.Resources.Resources._002
        Me.buttonWriteData.ImageAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.buttonWriteData.Location = New System.Drawing.Point(344, 434)
        Me.buttonWriteData.Name = "buttonWriteData"
        Me.buttonWriteData.Size = New System.Drawing.Size(160, 60)
        Me.buttonWriteData.TabIndex = 35
        Me.buttonWriteData.Text = "Export Sheet  Data"
        Me.buttonWriteData.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.buttonWriteData.UseVisualStyleBackColor = True
        '
        'ButtonUseExcel
        '
        Me.ButtonUseExcel.Image = Global.HOK.SheetManager.My.Resources.Resources.Excel2007Logo_Med
        Me.ButtonUseExcel.ImageAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.ButtonUseExcel.Location = New System.Drawing.Point(12, 12)
        Me.ButtonUseExcel.Name = "ButtonUseExcel"
        Me.ButtonUseExcel.Size = New System.Drawing.Size(160, 60)
        Me.ButtonUseExcel.TabIndex = 42
        Me.ButtonUseExcel.Text = "Connect Excel"
        Me.ButtonUseExcel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.ButtonUseExcel.UseVisualStyleBackColor = True
        '
        'form_Main
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1014, 505)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.DataGridViewSheets)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.ListBoxTableSet)
        Me.Controls.Add(Me.LabelSheetGroupSelection)
        Me.Controls.Add(Me.ButtonUseAccess)
        Me.Controls.Add(Me.buttonWriteData)
        Me.Controls.Add(Me.buttonRenumber)
        Me.Controls.Add(Me.buttonAddViews)
        Me.Controls.Add(Me.buttonCreate)
        Me.Controls.Add(Me.buttonClose)
        Me.Controls.Add(Me.checkBoxUpdateExisting)
        Me.Controls.Add(Me.GroupBoxTitleblocks)
        Me.Controls.Add(Me.ButtonUseExcel)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(1030, 543)
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(1030, 543)
        Me.Name = "form_Main"
        Me.Text = "Sheet Management"
        Me.GroupBoxTitleblocks.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        CType(Me.DataGridViewSheets, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents ListBoxTableSet As System.Windows.Forms.ListBox
    Private WithEvents buttonWriteData As System.Windows.Forms.Button
    Private WithEvents buttonRenumber As System.Windows.Forms.Button
    Private WithEvents buttonAddViews As System.Windows.Forms.Button
    Private WithEvents buttonCreate As System.Windows.Forms.Button
    Friend WithEvents TreeViewTitleBlocks As System.Windows.Forms.TreeView
    Private WithEvents buttonClose As System.Windows.Forms.Button
    Private WithEvents openFileDialog1 As System.Windows.Forms.OpenFileDialog
    Private WithEvents checkBoxUpdateExisting As System.Windows.Forms.CheckBox
    Private WithEvents listBoxTitleblocks As System.Windows.Forms.ListBox
    Private WithEvents LabelSheetGroupSelection As System.Windows.Forms.Label
    Friend WithEvents GroupBoxTitleblocks As System.Windows.Forms.GroupBox
    Private WithEvents ButtonUseExcel As System.Windows.Forms.Button
    Private WithEvents ButtonUseAccess As System.Windows.Forms.Button
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents LabelFilePath As System.Windows.Forms.Label
    Friend WithEvents DataGridViewSheets As System.Windows.Forms.DataGridView
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar
End Class
