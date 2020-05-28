using System;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using CCWin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using Voith.DAQ.Common;
using Voith.DAQ.DB;
using Voith.DAQ.Model;
using Voith.DAQ.Services;
using SVW;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NPOI.XSSF.UserModel;
using System.Text;

namespace Voith.DAQ.UI
{
    public partial class FrmMain : Skin_Color
    {
        delegate void OdinfoUpdate();
        DbContext _db = new DbContext();
        public FrmMain()
        {
            InitializeComponent();

            LogHelper.Info($"Start Program {Application.ProductVersion} ...");

            JsonConfigHelper config = new JsonConfigHelper("Config.json");
            var str = config["StationList"];
            SystemConfig.StationList = (JArray)JsonConvert.DeserializeObject(str);

            SystemConfig.DBStringCurve = config["DBStringCurve"];

            //txtTitle.Text += "-" + config["PlcIpAddress"].ToString();
        }

        private void FrmMain_Shown(object sender, System.EventArgs e)
        {
            UpdateSystemStatus();

            //new DB.DbContext().Db.CodeFirst.InitTables(typeof(GoodsOrder), typeof(ProductionOrder), typeof(Formula));//代码先行，创建数据库
            //new DB.DbContext().Db.CodeFirst.InitTables(typeof(GoodsOrder80));

            var factory = new ThreadFactory();
            new Task(factory.Run).Start();
        }

        /// <summary>
        /// 更新系统状态（心跳，产线就绪）
        /// </summary>
        private void UpdateSystemStatus()
        {
            skinGroupBox5.Visible = false;

            new Thread(() =>
                {
                    //写入mes就绪信号
                    PlcHelper.Write<bool>(1045, 0, true, 2);
                    var lifeBeat = true;
                    while (true)
                    {
                        try
                        {
                            var bytes = PlcHelper.ReadBytes(1045, 0, 1);

                            //plc系统就绪信号
                            this.lbReady.ForeColor = PlcConvert.GetBitAt(bytes, 0, 0) ? Color.Green : Color.Red;

                            //plc心跳信号
                            this.lbLifeBeat.ForeColor = PlcConvert.GetBitAt(bytes, 0, 1) ? Color.Green : Color.Gray;

                            //写入心跳信号
                            PlcHelper.Write<bool>(1045, 0, lifeBeat = !lifeBeat, 3);
                        }
                        catch 
                        {
                            lbReady.ForeColor = lbLifeBeat.ForeColor = Color.Red;
                            PlcHelper.ReConn(true);
                        }
                        try
                        {
                            var goodsOrder = _db.GoodsOrderDb.AsQueryable().Where(it => it.HeadOrder == 1).OrderBy(it => it.ID, OrderByType.Desc).First();

                            if (goodsOrder != null)
                            {
                                var goodsOrderLst = _db.GoodsOrderDb.GetList().Where(it => (it.ID >= goodsOrder.ID && it.ProductionOrderCode == goodsOrder.ProductionOrderCode)).ToList();

                                SystemConfig.OrderInfo oi = new SystemConfig.OrderInfo();
                                for (int i = 0; i < goodsOrderLst.Count; i++)
                                {
                                    if (goodsOrderLst[i].PalletCode != "-")
                                    {
                                        oi.OnlineCount++;
                                        oi.SerialNumber = goodsOrderLst[i].SerialNumber;
                                        oi.MaterielCode = goodsOrderLst[i].MaterielCode;
                                        oi.Type1 = goodsOrderLst[i].Type1;
                                        oi.Type2 = goodsOrderLst[i].Type2;
                                    }
                                    if (goodsOrderLst[i].OrderStatus == 1 || (goodsOrderLst[i].OrderStatus == 0 && goodsOrderLst[i].PalletCode != "-"))
                                    {
                                        oi.OfflineCount++;
                                    }
                                    if (goodsOrderLst[i].OrderStatus == 2 || goodsOrderLst[i].OrderStatus == 3)
                                    {
                                        if (goodsOrderLst[i].CheckResult == 1)
                                            oi.OKCount++;
                                        else if (goodsOrderLst[i].CheckResult == 2)
                                            oi.NOKCount++;
                                    }
                                }

                                SystemConfig.orderInfo.PCout = goodsOrderLst.Count;
                                SystemConfig.orderInfo.ProductionOrderCode = goodsOrder.ProductionOrderCode;
                                SystemConfig.orderInfo.MaterielCode = goodsOrder.MaterielCode;
                                SystemConfig.orderInfo.Type1 = goodsOrder.Type1;
                                SystemConfig.orderInfo.Type2 = goodsOrder.Type2;
                                SystemConfig.orderInfo.SerialNumber = oi.SerialNumber;
                                SystemConfig.orderInfo.OnlineCount = oi.OnlineCount;
                                SystemConfig.orderInfo.OfflineCount = oi.OfflineCount;
                                SystemConfig.orderInfo.OKCount = oi.OKCount;
                                SystemConfig.orderInfo.NOKCount = oi.NOKCount;
                                Invoke(new OdinfoUpdate(UpdateOrderInfo));
                            }
                        }
                        catch
                        {
                        }

                        Thread.Sleep(1000);
                    }
                })
            { IsBackground = true }.Start();
        }

        void UpdateOrderInfo()
        {
            label35.Visible = SystemConfig.SystemInitError;

            label13.Text = SystemConfig.orderInfo.ProductionOrderCode;
            label32.Text = $"{SystemConfig.orderInfo.MaterielCode}-" +
                $"{SystemConfig.orderInfo.Type1}-{SystemConfig.orderInfo.Type2}";
            label7.Text = SystemConfig.orderInfo.SerialNumber;
            label8.Text = SystemConfig.orderInfo.OnlineCount.ToString();
            label9.Text = SystemConfig.orderInfo.OfflineCount.ToString();
            label10.Text = SystemConfig.orderInfo.OKCount.ToString();
            label11.Text = SystemConfig.orderInfo.NOKCount.ToString();
            label34.Text = SystemConfig.orderInfo.PCout.ToString();
        }

        /// <summary>
        /// 关闭程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            //MessageBoxEx.Show(this,"关闭窗口");
            LogoutForm fm = new LogoutForm();
            fm.ShowDialog();

            if (fm.QuitPW == "123456")
            {

            }
            else
                e.Cancel = true;
        }

        private void FrmMain_Load(object sender, System.EventArgs e)
        {
            //ProgressBarHelper p = new ProgressBarHelper(this, "正在启动");
            //p.Show();
        }

        /// <summary>
        /// 导入配方
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFormulaImport_ClickAsync(object sender, System.EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            { Filter = @"Excel|*.xls;*.xlsx" };

            //判断用户是否正确的选择了文件
            if (fileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            //显示遮罩层
            ProgressBarHelper p = new ProgressBarHelper(this, "正在导入");

            FormulaImportType importType = DgbImportType.Show(fileDialog.FileName, p);
            if (importType == FormulaImportType.Cancel)
            {
                return;
            }

            p.Show();
        }

        /// <summary>
        /// 数据查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDataQuery_Click(object sender, System.EventArgs e)
        {
            //ExcelHelper.Excel2Pdf(@"D:\填写范例.xlsx", @"D:\11111.pdf");
            DataSelect fm = new DataSelect();
            fm.ShowDialog();
        }

        private void skinButton6_Click(object sender, System.EventArgs e)
        {
            OrderManage fm = new OrderManage();
            fm.ShowDialog();
        }
    }
}
