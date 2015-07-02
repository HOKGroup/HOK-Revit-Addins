Imports System.Data
Imports System.IO
Imports Microsoft.Office.Interop
Imports System.Windows.Forms
Imports Microsoft.Office.Interop.Access

''' <summary>
''' Class to connect Access and Excel via Interop
''' </summary>
''' <remarks></remarks>
Public Class clsUtilityInterop

    Private m_Settings As clsSettings
    Private m_MissingValue As Object = System.Reflection.Missing.Value
    Private m_DataTable As System.Data.DataTable
    Private m_AccessApp As Access.Application
    Private m_AccessOpenPrevious As Boolean
    Private m_ExcelApp As Excel.Application
    Private m_ExcelWorkbook As Excel.Workbook
    Private m_ExcelOpenPrevious As Boolean

    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="settings"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal settings As clsSettings)
        m_Settings = settings
    End Sub

    ''' <summary>
    ''' Fill from Access Database
    ''' </summary>
    ''' <param name="nameTable">Name of the table</param>
    ''' <returns>True on success</returns>
    ''' <remarks></remarks>
    Public Function FillDataTableFromAccessTable(ByVal nameTable As String) As Boolean
        If FillDataTableFromAccessTable(nameTable, "") Then
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Fill from Access Database
    ''' </summary>
    ''' <param name="nameTable">Name of the table</param>
    ''' <param name="whereClause">Where filter</param>
    ''' <returns>True on Success</returns>
    ''' <remarks></remarks>
    Public Function FillDataTableFromAccessTable(ByVal nameTable As String, ByVal whereClause As String) As Boolean
        Dim queryDef As Dao.QueryDef
        Dim recordset As Dao.Recordset
        Dim row As System.Data.DataRow

        Dim queryString As String
        Dim countColumns As Integer

        If whereClause = "" Then
            queryString = "SELECT * FROM " & nameTable
        Else
            queryString = "SELECT * FROM " & nameTable & " WHERE " & whereClause
        End If
        Try
            queryDef = m_AccessApp.CurrentDb().CreateQueryDef("", queryString)
            recordset = queryDef.OpenRecordset(m_MissingValue, m_MissingValue, m_MissingValue)

            'Create the data tabl as a copy of the original table
            m_DataTable = New System.Data.DataTable()
            countColumns = 0
            For Each field As Dao.Field In m_AccessApp.CurrentDb().TableDefs(nameTable).Fields
                m_DataTable.Columns.Add(field.Name, GetType(String))
                countColumns += 1
            Next

            'Add the data to the table
            While Not recordset.EOF
                row = m_DataTable.NewRow()
                For i As Integer = 0 To countColumns - 1
                    row(i) = recordset.Fields(i).Value.ToString
                Next
                m_DataTable.Rows.Add(row)
                recordset.MoveNext()
            End While
            Return True
        Catch
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Fill from Excel column
    ''' </summary>
    ''' <param name="nameColumn">Name of the column</param>
    ''' <returns>True on success</returns>
    ''' <remarks></remarks>
    Public Function FillDataTableFromExcelSheetNames(ByVal nameColumn As String) As Boolean
        Dim worksheet As Excel.Worksheet
        Dim row As DataRow = Nothing
        Try
            m_DataTable = New System.Data.DataTable()
            m_DataTable.Columns.Add(nameColumn, GetType(String))
            For i As Integer = 1 To m_ExcelWorkbook.Sheets.Count
                'note index starts at 1
                worksheet = DirectCast(m_ExcelWorkbook.Sheets(i), Excel.Worksheet)
                Dim worksheetName As String = worksheet.Name
                If Not worksheetName = "Renumber Sheets" Then
                    row = m_DataTable.NewRow()
                    row(0) = worksheetName
                    m_DataTable.Rows.Add(row)
                End If
            Next
            Return True
        Catch
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Fill from Excel Worksheet
    ''' </summary>
    ''' <param name="nameWorksheet">Worksheet name</param>
    ''' <returns>True on success</returns>
    ''' <remarks></remarks>
    Public Function FillDataTableFromExcelWorksheet(ByVal nameWorksheet As String) As Boolean
        Dim range As Excel.Range
        Dim worksheet As Excel.Worksheet
        Dim row As System.Data.DataRow

        Dim indexColumn As Integer
        Dim indexRow As Integer
        Dim countColumns As Integer = 0

        Try

            'Get the column headings from the first row of the spreadsheet and create the columns
            worksheet = DirectCast(m_ExcelWorkbook.Sheets(nameWorksheet), Excel.Worksheet)
            m_DataTable = New System.Data.DataTable
            indexColumn = 1
            range = DirectCast(worksheet.Cells(1, 1), Excel.Range)
            While range.Value2 IsNot Nothing
                m_DataTable.Columns.Add(range.Value2.ToString, GetType(String))
                countColumns = indexColumn
                indexColumn += 1
                range = DirectCast(worksheet.Cells(1, indexColumn), Excel.Range)
            End While

            'Continue with remaining rows and add the data to the table
            indexRow = 2
            range = DirectCast(worksheet.Cells(indexRow, 1), Excel.Range)
            While range.Value2 IsNot Nothing
                row = m_DataTable.NewRow()
                row(0) = range.Value2.ToString
                For i As Integer = 1 To countColumns - 1
                    range = DirectCast(worksheet.Cells(indexRow, i + 1), Excel.Range)
                    If range.Value2 IsNot Nothing Then
                        row(i) = range.Value2.ToString
                    End If
                Next
                m_DataTable.Rows.Add(row)
                indexRow += 1
                range = DirectCast(worksheet.Cells(indexRow, 1), Excel.Range)
            End While
            Return True
        Catch exception As Exception
            MessageBox.Show("Error at UtilityInterop.FillDataTableFromExcelWorksheet. " & vbLf & "System message:" & vbLf & exception.Message, m_Settings.ProgramName)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Fill Excel Worksheet from Table
    ''' </summary>
    ''' <param name="nameWorksheet">Worksheet name</param>
    ''' <returns>True on success</returns>
    ''' <remarks></remarks>
    Public Function FillExcelWorksheetFromDataTable(ByVal nameWorksheet As String) As Boolean
        ' Localized Variables
        Dim range As Excel.Range
        Dim worksheet As Excel.Worksheet
        Dim indexRow As Integer

        Try
            ' Assuming that data table columns and Excel headings are the same since Excel was read in order to create data table
            worksheet = DirectCast(m_ExcelWorkbook.Sheets(nameWorksheet), Excel.Worksheet)
            indexRow = 1
            For Each dataRow As System.Data.DataRow In m_DataTable.Rows
                For indexColumn As Integer = 0 To m_DataTable.Columns.Count - 1
                    range = DirectCast(worksheet.Cells(indexRow + 1, indexColumn + 1), Excel.Range)
                    If dataRow(indexColumn).ToString IsNot Nothing Then
                        range.Value2 = dataRow(indexColumn).ToString
                    End If
                Next
                indexRow += 1
            Next
            Return True
        Catch
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Start the Access Application
    ''' </summary>
    ''' <returns>True on success</returns>
    ''' <remarks></remarks>
    Public Function StartAccess() As Boolean

        If Not File.Exists(m_Settings.AccessPath) Then
            MessageBox.Show("Unable to locate file: " & Convert.ToString(m_Settings.AccessPath), m_Settings.ProgramName)
            Return False
        End If

        Try
            'Either finds running instance or starts a new one; failure to find file causes exception                
            m_AccessApp = DirectCast(System.Runtime.InteropServices.Marshal.BindToMoniker(m_Settings.AccessPath), Access.Application)

            'This isn't terribly elegant but it is a way of determining if Access was already open
            'Also follows logic of hiding Access if it wasn't already open
            m_AccessOpenPrevious = False
            Try
                m_AccessApp.Visible = False
            Catch
                'It seems to fail if Access was already open which is what we want.
                m_AccessOpenPrevious = True
            End Try
            Return True
        Catch ex As Exception
            MessageBox.Show("Unable to find or start Access Interop session with file: " & Convert.ToString(m_Settings.AccessPath) & ". " & vbLf & vbLf & "System Error Message: " & vbLf & Convert.ToString(ex), m_Settings.ProgramName)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Start the Excel Application
    ''' </summary>
    ''' <returns>True on success</returns>
    ''' <remarks></remarks>
    Public Function StartExcel() As Boolean
        Dim nameFile As String

        If Not File.Exists(m_Settings.ExcelPath) Then
            MessageBox.Show("Unable to locate file: " & Convert.ToString(m_Settings.AccessPath), m_Settings.ProgramName)
            Return False
        End If

        Try
            'Either finds running instance or starts a new one; failure to find file causes exception
            m_ExcelWorkbook = DirectCast(System.Runtime.InteropServices.Marshal.BindToMoniker(m_Settings.ExcelPath), Excel.Workbook)
            m_ExcelApp = m_ExcelWorkbook.Application

            'Test below not very logical but, so far, the only way to determine if Excel was running
            If m_ExcelWorkbook.Application.ActiveWorkbook Is Nothing Then
                m_ExcelOpenPrevious = False
            Else
                m_ExcelOpenPrevious = True
            End If
            nameFile = m_Settings.ExcelPath.Substring(m_Settings.ExcelPath.LastIndexOf("\") + 1)
            m_ExcelWorkbook.Application.Windows(nameFile).Visible = True
            'm_ExcelWorkbook.Application.Visible = true;
            Return True
        Catch ex As Exception
            MessageBox.Show("Unable to find or start Excel Interop session with file: " & _
                            Convert.ToString(m_Settings.ExcelPath) & ". " & vbLf & vbLf & _
                            "System Error Message: " & vbLf & Convert.ToString(ex), m_Settings.ProgramName)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Terminate the Access Application
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ShutDownAccess()
        Try
            If Not m_AccessOpenPrevious Then
                If m_AccessApp IsNot Nothing Then
                    m_AccessApp.CloseCurrentDatabase()
                    m_AccessApp.Quit(Access.AcQuitOption.acQuitSaveAll)
                    If m_DataTable IsNot Nothing Then
                        'System.Runtime.InteropServices.Marshal.ReleaseComObject(m_DataTable);
                        m_DataTable = Nothing
                    End If
                    If m_AccessApp IsNot Nothing Then
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(m_AccessApp)
                        m_AccessApp = Nothing
                    End If
                    GC.Collect()
                End If
            End If
            'Don't do anything; we're just trying to clean up
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Terminate the Excel Application
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ShutDownExcel()
        Try
            If Not m_ExcelOpenPrevious Then
                If m_ExcelApp IsNot Nothing Then
                    If m_ExcelWorkbook IsNot Nothing Then
                        m_ExcelWorkbook.Save()
                    End If
                    If m_ExcelApp IsNot Nothing Then
                        m_ExcelApp.Quit()
                    End If
                End If
            End If
            m_ExcelWorkbook = Nothing
            m_ExcelApp = Nothing
            GC.Collect()
            'Don't do anything; we're just trying to clean up
        Catch
        End Try
    End Sub

    Public ReadOnly Property AccessApplication() As Access.Application
        Get
            Return m_AccessApp
        End Get
    End Property

    Public ReadOnly Property DataTable() As System.Data.DataTable
        Get
            Return m_DataTable
        End Get
    End Property

    Public ReadOnly Property AccessWasOpen() As Boolean
        Get
            Return m_AccessOpenPrevious
        End Get
    End Property

    Public ReadOnly Property ExcelApplication() As Excel.Application
        Get
            Return m_ExcelApp
        End Get
    End Property

    Public ReadOnly Property Workbook() As Excel.Workbook
        Get
            Return m_ExcelWorkbook
        End Get
    End Property

    Public ReadOnly Property ExcelWasOpen() As Boolean
        Get
            Return m_ExcelOpenPrevious
        End Get
    End Property

End Class
