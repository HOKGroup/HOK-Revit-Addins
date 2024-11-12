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
        labelMessage = New System.Windows.Forms.Label()
        listBoxDwg = New System.Windows.Forms.ListBox()
        buttonSelect = New System.Windows.Forms.Button()
        buttonClose = New System.Windows.Forms.Button()
        label4 = New System.Windows.Forms.Label()
        SuspendLayout()
        ' 
        ' labelMessage
        ' 
        labelMessage.AutoSize = True
        labelMessage.Location = New System.Drawing.Point(16, 480)
        labelMessage.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        labelMessage.Name = "labelMessage"
        labelMessage.Size = New System.Drawing.Size(0, 15)
        labelMessage.TabIndex = 15
        ' 
        ' listBoxDwg
        ' 
        listBoxDwg.Font = New System.Drawing.Font("Monospac821 BT", 8.25F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        listBoxDwg.FormattingEnabled = True
        listBoxDwg.ItemHeight = 14
        listBoxDwg.Location = New System.Drawing.Point(12, 29)
        listBoxDwg.Margin = New System.Windows.Forms.Padding(4)
        listBoxDwg.Name = "listBoxDwg"
        listBoxDwg.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        listBoxDwg.Size = New System.Drawing.Size(572, 438)
        listBoxDwg.TabIndex = 13
        ' 
        ' buttonSelect
        ' 
        buttonSelect.Location = New System.Drawing.Point(12, 518)
        buttonSelect.Margin = New System.Windows.Forms.Padding(4)
        buttonSelect.Name = "buttonSelect"
        buttonSelect.Size = New System.Drawing.Size(208, 34)
        buttonSelect.TabIndex = 12
        buttonSelect.Text = "Select in Revit"
        buttonSelect.UseVisualStyleBackColor = True
        ' 
        ' buttonClose
        ' 
        buttonClose.Location = New System.Drawing.Point(377, 518)
        buttonClose.Margin = New System.Windows.Forms.Padding(4)
        buttonClose.Name = "buttonClose"
        buttonClose.Size = New System.Drawing.Size(208, 34)
        buttonClose.TabIndex = 11
        buttonClose.Text = "Close"
        buttonClose.UseVisualStyleBackColor = True
        ' 
        ' label4
        ' 
        label4.AutoSize = True
        label4.Font = New System.Drawing.Font("Monospac821 BT", 8.25F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        label4.Location = New System.Drawing.Point(14, 10)
        label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label4.Name = "label4"
        label4.Size = New System.Drawing.Size(504, 14)
        label4.TabIndex = 14
        label4.Text = "ElementId  Type         Name                                Workset No."
        ' 
        ' form_ElemAttachmentManager
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        ClientSize = New System.Drawing.Size(603, 564)
        Controls.Add(labelMessage)
        Controls.Add(listBoxDwg)
        Controls.Add(buttonSelect)
        Controls.Add(buttonClose)
        Controls.Add(label4)
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        Margin = New System.Windows.Forms.Padding(4)
        MaximizeBox = False
        MaximumSize = New System.Drawing.Size(619, 800)
        MinimizeBox = False
        MinimumSize = New System.Drawing.Size(619, 603)
        Name = "form_ElemAttachmentManager"
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "Attachment Link Manager"
        TopMost = True
        ResumeLayout(False)
        PerformLayout()

    End Sub
    Private WithEvents labelMessage As System.Windows.Forms.Label
    Private WithEvents listBoxDwg As System.Windows.Forms.ListBox
    Private WithEvents buttonSelect As System.Windows.Forms.Button
    Private WithEvents buttonClose As System.Windows.Forms.Button
    Private WithEvents label4 As System.Windows.Forms.Label
End Class
