using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.MongoEx.Entities;

namespace xLiAd.DiagnosticLogCenter.UserInterface.Models
{
    public partial class LogAdditionItem
    {
        /// <summary>
        /// 计算字段，本步骤是否成功
        /// </summary>
        public bool Sucess { get; set; }
        /// <summary>
        /// 计算字段，本步骤用时
        /// </summary>
        public int TotalMillionSeconds { get; set; }
        /// <summary>
        /// 本步骤完成时间
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 找到了对应的END或 EXCEPTION
        /// </summary>
        public bool WithEnd { get; set; } = false;


        //WithEnd 为真的，需要计算下列几个字段

        /// <summary>
        /// 显示的总长度  百分比
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int StartPoint { get; set; }

        public int EndPoint => StartPoint + Length;

        public string Color
        {
            get
            {
                switch (this.LogType)
                {
                    case Abstract.LogTypeEnum.SqlBefore:
                        return Sucess ? "006400" : "FF3030";
                    case Abstract.LogTypeEnum.DapperExSqlBefore:
                        return Sucess ? "006400" : "FF3030";
                    case Abstract.LogTypeEnum.HttpClientRequest:
                        return Sucess ? "CD6600" : "FF3030";
                    default:
                        return null;
                }
            }
        }

        public string TypeString
        {
            get
            {
                switch (this.LogType)
                {
                    case Abstract.LogTypeEnum.SqlBefore:
                        return "执行Sql";
                    case Abstract.LogTypeEnum.DapperExSqlBefore:
                        return "执行DapperEx";
                    case Abstract.LogTypeEnum.HttpClientRequest:
                        return "对外请求";
                    default:
                        return null;
                }
            }
        }
    }

    /// <summary>
    /// 这里的属性全是在前端显示需要的 计算出来的（一个guid 以一根棍的方式显示）
    /// </summary>
    public partial class Log : EntityModel, ICliEnvDate
    {
        /// <summary>
        /// 在第几行显示
        /// </summary>
        public int Line { get; set; }
        /// <summary>
        /// 显示的总长度  百分比
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int StartPoint { get; set; }

        public int EndPoint => StartPoint + Length;
    }
}
