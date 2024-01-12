using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.AgentFramework
{
    public static class FilterRule
    {
        internal static IEnumerable<string> AllowPath { get; set; }
        internal static IEnumerable<string> ForbbidenPath { get; set; }

        internal static bool ShouldRecord(string path)
        {
            if (AllowPath != null && AllowPath.Any())
            {
                if (string.IsNullOrEmpty(path))
                    return false;
                foreach (var item in AllowPath)
                {
                    bool r = Regex.IsMatch(path, item);
                    if (r)
                        return true;
                }
                return false;
            }
            else if (ForbbidenPath != null && ForbbidenPath.Any())
            {
                if (string.IsNullOrEmpty(path))
                    return true;
                foreach (var item in ForbbidenPath)
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
