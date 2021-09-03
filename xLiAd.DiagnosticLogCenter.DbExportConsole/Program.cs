using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.DbExportCore;

namespace xLiAd.DiagnosticLogCenter.DbExportConsole
{
    class Program
    {
        /// <summary>
        /// xLiAd.DiagnosticLogCenter.DbExportConsole.exe --clients acc --url "mongodb://zhanglei20:ab1b0455lgZwL@172.16.101.28:27017/LogCenterTest?authSource=admin&authMechanism=SCRAM-SHA-1" --dropafterbak 0 --maxdate 150d
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            Arguments arg = ArgumentsHelper.BindArguments<Arguments>(args);
            if(arg.Url.NullOrEmpty())
            {
                Console.WriteLine("Url 参数不能为空！");
                return;
            }
            CollectionManager cm = new CollectionManager(arg.Url);
            ICollectionExporter exporter = new CollectionExporter(cm, new TextLog());
            await exporter.Export(new DiagnosticLogCenter.DbExportCore.Settings.ExportSetting()
            {
                ClientNames = (arg.Clients.NullOrEmpty() || arg.Clients == "*") ? null : arg.Clients.Split(','),
                MaxDate = ProcessDate(arg.MaxDate),
                DropAfterBak = arg.DropAfterBak != null &&
                    (arg.DropAfterBak.Equals("true",StringComparison.OrdinalIgnoreCase) || arg.DropAfterBak == "1")
            });
        }

        static DateTime ProcessDate(string date)
        {
            var defaultResult = DateTime.Today.AddMonths(-3);
            if (date.NullOrEmpty())
                return defaultResult;
            if (Regex.IsMatch(date, "^([\\d]{1,3})[d|m]$"))
            {
                var d = date.Substring(0, date.Length - 1).ToInt();
                if (d < 1)
                    return defaultResult;
                if (date.EndsWith("d", StringComparison.OrdinalIgnoreCase))
                    return DateTime.Today.AddDays(-d);
                else
                    return DateTime.Today.AddMonths(-d);
            }
            var result = date.ToDateTime();
            return result ?? defaultResult;
        }
    }
}
