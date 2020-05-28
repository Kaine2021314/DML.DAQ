using System;
using System.Threading;
using CCWin;
using Voith.DAQ.Common;

namespace Voith.DAQ.UI
{
    public partial class FrmCreateOrder : Skin_Color
    {
        public FrmCreateOrder()
        {
            InitializeComponent();
        }

        private void FrmCreateOrder_Load(object sender, EventArgs e)
        {
            ReadOrderInfo();
        }

        /// <summary>
        /// 读取工单信息
        /// </summary>
        private void ReadOrderInfo()
        {
            new Thread(() =>
            {
                //写入当前MES模式（订单模式）
                PlcHelper.Write<short>(1030, 20, 1);

                while (true)
                {
                    //读取PLC请求的扫码信号
                    var bytes = PlcHelper.ReadBytes(SystemConfig.ControlDB, 1, 100);

                    var read1MaterialNumberFlag = PlcConvert.GetIntAt(bytes, 1); //PlcHelper.Read<short>(1030, 20, 1);//读取物料号标识
                    var read1OrderCodeFlag = PlcConvert.GetIntAt(bytes, 1);//PlcHelper.Read<short>(1030, 20, 1);//读取工单号标识
                    var read1OrderQtyFlag = PlcConvert.GetIntAt(bytes, 1);//PlcHelper.Read<short>(1030, 20, 1);//读取订单数量标识

                    Thread.Sleep(1000);
                }
            })
            { IsBackground = true }.Start();
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            MessageBoxEx.Show(this, "fasdf", "123");
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {

        }
    }
}
