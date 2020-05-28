using System;
using System.Windows.Forms;
using Spire.Xls;
using Voith.DAQ.Common;
using Voith.DAQ.DB;

namespace Voith.DAQ.Services
{
    class ManualExportData
    {
        /// <summary>
        /// 导出指定时间段内的数据
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="path"></param>
        /// <param name="progressBar"></param>
        public static void ExportSection(DateTime beginTime, DateTime endTime, string path, ProgressBarHelper progressBar)
        {
            try
            {
                DbContext db = new DbContext();
                var dt = db.Db.Ado.UseStoredProcedure().GetDataTable("sp_QueryProductData", new { beginTime, endTime });

                ExcelHelper.ReportToExcel(dt, path);
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "导出区间数据出错");
                MessageBox.Show(@"导出区间数据出错");
            }
            finally
            {
                progressBar.Close();
            }
        }

        /// <summary>
        /// 根据sn导出所有数据
        /// </summary>
        /// <param name="sn"></param>
        /// <param name="path"></param>
        /// <param name="progressBar"></param>
        public static void ExportSingle(string sn, string path, ProgressBarHelper progressBar)
        {
            try
            {
                DbContext db = new DbContext();
                var ds = db.Db.Ado.UseStoredProcedure().GetDataSetAll("sp_QueryProductData", new { sn });

                Workbook workbook = new Workbook();
                workbook.LoadFromFile(System.Environment.CurrentDirectory +
                                      "\\ExcelTemplate\\Retarder_Data_Retarder_Template.xlsx");
                Worksheet sheet = workbook.Worksheets[0];
                sheet.Range["A2"].Text = $"序列号：{sn} 生产时间：2019-7-19";
                sheet.Range["A2"].Style.HorizontalAlignment = HorizontalAlignType.Center;
                sheet.Range[2, 1, 2, 10].Merge();
                int firstRows = 4;
                sheet.Range[firstRows - 1, 1, firstRows - 1, 10].Merge();
                sheet.Range[firstRows - 1, 1].Text = "压装数据";
                sheet.Range[firstRows - 1, 1].Style.Font.IsBold = true;
                sheet.Range[firstRows - 1, 1].Style.HorizontalAlignment = HorizontalAlignType.Center;
                sheet.InsertDataTable(ds.Tables[1], true, firstRows, 1);
                firstRows = firstRows + ds.Tables[1].Rows.Count + 2;
                sheet.Range[firstRows - 1, 1, firstRows - 1, 10].Merge();
                sheet.Range[firstRows - 1, 1].Text = "选垫数据";
                sheet.Range[firstRows - 1, 1].Style.Font.IsBold = true;
                sheet.Range[firstRows - 1, 1].Style.HorizontalAlignment = HorizontalAlignType.Center;
                sheet.InsertDataTable(ds.Tables[2], true, firstRows, 1);
                firstRows = firstRows + ds.Tables[2].Rows.Count + 2;
                sheet.Range[firstRows - 1, 1, firstRows - 1, 10].Merge();
                sheet.Range[firstRows - 1, 1].Text = "气密测试数据";
                sheet.Range[firstRows - 1, 1].Style.Font.IsBold = true;
                sheet.Range[firstRows - 1, 1].Style.HorizontalAlignment = HorizontalAlignType.Center;
                sheet.InsertDataTable(ds.Tables[3], true, firstRows, 1);
                firstRows = firstRows + ds.Tables[3].Rows.Count + 2;
                sheet.Range[firstRows - 1, 1, firstRows - 1, 10].Merge();
                sheet.Range[firstRows - 1, 1].Text = "力矩记录数据";
                sheet.Range[firstRows - 1, 1].Style.Font.IsBold = true;
                sheet.Range[firstRows - 1, 1].Style.HorizontalAlignment = HorizontalAlignType.Center;
                sheet.InsertDataTable(ds.Tables[4], true, firstRows, 1);

                ExcelHelper.SaveXls(workbook, path, sn);

                //ExcelHelper.ReportToExcel(dt, path);
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "导出单工件数据出错");
                MessageBox.Show(@"导出单工件数据出错");
            }
            finally
            {
                progressBar.Close();
            }
        }
    }
}
