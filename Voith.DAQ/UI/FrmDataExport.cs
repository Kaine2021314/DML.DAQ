using CCWin;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Voith.DAQ.Common;
using Voith.DAQ.Services;

namespace Voith.DAQ.UI
{
    public partial class FrmDataExport : Skin_Color
    {
        public FrmDataExport()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 加载定时导出的配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmDataExport_Load(object sender, EventArgs e)
        {
            var config = new JsonConfigHelper("Config.json");
            txtExportPath.Text = config["ExcelExportPath"];

            var exportType = config["ExcelExportType"];

            if (string.IsNullOrWhiteSpace(exportType))
            {
                return;
            }

            switch (exportType)
            {
                case "Shifts":
                    rbClass.Checked = true;
                    break;
                case "Day":
                    rbDay.Checked = true;
                    break;
                case "Week":
                    rbWeek.Checked = true;
                    break;
                case "Month":
                    rbMonth.Checked = true;
                    break;
            }
        }

        /// <summary>
        /// 选择定时导出文件的路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSelectExportPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            txtExportPath.Text = dialog.SelectedPath + @"\";
        }

        /// <summary>
        /// 设置定时导出计划并执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtExportPath.Text))
            {
                MessageBoxEx.Show(this, "请选择导出路径");
                return;
            }

            #region 获取选择的定时导出周期
            TimingExportData.TimingExportType timingExportType;
            if (rbClass.Checked)
            {
                timingExportType = TimingExportData.TimingExportType.Shifts;
            }
            else if (rbDay.Checked)
            {
                timingExportType = TimingExportData.TimingExportType.Day;
            }
            else if (rbWeek.Checked)
            {
                timingExportType = TimingExportData.TimingExportType.Week;
            }
            else if (rbMonth.Checked)
            {
                timingExportType = TimingExportData.TimingExportType.Month;
            }
            else
            {
                MessageBoxEx.Show(this, "请选择导出周期");
                return;
            }
            #endregion

            TimingExportData.SetTaskAtFixedTime(timingExportType, txtExportPath.Text);

            var config = new JsonConfigHelper("Config.json")
            {
                ["ExcelExportPath"] = txtExportPath.Text,
                ["ExcelExportType"] = timingExportType.ToString()
            };
            config.Save();

            MessageBoxEx.Show(this, "设置完成", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 区间导出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExport_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            var path = dialog.SelectedPath + @"\";

            //显示遮罩层
            ProgressBarHelper p = new ProgressBarHelper(this, "正在导入");
            p.Show();
            Task.Run(() =>
                ManualExportData.ExportSection(dtpBegin.Value.Date, dtpEnd.Value.Date.AddDays(1).AddMilliseconds(-1),
                    path, p));

            MessageBoxEx.Show(this, "导出成功！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 校验选择的区间时间是否合法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DtpEnd_ValueChanged(object sender, EventArgs e)
        {
            DateTime beginTime = dtpBegin.Value;
            DateTime endTime = dtpEnd.Value;

            if (beginTime > endTime)
            {
                MessageBoxEx.Show(this, "开始时间不能大于截止时间！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        /// <summary>
        /// 导出单个SN所有数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnQuery_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSerialNumber.Text))
            {
                MessageBoxEx.Show(this, "序列号不能为空！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            var path = dialog.SelectedPath + @"\";

            //显示遮罩层
            ProgressBarHelper p = new ProgressBarHelper(this, "正在导入");
            p.Show();
            Task.Run(() => ManualExportData.ExportSingle(txtSerialNumber.Text, path, p));

            MessageBoxEx.Show(this, "导出成功！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
