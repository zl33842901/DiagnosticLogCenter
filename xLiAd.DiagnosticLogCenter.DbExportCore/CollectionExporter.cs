using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.DbExportCore.Settings;

namespace xLiAd.DiagnosticLogCenter.DbExportCore
{
    public class CollectionExporter : ICollectionExporter
    {
        private readonly ICollectionManager collectionManager;
        public CollectionExporter(ICollectionManager collectionManager)
        {
            this.collectionManager = collectionManager;
        }
        private string mongobin => Path.Combine(Directory.GetCurrentDirectory(), "mongodbbin");
        private string exe => Path.Combine(mongobin, "mongoexport.exe");

        public async Task<(List<ProcessItem> completed, List<ProcessItem> errors, string zipfile)> Export(ExportSetting setting)
        {
            var collections = await collectionManager.GetCollections();
            var nameModels = collections.Select(x => CollectionNameModel.CheckName(x))
                .Select(x => new ProcessItem(x, setting.Check(x)))
                .Where(x => x.ProcessEnum != ProcessEnum.None).ToArray();
            string savefolder;
            if (setting.OutputFolder?.Contains(':') ?? false)//因为一定是在 windows 里运行，所以带冒号是绝对路径，不带是相对路径
                savefolder = setting.OutputFolder;
            else
                savefolder = Path.Combine(Directory.GetCurrentDirectory(), setting.OutputFolder);
            List<Tuple<ProcessItem, string>> Completed = new List<Tuple<ProcessItem, string>>();
            List<ProcessItem> Errored = new List<ProcessItem>();
            foreach(var nameModel in nameModels)
            {
                var file = await DoDownload(nameModel.NameModel.ToString(), savefolder);
                if (!File.Exists(file) || new FileInfo(file).Length < 5)
                {
                    Errored.Add(nameModel);
                    continue;
                }
                if (nameModel.ProcessEnum == ProcessEnum.BakAndRemove)
                    await collectionManager.DropCollection(nameModel.ToString());
                Completed.Add(new Tuple<ProcessItem, string>(nameModel, file));
            }
            //压缩
            string result = null;
            if (Completed.Any())
            {
                result = Completed.Where(x => x.Item1.NameModel is LogCollectionNameModel).Select(x => ((LogCollectionNameModel)x.Item1.NameModel).ClientName).Distinct().ToStringBy("-") + "_" + setting.RealMaxDate.ToString("yyyy-MM-dd") + ".zip";
                result = Path.Combine(savefolder, result);
                using (FileStream zipToOpen = new FileStream(result, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                    {
                        foreach (var data in Completed)
                        {
                            var fn = Path.GetFileName(data.Item2);
                            archive.CreateEntryFromFile(data.Item2, fn);
                        }
                    }
                }
                //删除 json
                foreach (var data in Completed)
                    File.Delete(data.Item2);
            }
            return (Completed.Select(x => x.Item1).ToList(), Errored, result);
        }

        private Task<string> DoDownload(string collection, string savefolder)
        {
            var savefile = Path.Combine(savefolder, $"{collection}.json");
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = exe;
            p.StartInfo.WorkingDirectory = mongobin;
            p.StartInfo.Arguments = $" --uri=\"{collectionManager.MongoUrl.Url}\" --collection {collection} --out {savefile}";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序
            p.StandardInput.AutoFlush = true;
            p.WaitForExit();
            p.Close();
            return Task.FromResult(savefile);
        }
    }

    public interface ICollectionExporter
    {
        Task<(List<ProcessItem> completed, List<ProcessItem> errors, string zipfile)> Export(ExportSetting setting);
    }
}
