using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARUP.IssueTracker.Classes
{
    /// <summary>
    /// This is a enumeration for storing metadata from  Jira/BCF to Navisworks' embedded document database
    /// </summary>
    public enum MetadataTables
    {
        JiraIssue, BcfTopic, Viewpoint, BcfComment
    }
}
