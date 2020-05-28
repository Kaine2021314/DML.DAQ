using System;
using System.Threading;
using System.Windows.Forms;
using Voith.DAQ.Common;
using Voith.DAQ.DB;

namespace Voith.DAQ.Services
{
    class TimingExportData
    {
        private static TimingExportType _timingExportType;
        private static string _excelExportPath = string.Empty;
        private static System.Threading.Timer _timer;

        /// <summary>
        /// 设置定时导出计划（设置后，计划任务就开始执行）
        /// </summary>
        /// <param name="timingExportType">定时导出周期</param>
        /// <param name="excelExportPath">定时导出路径</param>
        public static void SetTaskAtFixedTime(TimingExportType timingExportType,string excelExportPath)
        {
            try
            {
                if (timingExportType != _timingExportType)
                {
                    _timer?.Dispose();
                }
                _timingExportType = timingExportType;
                _excelExportPath = excelExportPath;

                DateTime now = DateTime.Now;
                DateTime clock = default;
                switch (_timingExportType)
                {
                    case TimingExportType.Shifts:
                        clock = now.Date.AddHours(20);
                        //clock = now.AddSeconds(5); //测试5s执行一次
                        break;
                    case TimingExportType.Day:
                        clock = now.AddDays(1).Date;
                        //clock = now.AddSeconds(10); //测试10s执行一次
                        break;
                    case TimingExportType.Week:
                        clock = now.AddDays(DayOfWeek.Monday - now.DayOfWeek + 7).Date;
                        break;
                    case TimingExportType.Month:
                        clock = new DateTime(now.AddMonths(1).Year, now.AddMonths(1).Month, 1);
                        break;
                }

                int msUntilFour = (int)((clock - now).TotalMilliseconds);

                _timer = new System.Threading.Timer(DoPlan);
                _timer.Change(msUntilFour, Timeout.Infinite);
            }
            catch (Exception e)
            {
                LogHelper.Error(e, "定时导出数据出错");
                MessageBox.Show(@"定时导出数据出错");
            }
        }

        //要执行的任务
        private static void DoPlan(object state)
        {
            DateTime beginTime = default, endTime = default;
            try
            {
                switch (_timingExportType)
                {
                    case TimingExportType.Shifts:
                        beginTime = DateTime.Now;
                        endTime = DateTime.Now;
                        break;
                    case TimingExportType.Day:
                        beginTime = DateTime.Now.Date.AddDays(-1);
                        endTime = DateTime.Now.Date;
                        break;
                    case TimingExportType.Week:
                        beginTime = DateTime.Now.Date.AddDays(-7);
                        endTime = DateTime.Now.Date;
                        break;
                    case TimingExportType.Month:
                        beginTime = DateTime.Now.Date.AddMonths(-1);
                        endTime = DateTime.Now.Date;
                        break;
                }
                //执行功能...
                DbContext db = new DbContext();
                var dt = db.Db.Ado.UseStoredProcedure().GetDataTable("sp_QueryProductData", new {beginTime, endTime });

                ExcelHelper.ReportToExcel(dt, _excelExportPath);

                //再次设定
                SetTaskAtFixedTime(_timingExportType, _excelExportPath);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "执行导出任务出错");
                MessageBox.Show(@"执行导出任务出错");
            }
        }

        /// <summary>
        /// 定时导出类型
        /// </summary>
        public enum TimingExportType
        {
            /// <summary>
            /// 按班次导出
            /// </summary>
            Shifts,
            /// <summary>
            /// 按天导出
            /// </summary>
            Day,
            /// <summary>
            /// 按周导出
            /// </summary>
            Week,
            /// <summary>
            /// 按月导出
            /// </summary>
            Month
        }
    }
}
