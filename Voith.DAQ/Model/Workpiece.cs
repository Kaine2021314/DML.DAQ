namespace Voith.DAQ.Model
{
    public class Workpiece
    {
        /// <summary>
        /// 托盘号+
        /// </summary>
        public string TrayCode { get; set; }

        /// <summary>
        /// 产品序列号+
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// 产品型号+
        /// </summary>
        public string ProductTypeCode { get; set; }

        /// <summary>
        /// 物料类型++
        /// </summary>
        public string MaterielCode { get; set; }
        /// <summary>
        /// 动定轮类型++
        /// </summary>
        public string Type1 { get; set; }
        /// <summary>
        /// 节流板类型++
        /// </summary>
        public string Type2 { get; set; }

        /// <summary>
        /// 工位名称
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 工位序号 工位匹配使用
        /// </summary>
        public int StationIndex { get; set; }

        /// <summary>
        /// DB地址1
        /// </summary>
        public int DBAddr1 { get; set; }

        //StartAddr
        public int StartAddr { get; set; }
        //数据StartAddr
        public int DStartAddr { get; set; }
        //托盘状态 0 正常 1 空托盘 2 不合格托盘  +356
        public short TrayStatus { get; set; }

        public int EKSStartAddr { get; set; }
    }
}
