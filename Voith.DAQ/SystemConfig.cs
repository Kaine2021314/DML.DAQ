using Newtonsoft.Json.Linq;
using System;

namespace Voith.DAQ
{
    public class SystemConfig
    {
        public static bool SystemInitError = false;
        public class OrderInfo
        {
            public string ProductionOrderCode = "";
            public string MaterielCode = "";
            public string Type1 = "";
            public string Type2 = "";
            public string SerialNumber = "";
            public int OnlineCount = 0;
            public int OfflineCount = 0;
            public int OKCount = 0;
            public int NOKCount = 0;
            public int PCout = 0;
        }
        public static OrderInfo orderInfo = new OrderInfo();
        public static string DBStringCurve = "";
        public static DateTime tagdate = DateTime.Now;
        public static int tagcount = 1;

        /// <summary>
        /// 控制DB块
        /// </summary>
        public static int ControlDB = 1044;

        /// <summary>
        /// DT控制DB块
        /// </summary>
        public static int DTControlDB = 1045;

        /// <summary>
        /// 数据DB块
        /// </summary>
        public static int DataDB = 1046;

        /// <summary>
        /// 当前登录用户
        /// </summary>
        public static string LoginUser { get; set; }

        public static JArray DFTag;

        public static JArray StationList;
        public static JArray MaterielCode;
        public static JArray Type1List;
        public static JArray Type2List;

        public static byte[] GetProductionTypes(string t0, string t1, string t2)
        {
            byte[] r = new byte[3];
            int typeAdd = 1;
            for (int i = 0; i < MaterielCode.Count; i++)
            {
                if (t0 == MaterielCode[i].ToString())
                {
                    if(t0 == "153008271120CN")
                        typeAdd = 7;
                    r[0] = (byte)(i + typeAdd);
                    break;
                }
            }
            for (int i = 0; i < Type1List.Count; i++)
            {
                if (t1 == Type1List[i].ToString())
                {
                    r[1] = (byte)(i + 1);
                    break;
                }
            }
            for (int i = 0; i < Type2List.Count; i++)
            {
                if (t2 == Type2List[i].ToString())
                {
                    r[2] = (byte)(i + 1);
                    break;
                }
            }
            return r;
        }

        public static byte GetMaterielCodeType(string t0)
        {
            byte r = 1;
            for (int i = 0; i < MaterielCode.Count; i++)
            {
                if (t0 == MaterielCode[i].ToString())
                {
                    string TCode = t0.Substring(0, 9);

                    if (TCode == "153008411" ||
                        TCode == "153008448" ||
                        TCode == "153008646")
                    {
                        r = 31;
                    }
                    else
                        r = 1;
                    break;
                }
            }
            return r;
        }

        public static bool HasItem(JArray ja, string s)
        {
            bool b = false;

            if (ja != null)
            {
                foreach (JValue j in ja)
                {
                    if (j.ToString() == s)
                    {
                        b = true;
                        break;
                    }
                }
            }

            return b;
        }

        public static string GetItem(JArray ja, string s)
        {
            string r = null;

            if (ja != null)
            {
                foreach (JObject j in ja)
                {
                    if (j.ContainsKey(s))
                    {
                        r = j[s].ToString();
                        break;
                    }
                }
            }

            return r;
        }
    }
}
