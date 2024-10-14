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
        Me.buttonClose = New System.Windows.Forms.Button()
        Me.groupBoxSizeCrop = New System.Windows.Forms.GroupBox()
        Me.checkBoxCropShow = New System.Windows.Forms.CheckBox()
        Me.textBoxCropFixedY = New System.Windows.Forms.TextBox()
        Me.textBoxCropFixedX = New System.Windows.Forms.TextBox()
        Me.radioButtonSizeCropFixed = New System.Windows.Forms.RadioButton()
        Me.radioButtonSizeCropDynamic = New System.Windows.Forms.RadioButton()
        Me.textBoxCropSpace = New System.Windows.Forms.TextBox()
        Me.label10 = New System.Windows.Forms.Label()
        Me.label11 = New System.Windows.Forms.Label()
        Me.groupBoxViewType = New System.Windows.Forms.GroupBox()
        Me.checkBoxReplaceExisting = New System.Windows.Forms.CheckBox()
        Me.radioButtonType3dBoxCrop = New System.Windows.Forms.RadioButton()
        Me.textBoxVectorZ = New System.Windows.Forms.TextBox()
        Me.textBoxVectorY = New System.Windows.Forms.TextBox()
        Me.textBoxVectorX = New System.Windows.Forms.TextBox()
        Me.label4 = New System.Windows.Forms.Label()
        Me.radioButtonType3dCrop = New System.Windows.Forms.RadioButton()
        Me.radioButtonType2d = New System.Windows.Forms.RadioButton()
        Me.radioButtonType3dBox = New System.Windows.Forms.RadioButton()
        Me.label14 = New System.Windows.Forms.Label()
        Me.label15 = New System.Windows.Forms.Label()
        Me.label16 = New System.Windows.Forms.Label()
        Me.groupBoxSizeBox = New System.Windows.Forms.GroupBox()
        Me.label20 = New System.Windows.Forms.Label()
        Me.checkBoxBoxShow = New System.Windows.Forms.CheckBox()
        Me.textBoxBoxFixedZ = New System.Windows.Forms.TextBox()
        Me.textBoxBoxFixedY = New System.Windows.Forms.TextBox()
        Me.textBoxBoxFixedX = New System.Windows.Forms.TextBox()
        Me.radioButtonSizeBoxFixed = New System.Windows.Forms.RadioButton()
        Me.radioButtonSizeBoxDynamic = New System.Windows.Forms.RadioButton()
        Me.textBoxBoxSpace = New System.Windows.Forms.TextBox()
        Me.label17 = New System.Windows.Forms.Label()
        Me.label18 = New System.Windows.Forms.Label()
        Me.label19 = New System.Windows.Forms.Label()
        Me.groupBoxSelection = New System.Windows.Forms.GroupBox()
        Me.textBoxParameterGroupBy = New System.Windows.Forms.TextBox()
        Me.radioButtonGroupMultiple = New System.Windows.Forms.RadioButton()
        Me.radioButtonGroupSingle = New System.Windows.Forms.RadioButton()
        Me.checkBoxListExisting = New System.Windows.Forms.CheckBox()
        Me.checkBoxListReverse = New System.Windows.Forms.CheckBox()
        Me.textBoxPad2 = New System.Windows.Forms.TextBox()
        Me.textBoxPad1 = New System.Windows.Forms.TextBox()
        Me.checkBoxPad2 = New System.Windows.Forms.CheckBox()
        Me.checkBoxPad1 = New System.Windows.Forms.CheckBox()
        Me.textBoxParameterList2 = New System.Windows.Forms.TextBox()
        Me.label7 = New System.Windows.Forms.Label()
        Me.textBoxParameterList1 = New System.Windows.Forms.TextBox()
        Me.label8 = New System.Windows.Forms.Label()
        Me.groupBox2 = New System.Windows.Forms.GroupBox()
        Me.textBoxScale = New System.Windows.Forms.TextBox()
        Me.label5 = New System.Windows.Forms.Label()
        Me.textBoxPrefixViewTarget = New System.Windows.Forms.TextBox()
        Me.label6 = New System.Windows.Forms.Label()
        Me.textBoxParameterAreaName = New System.Windows.Forms.TextBox()
        Me.textBoxParameterViewName = New System.Windows.Forms.TextBox()
        Me.label1 = New System.Windows.Forms.Label()
        Me.label3 = New System.Windows.Forms.Label()
        Me.label9 = New System.Windows.Forms.Label()
        Me.buttonCreate = New System.Windows.Forms.Button()
        Me.listBoxAreas = New System.Windows.Forms.ListBox()
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.labelListTitle = New System.Windows.Forms.Label()
        Me.groupBoxSizeCrop.SuspendLayout()
        Me.groupBoxViewType.SuspendLayout()
        Me.groupBoxSizeBox.SuspendLayout()
        Me.groupBoxSelection.SuspendLayout()
        Me.groupBox2.SuspendLayout()
        Me.SuspendLayout()
        '
        'buttonClose
        '
        Me.buttonClose.Location = New System.Drawing.Point(484, 614)
        Me.buttonClose.Name = "buttonClose"
        Me.buttonClose.Size = New System.Drawing.Size(178, 29)
        Me.buttonClose.TabIndex = 47
        Me.buttonClose.Text = "Close"
        Me.buttonClose.UseVisualStyleBackColor = True
        '
        'groupBoxSizeCrop
        '
        Me.groupBoxSizeCrop.Controls.Add(Me.checkBoxCropShow)
        Me.groupBoxSizeCrop.Controls.Add(Me.textBoxCropFixedY)
        Me.groupBoxSizeCrop.Controls.Add(Me.textBoxCropFixedX)
        Me.groupBoxSizeCrop.Controls.Add(Me.radioButtonSizeCropFixed)
        Me.groupBoxSizeCrop.Controls.Add(Me.radioButtonSizeCropDynamic)
        Me.groupBoxSizeCrop.Controls.Add(Me.textBoxCropSpace)
        Me.groupBoxSizeCrop.Controls.Add(Me.label10)
        Me.groupBoxSizeCrop.Controls.Add(Me.label11)
        Me.groupBoxSizeCrop.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBoxSizeCrop.Location = New System.Drawing.Point(328, 494)
        Me.groupBoxSizeCrop.Name = "groupBoxSizeCrop"
        Me.groupBoxSizeCrop.Size = New System.Drawing.Size(334, 107)
        Me.groupBoxSizeCrop.TabIndex = 53
        Me.groupBoxSizeCrop.TabStop = False
        Me.groupBoxSizeCrop.Text = "View Size"
        '
        'checkBoxCropShow
        '
        Me.checkBoxCropShow.AutoSize = True
        Me.checkBoxCropShow.Location = New System.Drawing.Point(10, 79)
        Me.checkBoxCropShow.Name = "checkBoxCropShow"
        Me.checkBoxCropShow.Size = New System.Drawing.Size(99, 17)
        Me.checkBoxCropShow.TabIndex = 52
        Me.checkBoxCropShow.Text = "Show Crop Box"
        Me.checkBoxCropShow.UseVisualStyleBackColor = True
        '
        'textBoxCropFixedY
        '
        Me.textBoxCropFixedY.Location = New System.Drawing.Point(216, 46)
        Me.textBoxCropFixedY.Name = "textBoxCropFixedY"
        Me.textBoxCropFixedY.Size = New System.Drawing.Size(41, 20)
        Me.textBoxCropFixedY.TabIndex = 49
        '
        'textBoxCropFixedX
        '
        Me.textBoxCropFixedX.Location = New System.Drawing.Point(147, 46)
        Me.textBoxCropFixedX.Name = "textBoxCropFixedX"
        Me.textBoxCropFixedX.Size = New System.Drawing.Size(40, 20)
        Me.textBoxCropFixedX.TabIndex = 47
        '
        'radioButtonSizeCropFixed
        '
        Me.radioButtonSizeCropFixed.AutoSize = True
        Me.radioButtonSizeCropFixed.Location = New System.Drawing.Point(10, 48)
        Me.radioButtonSizeCropFixed.Name = "radioButtonSizeCropFixed"
        Me.radioButtonSizeCropFixed.Size = New System.Drawing.Size(107, 17)
        Me.radioButtonSizeCropFixed.TabIndex = 46
        Me.radioButtonSizeCropFixed.TabStop = True
        Me.radioButtonSizeCropFixed.Text = "Fixed Dimensions"
        Me.radioButtonSizeCropFixed.UseVisualStyleBackColor = True
        '
        'radioButtonSizeCropDynamic
        '
        Me.radioButtonSizeCropDynamic.AutoSize = True
        Me.radioButtonSizeCropDynamic.Location = New System.Drawing.Point(10, 19)
        Me.radioButtonSizeCropDynamic.Name = "radioButtonSizeCropDynamic"
        Me.radioButtonSizeCropDynamic.Size = New System.Drawing.Size(121, 17)
        Me.radioButtonSizeCropDynamic.TabIndex = 45
        Me.radioButtonSizeCropDynamic.TabStop = True
        Me.radioButtonSizeCropDynamic.Text = "Space Around Area:"
        Me.radioButtonSizeCropDynamic.UseVisualStyleBackColor = True
        '
        'textBoxCropSpace
        '
        Me.textBoxCropSpace.Location = New System.Drawing.Point(147, 18)
        Me.textBoxCropSpace.Name = "textBoxCropSpace"
        Me.textBoxCropSpace.Size = New System.Drawing.Size(41, 20)
        Me.textBoxCropSpace.TabIndex = 28
        '
        'label10
        '
        Me.label10.AutoSize = True
        Me.label10.Location = New System.Drawing.Point(128, 51)
        Me.label10.Name = "label10"
        Me.label10.Size = New System.Drawing.Size(17, 13)
        Me.label10.TabIndex = 48
        Me.label10.Text = "X:"
        '
        'label11
        '
        Me.label11.AutoSize = True
        Me.label11.Location = New System.Drawing.Point(197, 51)
        Me.label11.Name = "label11"
        Me.label11.Size = New System.Drawing.Size(17, 13)
        Me.label11.TabIndex = 50
        Me.label11.Text = "Y:"
        '
        'groupBoxViewType
        '
        Me.groupBoxViewType.Controls.Add(Me.checkBoxReplaceExisting)
        Me.groupBoxViewType.Controls.Add(Me.radioButtonType3dBoxCrop)
        Me.groupBoxViewType.Controls.Add(Me.textBoxVectorZ)
        Me.groupBoxViewType.Controls.Add(Me.textBoxVectorY)
        Me.groupBoxViewType.Controls.Add(Me.textBoxVectorX)
        Me.groupBoxViewType.Controls.Add(Me.label4)
        Me.groupBoxViewType.Controls.Add(Me.radioButtonType3dCrop)
        Me.groupBoxViewType.Controls.Add(Me.radioButtonType2d)
        Me.groupBoxViewType.Controls.Add(Me.radioButtonType3dBox)
        Me.groupBoxViewType.Controls.Add(Me.label14)
        Me.groupBoxViewType.Controls.Add(Me.label15)
        Me.groupBoxViewType.Controls.Add(Me.label16)
        Me.groupBoxViewType.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBoxViewType.Location = New System.Drawing.Point(328, 281)
        Me.groupBoxViewType.Name = "groupBoxViewType"
        Me.groupBoxViewType.Size = New System.Drawing.Size(334, 99)
        Me.groupBoxViewType.TabIndex = 52
        Me.groupBoxViewType.TabStop = False
        Me.groupBoxViewType.Text = "View Type"
        '
        'checkBoxReplaceExisting
        '
        Me.checkBoxReplaceExisting.AutoSize = True
        Me.checkBoxReplaceExisting.Location = New System.Drawing.Point(11, 71)
        Me.checkBoxReplaceExisting.Name = "checkBoxReplaceExisting"
        Me.checkBoxReplaceExisting.Size = New System.Drawing.Size(105, 17)
        Me.checkBoxReplaceExisting.TabIndex = 38
        Me.checkBoxReplaceExisting.Text = "Replace Existing"
        Me.checkBoxReplaceExisting.UseVisualStyleBackColor = True
        '
        'radioButtonType3dBoxCrop
        '
        Me.radioButtonType3dBoxCrop.AutoSize = True
        Me.radioButtonType3dBoxCrop.Location = New System.Drawing.Point(243, 16)
        Me.radioButtonType3dBoxCrop.Name = "radioButtonType3dBoxCrop"
        Me.radioButtonType3dBoxCrop.Size = New System.Drawing.Size(85, 17)
        Me.radioButtonType3dBoxCrop.TabIndex = 58
        Me.radioButtonType3dBoxCrop.TabStop = True
        Me.radioButtonType3dBoxCrop.Text = "3D Box-Crop"
        Me.radioButtonType3dBoxCrop.UseVisualStyleBackColor = True
        '
        'textBoxVectorZ
        '
        Me.textBoxVectorZ.Location = New System.Drawing.Point(278, 41)
        Me.textBoxVectorZ.Name = "textBoxVectorZ"
        Me.textBoxVectorZ.Size = New System.Drawing.Size(40, 20)
        Me.textBoxVectorZ.TabIndex = 56
        '
        'textBoxVectorY
        '
        Me.textBoxVectorY.Location = New System.Drawing.Point(215, 42)
        Me.textBoxVectorY.Name = "textBoxVectorY"
        Me.textBoxVectorY.Size = New System.Drawing.Size(40, 20)
        Me.textBoxVectorY.TabIndex = 54
        '
        'textBoxVectorX
        '
        Me.textBoxVectorX.Location = New System.Drawing.Point(144, 42)
        Me.textBoxVectorX.Name = "textBoxVectorX"
        Me.textBoxVectorX.Size = New System.Drawing.Size(40, 20)
        Me.textBoxVectorX.TabIndex = 52
        '
        'label4
        '
        Me.label4.AutoSize = True
        Me.label4.Location = New System.Drawing.Point(8, 45)
        Me.label4.Name = "label4"
        Me.label4.Size = New System.Drawing.Size(100, 13)
        Me.label4.TabIndex = 47
        Me.label4.Text = "3D Direction Vector"
        '
        'radioButtonType3dCrop
        '
        Me.radioButtonType3dCrop.AutoSize = True
        Me.radioButtonType3dCrop.Location = New System.Drawing.Point(164, 16)
        Me.radioButtonType3dCrop.Name = "radioButtonType3dCrop"
        Me.radioButtonType3dCrop.Size = New System.Drawing.Size(64, 17)
        Me.radioButtonType3dCrop.TabIndex = 46
        Me.radioButtonType3dCrop.TabStop = True
        Me.radioButtonType3dCrop.Text = "3D Crop"
        Me.radioButtonType3dCrop.UseVisualStyleBackColor = True
        '
        'radioButtonType2d
        '
        Me.radioButtonType2d.AutoSize = True
        Me.radioButtonType2d.Location = New System.Drawing.Point(10, 16)
        Me.radioButtonType2d.Name = "radioButtonType2d"
        Me.radioButtonType2d.Size = New System.Drawing.Size(39, 17)
        Me.radioButtonType2d.TabIndex = 44
        Me.radioButtonType2d.TabStop = True
        Me.radioButtonType2d.Text = "2D"
        Me.radioButtonType2d.UseVisualStyleBackColor = True
        '
        'radioButtonType3dBox
        '
        Me.radioButtonType3dBox.AutoSize = True
        Me.radioButtonType3dBox.Location = New System.Drawing.Point(86, 16)
        Me.radioButtonType3dBox.Name = "radioButtonType3dBox"
        Me.radioButtonType3dBox.Size = New System.Drawing.Size(60, 17)
        Me.radioButtonType3dBox.TabIndex = 45
        Me.radioButtonType3dBox.TabStop = True
        Me.radioButtonType3dBox.Text = "3D Box"
        Me.radioButtonType3dBox.UseVisualStyleBackColor = True
        '
        'label14
        '
        Me.label14.AutoSize = True
        Me.label14.Location = New System.Drawing.Point(260, 45)
        Me.label14.Name = "label14"
        Me.label14.Size = New System.Drawing.Size(17, 13)
        Me.label14.TabIndex = 57
        Me.label14.Text = "Z:"
        '
        'label15
        '
        Me.label15.AutoSize = True
        Me.label15.Location = New System.Drawing.Point(196, 45)
        Me.label15.Name = "label15"
        Me.label15.Size = New System.Drawing.Size(17, 13)
        Me.label15.TabIndex = 55
        Me.label15.Text = "Y:"
        '
        'label16
        '
        Me.label16.AutoSize = True
        Me.label16.Location = New System.Drawing.Point(126, 45)
        Me.label16.Name = "label16"
        Me.label16.Size = New System.Drawing.Size(17, 13)
        Me.label16.TabIndex = 53
        Me.label16.Text = "X:"
        '
        'groupBoxSizeBox
        '
        Me.groupBoxSizeBox.Controls.Add(Me.label20)
        Me.groupBoxSizeBox.Controls.Add(Me.checkBoxBoxShow)
        Me.groupBoxSizeBox.Controls.Add(Me.textBoxBoxFixedZ)
        Me.groupBoxSizeBox.Controls.Add(Me.textBoxBoxFixedY)
        Me.groupBoxSizeBox.Controls.Add(Me.textBoxBoxFixedX)
        Me.groupBoxSizeBox.Controls.Add(Me.radioButtonSizeBoxFixed)
        Me.groupBoxSizeBox.Controls.Add(Me.radioButtonSizeBoxDynamic)
        Me.groupBoxSizeBox.Controls.Add(Me.textBoxBoxSpace)
        Me.groupBoxSizeBox.Controls.Add(Me.label17)
        Me.groupBoxSizeBox.Controls.Add(Me.label18)
        Me.groupBoxSizeBox.Controls.Add(Me.label19)
        Me.groupBoxSizeBox.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBoxSizeBox.Location = New System.Drawing.Point(328, 386)
        Me.groupBoxSizeBox.Name = "groupBoxSizeBox"
        Me.groupBoxSizeBox.Size = New System.Drawing.Size(334, 102)
        Me.groupBoxSizeBox.TabIndex = 54
        Me.groupBoxSizeBox.TabStop = False
        Me.groupBoxSizeBox.Text = "Section Box Size"
        '
        'label20
        '
        Me.label20.AutoSize = True
        Me.label20.Location = New System.Drawing.Point(214, 20)
        Me.label20.Name = "label20"
        Me.label20.Size = New System.Drawing.Size(73, 13)
        Me.label20.TabIndex = 63
        Me.label20.Text = "( Z = .9 x HT )" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10)
        '
        'checkBoxBoxShow
        '
        Me.checkBoxBoxShow.AutoSize = True
        Me.checkBoxBoxShow.Location = New System.Drawing.Point(13, 75)
        Me.checkBoxBoxShow.Name = "checkBoxBoxShow"
        Me.checkBoxBoxShow.Size = New System.Drawing.Size(113, 17)
        Me.checkBoxBoxShow.TabIndex = 62
        Me.checkBoxBoxShow.Text = "Show Section Box"
        Me.checkBoxBoxShow.UseVisualStyleBackColor = True
        '
        'textBoxBoxFixedZ
        '
        Me.textBoxBoxFixedZ.Location = New System.Drawing.Point(281, 45)
        Me.textBoxBoxFixedZ.Name = "textBoxBoxFixedZ"
        Me.textBoxBoxFixedZ.Size = New System.Drawing.Size(41, 20)
        Me.textBoxBoxFixedZ.TabIndex = 60
        '
        'textBoxBoxFixedY
        '
        Me.textBoxBoxFixedY.Location = New System.Drawing.Point(215, 45)
        Me.textBoxBoxFixedY.Name = "textBoxBoxFixedY"
        Me.textBoxBoxFixedY.Size = New System.Drawing.Size(41, 20)
        Me.textBoxBoxFixedY.TabIndex = 58
        '
        'textBoxBoxFixedX
        '
        Me.textBoxBoxFixedX.Location = New System.Drawing.Point(147, 45)
        Me.textBoxBoxFixedX.Name = "textBoxBoxFixedX"
        Me.textBoxBoxFixedX.Size = New System.Drawing.Size(40, 20)
        Me.textBoxBoxFixedX.TabIndex = 56
        '
        'radioButtonSizeBoxFixed
        '
        Me.radioButtonSizeBoxFixed.AutoSize = True
        Me.radioButtonSizeBoxFixed.Location = New System.Drawing.Point(13, 47)
        Me.radioButtonSizeBoxFixed.Name = "radioButtonSizeBoxFixed"
        Me.radioButtonSizeBoxFixed.Size = New System.Drawing.Size(107, 17)
        Me.radioButtonSizeBoxFixed.TabIndex = 55
        Me.radioButtonSizeBoxFixed.TabStop = True
        Me.radioButtonSizeBoxFixed.Text = "Fixed Dimensions"
        Me.radioButtonSizeBoxFixed.UseVisualStyleBackColor = True
        '
        'radioButtonSizeBoxDynamic
        '
        Me.radioButtonSizeBoxDynamic.AutoSize = True
        Me.radioButtonSizeBoxDynamic.Location = New System.Drawing.Point(13, 18)
        Me.radioButtonSizeBoxDynamic.Name = "radioButtonSizeBoxDynamic"
        Me.radioButtonSizeBoxDynamic.Size = New System.Drawing.Size(121, 17)
        Me.radioButtonSizeBoxDynamic.TabIndex = 54
        Me.radioButtonSizeBoxDynamic.TabStop = True
        Me.radioButtonSizeBoxDynamic.Text = "Space Around Area:"
        Me.radioButtonSizeBoxDynamic.UseVisualStyleBackColor = True
        '
        'textBoxBoxSpace
        '
        Me.textBoxBoxSpace.Location = New System.Drawing.Point(147, 17)
        Me.textBoxBoxSpace.Name = "textBoxBoxSpace"
        Me.textBoxBoxSpace.Size = New System.Drawing.Size(40, 20)
        Me.textBoxBoxSpace.TabIndex = 52
        '
        'label17
        '
        Me.label17.AutoSize = True
        Me.label17.Location = New System.Drawing.Point(129, 50)
        Me.label17.Name = "label17"
        Me.label17.Size = New System.Drawing.Size(17, 13)
        Me.label17.TabIndex = 57
        Me.label17.Text = "X:"
        '
        'label18
        '
        Me.label18.AutoSize = True
        Me.label18.Location = New System.Drawing.Point(195, 50)
        Me.label18.Name = "label18"
        Me.label18.Size = New System.Drawing.Size(17, 13)
        Me.label18.TabIndex = 59
        Me.label18.Text = "Y:"
        '
        'label19
        '
        Me.label19.AutoSize = True
        Me.label19.Location = New System.Drawing.Point(263, 50)
        Me.label19.Name = "label19"
        Me.label19.Size = New System.Drawing.Size(17, 13)
        Me.label19.TabIndex = 61
        Me.label19.Text = "Z:"
        '
        'groupBoxSelection
        '
        Me.groupBoxSelection.Controls.Add(Me.textBoxParameterGroupBy)
        Me.groupBoxSelection.Controls.Add(Me.radioButtonGroupMultiple)
        Me.groupBoxSelection.Controls.Add(Me.radioButtonGroupSingle)
        Me.groupBoxSelection.Controls.Add(Me.checkBoxListExisting)
        Me.groupBoxSelection.Controls.Add(Me.checkBoxListReverse)
        Me.groupBoxSelection.Controls.Add(Me.textBoxPad2)
        Me.groupBoxSelection.Controls.Add(Me.textBoxPad1)
        Me.groupBoxSelection.Controls.Add(Me.checkBoxPad2)
        Me.groupBoxSelection.Controls.Add(Me.checkBoxPad1)
        Me.groupBoxSelection.Controls.Add(Me.textBoxParameterList2)
        Me.groupBoxSelection.Controls.Add(Me.label7)
        Me.groupBoxSelection.Controls.Add(Me.textBoxParameterList1)
        Me.groupBoxSelection.Controls.Add(Me.label8)
        Me.groupBoxSelection.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBoxSelection.Location = New System.Drawing.Point(328, 17)
        Me.groupBoxSelection.Name = "groupBoxSelection"
        Me.groupBoxSelection.Size = New System.Drawing.Size(334, 161)
        Me.groupBoxSelection.TabIndex = 50
        Me.groupBoxSelection.TabStop = False
        Me.groupBoxSelection.Text = "Selection Options"
        '
        'textBoxParameterGroupBy
        '
        Me.textBoxParameterGroupBy.Location = New System.Drawing.Point(180, 105)
        Me.textBoxParameterGroupBy.Name = "textBoxParameterGroupBy"
        Me.textBoxParameterGroupBy.Size = New System.Drawing.Size(139, 20)
        Me.textBoxParameterGroupBy.TabIndex = 37
        '
        'radioButtonGroupMultiple
        '
        Me.radioButtonGroupMultiple.AutoSize = True
        Me.radioButtonGroupMultiple.Location = New System.Drawing.Point(12, 105)
        Me.radioButtonGroupMultiple.Name = "radioButtonGroupMultiple"
        Me.radioButtonGroupMultiple.Size = New System.Drawing.Size(152, 17)
        Me.radioButtonGroupMultiple.TabIndex = 36
        Me.radioButtonGroupMultiple.TabStop = True
        Me.radioButtonGroupMultiple.Text = "Group Areas by Parameter:"
        Me.radioButtonGroupMultiple.UseVisualStyleBackColor = True
        '
        'radioButtonGroupSingle
        '
        Me.radioButtonGroupSingle.AutoSize = True
        Me.radioButtonGroupSingle.Location = New System.Drawing.Point(11, 14)
        Me.radioButtonGroupSingle.Name = "radioButtonGroupSingle"
        Me.radioButtonGroupSingle.Size = New System.Drawing.Size(84, 17)
        Me.radioButtonGroupSingle.TabIndex = 26
        Me.radioButtonGroupSingle.TabStop = True
        Me.radioButtonGroupSingle.Text = "Single Areas"
        Me.radioButtonGroupSingle.UseVisualStyleBackColor = True
        '
        'checkBoxListExisting
        '
        Me.checkBoxListExisting.AutoSize = True
        Me.checkBoxListExisting.Location = New System.Drawing.Point(11, 136)
        Me.checkBoxListExisting.Name = "checkBoxListExisting"
        Me.checkBoxListExisting.Size = New System.Drawing.Size(81, 17)
        Me.checkBoxListExisting.TabIndex = 25
        Me.checkBoxListExisting.Text = "List Existing"
        Me.checkBoxListExisting.UseVisualStyleBackColor = True
        '
        'checkBoxListReverse
        '
        Me.checkBoxListReverse.AutoSize = True
        Me.checkBoxListReverse.Location = New System.Drawing.Point(138, 136)
        Me.checkBoxListReverse.Name = "checkBoxListReverse"
        Me.checkBoxListReverse.Size = New System.Drawing.Size(85, 17)
        Me.checkBoxListReverse.TabIndex = 24
        Me.checkBoxListReverse.Text = "Reverse List"
        Me.checkBoxListReverse.UseVisualStyleBackColor = True
        '
        'textBoxPad2
        '
        Me.textBoxPad2.Location = New System.Drawing.Point(289, 71)
        Me.textBoxPad2.Name = "textBoxPad2"
        Me.textBoxPad2.Size = New System.Drawing.Size(31, 20)
        Me.textBoxPad2.TabIndex = 23
        '
        'textBoxPad1
        '
        Me.textBoxPad1.Location = New System.Drawing.Point(138, 72)
        Me.textBoxPad1.Name = "textBoxPad1"
        Me.textBoxPad1.Size = New System.Drawing.Size(31, 20)
        Me.textBoxPad1.TabIndex = 22
        '
        'checkBoxPad2
        '
        Me.checkBoxPad2.AutoSize = True
        Me.checkBoxPad2.Location = New System.Drawing.Point(181, 74)
        Me.checkBoxPad2.Name = "checkBoxPad2"
        Me.checkBoxPad2.Size = New System.Drawing.Size(91, 17)
        Me.checkBoxPad2.TabIndex = 21
        Me.checkBoxPad2.Text = "Pad w/ Zeros"
        Me.checkBoxPad2.UseVisualStyleBackColor = True
        '
        'checkBoxPad1
        '
        Me.checkBoxPad1.AutoSize = True
        Me.checkBoxPad1.Location = New System.Drawing.Point(30, 75)
        Me.checkBoxPad1.Name = "checkBoxPad1"
        Me.checkBoxPad1.Size = New System.Drawing.Size(91, 17)
        Me.checkBoxPad1.TabIndex = 20
        Me.checkBoxPad1.Text = "Pad w/ Zeros"
        Me.checkBoxPad1.UseVisualStyleBackColor = True
        '
        'textBoxParameterList2
        '
        Me.textBoxParameterList2.Location = New System.Drawing.Point(181, 49)
        Me.textBoxParameterList2.Name = "textBoxParameterList2"
        Me.textBoxParameterList2.Size = New System.Drawing.Size(139, 20)
        Me.textBoxParameterList2.TabIndex = 16
        '
        'label7
        '
        Me.label7.AutoSize = True
        Me.label7.Location = New System.Drawing.Point(27, 33)
        Me.label7.Name = "label7"
        Me.label7.Size = New System.Drawing.Size(86, 13)
        Me.label7.TabIndex = 15
        Me.label7.Text = "List Parameter 1:"
        '
        'textBoxParameterList1
        '
        Me.textBoxParameterList1.Location = New System.Drawing.Point(30, 50)
        Me.textBoxParameterList1.Name = "textBoxParameterList1"
        Me.textBoxParameterList1.Size = New System.Drawing.Size(139, 20)
        Me.textBoxParameterList1.TabIndex = 14
        '
        'label8
        '
        Me.label8.AutoSize = True
        Me.label8.Location = New System.Drawing.Point(178, 33)
        Me.label8.Name = "label8"
        Me.label8.Size = New System.Drawing.Size(86, 13)
        Me.label8.TabIndex = 17
        Me.label8.Text = "List Parameter 2:"
        '
        'groupBox2
        '
        Me.groupBox2.Controls.Add(Me.textBoxScale)
        Me.groupBox2.Controls.Add(Me.label5)
        Me.groupBox2.Controls.Add(Me.textBoxPrefixViewTarget)
        Me.groupBox2.Controls.Add(Me.label6)
        Me.groupBox2.Controls.Add(Me.textBoxParameterAreaName)
        Me.groupBox2.Controls.Add(Me.textBoxParameterViewName)
        Me.groupBox2.Controls.Add(Me.label1)
        Me.groupBox2.Controls.Add(Me.label3)
        Me.groupBox2.Controls.Add(Me.label9)
        Me.groupBox2.ForeColor = System.Drawing.SystemColors.ControlText
        Me.groupBox2.Location = New System.Drawing.Point(328, 184)
        Me.groupBox2.Name = "groupBox2"
        Me.groupBox2.Size = New System.Drawing.Size(334, 91)
        Me.groupBox2.TabIndex = 51
        Me.groupBox2.TabStop = False
        Me.groupBox2.Text = "Processing Options"
        '
        'textBoxScale
        '
        Me.textBoxScale.Location = New System.Drawing.Point(87, 59)
        Me.textBoxScale.Name = "textBoxScale"
        Me.textBoxScale.Size = New System.Drawing.Size(56, 20)
        Me.textBoxScale.TabIndex = 43
        '
        'label5
        '
        Me.label5.AutoSize = True
        Me.label5.Location = New System.Drawing.Point(9, 63)
        Me.label5.Name = "label5"
        Me.label5.Size = New System.Drawing.Size(63, 13)
        Me.label5.TabIndex = 42
        Me.label5.Text = "Scale (ratio)"
        '
        'textBoxPrefixViewTarget
        '
        Me.textBoxPrefixViewTarget.Location = New System.Drawing.Point(243, 59)
        Me.textBoxPrefixViewTarget.Name = "textBoxPrefixViewTarget"
        Me.textBoxPrefixViewTarget.Size = New System.Drawing.Size(77, 20)
        Me.textBoxPrefixViewTarget.TabIndex = 41
        '
        'label6
        '
        Me.label6.AutoSize = True
        Me.label6.Location = New System.Drawing.Point(151, 62)
        Me.label6.Name = "label6"
        Me.label6.Size = New System.Drawing.Size(93, 13)
        Me.label6.TabIndex = 39
        Me.label6.Text = "View Name Prefix:"
        '
        'textBoxParameterAreaName
        '
        Me.textBoxParameterAreaName.Location = New System.Drawing.Point(170, 33)
        Me.textBoxParameterAreaName.Name = "textBoxParameterAreaName"
        Me.textBoxParameterAreaName.Size = New System.Drawing.Size(150, 20)
        Me.textBoxParameterAreaName.TabIndex = 30
        '
        'textBoxParameterViewName
        '
        Me.textBoxParameterViewName.Location = New System.Drawing.Point(11, 33)
        Me.textBoxParameterViewName.Name = "textBoxParameterViewName"
        Me.textBoxParameterViewName.Size = New System.Drawing.Size(149, 20)
        Me.textBoxParameterViewName.TabIndex = 26
        '
        'label1
        '
        Me.label1.AutoSize = True
        Me.label1.Location = New System.Drawing.Point(8, 18)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(146, 13)
        Me.label1.TabIndex = 27
        Me.label1.Text = "Area Parameter - View Name:"
        '
        'label3
        '
        Me.label3.AutoSize = True
        Me.label3.Location = New System.Drawing.Point(168, 18)
        Me.label3.Name = "label3"
        Me.label3.Size = New System.Drawing.Size(145, 13)
        Me.label3.TabIndex = 31
        Me.label3.Text = "Area Parameter - Area Name:"
        '
        'label9
        '
        Me.label9.AutoSize = True
        Me.label9.Location = New System.Drawing.Point(72, 63)
        Me.label9.Name = "label9"
        Me.label9.Size = New System.Drawing.Size(16, 13)
        Me.label9.TabIndex = 43
        Me.label9.Text = "1:"
        '
        'buttonCreate
        '
        Me.buttonCreate.Location = New System.Drawing.Point(15, 614)
        Me.buttonCreate.Name = "buttonCreate"
        Me.buttonCreate.Size = New System.Drawing.Size(178, 29)
        Me.buttonCreate.TabIndex = 49
        Me.buttonCreate.Text = "Create Views"
        Me.buttonCreate.UseVisualStyleBackColor = True
        '
        'listBoxAreas
        '
        Me.listBoxAreas.FormattingEnabled = True
        Me.listBoxAreas.Location = New System.Drawing.Point(15, 24)
        Me.listBoxAreas.Name = "listBoxAreas"
        Me.listBoxAreas.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.listBoxAreas.Size = New System.Drawing.Size(300, 576)
        Me.listBoxAreas.TabIndex = 48
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(15, 614)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(647, 29)
        Me.ProgressBar1.TabIndex = 55
        '
        'labelListTitle
        '
        Me.labelListTitle.AutoSize = True
        Me.labelListTitle.Location = New System.Drawing.Point(15, 6)
        Me.labelListTitle.Name = "labelListTitle"
        Me.labelListTitle.Size = New System.Drawing.Size(203, 13)
        Me.labelListTitle.TabIndex = 56
        Me.labelListTitle.Text = "Select Areas For Which to Create a View:"
        '
        'form_ElemViewsFromAreas
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(676, 654)
        Me.Controls.Add(Me.labelListTitle)
        Me.Controls.Add(Me.buttonClose)
        Me.Controls.Add(Me.groupBoxSizeCrop)
        Me.Controls.Add(Me.groupBoxViewType)
        Me.Controls.Add(Me.groupBoxSizeBox)
        Me.Controls.Add(Me.groupBoxSelection)
        Me.Controls.Add(Me.groupBox2)
        Me.Controls.Add(Me.buttonCreate)
        Me.Controls.Add(Me.listBoxAreas)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximumSize = New System.Drawing.Size(692, 692)
        Me.MinimumSize = New System.Drawing.Size(692, 692)
        Me.Name = "form_ElemViewsFromAreas"
        Me.Text = "Views From Areas"
        Me.TopMost = True
        Me.groupBoxSizeCrop.ResumeLayout(False)
        Me.groupBoxSizeCrop.PerformLayout()
        Me.groupBoxViewType.ResumeLayout(False)
        Me.groupBoxViewType.PerformLayout()
        Me.groupBoxSizeBox.ResumeLayout(False)
        Me.groupBoxSizeBox.PerformLayout()
        Me.groupBoxSelection.ResumeLayout(False)
        Me.groupBoxSelection.PerformLayout()
        Me.groupBox2.ResumeLayout(False)
        Me.groupBox2.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

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
