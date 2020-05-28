using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voith.DAQ.Services
{
    /// <summary>
    /// 等待托盘进站
    /// </summary>
    class WaitPalletArrival
    {
        void Handle()
        {
            /**
             * 读取plc数据，判断托盘是否进站
             * 读取托盘号，判断是否是上线工位，如果是则从对单队列获取订单绑定托盘；如果不是则检索托盘绑定的订单
             * 缓存SN等信息，开启线程，轮询PLC判断是否有质量数据需要记录
             */


        }
    }
}
