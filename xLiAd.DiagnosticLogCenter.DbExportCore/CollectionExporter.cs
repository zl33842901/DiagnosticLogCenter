﻿using System;
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
        private readonly IExportLog exportLog;
        public CollectionExporter(ICollectionManager collectionManager, IExportLog exportLog)
        {
            this.collectionManager = collectionManager;
            this.exportLog = exportLog;
        }
        private string mongobin => Path.Combine(Directory.GetCurrentDirectory(), "mongodbbin");
        private string exe => Path.Combine(mongobin, "mongoexport.exe");

        public async Task<(List<ProcessItem> completed, List<ProcessItem> errors, List<string> zipfiles)> Export(ExportSetting setting)
        {
            exportLog?.Info("进入 Export 方法，开始导出");
            exportLog?.Info(setting);
            var collections = await collectionManager.GetCollections();
            var nameModels = collections.Select(x => CollectionNameModel.CheckName(x))
                .Select(x => new ProcessItem(x, setting.Check(x)))
                .Where(x => x.ProcessEnum != ProcessEnum.None).ToArray();
            exportLog?.Info($"需要处理的表数量：{nameModels.Length}");
            string savefolder;
            if (setting.OutputFolder?.Contains(':') ?? false)//因为一定是在 windows 里运行，所以带冒号是绝对路径，不带是相对路径
                savefolder = setting.OutputFolder;
            else
                savefolder = Path.Combine(Directory.GetCurrentDirectory(), setting.OutputFolder);
            exportLog?.Info($"保存路径：{savefolder}");
            List<Tuple<ProcessItem, string>> Completed = new List<Tuple<ProcessItem, string>>();
            List<ProcessItem> Errored = new List<ProcessItem>();
            List<Tuple<ProcessItem, string>> toCompress = new List<Tuple<ProcessItem, string>>();
            long tempBytes = 0;
            int tm = 1;
            List<string> resultZips = new List<string>();
            foreach (var nameModel in nameModels)
            {
                exportLog?.Info($"开始处理表{nameModel.NameModel}");
                var file = await DoDownload(nameModel.NameModel.ToString(), savefolder);
                exportLog?.Info($"表{nameModel.NameModel}下载完毕");
                if (!File.Exists(file) || new FileInfo(file).Length < 5)
                {
                    exportLog?.Info($"未找到文件：{file}");
                    Errored.Add(nameModel);
                    continue;
                }
                else
                {
                    tempBytes += new FileInfo(file).Length;
                }
                if (nameModel.ProcessEnum == ProcessEnum.BakAndRemove)
                {
                    exportLog?.Info($"将要删除表：{nameModel.NameModel}");
                    await collectionManager.DropCollection(nameModel.NameModel.ToString());
                    exportLog?.Info($"表：{nameModel.NameModel}删除完成");
                }
                exportLog?.Info($"表：{nameModel.NameModel}处理完毕");
                var nnm = new Tuple<ProcessItem, string>(nameModel, file);
                Completed.Add(nnm);
                toCompress.Add(nnm);
                if(tempBytes > 20L * 1024 * 1024 * 1024)
                {
                    resultZips.Add(Compress(toCompress, savefolder, setting.RealMaxDate, tm++));
                    toCompress = new List<Tuple<ProcessItem, string>>();
                    tempBytes = 0;
                }
            }
            //压缩
            resultZips.Add(Compress(toCompress, savefolder, setting.RealMaxDate, tm));
            exportLog?.Info($"处理完毕，程序将返回");
            return (Completed.Select(x => x.Item1).ToList(), Errored, resultZips);
        }

        private string Compress(List<Tuple<ProcessItem, string>> completed, string savefolder, DateTime maxDate, int index)
        {
            string result = null;
            if (completed.Any())
            {
                exportLog?.Info($"压缩开始，需要压缩的文件数量：{completed.Count}");
                result = completed.Where(x => x.Item1.NameModel is LogCollectionNameModel).Select(x => ((LogCollectionNameModel)x.Item1.NameModel).ClientName).Distinct().Take(8).ToStringBy("-") + "_" + maxDate.ToString("yyyy-MM-dd") + "_" + index + ".zip";
                result = Path.Combine(savefolder, result);
                using (FileStream zipToOpen = new FileStream(result, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                    {
                        foreach (var data in completed)
                        {
                            var fn = Path.GetFileName(data.Item2);
                            archive.CreateEntryFromFile(data.Item2, fn);
                        }
                    }
                }
                exportLog?.Info($"压缩完毕，将删除JSON文件");
                //删除 json
                foreach (var data in completed)
                    File.Delete(data.Item2);
            }
            return result;
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
        Task<(List<ProcessItem> completed, List<ProcessItem> errors, List<string> zipfiles)> Export(ExportSetting setting);
    }
}
