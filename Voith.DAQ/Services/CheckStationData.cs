using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using Voith.DAQ.Common;
using Voith.DAQ.DB;
using Voith.DAQ.Model;

namespace Voith.DAQ.Services
{
    /// <summary>
    /// 校验整个工作的数据是否合格
    /// </summary>
    class CheckStationData
    {/// <summary>
     /// 数据库访问对象
     /// </summary>
        private readonly DbContext _db;
        short FNo = 1;
        /// <summary>
        /// 当前工站在位的工件信息
        /// </summary>
        private Workpiece _workpiece;

        public CheckStationData(Workpiece workpiece)
        {
            _db = new DbContext();
            _workpiece = workpiece;

            Handle();
        }

        private void Handle()
        {
            new Thread(() =>
            {


                /*所有工位数据都在一个DB块，每个工位数据占用1000个字节，
                 根据工位位置确定读取的数据起始位置*/
                var startAddress = _workpiece.StartAddr;
                while (true)
                {
                    try
                    {
                        var datas = PlcHelper.Read<short>(SystemConfig.ControlDB, startAddress + 244, 1);
                        var formulaNo = PlcHelper.Read<short>(_workpiece.DBAddr1, 0);
                        FNo = formulaNo.Length > 0 && formulaNo[0] > 0 ? formulaNo[0] : (short)1;
                        if (datas[0] == 1)//校验本工位数据
                        {
                            if (Check(_workpiece.StationCode))
                            {
                                PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 244, 101);
                                PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 360, 1);
                                LogHelper.Info($"校验本工位数据->合格->{_workpiece.StationCode}->{_workpiece.SerialNumber}->101 1");

                                if (_workpiece.StationIndex == 70)//订单下线解绑
                                {
                                    string sql = $"update dbo.GoodsOrder set OrderStatus=2,OffLineTime='{ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }'" +
                                                      $"where SerialNumber='{ _workpiece.SerialNumber }'";
                                    _db.Db.Ado.ExecuteCommand(sql);
                                    LogHelper.Info($"校验本工位数据->订单下线解绑->{_workpiece.StationCode}->{_workpiece.SerialNumber}");

                                    //写入空托盘放行信号
                                    //PlcHelper.Write(SystemConfig.ControlDB, _workpiece.StartAddr + 360, (short)2);
                                    //PlcHelper.Write(SystemConfig.ControlDB, _workpiece.StartAddr + 248, (short)101);
                                }

                                string sql0 = $"update dbo.GoodsOrder set CheckResult=1 " +
                                                      $"where SerialNumber='{ _workpiece.SerialNumber }'";
                                _db.Db.Ado.ExecuteCommand(sql0);
                                LogHelper.Info($"校验本工位数据->合格-GoodsOrder更新1->{_workpiece.StationCode}->{_workpiece.SerialNumber}");
                            }
                            else
                            {
                                PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 244, 102);
                                PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 360, 3);
                                LogHelper.Info($"校验本工位数据->不合格->{_workpiece.StationCode}->{_workpiece.SerialNumber}->102 3");

                                string sql = $"update dbo.GoodsOrder set CheckResult=2 " +
                                                      $"where SerialNumber='{ _workpiece.SerialNumber }'";
                                _db.Db.Ado.ExecuteCommand(sql);
                                LogHelper.Info($"校验本工位数据->不合格-GoodsOrder更新2->{_workpiece.StationCode}->{_workpiece.SerialNumber}");
                            }

                            //记录循环时间和eks信息
                            var stationdata = PlcHelper.ReadBytes(SystemConfig.ControlDB, startAddress + 4, 26);
                            if (!string.IsNullOrEmpty(_workpiece.SerialNumber))
                                StationData(stationdata);
                            LogHelper.Info($"{_workpiece.StationCode}工位信息记录B");
                        }

                        datas = PlcHelper.Read<short>(SystemConfig.ControlDB, startAddress + 32, 1);
                        if (datas[0] == 1)//校验其他工位数据
                        {
                            var stationCode = PlcHelper.Read<short>(SystemConfig.ControlDB, startAddress + 30, 1)[0];
                            string stationName = String.Empty;
                            //switch (stationCode.ToString().Length)
                            //{
                            //    case 2:
                            //        stationName = "OP0" + stationCode;
                            //        break;
                            //    case 3:
                            //        stationName = "OP0" + stationCode.ToString().Substring(0, 2) + "-" +
                            //                      stationCode.ToString().Substring(2, 1);
                            //        break;
                            //}
                            foreach (JObject o in SystemConfig.StationList)
                            {
                                if (o["StationIndex"].ToString() == stationCode.ToString())
                                {
                                    stationName = o["StationCode"].ToString();
                                    break;
                                }
                            }

                            if (Check(stationName))
                            {
                                PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 32, 101);
                                LogHelper.Info($"校验前工位数据->合格->{_workpiece.StationCode}->101->{stationCode}");
                            }
                            else
                            {
                                PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 32, 102);
                                LogHelper.Info($"校验前工位数据->不合格->{_workpiece.StationCode}->102->{stationCode}");
                            }
                        }

