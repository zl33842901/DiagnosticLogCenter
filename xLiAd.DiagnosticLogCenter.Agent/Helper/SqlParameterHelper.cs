using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Agent.Helper
{
    public static class SqlParameterHelper
    {
        public static string ConvertToString(this SqlParameterCollection sqlParameterCollection)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\r\n");
            foreach(SqlParameter item in sqlParameterCollection)
            {
                sb.Append("  ");
                sb.Append(item.ParameterName);
                sb.Append(" : ");
                sb.Append("\"");
                sb.Append(item.Value);
                sb.Append("\",");
            }
            sb.Append("}");
            return sb.ToString();
        }


        public static string FormatDynamicString(this object parameters)
        {
            try
            {
                var di = parameters.ToDictionary();
                if (di == null)
                    return null;
                StringBuilder sbP = new StringBuilder();
                sbP.Append("{ \r\n");
                List<string> ls = new List<string>();
                foreach (var i in di)
                {
                    if (!(i.Value is string) && i.Value is IEnumerable arr)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var ob in arr)
                        {
                            if (sb.Length > 0)
                                sb.Append(", ");
                            sb.Append('"');
                            sb.Append(ob);
                            sb.Append('"');
                        }
                        ls.Add($"  \"{i.Key}\" : [ { sb } ]");
                    }
                    else
                    {
                        ls.Add($"  \"{i.Key}\" : \"{i.Value}\"");
                    }
                }
                sbP.Append(string.Join(",\r\n", ls));
                sbP.Append("\r\n}");
                return sbP.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        private static Dictionary<string, object> ToDictionary(this object parameters)
        {
            try
            {
                var p = parameters.GetType().GetField("parameters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (p == null)
                    return null;
                var di = p.GetValue(parameters) as IDictionary;// Dictionary<string, Dapper.DynamicParameters.ParamInfo>;
                Type paramInfoType = Type.GetType("Dapper.DynamicParameters+ParamInfo,Dapper");
                var pp = paramInfoType.GetProperty("Value");
                Dictionary<string, object> ls = new Dictionary<string, object>();
                foreach (DictionaryEntry i in di)
                {
                    ls.Add(i.Key.ToString(), pp.GetValue(i.Value));
                }
                return ls;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
