using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.AddInManager.Utils
{
    public static class ToolTipReader
    {
        public static ToolTipProperties ReadToolTip(string txtfile, string toolName)
        {
            ToolTipProperties ttt = new ToolTipProperties();
            try
            {
                if (!File.Exists(txtfile)) { return ttt; }
                using (StreamReader reader = File.OpenText(txtfile))
                {
                    string line;
                    int index = 0;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains("------------------")) { ttt = new ToolTipProperties(); continue; }

                        switch (index % 3)
                        {
                            case 0:
                                ttt.ToolName = line;
                                index++;
                                break;
                            case 1:
                                ttt.Description = line;
                                index++;
                                break;
                            case 2:
                                ttt.HelpUrl = line;
                                if (ttt.ToolName == toolName) { return ttt; }

                                index = 0;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return ttt;
        }
    }

    public class ToolTipProperties
    {
        private string toolName = "";
        private string description = "";
        private string helpUrl = "";

        public string ToolName { get { return toolName; } set { toolName = value; } }
        public string Description { get { return description; } set { description = value; } }
        public string HelpUrl { get { return helpUrl; } set { helpUrl = value; } }

        public ToolTipProperties()
        {
        }
    }
}
