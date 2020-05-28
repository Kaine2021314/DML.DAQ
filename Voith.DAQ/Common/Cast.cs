using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Serialization;

namespace SVW
{
    public class Cast
    {/// <summary>
     /// string型转换为bool型
     /// </summary>
     /// <param name="strValue">要转换的字符串</param>
     /// <param name="defValue">缺省值</param>
     /// <returns>转换后的bool类型结果</returns>
        public static bool StrToBool(object expression, bool defValue)
        {
            if (expression != null)
                return StrToBool(expression, defValue);

            return defValue;
        }

        /// <summary>
        /// string型转换为bool型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的bool类型结果</returns>
        public static bool StrToBool(string expression, bool defValue)
        {
            if (expression != null)
            {
                if (string.Compare(expression, "true", true) == 0)
                    return true;
                else if (string.Compare(expression, "false", true) == 0)
                    return false;
            }
            return defValue;
        }

        /// <summary>
        /// 将对象转换为Int32类型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static int ToIntExt(object expression)
        {
            return ToIntExt(expression, 0);
        }

        /// <summary>
        /// 将对象转换为Int32类型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static int ToIntExt(object expression, int defValue)
        {
            if (expression != null)
                return StrToInt(expression.ToString(), defValue);

            return defValue;
        }

        /// <summary>
        /// 将对象转换为Int32类型,转换失败返回0
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <returns>转换后的int类型结果</returns>
        public static int StrToInt(string str)
        {
            return StrToInt(str, 0);
        }

