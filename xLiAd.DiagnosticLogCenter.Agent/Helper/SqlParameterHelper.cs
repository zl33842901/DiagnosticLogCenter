using System;
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
    }
}
