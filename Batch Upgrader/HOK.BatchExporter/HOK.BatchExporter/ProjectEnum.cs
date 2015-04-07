using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32.TaskScheduler;

namespace HOK.BatchExporter
{
    public class ProjectHeader
    {
        public string ProjectName { get; set; }
        public string Office { get; set; }
    }

    public class RevitFileHeader
    {
        public string RevitFile { get; set; }
        public string OutputFolder { get; set; }
    }

    public enum WorksetConfiguration
    {
        AllEditable=0, AllWorksets, AskUserToSpecify, LastViewed
    }
}
