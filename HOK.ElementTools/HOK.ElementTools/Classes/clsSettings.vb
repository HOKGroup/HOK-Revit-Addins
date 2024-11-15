Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI

'Imports System.Windows.Forms

Public Class clsSettings

    Private Const cProgramName As String = "ElementTools"
    Private Const cIniFileName As String = "ElementTools.ini"
    Private Const cSpacer As String = "                                                                                       "

    'Local usage
    Private mCommandData As ExternalCommandData
    Private mApplication As UIApplication
    Private mDocument As Document
    Private mActiveView As Autodesk.Revit.DB.View
    Private mActiveViewPlan As ViewPlan
    Private mCurrentLevel As Level
    Private mLevelNameParam As Parameter
    Private mCurrentPhase As Phase
    Private mCurrentViewIsPlan As Boolean
    Private mProjectFolderPath As String
    Private mUIdoc As UIDocument

    Private mUtilityIni As clsUtilityIni
    Private mListSettings As New List(Of String)
    Private mIniPath As String


    'Managed with INI file (note that all are strings with no validation)

    'Placing areas - List Selection Options
    Private mAreasPlaceParamList1 As String
    'paramater to list first in selection list
    Private mAreasPlaceParamList2 As String
    'paramater to list second in selection list
    Private mAreasPlaceListPadYes1 As String
    'should first entry in selection list be padded with zeros (true/false)
    Private mAreasPlaceListPadYes2 As String
    'should second entry in selection list be padded with zeros (true/false)
    Private mAreasPlaceListPad1 As String
    'minimum length of first entry in selection list as padded with zeros
    Private mAreasPlaceListPad2 As String
    'minimum length of first entry in selection list as padded with zeros
    Private mAreasPlaceListReverse As String
    'reverse order of list
    'Placing areas - Processing Options 
    Private mAreasPlaceStartX As String
    'start location for first area X value. 
    Private mAreasPlaceStartY As String
    'start location for first area Y value. 
    Private mAreasPlaceSpace As String
    'space between areas
    Private mAreasPlaceNoRow As String
    'number in row before it wraps
    Private mAreasPlaceParamReqArea As String
    'parameter used to specify required area
    Private mAreasPlaceReqDefault As String
    'default to use if parameter for required area is missing
    'Placing rooms - List Selection Options 
    Private mRoomsPlaceParamList1 As String
    'paramater to list first in selection list
    Private mRoomsPlaceParamList2 As String
    'paramater to list second in selection list
    Private mRoomsPlaceListPadYes1 As String
    'should first entry in selection list be padded with zeros (true/false)
    Private mRoomsPlaceListPadYes2 As String
    'should second entry in selection list be padded with zeros (true/false)
    Private mRoomsPlaceListPad1 As String
    'minimum length of first entry in selection list as padded with zeros
    Private mRoomsPlaceListPad2 As String
    'minimum length of first entry in selection list as padded with zeros
    Private mRoomsPlaceListReverse As String
    'reverse order of list
    'Placing rooms - Processing Options 
    Private mRoomsPlaceStartX As String
    'start location for first room X value. 
    Private mRoomsPlaceStartY As String
    'start location for first room Y value. 
    Private mRoomsPlaceSpace As String
    'space between rooms
    Private mRoomsPlaceNoRow As String
    'number in row before it wraps
    Private mRoomsPlaceParamReqArea As String
    'parameter used to specify required area
    Private mRoomsPlaceReqDefault As String
    'default to use if parameter for required area is missing
    'Creating rooms from areas - List Selection Options
    Private mRoomsFromAreasIncludePlaced As String
    'include only placed areas
    Private mRoomsFromAreasIncludeNotPlaced As String
    'include only not-placed areas
    Private mRoomsFromAreasIncludeBoth As String
    'include both placed and not-place
    Private mRoomsFromAreasParamList1 As String
    'paramater to list first in selection list
    Private mRoomsFromAreasParamList2 As String
    'paramater to list second in selection list
    Private mRoomsFromAreasListPadYes1 As String
    'should first entry in selection list be padded with zeros (true/false)
    Private mRoomsFromAreasListPadYes2 As String
    'should second entry in selection list be padded with zeros (true/false)
    Private mRoomsFromAreasListPad1 As String
    'minimum length of first entry in selection list as padded with zeros
    Private mRoomsFromAreasListPad2 As String
    'minimum length of first entry in selection list as padded with zeros
    Private mRoomsFromAreasListReverse As String
    'reverse order of list
    'Creating rooms from areas - Processing Options
    'Creating Views from Rooms - List Selection Options
    Private mViewsFromRoomsGrouping As String
    'either "single" or "multiple"
    Private mViewsFromAreasGrouping As String
    'either "single" or "multiple"
    Private mViewsFromRoomsParamList1 As String
    'paramater to list first in selection list
    Private mViewsFromAreasParamList1 As String
    'paramater to list first in selection list
    Private mViewsFromRoomsParamList2 As String
    'paramater to list second in selection list       
    Private mViewsFromAreasParamList2 As String
    'paramater to list second in selection list  
    Private mViewsFromRoomsListPadYes1 As String
    'should first entry in selection list be padded with zeros (true/false)
    Private mViewsFromAreasListPadYes1 As String
    'should first entry in selection list be padded with zeros (true/false)
    Private mViewsFromRoomsListPadYes2 As String
    'should second entry in selection list be padded with zeros (true/false)
    Private mViewsFromAreasListPadYes2 As String
    'should second entry in selection list be padded with zeros (true/false)
    Private mViewsFromRoomsListPad1 As String
    'minimum length of first entry in selection list as padded with zeros
    Private mViewsFromAreasListPad1 As String
    'minimum length of first entry in selection list as padded with zeros
    Private mViewsFromRoomsListPad2 As String
    'minimum length of first entry in selection list as padded with zeros
    Private mViewsFromAreasListPad2 As String
    'minimum length of first entry in selection list as padded with zeros
    Private mViewsFromRoomsParamGroupBy As String
    'paramater to use in group selection mode
    Private mViewsFromAreasParamGroupBy As String
    'paramater to use in group selection mode
    Private mViewsFromRoomsListExisting As String
    'list rooms that already have a view
    Private mViewsFromAreasListExisting As String
    'list rooms that already have a view
    Private mViewsFromRoomsListReverse As String
    'reverse order of list
    'Creating Views from Rooms - Processing Options
    Private mViewsFromAreasListReverse As String
    'reverse order of list
    'Creating Views from Rooms - Processing Options
    Private mViewsFromRoomsParmViewName As String
    'parameter used to define view name (typically room number)
    Private mViewsFromAreasParmViewName As String
    'parameter used to define view name (typically room number)
    Private mViewsFromRoomsParmRoomName As String
    'parameter used to capture for room name (typically room name)
    Private mViewsFromAreasParmAreaName As String
    'parameter used to capture for room name (typically room name)
    Private mViewsFromRoomsScale As String
    'second part of ratio 1:x
    Private mViewsFromAreasScale As String
    'second part of ratio 1:x
    Private mViewsFromRoomsPrefixViewTarget As String
    'prefix to add to target view name
    Private mViewsFromAreasPrefixViewTarget As String
    'prefix to add to target view name
    Private mViewsFromRoomsViewType As String
    'either "2d" or "3dBox" or "3dCrop" or "3dBoxCrop"
    Private mViewsFromAreasViewType As String
    'either "2d" or "3dBox" or "3dCrop" or "3dBoxCrop"
    Private mViewsFromRoomsVectorX As String
    'view direction X  (when view is created)
    Private mViewsFromAreasVectorX As String
    'view direction X  (when view is created)
    Private mViewsFromRoomsVectorY As String
    'view direction Y  (when view is created)
    Private mViewsFromAreasVectorY As String
    'view direction Y  (when view is created)
    Private mViewsFromRoomsVectorZ As String
    'view direction Z  (when view is created)
    Private mViewsFromAreasVectorZ As String
    'view direction Z  (when view is created)
    Private mViewsFromRoomsReplaceExisting As String
    'delete and rebuild rooms that already have a view
    Private mViewsFromAreasReplaceExisting As String
    'delete and rebuild rooms that already have a view
    Private mViewsFromRoomsSizeBoxType As String
    'either "dynamic" or "fixed"
    Private mViewsFromAreasSizeBoxType As String
    'either "dynamic" or "fixed"
    Private mViewsFromRoomsBoxSpace As String
    'distance from outer edge of room to section box
    Private mViewsFromAreasBoxSpace As String
    'distance from outer edge of room to section box
    Private mViewsFromRoomsBoxFixedX As String
    'fixed X  (room is centered in section box)
    Private mViewsFromAreasBoxFixedX As String
    'fixed X  (room is centered in section box)
    Private mViewsFromRoomsBoxFixedY As String
    'fixed Y  (room is centered in section box)
    Private mViewsFromAreasBoxFixedY As String
    'fixed Y  (room is centered in section box)
    Private mViewsFromRoomsBoxFixedZ As String
    'fixed Y  (room is centered in section box)
    Private mViewsFromAreasBoxFixedZ As String
    'fixed Y  (room is centered in section box)
    Private mViewsFromRoomsBoxShow As String
    'section box visible, true or false
    Private mViewsFromAreasBoxShow As String
    'section box visible, true or false
    Private mViewsFromRoomsSizeCropType As String
    'either "dynamic" or "fixed"
    Private mViewsFromAreasSizeCropType As String
    'either "dynamic" or "fixed"
    Private mViewsFromRoomsCropSpace As String
    'distance from outer edge of room to crop box
    Private mViewsFromAreasCropSpace As String
    'distance from outer edge of room to crop box
    Private mViewsFromRoomsCropFixedX As String
    'fixed X  (room is centered in crop box)
    Private mViewsFromAreasCropFixedX As String
    'fixed X  (room is centered in crop box)
    Private mViewsFromRoomsCropFixedY As String
    'fixed Y  (room is centered in crop box)\
    Private mViewsFromAreasCropFixedY As String
    'fixed Y  (room is centered in crop box)
    Private mViewsFromRoomsCropShow As String
    'crop box visible, true or false
    'Creating Images from Views - List Selection Options
    Private mViewsFromAreasCropShow As String
    'crop box visible, true or false
    'Creating Images from Views - List Selection Options
    Private mImagesFromViewsIncludeExisting As String
    'include processing of views that already have an image
    Private mImagesFromViewsRestrictPrefix As String
    'only show views that start with prefix
    Private mImagesFromViewsRestrictPrefixValue As String
    'string to test for prefix
    Private mImagesFromViewsListReverse As String
    'reverse order of list
    'Creating Images from Views - Processing Options
    Private mImagesFromViewsFolderPath As String
    'folder where images will be placed
    'Adding Tags to Existing Views - List Selection Options
    Private mTagViewsIncludeExisting As String
    'include processing of views that already have an image
    Private mTagViewsRestrictPrefix As String
    'only show views that start with prefix
    Private mTagViewsRestrictPrefixValue As String
    'string to test for prefix
    Private mTagViewsRestrictPrefixValueArea As String
    'string to test for prefix
    Private mTagViewsListReverse As String
    'reverse order of list
    'Creating Tagged Views from Views - Processing Options
    Private mTagViewsRoomTag As String
    'room tag to use
    Private mTagViewsAreaTag As String
    'room tag to use
    Private mTagViewsParmViewName As String
    'parameter used to define view name
    Private mTagViewsPrefixViewSource As String
    'prefix to strip from source view name
    Private mTagViewsPrefixViewSourceArea As String
    'prefix to strip from source view name
    Private mTagViewsStripSuffix As String
    'true or false; to remove "-2D", "-3DB", "-3DC", "-3DBC" values
    'Creating Sheets from Views - List Selection Options
    Private mSheetsFromViewsIncludeExisting As String
    'include processing of views that already have an image
    Private mSheetsFromViewsRestrictPrefix As String
    'only show views that start with prefix
    Private mSheetsFromViewsRestrictPrefixValue As String
    Private mSheetsFromViewsListReverse As String
    Private mSheetsFromViewsTitleblock As String

    Public Sub New(ByRef commandData As ExternalCommandData, ByRef message As String, ByRef elementSet As ElementSet)
        'Note:  Not currently using message or elementset.
        mCommandData = commandData
        RefreshRevitValues()
        'Need to do this before we use mDocument in next line
        mUtilityIni = New clsUtilityIni(mDocument, cIniFileName)
        'Need to do this so we can use mUtilityIni in next two lines
        mProjectFolderPath = CalculateProjectFolderPath()
        If mProjectFolderPath = "" Then
            mIniPath = ""
        Else
            mIniPath = mProjectFolderPath & "\" & cIniFileName
        End If

        mUIdoc = commandData.Application.ActiveUIDocument

        'Read ini or set defaults as necessary.
        RefreshIniValues()
    End Sub

