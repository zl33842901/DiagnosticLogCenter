using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.DbExportCore.Settings
{
    public class ProcessItem
    {
        public ProcessItem(CollectionNameModel nameModel, ProcessEnum processEnum)
        {
            NameModel = nameModel;
            ProcessEnum = processEnum;
        }

        public CollectionNameModel NameModel { get; }
        public ProcessEnum ProcessEnum { get; }

        public override string ToString()
        {
            return NameModel.ToString() + "  " + ProcessEnum.ToString();
        }
    }
}
