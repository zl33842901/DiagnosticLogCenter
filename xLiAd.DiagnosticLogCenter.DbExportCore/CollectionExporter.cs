using System;
using System.Collections.Generic;
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

        public async Task Export(ExportSetting setting)
        {
            var collections = await collectionManager.GetCollections();
            var nameModels = collections.Select(x => CollectionNameModel.CheckName(x))
                .Select(x => new ProcessItem(x, setting.Check(x)))
                .Where(x => x.ProcessEnum != ProcessEnum.None).ToArray();
        }
    }

    public interface ICollectionExporter
    {
        Task Export(ExportSetting setting);
    }
}
