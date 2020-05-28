using System;

namespace Voith.DAQ.Model
{
    /// <summary>
    /// 工单信息
    /// </summary>
    class ProductionOrder
    {
        /// <summary>
        /// 主键自增
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 工单号
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 物料号
        /// </summary>
        public string MaterielCode { get; set; }

        /// <summary>
        /// 订单数量
        /// </summary>
        public int OrderQty { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime LocalTime { get; set; } = DateTime.Now;
    }
}