        /// <summary>
        /// 将字符串转换为guid,如果失败则返回guid.empty
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Guid StrToGuid(string str)
        {
            try
            {
                return Guid.Parse(str);
            }
            catch (Exception)
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// 将对象转换为Int32类型
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static int StrToInt(string str, int defValue)
        {
            if (string.IsNullOrEmpty(str) || str.Trim().Length >= 11 || !Regex.IsMatch(str.Trim(), @"^([-]|[0-9])[0-9]*(\.\w*)?$"))
                return defValue;

            int rv;
            if (Int32.TryParse(str, out rv))
                return rv;

            return Convert.ToInt32(StrToFloat(str, defValue));
        }

        /// <summary>
        /// string型转换为float型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static float StrToFloat(object strValue, float defValue)
        {
            if ((strValue == null))
                return defValue;

            return StrToFloat(strValue.ToString(), defValue);
        }

        /// <summary>
        /// string型转换为float型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static float ObjectToFloat(object strValue, float defValue)
        {
            if ((strValue == null))
                return defValue;

            return StrToFloat(strValue.ToString(), defValue);
        }

        /// <summary>
        /// string型转换为float型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static float ObjectToFloat(object strValue)
        {
            return ObjectToFloat(strValue.ToString(), 0);
        }

        /// <summary>
        /// string型转换为float型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <returns>转换后的int类型结果</returns>
        public static float StrToFloat(string strValue)
        {
            if ((strValue == null))
                return 0;

            return StrToFloat(strValue.ToString(), 0);
        }

        /// <summary>
        /// string型转换为float型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static float StrToFloat(string strValue, float defValue)
        {
            if ((strValue == null) || (strValue.Length > 10))
                return defValue;

            float intValue = defValue;
            if (strValue != null)
            {
                bool IsFloat = Regex.IsMatch(strValue, @"^([-]|[0-9])[0-9]*(\.\w*)?$");
                if (IsFloat)
                    float.TryParse(strValue, out intValue);
            }
            return intValue;
        }


        /// <summary>
        /// string型转换为Decimal型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的Decimal类型结果</returns>
        public static decimal StrToDecimal(object strValue, decimal defValue)
        {
            if ((strValue == null))
                return defValue;

            return StrToDecimal(strValue.ToString(), defValue);
        }

        /// <summary>
        /// string型转换为Decimal型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的Decimal类型结果</returns>
        public static decimal ObjectToDecimal(object strValue, decimal defValue)
        {
            if ((strValue == null))
                return defValue;

            return StrToDecimal(strValue.ToString(), defValue);
        }

        /// <summary>
        /// string型转换为float型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的Decimal类型结果</returns>
        public static decimal ObjectToDecimal(object strValue)
        {
            return ObjectToDecimal(strValue.ToString(), 0);
        }

        /// <summary>
        /// string型转换为Decimal型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <returns>转换后的Decimal类型结果</returns>
        public static decimal StrToDecimal(string strValue)
        {
            if ((strValue == null))
                return 0;

            return StrToDecimal(strValue.ToString(), 0);
        }

        /// <summary>
        /// string型转换为Decimal型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static decimal StrToDecimal(string strValue, decimal defValue)
        {
            if ((strValue == null) || (strValue.Length > 20))
                return defValue;

            decimal intValue = defValue;
            if (strValue != null)
            {
                //bool IsFloat = Regex.IsMatch(strValue, @"^([-]|[0-9])[0-9]*(\.\w*)?$");
                //if (IsFloat)
                decimal.TryParse(strValue, out intValue);
            }
            return intValue;
        }



        /// <summary>
        /// 将对象转换为日期时间类型
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static DateTime StrToDateTime(string str, DateTime defValue)
        {
            if (!string.IsNullOrEmpty(str))
            {
                DateTime dateTime;
                if (DateTime.TryParse(str, out dateTime))
                    return dateTime;
            }
            return defValue;
        }

        /// <summary>
        /// 将对象转换为日期时间类型
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <returns>转换后的int类型结果</returns>
        public static DateTime StrToDateTime(string str)
        {
            return StrToDateTime(str, DateTime.Now);
        }

        /// <summary>
        /// 将对象转换为日期时间类型
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns>转换后的int类型结果</returns>
        public static DateTime ObjectToDateTime(object obj)
        {
            return StrToDateTime(obj.ToString());
        }

        /// <summary>
        /// 将对象转换为日期时间类型
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static DateTime ObjectToDateTime(object obj, DateTime defValue)
        {
            return StrToDateTime(obj.ToString(), defValue);
        }

        ///// <summary>
        ///// 字符串转成整型数组
        ///// </summary>
        ///// <param name="idList">要转换的字符串</param>
        ///// <returns>转换后的int类型结果</returns>
        //public static int[] StringToIntArray(string idList)
        //{
        //    return StringToIntArray(idList, -1);
        //}

        ///// <summary>
        ///// 字符串转成整型数组
        ///// </summary>
        ///// <param name="idList">要转换的字符串</param>
        ///// <param name="defValue">缺省值</param>
        ///// <returns>转换后的int类型结果</returns>
        //public static int[] StringToIntArray(string idList, int defValue)
        //{
        //    if (string.IsNullOrEmpty(idList))
        //        return null;
        //    string[] strArr = Utils.SplitString(idList, ",");
        //    int[] intArr = new int[strArr.Length];
        //    for (int i = 0; i < strArr.Length; i++)
        //        intArr[i] = StrToInt(strArr[i], defValue);

        //    return intArr;
        //}


        //public static string ObjectToXml(IList data)
        //{
        //    string xml = "";
        //    if (data == null || data.Count == 0)
        //    {
        //        return xml;
        //    }
        //    using (StringWriterWithEncoding sw = new StringWriterWithEncoding(Encoding.UTF8))
        //    //using (StringWriter sw = new StringWriter())
        //    {
        //        XmlSerializer xz = new XmlSerializer(data.GetType());

        //        xz.Serialize(sw, data);
        //        xml = sw.ToString();
        //    }
        //    return xml;
        //}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="data">需要转换的数据对象</param>
        /// <returns>XML字符</returns>
        /// 

        /// <summary>
        /// 对象序列号为XML
        /// </summary>
        /// <typeparam name="T">序列化的对象类型</typeparam>
        /// <param name="obj">序列化的对象</param>
        /// <param name="encoding">编码，空为UTF-8</param>
        /// <param name="rootAttributeName">根节点名称</param>
        /// <returns>序列化后的XML字符串</returns>
        public static string ObjectSerializeXml<T>(T obj, Encoding encoding = null, string rootAttributeName = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer serializer;
                if (string.IsNullOrWhiteSpace(rootAttributeName))
                {
                    serializer = new XmlSerializer(typeof(T));
                }
                else
                {
                    serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(rootAttributeName));
                }
                serializer.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(ms, encoding))
                {
                    return reader.ReadToEnd();
                }

            }
        }

        /// <summary>
        /// 对象转换为字符串
        /// </summary>
        /// <param name="inputVal">输入值</param>
        /// <param name="defValue">转换不成功默认值</param>
        /// <returns></returns>
        public static string ObjectToStr(object inputVal, string defValue = "")
        {
            if (inputVal == null || inputVal == DBNull.Value)
            {
                return defValue;
            }

            string strVal = Convert.ToString(inputVal);
            if (string.IsNullOrWhiteSpace(strVal))
            {
                return defValue;
            }

            return strVal;
        }

        /// <summary>
        /// 转换为字符串，当为空时候，返回默认值
        /// </summary>
        /// <param name="inputVal">输入值</param>
        /// <param name="defValue">默认值</param>
        /// <returns></returns>
        public static string ToStringExt(object inputVal, string defValue = "")
        {
            if (inputVal == null || inputVal == DBNull.Value)
            {
                return defValue;
            }

            string returnVal = inputVal.ToString();
            if (string.IsNullOrWhiteSpace(returnVal))
            {
                return defValue;
            }

            return returnVal;
        }


        /// <summary>
        /// 转换为字符串，当为空时候，返回默认值
        /// </summary>
        /// <param name="inputVal">输入值</param>
        /// <param name="defValue">默认值</param>
        /// <returns></returns>
        public static string ToStringExt(string inputVal, string defValue = "")
        {
            if (string.IsNullOrWhiteSpace(inputVal))
            {
                return defValue;
            }

            return inputVal;
        }

        /// <summary>
        /// 转为为数据库可接受的参数值(Null或者Empty时候转换为DBNull.Value)
        /// </summary>
        /// <param name="inputVal">输入值</param>
        /// <returns></returns>
        public static object ToDBValue(object inputVal)
        {
            if (inputVal == null || string.IsNullOrWhiteSpace(inputVal.ToString()))
            {
                return DBNull.Value;
            }
            return inputVal;
        }

    }
}