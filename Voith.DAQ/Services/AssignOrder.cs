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
