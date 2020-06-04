using SqlSugar;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using Voith.DAQ.Common;
using Voith.DAQ.DB;
using Voith.DAQ.Model;

namespace Voith.DAQ.Services
{
    class ScanFlow
    {
        /// <summary>
        /// 数据库访问对象
        /// </summary>
        private readonly DbContext _db;

        /// <summary>
        /// 当前工站在位的工件信息
        /// </summary>
        private Workpiece _workpiece;

        public ScanFlow(Workpiece workpiece)
        {
            _db = new DbContext();
            _workpiece = workpiece;

            Handle();
        }

        public void Handle()
        {
            new Thread(() =>
            {
                var startAddress = _workpiece.StartAddr;
                while (true)
                {
                    try
                    {
                        var signals = PlcHelper.Read<short>(SystemConfig.ControlDB, startAddress + 16, 2);
                        if (signals[1] == 1)
                        {
                            short rcode = 102;
                            var scanerCode = PlcHelper.Read<string>(SystemConfig.ControlDB, startAddress + 20, 98)[0];

                            //获取托盘状态
                            //var TrayStatus = PlcHelper.Read<short>(SystemConfig.ControlDB, startAddress + 126, 2);
                            //_workpiece.TrayStatus = TrayStatus[0];
                            //LogHelper.Info($"扫码->{_workpiece.StationCode}->{scanerCode}->{TrayStatus[0]}");

                            if (_workpiece.TrayStatus == 0)
                            {
                                var scanerType = signals[0];
                                LogHelper.Info($"扫码 类型->{_workpiece.StationCode}->{scanerType}");
                                switch (scanerType)
                                {
                                    case 1://工件扫描 不扫托盘号
                                        if (!string.IsNullOrEmpty(scanerCode) /*&& scanerCode.Length == 3 && scanerCode.Substring(0, 1) == "A"*/)
                                        {
                                            if (_workpiece.StationIndex == 6200000)
                                            {
                                                //NOK岔道口处理

                                                //if (Check("OP061"))
                                                //{
                                                //    PlcHelper.Write<short>(SystemConfig.DTControlDB, startAddress + 244, 101);
                                                //    PlcHelper.Write<short>(SystemConfig.DTControlDB, startAddress + 360, 1);
                                                //    LogHelper.Info($"OP062校验OP61->合格->{_workpiece.StationCode}->{_workpiece.SerialNumber}->101 1");
                                                //}
                                                //else
                                                //{
                                                //    PlcHelper.Write<short>(SystemConfig.DTControlDB, startAddress + 244, 102);
                                                //    PlcHelper.Write<short>(SystemConfig.DTControlDB, startAddress + 360, 3);
                                                //    LogHelper.Info($"OP062校验OP61->不合格->{_workpiece.StationCode}->{_workpiece.SerialNumber}->102 3");
                                                //}
                                                //rcode = 101;
                                            }
                                            else
                                            {
                                                if (!Check(_workpiece.StationCode))
                                                {
                                                    rcode = QueryOrderInfo(scanerCode, 2);
                                                    //PlcHelper.Write<short>(SystemConfig.DTControlDB, startAddress + 114, 0);//本工位合格放行
                                                    LogHelper.Info($"扫码 订单获取完成->{_workpiece.StationCode}->{_workpiece.SerialNumber}->{scanerCode}");
                                                }
                                                else
                                                {
                                                    if (string.IsNullOrEmpty(_workpiece.SerialNumber))
                                                    {
                                                        LogHelper.Info($"扫码 工件未绑定条码->{_workpiece.StationCode}->->{scanerCode}-> 102");
                                                        rcode = 102;
                                                        //PlcHelper.Write<short>(SystemConfig.DTControlDB, startAddress + 114, 2);//
                                                    }
                                                    else
                                                    {
                                                        LogHelper.Info($"扫码 本工位合格放行->{_workpiece.StationCode}->{_workpiece.SerialNumber}->{scanerCode}-> 101 1");
                                                        rcode = 101;
                                                        //PlcHelper.Write<short>(SystemConfig.DTControlDB, startAddress + 114, 1);//本工位合格放行
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }
                                PlcHelper.Write(SystemConfig.DTControlDB, _workpiece.StartAddr + 110, rcode);
                            }
                            else if (_workpiece.TrayStatus == 2 && !string.IsNullOrEmpty(scanerCode) && scanerCode.Length == 3 && scanerCode.Substring(0, 1) == "A")
                            {
                                //不合格托盘

                                //string sql = "INSERT INTO dbo.EmptyPalletLog(PalletCode,Flag)" +
                                //$"VALUES('{ scanerCode }', 0)";
                                //_db.Db.Ado.ExecuteCommand(sql);
                                ////+360 给2 空托盘放行信号
                                //LogHelper.Info($"托盘扫码 空托盘放行信号->{_workpiece.StationCode}->{_workpiece.SerialNumber}->{scanerCode}-> 101 2");
                                //PlcHelper.Write(SystemConfig.DTControlDB, _workpiece.StartAddr + 110, (short)101);
                                //PlcHelper.Write(SystemConfig.DTControlDB, _workpiece.StartAddr + 114, (short)2);
                            }
                        }
                        Thread.Sleep(300);
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e, "ScanFlow");
                    }
                }
            })
            { IsBackground = true }.Start();
        }

        /// <summary>
        /// 查询并赋值订单信息
        /// </summary>
        /// <param name="scanerCode"></param>
        private short QueryOrderInfo(string scanerCode,int mode)
        {
            short R = 102;

            try
            {
                var startAddress = _workpiece.StartAddr;

                if (string.IsNullOrWhiteSpace(scanerCode))
                {
                    //PlcHelper.Write<short>(SystemConfig.DTControlDB, startAddress + 110, 102);
                    return 102;
                }
                _workpiece.TrayCode = scanerCode;

                GoodsOrder order = null;

                if (_workpiece.StationCode == "OP010")
                {
                    var orders0 = _db.GoodsOrderDb.GetList(it => (it.OrderStatus == 1 || it.OrderStatus == 0) && it.PalletCode == _workpiece.TrayCode);

                    if (orders0.Count > 0)
                    {
                        orders0 = _db.GoodsOrderDb.GetList(it => (it.OrderStatus == 1 || it.OrderStatus == 0) && it.PalletCode == _workpiece.TrayCode);
                        order = orders0.FirstOrDefault();
                        PlcHelper.Write(SystemConfig.DTControlDB, 132, (short)101);//订单获取成功
                        LogHelper.Info("QueryOrderInfo-op010->订单获取 已有");
                    }
                    else
                    {
                        LogHelper.Info("QueryOrderInfo-op010->订单获取 新");

                        order = AssignOrder.GetOrder(mode); //_db.GoodsOrderDb.GetList(it => it.OrderStatus == 0).OrderBy(it => it.ID).FirstOrDefault();
                        if (order != null)
                        {
                            order.PalletCode = _workpiece.TrayCode;
                            order.OrderStatus = 1;
                            order.OnLineTime = DateTime.Now;
                            _db.GoodsOrderDb.Update(order);
                            PlcHelper.Write(SystemConfig.DTControlDB, 132, (short)101);//订单获取成功
                            LogHelper.Info("QueryOrderInfo->Has");
                        }
                        else
                        {
                            PlcHelper.Write(SystemConfig.DTControlDB, 132, (short)102);//订单获取失败
                            LogHelper.Info("QueryOrderInfo->null");
                        }
                    }
                }
                else
                {
                    var orders = _db.GoodsOrderDb.GetList(it => it.OrderStatus == 1 && it.PalletCode == _workpiece.TrayCode);
                    if (orders.Count > 0)
                    {
                        orders = _db.GoodsOrderDb.GetList(it => it.OrderStatus == 1 && it.PalletCode == _workpiece.TrayCode);
                        order = orders.FirstOrDefault();
                    }
                }

                if (order != null)
                {
                    _workpiece.SerialNumber = order.SerialNumber;
                    _workpiece.ProductTypeCode = order.ProductType;
                    _workpiece.MaterielCode = order.MaterielCode;
                    _workpiece.Type1 = order.Type1;
                    _workpiece.Type2 = order.Type2;

                    var orders = _db.GoodsOrderDb.AsQueryable().Where(it => it.HeadOrder == 1).OrderBy(it => it.ID, OrderByType.Desc).First();

                    int lastOrders = orders.Count - _db.GoodsOrderDb.AsQueryable().Where(it => it.ID >= orders.ID && it.OrderStatus == 2).Count();
                    int OKOrdersCount = _db.GoodsOrderDb.AsQueryable().Where(it => it.ID >= orders.ID && it.OrderStatus == 2).Count();
                    int INGOrdersCount = _db.GoodsOrderDb.AsQueryable().Where(it => it.ID >= orders.ID && it.OrderStatus == 1).Count();
                    //int NOKOrdersCount = _db.GoodsOrderDb.AsQueryable().Where(it => it.ID >= orders.ID && it.OrderStatus == 1).Count();
                    DateTime dnow = DateTime.Now;
                    int DayOKOrdersCount = _db.Db.Ado.GetDataTable($"select * from GoodsOrder where OnLineTime between '{ dnow.ToString("yyyy-MM-dd") } 00:00:01' and '{ dnow.ToString("yyyy-MM-dd") } 23:59:59'").Rows.Count;
                    PlcHelper.Write<short>(SystemConfig.DTControlDB, 14, (short)orders.Count);//当前订单总数
                    PlcHelper.Write<short>(SystemConfig.DTControlDB, 16, (short)lastOrders);//当前订单剩余数量
                    PlcHelper.Write<short>(SystemConfig.DTControlDB, 18, (short)OKOrdersCount);//当前订单OK数量
                    PlcHelper.Write<short>(SystemConfig.DTControlDB, 20, (short)INGOrdersCount);//当前订单生成中数量
                    PlcHelper.Write<short>(SystemConfig.DTControlDB, 22, (short)0);//当前订单NG数量
                    PlcHelper.Write<short>(SystemConfig.DTControlDB, 24, (short)DayOKOrdersCount);//当天OK数量
                    PlcHelper.Write<short>(SystemConfig.DTControlDB, 26, (short)0);//当天NG数量

                    LogHelper.Info($"扫码 QueryOrderInfo->{_workpiece.StationCode}->{ _workpiece.SerialNumber}->" +
                        $"{ _workpiece.ProductTypeCode}->{ _workpiece.MaterielCode}->{_workpiece.Type1}->{_workpiece.Type2}");
                    R = 101;
                }
                else
                {
                    LogHelper.Info("QueryOrderInfo->订单异常");
                    _workpiece.SerialNumber = "";
                    //PlcHelper.Write<short>(SystemConfig.DTControlDB, startAddress + 110, 102);
                    R = 102;
                }
            }
            catch (Exception e)
            {
                R = 102;
                LogHelper.Error(e, "QueryOrderInfo");
            }

            return R;
        }

        private bool Check(string stationCode)
        {
            bool checkFlag0 = true;
            //var signals = PlcHelper.Read<short>(SystemConfig.ControlDB, _workpiece.StartAddr + 240, 2);
            
            var formulaNo = PlcHelper.Read<short>(_workpiece.DBAddr1, 0);//未定需要修改
            short FNo = formulaNo.Length > 0 && formulaNo[0] > 0 ? formulaNo[0] : (short)1;
            
            string sql =
                $"SELECT * FROM dbo.Formula WHERE StationName = '{stationCode}' AND FormulaNum = '{FNo.ToString("000")}' ORDER BY WorkStep";
            var formulas = _db.Db.Ado.GetDataTable(sql);

            sql =
                $"SELECT * FROM dbo.QualityData WHERE SerialNumber = '{_workpiece.SerialNumber}' AND StationCode = '{stationCode}' ORDER BY StepNo,ID";
            var dataList = _db.Db.Ado.GetDataTable(sql);

            switch (stationCode)
            {
                case "OP0999":
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
    }
}
