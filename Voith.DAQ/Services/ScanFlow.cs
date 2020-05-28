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

        /// <summary>
        /// 下发配方线程
        /// </summary>
        public void Handle()
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
                        var signals = PlcHelper.Read<short>(SystemConfig.ControlDB, startAddress + 246, 2);
                        if (signals[1] == 1)
                        {
                            short rcode = 102;
                            var scanerCode = PlcHelper.Read<string>(SystemConfig.ControlDB, startAddress + 250, 98)[0];

                            //获取托盘状态
                            var TrayStatus = PlcHelper.Read<short>(SystemConfig.ControlDB, startAddress + 356, 2);
                            _workpiece.TrayStatus = TrayStatus[0];
                            LogHelper.Info($"托盘扫码->{_workpiece.StationCode}->{scanerCode}->{TrayStatus[0]}");

                            if (_workpiece.StationIndex == 10 && !string.IsNullOrEmpty(scanerCode) && scanerCode.Length == 3 && scanerCode.Substring(0, 1) == "A")
                            {
                                //未解绑托盘解绑 除非当前托盘号与已上线最后一个订单所绑定托盘号一致
                                //防止复位之后占用新订单问题
                                string sql = $"SELECT TOP 1 * FROM dbo.GoodsOrder WHERE OrderStatus <> 0 ORDER BY ID desc";
                                var vt1 = _db.Db.Ado.GetDataTable(sql);
                                sql = $"SELECT TOP 1 * FROM dbo.GoodsOrder WHERE PalletCode = '{ scanerCode }' AND OrderStatus = 1 ORDER BY ID desc";
                                var vt2 = _db.Db.Ado.GetDataTable(sql);
                                if (vt1.Rows.Count > 0 && vt2.Rows.Count > 0)
                                {
                                    if (vt1.Rows[0]["ID"].ToString() != vt2.Rows[0]["ID"].ToString())
                                    {
                                        sql = $"update dbo.GoodsOrder set OrderStatus=3,OffLineTime='{ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }'" +
                                              $"where PalletCode='{ scanerCode }' and OrderStatus=1";
                                        int r = _db.Db.Ado.ExecuteCommand(sql);
                                        if (r > 0)
                                            LogHelper.Info($"订单上线 状态1-3 解绑->{scanerCode}-");
                                    }
                                }
                                //空托盘解绑
                                sql = "update dbo.EmptyPalletLog set Flag=1 " +
                                      $"where PalletCode='{ scanerCode }'";
                                _db.Db.Ado.ExecuteCommand(sql);
                                LogHelper.Info($"托盘扫码 空托盘解绑OP010->{_workpiece.StationCode}-> 101 2");
                            }

                            if (_workpiece.TrayStatus == 0)
                            {
                                var scanerType = signals[0];
                                LogHelper.Info($"托盘扫码 类型->{_workpiece.StationCode}->{scanerType}");
                                switch (scanerType)
                                {
                                    case 1://员工号扫描
                                        break;
                                    case 2://托盘号扫描
                                           //if (!CheckStationResult(scanerCode, 1))
                                           //{
                                           //    //工位数据校验失败，重新开始流程
                                           //    continue;
                                           //}
                                        if (!string.IsNullOrEmpty(scanerCode) && scanerCode.Length == 3 && scanerCode.Substring(0, 1) == "A")
                                        {
                                            bool isEmpty = false;

                                            //记录空托盘号
                                            string sql0 = "SELECT PalletCode FROM dbo.EmptyPalletLog WHERE" +
                                            $" PalletCode ='{ scanerCode }' and Flag=0";
                                            var vt0 = _db.Db.Ado.GetDataTable(sql0);
                                            isEmpty = vt0.Rows.Count > 0;
                                            if (isEmpty)
                                            {
                                                //if (_workpiece.StationIndex == 70)//空托盘解绑
                                                //{
                                                //    string sql = "update dbo.EmptyPalletLog set Flag=1 " +
                                                //    $"where PalletCode='{ scanerCode }'";
                                                //    _db.Db.Ado.ExecuteCommand(sql);
                                                //    LogHelper.Info($"托盘扫码 空托盘解绑->{_workpiece.StationCode}-> 101 2");
                                                //}
                                                //+360 给2 空托盘放行信号
                                                rcode = 101;
                                                //PlcHelper.Write(SystemConfig.ControlDB, _workpiece.StartAddr + 248, (short)101);
                                                PlcHelper.Write(SystemConfig.ControlDB, _workpiece.StartAddr + 360, (short)2);
                                            }
                                            else
                                            {
                                                QueryOrderInfo(scanerCode, 2);

                                                if (_workpiece.StationIndex == 62)
                                                {
                                                    if (Check("OP061"))
                                                    {
                                                        PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 244, 101);
                                                        PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 360, 1);
                                                        LogHelper.Info($"OP062校验OP61->合格->{_workpiece.StationCode}->{_workpiece.SerialNumber}->101 1");
                                                    }
                                                    else
                                                    {
                                                        PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 244, 102);
                                                        PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 360, 3);
                                                        LogHelper.Info($"OP062校验OP61->不合格->{_workpiece.StationCode}->{_workpiece.SerialNumber}->102 3");
                                                    }
                                                    rcode = 101;
                                                }
                                                else
                                                {
                                                    if (!Check(_workpiece.StationCode))
                                                    {
                                                        rcode = QueryOrderInfo(scanerCode, 2);

                                                        if (_workpiece.StationIndex == 30)
                                                        {
                                                            try
                                                            {
                                                                string sql = $"SELECT TOP 1 MeasureValue FROM dbo.OP20ThreePointData WHERE SerialNumber = '{ _workpiece.SerialNumber }' AND MeasureStatus = 2 ORDER BY ID desc";
                                                                var vt1 = _db.Db.Ado.GetDataTable(sql);

                                                                sql = $"SELECT TOP 1 MeasureValue FROM dbo.CLXDCLZ WHERE SerialNumber = '{ _workpiece.SerialNumber }' AND MeasureStatus = 2 ORDER BY ID desc";
                                                                var vt2 = _db.Db.Ado.GetDataTable(sql);

                                                                float v1 = float.Parse(vt1.Rows[0][0].ToString());
                                                                float v2 = float.Parse(vt2.Rows[0][0].ToString());
                                                                PlcHelper.Write<float>(SystemConfig.ControlDB, _workpiece.StartAddr + 370, v1);
                                                                PlcHelper.Write<float>(SystemConfig.ControlDB, _workpiece.StartAddr + 374, v2);
                                                                LogHelper.Info($"托盘扫码 OP30->{_workpiece.StationCode}->{_workpiece.SerialNumber}->{scanerCode}->{v1}->{v2}");
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                rcode = 102;
                                                                LogHelper.Error(ex, "30 获取v1 v2数据异常");
                                                            }
                                                        }
                                                        PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 360, 0);//本工位合格放行
                                                        LogHelper.Info($"托盘扫码 订单获取完成->{_workpiece.StationCode}->{_workpiece.SerialNumber}->{scanerCode}");
                                                    }
                                                    else
                                                    {
                                                        if (string.IsNullOrEmpty(_workpiece.SerialNumber))
                                                        {
                                                            LogHelper.Info($"托盘扫码 托盘未绑定条码->{_workpiece.StationCode}->->{scanerCode}-> 102");
                                                            rcode = 102;
                                                            PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 360, 2);//

                                                            if (_workpiece.StationIndex == 70)
                                                            {
                                                                rcode = 101;
                                                                //写入空托盘放行信号
                                                                PlcHelper.Write(SystemConfig.ControlDB, _workpiece.StartAddr + 360, (short)2);
                                                                PlcHelper.Write(SystemConfig.ControlDB, _workpiece.StartAddr + 248, (short)101);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            LogHelper.Info($"托盘扫码 本工位合格放行->{_workpiece.StationCode}->{_workpiece.SerialNumber}->{scanerCode}-> 101 1");
                                                            rcode = 101;
                                                            PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 360, 1);//本工位合格放行
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case 3://精追码扫描 40上线
                                        if (!string.IsNullOrEmpty(scanerCode) && scanerCode.Length == 13 && scanerCode.Substring(0, 1) == "D")
                                        {
                                            LogHelper.Info($"托盘扫码 40上线绑定->{_workpiece.SerialNumber}");
                                            BindingData40(scanerCode);
                                            rcode = 101;

                                            try
                                            {
                                                //拧紧曲线数据绑定
                                                using (SqlConnection con = new SqlConnection(SystemConfig.DBStringCurve))
                                                {
                                                    con.Open();
                                                    SqlCommand cmd = new SqlCommand($"update TNResult set vin='{_workpiece.SerialNumber}' where vin='{scanerCode}'", con);
                                                    cmd.ExecuteNonQuery();
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                LogHelper.Error(e, "拧紧曲线数据更新异常");
                                            }
                                        }
                                        break;
                                    case 4://批追码扫描
                                        break;
                                    case 5://创建订单
                                        rcode = QueryOrderInfo(scanerCode, 5);
                                        break;
                                    case 6://80上线扫码
                                        if (!string.IsNullOrEmpty(scanerCode) && scanerCode.Length == 13 && scanerCode.Substring(0, 1) == "D")
                                        {
                                            LogHelper.Info($"托盘扫码 80上线扫码->{_workpiece.SerialNumber}");
                                            QueryOrderInfo80(scanerCode);
                                            rcode = 101;
                                        }
                                        break;
                                    case 7://扫码总成条码 SN
                                        break;
                                }

                                PlcHelper.Write(SystemConfig.ControlDB, _workpiece.StartAddr + 248, rcode);
                            }
                            else if (_workpiece.StationIndex == 10 && !string.IsNullOrEmpty(scanerCode) && scanerCode.Length == 3 && scanerCode.Substring(0, 1) == "A")
                            {
                                //记录空托盘号
                                string sql = "INSERT INTO dbo.EmptyPalletLog(PalletCode,Flag)" +
                                $"VALUES('{ scanerCode }', 0)";
                                _db.Db.Ado.ExecuteCommand(sql);
                                //+360 给2 空托盘放行信号
                                LogHelper.Info($"托盘扫码 空托盘放行信号->{_workpiece.StationCode}->{_workpiece.SerialNumber}->{scanerCode}-> 101 2");
                                PlcHelper.Write(SystemConfig.ControlDB, _workpiece.StartAddr + 248, (short)101);
                                PlcHelper.Write(SystemConfig.ControlDB, _workpiece.StartAddr + 360, (short)2);
                            }

                            //PlcHelper.Write(SystemConfig.ControlDB, _workpiece.StartAddr + 248, rcode);
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
                /*所有工位数据都在一个DB块，每个工位数据占用1000个字节，
                    根据工位位置确定读取的数据起始位置*/
                var startAddress = _workpiece.StartAddr;

                if (string.IsNullOrWhiteSpace(scanerCode))
                {
                    //PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 248, 102);
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
                        LogHelper.Info("QueryOrderInfo-op010->订单获取-1");
                    }
                    else
                    {
                        LogHelper.Info("QueryOrderInfo-op010->订单获取-2->mode>" + mode);
                        //var orders = _db.GoodsOrderDb.GetList(it => it.OrderStatus == 0);
                        //if (orders.Count > 0)
                        //{
                        order = AssignOrder.GetOrder(mode); //_db.GoodsOrderDb.GetList(it => it.OrderStatus == 0).OrderBy(it => it.ID).FirstOrDefault();
                        if (order != null)
                        {
                            LogHelper.Info("QueryOrderInfo->Has->" + mode);
                            order.PalletCode = _workpiece.TrayCode;
                            //order.OrderStatus = 1;
                            order.OnLineTime = DateTime.Now;
                            _db.GoodsOrderDb.Update(order);
                        }
                        else
                        {
                            LogHelper.Info("QueryOrderInfo->null->" + mode);
                        }
                        //}
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
                    PlcHelper.Write<short>(SystemConfig.ControlDB, 16, (short)orders.Count);//当前订单总数
                    PlcHelper.Write<short>(SystemConfig.ControlDB, 18, (short)lastOrders);//当前订单剩余数量
                    PlcHelper.Write<short>(SystemConfig.ControlDB, 22, (short)OKOrdersCount);//当前订单OK数量
                    PlcHelper.Write<short>(SystemConfig.ControlDB, 24, (short)INGOrdersCount);//当前订单生成中数量
                    PlcHelper.Write<short>(SystemConfig.ControlDB, 26, (short)0);//当前订单NG数量
                    PlcHelper.Write<short>(SystemConfig.ControlDB, 28, (short)DayOKOrdersCount);//当天OK数量
                    PlcHelper.Write<short>(SystemConfig.ControlDB, 30, (short)0);//当天NG数量

                    LogHelper.Info($"托盘扫码 QueryOrderInfo->{_workpiece.StationCode}->{ _workpiece.SerialNumber}->" +
                        $"{ _workpiece.ProductTypeCode}->{ _workpiece.MaterielCode}->{_workpiece.Type1}->{_workpiece.Type2}");
                    R = 101;
                }
                else
                {
                    LogHelper.Info("订单异常");
                    _workpiece.SerialNumber = "";
                    //PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 248, 102);
                    R = 102;
                }
            }
            catch (Exception e)
            {
                R = 102;
                LogHelper.Error(e);
            }

            return R;
        }

        private void QueryOrderInfo80(string scanerCode)
        {
            try
            {
                /*所有工位数据都在一个DB块，每个工位数据占用1000个字节，
                    根据工位位置确定读取的数据起始位置*/
                var startAddress = _workpiece.StartAddr;

                if (string.IsNullOrWhiteSpace(scanerCode))
                {
                    PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 248, 102);
                    return;
                }
                //_workpiece.TrayCode = scanerCode;

                GoodsOrder80 order = new GoodsOrder80();

                var orders = _db.GoodsOrder80Db.GetList(it => it.OrderStatus == 50 && it.SerialNumber == scanerCode);
                if (orders.Count <= 0)
                {
                    order = new GoodsOrder80
                    {
                        MaterielCode = "-",
                        ProductionOrderCode = "-",
                        SerialNumber = scanerCode,
                        OrderStatus = 50,
                        CheckResult = 0,
                        LocalTime = DateTime.Now,
                        Type1 = "",
                        Type2 = "",
                        PalletCode = "",
                        ProductType = "50",
                    };

                    _db.GoodsOrder80Db.Insert(order);
                }
                else
                {
                    orders = _db.GoodsOrder80Db.GetList(it => it.OrderStatus == 50 && it.SerialNumber == scanerCode);
                    order = orders.FirstOrDefault();
                }

                if (order != null)
                {
                    _workpiece.SerialNumber = order.SerialNumber;
                    //_workpiece.ProductTypeCode = order.ProductType;
                    PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 248, 101);
                }
                else
                {
                    LogHelper.Info("订单异常");
                    PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 248, 102);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        private void BindingData40(string scanerCode)
        {
            try
            {
                bool b = true;
                /*所有工位数据都在一个DB块，每个工位数据占用1000个字节，
                    根据工位位置确定读取的数据起始位置*/
                var startAddress = _workpiece.StartAddr;

                if (string.IsNullOrWhiteSpace(scanerCode))
                {
                    PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 248, 102);
                    return;
                }
                //_workpiece.TrayCode = scanerCode;

                GoodsOrder80 order80 = new GoodsOrder80();
                //GoodsOrder order;

                var orders80 = _db.GoodsOrder80Db.GetList(it => it.OrderStatus == 50 && it.SerialNumber == scanerCode);
                if (orders80.Count > 0)
                {
                    order80 = orders80.FirstOrDefault();
                    order80.OrderStatus = 51;
                    b &= _db.GoodsOrder80Db.Update(order80);
                }

                string sql = "update dbo.Tighten set SerialNumber='" + _workpiece.SerialNumber + "' where SerialNumber='"+ scanerCode + "'";
                b &= _db.Db.Ado.ExecuteCommand(sql) > 0;
                sql = "update dbo.QualityData set SerialNumber='" + _workpiece.SerialNumber + "' where SerialNumber='" + scanerCode + "'";
                b &= _db.Db.Ado.ExecuteCommand(sql) > 0;
                sql = "update dbo.StationData set SerialNumber='" + _workpiece.SerialNumber + "' where SerialNumber='" + scanerCode + "'";
                b &= _db.Db.Ado.ExecuteCommand(sql) > 0;

                if (order80 != null && b)
                {
                    //_workpiece.SerialNumber = order.SerialNumber;
                    //_workpiece.ProductTypeCode = order.ProductType;
                    PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 248, 101);
                }
                else
                {
                    LogHelper.Info("支线80数据绑定异常");
                    PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 248, 102);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        private void ExactReview(string barCode, int stationIndex)
        {
            try
            {
                var signals = PlcHelper.Read<byte>(SystemConfig.ControlDB, 1002 + 500 * stationIndex);
                if (PlcConvert.GetBitAt(signals, 0, 0))//合法性判断
                {

                }

                if (PlcConvert.GetBitAt(signals, 0, 1))//唯一性判断
                {

                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e, "精追码校验失败：");
            }
        }

        private void Login(string userId, int stationIndex)
        {
            try
            {
                SystemConfig.LoginUser = userId;
                PlcHelper.Write(SystemConfig.ControlDB, 1010 + 500 * stationIndex, userId);
            }
            catch (Exception e)
            {
                LogHelper.Error(e, "登录出错：");
            }
        }

        private bool Check(string stationCode)
        {
            bool checkFlag0 = true;
            //var signals = PlcHelper.Read<short>(SystemConfig.ControlDB, _workpiece.StartAddr + 240, 2);
            
            var formulaNo = PlcHelper.Read<short>(_workpiece.DBAddr1, 0);
            short FNo = formulaNo.Length > 0 && formulaNo[0] > 0 ? formulaNo[0] : (short)1;
            
            string sql =
                $"SELECT * FROM dbo.Formula WHERE StationName = '{stationCode}' AND FormulaNum = '{FNo.ToString("000")}' ORDER BY WorkStep";
            var formulas = _db.Db.Ado.GetDataTable(sql);

            sql =
                $"SELECT * FROM dbo.QualityData WHERE SerialNumber = '{_workpiece.SerialNumber}' AND StationCode = '{stationCode}' ORDER BY StepNo,ID";
            var dataList = _db.Db.Ado.GetDataTable(sql);

            switch (stationCode)
            {
                case "OP025":
                    checkFlag0 = OtherCheckData(dataList, 1);
                    break;
                case "OP045":
                case "OP046":
                    checkFlag0 = OtherCheckData(dataList, 26);
                    break;
                case "OP070":
                    checkFlag0 = OtherCheckData(dataList, 1);
                    break;
                case "OP051":
                    checkFlag0 = OtherCheckData(dataList, 1);
                    break;
                case "OP061":
                    checkFlag0 = OtherCheckData(dataList, 3, 2);
                    break;
                case "OP081":
                case "OP082":
                    checkFlag0 = OtherCheckData(dataList, 25);
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

            string[] operationTypeList = new[] { "3", "4" };//配方中有质量数据的操作类型编号
            //string[] operationTypeList = new[] { "3", "4", "5" };//配方中有质量数据的操作类型编号
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

        private static bool OtherCheckData(DataTable dataList, short count, short startindex = 1)
        {
            bool checkFlag0 = true;

            for (int i = startindex; i <= count; i++)
            {
                bool checkFlag = false;
                foreach (DataRow row in dataList.Rows)
                {
                    if (int.Parse(row["StepNo"].ToString()) == i)
                    {
                        if (row["CheckResult"]?.ToString() == "True")
                        {
                            //校验合格
                            //goto NextData;
                            checkFlag = true;
                            break;
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

            return checkFlag0;
        }
    }
}
