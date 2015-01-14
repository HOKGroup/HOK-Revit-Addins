using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System.Windows.Forms;

namespace HOK.MessageSuppresser
{
    public class AppCommand:IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            application.DialogBoxShowing -= new EventHandler<DialogBoxShowingEventArgs>(dialogBoxShowing);
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            application.DialogBoxShowing += new EventHandler<DialogBoxShowingEventArgs>(dialogBoxShowing);
            return Result.Succeeded;
        }

        public void dialogBoxShowing(object sender, DialogBoxShowingEventArgs e)
        {
            try
            {
                TaskDialogShowingEventArgs taskDialog = e as TaskDialogShowingEventArgs;
                if (null != taskDialog)
                {
                    string dialogId = taskDialog.DialogId;
                    if (dialogId == "TaskDialog_Copied_Central_Model")
                    {
                        DialogResult  dr = MessageBox.Show("Would you like to suppress the message about the copied central model?", "Copied Centeral Model", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (dr == DialogResult.Yes)
                        {
                            if (taskDialog.Cancellable)
                            {
                                taskDialog.Cancel = true;
                            }
                            taskDialog.OverrideResult((int)DialogResult.OK);
                        }
                    }
                   
                }
            }
            catch(Exception ex)
            {
                string message = ex.Message;
            }
        }
    }
}
