<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class form_ElemMenu
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(form_ElemMenu))
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.ButtonManageAttachmentLinks = New System.Windows.Forms.Button()
        Me.ButtonCreateSheetsFromViews = New System.Windows.Forms.Button()
        Me.ButtonCreateTaggedViewsFromRooms = New System.Windows.Forms.Button()
        Me.ButtonCreateViewsFromRooms = New System.Windows.Forms.Button()
        Me.ToolTipFormMain = New System.Windows.Forms.ToolTip(Me.components)
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.ButtonCreateRoomsFromAreas = New System.Windows.Forms.Button()
        Me.ButtonPlaceUnplacedRooms = New System.Windows.Forms.Button()
        Me.ButtonPlaceUnplacedAreas = New System.Windows.Forms.Button()
        Me.buttonReloadSettings = New System.Windows.Forms.Button()
        Me.ButtonCancel = New System.Windows.Forms.Button()
        Me.ButtonCreateTaggedViewsFromAreas = New System.Windows.Forms.Button()
        Me.ButtonCreateViewsFromAreas = New System.Windows.Forms.Button()
        Me.GroupBox3.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.ButtonManageAttachmentLinks)
        Me.GroupBox3.Location = New System.Drawing.Point(12, 307)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(310, 59)
        Me.GroupBox3.TabIndex = 27
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Linked Elements"
        '
        'ButtonManageAttachmentLinks
        '
        Me.ButtonManageAttachmentLinks.Location = New System.Drawing.Point(6, 19)
        Me.ButtonManageAttachmentLinks.Name = "ButtonManageAttachmentLinks"
        Me.ButtonManageAttachmentLinks.Size = New System.Drawing.Size(298, 23)
        Me.ButtonManageAttachmentLinks.TabIndex = 28
        Me.ButtonManageAttachmentLinks.Text = "Manage Attachment Links"
        Me.ButtonManageAttachmentLinks.UseVisualStyleBackColor = True
        '
        'ButtonCreateSheetsFromViews
        '
        Me.ButtonCreateSheetsFromViews.Location = New System.Drawing.Point(6, 135)
        Me.ButtonCreateSheetsFromViews.Name = "ButtonCreateSheetsFromViews"
        Me.ButtonCreateSheetsFromViews.Size = New System.Drawing.Size(298, 23)
        Me.ButtonCreateSheetsFromViews.TabIndex = 27
        Me.ButtonCreateSheetsFromViews.Text = "Create Sheets from Views"
        Me.ButtonCreateSheetsFromViews.UseVisualStyleBackColor = True
        '
        'ButtonCreateTaggedViewsFromRooms
        '
        Me.ButtonCreateTaggedViewsFromRooms.Location = New System.Drawing.Point(6, 48)
        Me.ButtonCreateTaggedViewsFromRooms.Name = "ButtonCreateTaggedViewsFromRooms"
        Me.ButtonCreateTaggedViewsFromRooms.Size = New System.Drawing.Size(298, 23)
        Me.ButtonCreateTaggedViewsFromRooms.TabIndex = 26
        Me.ButtonCreateTaggedViewsFromRooms.Text = "Create Tagged Views from Rooms"
        Me.ButtonCreateTaggedViewsFromRooms.UseVisualStyleBackColor = True
        '
        'ButtonCreateViewsFromRooms
        '
        Me.ButtonCreateViewsFromRooms.Location = New System.Drawing.Point(6, 19)
        Me.ButtonCreateViewsFromRooms.Name = "ButtonCreateViewsFromRooms"
        Me.ButtonCreateViewsFromRooms.Size = New System.Drawing.Size(298, 23)
        Me.ButtonCreateViewsFromRooms.TabIndex = 25
        Me.ButtonCreateViewsFromRooms.Text = "Create Views from Rooms"
        Me.ButtonCreateViewsFromRooms.UseVisualStyleBackColor = True
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.ButtonCreateTaggedViewsFromAreas)
        Me.GroupBox2.Controls.Add(Me.ButtonCreateViewsFromAreas)
        Me.GroupBox2.Controls.Add(Me.ButtonCreateSheetsFromViews)
        Me.GroupBox2.Controls.Add(Me.ButtonCreateTaggedViewsFromRooms)
        Me.GroupBox2.Controls.Add(Me.ButtonCreateViewsFromRooms)
        Me.GroupBox2.Location = New System.Drawing.Point(12, 129)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(310, 172)
        Me.GroupBox2.TabIndex = 26
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "View and Sheet Elements"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.ButtonCreateRoomsFromAreas)
        Me.GroupBox1.Controls.Add(Me.ButtonPlaceUnplacedRooms)
        Me.GroupBox1.Controls.Add(Me.ButtonPlaceUnplacedAreas)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 12)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(310, 111)
        Me.GroupBox1.TabIndex = 25
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Room and Area Elements"
        '
        'ButtonCreateRoomsFromAreas
        '
        Me.ButtonCreateRoomsFromAreas.Location = New System.Drawing.Point(6, 77)
        Me.ButtonCreateRoomsFromAreas.Name = "ButtonCreateRoomsFromAreas"
        Me.ButtonCreateRoomsFromAreas.Size = New System.Drawing.Size(298, 23)
        Me.ButtonCreateRoomsFromAreas.TabIndex = 24
        Me.ButtonCreateRoomsFromAreas.Text = "Create Rooms from Areas"
        Me.ButtonCreateRoomsFromAreas.UseVisualStyleBackColor = True
        '
        'ButtonPlaceUnplacedRooms
        '
        Me.ButtonPlaceUnplacedRooms.Location = New System.Drawing.Point(6, 48)
        Me.ButtonPlaceUnplacedRooms.Name = "ButtonPlaceUnplacedRooms"
        Me.ButtonPlaceUnplacedRooms.Size = New System.Drawing.Size(298, 23)
        Me.ButtonPlaceUnplacedRooms.TabIndex = 23
        Me.ButtonPlaceUnplacedRooms.Text = "Place Unplaced Rooms"
        Me.ButtonPlaceUnplacedRooms.UseVisualStyleBackColor = True
        '
        'ButtonPlaceUnplacedAreas
        '
        Me.ButtonPlaceUnplacedAreas.Location = New System.Drawing.Point(6, 19)
        Me.ButtonPlaceUnplacedAreas.Name = "ButtonPlaceUnplacedAreas"
        Me.ButtonPlaceUnplacedAreas.Size = New System.Drawing.Size(298, 23)
        Me.ButtonPlaceUnplacedAreas.TabIndex = 22
        Me.ButtonPlaceUnplacedAreas.Text = "Place Unplaced Areas"
        Me.ButtonPlaceUnplacedAreas.UseVisualStyleBackColor = True
        '
        'buttonReloadSettings
        '
        Me.buttonReloadSettings.Location = New System.Drawing.Point(18, 372)
        Me.buttonReloadSettings.Name = "buttonReloadSettings"
        Me.buttonReloadSettings.Size = New System.Drawing.Size(170, 23)
        Me.buttonReloadSettings.TabIndex = 24
        Me.buttonReloadSettings.Text = "Reload Default Settings"
        Me.buttonReloadSettings.UseVisualStyleBackColor = True
        '
        'ButtonCancel
        '
        Me.ButtonCancel.Location = New System.Drawing.Point(241, 372)
        Me.ButtonCancel.Name = "ButtonCancel"
        Me.ButtonCancel.Size = New System.Drawing.Size(75, 23)
        Me.ButtonCancel.TabIndex = 23
        Me.ButtonCancel.Text = "Cancel"
        Me.ButtonCancel.UseVisualStyleBackColor = True
        '
        'ButtonCreateTaggedViewsFromAreas
        '
        Me.ButtonCreateTaggedViewsFromAreas.Location = New System.Drawing.Point(6, 106)
        Me.ButtonCreateTaggedViewsFromAreas.Name = "ButtonCreateTaggedViewsFromAreas"
        Me.ButtonCreateTaggedViewsFromAreas.Size = New System.Drawing.Size(298, 23)
        Me.ButtonCreateTaggedViewsFromAreas.TabIndex = 29
        Me.ButtonCreateTaggedViewsFromAreas.Text = "Create Tagged Views from Areas"
        Me.ButtonCreateTaggedViewsFromAreas.UseVisualStyleBackColor = True
        '
        'ButtonCreateViewsFromAreas
        '
        Me.ButtonCreateViewsFromAreas.Location = New System.Drawing.Point(6, 77)
        Me.ButtonCreateViewsFromAreas.Name = "ButtonCreateViewsFromAreas"
        Me.ButtonCreateViewsFromAreas.Size = New System.Drawing.Size(298, 23)
        Me.ButtonCreateViewsFromAreas.TabIndex = 28
        Me.ButtonCreateViewsFromAreas.Text = "Create Views from Areas"
        Me.ButtonCreateViewsFromAreas.UseVisualStyleBackColor = True
        '
        'form_ElemMenu
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(335, 407)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.buttonReloadSettings)
        Me.Controls.Add(Me.ButtonCancel)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(351, 445)
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(351, 445)
        Me.Name = "form_ElemMenu"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Element Tools"
        Me.TopMost = True
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Private WithEvents ButtonManageAttachmentLinks As System.Windows.Forms.Button
    Private WithEvents ButtonCreateSheetsFromViews As System.Windows.Forms.Button
    Private WithEvents ButtonCreateTaggedViewsFromRooms As System.Windows.Forms.Button
    Private WithEvents ButtonCreateViewsFromRooms As System.Windows.Forms.Button
    Friend WithEvents ToolTipFormMain As System.Windows.Forms.ToolTip
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Private WithEvents ButtonCreateRoomsFromAreas As System.Windows.Forms.Button
    Private WithEvents ButtonPlaceUnplacedRooms As System.Windows.Forms.Button
    Private WithEvents ButtonPlaceUnplacedAreas As System.Windows.Forms.Button
    Private WithEvents buttonReloadSettings As System.Windows.Forms.Button
    Friend WithEvents ButtonCancel As System.Windows.Forms.Button
    Private WithEvents ButtonCreateTaggedViewsFromAreas As System.Windows.Forms.Button
    Private WithEvents ButtonCreateViewsFromAreas As System.Windows.Forms.Button
End Class
