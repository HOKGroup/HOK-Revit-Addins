Imports autodesk.Revit.DB
Imports System.Runtime.CompilerServices

Public Module ElementIdExtensionModule

#If RELEASE2024 Then
    <Extension>
    Public Function NewElementId(L As Long) As ElementId
        Return New ElementId(L)
    End Function
#Else
    <Extension>
    Public Function Value(ID As ElementId) As Long
        Return ID.IntegerValue
    End Function
    <Extension>
    Public Function NewElementId(L As Long) As ElementId
        If L > Int32.MaxValue OrElse L < Int32.MinValue Then
            Throw New OverflowException("Value for ElementId out of range.")
        End If
        Return New ElementId(CInt(L))
    End Function
#End If

End Module