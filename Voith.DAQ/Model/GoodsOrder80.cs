using SqlSugar;
using System;

namespace Voith.DAQ.Model
{
    /// <summary>
    /// 订单信息
    /// </summary>
    class GoodsOrder80
    {
        /// <summary>
        /// 主键自增
        /// </summary>
        [SugarColumn(IsNullable =false ,IsPrimaryKey =true,IsIdentity =true)]
        public int ID { get; set; }

        /// <summary>
        /// 工单号
        /// </summary>
        [SugarColumn(Length = 100)]
        public string ProductionOrderCode { get; set; }

        /// <summary>
        /// 流水号（总成号）
        /// </summary>
        [SugarColumn(Length = 100)]
        public string SerialNumber { get; set; }

        /// <summary>
        /// 物料号
        /// </summary>
        [SugarColumn(Length = 100)]
        public string MaterielCode { get; set; }

        /// <summary>
        /// 托盘号
        /// </summary>
        [SugarColumn(Length = 20)]
        public string PalletCode { get; set; }

        /// <summary>
        /// 产品类型
        /// </summary>
        [SugarColumn(Length = 20)]
        public string ProductType { get; set; }

        /// <summary>
        /// 订单状态(0：初始状态，1：已上线绑定托盘，2已下线)
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int OrderStatus { get; set; }

        /// <summary>
        /// 下线校验结果（0：初始状态未校验，1：已校验合格，2：已校验不合格）
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int CheckResult { get; set; } = 0;

        /// <summary>
        /// 上线时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime OnLineTime { get; set; }

        /// <summary>
        /// 下线时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime OffLineTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(DefaultValue = "GETDATE()")]
        public DateTime LocalTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 扫码类型1 TC/WAG 
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Type1 { get; set; }

        /// <summary>
        /// 扫码类型2 9.9/10.5 
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Type2 { get; set; }
    }
}
