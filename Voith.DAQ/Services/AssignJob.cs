using SqlSugar;
using System;
using System.Data;
using System.Threading;
using Voith.DAQ.Common;
using Voith.DAQ.DB;
using Voith.DAQ.Model;

namespace Voith.DAQ.Services
{
    /// <summary>
    /// 下发工作内容，根据操作结果判断哪些步骤需要重新操作；若是托盘新工件进站，则下发所有工步操作内容
    /// </summary>
    class AssignJob
    {
        /// <summary>
        /// 数据库访问对象
        /// </summary>
        private readonly DbContext _db;

        /// <summary>
        /// 当前工站在位的工件信息
        /// </summary>
        private Workpiece _workpiece;

        public AssignJob(Workpiece workpiece)
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
                        var datas = PlcHelper.Read<short>(SystemConfig.ControlDB, startAddress + 34, 1);
                        //var temp = PlcHelper.ReadBytes(SystemConfig.ControlDB, 3040, 98);
                        //PlcHelper.Write<string>(SystemConfig.ControlDB, 3040, "AAAA");
                        if (datas[0] == 1)
                        {
                            short rcode = 102;
                            //PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 36, Int16.Parse(_workpiece.ProductTypeCode));
                            if (_workpiece.StationIndex == 90)//下发订单产品类型
                            {
                                var goodsOrder = _db.GoodsOrderDb.AsQueryable().Where(it => it.OrderStatus == 1).OrderBy(it => it.ID, OrderByType.Desc).First();
                                if (goodsOrder != null)
                                {
                                    PlcHelper.WriteBytes(SystemConfig.ControlDB, startAddress + 36,
                                        SystemConfig.GetProductionTypes(goodsOrder.MaterielCode, goodsOrder.Type1, goodsOrder.Type2));
                                    LogHelper.Info($"{_workpiece.StationCode}->{goodsOrder.MaterielCode}->{goodsOrder.Type1}->{goodsOrder.Type2}");
                                    rcode = 101;
                                }
                            }
                            else
                            {
                                PlcHelper.WriteBytes(SystemConfig.ControlDB, startAddress + 36,
                                    SystemConfig.GetProductionTypes(_workpiece.MaterielCode, _workpiece.Type1, _workpiece.Type2));
                                PlcHelper.Write<string>(SystemConfig.ControlDB, startAddress + 40 + 2, _workpiece.SerialNumber);
                                LogHelper.Info($"{_workpiece.StationCode}->{_workpiece.MaterielCode}->{_workpiece.Type1}->{_workpiece.Type2}->" +
                                    $"{ _workpiece.SerialNumber}");

                                string sql =
                                    $"SELECT COUNT(1) FROM dbo.Formula WHERE StationName = '{_workpiece.StationCode}'";
                                var stepCount = Convert.ToInt32(_db.Db.Ado.GetScalar(sql));

                                switch (_workpiece.StationIndex)
                                {
                                    case 45:
                                    case 46:
                                        stepCount = 26;
                                        break;
                                    case 61:
                                        stepCount = 3;
                                        break;
                                    case 81:
                                    case 82:
                                        stepCount = 25;
                                        break;
                                    default:
                                        break;
                                }

                                sql =
                                    $"SELECT * FROM dbo.QualityData WHERE SerialNumber = '{_workpiece.SerialNumber}' AND StationCode = '{_workpiece.StationCode}' ORDER BY StepNo,ID";
                                var dataList = _db.Db.Ado.GetDataTable(sql);

                                byte[] enableBytes = new byte[stepCount];
                                string ebstr = "";
                                for (int i = 1; i <= stepCount; i++)
                                {
                                    enableBytes[i - 1] = 1;
                                    foreach (DataRow row in dataList.Rows)
                                    {
                                        if (row["StepNo"]?.ToString() == i.ToString())
                                        {
                                            enableBytes[i - 1] = (byte)(Convert.ToInt32(row["CheckResult"]) == 1 ? 2 : 1);
                                            ebstr += enableBytes[i - 1].ToString() + ",";
                                            //goto End;
                                        }
                                    }

                                    //enableBytes[i - 1] = 1;

                                    //End:;
                                }

                                //OP061 条码使能判断
                                if(_workpiece.StationIndex == 61)
                                {
                                    sql = $"SELECT * FROM dbo.KeyCodeInfo WHERE SerialNumber = '{_workpiece.SerialNumber}' AND StationCode = '{_workpiece.StationCode}' ORDER BY ID desc";
                                    var keycode = _db.Db.Ado.GetDataTable(sql);

                                    enableBytes[0] = (byte)(keycode != null && keycode.Rows.Count > 0 &&
                                    !string.IsNullOrEmpty(keycode.Rows[0]["KeyCode"].ToString()) ? 2 : 1);

                                    ebstr += "KeyCode=" + enableBytes[0].ToString();
                                }

                                PlcHelper.WriteBytes(SystemConfig.ControlDB, startAddress + 140, enableBytes);
                                LogHelper.Info($"{_workpiece.StationCode}->101 使能信号->{ebstr}");
                                rcode = 101;
                            }
                            PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 34, rcode);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                        try
                        {
                            PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 34, 102);
                        }
                        catch { }
                    }

                    Thread.Sleep(300);
                }
            })
            { IsBackground = true }.Start();
        }
    }
}
