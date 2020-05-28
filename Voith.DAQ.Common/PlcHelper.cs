using HslCommunication;
using HslCommunication.Profinet.Siemens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Voith.DAQ.Common
{
    /// <summary>
    /// PLC访问基类
    /// </summary>
    public class PlcHelper
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
        private static SiemensS7Net _siemensTcpNet { get; set; }

        /// <summary>
        /// PLC访问实例
        /// </summary>
        private static SiemensS7Net SiemensTcpNet
        {
            get
            {
                return ReConn();
            }
        }

        public static SiemensS7Net ReConn(bool rc = false)
        {
            if (_siemensTcpNet == null || rc)
            {
                _siemensTcpNet = new SiemensS7Net(SiemensPLCS.S1500, PlcIp)
                {
                    ConnectTimeOut = 2500
                };
                _siemensTcpNet.Rack = 0;
                _siemensTcpNet.Slot = 1;

                OperateResult connect = _siemensTcpNet.ConnectServer();
                LogHelper.Info(
                    connect.IsSuccess
                    ? $"与PLC：{PlcIp}连接成功！{connect.ErrorCode}-"
                    : $"与PLC：{PlcIp}连接失败！错误信息：{connect.ErrorCode}，{connect.Message}"
                    );
            }

            return _siemensTcpNet;
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
        //Start:
            try
            {
                var result = SiemensTcpNet.Read("DB" + db + "." + startByteAdr, (ushort)count);//.Content;
                if (result.IsSuccess)
                {
                    return result.Content;
                }

                LogHelper.Info($"ReadBytes失败：{result.ToMessageShowString()}\nDB块：db，开始地址：{startByteAdr}，长度：{count}");
                Thread.Sleep(300);
                //goto Start;
            }
            catch (Exception e)
            {
                LogHelper.Error(e, $"ReadBytes出错：DB块：db，开始地址：{startByteAdr}，长度：{count}");
                Thread.Sleep(300);
                //goto Start;
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
        //Start:
            try
            {
                if (typeof(T) == typeof(bool))
                {
                    var result = SiemensTcpNet.ReadBool("DB" + db + "." + startByteAdr + "." + bitAdr);
                    if (result.IsSuccess)
                    {
                        return new[] { (T)(object)result.Content };
                    }
                    else
                    {
                        LogHelper.Info(
                            $"Read{typeof(T).Name}失败：{result.ToMessageShowString()}\nDB块：db，开始地址：{startByteAdr}，长度：{varCount}，Bit位：{bitAdr}");
                        Thread.Sleep(300);
                        //goto Start;
                    }
                }

                else if (typeof(T) == typeof(short))
                {
                    var result = SiemensTcpNet.ReadInt16("DB" + db + "." + startByteAdr, (ushort)varCount);
                    if (result.IsSuccess)
                    {
                        return (T[])(object)result.Content;
                    }
                    else
                    {
                        LogHelper.Info(
                            $"Read{typeof(T).Name}失败：{result.ToMessageShowString()}\nDB块：db，开始地址：{startByteAdr}，长度：{varCount}，Bit位：{bitAdr}");
                        Thread.Sleep(300);
                        //goto Start;
                    }
                }

                else if (typeof(T) == typeof(int))
                {
                    var result = SiemensTcpNet.ReadInt32("DB" + db + "." + startByteAdr, (ushort)varCount);
                    if (result.IsSuccess)
                    {
                        return (T[])(object)result.Content;
                    }
                    else
                    {
                        LogHelper.Info(
                            $"Read{typeof(T).Name}失败：{result.ToMessageShowString()}\nDB块：db，开始地址：{startByteAdr}，长度：{varCount}，Bit位：{bitAdr}");
                        Thread.Sleep(300);
                        //goto Start;
                    }
                }

                else if (typeof(T) == typeof(float))
                {
                    var result = SiemensTcpNet.ReadFloat("DB" + db + "." + startByteAdr, (ushort)varCount);
                    if (result.IsSuccess)
                    {
                        return (T[])(object)result.Content;
                    }
                    else
                    {
                        LogHelper.Info(
                            $"Read{typeof(T).Name}失败：{result.ToMessageShowString()}\nDB块：db，开始地址：{startByteAdr}，长度：{varCount}，Bit位：{bitAdr}");
                        Thread.Sleep(300);
                        //goto Start;
                    }
                }

                else if (typeof(T) == typeof(string))
                {
                    var result = SiemensTcpNet.ReadString("DB" + db + "." + startByteAdr, (ushort)varCount);
                    if (result.IsSuccess)
                    {
                        var str = result.Content.Substring(2, varCount - 2);

                        var endIndex = str.IndexOf("\r", StringComparison.Ordinal) < 0
                            ? 0
                            : str.IndexOf("\r", StringComparison.Ordinal);

                        if (endIndex == 0 && varCount - 2 <= result.Content.Length - 2)
                        {
                            endIndex = varCount - 2;
                        }

                        str = str.Substring(0, endIndex).Trim("\n".ToCharArray()).Trim("\0".ToCharArray());
                        return new[] { (T)(object)str };
                    }
                    else
                    {
                        LogHelper.Info(
                            $"Read{typeof(T).Name}失败：{result.ToMessageShowString()}\nDB块：db，开始地址：{startByteAdr}，长度：{varCount}，Bit位：{bitAdr}");
                        Thread.Sleep(300);
                        //goto Start;
                    }
                }

                else if (typeof(T) == typeof(DateTime))
                {
                    var result = SiemensTcpNet.Read("DB" + db + "." + startByteAdr, 8);
                    if (result.IsSuccess)
                    {
                        return new[] { (T)(object)PlcConvert.GetLDTAt(result.Content, 0) };
                    }
                    else
                    {
                        LogHelper.Info(
                            $"Read{typeof(T).Name}失败：{result.ToMessageShowString()}\nDB块：db，开始地址：{startByteAdr}，长度：{varCount}，Bit位：{bitAdr}");
                        Thread.Sleep(300);
                        //goto Start;
                    }
                }

                else
                {
                    var result = SiemensTcpNet.ReadByte("DB" + db + "." + startByteAdr);
                    if (result.IsSuccess)
                    {
                        return new[] { (T)(object)result.Content };
                    }
                    else
                    {
                        LogHelper.Info(
                            $"Read{typeof(T).Name}失败：{result.ToMessageShowString()}\nDB块：db，开始地址：{startByteAdr}，长度：{varCount}，Bit位：{bitAdr}");
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
        //Start:
            try
            {
                //byte数组长度大于452个字节,会出现问题,以下做循环写入
                for (int i = 0; i < Convert.ToInt32(Math.Ceiling((decimal)(value.Length / 450.0))); i++)
                {
                    var result = SiemensTcpNet.Write("DB" + db + "." + (startByteAdr + 450 * i), value.Skip(450 * i).Take(450).ToArray());//.Content;
                    if (!result.IsSuccess)
                    {
                        LogHelper.Info($"WriteBytes失败：{result.ToMessageShowString()}\nDB块：db，开始地址：{startByteAdr}");
                        Thread.Sleep(300);
                        //goto Start;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                LogHelper.Error(e, $"WriteBytes出错：DB块：db，开始地址：{startByteAdr}");
                Thread.Sleep(300);
                //goto Start;
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
        //Start:
            try
            {
                if (typeof(T) == typeof(bool))
                {
                    var result = SiemensTcpNet.Write("DB" + db + "." + startByteAdr + "." + bitAdr, (bool)(object)value);//.Content;
                    if (result.IsSuccess)
                    {
                        return result.IsSuccess;
                    }

                    LogHelper.Info(
                        $"Write{typeof(T).Name}失败：{result.ToMessageShowString()}\nDB块：db，开始地址：{startByteAdr}，数据：{value}，Bit位：{bitAdr}");
                    Thread.Sleep(300);
                    //goto Start;
                }

                else if (typeof(T) == typeof(short))
                {
                    var result = SiemensTcpNet.Write("DB" + db + "." + startByteAdr, (short)(object)value);//.Content;
                    if (result.IsSuccess)
                    {
                        return result.IsSuccess;
                    }

                    LogHelper.Info(
                        $"Write{typeof(T).Name}失败：{result.ToMessageShowString()}\nDB块：db，开始地址：{startByteAdr}，数据：{value}，Bit位：{bitAdr}");
                    Thread.Sleep(300);
                    //goto Start;
                }

                else if (typeof(T) == typeof(int))
                {
                    var result = SiemensTcpNet.Write("DB" + db + "." + startByteAdr, (int)(object)value);//.Content;
                    if (result.IsSuccess)
                    {
                        return result.IsSuccess;
                    }

                    LogHelper.Info(
                        $"Write{typeof(T).Name}失败：{result.ToMessageShowString()}\nDB块：db，开始地址：{startByteAdr}，数据：{value}，Bit位：{bitAdr}");
                    Thread.Sleep(300);
                    //goto Start;
                }

                else if (typeof(T) == typeof(float))
                {
                    var result = SiemensTcpNet.Write("DB" + db + "." + startByteAdr, (float)(object)value);//.Content;
                    if (result.IsSuccess)
                    {
                        return result.IsSuccess;
                    }

                    LogHelper.Info(
                        $"Write{typeof(T).Name}失败：{result.ToMessageShowString()}\nDB块：db，开始地址：{startByteAdr}，数据：{value}，Bit位：{bitAdr}");
                    Thread.Sleep(300);
                    //goto Start;
                }

                else if (typeof(T) == typeof(string))
                {
                    var result = SiemensTcpNet.Write("DB" + db + "." + startByteAdr, value.ToString(), value.ToString().Length);//.Content;
                    if (result.IsSuccess)
                    {
                        return result.IsSuccess;
                    }

                    LogHelper.Info(
                        $"Write{typeof(T).Name}失败：{result.ToMessageShowString()}\nDB块：db，开始地址：{startByteAdr}，数据：{value}，Bit位：{bitAdr}");
                    Thread.Sleep(300);
                    //goto Start;
                }

                else if (typeof(T) == typeof(DateTime))
                {
                    var bytes = new byte[8];
                    PlcConvert.SetLDTAt(bytes, 0, (DateTime)(object)value);
                    var result = SiemensTcpNet.Write("DB" + db + "." + startByteAdr, bytes);//.Content;
                    if (result.IsSuccess)
                    {
                        return result.IsSuccess;
                    }

                    LogHelper.Info(
                        $"Write{typeof(T).Name}失败：{result.ToMessageShowString()}\nDB块：db，开始地址：{startByteAdr}，数据：{value}，Bit位：{bitAdr}");
                    Thread.Sleep(300);
                    //goto Start;
                }

                else
                {
                    var result = SiemensTcpNet.Write("DB" + db + "." + startByteAdr, (byte)(object)value);//.Content;
                    if (result.IsSuccess)
                    {
                        return result.IsSuccess;
                    }

                    LogHelper.Info(
                        $"Write{typeof(T).Name}失败：{result.ToMessageShowString()}\nDB块：db，开始地址：{startByteAdr}，数据：{value}，Bit位：{bitAdr}");
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
    }
}
