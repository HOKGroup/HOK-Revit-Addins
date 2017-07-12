using HOK.Core.Utilities;
using System;
using System.IO;

namespace HOK.AddInManager.Utils
{
    public static class ToolTipReader
    {
        public static ToolTipProperties ReadToolTip(string txtfile, string toolName)
        {
            var ttt = new ToolTipProperties();
            try
            {
                if (!File.Exists(txtfile)) { return ttt; }
                using (var reader = File.OpenText(txtfile))
                {
                    string line;
                    var index = 0;

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
                Log.AppendLog("HOK.AddInManager.Utils.ToolTipReader.ReadToolTip: " + ex.Message);
            }
            return ttt;
        }
    }

    public class ToolTipProperties
    {
        public string ToolName { get; set; } = "";
        public string Description { get; set; } = "";
        public string HelpUrl { get; set; } = "";
    }
}