#Region "********************************************** Public Properties ****************************************************"

    Public ReadOnly Property UIdoc As UIDocument
        Get
            Return mUIdoc
        End Get
    End Property

    Public ReadOnly Property ProgramName As String
        Get
            Return cProgramName
        End Get
    End Property
    Public ReadOnly Property Spacer As String
        Get
            Return cSpacer
        End Get
    End Property

    Public ReadOnly Property Application As UIApplication
        Get
            Return mApplication
        End Get
    End Property
    Public ReadOnly Property Document As Document
        Get
            Return mDocument
        End Get
    End Property
    Public ReadOnly Property ActiveView As View
        Get
            Return mActiveView
        End Get
    End Property
    Public ReadOnly Property ActiveViewPlan As ViewPlan
        Get
            Return mActiveViewPlan
        End Get
    End Property
    Public ReadOnly Property CurrentLevel As Level
        Get
            Return mCurrentLevel
        End Get
    End Property
    Public Property LevelParameter As Parameter
        Get
            Return mLevelNameParam
        End Get
        Set(ByVal value As Parameter)
            mLevelNameParam = value
        End Set
    End Property
    Public ReadOnly Property CurrentPhase As Phase
        Get
            Return mCurrentPhase
        End Get
    End Property
    Public ReadOnly Property CurrentViewIsPlan As Boolean
        Get
            Return mCurrentViewIsPlan
        End Get
    End Property
    Public ReadOnly Property ProjectFolderPath As String
        Get
            Return mProjectFolderPath
        End Get
    End Property

    Public Property AreasPlaceParamList1 As String
        Get
            Return mAreasPlaceParamList1
        End Get
        Set(ByVal value As String)
            mAreasPlaceParamList1 = value
        End Set
    End Property
    Public Property AreasPlaceParamList2 As String
        Get
            Return mAreasPlaceParamList2
        End Get
        Set(ByVal value As String)
            mAreasPlaceParamList2 = value
        End Set
    End Property
    Public Property AreasPlaceListPadYes1() As String
        Get
            Return mAreasPlaceListPadYes1
        End Get
        Set(ByVal value As String)
            mAreasPlaceListPadYes1 = value
        End Set
    End Property
    Public Property AreasPlaceListPadYes2 As String
        Get
            Return mAreasPlaceListPadYes2
        End Get
        Set(ByVal value As String)
            mAreasPlaceListPadYes2 = value
        End Set
    End Property
    Public Property AreasPlaceListPad1 As String
        Get
            Return mAreasPlaceListPad1
        End Get
        Set(ByVal value As String)
            mAreasPlaceListPad1 = value
        End Set
    End Property
    Public Property AreasPlaceListPad2() As String
        Get
            Return mAreasPlaceListPad2
        End Get
        Set(ByVal value As String)
            mAreasPlaceListPad2 = value
        End Set
    End Property
    Public Property AreasPlaceListReverse() As String
        Get
            Return mAreasPlaceListReverse
        End Get
        Set(ByVal value As String)
            mAreasPlaceListReverse = value
        End Set
    End Property

    Public Property AreasPlaceStartX() As String
        Get
            Return mAreasPlaceStartX
        End Get
        Set(ByVal value As String)
            mAreasPlaceStartX = value
        End Set
    End Property
    Public Property AreasPlaceStartY() As String
        Get
            Return mAreasPlaceStartY
        End Get
        Set(ByVal value As String)
            mAreasPlaceStartY = value
        End Set
    End Property
    Public Property AreasPlaceSpace() As String
        Get
            Return mAreasPlaceSpace
        End Get
        Set(ByVal value As String)
            mAreasPlaceSpace = value
        End Set
    End Property
    Public Property AreasPlaceNoRow() As String
        Get
            Return mAreasPlaceNoRow
        End Get
        Set(ByVal value As String)
            mAreasPlaceNoRow = value
        End Set
    End Property
    Public Property AreasPlaceParamReqArea() As String
        Get
            Return mAreasPlaceParamReqArea
        End Get
        Set(ByVal value As String)
            mAreasPlaceParamReqArea = value
        End Set
    End Property
    Public Property AreasPlaceReqDefault() As String
        Get
            Return mAreasPlaceReqDefault
        End Get
        Set(ByVal value As String)
            mAreasPlaceReqDefault = value
        End Set
    End Property

    Public Property RoomsPlaceParamList1() As String
        Get
            Return mRoomsPlaceParamList1
        End Get
        Set(ByVal value As String)
            mRoomsPlaceParamList1 = value
        End Set
    End Property
    Public Property RoomsPlaceParamList2() As String
        Get
            Return mRoomsPlaceParamList2
        End Get
        Set(ByVal value As String)
            mRoomsPlaceParamList2 = value
        End Set
    End Property
    Public Property RoomsPlaceListPadYes1() As String
        Get
            Return mRoomsPlaceListPadYes1
        End Get
        Set(ByVal value As String)
            mRoomsPlaceListPadYes1 = value
        End Set
    End Property
    Public Property RoomsPlaceListPadYes2() As String
        Get
            Return mRoomsPlaceListPadYes2
        End Get
        Set(ByVal value As String)
            mRoomsPlaceListPadYes2 = value
        End Set
    End Property
    Public Property RoomsPlaceListPad1() As String
        Get
            Return mRoomsPlaceListPad1
        End Get
        Set(ByVal value As String)
            mRoomsPlaceListPad1 = value
        End Set
    End Property
    Public Property RoomsPlaceListPad2() As String
        Get
            Return mRoomsPlaceListPad2
        End Get
        Set(ByVal value As String)
            mRoomsPlaceListPad2 = value
        End Set
    End Property
    Public Property RoomsPlaceListReverse() As String
        Get
            Return mRoomsPlaceListReverse
        End Get
        Set(ByVal value As String)
            mRoomsPlaceListReverse = value
        End Set
    End Property

    Public Property RoomsPlaceStartX() As String
        Get
            Return mRoomsPlaceStartX
        End Get
        Set(ByVal value As String)
            mRoomsPlaceStartX = value
        End Set
    End Property
    Public Property RoomsPlaceStartY() As String
        Get
            Return mRoomsPlaceStartY
        End Get
        Set(ByVal value As String)
            mRoomsPlaceStartY = value
        End Set
    End Property
    Public Property RoomsPlaceSpace() As String
        Get
            Return mRoomsPlaceSpace
        End Get
        Set(ByVal value As String)
            mRoomsPlaceSpace = value
        End Set
    End Property
    Public Property RoomsPlaceNoRow() As String
        Get
            Return mRoomsPlaceNoRow
        End Get
        Set(ByVal value As String)
            mRoomsPlaceNoRow = value
        End Set
    End Property
    Public Property RoomsPlaceParamReqArea() As String
        Get
            Return mRoomsPlaceParamReqArea
        End Get
        Set(ByVal value As String)
            mRoomsPlaceParamReqArea = value
        End Set
    End Property
    Public Property RoomsPlaceReqDefault() As String
        Get
            Return mRoomsPlaceReqDefault
        End Get
        Set(ByVal value As String)
            mRoomsPlaceReqDefault = value
        End Set
    End Property

    Public Property RoomsFromAreasIncludePlaced() As String
        Get
            Return mRoomsFromAreasIncludePlaced
        End Get
        Set(ByVal value As String)
            mRoomsFromAreasIncludePlaced = value
        End Set
    End Property
    Public Property RoomsFromAreasIncludeNotPlaced() As String
        Get
            Return mRoomsFromAreasIncludeNotPlaced
        End Get
        Set(ByVal value As String)
            mRoomsFromAreasIncludeNotPlaced = value
        End Set
    End Property
    Public Property RoomsFromAreasIncludeBoth() As String
        Get
            Return mRoomsFromAreasIncludeBoth
        End Get
        Set(ByVal value As String)
            mRoomsFromAreasIncludeBoth = value
        End Set
    End Property
    Public Property RoomsFromAreasParamList1() As String
        Get
            Return mRoomsFromAreasParamList1
        End Get
        Set(ByVal value As String)
            mRoomsFromAreasParamList1 = value
        End Set
    End Property
    Public Property RoomsFromAreasParamList2() As String
        Get
            Return mRoomsFromAreasParamList2
        End Get
        Set(ByVal value As String)
            mRoomsFromAreasParamList2 = value
        End Set
    End Property
    Public Property RoomsFromAreasListPadYes1() As String
        Get
            Return mRoomsFromAreasListPadYes1
        End Get
        Set(ByVal value As String)
            mRoomsFromAreasListPadYes1 = value
        End Set
    End Property
    Public Property RoomsFromAreasListPadYes2() As String
        Get
            Return mRoomsFromAreasListPadYes2
        End Get
        Set(ByVal value As String)
            mRoomsFromAreasListPadYes2 = value
        End Set
    End Property
    Public Property RoomsFromAreasListPad1() As String
        Get
            Return mRoomsFromAreasListPad1
        End Get
        Set(ByVal value As String)
            mRoomsFromAreasListPad1 = value
        End Set
    End Property
    Public Property RoomsFromAreasListPad2() As String
        Get
            Return mRoomsFromAreasListPad2
        End Get
        Set(ByVal value As String)
            mRoomsFromAreasListPad2 = value
        End Set
    End Property
    Public Property RoomsFromAreasListReverse() As String
        Get
            Return mRoomsFromAreasListReverse
        End Get
        Set(ByVal value As String)
            mRoomsFromAreasListReverse = value
        End Set
    End Property

    Public Property ViewsFromRoomsGrouping() As String
        Get
            Return mViewsFromRoomsGrouping
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsGrouping = value
        End Set
    End Property

    Public Property ViewsFromAreasGrouping() As String
        Get
            Return mViewsFromAreasGrouping
        End Get
        Set(ByVal value As String)
            mViewsFromAreasGrouping = value
        End Set
    End Property

    Public Property ViewsFromRoomsParamList1() As String
        Get
            Return mViewsFromRoomsParamList1
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsParamList1 = value
        End Set
    End Property

    Public Property ViewsFromAreasParamList1() As String
        Get
            Return mViewsFromAreasParamList1
        End Get
        Set(ByVal value As String)
            mViewsFromAreasParamList1 = value
        End Set
    End Property

    Public Property ViewsFromRoomsParamList2() As String
        Get
            Return mViewsFromRoomsParamList2
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsParamList2 = value
        End Set
    End Property
    Public Property ViewsFromAreasParamList2() As String
        Get
            Return mViewsFromAreasParamList2
        End Get
        Set(ByVal value As String)
            mViewsFromAreasParamList2 = value
        End Set
    End Property
    Public Property ViewsFromRoomsListPadYes1() As String
        Get
            Return mViewsFromRoomsListPadYes1
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsListPadYes1 = value
        End Set
    End Property
    Public Property ViewsFromAreasListPadYes1() As String
        Get
            Return mViewsFromAreasListPadYes1
        End Get
        Set(ByVal value As String)
            mViewsFromAreasListPadYes1 = value
        End Set
    End Property
    Public Property ViewsFromRoomsListPadYes2() As String
        Get
            Return mViewsFromRoomsListPadYes2
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsListPadYes2 = value
        End Set
    End Property
    Public Property ViewsFromAreasListPadYes2() As String
        Get
            Return mViewsFromAreasListPadYes2
        End Get
        Set(ByVal value As String)
            mViewsFromAreasListPadYes2 = value
        End Set
    End Property
    Public Property ViewsFromRoomsListPad1() As String
        Get
            Return mViewsFromRoomsListPad1
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsListPad1 = value
        End Set
    End Property
    Public Property ViewsFromAreasListPad1() As String
        Get
            Return mViewsFromAreasListPad1
        End Get
        Set(ByVal value As String)
            mViewsFromAreasListPad1 = value
        End Set
    End Property
    Public Property ViewsFromRoomsListPad2() As String
        Get
            Return mViewsFromRoomsListPad2
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsListPad2 = value
        End Set
    End Property
    Public Property ViewsFromAreasListPad2() As String
        Get
            Return mViewsFromAreasListPad2
        End Get
        Set(ByVal value As String)
            mViewsFromAreasListPad2 = value
        End Set
    End Property
    Public Property ViewsFromRoomsParamGroupBy() As String
        Get
            Return mViewsFromRoomsParamGroupBy
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsParamGroupBy = value
        End Set
    End Property
    Public Property ViewsFromAreasParamGroupBy() As String
        Get
            Return mViewsFromAreasParamGroupBy
        End Get
        Set(ByVal value As String)
            mViewsFromAreasParamGroupBy = value
        End Set
    End Property
    Public Property ViewsFromRoomsListExisting() As String
        Get
            Return mViewsFromRoomsListExisting
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsListExisting = value
        End Set
    End Property
    Public Property ViewsFromAreasListExisting() As String
        Get
            Return mViewsFromAreasListExisting
        End Get
        Set(ByVal value As String)
            mViewsFromAreasListExisting = value
        End Set
    End Property
    Public Property ViewsFromRoomsListReverse() As String
        Get
            Return mViewsFromRoomsListReverse
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsListReverse = value
        End Set
    End Property
    Public Property ViewsFromAreasListReverse() As String
        Get
            Return mViewsFromAreasListReverse
        End Get
        Set(ByVal value As String)
            mViewsFromAreasListReverse = value
        End Set
    End Property

    Public Property ViewsFromRoomsParmViewName() As String
        Get
            Return mViewsFromRoomsParmViewName
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsParmViewName = value
        End Set
    End Property
    Public Property ViewsFromAreasParmViewName() As String
        Get
            Return mViewsFromAreasParmViewName
        End Get
        Set(ByVal value As String)
            mViewsFromAreasParmViewName = value
        End Set
    End Property
    Public Property ViewsFromRoomsParmRoomName() As String
        Get
            Return mViewsFromRoomsParmRoomName
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsParmRoomName = value
        End Set
    End Property
    Public Property ViewsFromAreasParmAreaName() As String
        Get
            Return mViewsFromAreasParmAreaName
        End Get
        Set(ByVal value As String)
            mViewsFromAreasParmAreaName = value
        End Set
    End Property
    Public Property ViewsFromRoomsScale() As String
        Get
            Return mViewsFromRoomsScale
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsScale = value
        End Set
    End Property
    Public Property ViewsFromAreasScale() As String
        Get
            Return mViewsFromAreasScale
        End Get
        Set(ByVal value As String)
            mViewsFromAreasScale = value
        End Set
    End Property
    Public Property ViewsFromRoomsPrefixViewTarget() As String
        Get
            Return mViewsFromRoomsPrefixViewTarget
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsPrefixViewTarget = value
        End Set
    End Property
    Public Property ViewsFromAreasPrefixViewTarget() As String
        Get
            Return mViewsFromAreasPrefixViewTarget
        End Get
        Set(ByVal value As String)
            mViewsFromAreasPrefixViewTarget = value
        End Set
    End Property
    Public Property ViewsFromRoomsViewType() As String
        Get
            Return mViewsFromRoomsViewType
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsViewType = value
        End Set
    End Property
    Public Property ViewsFromAreasViewType() As String
        Get
            Return mViewsFromAreasViewType
        End Get
        Set(ByVal value As String)
            mViewsFromAreasViewType = value
        End Set
    End Property
    Public Property ViewsFromRoomsVectorX() As String
        Get
            Return mViewsFromRoomsVectorX
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsVectorX = value
        End Set
    End Property
    Public Property ViewsFromAreasVectorX() As String
        Get
            Return mViewsFromAreasVectorX
        End Get
        Set(ByVal value As String)
            mViewsFromAreasVectorX = value
        End Set
    End Property
    Public Property ViewsFromRoomsVectorY() As String
        Get
            Return mViewsFromRoomsVectorY
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsVectorY = value
        End Set
    End Property
    Public Property ViewsFromAreasVectorY() As String
        Get
            Return mViewsFromAreasVectorY
        End Get
        Set(ByVal value As String)
            mViewsFromAreasVectorY = value
        End Set
    End Property
    Public Property ViewsFromRoomsVectorZ() As String
        Get
            Return mViewsFromRoomsVectorZ
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsVectorZ = value
        End Set
    End Property
    Public Property ViewsFromAreasVectorZ() As String
        Get
            Return mViewsFromAreasVectorZ
        End Get
        Set(ByVal value As String)
            mViewsFromAreasVectorZ = value
        End Set
    End Property
    Public Property ViewsFromRoomsReplaceExisting() As String
        Get
            Return mViewsFromRoomsReplaceExisting
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsReplaceExisting = value
        End Set
    End Property
    Public Property ViewsFromAreasReplaceExisting() As String
        Get
            Return mViewsFromAreasReplaceExisting
        End Get
        Set(ByVal value As String)
            mViewsFromAreasReplaceExisting = value
        End Set
    End Property

    Public Property ViewsFromRoomsSizeBoxType() As String
        Get
            Return mViewsFromRoomsSizeBoxType
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsSizeBoxType = value
        End Set
    End Property
    Public Property ViewsFromAreasSizeBoxType() As String
        Get
            Return mViewsFromAreasSizeBoxType
        End Get
        Set(ByVal value As String)
            mViewsFromAreasSizeBoxType = value
        End Set
    End Property
    Public Property ViewsFromRoomsBoxSpace() As String
        Get
            Return mViewsFromRoomsBoxSpace
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsBoxSpace = value
        End Set
    End Property
    Public Property ViewsFromAreasBoxSpace() As String
        Get
            Return mViewsFromAreasBoxSpace
        End Get
        Set(ByVal value As String)
            mViewsFromAreasBoxSpace = value
        End Set
    End Property
    Public Property ViewsFromRoomsBoxFixedX() As String
        Get
            Return mViewsFromRoomsBoxFixedX
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsBoxFixedX = value
        End Set
    End Property
    Public Property ViewsFromAreasBoxFixedX() As String
        Get
            Return mViewsFromAreasBoxFixedX
        End Get
        Set(ByVal value As String)
            mViewsFromAreasBoxFixedX = value
        End Set
    End Property
    Public Property ViewsFromRoomsBoxFixedY() As String
        Get
            Return mViewsFromRoomsBoxFixedY
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsBoxFixedY = value
        End Set
    End Property
    Public Property ViewsFromAreasBoxFixedY() As String
        Get
            Return mViewsFromAreasBoxFixedY
        End Get
        Set(ByVal value As String)
            mViewsFromAreasBoxFixedY = value
        End Set
    End Property
    Public Property ViewsFromRoomsBoxFixedZ() As String
        Get
            Return mViewsFromRoomsBoxFixedZ
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsBoxFixedZ = value
        End Set
    End Property
    Public Property ViewsFromAreasBoxFixedZ() As String
        Get
            Return mViewsFromAreasBoxFixedZ
        End Get
        Set(ByVal value As String)
            mViewsFromAreasBoxFixedZ = value
        End Set
    End Property
    Public Property ViewsFromRoomsBoxShow() As String
        Get
            Return mViewsFromRoomsBoxShow
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsBoxShow = value
        End Set
    End Property
    Public Property ViewsFromAreasBoxShow() As String
        Get
            Return mViewsFromAreasBoxShow
        End Get
        Set(ByVal value As String)
            mViewsFromAreasBoxShow = value
        End Set
    End Property
    Public Property ViewsFromRoomsSizeCropType() As String
        Get
            Return mViewsFromRoomsSizeCropType
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsSizeCropType = value
        End Set
    End Property
    Public Property ViewsFromAreasSizeCropType() As String
        Get
            Return mViewsFromAreasSizeCropType
        End Get
        Set(ByVal value As String)
            mViewsFromAreasSizeCropType = value
        End Set
    End Property
    Public Property ViewsFromRoomsCropSpace() As String
        Get
            Return mViewsFromRoomsCropSpace
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsCropSpace = value
        End Set
    End Property
    Public Property ViewsFromAreasCropSpace() As String
        Get
            Return mViewsFromAreasCropSpace
        End Get
        Set(ByVal value As String)
            mViewsFromAreasCropSpace = value
        End Set
    End Property
    Public Property ViewsFromRoomsCropFixedX() As String
        Get
            Return mViewsFromRoomsCropFixedX
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsCropFixedX = value
        End Set
    End Property
    Public Property ViewsFromAreasCropFixedX() As String
        Get
            Return mViewsFromAreasCropFixedX
        End Get
        Set(ByVal value As String)
            mViewsFromAreasCropFixedX = value
        End Set
    End Property
    Public Property ViewsFromRoomsCropFixedY() As String
        Get
            Return mViewsFromRoomsCropFixedY
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsCropFixedY = value
        End Set
    End Property
    Public Property ViewsFromAreasCropFixedY() As String
        Get
            Return mViewsFromAreasCropFixedY
        End Get
        Set(ByVal value As String)
            mViewsFromAreasCropFixedY = value
        End Set
    End Property
    Public Property ViewsFromRoomsCropShow() As String
        Get
            Return mViewsFromRoomsCropShow
        End Get
        Set(ByVal value As String)
            mViewsFromRoomsCropShow = value
        End Set
    End Property
    Public Property ViewsFromAreasCropShow() As String
        Get
            Return mViewsFromAreasCropShow
        End Get
        Set(ByVal value As String)
            mViewsFromAreasCropShow = value
        End Set
    End Property

    Public Property ImagesFromViewsIncludeExisting() As String
        Get
            Return mImagesFromViewsIncludeExisting
        End Get
        Set(ByVal value As String)
            mImagesFromViewsIncludeExisting = value
        End Set
    End Property
    Public Property ImagesFromViewsRestrictPrefix() As String
        Get
            Return mImagesFromViewsRestrictPrefix
        End Get
        Set(ByVal value As String)
            mImagesFromViewsRestrictPrefix = value
        End Set
    End Property
    Public Property ImagesFromViewsRestrictPrefixValue() As String
        Get
            Return mImagesFromViewsRestrictPrefixValue
        End Get
        Set(ByVal value As String)
            mImagesFromViewsRestrictPrefixValue = value
        End Set
    End Property
    Public Property ImagesFromViewsListReverse() As String
        Get
            Return mImagesFromViewsListReverse
        End Get
        Set(ByVal value As String)
            mImagesFromViewsListReverse = value
        End Set
    End Property

    Public Property ImagesFromViewsFolderPath() As String
        Get
            Return mImagesFromViewsFolderPath
        End Get
        Set(ByVal value As String)
            mImagesFromViewsFolderPath = value
        End Set
    End Property

    Public Property TagViewsIncludeExisting() As String
        Get
            Return mTagViewsIncludeExisting
        End Get
        Set(ByVal value As String)
            mTagViewsIncludeExisting = value
        End Set
    End Property
    Public Property TagViewsRestrictPrefix() As String
        Get
            Return mTagViewsRestrictPrefix
        End Get
        Set(ByVal value As String)
            mTagViewsRestrictPrefix = value
        End Set
    End Property
    Public Property TagViewsRestrictPrefixValue() As String
        Get
            Return mTagViewsRestrictPrefixValue
        End Get
        Set(ByVal value As String)
            mTagViewsRestrictPrefixValue = value
        End Set
    End Property

    Public Property TagViewsRestrictPrefixValueArea() As String
        Get
            Return mTagViewsRestrictPrefixValueArea
        End Get
        Set(ByVal value As String)
            mTagViewsRestrictPrefixValueArea = value
        End Set
    End Property

    Public Property TagViewsListReverse() As String
        Get
            Return mTagViewsListReverse
        End Get
        Set(ByVal value As String)
            mTagViewsListReverse = value
        End Set
    End Property

    Public Property TagViewsRoomTag() As String
        Get
            Return mTagViewsRoomTag
        End Get
        Set(ByVal value As String)
            mTagViewsRoomTag = value
        End Set
    End Property

    Public Property TagViewsAreaTag() As String
        Get
            Return mTagViewsAreaTag
        End Get
        Set(ByVal value As String)
            mTagViewsAreaTag = value
        End Set
    End Property

    Public Property TagViewsParmViewName() As String
        Get
            Return mTagViewsParmViewName
        End Get
        Set(ByVal value As String)
            mTagViewsParmViewName = value
        End Set
    End Property
    Public Property TagViewsPrefixViewSource() As String
        Get
            Return mTagViewsPrefixViewSource
        End Get
        Set(ByVal value As String)
            mTagViewsPrefixViewSource = value
        End Set
    End Property

    Public Property TagViewsPrefixViewSourceArea() As String
        Get
            Return mTagViewsPrefixViewSourceArea
        End Get
        Set(ByVal value As String)
            mTagViewsPrefixViewSourceArea = value
        End Set
    End Property

    Public Property TagViewsStripSuffix() As String
        Get
            Return mTagViewsStripSuffix
        End Get
        Set(ByVal value As String)
            mTagViewsStripSuffix = value
        End Set
    End Property

    Public Property SheetsFromViewsIncludeExisting() As String
        Get
            Return mSheetsFromViewsIncludeExisting
        End Get
        Set(ByVal value As String)
            mSheetsFromViewsIncludeExisting = value
        End Set
    End Property
    Public Property SheetsFromViewsRestrictPrefix() As String
        Get
            Return mSheetsFromViewsRestrictPrefix
        End Get
        Set(ByVal value As String)
            mSheetsFromViewsRestrictPrefix = value
        End Set
    End Property
    Public Property SheetsFromViewsRestrictPrefixValue() As String
        Get
            Return mSheetsFromViewsRestrictPrefixValue
        End Get
        Set(ByVal value As String)
            mSheetsFromViewsRestrictPrefixValue = value
        End Set
    End Property
    Public Property SheetsFromViewsListReverse() As String
        Get
            Return mSheetsFromViewsListReverse
        End Get
        Set(ByVal value As String)
            mSheetsFromViewsListReverse = value
        End Set
    End Property

    Public Property SheetsFromViewsTitleblock() As String
        Get
            Return mSheetsFromViewsTitleblock
        End Get
        Set(ByVal value As String)
            mSheetsFromViewsTitleblock = value
        End Set
    End Property
