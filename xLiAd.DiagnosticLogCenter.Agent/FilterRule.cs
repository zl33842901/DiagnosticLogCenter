using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.XPath;

namespace xLiAd.DiagnosticLogCenter.Agent
{
    public static class FilterRule
    {
        internal static IEnumerable<string> AllowPath { get; set; }
        internal static IEnumerable<string> ForbbidenPath { get; set; }

        internal static bool ShouldRecord(string path)
        {
            if (AllowPath.AnyX())
            {
                if (path.NullOrEmpty())
                    return false;
                foreach (var item in AllowPath)
                {
                    bool r = Regex.IsMatch(path, item);
                    if (r)
                        return true;
                }
                return false;
            }
            else if (ForbbidenPath.AnyX())
            {
                if (path.NullOrEmpty())
                    return true;
                foreach (var item in AllowPath)
                {
                    bool r = Regex.IsMatch(path, item);
                    if (r)
                        return false;
                }
                return true;
            }
            else
                return true;
        }
    }
}
