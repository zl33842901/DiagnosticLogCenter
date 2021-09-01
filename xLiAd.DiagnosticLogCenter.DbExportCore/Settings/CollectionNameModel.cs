using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace xLiAd.DiagnosticLogCenter.DbExportCore.Settings
{
    public abstract class CollectionNameModel
    {
        public static CollectionNameModel CheckName(string collectionName)
        {
            CollectionNameModel result = LogCollectionNameModel.CheckName(collectionName);
            if (result == null)
                result = TraceOrPageIdNameModel.CheckName(collectionName);
            if (result == null)
                result = new OtherNameModel(collectionName);
            return result;
        }
    }

    public class LogCollectionNameModel : CollectionNameModel
    {
        public LogCollectionNameModel(string preffix, string clientName, string envName, DateTime date)
        {
            Preffix = preffix;
            ClientName = clientName;
            EnvName = envName;
            Date = date;
        }

        public string Preffix { get; }
        public string ClientName { get; }
        public string EnvName { get; }
        public DateTime Date { get; }
        public override string ToString()
        {
            return $"{Preffix}-{ClientName}-{EnvName}-{Date:yyyy.MM.dd}";
        }
        public static new LogCollectionNameModel CheckName(string collectionName)
        {
            if (string.IsNullOrEmpty(collectionName))
                return null;
            var ss = collectionName.Split('-');
            if (ss.Length != 4)
                return null;
            var pattern = "^[\\w]+$";
            if (Regex.IsMatch(ss[0], pattern) && Regex.IsMatch(ss[1], pattern) && Regex.IsMatch(ss[2], pattern)
                && Regex.IsMatch(ss[3], "^20[\\d]{2}\\.[\\d]{2}\\.[\\d]{2}$"))
                return new LogCollectionNameModel(ss[0], ss[1], ss[2], DateTime.Parse(ss[3]));
            else
                return null;
        }
    }

    public abstract class TraceOrPageIdNameModel : CollectionNameModel
    {
        public abstract string Name { get; }
        public int Year { get; protected set; }
        public int Month { get; protected set; }
        public int Index { get; protected set; }

        public static new TraceOrPageIdNameModel CheckName(string collectionName)
        {
            if (string.IsNullOrEmpty(collectionName))
                return null;
            var ss = collectionName.Split('-');
            if (ss.Length != 4 && ss.Length != 3)
                return null;
            if (ss[0].Equals("TraceId", StringComparison.OrdinalIgnoreCase))
                return new TraceIdNameModel(ss[1].ToInt(), ss[2].ToInt(), ss.Length == 3 ? 0 : ss[3].ToInt());
            else if (ss[0].Equals("PageId", StringComparison.OrdinalIgnoreCase))
                return new PageIdNameModel(ss[1].ToInt(), ss[2].ToInt(), ss.Length == 3 ? 0 : ss[3].ToInt());
            else
                return null;
        }
        public override string ToString()
        {
            return $"{Name}-{Year}-{Month.ToString().PadLeft(2, '0')}-{Index}";
        }
    }

    public class TraceIdNameModel : TraceOrPageIdNameModel
    {
        public TraceIdNameModel(int year, int month, int index)
        {
            Year = year;
            Month = month;
            Index = index;
        }
        public override string Name => "TraceId";
    }
    public class PageIdNameModel : TraceOrPageIdNameModel
    {
        public PageIdNameModel(int year, int month, int index)
        {
            Year = year;
            Month = month;
            Index = index;
        }
        public override string Name => "PageId";
    }

    public class OtherNameModel : CollectionNameModel
    {
        public OtherNameModel(string name)
        {
            Name = name;
        }
        public string Name { get; }
        public override string ToString()
        {
            return Name;
        }
    }
}
