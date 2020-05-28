using System;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using Voith.DAQ.Common;
using Voith.DAQ.DB;
using Voith.DAQ.Model;
using Voith.DAQ.UI;

namespace Voith.DAQ.Services
{
    public class ThreadFactory
    {
        /// <summary>
        /// 数据库访问对象
        /// </summary>
        private readonly DbContext _db = new DbContext();

        /// <summary>
        /// 当前工位号
        /// </summary>
        //private readonly string _stationCode;

        /// <summary>
        /// 当前工位顺序
        /// </summary>
        //private readonly int _stationIndex;

        /// <summary>
        /// 托盘号
        /// </summary>
        //private string _trayCode;

        /// <summary>
        /// 流水号
        /// </summary>
        //private string _serialNumber;

        /// <summary>
        /// 产品型号
        /// </summary>
        //private string _productTypeCode;

        ///// <summary>
        ///// 构造函数
        ///// </summary>
        ///// <param name="stationCode">工位号</param>
        ///// <param name="stationIndex">工位顺序</param>
        //public ThreadFactory()
        //{
        //    //_dbBlock = dbBlock;
        //    _db = new DbContext();
        //    _stationCode = stationCode;
        //    _stationIndex = stationIndex;

        //    Run();
        //}

        public void Run()
        {
            JsonConfigHelper config = new JsonConfigHelper("Config.json");
            var str = config["StationList"];
            var jArray = (JArray)JsonConvert.DeserializeObject(str);
            string stationCode = "";

            foreach (var obj in jArray)
            {
                try
                {
                    stationCode = obj["StationCode"]?.ToString();
                    int stationIndex = Convert.ToInt32(obj["StationIndex"]);
                    int DBAddr1 = Convert.ToInt32(obj["DBAddr1"]);//配方DB地址
                    int StartAddr = Convert.ToInt32(obj["StartAddr"]);//MES Control
                    int DStartAddr = Convert.ToInt32(obj["DStartAddr"]);//MES Date
                    //int EKSStartAddr = Convert.ToInt32(obj["EKSStartAddr"]);//
                    //if (stationIndex != 36)
                    //    continue;

                    LogHelper.Info("启动线程->" + stationCode);

                    //获取订单信息
                    byte[] rb = PlcHelper.ReadBytes(SystemConfig.ControlDB, StartAddr + 40 + 2, 8);

                    if(stationIndex == 10)
                        PlcHelper.Write(SystemConfig.ControlDB, 14, (short)2);

                    string sn = Encoding.ASCII.GetString(rb);
                    //if (sn == null || sn.Length < 1)
                    //    continue;
                    var goodsOrder = _db.GoodsOrderDb.AsQueryable().Where(it => it.SerialNumber == sn.Trim()).OrderBy(it => it.ID, OrderByType.Desc).First();

                    Workpiece workpiece;
                    if (goodsOrder != null)
                    {
                        workpiece = new Workpiece
                        {
                            StationCode = stationCode,
                            StationIndex = stationIndex,
                            DBAddr1 = DBAddr1,
                            StartAddr = StartAddr,
                            DStartAddr = DStartAddr,
                            //EKSStartAddr = EKSStartAddr,

                            SerialNumber = goodsOrder.SerialNumber,
                            TrayCode = goodsOrder.PalletCode,
                            ProductTypeCode = goodsOrder.ProductType,
                            MaterielCode = goodsOrder.MaterielCode,
                            Type1 = goodsOrder.Type1,
                            Type2 = goodsOrder.Type2
                        };
                        LogHelper.Info($"{stationCode}->{goodsOrder.ID}->{goodsOrder.SerialNumber}");
                    }
                    else
                    {
                        workpiece = new Workpiece
                        {
                            StationCode = stationCode,
                            StationIndex = stationIndex,
                            DBAddr1 = DBAddr1,
                            StartAddr = StartAddr,
                            DStartAddr = DStartAddr,
                            //EKSStartAddr = EKSStartAddr
                        };
                    }

                    new AssignJob(workpiece);
                    new RecordData(workpiece);
                    new AssignFormula(workpiece);
                    new CheckStationData(workpiece);
                    new ScanFlow(workpiece);
                }
                catch (Exception ex)
                {
                    SystemConfig.SystemInitError = true;
                    LogHelper.Error(ex, $"{stationCode}初始化失败->{ex.Message}");
                }
            }
        }

        /// <summary>
        /// 检测工位装配结果
        /// </summary>
        /// <param name="palletCode">托盘号</param>
        /// <param name="dbAddress">检测结果写入的DB地址</param>
        private bool CheckStationResult(string palletCode, int dbAddress)
        {
            try
            {
                //string sql = $"SELECT SerialNumber FROM dbo.GoodsOrder WHERE PalletCode = '{palletCode}'";
                //var sn = _db.Db.Ado.GetScalar(sql)?.ToString();
                //var stationCode = "OP" + PlcHelper.Read<int>(_dbBlock, 1, 1);

                //sql =
                //    $"SELECT * FROM dbo.QualityData WHERE SerialNumber = '{sn}' AND StationCode = '{stationCode}' ORDER BY StepNo";
                //var dt = _db.Db.Ado.GetDataTable(sql);

                //var stepCount = 1;//步骤总数，从配方取
                //for (int i = 1; i <= stepCount; i++)
                //{
                //    //判断是否缺少步骤，或者装配结果为不合格
                //    if (dt.Rows[i]["StepNo"]?.ToString() != i.ToString() || dt.Rows[i]["CheckResult"]?.ToString() != "1")
                //    {
                //        PlcHelper.Write<short>(_dbBlock, dbAddress, (short)102);
                //        return false;
                //    }
                //}

                //PlcHelper.Write<short>(_dbBlock, dbAddress, (short)101);
                return true;
            }
            catch (Exception e)
            {
                LogHelper.Error(e, "CheckStationResult");
                return false;
            }
        }

        private void DownloadWorkInfo(string palletCode)
        {
            try
            {
                GoodsOrder goodsOrder =
                    _db.Db.Queryable<GoodsOrder>().First(it => it.PalletCode == palletCode && it.OrderStatus == 1);

                if (goodsOrder == null)
                {
                //BEGIN:
                    goodsOrder = _db.GoodsOrderDb.GetList(it => it.OrderStatus == 0).OrderBy(it => it.LocalTime)
                        .FirstOrDefault();
                    if (goodsOrder == null)
                    {
                        //进入订单创建模式
                        FrmCreateOrder frmCreateOrder = new FrmCreateOrder();
                        frmCreateOrder.ShowDialog();
                        //goto BEGIN;
                    }
                    goodsOrder.PalletCode = palletCode;
                    goodsOrder.OrderStatus = 1;
                    goodsOrder.OnLineTime = DateTime.Now;

                    _db.GoodsOrderDb.Update(goodsOrder);
                }

                var productType = goodsOrder.ProductType;
                var sn = goodsOrder.SerialNumber;
                //var enable = "";



            }
            catch (Exception e)
            {
                LogHelper.Error(e, "DownloadWorkInfo");
                //throw;
            }
        }

    }
}
