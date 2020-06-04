using System;
using System.Collections;
using System.Threading;
using System.Windows.Forms;
using SqlSugar;
using Voith.DAQ.Common;
using Voith.DAQ.DB;
using Voith.DAQ.Model;

namespace Voith.DAQ.Services
{
    /// <summary>
    /// 下发订单
    /// </summary>
    class AssignOrder
    {
        private static readonly DbContext Db = new DbContext();

        /// <summary>
        /// 获取一个未上线的订单
        /// </summary>
        /// <returns></returns>
        public static GoodsOrder GetOrder(int mode)
        {
            try
            {
                //BEGIN:
                //获取最早一个未上线的订单
                var goodsOrder = Db.GoodsOrderDb.AsQueryable().Where(it => it.OrderStatus == 0).OrderBy(it => it.ID, OrderByType.Asc).First();

                //判断是否有订单，如果没有，则进入创建订单页面
                if (goodsOrder == null)
                {
                    LogHelper.Info("GetOrder-无可用订单项");
                    //OP010 告知订单下发异常 订单用完
                    //PlcHelper.Write(SystemConfig.ControlDB, 132, (short)102);//订单获取失败
                }
                else
                {
                    LogHelper.Info("GetOrder-订单获取到");
                    //PlcHelper.Write(SystemConfig.DTControlDB, 132, (short)101);//订单获取成功
                }

                return goodsOrder;
            }
            catch (Exception e)
            {
                LogHelper.Error(e, "下发订单出错");
                //MessageBox.Show("下发订单出错！");
                return null;
            }
        }

