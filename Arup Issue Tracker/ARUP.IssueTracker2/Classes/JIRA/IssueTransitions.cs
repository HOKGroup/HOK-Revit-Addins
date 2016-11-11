using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARUP.IssueTracker.Classes
{
    public class IssueTransitions
    {
        public string expand { get; set; }
        public List<Transition> transitions { get; set; }
    }
}