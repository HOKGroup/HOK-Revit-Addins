Imports System.IO
Imports System.Reflection
Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI
Imports System.Windows.Forms

''' <summary>
''' Application settings manager
''' </summary>
''' <remarks></remarks>
Public Class clsSettings

    Private m_CommandData As ExternalCommandData
    Private m_IniFileName As String = "SheetMaker.ini"
    Private m_IniPath As String
    Private m_AccessFilePath As String
    Private m_ExcelFilePath As String
    Private m_Application As UIApplication
    Private m_Doc As Document
    Private m_Views As New Dictionary(Of String, Autodesk.Revit.DB.View)
    Private m_Sheets As New Dictionary(Of String, ViewSheet)

    ' New Scanning
    Private m_clsSheets As New Dictionary(Of String, clsSheet)
    Private m_clsTblks As New Dictionary(Of String, clsTblk)

    ''' <summary>
    ''' Class Constructor
    ''' </summary>
    ''' <param name="commandData">Revit ExternalCommandData object</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal commandData As ExternalCommandData)
        ' Record general variables
        m_CommandData = commandData
        m_Doc = m_CommandData.Application.ActiveUIDocument.Document
        ' Calculate the location for which to save the ini file
        Dim directoryName As String = String.Empty
        If m_Doc.IsWorkshared = True Then
            ' Save workshared ini's at location of model
            If (String.IsNullOrEmpty(m_Doc.GetWorksharingCentralModelPath().ToString())) Then
                directoryName = Path.GetDirectoryName(m_Doc.GetWorksharingCentralModelPath().ToString())
            ElseIf String.IsNullOrEmpty(m_Doc.PathName) = False Then
                directoryName = Path.GetDirectoryName(m_Doc.PathName)
            End If

        ElseIf String.IsNullOrEmpty(m_Doc.PathName) = False Then
            ' Save non workshared ini's at local model
            directoryName = Path.GetDirectoryName(m_Doc.PathName)
        End If

        If String.IsNullOrEmpty(directoryName) = False Then
            m_IniPath = Path.Combine(directoryName, m_IniFileName)
        End If


        If Not File.Exists(m_IniPath) Then
            ' Files not saved will have their ini saved in the base model directory

            Dim m_Dlg As New TaskDialog("Sheet Manager")
            m_Dlg.MainInstruction = "SheetMaker.ini File Not Found"
            m_Dlg.MainContent = "Failed to find INI file." & vbLf & "Please create a new INI file or open an existing file."
            m_Dlg.AllowCancellation = True
            m_Dlg.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Create a New INI File.")
            m_Dlg.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Open an Existing INI File.")

            Dim result As TaskDialogResult = m_Dlg.Show
            Select Case result
                Case TaskDialogResult.CommandLink1
                    Dim folderDialog As FolderBrowserDialog = New FolderBrowserDialog()
                    folderDialog.Description = "Select the directory that you want to use As the default."
                    If (Directory.Exists(directoryName)) Then
                        folderDialog.SelectedPath = directoryName
                    End If
                    Dim fdr As DialogResult = folderDialog.ShowDialog()
                    If (fdr = DialogResult.OK) Then
                        m_IniPath = folderDialog.SelectedPath & "\" & m_IniFileName
                        WriteIniFile()
                    End If
                Case TaskDialogResult.CommandLink2
                    Dim openfileDialog As OpenFileDialog = New OpenFileDialog()
                    openfileDialog.DefaultExt = "ini"
                    openfileDialog.Filter = "ini files (*.ini)|*.ini"
                    Dim odr As DialogResult = openfileDialog.ShowDialog()
                    If (odr = DialogResult.OK) Then
                        Dim openFileName As String = openfileDialog.FileName
                        If (openFileName.Contains(m_IniFileName)) Then
                            m_IniPath = openFileName
                        Else
                            MessageBox.Show("Please select a valid ini file." & vbLf & "The file name should be SheetMaker.ini", "File Name: SheetMaker.ini", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                            odr = openfileDialog.ShowDialog()
                        End If
                    End If
            End Select
        End If

        Dim currentFolder As String = Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.LastIndexOf("\"))
        m_AccessFilePath = currentFolder & "\SheetMakerSampleData.accdb"
        m_ExcelFilePath = currentFolder & "\SheetMakerSampleData.xlsx"

        ' List Sheets and Titleblocks:
        GetSheetsAndTitleblockInstances()

        ' Get the Views
        Dim m_dViews As IList(Of Element)
        Dim m_dViewSheet As ViewSheet
        Dim m_dView As Autodesk.Revit.DB.View
        Dim CollectorViews As New FilteredElementCollector(m_Doc)
        CollectorViews.OfCategory(BuiltInCategory.OST_Views)
        m_dViews = CollectorViews.ToElements()
        ' Verify what we've collected
        For Each elementTest As Element In m_dViews
            m_dView = TryCast(elementTest, Autodesk.Revit.DB.View)
            m_dViewSheet = TryCast(elementTest, ViewSheet)
            If m_dViewSheet IsNot Nothing Then
                Continue For
            End If
            'We don't want to get any of the sheets in case they had the same name 
            If Not m_Views.ContainsKey(m_dView.ViewName) Then
                m_Views.Add(m_dView.ViewName, m_dView)
            End If
        Next

        Dim collectorSchedule As New FilteredElementCollector(m_Doc)
        collectorSchedule.OfClass(GetType(ViewSchedule))
        Dim m_dSchedules As IList(Of Element)
        m_dSchedules = collectorSchedule.ToElements()

        For Each element As Element In m_dSchedules
            Dim schedule As ViewSchedule = TryCast(element, ViewSchedule)
            If Not m_Views.ContainsKey(schedule.ViewName) Then
                m_Views.Add(schedule.ViewName, schedule)
            End If
        Next

        'Reading ini overrides defaults
        ReadIniFile()



    End Sub

    ''' <summary>
    ''' Acquire a list of all sheets and titleblock instances
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub GetSheetsAndTitleblockInstances()

        ' Get all Sheets as Titleblocks
        Dim m_ColTblk As New FilteredElementCollector(m_Doc)
        m_ColTblk.WhereElementIsNotElementType()
        Dim m_Tblk As IList(Of Element)
        m_Tblk = m_ColTblk.OfCategory(BuiltInCategory.OST_TitleBlocks).ToElements

        ' List all Paras...
        For Each x As Element In m_Tblk

            Dim m_ParaList As New List(Of clsPara)

            Dim m_SheetNumber As String = ""

            For Each p As Parameter In x.Parameters
                If p IsNot Nothing Then
                    Dim m_para As New clsPara(p)

                    If m_para.Name = "Sheet Number" Then
                        m_SheetNumber = m_para.Value
                    End If

                    m_ParaList.Add(m_para)
                End If
            Next

            Dim m_clsTblkItem As New clsTblk(m_ParaList, x)

            Try
                m_clsTblks.Add(m_SheetNumber, m_clsTblkItem)
            Catch ex As Exception
                ' Dictionary Failure
            End Try

        Next


        ' Get all Sheets as Sheets
        Dim m_ColSheets As New FilteredElementCollector(m_Doc)
        m_ColSheets.WhereElementIsNotElementType()
        Dim m_ListSheets As IList(Of Element)
        m_ListSheets = m_ColSheets.OfCategory(BuiltInCategory.OST_Sheets).ToElements

        ' List all Paras...
        For Each x In m_ListSheets
            Try
                Dim m_Sht As ViewSheet = TryCast(x, ViewSheet)

                Dim m_SheetNumber As String = ""

                ' Add to the source dictionary object
                If Not m_Sheets.ContainsKey(m_Sht.SheetNumber) Then
                    m_Sheets.Add(m_Sht.SheetNumber, m_Sht)
                End If

                Dim m_ParaList As New List(Of clsPara)

                For Each p As Parameter In m_Sht.Parameters
                    If p IsNot Nothing Then
                        Dim m_para As New clsPara(p)

                        If m_para.Name = "Sheet Number" Then
                            m_SheetNumber = m_para.Value
                        End If

                        m_ParaList.Add(m_para)
                    End If
                Next

                Dim m_clsSheetItem As New clsSheet(m_ParaList, m_Sht)

                Try
                    m_clsSheets.Add(m_SheetNumber, m_clsSheetItem)
                Catch ex As Exception
                    ' Dictionary Failure
                End Try

            Catch ex As Exception

            End Try

        Next

    End Sub

    ''' <summary>
    ''' Write the ini file out for settings reuse
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WriteIniFile() ' Add info for views
        Try
            'Note, if file doesn't exist it is created
            ' The using statement also closes the StreamWriter.
            If m_IniPath <> "" Then
                'incase project has not been saved yet
                Using sw As New StreamWriter(m_IniPath)
                    sw.WriteLine(m_AccessFilePath)
                    sw.WriteLine(m_ExcelFilePath)
                    sw.WriteLine("")
                    sw.WriteLine("*** Only the first two lines of this file are read ***")
                    sw.WriteLine("    Format:")
                    sw.WriteLine("    String - Path and filename of Access file")
                    sw.WriteLine("    String - Path and filename of Excel file")
                End Using
            End If
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, ex.Source)
        End Try
    End Sub

    ''' <summary>
    ''' Read the ini file for settings reuse
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ReadIniFile()

        ' Add info for views

        'Read the ini file if possible if not it will get written later when user sets path
        Try
            If File.Exists(m_IniPath) Then
                Dim input As String = ""
                Using sr As StreamReader = File.OpenText(m_IniPath)
                    If (InlineAssignHelper(input, sr.ReadLine())) Is Nothing Then
                        MsgBox("Unable to read Access file path from file: " & vbLf & m_IniPath, MsgBoxStyle.Information, ProgramName)
                    Else
                        m_AccessFilePath = input

                    End If
                    If (InlineAssignHelper(input, sr.ReadLine())) Is Nothing Then
                        MsgBox("Unable to read Excel file path from file: " & vbLf & m_IniPath, MsgBoxStyle.Information, ProgramName)
                    Else
                        m_ExcelFilePath = input

                    End If
                    sr.Close()
                End Using
            End If
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, ex.Source)
        End Try
    End Sub

    ''' <summary>
    ''' Basic helper
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="target"></param>
    ''' <param name="value"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, ByVal value As T) As T
        target = value
        Return value
    End Function

    Public ReadOnly Property clsSheetsList As Dictionary(Of String, clsSheet)
        Get
            Return m_clsSheets
        End Get
    End Property

    Public ReadOnly Property clsTblksList As Dictionary(Of String, clsTblk)
        Get
            Return m_clsTblks
        End Get
    End Property

    Public ReadOnly Property ApplicationVersion() As String
        Get
            Return "v" & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString
        End Get
    End Property

    Public ReadOnly Property ProgramName() As String
        Get
            Return "SheetManager"
        End Get
    End Property

    Public ReadOnly Property Application As UIApplication
        Get
            Return m_CommandData.Application
        End Get
    End Property

    Public ReadOnly Property Document As Document
        Get
            Return m_Doc
        End Get
    End Property

    Public ReadOnly Property Sheets As Dictionary(Of String, ViewSheet)
        Get
            Return m_Sheets
        End Get
    End Property

    Public ReadOnly Property Views As Dictionary(Of String, Autodesk.Revit.DB.View)
        Get
            Return m_Views
        End Get
    End Property

    Public Property IniPath() As String
        Get
            Return m_IniPath
        End Get
        Set(ByVal value As String)
            m_IniPath = value
            WriteIniFile()
        End Set
    End Property

    Public Property AccessPath() As String
        Get
            Return m_AccessFilePath
        End Get
        Set(ByVal value As String)
            m_AccessFilePath = value
            WriteIniFile()
        End Set
    End Property

    Public Property ExcelPath() As String
        Get
            Return m_ExcelFilePath
        End Get
        Set(ByVal value As String)
            m_ExcelFilePath = value
            WriteIniFile()
        End Set
    End Property
End Class