#End Region

    Private Sub RefreshRevitValues()
        'Gets Revit settings for document, view, etc; 
        mApplication = mCommandData.Application
        mDocument = mApplication.ActiveUIDocument.Document
        mActiveView = mDocument.ActiveView
        mCurrentViewIsPlan = True
        Try
            mActiveViewPlan = DirectCast(mActiveView, ViewPlan)
        Catch
            mCurrentViewIsPlan = False
        End Try
        mCurrentLevel = mActiveView.GenLevel
        Dim parameter As Parameter = mActiveView.Parameter(BuiltInParameter.VIEW_PHASE)
        If parameter IsNot Nothing Then
            Dim elementId As ElementId = parameter.AsElementId()
            mCurrentPhase = TryCast(mDocument.GetElement(elementId), Phase)
        End If
    End Sub

    Private Sub RefreshIniValues()
        'reads ini if possible; creates ini and sets defaults if necessary.

        If mUtilityIni.ReadIniFile(mIniPath, mListSettings) Then
            ReadListSettings()
        Else
            WriteDefaultIniValues()
        End If

    End Sub
    Public Function CalculateProjectFolderPath() As String
        Dim path As String = ""
        Try
            path = mDocument.PathName
            If path <> "" Then
                Dim pos As Integer = path.LastIndexOf("\")
                path = path.Substring(0, pos)
            End If
        Catch
            path = ""
        End Try
        Return path
    End Function

    Public Sub Refresh()
        RefreshRevitValues()
        'Do the basic Revit ones each time since user may have switched project
        RefreshIniValues()
        'Read the ini file or set defaults if it is not found
    End Sub
    Public Sub WriteIni()
        If mIniPath = "" Then
            System.Windows.Forms.MessageBox.Show("Note: Settings cannot be saved until project is saved.", cProgramName)
            Return
        End If
        WriteListSettings()
        mUtilityIni.WriteIniFile(mIniPath, mListSettings)
    End Sub
    Public Sub ReadIni()
        mUtilityIni.ReadIniFile(mIniPath, mListSettings)
        ReadListSettings()
    End Sub

    Public Sub ReloadDefaults()
        WriteDefaultIniValues()
        mUtilityIni.ReadIniFile(mIniPath, mListSettings)
        ReadListSettings()
    End Sub

#Region "INI Stuff"
    Private Sub WriteDefaultIniValues()
        'These are the hard-coded defaults

        mAreasPlaceParamList1 = "Number"
        mAreasPlaceParamList2 = "Name"
        mAreasPlaceListPadYes1 = "true"
        mAreasPlaceListPadYes2 = "false"
        mAreasPlaceListPad1 = "5"
        mAreasPlaceListPad2 = "5"
        mAreasPlaceListReverse = "false"

        mAreasPlaceStartX = "0"
        mAreasPlaceStartY = "0"
        mAreasPlaceSpace = "1"
        mAreasPlaceNoRow = "10"
        mAreasPlaceParamReqArea = "AreaPlanned"
        mAreasPlaceReqDefault = "200"

        mRoomsPlaceParamList1 = "Number"
        mRoomsPlaceParamList2 = "Name"
        mRoomsPlaceListPadYes1 = "true"
        mRoomsPlaceListPadYes2 = "false"
        mRoomsPlaceListPad1 = "5"
        mRoomsPlaceListPad2 = "5"
        mRoomsPlaceListReverse = "false"

        mRoomsPlaceStartX = "0"
        mRoomsPlaceStartY = "0"
        mRoomsPlaceSpace = "10"
        mRoomsPlaceNoRow = "10"
        mRoomsPlaceParamReqArea = "Required Area"
        mRoomsPlaceReqDefault = "200"

        mRoomsFromAreasIncludePlaced = "false"
        mRoomsFromAreasIncludeNotPlaced = "false"
        mRoomsFromAreasIncludeBoth = "true"
        mRoomsFromAreasParamList1 = "Number"
        mRoomsFromAreasParamList2 = "Name"
        mRoomsFromAreasListPadYes1 = "true"
        mRoomsFromAreasListPadYes2 = "false"
        mRoomsFromAreasListPad1 = "5"
        mRoomsFromAreasListPad2 = "5"
        mRoomsFromAreasListReverse = "false"

        mViewsFromRoomsGrouping = "single"
        mViewsFromRoomsParamList1 = "Number"
        mViewsFromRoomsParamList2 = "Name"
        mViewsFromRoomsListPadYes1 = "true"
        mViewsFromRoomsListPadYes2 = "false"
        mViewsFromRoomsListPad1 = "5"
        mViewsFromRoomsListPad2 = "5"
        mViewsFromRoomsParamGroupBy = ""
        mViewsFromRoomsListExisting = "true"
        mViewsFromRoomsListReverse = "false"

        mViewsFromRoomsParmViewName = "Number"
        mViewsFromRoomsParmRoomName = "Name"
        mViewsFromRoomsScale = "192"
        mViewsFromRoomsPrefixViewTarget = "Room"
        mViewsFromRoomsViewType = "2d"
        mViewsFromRoomsVectorX = "1"
        mViewsFromRoomsVectorY = "3"
        mViewsFromRoomsVectorZ = "-10"
        mViewsFromRoomsReplaceExisting = "true"

        mViewsFromRoomsSizeBoxType = "dynamic"
        mViewsFromRoomsBoxSpace = "10"
        mViewsFromRoomsBoxFixedX = "50"
        mViewsFromRoomsBoxFixedY = "50"
        mViewsFromRoomsBoxFixedZ = "50"
        mViewsFromRoomsBoxShow = "false"
        mViewsFromRoomsSizeCropType = "dynamic"
        mViewsFromRoomsCropSpace = "10"
        mViewsFromRoomsCropFixedX = "50"
        mViewsFromRoomsCropFixedY = "50"
        'mViewsFromRoomsCropFixedZ = "50";
        mViewsFromRoomsCropShow = "false"

        mViewsFromAreasGrouping = "single"
        mViewsFromAreasParamList1 = "Number"
        mViewsFromAreasParamList2 = "Name"
        mViewsFromAreasListPadYes1 = "true"
        mViewsFromAreasListPadYes2 = "false"
        mViewsFromAreasListPad1 = "5"
        mViewsFromAreasListPad2 = "5"
        mViewsFromAreasParamGroupBy = ""
        mViewsFromAreasListExisting = "true"
        mViewsFromAreasListReverse = "false"

        mViewsFromAreasParmViewName = "Number"
        mViewsFromAreasParmAreaName = "Name"
        mViewsFromAreasScale = "192"
        mViewsFromAreasPrefixViewTarget = "Area"
        mViewsFromAreasViewType = "2d"
        mViewsFromAreasVectorX = "1"
        mViewsFromAreasVectorY = "3"
        mViewsFromAreasVectorZ = "-10"
        mViewsFromAreasReplaceExisting = "true"

        mViewsFromAreasSizeBoxType = "dynamic"
        mViewsFromAreasBoxSpace = "10"
        mViewsFromAreasBoxFixedX = "50"
        mViewsFromAreasBoxFixedY = "50"
        mViewsFromAreasBoxFixedZ = "50"
        mViewsFromAreasBoxShow = "false"
        mViewsFromAreasSizeCropType = "dynamic"
        mViewsFromAreasCropSpace = "10"
        mViewsFromAreasCropFixedX = "50"
        mViewsFromAreasCropFixedY = "50"
        'mViewsFromAreasCropFixedZ = "50";
        mViewsFromAreasCropShow = "false"

        mImagesFromViewsIncludeExisting = "false"
        mImagesFromViewsRestrictPrefix = "true"
        mImagesFromViewsRestrictPrefixValue = "Room"
        mImagesFromViewsListReverse = "false"

        mImagesFromViewsFolderPath = ""

        mTagViewsIncludeExisting = "false"
        mTagViewsRestrictPrefix = "true"
        mTagViewsRestrictPrefixValue = "Room"
        mTagViewsRestrictPrefixValueArea = "Area"
        mTagViewsListReverse = "false"

        mTagViewsRoomTag = "Room Data Tag"
        mTagViewsAreaTag = "Area Data Tag"
        mTagViewsParmViewName = "Number"
        mTagViewsPrefixViewSource = "Room"
        mTagViewsPrefixViewSourceArea = "Area"
        mTagViewsStripSuffix = "true"

        mSheetsFromViewsIncludeExisting = "false"
        mSheetsFromViewsRestrictPrefix = "true"
        mSheetsFromViewsRestrictPrefixValue = "RDS"
        mSheetsFromViewsListReverse = "false"

        mSheetsFromViewsTitleblock = "RoomDataSheet"

        WriteListSettings()
        mUtilityIni.WriteIniFile(mIniPath, mListSettings)
    End Sub

    Private Sub ReadListSettings()
        Try
            Dim i As Integer = 0

            mAreasPlaceParamList1 = mListSettings(i)
            i += 1
            mAreasPlaceParamList2 = mListSettings(i)
            i += 1
            mAreasPlaceListPadYes1 = mListSettings(i)
            i += 1
            mAreasPlaceListPadYes2 = mListSettings(i)
            i += 1
            mAreasPlaceListPad1 = mListSettings(i)
            i += 1
            mAreasPlaceListPad2 = mListSettings(i)
            i += 1
            mAreasPlaceListReverse = mListSettings(i)
            i += 1

            mAreasPlaceStartX = mListSettings(i)
            i += 1
            mAreasPlaceStartY = mListSettings(i)
            i += 1
            mAreasPlaceSpace = mListSettings(i)
            i += 1
            mAreasPlaceNoRow = mListSettings(i)
            i += 1
            mAreasPlaceParamReqArea = mListSettings(i)
            i += 1
            mAreasPlaceReqDefault = mListSettings(i)
            i += 1

            mRoomsPlaceParamList1 = mListSettings(i)
            i += 1
            mRoomsPlaceParamList2 = mListSettings(i)
            i += 1
            mRoomsPlaceListPadYes1 = mListSettings(i)
            i += 1
            mRoomsPlaceListPadYes2 = mListSettings(i)
            i += 1
            mRoomsPlaceListPad1 = mListSettings(i)
            i += 1
            mRoomsPlaceListPad2 = mListSettings(i)
            i += 1
            mRoomsPlaceListReverse = mListSettings(i)
            i += 1

            mRoomsPlaceStartX = mListSettings(i)
            i += 1
            mRoomsPlaceStartY = mListSettings(i)
            i += 1
            mRoomsPlaceSpace = mListSettings(i)
            i += 1
            mRoomsPlaceNoRow = mListSettings(i)
            i += 1
            mRoomsPlaceParamReqArea = mListSettings(i)
            i += 1
            mRoomsPlaceReqDefault = mListSettings(i)
            i += 1

            mRoomsFromAreasIncludePlaced = mListSettings(i)
            i += 1
            mRoomsFromAreasIncludeNotPlaced = mListSettings(i)
            i += 1
            mRoomsFromAreasIncludeBoth = mListSettings(i)
            i += 1
            mRoomsFromAreasParamList1 = mListSettings(i)
            i += 1
            mRoomsFromAreasParamList2 = mListSettings(i)
            i += 1
            mRoomsFromAreasListPadYes1 = mListSettings(i)
            i += 1
            mRoomsFromAreasListPadYes2 = mListSettings(i)
            i += 1
            mRoomsFromAreasListPad1 = mListSettings(i)
            i += 1
            mRoomsFromAreasListPad2 = mListSettings(i)
            i += 1
            mRoomsFromAreasListReverse = mListSettings(i)
            i += 1

            mViewsFromRoomsGrouping = mListSettings(i)
            i += 1
            mViewsFromRoomsParamList1 = mListSettings(i)
            i += 1
            mViewsFromRoomsParamList2 = mListSettings(i)
            i += 1
            mViewsFromRoomsListPadYes1 = mListSettings(i)
            i += 1
            mViewsFromRoomsListPadYes2 = mListSettings(i)
            i += 1
            mViewsFromRoomsListPad1 = mListSettings(i)
            i += 1
            mViewsFromRoomsListPad2 = mListSettings(i)
            i += 1
            mViewsFromRoomsParamGroupBy = mListSettings(i)
            i += 1
            mViewsFromRoomsListExisting = mListSettings(i)
            i += 1
            mViewsFromRoomsListReverse = mListSettings(i)
            i += 1

            mViewsFromRoomsParmViewName = mListSettings(i)
            i += 1
            mViewsFromRoomsParmRoomName = mListSettings(i)
            i += 1
            mViewsFromRoomsScale = mListSettings(i)
            i += 1
            mViewsFromRoomsPrefixViewTarget = mListSettings(i)
            i += 1
            mViewsFromRoomsSizeBoxType = mListSettings(i)
            i += 1
            mViewsFromRoomsViewType = mListSettings(i)
            i += 1
            mViewsFromRoomsVectorX = mListSettings(i)
            i += 1
            mViewsFromRoomsVectorY = mListSettings(i)
            i += 1
            mViewsFromRoomsVectorZ = mListSettings(i)
            i += 1
            mViewsFromRoomsReplaceExisting = mListSettings(i)
            i += 1

            mViewsFromRoomsSizeBoxType = mListSettings(i)
            i += 1
            mViewsFromRoomsBoxSpace = mListSettings(i)
            i += 1
            mViewsFromRoomsBoxFixedX = mListSettings(i)
            i += 1
            mViewsFromRoomsBoxFixedY = mListSettings(i)
            i += 1
            mViewsFromRoomsBoxFixedZ = mListSettings(i)
            i += 1
            mViewsFromRoomsBoxShow = mListSettings(i)
            i += 1
            mViewsFromRoomsSizeCropType = mListSettings(i)
            i += 1
            mViewsFromRoomsCropSpace = mListSettings(i)
            i += 1
            mViewsFromRoomsCropFixedX = mListSettings(i)
            i += 1
            mViewsFromRoomsCropFixedY = mListSettings(i)
            i += 1
            'mViewsFromRoomsCropFixedZ = mListSettings[i]; i++;
            mViewsFromRoomsCropShow = mListSettings(i)
            i += 1

            mViewsFromAreasGrouping = mListSettings(i)
            i += 1
            mViewsFromAreasParamList1 = mListSettings(i)
            i += 1
            mViewsFromAreasParamList2 = mListSettings(i)
            i += 1
            mViewsFromAreasListPadYes1 = mListSettings(i)
            i += 1
            mViewsFromAreasListPadYes2 = mListSettings(i)
            i += 1
            mViewsFromAreasListPad1 = mListSettings(i)
            i += 1
            mViewsFromAreasListPad2 = mListSettings(i)
            i += 1
            mViewsFromAreasParamGroupBy = mListSettings(i)
            i += 1
            mViewsFromAreasListExisting = mListSettings(i)
            i += 1
            mViewsFromAreasListReverse = mListSettings(i)
            i += 1

            mViewsFromAreasParmViewName = mListSettings(i)
            i += 1
            mViewsFromAreasParmAreaName = mListSettings(i)
            i += 1
            mViewsFromAreasScale = mListSettings(i)
            i += 1
            mViewsFromAreasPrefixViewTarget = mListSettings(i)
            i += 1
            mViewsFromAreasSizeBoxType = mListSettings(i)
            i += 1
            mViewsFromAreasViewType = mListSettings(i)
            i += 1
            mViewsFromAreasVectorX = mListSettings(i)
            i += 1
            mViewsFromAreasVectorY = mListSettings(i)
            i += 1
            mViewsFromAreasVectorZ = mListSettings(i)
            i += 1
            mViewsFromAreasReplaceExisting = mListSettings(i)
            i += 1

            mViewsFromAreasSizeBoxType = mListSettings(i)
            i += 1
            mViewsFromAreasBoxSpace = mListSettings(i)
            i += 1
            mViewsFromAreasBoxFixedX = mListSettings(i)
            i += 1
            mViewsFromAreasBoxFixedY = mListSettings(i)
            i += 1
            mViewsFromAreasBoxFixedZ = mListSettings(i)
            i += 1
            mViewsFromAreasBoxShow = mListSettings(i)
            i += 1
            mViewsFromAreasSizeCropType = mListSettings(i)
            i += 1
            mViewsFromAreasCropSpace = mListSettings(i)
            i += 1
            mViewsFromAreasCropFixedX = mListSettings(i)
            i += 1
            mViewsFromAreasCropFixedY = mListSettings(i)
            i += 1
            'mViewsFromAreasCropFixedZ = mListSettings[i]; i++;
            mViewsFromAreasCropShow = mListSettings(i)
            i += 1

            mImagesFromViewsIncludeExisting = mListSettings(i)
            i += 1
            mImagesFromViewsRestrictPrefix = mListSettings(i)
            i += 1
            mImagesFromViewsRestrictPrefixValue = mListSettings(i)
            i += 1
            mImagesFromViewsListReverse = mListSettings(i)
            i += 1

            mImagesFromViewsFolderPath = mListSettings(i)
            i += 1

            mTagViewsIncludeExisting = mListSettings(i)
            i += 1
            mTagViewsRestrictPrefix = mListSettings(i)
            i += 1
            mTagViewsRestrictPrefixValue = mListSettings(i)
            i += 1
            mTagViewsRestrictPrefixValueArea = mListSettings(i)
            i += 1
            mTagViewsListReverse = mListSettings(i)
            i += 1

            mTagViewsRoomTag = mListSettings(i)
            i += 1
            mTagViewsAreaTag = mListSettings(i)
            i += 1
            mTagViewsParmViewName = mListSettings(i)
            i += 1
            mTagViewsPrefixViewSource = mListSettings(i)
            i += 1
            mTagViewsPrefixViewSourceArea = mListSettings(i)
            i += 1
            mTagViewsStripSuffix = mListSettings(i)
            i += 1

            mSheetsFromViewsIncludeExisting = mListSettings(i)
            i += 1
            mSheetsFromViewsRestrictPrefix = mListSettings(i)
            i += 1
            mSheetsFromViewsRestrictPrefixValue = mListSettings(i)
            i += 1
            mSheetsFromViewsListReverse = mListSettings(i)
            i += 1

            mSheetsFromViewsTitleblock = mListSettings(i)
            i += 1
        Catch
            'Assume failure is due to an out-of-date ini file so make a new one.
            WriteDefaultIniValues()
        End Try
    End Sub

    Private Sub WriteListSettings()
        mListSettings.Clear()

        mListSettings.Add(mAreasPlaceParamList1)
        mListSettings.Add(mAreasPlaceParamList2)
        mListSettings.Add(mAreasPlaceListPadYes1)
        mListSettings.Add(mAreasPlaceListPadYes2)
        mListSettings.Add(mAreasPlaceListPad1)
        mListSettings.Add(mAreasPlaceListPad2)
        mListSettings.Add(mAreasPlaceListReverse)

        mListSettings.Add(mAreasPlaceStartX)
        mListSettings.Add(mAreasPlaceStartY)
        mListSettings.Add(mAreasPlaceSpace)
        mListSettings.Add(mAreasPlaceNoRow)
        mListSettings.Add(mAreasPlaceParamReqArea)
        mListSettings.Add(mAreasPlaceReqDefault)

        mListSettings.Add(mRoomsPlaceParamList1)
        mListSettings.Add(mRoomsPlaceParamList2)
        mListSettings.Add(mRoomsPlaceListPadYes1)
        mListSettings.Add(mRoomsPlaceListPadYes2)
        mListSettings.Add(mRoomsPlaceListPad1)
        mListSettings.Add(mRoomsPlaceListPad2)
        mListSettings.Add(mRoomsPlaceListReverse)

        mListSettings.Add(mRoomsPlaceStartX)
        mListSettings.Add(mRoomsPlaceStartY)
        mListSettings.Add(mRoomsPlaceSpace)
        mListSettings.Add(mRoomsPlaceNoRow)
        mListSettings.Add(mRoomsPlaceParamReqArea)
        mListSettings.Add(mRoomsPlaceReqDefault)

        mListSettings.Add(RoomsFromAreasIncludePlaced)
        mListSettings.Add(mRoomsFromAreasIncludeNotPlaced)
        mListSettings.Add(mRoomsFromAreasIncludeBoth)
        mListSettings.Add(mRoomsFromAreasParamList1)
        mListSettings.Add(mRoomsFromAreasParamList2)
        mListSettings.Add(mRoomsFromAreasListPadYes1)
        mListSettings.Add(mRoomsFromAreasListPadYes2)
        mListSettings.Add(mRoomsFromAreasListPad1)
        mListSettings.Add(mRoomsFromAreasListPad2)
        mListSettings.Add(mRoomsFromAreasListReverse)

        'Views From Room
        mListSettings.Add(mViewsFromRoomsGrouping)
        mListSettings.Add(mViewsFromRoomsParamList1)
        mListSettings.Add(mViewsFromRoomsParamList2)
        mListSettings.Add(mViewsFromRoomsListPadYes1)
        mListSettings.Add(mViewsFromRoomsListPadYes2)
        mListSettings.Add(mViewsFromRoomsListPad1)
        mListSettings.Add(mViewsFromRoomsListPad2)
        mListSettings.Add(mViewsFromRoomsParamGroupBy)
        mListSettings.Add(mViewsFromRoomsListExisting)
        mListSettings.Add(mViewsFromRoomsListReverse)

        mListSettings.Add(mViewsFromRoomsParmViewName)
        mListSettings.Add(mViewsFromRoomsParmRoomName)
        mListSettings.Add(mViewsFromRoomsScale)
        mListSettings.Add(mViewsFromRoomsPrefixViewTarget)
        mListSettings.Add(mViewsFromRoomsSizeBoxType)
        mListSettings.Add(mViewsFromRoomsViewType)
        mListSettings.Add(mViewsFromRoomsVectorX)
        mListSettings.Add(mViewsFromRoomsVectorY)
        mListSettings.Add(mViewsFromRoomsVectorZ)
        mListSettings.Add(mViewsFromRoomsReplaceExisting)

        mListSettings.Add(mViewsFromRoomsSizeBoxType)
        mListSettings.Add(mViewsFromRoomsBoxSpace)
        mListSettings.Add(mViewsFromRoomsBoxFixedX)
        mListSettings.Add(mViewsFromRoomsBoxFixedY)
        mListSettings.Add(mViewsFromRoomsBoxFixedZ)
        mListSettings.Add(mViewsFromRoomsBoxShow)
        mListSettings.Add(mViewsFromRoomsSizeBoxType)
        mListSettings.Add(mViewsFromRoomsCropSpace)
        mListSettings.Add(mViewsFromRoomsCropFixedX)
        mListSettings.Add(mViewsFromRoomsCropFixedY)
        'mListSettings.Add(mViewsFromRoomsCropFixedZ);
        mListSettings.Add(mViewsFromRoomsCropShow)

        'Views From Area
        mListSettings.Add(mViewsFromAreasGrouping)
        mListSettings.Add(mViewsFromAreasParamList1)
        mListSettings.Add(mViewsFromAreasParamList2)
        mListSettings.Add(mViewsFromAreasListPadYes1)
        mListSettings.Add(mViewsFromAreasListPadYes2)
        mListSettings.Add(mViewsFromAreasListPad1)
        mListSettings.Add(mViewsFromAreasListPad2)
        mListSettings.Add(mViewsFromAreasParamGroupBy)
        mListSettings.Add(mViewsFromAreasListExisting)
        mListSettings.Add(mViewsFromAreasListReverse)

        mListSettings.Add(mViewsFromAreasParmViewName)
        mListSettings.Add(mViewsFromAreasParmAreaName)
        mListSettings.Add(mViewsFromAreasScale)
        mListSettings.Add(mViewsFromAreasPrefixViewTarget)
        mListSettings.Add(mViewsFromAreasSizeBoxType)
        mListSettings.Add(mViewsFromAreasViewType)
        mListSettings.Add(mViewsFromAreasVectorX)
        mListSettings.Add(mViewsFromAreasVectorY)
        mListSettings.Add(mViewsFromAreasVectorZ)
        mListSettings.Add(mViewsFromAreasReplaceExisting)

        mListSettings.Add(mViewsFromAreasSizeBoxType)
        mListSettings.Add(mViewsFromAreasBoxSpace)
        mListSettings.Add(mViewsFromAreasBoxFixedX)
        mListSettings.Add(mViewsFromAreasBoxFixedY)
        mListSettings.Add(mViewsFromAreasBoxFixedZ)
        mListSettings.Add(mViewsFromAreasBoxShow)
        mListSettings.Add(mViewsFromAreasSizeBoxType)
        mListSettings.Add(mViewsFromAreasCropSpace)
        mListSettings.Add(mViewsFromAreasCropFixedX)
        mListSettings.Add(mViewsFromAreasCropFixedY)
        'mListSettings.Add(mViewsFromAreasCropFixedZ);
        mListSettings.Add(mViewsFromAreasCropShow)


        mListSettings.Add(mImagesFromViewsIncludeExisting)
        mListSettings.Add(mImagesFromViewsRestrictPrefix)
        mListSettings.Add(mImagesFromViewsRestrictPrefixValue)
        mListSettings.Add(mImagesFromViewsListReverse)

        mListSettings.Add(mImagesFromViewsFolderPath)

        mListSettings.Add(mTagViewsIncludeExisting)
        mListSettings.Add(mTagViewsRestrictPrefix)
        mListSettings.Add(mTagViewsRestrictPrefixValue)
        mListSettings.Add(mTagViewsRestrictPrefixValueArea)
        mListSettings.Add(mTagViewsListReverse)

        mListSettings.Add(mTagViewsRoomTag)
        mListSettings.Add(mTagViewsAreaTag)
        mListSettings.Add(mTagViewsParmViewName)
        mListSettings.Add(mTagViewsPrefixViewSource)
        mListSettings.Add(mTagViewsPrefixViewSourceArea)
        mListSettings.Add(mTagViewsStripSuffix)

        mListSettings.Add(mSheetsFromViewsIncludeExisting)
        mListSettings.Add(mSheetsFromViewsRestrictPrefix)
        mListSettings.Add(mSheetsFromViewsRestrictPrefixValue)
        mListSettings.Add(mSheetsFromViewsListReverse)

        mListSettings.Add(mSheetsFromViewsTitleblock)
    End Sub
#End Region

End Class
