using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SVW
{
    public static class StringExtenstions
    {
        /// <summary>
        /// 转换成字符串(转换不成功可设置默认值)
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <param name="defaultVal">默认值，默认为空字符串""</param>
        /// <returns></returns>
        public static string ToStringExt(this object input, string defaultVal = "")
        {
            return Cast.ToStringExt(input, defaultVal);
        }

        /// <summary>
        /// 转换成GUID(转换不成功可设置默认值)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="DefaultGuidString"></param>
        /// <returns></returns>
        public static Guid ToGuidExt(this string input, string defaultVal)
        {
            Guid id = Guid.Parse(defaultVal);
            Guid.TryParse(input, out id);
            return id;
        }

        /// <summary>
        /// 转换成Int类型(转换不成功可设置默认值)
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <param name="defaultVal">默认值，默认为0</param>
        /// <returns></returns>
        public static int ToIntExt(this string input, int defaultVal = 0)
        {
            return Cast.ToIntExt(input, defaultVal);
        }
        /// <summary>
        /// 将对象转换为Int32类型
        /// </summary>
        /// <param name="expression">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static int StrToInt(this object expression, int defValue)
        {
            return Cast.ToIntExt(expression, defValue);
        }
        /// <summary>
        /// 将字符串转换为Int32类型
        /// </summary>
        /// <param name="expression">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static int StrToInt(string expression, int defValue)
        {
            return Cast.StrToInt(expression, defValue);
        }
        /// <summary>
        /// 转换成decimal类型(转换不成功可设置默认值)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public static decimal ToDecimalExt(this string input, int defaultVal = 0)
        {
            decimal value = 0;
            decimal.TryParse(input, out value);
            return value;
        }
        /// <summary>
        /// float(转换不成功可设置默认值)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public static float ToFloatExt(this string input)
        {
            return Cast.StrToFloat(input);
        }
        /// <summary>
        /// 替换新的字符串
        /// </summary>
        /// <param name="inputStr">输入字符串</param>
        /// <param name="replaceStr">替换的字符</param>
        /// <param name="startIndex">替换起始索引</param>
        /// <param name="endIndex">替换结束索引</param>
        /// <returns></returns>
        public static string Replace(this string inputStr, string replaceStr, int startIndex, int endIndex)
        {
            int strLen = inputStr.Length;
            if (strLen <= startIndex + 1)
            {
                return inputStr;
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < strLen; i++)
            {
                if (i >= startIndex && i <= endIndex)
                {
                    sb.Append(replaceStr);
                }
                else
                {
                    sb.Append(inputStr[i]);

                }
            }

            return sb.ToString();
        }
    }
}