//using HslCommunication;
//using HslCommunication.Profinet.Siemens;
using S7.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using S7.Net.Types;
using System.Text;

namespace Voith.DAQ.Common
{
    /// <summary>
    /// PLC访问基类
    /// </summary>
    public class PlcHelperS7
    {
        /// <summary>
        /// PLC地址，动态获取
        /// </summary>
        private static string PlcIp
        {
            get
            {
                var sr = new StreamReader("Config.json");
                var configJson = (JObject)JsonConvert.DeserializeObject(sr.ReadToEnd());
                return configJson["PlcIpAddress"]?.ToString();
            }
        }

        /// <summary>
        /// 静态读写类
        /// </summary>
        private static Plc _siemensTcpNet { get; set; }

        /// <summary>
        /// PLC访问实例
        /// </summary>
        private static Plc SiemensTcpNet
        {
            get
            {
                if (_siemensTcpNet == null)
                {
                    _siemensTcpNet = new Plc(CpuType.S71500, PlcIp, 0, 1);

                    _siemensTcpNet.Open();
                    LogHelper.Info(
                        true
                        ? $"与PLC：{PlcIp}连接成功！"
                        : $"与PLC：{PlcIp}连接失败！"
                        );
                }

                return _siemensTcpNet;
            }
        }

        /// <summary>
        /// 从指定地址读取指定长度的Byte数组
        /// </summary>
        /// <param name="db"></param>
        /// <param name="startByteAdr"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static byte[] ReadBytes(int db, int startByteAdr, int count)
        {
            try
            {
                var result = _siemensTcpNet.ReadBytes(DataType.DataBlock, db, startByteAdr, (ushort)count);
                if (result != null && result.Length == count)
                {
                    return result;
                }

                LogHelper.Info($"ReadBytes失败：DB块：{db}，开始地址：{startByteAdr}，长度：{count}");
                Thread.Sleep(300);
            }
            catch (Exception e)
            {
                LogHelper.Error(e, $"ReadBytes出错：DB块：{db}，开始地址：{startByteAdr}，长度：{count}");
                Thread.Sleep(300);
            }
            return null;
        }

