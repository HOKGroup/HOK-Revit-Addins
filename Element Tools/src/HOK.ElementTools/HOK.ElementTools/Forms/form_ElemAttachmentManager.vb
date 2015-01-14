Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI

Imports System.Windows.Forms
Imports System.IO


Public Class form_ElemAttachmentManager

    Private Const cOutputFileName As String = "RevitAttachmentLinks.txt"
    Private mSettings As clsSettings
    Private mListItems As New List(Of String)
    Private mPathOutput As String

    Public Sub New(ByVal settings As clsSettings)
        InitializeComponent()
        mSettings = settings

        'mElementSet = elementSet;

        'Check if file can be saved and create message as needed
        mPathOutput = Convert.ToString(mSettings.ProjectFolderPath) & "\" & cOutputFileName
        If settings.ProjectFolderPath = "" Then
            labelMessage.Text = "Text file cannot be written unless project is saved."
        Else
            labelMessage.Text = "Attachment Link information written to:" & vbLf & mPathOutput
        End If

        'Fill the list box
        FillDwgList()
    End Sub

    Private Sub buttonClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonClose.Click
        Me.DialogResult = Windows.Forms.DialogResult.OK
    End Sub

    Private Sub buttonSelect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonSelect.Click
        Dim elementId As ElementId

        Dim docUI As UIDocument = mSettings.UIdoc

#If RELEASE2013 Or RELEASE2014 Then
        'In case anything was already slected by accident
        'Process for each selection.
        For Each listItem As String In listBoxDwg.SelectedItems
            elementId = New ElementId(CInt(Convert.ToInt64(listItem.Substring(0, listItem.IndexOf(" ")))))
            '' ''elementId.Value = CInt(Convert.ToInt64(listItem.Substring(0, listItem.IndexOf(" "))))
            docUI.Selection.Elements.Add(mSettings.Document.GetElement(elementId))
        Next
#ElseIf RELEASE2015 Then
        Dim elementIds As New List(Of ElementId)

        For Each listItem As String In listBoxDwg.SelectedItems
            elementId = New ElementId(CInt(Convert.ToInt64(listItem.Substring(0, listItem.IndexOf(" ")))))
            '' ''elementId.Value = CInt(Convert.ToInt64(listItem.Substring(0, listItem.IndexOf(" "))))
            elementIds.Add(elementId)
        Next
        docUI.Selection.SetElementIds(elementIds)
#End If

    End Sub

    Private Sub FillDwgList()
        Dim elements As New List(Of Element)
        Dim name As String
        Dim elementId As String
        Dim workset As String
        Dim include As Boolean

        'Note that we are using an old style iterator to look through the whole project.  Haven't found a way of 
        'using a filter to get DWG attachments.

        Dim filter As New ElementIsElementTypeFilter(True)
        Dim filCollector As New FilteredElementCollector(mSettings.Document)
        filCollector.WherePasses(filter)
        Dim itorElements As IEnumerator = filCollector.GetElementIterator


        '' ''Dim itorElements As ElementIterator = mSettings.Document.Elements

        itorElements.Reset()

        Dim symbol As FamilySymbol

        Dim importInstance As ImportInstance

        mListItems.Clear()
        listBoxDwg.Items.Clear()

        'Write the text file.  (using also closes file)
        Using streamWriter As New StreamWriter(mPathOutput)
            streamWriter.WriteLine("ElementId  Type        Name                               Workset No.")

            While itorElements.MoveNext()

                'Process ImportInstances
                importInstance = TryCast(itorElements.Current, ImportInstance)
                If importInstance IsNot Nothing Then
                    Try
                        'Debug.Print(itorElements.Current.ToString);

                        ' The old way
                        '' ''name = importInstance.ObjectType.Name

                        ' The New Way
                        name = mSettings.Document.GetElement(importInstance.GetTypeId).Name

                        If name.Length >= 4 Then
                            include = False
                            If name.ToUpper().Contains(".DWG") Then
                                include = True
                            ElseIf name.ToUpper().Contains(".DXF") Then
                                include = True
                            ElseIf name.ToUpper().Contains(".DGN") Then
                                include = True
                            ElseIf name.ToUpper().Contains(".SKP") Then
                                include = True
                            ElseIf name.ToUpper().Contains(".SAT") Then
                                include = True
                            End If
                            If include Then
                                elementId = PadWithBlanks(importInstance.Id.IntegerValue.ToString, 10)
                                name = PadWithBlanks(name, 35)
                                workset = Convert.ToString(importInstance.Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).AsInteger())
                                mListItems.Add(elementId & "instance    " & name & " " & workset)
                                streamWriter.WriteLine(elementId & "instance    " & name & " " & workset)
                                Continue While
                            End If
                        End If
                        Continue While
                        'MessageBox.Show(instance.Name);
                        'Ignore objects with no ObjectType.Name value
                    Catch
                    End Try
                End If

                'Process symbols
                symbol = TryCast(itorElements.Current, FamilySymbol)
                If symbol IsNot Nothing Then

                    ' The NEW way
                    If symbol.GetType Is Nothing Then
                        name = symbol.Name
                        If name.Length >= 4 Then
                            include = False
                            If name.Substring(name.Length - 4, 4).ToUpper() = ".DWG" Then
                                include = True
                            ElseIf name.Substring(name.Length - 4, 4).ToUpper() = ".DXF" Then
                                include = True
                            ElseIf name.Substring(name.Length - 4, 4).ToUpper() = ".DGN" Then
                                include = True
                            ElseIf name.Substring(name.Length - 4, 4).ToUpper() = ".SKP" Then
                                include = True
                            ElseIf name.Substring(name.Length - 4, 4).ToUpper() = ".SAT" Then
                                include = True
                            End If
                            If include Then
                                'if (name.Substring(name.Length - 4, 4).ToUpper() == ".DWG") {
                                name = PadWithBlanks(symbol.Name, 35)
                                elementId = PadWithBlanks(symbol.Id.IntegerValue.ToString, 10)
                                workset = Convert.ToString(symbol.Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).AsInteger())
                                mListItems.Add(elementId & "definition  " & name & " " & workset)
                                streamWriter.WriteLine(elementId & "definition  " & name & " " & workset)
                            End If
                        End If
                    End If
                End If
            End While
        End Using

        mListItems.Sort()
        'if (checkBoxListReverse.Checked) mListItems.Reverse();
        For Each item As String In mListItems
            listBoxDwg.Items.Add(item)
        Next
    End Sub

    Private Function PadWithBlanks(ByVal inputString As String, ByVal noOfCharacters As Integer) As String
        Dim returnString As String = inputString
        While returnString.Length <= noOfCharacters
            returnString = returnString & " "
        End While
        Return returnString
    End Function
End Class