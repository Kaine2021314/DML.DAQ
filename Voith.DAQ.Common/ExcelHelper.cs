using System;
using Spire.Xls;
using System.Data;
using System.IO;

namespace Voith.DAQ.Common
{
    /// <summary>
    /// Excel帮助类
    /// </summary>
    public class ExcelHelper
    {
        #region 导入
        /// <summary>
        /// 将Excel以文件流转换DataTable
        /// </summary>
        /// <param name="hasTitle">是否有表头</param>
        /// <param name="path">文件路径</param>
        /// <param name="tableindex">文件簿索引</param>
        public static DataTable ExcelToDataTableFormPath(bool hasTitle = true, string path = "", int tableindex = 0)
        {
            //新建Workbook
            Workbook workbook = new Workbook();
            //将当前路径下的文件内容读取到workbook对象里面
            workbook.LoadFromFile(path);
            //得到第一个Sheet页
            Worksheet sheet = workbook.Worksheets[tableindex];
            return SheetToDataTable(hasTitle, sheet);
        }
        /// <summary>
        /// 将Excel以文件流转换DataTable
        /// </summary>
        /// <param name="hasTitle">是否有表头</param>
        /// <param name="stream">文件流</param>
        /// <param name="tableindex">文件簿索引</param>
        public static DataTable ExcelToDataTableFormStream(bool hasTitle = true, Stream stream = null, int tableindex = 0)
        {
            //新建Workbook
            Workbook workbook = new Workbook();
            //将文件流内容读取到workbook对象里面
            workbook.LoadFromStream(stream);
            //得到第一个Sheet页
            Worksheet sheet = workbook.Worksheets[tableindex];

            int iRowCount = sheet.Rows.Length;
            int iColCount = sheet.Columns.Length;
            DataTable dt = new DataTable();
            //生成列头
            for (int i = 0; i < iColCount; i++)
            {
                var name = "column" + i;
                if (hasTitle)
                {
                    var txt = sheet.Range[1, i + 1].Text;
                    if (!string.IsNullOrEmpty(txt)) name = txt;
                }
                while (dt.Columns.Contains(name)) name = name + "_1";//重复行名称会报错。
                dt.Columns.Add(new DataColumn(name, typeof(string)));
            }
            //生成行数据
            int rowIdx = hasTitle ? 2 : 1;
            for (int iRow = rowIdx; iRow <= iRowCount; iRow++)
            {
                DataRow dr = dt.NewRow();
                for (int iCol = 1; iCol <= iColCount; iCol++)
                {
                    dr[iCol - 1] = sheet.Range[iRow, iCol].Text;
                }
                dt.Rows.Add(dr);
            }
            return SheetToDataTable(hasTitle, sheet);
        }

        /// <summary>
        /// 将Excel以文件流转换DataSet
        /// </summary>
        /// <param name="hasTitle">是否有表头</param>
        /// <param name="path">文件路径</param>
        public static DataSet ExcelToDataSetFormPath(bool hasTitle = true, string path = "")
        {
            //新建Workbook
            Workbook workbook = new Workbook();
            //将当前路径下的文件内容读取到workbook对象里面
            workbook.LoadFromFile(path);

            DataSet ds = new DataSet();
            foreach (var worksheet in workbook.Worksheets)
            {
                var sheet = (Worksheet) worksheet;
                if (!sheet.Name.Contains("OP"))
                    continue;
                var dt = SheetToDataTable(hasTitle, sheet);
                //Console.WriteLine(sheet.Name);
                dt.TableName = sheet.Name;
                ds.Tables.Add(dt);
            }

            return ds;
        }

        private static DataTable SheetToDataTable(bool hasTitle, Worksheet sheet)
        {
            int iRowCount = sheet.Rows.Length;
            int iColCount = sheet.Columns.Length;
            var dt = new DataTable();
            //生成列头
            for (var i = 0; i < iColCount; i++)
            {
                var name = "column" + i;
                if (hasTitle)
                {
                    var txt = sheet.Range[1, i + 1].Text;
                    if (!string.IsNullOrEmpty(txt)) name = txt;
                    else
                    {
                        iColCount = i;
                        break;
                    }
                }
                while (dt.Columns.Contains(name)) name = name + "_1";//重复行名称会报错。
                dt.Columns.Add(new DataColumn(name, typeof(string)));
                //Console.WriteLine("c->" + name);
            }
            //生成行数据
            // ReSharper disable once SuggestVarOrType_BuiltInTypes
            var rowIdx = hasTitle ? 2 : 1;
            for (var iRow = rowIdx; iRow <= iRowCount; iRow++)
            {
                var dr = dt.NewRow();
                for (var iCol = 1; iCol <= iColCount; iCol++)
                {
                    var cell = sheet.Range[iRow, iCol];
                    dr[iCol - 1] = sheet.Range[iRow, iCol].FormulaValue;
                    if (cell.HasFormula)
                    {
                        //double value = cell.FormulaNumberValue;
                        //cell.Clear(ExcelClearOptions.ClearAll);
                        //cell.NumberValue = value;
                        dr[iCol - 1] = sheet.Range[iRow, iCol].FormulaValue;
                        //Console.WriteLine("r0->" + sheet.Range[iRow, iCol].FormulaValue);
                    }
                    else
                    {
                        dr[iCol - 1] = sheet.Range[iRow, iCol].Value;
                        //Console.WriteLine("r1->" + sheet.Range[iRow, iCol].Value);
                    }
                }
                dt.Rows.Add(dr);
                //Console.WriteLine("code->" + dr[5].ToString());
                if (dr[5].ToString() == "100")
                    break;
            }
            return dt;
        }
        #endregion

        #region 导出
        /// <summary>
        /// 读取数据库 并导入到Excel
        /// </summary>
        /// <param name="excelFilePath">导出excel的文件路径</param>
        /// <param name="dt">从数据库中获取</param>
        /// <param name="fileName">文件名</param>
        /// <param name="sheetIndex">索引值</param>
        public static void ReportToExcel(DataTable dt, string excelFilePath, string fileName = "", int sheetIndex = 0)
        {
            Workbook workbook = new Workbook();
            Worksheet sheet = workbook.Worksheets[sheetIndex];
            sheet.InsertDataTable(dt, true, 1, 1);
            SaveXls(workbook, excelFilePath, fileName);
        }

        /// <summary>
        /// 保存Excel文件
        /// </summary>
        /// <param name="workbook">workBook</param>
        /// <param name="excelFilePath">导出excel的文件路径</param>
        /// <param name="fileName">文件名称</param>
        public static void SaveXls(Workbook workbook, string excelFilePath, string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                workbook.SaveToFile(excelFilePath + fileName + ".xlsx", ExcelVersion.Version2010);
            }
            else
            {
                workbook.SaveToFile(excelFilePath + DateTime.Now.ToString("yyyyMMddhhmmssfff") + ".xlsx", ExcelVersion.Version2010);
            }
        }
        #endregion

        /// <summary>
        /// excel转pdf
        /// </summary>
        /// <param name="filePath">excel路径</param>
        /// <param name="savePath">pdf保存路径</param>
        public static void Excel2Pdf(string filePath, string savePath)
        {
            Aspose.Cells.Workbook excel = new Aspose.Cells.Workbook(filePath);
            excel.Save(savePath, Aspose.Cells.SaveFormat.Pdf);
        }
    }
}