        /// <summary>
        /// 进入订单创建模式---
        /// </summary>
        private static void CreateOrder()
        {
            //写入当前MES模式（订单模式）
            PlcHelper.Write<short>(SystemConfig.ControlDB, 20, 1);

            string materialNumber = string.Empty;//物料号
            string orderCode = String.Empty;//工位号
            string orderQty = string.Empty;//订单数量
            string firstSerialNumber = string.Empty;//第一个订单SN
                                                    //string 
                                                    //
                                                    //
                                                    //
                                                    //
                                                    //MaterielCode 14位 物料号   materialNumber
                                                    //ProductionOrderCode 9位 订单号 orderCode
                                                    //orderQty 2位 订单数量
                                                    //firstSerialNumber 8位 起始订单号
            string type1 = string.Empty;            //动定轮类型 TC/WAG
            string type2 = string.Empty;            //节流板类型 9.9/10.5
            BitArray barr = new BitArray(16);
            while (true)
            {
                try
                {
                    var bytes = PlcHelper.ReadBytes(SystemConfig.ControlDB, 1248, 102);
                    var signal = PlcConvert.GetIntAt(bytes, 0);

                    var TrayStatus = PlcHelper.Read<short>(SystemConfig.ControlDB, 1356, 2);
                    var v0 = TrayStatus[0];
                    if (v0 != 0)
                        break;

                    if (signal == 1)
                    {
                        var barcode = PlcConvert.GetStringAt(bytes, 2).Trim('\0').Trim('\r').Trim().Replace("\n","").Replace("\r", "");
                        LogHelper.Info("订单创建扫码 CreateOrder->-" + barcode + "-");
                        switch (barcode.Length)
                        {
                            case 14:
                                if (SystemConfig.HasItem(SystemConfig.MaterielCode, barcode))
                                {
                                    LogHelper.Info($"订单创建扫码 CreateOrder-materialNumber>{materialNumber}");
                                    materialNumber = barcode;
                                    PlcHelper.Write(SystemConfig.ControlDB, 1248, (short)101);
                                    barr.Set(0, true);
                                }
                                else
                                    PlcHelper.Write(SystemConfig.ControlDB, 1248, (short)102);
                                break;
                            case 9:
                                LogHelper.Info($"订单创建扫码 CreateOrder-orderCode>{orderCode}");
                                orderCode = barcode;
                                PlcHelper.Write(SystemConfig.ControlDB, 1248, (short)101);
                                barr.Set(1, true);
                                break;
                            case 8:
                                LogHelper.Info($"订单创建扫码 CreateOrder-firstSerialNumber>{firstSerialNumber}");
                                firstSerialNumber = barcode;//3095782
                                PlcHelper.Write(SystemConfig.ControlDB, 1248, (short)101);
                                barr.Set(3, true);
                                break;
                            //case 2:
                            //    orderQty = barcode;
                            //    break;
                            default:
                                if (!string.IsNullOrEmpty(barcode))
                                {
                                    if (SystemConfig.HasItem(SystemConfig.Type1List, barcode))
                                    {
                                        type1 = barcode;
                                        PlcHelper.Write(SystemConfig.ControlDB, 1248, (short)101);
                                        barr.Set(4, true);
                                        LogHelper.Info($"订单创建扫码 CreateOrder-type1>{type1}");
                                    }
                                    else if (SystemConfig.HasItem(SystemConfig.Type2List, barcode))
                                    {
                                        type2 = barcode;
                                        PlcHelper.Write(SystemConfig.ControlDB, 1248, (short)101);
                                        barr.Set(5, true);
                                        LogHelper.Info($"订单创建扫码 CreateOrder-type2>{type2}");
                                    }
                                    else
                                    {
                                        LogHelper.Info($"订单创建扫码 CreateOrder-count>{barcode}");

                                        if (!string.IsNullOrEmpty(barcode) && barcode.Length == 3 && barcode.Substring(0, 1) == "A")
                                        {
                                            PlcHelper.Write(SystemConfig.ControlDB, 1248, (short)1002);
                                            break;
                                        }

                                        byte v;
                                        byte.TryParse(barcode, out v);
                                        if (barcode.Length < 4 && v >= 0)
                                        {
                                            orderQty = v.ToString();
                                            PlcHelper.Write(SystemConfig.ControlDB, 1248, (short)101);
                                            barr.Set(2, true);
                                        }
                                        else
                                            PlcHelper.Write(SystemConfig.ControlDB, 1248, (short)102);
                                    }
                                }
                                break;
                        }
                        PlcHelper.Write(SystemConfig.ControlDB, 1374, (short)BitToUshort(barr));//六个条码的扫码状态
                        LogHelper.Info($"订单创建扫码 CreateOrder-0->{materialNumber}-{orderCode}-{orderQty}-{firstSerialNumber}-{type1}-{type2}");

                        if (!string.IsNullOrWhiteSpace(materialNumber) && !string.IsNullOrWhiteSpace(orderCode) &&
                            !string.IsNullOrWhiteSpace(orderQty) && !string.IsNullOrWhiteSpace(firstSerialNumber) &&
                            !string.IsNullOrWhiteSpace(type1) && !string.IsNullOrWhiteSpace(type2))
                        {
                            byte pt = SystemConfig.GetMaterielCodeType(materialNumber);
                            string ptStr = pt.ToString();

                            string sn = firstSerialNumber;
                            //int lastNo = Convert.ToInt32(firstSerialNumber.Substring(firstSerialNumber.Length - 1, 1));
                            int qty = Convert.ToInt32(orderQty);
                            for (int i = 0; i < qty; i++)
                            {
                                GoodsOrder order = new GoodsOrder
                                {
                                    MaterielCode = materialNumber,
                                    ProductionOrderCode = orderCode,
                                    SerialNumber = (Convert.ToInt32(sn) + i).ToString(),//sn + (lastNo + i).ToString(),
                                    OrderStatus = 0,
                                    CheckResult = 0,
                                    LocalTime = DateTime.Now,
                                    Type1 = type1,
                                    Type2 = type2,

                                    PalletCode = "-",
                                    ProductType = ptStr,
                                };

                                if (i == 0)
                                {
                                    order.Count = qty;
                                    order.HeadOrder = 1;
                                    order.RCount = qty;
                                }

                                Db.GoodsOrderDb.Insert(order);
                            }

                            //写入订单数量
                            PlcHelper.Write(SystemConfig.ControlDB, 16, (short)qty);
                            //写入当前MES模式（生产模式）
                            string dt = DateTime.Now.ToString("yyyy-MM-dd");
                            dt = string.Format("'{0} 00:00:01' and '{1} 23:59:59'", dt, dt);
                            //var currentOrderNum = Db.GoodsOrderDb.AsQueryable().Where("OrderStatus=1 and OnLineTime between" + dt).Count();
                            PlcHelper.Write<short>(SystemConfig.ControlDB, 20, (short)qty);

                            //切换到生成模式
                            PlcHelper.Write(SystemConfig.ControlDB, 14, (short)2);

                            return;
                        }
                        else
                        {
                            LogHelper.Info($"订单创建扫码 CreateOrder->{materialNumber}-{orderCode}-{orderQty}-{firstSerialNumber}-{type1}-{type2}");
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }

                Thread.Sleep(300);
            }
        }

        public static int BitToUshort(BitArray bit)
        {
            int[] res = new int[1];
            for (int i = 0; i < bit.Count; i++)
            {
                bit.CopyTo(res, 0);
            }
            return res[0];
        }
    }
}
