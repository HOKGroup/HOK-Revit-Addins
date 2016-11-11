using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ARUP.IssueTracker.Classes
{
    public static class IndexByName
    {
        public static int Get<T>(string propertyValue, string propertyName, List<T> mylist) where T : new()
        {
            if (mylist.Count == 0)
                return -1;
            Type t = mylist[0].GetType();
            PropertyInfo prop = t.GetProperty(propertyName);
            for (var i = 0; i < mylist.Count; i++)
            {
                string theValue = prop.GetValue(mylist[i], null).ToString();
                if (theValue == propertyValue)
                    return i;
            }
            return -1;
        }
    }
}
