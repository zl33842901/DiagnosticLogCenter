using System;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.DbExportCore;
using Xunit;

namespace xLiAd.DiagnosticeLogCenter.DbExportCore.Test
{
    public class UnitTest1
    {
        string url = "mongodb://zhanglei20:ab1b0455lgZwL@172.16.101.28:27017/LogCenterTest?authSource=admin&authMechanism=SCRAM-SHA-1";
        string urlprd = "mongodb://zhanglei20:ab1b0455lgZwL@172.16.250.149:27017/LogCenter?authSource=admin&authMechanism=SCRAM-SHA-1";
        [Fact]
        public async Task Test1()
        {
            CollectionManager cm = new CollectionManager(url);
            var rst = await cm.GetCollections();
        }
        [Fact]
        public async Task Test2()
        {
            CollectionManager cm = new CollectionManager(urlprd);
            ICollectionExporter exporter = new CollectionExporter(cm, null);
            await exporter.Export(new DiagnosticLogCenter.DbExportCore.Settings.ExportSetting()
            {
                ClientNames = null,
                MaxDate = DateTime.Today.AddMonths(-3),
                DropAfterBak = true
            });
        }
    }
}
