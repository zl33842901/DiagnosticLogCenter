using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.DbExportConsole
{
    public class Arguments
    {
        public string Clients { get; set; }
        public string MaxDate { get; set; }
        public string DropAfterBak { get; set; }
        public string Url { get; set; }
    }

    public static class ArgumentsHelper
    {
        public static T BindArguments<T>(string[] args) where T : class
        {
            var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.CanWrite && x.CanRead && x.PropertyType == typeof(string)).ToArray();
            PropertyInfo prop = null;
            T result = Activator.CreateInstance<T>();
            foreach (var arg in args)
            {
                if(arg.StartsWith('-'))
                {
                    if (prop != null)
                        SetValue(prop, result, string.Empty);
                    var propName = arg.TrimStart('-');
                    prop = props.Where(x => x.Name.Equals(propName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if(prop == null && propName.Length == 1)
                    {
                        prop = props.Where(x => x.Name.StartsWith(propName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    }//其余情况只好丢掉了
                }
                else
                {
                    if (prop != null)
                    {
                        SetValue(prop, result, arg);
                        prop = null;
                    }
                }
            }
            return result;
        }

        private static void SetValue(PropertyInfo prop, object o, string value)
        {
            prop.SetValue(o, value);
        }
    }
}
