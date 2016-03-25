using HOK.SheetManager.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SheetManager.AddIn.Classes
{
    public class MessageInfo
    {
        private Guid dbId = Guid.Empty;
        private string itemName = "";
        private string message = "";

        public Guid DBId { get { return dbId; } set { dbId = value; } }
        public string ItemName { get { return itemName; } set { itemName = value; } }
        public string Message { get { return message; } set { message = value; } }

        public MessageInfo()
        {

        }

        public MessageInfo(RevitSheet sheet, string messageStr)
        {
            dbId = sheet.Id;
            itemName = sheet.Number + " " + sheet.Name;
            message = messageStr;
        }

        public MessageInfo(Guid id, string name, string messageStr)
        {
            dbId = id;
            itemName = name;
            message = messageStr;
        }
    }
}
