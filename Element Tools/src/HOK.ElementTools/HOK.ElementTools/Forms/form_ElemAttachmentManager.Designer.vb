<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class form_ElemAttachmentManager
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(form_ElemAttachmentManager))
        Me.labelMessage = New System.Windows.Forms.Label()
        Me.listBoxDwg = New System.Windows.Forms.ListBox()
        Me.buttonSelect = New System.Windows.Forms.Button()
        Me.buttonClose = New System.Windows.Forms.Button()
        Me.label4 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'labelMessage
        '
        Me.labelMessage.AutoSize = True
        Me.labelMessage.Location = New System.Drawing.Point(14, 416)
        Me.labelMessage.Name = "labelMessage"
        Me.labelMessage.Size = New System.Drawing.Size(72, 13)
        Me.labelMessage.TabIndex = 15
        Me.labelMessage.Text = "labelMessage"
        '
        'listBoxDwg
        '
        Me.listBoxDwg.Font = New System.Drawing.Font("Monospac821 BT", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.listBoxDwg.FormattingEnabled = True
        Me.listBoxDwg.ItemHeight = 14
        Me.listBoxDwg.Location = New System.Drawing.Point(10, 25)
        Me.listBoxDwg.Name = "listBoxDwg"
        Me.listBoxDwg.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.listBoxDwg.Size = New System.Drawing.Size(491, 382)
        Me.listBoxDwg.TabIndex = 13
        '
        'buttonSelect
        '
        Me.buttonSelect.Location = New System.Drawing.Point(10, 449)
        Me.buttonSelect.Name = "buttonSelect"
        Me.buttonSelect.Size = New System.Drawing.Size(178, 29)
        Me.buttonSelect.TabIndex = 12
        Me.buttonSelect.Text = "Select in Revit"
        Me.buttonSelect.UseVisualStyleBackColor = True
        '
        'buttonClose
        '
        Me.buttonClose.Location = New System.Drawing.Point(323, 449)
        Me.buttonClose.Name = "buttonClose"
        Me.buttonClose.Size = New System.Drawing.Size(178, 29)
        Me.buttonClose.TabIndex = 11
        Me.buttonClose.Text = "Close"
        Me.buttonClose.UseVisualStyleBackColor = True
        '
        'label4
        '
        Me.label4.AutoSize = True
        Me.label4.Font = New System.Drawing.Font("Monospac821 BT", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.label4.Location = New System.Drawing.Point(12, 9)
        Me.label4.Name = "label4"
        Me.label4.Size = New System.Drawing.Size(504, 14)
        Me.label4.TabIndex = 14
        Me.label4.Text = "ElementId  Type         Name                                Workset No."
        '
        'form_ElemAttachmentManager
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(511, 489)
        Me.Controls.Add(Me.labelMessage)
        Me.Controls.Add(Me.listBoxDwg)
        Me.Controls.Add(Me.buttonSelect)
        Me.Controls.Add(Me.buttonClose)
        Me.Controls.Add(Me.label4)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(527, 527)
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(527, 527)
        Me.Name = "form_ElemAttachmentManager"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Attachment Link Manager"
        Me.TopMost = True
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents labelMessage As System.Windows.Forms.Label
    Private WithEvents listBoxDwg As System.Windows.Forms.ListBox
    Private WithEvents buttonSelect As System.Windows.Forms.Button
    Private WithEvents buttonClose As System.Windows.Forms.Button
    Private WithEvents label4 As System.Windows.Forms.Label
End Class
