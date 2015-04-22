using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    public enum LinkItemStatus
    {
        None,
        Added,
        Updated, 
        Deleted
    }

    public class LinkInfo : INotifyPropertyChanged
    {
        private LinkItemType itemType = LinkItemType.None;
        private string sourceModelName = "";
        private string sourceModelPath = "";
        private string sourceModelId = "";
        private string destModelName = "";
        private string destModelPath = "";

        private int sourceItemId = -1;
        private string sourceItemName = "";
        private int destItemId = -1;
        private string destItemName = "";
        private string destImagePath1 = "";
        private string destImagePath2 = "";

        private string linkModified = "";
        private string linkModifiedBy = "";
        private LinkItemStatus itemStatus = LinkItemStatus.None;

        public LinkItemType ItemType { get { return itemType; } set { itemType = value; onPropertyChanged(this, "ItemType"); } }
        public string SourceModelName { get { return sourceModelName; } set { sourceModelName = value; onPropertyChanged(this, "SourceModelName"); } }
        public string SourceModelPath { get { return sourceModelPath; } set { sourceModelPath = value; onPropertyChanged(this, "SourceModelPath"); } }
        public string SourceModelId { get { return sourceModelId; } set { sourceModelId = value; onPropertyChanged(this, "SourceModelId"); } }
        public string DestModelName { get { return destModelName; } set { destModelName = value; onPropertyChanged(this, "DestModelName"); } }
        public string DestModelPath { get { return destModelPath; } set { destModelPath = value; onPropertyChanged(this, "DestModelPath"); } }

        public int SourceItemId { get { return sourceItemId; } set { sourceItemId = value; onPropertyChanged(this, "SourceItemId"); } }
        public string SourceItemName { get { return sourceItemName; } set { sourceItemName = value; onPropertyChanged(this, "SourceItemName"); } }
        public int DestItemId { get { return destItemId; } set { destItemId = value; onPropertyChanged(this, "DestItemId"); } }
        public string DestItemName { get { return destItemName; } set { destItemName = value; onPropertyChanged(this, "DestItemName"); } }
        public string DestImagePath1 { get { return destImagePath1; } set { destImagePath1 = value; onPropertyChanged(this, "DestImagePath1"); } }
        public string DestImagePath2 { get { return destImagePath2; } set { destImagePath2 = value; onPropertyChanged(this, "DestImagePath2"); } }

        public string LinkModified { get { return linkModified; } set { linkModified = value; onPropertyChanged(this, "LinkModified"); } }
        public string LinkModifiedBy { get { return linkModifiedBy; } set { linkModifiedBy = value; onPropertyChanged(this, "LinkModifiedBy"); } }
        public LinkItemStatus ItemStatus { get { return itemStatus; } set { itemStatus = value; onPropertyChanged(this, "ItemStatus"); } }

        public LinkInfo()
        {
        }

        public LinkItemType GetLinkItemType(string itemType)
        {
            LinkItemType lit = (LinkItemType)Enum.Parse(typeof(LinkItemType), itemType);
            return lit;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(object sender, string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
