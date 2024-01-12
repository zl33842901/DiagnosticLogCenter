using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.AgentFramework
{
    public class LocalCacheHelper
    {
        private string realSavePath;
        private const string tryFileName = "try.txt";
        private static object forWriteLock = new object();
        public LocalCacheHelper(string savePath = null)
        {
            bool success = false;
            if (!savePath.NullOrEmpty())
                success = TrySavePath(savePath);
            if (!success)
                success = TrySavePath(Path.Combine(Directory.GetCurrentDirectory(), "DlcCache"));
            if (!success)
                success = TrySavePath("C:\\DlcCache");
            if (!success)
                success = TrySavePath("/DlcCache");
        }

        private bool TrySavePath(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch { return false; }
            var filePath = Path.Combine(path, tryFileName);
            lock (forWriteLock)
            {
                try
                {
                    StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8);
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    sw.Close();
                    File.Delete(filePath); //必须能删掉，才可以使用。
                    if (File.Exists(filePath))
                        return false;
                    realSavePath = path;
                    return true;
                }
                catch (Exception ex) { Console.WriteLine($"Dlc在检查 {path} 位置是否可用时发生异常：" + ex.Message); return false; }
            }
        }

        public bool WriteLog(List<LogEntity> log)
        {
            if (realSavePath.NullOrEmpty())
                return false;
            var filePath = Path.Combine(realSavePath, DateTime.Now.ToString("yyyy-MM-dd_HH") + ".dlc");
            try
            {
                StreamWriter sw = new StreamWriter(filePath, true, Encoding.UTF8);
                sw.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(log));
                sw.Close();
                return true;
            }
            catch { return false; }
        }

        public LogCacheModel PeekClearLog()
        {
            if (realSavePath.NullOrEmpty())
                return null;
            var file = new DirectoryInfo(realSavePath).GetFiles("*.dlc").OrderBy(x => x.CreationTime).FirstOrDefault();
            if (file != null)
            {
                List<List<LogEntity>> result = new List<List<LogEntity>>();
                StreamReader sr = new StreamReader(file.FullName, Encoding.UTF8);
                string line;
                while (!(line = sr.ReadLine()).NullOrEmpty())
                {
                    result.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<List<LogEntity>>(line));
                }
                sr.Close();
                Action action = () => File.Delete(file.FullName);
                return new LogCacheModel() { Logs = result, action = action };
            }
            return null;
        }
    }

    public class LogCacheModel
    {
        public List<List<LogEntity>> Logs { get; set; }

        public Action action { get; set; }
    }
}
