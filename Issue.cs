using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot
{
    public class Issue
    {
        public string SystemName { get; set; }
        public string IssueType { get; set; }
        public string HaveErrorMessage  { get; set; }
        public string ErrorMessage { get; set; }
        public string Priority { get; set; }
        public string Urgency { get; set; }
    }
}