                        //if (_workpiece.StationCode == "OP010")
                        //{
                        //    datas = PlcHelper.Read<short>(SystemConfig.ControlDB, startAddress + 370, 1);
                        //    if (datas[0] == 2)
                        //    {
                        //        PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 370, 0);
                        //        var order = _db.GoodsOrderDb.GetList(it => it.OrderStatus == 0 && it.PalletCode == _workpiece.TrayCode).FirstOrDefault();
                        //        string sql = $"SELECT TOP 1 * FROM dbo.GoodsOrder WHERE OrderStatus = 1 ORDER BY ID desc";
                        //        var vt2 = _db.Db.Ado.GetDataTable(sql);
                        //        if (order != null)
                        //        {
                        //            LogHelper.Info($"校验本工位数据->10工位订单上线更新->{_workpiece.StationCode}");
                        //            order.OrderStatus = 1;
                        //            _db.GoodsOrderDb.Update(order);
                        //            PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 372, 201);
                        //        }
                        //        else
                        //        {
                        //            if (vt2 != null && vt2.Rows.Count > 0 && vt2.Rows[0]["PalletCode"].ToString() == _workpiece.TrayCode)
                        //            {
                        //                LogHelper.Info($"校验本工位数据->10工位订单上线更新 NoUpdate->{_workpiece.StationCode}");
                        //                PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 372, 201);
                        //            }
                        //            else
                        //            {
                        //                LogHelper.Info($"校验本工位数据->10工位订单上线更新 Failed->{_workpiece.StationCode}");
                        //                PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 372, 202);
                        //            }
                        //        }
                        //    }
                        //}
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                    }

                    Thread.Sleep(300);
                }
            })
            { IsBackground = true }.Start();
        }

        private bool Check(string stationCode)
        {
            bool checkFlag0 = true;
            //var signals = PlcHelper.Read<short>(SystemConfig.ControlDB, _workpiece.StartAddr + 240, 2);
            
            //int ReportType = 1;//1 非一体轴 2 一体轴
            //string TCode = "";
            //string FormulaNum = "001";
            //if (_workpiece.SerialNumber.Length > 3)
            //{
            //    TCode = _workpiece.SerialNumber.Substring(0, 9);
            //    if (TCode == "153008411" ||
            //        TCode == "153008448" ||
            //        TCode == "153008646")
            //    {
            //        ReportType = 2;
            //        FormulaNum = "031";
            //    }
            //}

            string sql =
                $"SELECT * FROM dbo.Formula WHERE StationName = '{stationCode}' AND FormulaNum = '{FNo.ToString("000")}' ORDER BY WorkStep";
            var formulas = _db.Db.Ado.GetDataTable(sql);

            sql =
                $"SELECT * FROM dbo.QualityData WHERE SerialNumber = '{_workpiece.SerialNumber}' AND StationCode = '{stationCode}' ORDER BY StepNo,ID";
            var dataList = _db.Db.Ado.GetDataTable(sql);

            sql =
               $"SELECT * FROM dbo.KeyCodeInfo WHERE SerialNumber = '{_workpiece.SerialNumber}' AND StationCode = '{stationCode}' ORDER BY ID desc";
            var keycode = _db.Db.Ado.GetDataTable(sql);

            switch (stationCode)
            {
                case "OP025000000":
                    break;
                default:
                    checkFlag0 = StandardCheckData(formulas, dataList);
                    break;
            }

            return checkFlag0;
        }

        private static bool StandardCheckData(DataTable formulas, DataTable dataList)
        {
            bool checkFlag0 = true;

            string[] operationTypeList = new[] { "1", "2", "3", "4" };//配方中有质量数据的操作类型编号
            foreach (DataRow formula in formulas.Rows)
            {
                if (operationTypeList.Contains(formula["OperationTypeId"]?.ToString()))
                {
                    bool checkFlag = false;
                    foreach (DataRow row in dataList.Rows)
                    {
                        if (row["StepNo"]?.ToString() == formula["WorkStep"]?.ToString())
                        {
                            if (row["CheckResult"]?.ToString() == "True")
                            {
                                //校验合格
                                //goto NextData;
                                checkFlag = true;
                            }
                            else
                            {
                                //校验不合格
                                checkFlag = false;
                            }
                        }
                    }
                    checkFlag0 &= checkFlag;
                }
            }
            checkFlag0 &= formulas.Rows.Count > 0;
            return checkFlag0;
        }

        private bool StationData(byte[] bytes)
        {
            try
            {
                var WorkingTime = PlcConvert.GetDIntAt(bytes, 0);
                var EKSLevel = PlcConvert.GetIntAt(bytes, 4);
                var EKSCode = Encoding.ASCII.GetString(bytes, 8, 18).Trim('\0').Trim();

                string sql = "INSERT INTO dbo.StationData(SerialNumber,StationCode,EKS,EKSLevel,WorkingTime)" +
                             $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{EKSCode}',{EKSLevel},{WorkingTime});";
                if (!string.IsNullOrEmpty(_workpiece.SerialNumber))
                    _db.Db.Ado.ExecuteCommand(sql);
                else
                    LogHelper.Info($"工位数据记录B->{_workpiece.SerialNumber}-{_workpiece.StationCode}- 条码为空不记录");

                //int result;
                //if (stationPressureSet != 0 && stationShiftSet != 0 && stationPressureApex != 0 && stationShiftApex != 0 && pressureResult == 1)
                //{
                //    result = 1;
                //}
                //else
                //{
                //    result = 0;
                //}
                //result = Status == 2 ? 1 : 0;
                //sql = "INSERT INTO dbo.QualityData(SerialNumber,StationCode,DataType,DataTableName,StepNo,DataPrimaryKey,CheckResult)" +
                //        $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{6}','',{stepNo},'',{result})";
                //_db.Db.Ado.ExecuteCommand(sql);

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "工位数据记录出错B");
                //MessageBox.Show("注油数据采集出错");
                return false;
            }
        }
    }
}
