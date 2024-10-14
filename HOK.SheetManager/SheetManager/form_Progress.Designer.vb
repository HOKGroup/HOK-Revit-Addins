<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class form_Progress
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(form_Progress))
        Me.progressBar1 = New System.Windows.Forms.ProgressBar()
        Me.SuspendLayout()
        '
        'progressBar1
        '
        Me.progressBar1.Location = New System.Drawing.Point(1, 1)
        Me.progressBar1.Name = "progressBar1"
        Me.progressBar1.Size = New System.Drawing.Size(539, 31)
        Me.progressBar1.TabIndex = 2
        '
        'form_Progress
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(541, 33)
        Me.Controls.Add(Me.progressBar1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(557, 71)
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(557, 71)
        Me.Name = "form_Progress"
        Me.Text = "Working..."
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents progressBar1 As System.Windows.Forms.ProgressBar
End Class
