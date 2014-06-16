<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class form_ElemSelectTitleblock
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(form_ElemSelectTitleblock))
        Me.buttonCancel = New System.Windows.Forms.Button()
        Me.listBoxSelect = New System.Windows.Forms.ListBox()
        Me.SuspendLayout()
        '
        'buttonCancel
        '
        Me.buttonCancel.Location = New System.Drawing.Point(276, 224)
        Me.buttonCancel.Name = "buttonCancel"
        Me.buttonCancel.Size = New System.Drawing.Size(108, 26)
        Me.buttonCancel.TabIndex = 3
        Me.buttonCancel.Text = "Cancel"
        Me.buttonCancel.UseVisualStyleBackColor = True
        '
        'listBoxSelect
        '
        Me.listBoxSelect.FormattingEnabled = True
        Me.listBoxSelect.Location = New System.Drawing.Point(12, 12)
        Me.listBoxSelect.Name = "listBoxSelect"
        Me.listBoxSelect.Size = New System.Drawing.Size(372, 199)
        Me.listBoxSelect.TabIndex = 2
        '
        'form_ElemSelectTitleblock
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(395, 262)
        Me.Controls.Add(Me.buttonCancel)
        Me.Controls.Add(Me.listBoxSelect)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(411, 300)
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(411, 300)
        Me.Name = "form_ElemSelectTitleblock"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Select Titleblock"
        Me.TopMost = True
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents buttonCancel As System.Windows.Forms.Button
    Private WithEvents listBoxSelect As System.Windows.Forms.ListBox
End Class