        /// <summary>
        /// 从指定地址读取指定长度的T类型数组
        /// </summary>
        /// <typeparam name="T">读取数据类型</typeparam>
        /// <param name="db">DB块</param>
        /// <param name="startByteAdr">开始地址</param>
        /// <param name="varCount">长度</param>
        /// <param name="bitAdr">比特位</param>
        /// <returns></returns>
        public static T[] Read<T>(int db, int startByteAdr, int varCount = 1, byte bitAdr = 0)
        {
            try
            {
                if (typeof(T) == typeof(bool))
                {
                    var result = SiemensTcpNet.Read("DB" + db + ".DBX" + startByteAdr + "." + bitAdr);
                    if (result != null)
                    {
                        return new[] { (T)(object)result};
                    }
                    else
                    {
                        LogHelper.Info(
                            $"Read{typeof(T).Name}失败：DB块：{db}，开始地址：{startByteAdr}，长度：{varCount}，Bit位：{bitAdr}");
                        Thread.Sleep(300);
                        //goto Start;
                    }
                }

                else if (typeof(T) == typeof(short))
                {
                    var result = SiemensTcpNet.ReadBytes(DataType.DataBlock, db, startByteAdr, varCount * 2);
                    if (result != null)
                    {
                        List<short> vlst = new List<short>();
                        for (int k = 1; k <= varCount; k++)
                        {
                            int saddr = startByteAdr + GetVarLen("Int", k - 1);
                            short v = (short)GetValue(VarType.Int, varCount, saddr, bitAdr, result);
                            vlst.Add(v);
                        }
                        return (T[])(object)vlst.ToArray();
                    }
                    else
                    {
                        LogHelper.Info(
                            $"Read{typeof(T).Name}失败：DB块：{db}，开始地址：{startByteAdr}，长度：{varCount}");
                        Thread.Sleep(300);
                        //goto Start;
                    }
                }

                else if (typeof(T) == typeof(int))
                {
                    var result = SiemensTcpNet.ReadBytes(DataType.DataBlock, db, startByteAdr, varCount * 4);
                    if (result != null)
                    {
                        List<int> vlst = new List<int>();
                        for (int k = 1; k <= varCount; k++)
                        {
                            int saddr = startByteAdr + GetVarLen("DInt", k - 1);
                            int v = (int)GetValue(VarType.Int, varCount, saddr, bitAdr, result);
                            vlst.Add(v);
                        }
                        return (T[])(object)vlst.ToArray();
                    }
                    else
                    {
                        LogHelper.Info(
                            $"Read{typeof(T).Name}失败：DB块：{db}，开始地址：{startByteAdr}，长度：{varCount}，Bit位：{bitAdr}");
                        Thread.Sleep(300);
                        //goto Start;
                    }
                }

                else if (typeof(T) == typeof(float))
                {
                    var result = SiemensTcpNet.ReadBytes(DataType.DataBlock, db, startByteAdr, varCount * 4);
                    if (result != null)
                    {
                        List<float> vlst = new List<float>();
                        for (int k = 1; k <= varCount; k++)
                        {
                            int saddr = startByteAdr + GetVarLen("Real", k - 1);
                            float v = (float)GetValue(VarType.Real, varCount, saddr, bitAdr, result);
                            vlst.Add(v);
                        }
                        return (T[])(object)vlst.ToArray();
                    }
                    else
                    {
                        LogHelper.Info(
                            $"Read{typeof(T).Name}失败：DB块：{db}，开始地址：{startByteAdr}，长度：{varCount}，Bit位：{bitAdr}");
                        Thread.Sleep(300);
                        //goto Start;
                    }
                }

                else if (typeof(T) == typeof(string))
                {
                    var result = SiemensTcpNet.ReadBytes(DataType.DataBlock, db, startByteAdr, varCount);
                    if (result != null)
                    {
                        var str = (string)GetValue(VarType.String, varCount, startByteAdr, bitAdr, result);
                        return new[] { (T)(object)str };
                    }
                    else
                    {
                        LogHelper.Info(
                            $"Read{typeof(T).Name}失败：DB块：db，开始地址：{startByteAdr}，长度：{varCount}");
                        Thread.Sleep(300);
                        //goto Start;
                    }
                }

                //else if (typeof(T) == typeof(DateTime))
                //{
                //    var result = SiemensTcpNet.Read("DB" + db + "." + startByteAdr, 8);
                //    if (result.IsSuccess)
                //    {
                //        return new[] { (T)(object)PlcConvert.GetLDTAt(result.Content, 0) };
                //    }
                //    else
                //    {
                //        LogHelper.Info(
                //            $"Read{typeof(T).Name}失败：{result.ToMessageShowString()}\nDB块：db，开始地址：{startByteAdr}，长度：{varCount}，Bit位：{bitAdr}");
                //        Thread.Sleep(300);
                //        //goto Start;
                //    }
                //}

                else
                {
                    var result = SiemensTcpNet.Read("DB" + db + ".DBB" + startByteAdr);
                    if (result != null)
                    {
                        return new[] { (T)(object)result };
                    }
                    else
                    {
                        LogHelper.Info(
                            $"Read{typeof(T).Name}失败：DB块：db，开始地址：{startByteAdr}，长度：{varCount}");
                        Thread.Sleep(300);
                        //goto Start;
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e, $"ReadBytes出错：DB块：db，开始地址：{startByteAdr}，长度：{varCount}");
                Thread.Sleep(300);
                //goto Start;
            }
            return null;
        }

        /// <summary>
        /// 写入字节数据
        /// </summary>
        /// <param name="db"></param>
        /// <param name="startByteAdr"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool WriteBytes(int db, int startByteAdr, byte[] value)
        {
            try
            {
                SiemensTcpNet.WriteBytes(DataType.DataBlock, db, startByteAdr, value);
                return true;
            }
            catch (Exception e)
            {
                LogHelper.Error(e, $"WriteBytes出错：DB块：db，开始地址：{startByteAdr}");
                Thread.Sleep(300);
                return false;
            }
        }

        /// <summary>
        /// 向PLC写入指定数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="startByteAdr"></param>
        /// <param name="value"></param>
        /// <param name="bitAdr"></param>
        /// <returns></returns>
        public static bool Write<T>(int db, int startByteAdr, T value, int bitAdr = -1)
        {
            try
            {
                if (typeof(T) == typeof(bool))
                {
                    SiemensTcpNet.WriteBit(DataType.DataBlock, db, startByteAdr, bitAdr, (bool)(object)value);
                    LogHelper.Info(
                        $"Write{typeof(T).Name}失败：DB块：db，开始地址：{startByteAdr}，数据：{value}，Bit位：{bitAdr}");
                    Thread.Sleep(300);
                }

                else if (typeof(T) == typeof(short))
                {
                    byte[] data = GetValueBytes("Int", ((short)(object)value).ToString());
                    SiemensTcpNet.WriteBytes(DataType.DataBlock, db, startByteAdr, data);
                    LogHelper.Info(
                        $"Write{typeof(T).Name}失败：DB块：db，开始地址：{startByteAdr}，数据：{value}");
                    Thread.Sleep(300);
                    //goto Start;
                }

                else if (typeof(T) == typeof(int))
                {
                    byte[] data = GetValueBytes("DInt", ((short)(object)value).ToString());
                    SiemensTcpNet.WriteBytes(DataType.DataBlock, db, startByteAdr, data);
                    LogHelper.Info(
                        $"Write{typeof(T).Name}失败：DB块：db，开始地址：{startByteAdr}，数据：{value}，Bit位：{bitAdr}");
                    Thread.Sleep(300);
                    //goto Start;
                }

                else if (typeof(T) == typeof(float))
                {
                    byte[] data = GetValueBytes("Real", ((short)(object)value).ToString());
                    SiemensTcpNet.WriteBytes(DataType.DataBlock, db, startByteAdr, data);
                    LogHelper.Info(
                        $"Write{typeof(T).Name}失败：DB块：db，开始地址：{startByteAdr}，数据：{value}，Bit位：{bitAdr}");
                    Thread.Sleep(300);
                    //goto Start;
                }

                else if (typeof(T) == typeof(string))
                {
                    byte[] vArr = Encoding.ASCII.GetBytes((string)(object)value);
                    byte[] vArrS = new byte[vArr.Length + 1];
                    vArrS[0] = (byte)vArr.Length;
                    Array.Copy(vArr, 0, vArrS, 1, vArr.Length);
                    SiemensTcpNet.WriteBytes(DataType.DataBlock, db, startByteAdr, vArrS);
                    LogHelper.Info(
                        $"Write{typeof(T).Name}失败：DB块：db，开始地址：{startByteAdr}，数据：{value}，Bit位：{bitAdr}");
                    Thread.Sleep(300);
                    //goto Start;
                }

                else
                {
                    SiemensTcpNet.WriteBytes(DataType.DataBlock, db, startByteAdr, new byte[] { (byte)(object)value });
                    LogHelper.Info(
                        $"Write{typeof(T).Name}失败：DB块：db，开始地址：{startByteAdr}，数据：{value}，Bit位：{bitAdr}");
                    Thread.Sleep(300);
                    //goto Start;
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e, $"ReadBytes出错：DB块：db，开始地址：{startByteAdr}，数据：{value}");
                Thread.Sleep(300);
                //goto Start;
            }
            return false;
        }


        static int GetVarLen(string VarType, int Len)
        {
            //if (Len == 0)
            //    Len++;
            int vLen = 2;
            switch (VarType)
            {
                case "Bit":
                    vLen = Len / 8;
                    vLen += Len % 8 > 0 ? 1 : 0;
                    break;
                case "Byte":
                    vLen = 1 * Len;
                    break;
                case "Word":
                case "Int":
                    vLen = 2 * Len;
                    break;
                case "DWord":
                case "DInt":
                case "Real":
                    vLen = 4 * Len;
                    break;
                case "String":
                    vLen = Len;
                    break;
                default:
                    break;
            }

            return vLen;
        }

        static object GetValue(VarType varType, int Length, int StartAddr, int bitAdr,
           byte[] bytes)
        {
            object v = null;
            if (bytes == null) return v;

            switch (varType)
            {
                case VarType.Byte:
                    if (StartAddr < bytes.Length)
                    {
                        v = bytes[StartAddr];
                    }
                    else
                    {
                        v = (byte)0;
                        //LogR.Logger.Info("GGG:Byte,0");
                    }
                    break;
                case VarType.Word:
                    if (StartAddr < bytes.Length - 1)
                    {
                        v = S7.Net.Types.Word.FromByteArray(new byte[] { bytes[StartAddr],
                        bytes[StartAddr + 1] });
                    }
                    else
                    {
                        v = (ushort)0;
                        //LogR.Logger.Info("GGG:Word,0");
                    }
                    break;
                case VarType.Int:
                    if (StartAddr < bytes.Length - 1)
                    {
                        v = S7.Net.Types.Int.FromByteArray(new byte[] { bytes[StartAddr],
                        bytes[StartAddr + 1] });
                    }
                    else
                    {
                        v = (short)0;
                        //LogR.Logger.Info("GGG:Int,0");
                    }
                    break;
                case VarType.DWord:
                    if (StartAddr < bytes.Length - 3)
                    {
                        v = S7.Net.Types.DWord.FromByteArray(new byte[] { bytes[StartAddr],
                        bytes[StartAddr + 1], bytes[StartAddr + 2], bytes[StartAddr + 3] });
                    }
                    else
                    {
                        v = (uint)0;
                        //LogR.Logger.Info("GGG:DWord,0");
                    }
                    break;
                case VarType.DInt:
                    if (StartAddr < bytes.Length - 3)
                    {
                        v = S7.Net.Types.DInt.FromByteArray(new byte[] { bytes[StartAddr],
                        bytes[StartAddr + 1], bytes[StartAddr + 2], bytes[StartAddr + 3] });
                    }
                    else
                    {
                        v = 0;
                        //LogR.Logger.Info("GGG:DInt,0");
                    }
                    break;
                case VarType.Real:
                    if (StartAddr < bytes.Length - 3)
                    {
                        v = S7.Net.Types.Double.FromByteArray(new byte[] { bytes[StartAddr],
                        bytes[StartAddr + 1], bytes[StartAddr + 2], bytes[StartAddr + 3] });
                        //LogR.Logger.Info("Real77777777799999->" + bytes[StartAddr].ToString() + "->" +
                        //bytes[StartAddr + 1].ToString() + "->" +
                        //bytes[StartAddr + 2].ToString() + "->" +
                        //bytes[StartAddr + 3].ToString());
                        v = ((double)v).ToString("0.00");
                        //LogR.Logger.Info("Real77777777799999 V->" + v.ToString());
                    }
                    else
                    {
                        v = 0.0;
                        //LogR.Logger.Info("GGG:Real,0");
                    }
                    break;
                case VarType.String:
                    if (StartAddr + Length <= bytes.Length)
                    {
                        int strLen = bytes[StartAddr + 1];
                        byte[] temp = new byte[strLen];
                        Array.Copy(bytes, StartAddr + 2, temp, 0, strLen);
                        string strValue = string.Empty;
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (temp[i] != 0)
                            {
                                strValue += Convert.ToChar(temp[i]);
                            }
                            else
                            {
                                break;
                            }
                        }
                        v = strValue;//S7Type.String.FromByteArray(temp).Trim();
                    }
                    else
                    {
                        v = "0";
                        //LogR.Logger.Info("GGG:String,0");
                    }
                    break;
                //case VarType.Timer:
                //    if (varCount == 1)
                //        return S7Type.Timer.FromByteArray(bytes);
                //    else
                //        return S7Type.Timer.ToArray(bytes);
                //case VarType.Counter:
                //    if (varCount == 1)
                //        return S7Type.Counter.FromByteArray(bytes);
                //    else
                //        return S7Type.Counter.ToArray(bytes);
                case VarType.Bit:
                    if (bitAdr <= 7 && StartAddr < bytes.Length)
                    {
                        BitArray bitArr = new BitArray(new byte[] { bytes[StartAddr] });
                        v = bitArr[bitAdr] ? 1 : 0;
                    }
                    else
                    {
                        v = 0;
                        //LogR.Logger.Info("GGG:Bit,0");
                    }
                    break;
                default:
                    break;
            }

            return v;
        }

        static byte[] GetValueBytes(string VarType, string Value)
        {
            byte[] v = null;
            switch (VarType)
            {
                case "Byte":
                    v = new byte[] { byte.Parse(Value) };
                    break;
                case "Word":
                    v = Word.ToByteArray(ushort.Parse(Value));
                    break;
                case "Int":
                    v = Int.ToByteArray(short.Parse(Value));
                    break;
                case "DWord":
                    v = DWord.ToByteArray(uint.Parse(Value));
                    break;
                case "DInt":
                    v = DInt.ToByteArray(int.Parse(Value));
                    break;
                case "Real":
                    v = S7.Net.Types.Single.ToByteArray(float.Parse(Value));
                    break;
                default:
                    break;
            }
            return v;
        }
    }
}
