using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HOK.ModelManager.GoogleDocs
{
    public enum LinkItemType
    {
        DraftingView=0,
        Legend=1,
        Sheet=2,
        Families=3,
        None=4
    }

    public class LinkInfo
    {
        private LinkItemType itemType = LinkItemType.None;
        private string sourceModelName = "";
        private string sourceModelPath = "";
        private string destModelName = "";
        private string destModelPath = "";

        private int sourceItemId = 0;
        private string sourceItemName = "";
        private int destItemId = 0;
        private string destItemName = "";
        private string destImagePath1 = "";
        private string destImagePath2 = "";

        private string linkModified = "";
        private string linkModifiedBy = "";

        public LinkItemType ItemType { get { return itemType; } set { itemType = value; } }
        public string SourceModelName { get { return sourceModelName; } set { sourceModelName = value; } }
        public string SourceModelPath { get { return sourceModelPath; } set { sourceModelPath = value; } }
        public string DestModelName { get { return destModelName; } set { destModelName = value; } }
        public string DestModelPath { get { return destModelPath; } set { destModelPath = value; } }

        public int SourceItemId { get { return sourceItemId; } set { sourceItemId = value; } }
        public string SourceItemName { get { return sourceItemName; } set { sourceItemName = value; } }
        public int DestItemId { get { return destItemId; } set { destItemId = value; } }
        public string DestItemName { get { return destItemName; } set { destItemName = value; } }
        public string DestImagePath1 { get { return destImagePath1; } set { destImagePath1 = value; } }
        public string DestImagePath2 { get { return destImagePath2; } set { destImagePath2 = value; } }

        public string LinkModified { get { return linkModified; } set { linkModified = value; } }
        public string LinkModifiedBy { get { return linkModifiedBy; } set { linkModifiedBy = value; } }

        public LinkInfo()
        {
        }

        public LinkItemType GetLinkItemType(string itemType)
        {
            LinkItemType lit = (LinkItemType)Enum.Parse(typeof(LinkItemType), itemType);
            return lit;
        }
    }
}
