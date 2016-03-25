using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SheetDataEditor
{
    public static class DatabaseResources
    {
        public static string[] tableNames = new string[] { "Discipline", "LinkedProjects", "LinkedRevisions", "LinkedSheets", "ReplaceItems", "RevisionDocuments", "Revisions"
        , "RevisionsOnSheet", "Sheets", "Views","ViewTypes"};

        public static Dictionary<Guid, RevitViewType> GetDefaultViewTypes()
        {
            Dictionary<Guid, RevitViewType> viewTypes = new Dictionary<Guid, RevitViewType>();
            try
            {
                RevitViewType viewType = new RevitViewType(new Guid("a8b93207-dbe5-4636-bf14-78752f499003"), "Undefined", ViewTypeEnum.Undefined);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("f10e767b-f4b8-4ad2-8395-2fabb1a6984e"), "Floor Plan", ViewTypeEnum.FloorPlan);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("15405703-3dda-45e9-98b1-9247f013a9e5"), "Ceiling Plan", ViewTypeEnum.CeilingPlan);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("b3187c7f-5cd2-4761-a191-70874399f114"), "Elevation", ViewTypeEnum.Elevation);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("76b9129a-c4e7-42f6-bc5e-c0a025cdfc29"), "3D", ViewTypeEnum.ThreeD);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("e18fe0ce-94c6-436e-ac8f-78cd027ebc32"), "Schedule", ViewTypeEnum.Schedule);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("46007e84-7433-4df8-98da-0442e79ee7c2"), "Drawing Sheet", ViewTypeEnum.DrawingSheet);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("5c57b817-fc05-42f2-aa13-84e96979cadd"), "Project Browser", ViewTypeEnum.ProjectBrowser);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("4ace6fa0-ce49-401d-a3d8-b4f48f569884"), "Report", ViewTypeEnum.Report);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("0e59bce9-979c-4a63-8fcb-cc8635c789c3"), "Drafting View", ViewTypeEnum.DraftingView);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("0dec12ec-686a-4572-a007-d3f74248cf51"), "Legend", ViewTypeEnum.Legend);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("74de5570-19f2-4323-aab0-08fb84d07f3c"), "System Browser", ViewTypeEnum.SystemBrowser);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("f472ba01-4761-4f8c-9ef1-cc4866add85c"), "Engineering Plan", ViewTypeEnum.EngineeringPlan);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("690251b8-ceac-477c-8371-165390e25973"), "Area Plan", ViewTypeEnum.AreaPlan);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("3eba3749-b108-4061-9139-780a7d21f3d6"), "Section", ViewTypeEnum.Section);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("bc5bf3c2-ca7e-4979-a658-aa3b45670c5b"), "Detail", ViewTypeEnum.Detail);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("7a5148e7-5c94-4f54-a9c3-204110a929d1"), "Cost Report", ViewTypeEnum.CostReport);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("d3df7aaf-3e0a-4b54-aab1-cae9efd6318c"), "Loads Report", ViewTypeEnum.LoadsReport);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("7753d485-7ab6-4980-80ad-fffa1cce63cd"), "Presure Loss Report", ViewTypeEnum.PresureLossReport);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("cb463925-c003-4885-b79d-4848f3c4cc2c"), "Column Schedule", ViewTypeEnum.ColumnSchedule);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("b8360857-5789-44db-ba9d-9c20ff382be9"), "Panel Schedule", ViewTypeEnum.PanelSchedule);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("621ee3cd-1995-48c9-af93-ef69469d2c87"), "Walkthrough", ViewTypeEnum.Walkthrough);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("0c059550-fd7f-4201-9b31-83e41a0b0742"), "Rendering", ViewTypeEnum.Rendering);
                viewTypes.Add(viewType.Id, viewType);
                viewType = new RevitViewType(new Guid("fcda21a6-a6ce-4498-bfd0-c5fa0123224c"), "Internal", ViewTypeEnum.Internal);
                viewTypes.Add(viewType.Id, viewType);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return viewTypes;
        }

        public static Dictionary<Guid, Discipline> GetDefaultDsiciplines()
        {
            Dictionary<Guid, Discipline> disciplines = new Dictionary<Guid, Discipline>();
            try
            {
                Discipline discipline = new Discipline(new Guid("261ad8fd-ee67-4e52-b2d4-cea0987624bd"), "General");
                disciplines.Add(discipline.Id, discipline);
                discipline = new Discipline(new Guid("02aceb80-5ae9-4c03-8b69-36a39425e4fc"), "Architecture");
                disciplines.Add(discipline.Id, discipline);
                discipline = new Discipline(new Guid("61ac2d5c-9e8d-401e-8b95-3852e8d5782a"), "Civil");
                disciplines.Add(discipline.Id, discipline);
                discipline = new Discipline(new Guid("e73faf18-f224-4a00-8ab0-c9eb00a21d0d"), "Interiors");
                disciplines.Add(discipline.Id, discipline);
                discipline = new Discipline(new Guid("e36d69c1-7377-47c8-a9d0-1f9cdd81d2e5"), "Landscape");
                disciplines.Add(discipline.Id, discipline);
                discipline = new Discipline(new Guid("613dce87-f72f-472a-a84c-c86367e03016"), "MEP");
                disciplines.Add(discipline.Id, discipline);
                discipline = new Discipline(new Guid("d7ce66d1-8877-41eb-863f-10378307f92d"), "Mechanical");
                disciplines.Add(discipline.Id, discipline);
                discipline = new Discipline(new Guid("8860ec00-743b-40d0-91d5-ffa4947a0893"), "Electrical");
                disciplines.Add(discipline.Id, discipline);
                discipline = new Discipline(new Guid("10ae8cee-8828-43d2-9890-e9264a179e9c"), "Plumbing");
                disciplines.Add(discipline.Id, discipline);
                discipline = new Discipline(new Guid("2d351bc6-ed92-426e-8639-d20fa7277609"), "Structural");
                disciplines.Add(discipline.Id, discipline);
                discipline = new Discipline(new Guid("a7ea4cd3-352e-4d8b-b2ac-9f948d5dcf05"), "Telecommunications");
                disciplines.Add(discipline.Id, discipline);
                discipline = new Discipline(new Guid("aa4cdd94-a8a3-4ced-8003-939fdf28d546"), "LEED");
                disciplines.Add(discipline.Id, discipline);
                discipline = new Discipline(new Guid("cd931d10-c037-4657-9f1c-85283f202d0b"), "Demolition");
                disciplines.Add(discipline.Id, discipline);
                discipline = new Discipline(new Guid("00000000-0000-0000-0000-000000000000"), "Undefined");
                disciplines.Add(discipline.Id, discipline);

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return disciplines;
        }

    }
}
