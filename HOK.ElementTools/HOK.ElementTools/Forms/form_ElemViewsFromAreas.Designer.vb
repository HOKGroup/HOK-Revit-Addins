<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class form_ElemViewsFromAreas
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(form_ElemViewsFromAreas))
        buttonClose = New System.Windows.Forms.Button()
        groupBoxSizeCrop = New System.Windows.Forms.GroupBox()
        checkBoxCropShow = New System.Windows.Forms.CheckBox()
        textBoxCropFixedY = New System.Windows.Forms.TextBox()
        textBoxCropFixedX = New System.Windows.Forms.TextBox()
        radioButtonSizeCropFixed = New System.Windows.Forms.RadioButton()
        radioButtonSizeCropDynamic = New System.Windows.Forms.RadioButton()
        textBoxCropSpace = New System.Windows.Forms.TextBox()
        label10 = New System.Windows.Forms.Label()
        label11 = New System.Windows.Forms.Label()
        groupBoxViewType = New System.Windows.Forms.GroupBox()
        checkBoxReplaceExisting = New System.Windows.Forms.CheckBox()
        radioButtonType3dBoxCrop = New System.Windows.Forms.RadioButton()
        textBoxVectorZ = New System.Windows.Forms.TextBox()
        textBoxVectorY = New System.Windows.Forms.TextBox()
        textBoxVectorX = New System.Windows.Forms.TextBox()
        label4 = New System.Windows.Forms.Label()
        radioButtonType3dCrop = New System.Windows.Forms.RadioButton()
        radioButtonType2d = New System.Windows.Forms.RadioButton()
        radioButtonType3dBox = New System.Windows.Forms.RadioButton()
        label14 = New System.Windows.Forms.Label()
        label15 = New System.Windows.Forms.Label()
        label16 = New System.Windows.Forms.Label()
        groupBoxSizeBox = New System.Windows.Forms.GroupBox()
        label20 = New System.Windows.Forms.Label()
        checkBoxBoxShow = New System.Windows.Forms.CheckBox()
        textBoxBoxFixedZ = New System.Windows.Forms.TextBox()
        textBoxBoxFixedY = New System.Windows.Forms.TextBox()
        textBoxBoxFixedX = New System.Windows.Forms.TextBox()
        radioButtonSizeBoxFixed = New System.Windows.Forms.RadioButton()
        radioButtonSizeBoxDynamic = New System.Windows.Forms.RadioButton()
        textBoxBoxSpace = New System.Windows.Forms.TextBox()
        label17 = New System.Windows.Forms.Label()
        label18 = New System.Windows.Forms.Label()
        label19 = New System.Windows.Forms.Label()
        groupBoxSelection = New System.Windows.Forms.GroupBox()
        textBoxParameterGroupBy = New System.Windows.Forms.TextBox()
        radioButtonGroupMultiple = New System.Windows.Forms.RadioButton()
        radioButtonGroupSingle = New System.Windows.Forms.RadioButton()
        checkBoxListExisting = New System.Windows.Forms.CheckBox()
        checkBoxListReverse = New System.Windows.Forms.CheckBox()
        textBoxPad2 = New System.Windows.Forms.TextBox()
        textBoxPad1 = New System.Windows.Forms.TextBox()
        checkBoxPad2 = New System.Windows.Forms.CheckBox()
        checkBoxPad1 = New System.Windows.Forms.CheckBox()
        textBoxParameterList2 = New System.Windows.Forms.TextBox()
        label7 = New System.Windows.Forms.Label()
        textBoxParameterList1 = New System.Windows.Forms.TextBox()
        label8 = New System.Windows.Forms.Label()
        groupBox2 = New System.Windows.Forms.GroupBox()
        textBoxScale = New System.Windows.Forms.TextBox()
        label5 = New System.Windows.Forms.Label()
        textBoxPrefixViewTarget = New System.Windows.Forms.TextBox()
        label6 = New System.Windows.Forms.Label()
        textBoxParameterAreaName = New System.Windows.Forms.TextBox()
        textBoxParameterViewName = New System.Windows.Forms.TextBox()
        label1 = New System.Windows.Forms.Label()
        label3 = New System.Windows.Forms.Label()
        label9 = New System.Windows.Forms.Label()
        buttonCreate = New System.Windows.Forms.Button()
        listBoxAreas = New System.Windows.Forms.ListBox()
        ProgressBar1 = New System.Windows.Forms.ProgressBar()
        labelListTitle = New System.Windows.Forms.Label()
        groupBoxSizeCrop.SuspendLayout()
        groupBoxViewType.SuspendLayout()
        groupBoxSizeBox.SuspendLayout()
        groupBoxSelection.SuspendLayout()
        groupBox2.SuspendLayout()
        SuspendLayout()
        ' 
        ' buttonClose
        ' 
        buttonClose.Location = New System.Drawing.Point(565, 708)
        buttonClose.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        buttonClose.Name = "buttonClose"
        buttonClose.Size = New System.Drawing.Size(208, 33)
        buttonClose.TabIndex = 47
        buttonClose.Text = "Close"
        buttonClose.UseVisualStyleBackColor = True
        ' 
        ' groupBoxSizeCrop
        ' 
        groupBoxSizeCrop.Controls.Add(checkBoxCropShow)
        groupBoxSizeCrop.Controls.Add(textBoxCropFixedY)
        groupBoxSizeCrop.Controls.Add(textBoxCropFixedX)
        groupBoxSizeCrop.Controls.Add(radioButtonSizeCropFixed)
        groupBoxSizeCrop.Controls.Add(radioButtonSizeCropDynamic)
        groupBoxSizeCrop.Controls.Add(textBoxCropSpace)
        groupBoxSizeCrop.Controls.Add(label10)
        groupBoxSizeCrop.Controls.Add(label11)
        groupBoxSizeCrop.ForeColor = Drawing.SystemColors.ControlText
        groupBoxSizeCrop.Location = New System.Drawing.Point(383, 570)
        groupBoxSizeCrop.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        groupBoxSizeCrop.Name = "groupBoxSizeCrop"
        groupBoxSizeCrop.Padding = New System.Windows.Forms.Padding(4, 3, 4, 3)
        groupBoxSizeCrop.Size = New System.Drawing.Size(390, 123)
        groupBoxSizeCrop.TabIndex = 53
        groupBoxSizeCrop.TabStop = False
        groupBoxSizeCrop.Text = "View Size"
        ' 
        ' checkBoxCropShow
        ' 
        checkBoxCropShow.AutoSize = True
        checkBoxCropShow.Location = New System.Drawing.Point(12, 91)
        checkBoxCropShow.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        checkBoxCropShow.Name = "checkBoxCropShow"
        checkBoxCropShow.Size = New System.Drawing.Size(107, 19)
        checkBoxCropShow.TabIndex = 52
        checkBoxCropShow.Text = "Show Crop Box"
        checkBoxCropShow.UseVisualStyleBackColor = True
        ' 
        ' textBoxCropFixedY
        ' 
        textBoxCropFixedY.Location = New System.Drawing.Point(252, 53)
        textBoxCropFixedY.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxCropFixedY.Name = "textBoxCropFixedY"
        textBoxCropFixedY.Size = New System.Drawing.Size(47, 23)
        textBoxCropFixedY.TabIndex = 49
        ' 
        ' textBoxCropFixedX
        ' 
        textBoxCropFixedX.Location = New System.Drawing.Point(172, 53)
        textBoxCropFixedX.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxCropFixedX.Name = "textBoxCropFixedX"
        textBoxCropFixedX.Size = New System.Drawing.Size(46, 23)
        textBoxCropFixedX.TabIndex = 47
        ' 
        ' radioButtonSizeCropFixed
        ' 
        radioButtonSizeCropFixed.AutoSize = True
        radioButtonSizeCropFixed.Location = New System.Drawing.Point(12, 55)
        radioButtonSizeCropFixed.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        radioButtonSizeCropFixed.Name = "radioButtonSizeCropFixed"
        radioButtonSizeCropFixed.Size = New System.Drawing.Size(118, 19)
        radioButtonSizeCropFixed.TabIndex = 46
        radioButtonSizeCropFixed.TabStop = True
        radioButtonSizeCropFixed.Text = "Fixed Dimensions"
        radioButtonSizeCropFixed.UseVisualStyleBackColor = True
        ' 
        ' radioButtonSizeCropDynamic
        ' 
        radioButtonSizeCropDynamic.AutoSize = True
        radioButtonSizeCropDynamic.Location = New System.Drawing.Point(12, 22)
        radioButtonSizeCropDynamic.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        radioButtonSizeCropDynamic.Name = "radioButtonSizeCropDynamic"
        radioButtonSizeCropDynamic.Size = New System.Drawing.Size(129, 19)
        radioButtonSizeCropDynamic.TabIndex = 45
        radioButtonSizeCropDynamic.TabStop = True
        radioButtonSizeCropDynamic.Text = "Space Around Area:"
        radioButtonSizeCropDynamic.UseVisualStyleBackColor = True
        ' 
        ' textBoxCropSpace
        ' 
        textBoxCropSpace.Location = New System.Drawing.Point(172, 21)
        textBoxCropSpace.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxCropSpace.Name = "textBoxCropSpace"
        textBoxCropSpace.Size = New System.Drawing.Size(47, 23)
        textBoxCropSpace.TabIndex = 28
        ' 
        ' label10
        ' 
        label10.AutoSize = True
        label10.Location = New System.Drawing.Point(149, 59)
        label10.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label10.Name = "label10"
        label10.Size = New System.Drawing.Size(17, 15)
        label10.TabIndex = 48
        label10.Text = "X:"
        ' 
        ' label11
        ' 
        label11.AutoSize = True
        label11.Location = New System.Drawing.Point(230, 59)
        label11.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label11.Name = "label11"
        label11.Size = New System.Drawing.Size(17, 15)
        label11.TabIndex = 50
        label11.Text = "Y:"
        ' 
        ' groupBoxViewType
        ' 
        groupBoxViewType.Controls.Add(checkBoxReplaceExisting)
        groupBoxViewType.Controls.Add(radioButtonType3dBoxCrop)
        groupBoxViewType.Controls.Add(textBoxVectorZ)
        groupBoxViewType.Controls.Add(textBoxVectorY)
        groupBoxViewType.Controls.Add(textBoxVectorX)
        groupBoxViewType.Controls.Add(label4)
        groupBoxViewType.Controls.Add(radioButtonType3dCrop)
        groupBoxViewType.Controls.Add(radioButtonType2d)
        groupBoxViewType.Controls.Add(radioButtonType3dBox)
        groupBoxViewType.Controls.Add(label14)
        groupBoxViewType.Controls.Add(label15)
        groupBoxViewType.Controls.Add(label16)
        groupBoxViewType.ForeColor = Drawing.SystemColors.ControlText
        groupBoxViewType.Location = New System.Drawing.Point(383, 324)
        groupBoxViewType.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        groupBoxViewType.Name = "groupBoxViewType"
        groupBoxViewType.Padding = New System.Windows.Forms.Padding(4, 3, 4, 3)
        groupBoxViewType.Size = New System.Drawing.Size(390, 114)
        groupBoxViewType.TabIndex = 52
        groupBoxViewType.TabStop = False
        groupBoxViewType.Text = "View Type"
        ' 
        ' checkBoxReplaceExisting
        ' 
        checkBoxReplaceExisting.AutoSize = True
        checkBoxReplaceExisting.Location = New System.Drawing.Point(13, 82)
        checkBoxReplaceExisting.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        checkBoxReplaceExisting.Name = "checkBoxReplaceExisting"
        checkBoxReplaceExisting.Size = New System.Drawing.Size(111, 19)
        checkBoxReplaceExisting.TabIndex = 38
        checkBoxReplaceExisting.Text = "Replace Existing"
        checkBoxReplaceExisting.UseVisualStyleBackColor = True
        ' 
        ' radioButtonType3dBoxCrop
        ' 
        radioButtonType3dBoxCrop.AutoSize = True
        radioButtonType3dBoxCrop.Location = New System.Drawing.Point(284, 18)
        radioButtonType3dBoxCrop.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        radioButtonType3dBoxCrop.Name = "radioButtonType3dBoxCrop"
        radioButtonType3dBoxCrop.Size = New System.Drawing.Size(93, 19)
        radioButtonType3dBoxCrop.TabIndex = 58
        radioButtonType3dBoxCrop.TabStop = True
        radioButtonType3dBoxCrop.Text = "3D Box-Crop"
        radioButtonType3dBoxCrop.UseVisualStyleBackColor = True
        ' 
        ' textBoxVectorZ
        ' 
        textBoxVectorZ.Location = New System.Drawing.Point(324, 47)
        textBoxVectorZ.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxVectorZ.Name = "textBoxVectorZ"
        textBoxVectorZ.Size = New System.Drawing.Size(46, 23)
        textBoxVectorZ.TabIndex = 56
        ' 
        ' textBoxVectorY
        ' 
        textBoxVectorY.Location = New System.Drawing.Point(251, 48)
        textBoxVectorY.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxVectorY.Name = "textBoxVectorY"
        textBoxVectorY.Size = New System.Drawing.Size(46, 23)
        textBoxVectorY.TabIndex = 54
        ' 
        ' textBoxVectorX
        ' 
        textBoxVectorX.Location = New System.Drawing.Point(168, 48)
        textBoxVectorX.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxVectorX.Name = "textBoxVectorX"
        textBoxVectorX.Size = New System.Drawing.Size(46, 23)
        textBoxVectorX.TabIndex = 52
        ' 
        ' label4
        ' 
        label4.AutoSize = True
        label4.Location = New System.Drawing.Point(9, 52)
        label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label4.Name = "label4"
        label4.Size = New System.Drawing.Size(108, 15)
        label4.TabIndex = 47
        label4.Text = "3D Direction Vector"
        ' 
        ' radioButtonType3dCrop
        ' 
        radioButtonType3dCrop.AutoSize = True
        radioButtonType3dCrop.Location = New System.Drawing.Point(191, 18)
        radioButtonType3dCrop.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        radioButtonType3dCrop.Name = "radioButtonType3dCrop"
        radioButtonType3dCrop.Size = New System.Drawing.Size(68, 19)
        radioButtonType3dCrop.TabIndex = 46
        radioButtonType3dCrop.TabStop = True
        radioButtonType3dCrop.Text = "3D Crop"
        radioButtonType3dCrop.UseVisualStyleBackColor = True
        ' 
        ' radioButtonType2d
        ' 
        radioButtonType2d.AutoSize = True
        radioButtonType2d.Location = New System.Drawing.Point(12, 18)
        radioButtonType2d.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        radioButtonType2d.Name = "radioButtonType2d"
        radioButtonType2d.Size = New System.Drawing.Size(39, 19)
        radioButtonType2d.TabIndex = 44
        radioButtonType2d.TabStop = True
        radioButtonType2d.Text = "2D"
        radioButtonType2d.UseVisualStyleBackColor = True
        ' 
        ' radioButtonType3dBox
        ' 
        radioButtonType3dBox.AutoSize = True
        radioButtonType3dBox.Location = New System.Drawing.Point(100, 18)
        radioButtonType3dBox.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        radioButtonType3dBox.Name = "radioButtonType3dBox"
        radioButtonType3dBox.Size = New System.Drawing.Size(62, 19)
        radioButtonType3dBox.TabIndex = 45
        radioButtonType3dBox.TabStop = True
        radioButtonType3dBox.Text = "3D Box"
        radioButtonType3dBox.UseVisualStyleBackColor = True
        ' 
        ' label14
        ' 
        label14.AutoSize = True
        label14.Location = New System.Drawing.Point(303, 52)
        label14.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label14.Name = "label14"
        label14.Size = New System.Drawing.Size(17, 15)
        label14.TabIndex = 57
        label14.Text = "Z:"
        ' 
        ' label15
        ' 
        label15.AutoSize = True
        label15.Location = New System.Drawing.Point(229, 52)
        label15.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label15.Name = "label15"
        label15.Size = New System.Drawing.Size(17, 15)
        label15.TabIndex = 55
        label15.Text = "Y:"
        ' 
        ' label16
        ' 
        label16.AutoSize = True
        label16.Location = New System.Drawing.Point(147, 52)
        label16.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label16.Name = "label16"
        label16.Size = New System.Drawing.Size(17, 15)
        label16.TabIndex = 53
        label16.Text = "X:"
        ' 
        ' groupBoxSizeBox
        ' 
        groupBoxSizeBox.Controls.Add(label20)
        groupBoxSizeBox.Controls.Add(checkBoxBoxShow)
        groupBoxSizeBox.Controls.Add(textBoxBoxFixedZ)
        groupBoxSizeBox.Controls.Add(textBoxBoxFixedY)
        groupBoxSizeBox.Controls.Add(textBoxBoxFixedX)
        groupBoxSizeBox.Controls.Add(radioButtonSizeBoxFixed)
        groupBoxSizeBox.Controls.Add(radioButtonSizeBoxDynamic)
        groupBoxSizeBox.Controls.Add(textBoxBoxSpace)
        groupBoxSizeBox.Controls.Add(label17)
        groupBoxSizeBox.Controls.Add(label18)
        groupBoxSizeBox.Controls.Add(label19)
        groupBoxSizeBox.ForeColor = Drawing.SystemColors.ControlText
        groupBoxSizeBox.Location = New System.Drawing.Point(383, 445)
        groupBoxSizeBox.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        groupBoxSizeBox.Name = "groupBoxSizeBox"
        groupBoxSizeBox.Padding = New System.Windows.Forms.Padding(4, 3, 4, 3)
        groupBoxSizeBox.Size = New System.Drawing.Size(390, 118)
        groupBoxSizeBox.TabIndex = 54
        groupBoxSizeBox.TabStop = False
        groupBoxSizeBox.Text = "Section Box Size"
        ' 
        ' label20
        ' 
        label20.AutoSize = True
        label20.Location = New System.Drawing.Point(250, 23)
        label20.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label20.Name = "label20"
        label20.Size = New System.Drawing.Size(78, 15)
        label20.TabIndex = 63
        label20.Text = "( Z = .9 x HT )" & vbCrLf
        ' 
        ' checkBoxBoxShow
        ' 
        checkBoxBoxShow.AutoSize = True
        checkBoxBoxShow.Location = New System.Drawing.Point(15, 87)
        checkBoxBoxShow.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        checkBoxBoxShow.Name = "checkBoxBoxShow"
        checkBoxBoxShow.Size = New System.Drawing.Size(120, 19)
        checkBoxBoxShow.TabIndex = 62
        checkBoxBoxShow.Text = "Show Section Box"
        checkBoxBoxShow.UseVisualStyleBackColor = True
        ' 
        ' textBoxBoxFixedZ
        ' 
        textBoxBoxFixedZ.Location = New System.Drawing.Point(328, 52)
        textBoxBoxFixedZ.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxBoxFixedZ.Name = "textBoxBoxFixedZ"
        textBoxBoxFixedZ.Size = New System.Drawing.Size(47, 23)
        textBoxBoxFixedZ.TabIndex = 60
        ' 
        ' textBoxBoxFixedY
        ' 
        textBoxBoxFixedY.Location = New System.Drawing.Point(251, 52)
        textBoxBoxFixedY.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxBoxFixedY.Name = "textBoxBoxFixedY"
        textBoxBoxFixedY.Size = New System.Drawing.Size(47, 23)
        textBoxBoxFixedY.TabIndex = 58
        ' 
        ' textBoxBoxFixedX
        ' 
        textBoxBoxFixedX.Location = New System.Drawing.Point(172, 52)
        textBoxBoxFixedX.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxBoxFixedX.Name = "textBoxBoxFixedX"
        textBoxBoxFixedX.Size = New System.Drawing.Size(46, 23)
        textBoxBoxFixedX.TabIndex = 56
        ' 
        ' radioButtonSizeBoxFixed
        ' 
        radioButtonSizeBoxFixed.AutoSize = True
        radioButtonSizeBoxFixed.Location = New System.Drawing.Point(15, 54)
        radioButtonSizeBoxFixed.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        radioButtonSizeBoxFixed.Name = "radioButtonSizeBoxFixed"
        radioButtonSizeBoxFixed.Size = New System.Drawing.Size(118, 19)
        radioButtonSizeBoxFixed.TabIndex = 55
        radioButtonSizeBoxFixed.TabStop = True
        radioButtonSizeBoxFixed.Text = "Fixed Dimensions"
        radioButtonSizeBoxFixed.UseVisualStyleBackColor = True
        ' 
        ' radioButtonSizeBoxDynamic
        ' 
        radioButtonSizeBoxDynamic.AutoSize = True
        radioButtonSizeBoxDynamic.Location = New System.Drawing.Point(15, 21)
        radioButtonSizeBoxDynamic.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        radioButtonSizeBoxDynamic.Name = "radioButtonSizeBoxDynamic"
        radioButtonSizeBoxDynamic.Size = New System.Drawing.Size(129, 19)
        radioButtonSizeBoxDynamic.TabIndex = 54
        radioButtonSizeBoxDynamic.TabStop = True
        radioButtonSizeBoxDynamic.Text = "Space Around Area:"
        radioButtonSizeBoxDynamic.UseVisualStyleBackColor = True
        ' 
        ' textBoxBoxSpace
        ' 
        textBoxBoxSpace.Location = New System.Drawing.Point(172, 20)
        textBoxBoxSpace.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxBoxSpace.Name = "textBoxBoxSpace"
        textBoxBoxSpace.Size = New System.Drawing.Size(46, 23)
        textBoxBoxSpace.TabIndex = 52
        ' 
        ' label17
        ' 
        label17.AutoSize = True
        label17.Location = New System.Drawing.Point(150, 58)
        label17.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label17.Name = "label17"
        label17.Size = New System.Drawing.Size(17, 15)
        label17.TabIndex = 57
        label17.Text = "X:"
        ' 
        ' label18
        ' 
        label18.AutoSize = True
        label18.Location = New System.Drawing.Point(227, 58)
        label18.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label18.Name = "label18"
        label18.Size = New System.Drawing.Size(17, 15)
        label18.TabIndex = 59
        label18.Text = "Y:"
        ' 
        ' label19
        ' 
        label19.AutoSize = True
        label19.Location = New System.Drawing.Point(307, 58)
        label19.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label19.Name = "label19"
        label19.Size = New System.Drawing.Size(17, 15)
        label19.TabIndex = 61
        label19.Text = "Z:"
        ' 
        ' groupBoxSelection
        ' 
        groupBoxSelection.Controls.Add(textBoxParameterGroupBy)
        groupBoxSelection.Controls.Add(radioButtonGroupMultiple)
        groupBoxSelection.Controls.Add(radioButtonGroupSingle)
        groupBoxSelection.Controls.Add(checkBoxListExisting)
        groupBoxSelection.Controls.Add(checkBoxListReverse)
        groupBoxSelection.Controls.Add(textBoxPad2)
        groupBoxSelection.Controls.Add(textBoxPad1)
        groupBoxSelection.Controls.Add(checkBoxPad2)
        groupBoxSelection.Controls.Add(checkBoxPad1)
        groupBoxSelection.Controls.Add(textBoxParameterList2)
        groupBoxSelection.Controls.Add(label7)
        groupBoxSelection.Controls.Add(textBoxParameterList1)
        groupBoxSelection.Controls.Add(label8)
        groupBoxSelection.ForeColor = Drawing.SystemColors.ControlText
        groupBoxSelection.Location = New System.Drawing.Point(383, 20)
        groupBoxSelection.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        groupBoxSelection.Name = "groupBoxSelection"
        groupBoxSelection.Padding = New System.Windows.Forms.Padding(4, 3, 4, 3)
        groupBoxSelection.Size = New System.Drawing.Size(390, 186)
        groupBoxSelection.TabIndex = 50
        groupBoxSelection.TabStop = False
        groupBoxSelection.Text = "Selection Options"
        ' 
        ' textBoxParameterGroupBy
        ' 
        textBoxParameterGroupBy.Location = New System.Drawing.Point(210, 121)
        textBoxParameterGroupBy.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxParameterGroupBy.Name = "textBoxParameterGroupBy"
        textBoxParameterGroupBy.Size = New System.Drawing.Size(162, 23)
        textBoxParameterGroupBy.TabIndex = 37
        ' 
        ' radioButtonGroupMultiple
        ' 
        radioButtonGroupMultiple.AutoSize = True
        radioButtonGroupMultiple.Location = New System.Drawing.Point(14, 121)
        radioButtonGroupMultiple.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        radioButtonGroupMultiple.Name = "radioButtonGroupMultiple"
        radioButtonGroupMultiple.Size = New System.Drawing.Size(166, 19)
        radioButtonGroupMultiple.TabIndex = 36
        radioButtonGroupMultiple.TabStop = True
        radioButtonGroupMultiple.Text = "Group Areas by Parameter:"
        radioButtonGroupMultiple.UseVisualStyleBackColor = True
        ' 
        ' radioButtonGroupSingle
        ' 
        radioButtonGroupSingle.AutoSize = True
        radioButtonGroupSingle.Location = New System.Drawing.Point(13, 16)
        radioButtonGroupSingle.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        radioButtonGroupSingle.Name = "radioButtonGroupSingle"
        radioButtonGroupSingle.Size = New System.Drawing.Size(89, 19)
        radioButtonGroupSingle.TabIndex = 26
        radioButtonGroupSingle.TabStop = True
        radioButtonGroupSingle.Text = "Single Areas"
        radioButtonGroupSingle.UseVisualStyleBackColor = True
        ' 
        ' checkBoxListExisting
        ' 
        checkBoxListExisting.AutoSize = True
        checkBoxListExisting.Location = New System.Drawing.Point(13, 157)
        checkBoxListExisting.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        checkBoxListExisting.Name = "checkBoxListExisting"
        checkBoxListExisting.Size = New System.Drawing.Size(88, 19)
        checkBoxListExisting.TabIndex = 25
        checkBoxListExisting.Text = "List Existing"
        checkBoxListExisting.UseVisualStyleBackColor = True
        ' 
        ' checkBoxListReverse
        ' 
        checkBoxListReverse.AutoSize = True
        checkBoxListReverse.Location = New System.Drawing.Point(161, 157)
        checkBoxListReverse.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        checkBoxListReverse.Name = "checkBoxListReverse"
        checkBoxListReverse.Size = New System.Drawing.Size(87, 19)
        checkBoxListReverse.TabIndex = 24
        checkBoxListReverse.Text = "Reverse List"
        checkBoxListReverse.UseVisualStyleBackColor = True
        ' 
        ' textBoxPad2
        ' 
        textBoxPad2.Location = New System.Drawing.Point(337, 82)
        textBoxPad2.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxPad2.Name = "textBoxPad2"
        textBoxPad2.Size = New System.Drawing.Size(35, 23)
        textBoxPad2.TabIndex = 23
        ' 
        ' textBoxPad1
        ' 
        textBoxPad1.Location = New System.Drawing.Point(161, 83)
        textBoxPad1.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxPad1.Name = "textBoxPad1"
        textBoxPad1.Size = New System.Drawing.Size(35, 23)
        textBoxPad1.TabIndex = 22
        ' 
        ' checkBoxPad2
        ' 
        checkBoxPad2.AutoSize = True
        checkBoxPad2.Location = New System.Drawing.Point(211, 85)
        checkBoxPad2.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        checkBoxPad2.Name = "checkBoxPad2"
        checkBoxPad2.Size = New System.Drawing.Size(95, 19)
        checkBoxPad2.TabIndex = 21
        checkBoxPad2.Text = "Pad w/ Zeros"
        checkBoxPad2.UseVisualStyleBackColor = True
        ' 
        ' checkBoxPad1
        ' 
        checkBoxPad1.AutoSize = True
        checkBoxPad1.Location = New System.Drawing.Point(35, 87)
        checkBoxPad1.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        checkBoxPad1.Name = "checkBoxPad1"
        checkBoxPad1.Size = New System.Drawing.Size(95, 19)
        checkBoxPad1.TabIndex = 20
        checkBoxPad1.Text = "Pad w/ Zeros"
        checkBoxPad1.UseVisualStyleBackColor = True
        ' 
        ' textBoxParameterList2
        ' 
        textBoxParameterList2.Location = New System.Drawing.Point(211, 57)
        textBoxParameterList2.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxParameterList2.Name = "textBoxParameterList2"
        textBoxParameterList2.Size = New System.Drawing.Size(162, 23)
        textBoxParameterList2.TabIndex = 16
        ' 
        ' label7
        ' 
        label7.AutoSize = True
        label7.Location = New System.Drawing.Point(31, 38)
        label7.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label7.Name = "label7"
        label7.Size = New System.Drawing.Size(94, 15)
        label7.TabIndex = 15
        label7.Text = "List Parameter 1:"
        ' 
        ' textBoxParameterList1
        ' 
        textBoxParameterList1.Location = New System.Drawing.Point(35, 58)
        textBoxParameterList1.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxParameterList1.Name = "textBoxParameterList1"
        textBoxParameterList1.Size = New System.Drawing.Size(162, 23)
        textBoxParameterList1.TabIndex = 14
        ' 
        ' label8
        ' 
        label8.AutoSize = True
        label8.Location = New System.Drawing.Point(208, 38)
        label8.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label8.Name = "label8"
        label8.Size = New System.Drawing.Size(94, 15)
        label8.TabIndex = 17
        label8.Text = "List Parameter 2:"
        ' 
        ' groupBox2
        ' 
        groupBox2.Controls.Add(textBoxScale)
        groupBox2.Controls.Add(label5)
        groupBox2.Controls.Add(textBoxPrefixViewTarget)
        groupBox2.Controls.Add(label6)
        groupBox2.Controls.Add(textBoxParameterAreaName)
        groupBox2.Controls.Add(textBoxParameterViewName)
        groupBox2.Controls.Add(label1)
        groupBox2.Controls.Add(label3)
        groupBox2.Controls.Add(label9)
        groupBox2.ForeColor = Drawing.SystemColors.ControlText
        groupBox2.Location = New System.Drawing.Point(383, 212)
        groupBox2.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        groupBox2.Name = "groupBox2"
        groupBox2.Padding = New System.Windows.Forms.Padding(4, 3, 4, 3)
        groupBox2.Size = New System.Drawing.Size(390, 105)
        groupBox2.TabIndex = 51
        groupBox2.TabStop = False
        groupBox2.Text = "Processing Options"
        ' 
        ' textBoxScale
        ' 
        textBoxScale.Location = New System.Drawing.Point(102, 68)
        textBoxScale.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxScale.Name = "textBoxScale"
        textBoxScale.Size = New System.Drawing.Size(65, 23)
        textBoxScale.TabIndex = 43
        ' 
        ' label5
        ' 
        label5.AutoSize = True
        label5.Location = New System.Drawing.Point(10, 73)
        label5.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label5.Name = "label5"
        label5.Size = New System.Drawing.Size(69, 15)
        label5.TabIndex = 42
        label5.Text = "Scale (ratio)"
        ' 
        ' textBoxPrefixViewTarget
        ' 
        textBoxPrefixViewTarget.Location = New System.Drawing.Point(284, 68)
        textBoxPrefixViewTarget.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxPrefixViewTarget.Name = "textBoxPrefixViewTarget"
        textBoxPrefixViewTarget.Size = New System.Drawing.Size(89, 23)
        textBoxPrefixViewTarget.TabIndex = 41
        ' 
        ' label6
        ' 
        label6.AutoSize = True
        label6.Location = New System.Drawing.Point(176, 72)
        label6.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label6.Name = "label6"
        label6.Size = New System.Drawing.Size(103, 15)
        label6.TabIndex = 39
        label6.Text = "View Name Prefix:"
        ' 
        ' textBoxParameterAreaName
        ' 
        textBoxParameterAreaName.Location = New System.Drawing.Point(198, 38)
        textBoxParameterAreaName.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxParameterAreaName.Name = "textBoxParameterAreaName"
        textBoxParameterAreaName.Size = New System.Drawing.Size(174, 23)
        textBoxParameterAreaName.TabIndex = 30
        ' 
        ' textBoxParameterViewName
        ' 
        textBoxParameterViewName.Location = New System.Drawing.Point(13, 38)
        textBoxParameterViewName.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        textBoxParameterViewName.Name = "textBoxParameterViewName"
        textBoxParameterViewName.Size = New System.Drawing.Size(173, 23)
        textBoxParameterViewName.TabIndex = 26
        ' 
        ' label1
        ' 
        label1.AutoSize = True
        label1.Location = New System.Drawing.Point(9, 21)
        label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label1.Name = "label1"
        label1.Size = New System.Drawing.Size(162, 15)
        label1.TabIndex = 27
        label1.Text = "Area Parameter - View Name:"
        ' 
        ' label3
        ' 
        label3.AutoSize = True
        label3.Location = New System.Drawing.Point(196, 21)
        label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label3.Name = "label3"
        label3.Size = New System.Drawing.Size(161, 15)
        label3.TabIndex = 31
        label3.Text = "Area Parameter - Area Name:"
        ' 
        ' label9
        ' 
        label9.AutoSize = True
        label9.Location = New System.Drawing.Point(84, 73)
        label9.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        label9.Name = "label9"
        label9.Size = New System.Drawing.Size(16, 15)
        label9.TabIndex = 43
        label9.Text = "1:"
        ' 
        ' buttonCreate
        ' 
        buttonCreate.Location = New System.Drawing.Point(18, 708)
        buttonCreate.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        buttonCreate.Name = "buttonCreate"
        buttonCreate.Size = New System.Drawing.Size(208, 33)
        buttonCreate.TabIndex = 49
        buttonCreate.Text = "Create Views"
        buttonCreate.UseVisualStyleBackColor = True
        ' 
        ' listBoxAreas
        ' 
        listBoxAreas.FormattingEnabled = True
        listBoxAreas.ItemHeight = 15
        listBoxAreas.Location = New System.Drawing.Point(18, 28)
        listBoxAreas.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        listBoxAreas.Name = "listBoxAreas"
        listBoxAreas.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        listBoxAreas.Size = New System.Drawing.Size(349, 664)
        listBoxAreas.TabIndex = 48
        ' 
        ' ProgressBar1
        ' 
        ProgressBar1.Location = New System.Drawing.Point(18, 708)
        ProgressBar1.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        ProgressBar1.Name = "ProgressBar1"
        ProgressBar1.Size = New System.Drawing.Size(755, 33)
        ProgressBar1.TabIndex = 55
        ' 
        ' labelListTitle
        ' 
        labelListTitle.AutoSize = True
        labelListTitle.Location = New System.Drawing.Point(18, 7)
        labelListTitle.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        labelListTitle.Name = "labelListTitle"
        labelListTitle.Size = New System.Drawing.Size(218, 15)
        labelListTitle.TabIndex = 56
        labelListTitle.Text = "Select Areas For Which to Create a View:"
        ' 
        ' form_ElemViewsFromAreas
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        ClientSize = New System.Drawing.Size(789, 753)
        Controls.Add(labelListTitle)
        Controls.Add(buttonClose)
        Controls.Add(groupBoxSizeCrop)
        Controls.Add(groupBoxViewType)
        Controls.Add(groupBoxSizeBox)
        Controls.Add(groupBoxSelection)
        Controls.Add(groupBox2)
        Controls.Add(buttonCreate)
        Controls.Add(listBoxAreas)
        Controls.Add(ProgressBar1)
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        MaximumSize = New System.Drawing.Size(805, 900)
        MinimumSize = New System.Drawing.Size(805, 792)
        Name = "form_ElemViewsFromAreas"
        Text = "Views From Areas"
        TopMost = True
        groupBoxSizeCrop.ResumeLayout(False)
        groupBoxSizeCrop.PerformLayout()
        groupBoxViewType.ResumeLayout(False)
        groupBoxViewType.PerformLayout()
        groupBoxSizeBox.ResumeLayout(False)
        groupBoxSizeBox.PerformLayout()
        groupBoxSelection.ResumeLayout(False)
        groupBoxSelection.PerformLayout()
        groupBox2.ResumeLayout(False)
        groupBox2.PerformLayout()
        ResumeLayout(False)
        PerformLayout()

    End Sub
    Private WithEvents buttonClose As System.Windows.Forms.Button
    Private WithEvents groupBoxSizeCrop As System.Windows.Forms.GroupBox
    Private WithEvents checkBoxCropShow As System.Windows.Forms.CheckBox
    Private WithEvents textBoxCropFixedY As System.Windows.Forms.TextBox
    Private WithEvents textBoxCropFixedX As System.Windows.Forms.TextBox
    Private WithEvents radioButtonSizeCropFixed As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonSizeCropDynamic As System.Windows.Forms.RadioButton
    Private WithEvents textBoxCropSpace As System.Windows.Forms.TextBox
    Private WithEvents label10 As System.Windows.Forms.Label
    Private WithEvents label11 As System.Windows.Forms.Label
    Private WithEvents groupBoxViewType As System.Windows.Forms.GroupBox
    Private WithEvents checkBoxReplaceExisting As System.Windows.Forms.CheckBox
    Private WithEvents radioButtonType3dBoxCrop As System.Windows.Forms.RadioButton
    Private WithEvents textBoxVectorZ As System.Windows.Forms.TextBox
    Private WithEvents textBoxVectorY As System.Windows.Forms.TextBox
    Private WithEvents textBoxVectorX As System.Windows.Forms.TextBox
    Private WithEvents label4 As System.Windows.Forms.Label
    Private WithEvents radioButtonType3dCrop As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonType2d As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonType3dBox As System.Windows.Forms.RadioButton
    Private WithEvents label14 As System.Windows.Forms.Label
    Private WithEvents label15 As System.Windows.Forms.Label
    Private WithEvents label16 As System.Windows.Forms.Label
    Private WithEvents groupBoxSizeBox As System.Windows.Forms.GroupBox
    Private WithEvents label20 As System.Windows.Forms.Label
    Private WithEvents checkBoxBoxShow As System.Windows.Forms.CheckBox
    Private WithEvents textBoxBoxFixedZ As System.Windows.Forms.TextBox
    Private WithEvents textBoxBoxFixedY As System.Windows.Forms.TextBox
    Private WithEvents textBoxBoxFixedX As System.Windows.Forms.TextBox
    Private WithEvents radioButtonSizeBoxFixed As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonSizeBoxDynamic As System.Windows.Forms.RadioButton
    Private WithEvents textBoxBoxSpace As System.Windows.Forms.TextBox
    Private WithEvents label17 As System.Windows.Forms.Label
    Private WithEvents label18 As System.Windows.Forms.Label
    Private WithEvents label19 As System.Windows.Forms.Label
    Private WithEvents groupBoxSelection As System.Windows.Forms.GroupBox
    Private WithEvents textBoxParameterGroupBy As System.Windows.Forms.TextBox
    Private WithEvents radioButtonGroupMultiple As System.Windows.Forms.RadioButton
    Private WithEvents radioButtonGroupSingle As System.Windows.Forms.RadioButton
    Private WithEvents checkBoxListExisting As System.Windows.Forms.CheckBox
    Private WithEvents checkBoxListReverse As System.Windows.Forms.CheckBox
    Private WithEvents textBoxPad2 As System.Windows.Forms.TextBox
    Private WithEvents textBoxPad1 As System.Windows.Forms.TextBox
    Private WithEvents checkBoxPad2 As System.Windows.Forms.CheckBox
    Private WithEvents checkBoxPad1 As System.Windows.Forms.CheckBox
    Private WithEvents textBoxParameterList2 As System.Windows.Forms.TextBox
    Private WithEvents label7 As System.Windows.Forms.Label
    Private WithEvents textBoxParameterList1 As System.Windows.Forms.TextBox
    Private WithEvents label8 As System.Windows.Forms.Label
    Private WithEvents groupBox2 As System.Windows.Forms.GroupBox
    Private WithEvents textBoxScale As System.Windows.Forms.TextBox
    Private WithEvents label5 As System.Windows.Forms.Label
    Private WithEvents textBoxPrefixViewTarget As System.Windows.Forms.TextBox
    Private WithEvents label6 As System.Windows.Forms.Label
    Private WithEvents textBoxParameterAreaName As System.Windows.Forms.TextBox
    Private WithEvents textBoxParameterViewName As System.Windows.Forms.TextBox
    Private WithEvents label1 As System.Windows.Forms.Label
    Private WithEvents label3 As System.Windows.Forms.Label
    Private WithEvents label9 As System.Windows.Forms.Label
    Private WithEvents buttonCreate As System.Windows.Forms.Button
    Private WithEvents listBoxAreas As System.Windows.Forms.ListBox
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar
    Private WithEvents labelListTitle As System.Windows.Forms.Label
End Class
