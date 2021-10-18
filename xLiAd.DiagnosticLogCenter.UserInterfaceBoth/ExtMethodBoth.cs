using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.UserInterface.Models;

namespace xLiAd.DiagnosticLogCenter.UserInterfaceBoth
{
    public static partial class ExtMethodBoth
    {
        public static void ProcessEndAndException(this IEnumerable<Log> logs)
        {
            List<Log> result = new List<Log>();
            foreach(var log in logs)
            {
                ProcessEndAndException(log);
            }
        }

        private static Log ProcessEndAndException(this Log log)
        {
            List<LogAdditionItem> items = new List<LogAdditionItem>();
            foreach(var addtion in log.Addtions)
            {
                if(addtion.LogType == Abstract.LogTypeEnum.SqlAfter || addtion.LogType == Abstract.LogTypeEnum.SqlException)
                {
                    var entryItem = log.Addtions.Where(x => x.HttpId == addtion.HttpId && x.LogType == Abstract.LogTypeEnum.SqlBefore).FirstOrDefault();
                    if(entryItem != null)
                    {
                        entryItem.Sucess = addtion.LogType == Abstract.LogTypeEnum.SqlAfter;
                        entryItem.TotalMillionSeconds = Convert.ToInt32((addtion.HappenTime - entryItem.HappenTime).TotalMilliseconds);
                        entryItem.EndTime = addtion.HappenTime;
                        entryItem.WithEnd = true;
                        if(addtion.LogType == Abstract.LogTypeEnum.SqlException)
                        {
                            entryItem.Message = addtion.Message;
                            entryItem.Content = addtion.Content;
                        }
                    }
                    else
                        items.Add(addtion);
                }
                else if(addtion.LogType == Abstract.LogTypeEnum.DapperExSqlAfter || addtion.LogType == Abstract.LogTypeEnum.DapperExSqlException)
                {
                    var entryItem = log.Addtions.Where(x => x.HttpId == addtion.HttpId && x.LogType == Abstract.LogTypeEnum.DapperExSqlBefore).FirstOrDefault();
                    if (entryItem != null)
                    {
                        entryItem.Sucess = addtion.LogType == Abstract.LogTypeEnum.DapperExSqlAfter;
                        entryItem.TotalMillionSeconds = Convert.ToInt32((addtion.HappenTime - entryItem.HappenTime).TotalMilliseconds);
                        entryItem.EndTime = addtion.HappenTime;
                        entryItem.WithEnd = true;
                        if (addtion.LogType == Abstract.LogTypeEnum.DapperExSqlException)
                        {
                            entryItem.Message = addtion.Message;
                            entryItem.Content = addtion.Content;
                        }
                    }
                    else
                        items.Add(addtion);
                }
                else if(addtion.LogType == Abstract.LogTypeEnum.HttpClientResponse || addtion.LogType == Abstract.LogTypeEnum.HttpClientException)
                {
                    var entryItem = log.Addtions.Where(x => x.HttpId == addtion.HttpId && x.LogType == Abstract.LogTypeEnum.HttpClientRequest).FirstOrDefault();
                    if (entryItem != null)
                    {
                        entryItem.Sucess = addtion.LogType == Abstract.LogTypeEnum.HttpClientResponse;
                        entryItem.TotalMillionSeconds = Convert.ToInt32((addtion.HappenTime - entryItem.HappenTime).TotalMilliseconds);
                        entryItem.EndTime = addtion.HappenTime;
                        entryItem.WithEnd = true;
                        entryItem.StatuCode = addtion.StatuCode;
                        entryItem.Content = $"参数：\r\n{entryItem.Content}\r\n返回：\r\n{addtion.Content}";
                        if (addtion.LogType == Abstract.LogTypeEnum.HttpClientException)
                        {
                            entryItem.Message = entryItem.Message + addtion.Message;
                            //entryItem.Content = entryItem.Content + addtion.Content;
                        }
                    }
                    else
                        items.Add(addtion);
                }
                else if(addtion.LogType == Abstract.LogTypeEnum.MethodLeave || addtion.LogType == Abstract.LogTypeEnum.MethodException)
                {
                    var entryItem = log.Addtions.Where(x => x.MethodName == addtion.MethodName && x.ClassName == addtion.ClassName && x.LogType == Abstract.LogTypeEnum.MethodEntry && x.WithEnd == false).FirstOrDefault();
                    if(entryItem != null)
                    {
                        entryItem.Sucess = addtion.LogType == Abstract.LogTypeEnum.MethodLeave;
                        entryItem.TotalMillionSeconds = Convert.ToInt32((addtion.HappenTime - entryItem.HappenTime).TotalMilliseconds);
                        entryItem.EndTime = addtion.HappenTime;
                        entryItem.WithEnd = true;
                        if (!addtion.Content.NullOrEmpty())
                            items.Add(addtion);
                    }
                    else
                        items.Add(addtion);
                }
                else
                {
                    items.Add(addtion);
                }
            }
            log.Addtions = items.ToArray();
            return log;
        }
    }
}
