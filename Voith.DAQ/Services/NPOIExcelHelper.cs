using CCWin.Imaging;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using Voith.DAQ.Common;
using static NPOI.HSSF.Util.HSSFColor;

namespace Voith.DAQ.Services
{
    public class NPOIExcelHelper
    {
        private bool disposed;
        private readonly string fileName; //文件名
        private FileStream fs;
        private IWorkbook workbook;

        public NPOIExcelHelper(string fileName)
        {
            this.fileName = fileName;
            disposed = false;
        }

        /// <summary>
        /// 将DataTable数据导入到excel中
        /// </summary>
        /// <param name="dtDataPress">要导入的数据</param>
        /// <param name="isColumnWritten">DataTable的列名是否要导入</param>
        /// <param name="sheetName">要导入的excel的sheet的名称</param>
        /// <returns>导入数据行数(包含列名那一行)</returns>
        public int DataTableToExcelOrderReportBySN(string goodsOrderInfo,
            DataTable dtDataPress,
            DataTable dtDatatCLXDCLZ,
            DataTable dtDataLeakTest,
            DataTable dtDatadtTighten,
            DataTable dtDatadtTightenRight,
            string sheetName, bool isColumnWritten)
        {
            var i = 0;
            var j = 0;
            var count = 0;
            ISheet sheet = null;

            fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                workbook = new XSSFWorkbook();
            else if (fileName.IndexOf(".xls") > 0) // 2003版本
                workbook = new HSSFWorkbook();

            try
            {
                if (workbook != null)
                {
                    sheet = workbook.CreateSheet(sheetName);
                }
                else
                {
                    return -1;
                }
                #region 标题合并单元格，字体居中加粗，先定义单元格样式

                ICellStyle cellFirstStyle = SetFirstTitleStyle();
                ICellStyle cellStyleTitle = SetTitleStyle();
                ICellStyle cellStyleHeader = SetTableHeaderStyle();
                #endregion
                if (isColumnWritten) //写入DataTable的列名
                {
                    //sheet.SetColumnWidth(3, 13 * 256)
                    for (int k = 0; k < 20; k++)
                    {
                        sheet.SetColumnWidth(k, 10 * 170); //第4列的列宽为13
                    }
                    //设置第一行
                    var row = sheet.CreateRow(0);
                    ICell cell0 = row.CreateCell(0);//创建单元格
                    cell0.SetCellValue("缓速器生产数据追溯报表");//赋值
                    cell0.CellStyle = cellFirstStyle;//设置样式  
                    cell0.CellStyle.SetFont(SetFontStyle(18));
                    row.HeightInPoints = 35;//行高
                    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 20));

                    //设置第二行，合并单元格
                    var row1 = sheet.CreateRow(1);
                    ICell cell1 = row1.CreateCell(0);//创建单元格
                    cell1.SetCellValue(goodsOrderInfo);//赋值
                    cell1.CellStyle = cellStyleTitle;//设置样式
                    var row2 = sheet.CreateRow(2);

                    sheet.AddMergedRegion(new CellRangeAddress(1, 2, 0, 20));//合并单元格（第几行，到第几行，第几列，到第几列）

                    count = 3;
                }
                else
                {
                    count = 0;
                }
                #region 压装工序信息
                if (isColumnWritten)
                {
                    var row2 = sheet.CreateRow(count);
                    ICell cell2 = row2.CreateCell(0);//创建单元格
                    cell2.SetCellValue("压装工序信息");//赋值
                    cell2.CellStyle = cellFirstStyle;//设置样式
                    cell2.CellStyle.SetFont(SetFontStyle(11));
                    sheet.AddMergedRegion(new CellRangeAddress(count, count, 0, dtDataPress.Columns.Count * 2));//合并单元格（第几行，到第几行，第几列，到第几列）

                    count = count + 1;

                    var rowTitle = sheet.CreateRow(count);
                    for (j = 0; j < dtDataPress.Columns.Count + 1; ++j)
                    {
                        if (j == 0)
                        {
                            ICell cell = rowTitle.CreateCell(j);
                            cell.SetCellValue(dtDataPress.Columns[j].ColumnName);
                            cell.CellStyle = cellStyleHeader;
                        }
                        else if (j == 1)
                        {
                            ICell cellcol = rowTitle.CreateCell(j * 2 - 1);
                            cellcol.SetCellValue(dtDataPress.Columns[j].ColumnName);
                            cellcol.CellStyle = cellStyleHeader;

                            //ICell cell = rowTitle.CreateCell((j + 1) * 2 - 1);
                            ICell cell = rowTitle.CreateCell(j * 2);
                            cell.CellStyle = cellStyleHeader;
                            sheet.AddMergedRegion(new CellRangeAddress(count, count, j * 2 - 1, j * 2));
                        }
                        else if (j == 2)
                        {
                            ICell cellcol = rowTitle.CreateCell(j * 2 - 1);
                            cellcol.SetCellValue(dtDataPress.Columns[j].ColumnName);
                            cellcol.CellStyle = cellStyleHeader;

                            //ICell cell = rowTitle.CreateCell((j + 1) * 2 - 1);
                            ICell cell0 = rowTitle.CreateCell(j * 2);
                            cell0.CellStyle = cellStyleHeader;
                            cell0 = rowTitle.CreateCell(j * 2 + 1);
                            cell0.CellStyle = cellStyleHeader;
                            ICell cell = rowTitle.CreateCell(j * 2 + 2);
                            cell.CellStyle = cellStyleHeader;
                            sheet.AddMergedRegion(new CellRangeAddress(count, count, j * 2 - 1, j * 2 + 2));
                            j++;
                        }
                        else
                        {
                            ICell cellcol = rowTitle.CreateCell(j * 2 - 1);
                            cellcol.SetCellValue(dtDataPress.Columns[j - 1].ColumnName);
                            cellcol.CellStyle = cellStyleHeader;

                            //ICell cell = rowTitle.CreateCell((j + 1) * 2 - 1);
                            ICell cell = rowTitle.CreateCell(j * 2);
                            cell.CellStyle = cellStyleHeader;
                            sheet.AddMergedRegion(new CellRangeAddress(count, count, j * 2 - 1, j * 2));//合并单元格（第几行，到第几行，第几列，到第几列）
                        }
                    }
                    count = count + 1;
                }
                for (i = 0; i < dtDataPress.Rows.Count; ++i)
                {
                    var row = sheet.CreateRow(count);
                    row.HeightInPoints = 13;
                    for (j = 0; j < dtDataPress.Columns.Count + 1; ++j)
                    {
                        if (j == 0)
                        {
                            ICell cell = row.CreateCell(j);
                            cell.SetCellValue(dtDataPress.Rows[i][j].ToString());
                            cell.CellStyle = cellStyleTitle;
                        }
                        else if (j == 1)
                        {
                            ICell cellcol = row.CreateCell(j * 2 - 1);
                            cellcol.SetCellValue(dtDataPress.Rows[i][j].ToString());
                            cellcol.CellStyle = cellStyleTitle;

                            ICell cell = row.CreateCell(j * 2);
                            cell.CellStyle = cellStyleTitle;
                            sheet.AddMergedRegion(new CellRangeAddress(count, count, j * 2 - 1, j * 2));
                        }
                        else if (j == 2)
                        {
                            ICell cellcol = row.CreateCell(j * 2 - 1);
                            cellcol.SetCellValue(dtDataPress.Rows[i][j].ToString());
                            cellcol.CellStyle = cellStyleTitle;
                            ICell cell0 = row.CreateCell(j * 2);
                            cell0.CellStyle = cellStyleTitle;
                            cell0 = row.CreateCell(j * 2 + 1);
                            cell0.CellStyle = cellStyleTitle;
                            ICell cell = row.CreateCell(j * 2 + 2);
                            cell.CellStyle = cellStyleTitle;
                            sheet.AddMergedRegion(new CellRangeAddress(count, count, j * 2 - 1, j * 2 + 2));
                            j++;
                        }
                        else
                        {
                            ICell cellcol = row.CreateCell(j * 2 - 1);
                            cellcol.SetCellValue(dtDataPress.Rows[i][j - 1].ToString());
                            cellcol.CellStyle = cellStyleTitle;

                            ICell cell = row.CreateCell(j * 2);
                            cell.CellStyle = cellStyleTitle;
                            sheet.AddMergedRegion(new CellRangeAddress(count, count, j * 2 - 1, j * 2));//合并单元格（第几行，到第几行，第几列，到第几列）
                        }
                    }
                    ++count;
                }
                count = count + 2;
                #endregion

                #region 选垫/气密测试
                int col2 = dtDatatCLXDCLZ.Columns.Count * 2 + 3;

                if (isColumnWritten)
                {
                    var row2 = sheet.CreateRow(count);
                    ICell cell0 = row2.CreateCell(0);//创建单元格
                    cell0.SetCellValue("选垫数据");//赋值
                    cell0.CellStyle = cellFirstStyle;//设置样式
                    cell0.CellStyle.SetFont(SetFontStyle(11));
                    sheet.AddMergedRegion(new CellRangeAddress(count, count, 0, dtDatatCLXDCLZ.Columns.Count * 2));//合并单元格（第几行，到第几行，第几列，到第几列）

                    ICell cell1 = row2.CreateCell(col2);//创建单元格
                    cell1.SetCellValue("气密测试数据");//赋值
                    cell1.CellStyle = cellFirstStyle;//设置样式
                    cell1.CellStyle.SetFont(SetFontStyle(11));
                    sheet.AddMergedRegion(new CellRangeAddress(count, count, col2, col2 + dtDataLeakTest.Columns.Count * 2 - 1));//合并单元格（第几行，到第几行，第几列，到第几列）

                    #region OP15站监测数据 Title
                    count = count + 1;
                    var rowTitle = sheet.CreateRow(count);
                    for (j = 0; j < dtDatatCLXDCLZ.Columns.Count; ++j)
                    {
                        ICell cellcol = rowTitle.CreateCell((j * 2));
                        cellcol.SetCellValue(dtDatatCLXDCLZ.Columns[j].ColumnName);
                        cellcol.CellStyle = cellStyleHeader;

                        ICell cell = rowTitle.CreateCell((j + 1) * 2 - 1);
                        cell.CellStyle = cellStyleHeader;
                        if (j == dtDatatCLXDCLZ.Columns.Count - 1)
                        {
                            cell = rowTitle.CreateCell((j + 1) * 2);
                            cell.CellStyle = cellStyleHeader;
                            sheet.AddMergedRegion(new CellRangeAddress(count, count, j * 2, (j + 1) * 2));
                        }
                        else
                            sheet.AddMergedRegion(new CellRangeAddress(count, count, j * 2, (j + 1) * 2 - 1));//合并单元格（第几行，到第几行，第几列，到第几列）
                    }

                    #endregion
                    #region 气密测试数据Title
                    for (j = 0; j < dtDataLeakTest.Columns.Count; ++j)
                    {
                        ICell cellcol = rowTitle.CreateCell((j * 2 + col2));
                        cellcol.SetCellValue(dtDataLeakTest.Columns[j].ColumnName);
                        cellcol.CellStyle = cellStyleHeader;

                        ICell cell = rowTitle.CreateCell(((j + 1) * 2 - 1) + col2);
                        cell.CellStyle = cellStyleHeader;
                        cell.CellStyle.SetFont(SetFontStyle(11));
                        sheet.AddMergedRegion(new CellRangeAddress(count, count, j * 2 + col2, ((j + 1) * 2 - 1) + col2));//合并单元格（第几行，到第几行，第几列，到第几列）

                    }
                    #endregion 
                    count = count + 1;
                }
                #region 左侧- OP15站监测数据-Data
                int rightcount1 = count;
                for (i = 0; i < dtDatatCLXDCLZ.Rows.Count; ++i)
                {
                    var row = sheet.CreateRow(count);
                    row.HeightInPoints = 13;
                    for (j = 0; j < dtDatatCLXDCLZ.Columns.Count; ++j)
                    {

                        ICell cellcol = row.CreateCell(j * 2);
                        cellcol.SetCellValue(dtDatatCLXDCLZ.Rows[i][j].ToString());
                        cellcol.CellStyle = cellStyleTitle;

                        ICell cell = row.CreateCell((j + 1) * 2 - 1);
                        cell.CellStyle = cellStyleTitle;

                        if (j == dtDatatCLXDCLZ.Columns.Count - 1)
                        {
                            cell = row.CreateCell((j + 1) * 2);
                            cell.CellStyle = cellStyleTitle;
                            sheet.AddMergedRegion(new CellRangeAddress(count, count, j * 2, (j + 1) * 2));
                        }
                        else
                            sheet.AddMergedRegion(new CellRangeAddress(count, count, j * 2, (j + 1) * 2 - 1));//合并单元格（第几行，到第几行，第几列，到第几列）

                    }
                    ++count;
                }
                #endregion
                #region 右侧-气密测试数据-Data
                for (i = 0; i < dtDataLeakTest.Rows.Count; ++i)
                {

                    var row = sheet.GetRow(rightcount1);
                    if (row == null)
                    {
                        row = sheet.CreateRow(rightcount1);
                        ++count;
                    }
                    for (j = 0; j < dtDataLeakTest.Columns.Count; ++j)
                    {
                        ICell cellcol = row.CreateCell(j * 2 + col2);
                        cellcol.SetCellValue(dtDataLeakTest.Rows[i][j].ToString());
                        cellcol.CellStyle = cellStyleTitle;

                        ICell cell = row.CreateCell(((j + 1) * 2 - 1) + col2);
                        cell.SetCellValue(dtDataLeakTest.Rows[i][j].ToString());
                        cell.CellStyle = cellStyleTitle;
                        sheet.AddMergedRegion(new CellRangeAddress(rightcount1, rightcount1, j * 2 + col2, ((j + 1) * 2 - 1) + col2));//合并单元格（第几行，到第几行，第几列，到第几列）

                    }
                    ++rightcount1;
                }
                #endregion

                count = count + 1;
                #endregion


                #region 拧紧

                int col3 = dtDatadtTighten.Columns.Count + 1;
                if (isColumnWritten)
                {
                    var row2 = sheet.CreateRow(count);
                    ICell cell0 = row2.CreateCell(0);//创建单元格
                    cell0.SetCellValue("力矩记录");//赋值
                    cell0.CellStyle = cellFirstStyle;//设置样式
                    cell0.CellStyle.SetFont(SetFontStyle(11));
                    sheet.AddMergedRegion(new CellRangeAddress(count, count, 0, dtDatadtTighten.Columns.Count * 2));//合并单元格（第几行，到第几行，第几列，到第几列）

                    #region 左侧力矩 Title
                    count = count + 1;
                    var rowTitle = sheet.CreateRow(count);
                    for (j = 0; j < dtDatadtTighten.Columns.Count; ++j)
                    {
                        //rowTitle.CreateCell(j).SetCellValue(dtDatadtTighten.Columns[j].ColumnName);
                        ICell cell = rowTitle.CreateCell(j);
                        cell.SetCellValue(dtDatadtTighten.Columns[j].ColumnName);
                        cell.CellStyle = cellStyleHeader;
                    }
                    count = count + 1;
                    #endregion
                    #region 右侧力矩 Title
                    for (j = 0; j < dtDatadtTightenRight.Columns.Count; ++j)
                    {
                        //rowTitle.CreateCell(j + col3).SetCellValue(dtDatadtTightenRight.Columns[j].ColumnName);
                        ICell cell = rowTitle.CreateCell(j + col3);
                        cell.SetCellValue(dtDatadtTightenRight.Columns[j].ColumnName);
                        cell.CellStyle = cellStyleHeader;
                    }
                    #endregion 
                }

                #region 左侧-力矩数据
                int rightcount2 = count;
                for (i = 0; i < dtDatadtTighten.Rows.Count; ++i)
                {

                    var row = sheet.CreateRow(count);
                    row.HeightInPoints = 13;
                    for (j = 0; j < dtDatadtTighten.Columns.Count; ++j)
                    {
                        //row.CreateCell(j).SetCellValue(dtDatadtTighten.Rows[i][j].ToString());
                        ICell cell = row.CreateCell(j);
                        cell.SetCellValue(dtDatadtTighten.Rows[i][j].ToString());
                        cell.CellStyle = cellStyleTitle;
                    }
                    ++count;
                }
                #endregion
                #region 右侧-力矩数据
                for (i = 0; i < dtDatadtTightenRight.Rows.Count; ++i)
                {

                    var row = sheet.GetRow(rightcount2);
                    for (j = 0; j < dtDatadtTightenRight.Columns.Count; ++j)
                    {
                        //row.CreateCell(j + col3).SetCellValue(dtDatadtTightenRight.Rows[i][j].ToString());
                        ICell cell = row.CreateCell(j + col3);
                        cell.SetCellValue(dtDatadtTightenRight.Rows[i][j].ToString());
                        cell.CellStyle = cellStyleTitle;
                    }
                    ++rightcount2;
                }
                #endregion

                #endregion

                workbook.Write(fs); //写入到excel
                return count;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "导出单件订单Excel报表异常！ ");
                return -1;
            }
        }

        /// <summary>
        /// 设置字体
        /// </summary>
        /// <returns></returns>
        private IFont SetFontStyle(int fontsise)
        {
            IFont font = workbook.CreateFont();//声明字体
            //font.FontName = "宋体";//字体
            font.Boldweight = (Int16)FontBoldWeight.Bold;//加粗
            font.FontHeightInPoints = fontsise;//字号
            //font.Color = HSSFColor.Red.Index;//颜色
            //font.Boldweight = 700;//粗体
            //font.IsItalic = true;//斜体
            //font.Underline = FontUnderlineType.Double;//添加双下划线
            return font;
        }

        /// <summary>
        /// 设置首行标题样式
        /// </summary>
        /// <returns></returns>
        private ICellStyle SetFirstTitleStyle()
        {
            ICellStyle cellStyle = workbook.CreateCellStyle();//声明样式
            cellStyle.Alignment = HorizontalAlignment.Center;//水平居中
            //cellStyle.Alignment = HorizontalAlignment.Distributed;//分散对齐会自动换行
            cellStyle.WrapText = true;//自动换行
            cellStyle.VerticalAlignment = VerticalAlignment.Center;//垂直居中

            #region 设置边框
            //cellStyle.BorderBottom = BorderStyle.Thin; //下边框
            //cellStyle.BorderLeft = BorderStyle.Thin;//左边框
            //cellStyle.BorderTop = BorderStyle.Thin;//上边框
            //cellStyle.BorderRight = BorderStyle.Thin;//右边框
            #endregion

            #region 设置背景颜色
            cellStyle.FillPattern = FillPattern.SolidForeground;//添加背景色必须加这句
            //fCellStyle.FillForegroundColor = HSSFColor.Grey50Percent.Index;//设置背景填充色50%的灰色
            cellStyle.FillForegroundColor = HSSFColor.Grey50Percent.Index;//设置背景填充色50%的灰色
            #endregion
            //cellStyle.TopBorderColor = HSSFColor.Black.Index; //边框颜色

            #region 增加斜线头
            //cellStyle.BorderDiagonalLineStyle = BorderStyle.Dashed;
            //cellStyle.BorderDiagonal = BorderDiagonal.Backward;
            //cellStyle.BorderTop = BorderStyle.Thin;
            //cellStyle.BorderDiagonalColor = IndexedColors.Black.Index;
            #endregion

            #region 设置单元格显示格式
            //HSSFDataFormat format = (HSSFDataFormat)workbook.CreateDataFormat();
            //cellStyle.DataFormat = format.GetFormat("yyyy-mm-dd");
            #endregion

            return cellStyle;
        }

        /// <summary>
        /// 设置标题样式
        /// </summary>
        /// <returns></returns>
        private ICellStyle SetTitleStyle()
        {
            ICellStyle cellStyle = workbook.CreateCellStyle();//声明样式
            cellStyle.Alignment = HorizontalAlignment.Center;//水平居中
            //cellStyle.Alignment = HorizontalAlignment.Distributed;//分散对齐会自动换行
            cellStyle.WrapText = true;//自动换行
            cellStyle.VerticalAlignment = VerticalAlignment.Center;//垂直居中
            cellStyle.BorderBottom = BorderStyle.Thin; //下边框
            cellStyle.BorderLeft = BorderStyle.Thin;//左边框
            cellStyle.BorderTop = BorderStyle.Thin;//上边框
            cellStyle.BorderRight = BorderStyle.Thin;//右边框

            //cellStyle.FillPattern = FillPattern.SolidForeground;//添加背景色必须加这句
            ////fCellStyle.FillForegroundColor = HSSFColor.Grey50Percent.Index;//设置背景填充色50%的灰色
            //cellStyle.FillForegroundColor = HSSFColor.Orange.Index;//设置背景填充色50%的灰色

            //cellStyle.TopBorderColor = HSSFColor.Black.Index; //边框颜色
            return cellStyle;
        }

        /// <summary>
        /// 设置标题样式
        /// </summary>
        /// <returns></returns>
        private ICellStyle SetTableHeaderStyle()
        {
            ICellStyle cellStyle = workbook.CreateCellStyle();//声明样式
            cellStyle.Alignment = HorizontalAlignment.Center;//水平居中
            //cellStyle.Alignment = HorizontalAlignment.Distributed;//分散对齐会自动换行
            cellStyle.WrapText = true;//自动换行
            cellStyle.VerticalAlignment = VerticalAlignment.Center;//垂直居中
            cellStyle.BorderBottom = BorderStyle.Thin; //下边框
            cellStyle.BorderLeft = BorderStyle.Thin;//左边框
            cellStyle.BorderTop = BorderStyle.Thin;//上边框
            cellStyle.BorderRight = BorderStyle.Thin;//右边框

            cellStyle.FillPattern = FillPattern.SolidForeground;//添加背景色必须加这句
            ////fCellStyle.FillForegroundColor = HSSFColor.Grey50Percent.Index;//设置背景填充色50%的灰色
            cellStyle.FillForegroundColor = HSSFColor.LightYellow.Index;//设置背景填充色50%的灰色
            //HSSFColor.Gold.Index//金色
            //cellStyle.TopBorderColor = HSSFColor.Black.Index; //边框颜色
            return cellStyle;
        }

        /// <summary>
        /// 将DataTable数据导入到excel中
        /// </summary>
        /// <param name="fileTemplatePath">Excel模板数据</param>
        /// <param name="data">要导入的数据</param>
        /// <param name="isColumnWritten">DataTable的列名是否要导入</param>
        /// <param name="sheetName">要导入的excel的sheet的名称</param>
        /// <returns>导入数据行数(包含列名那一行)</returns>
        public int DataTableToExcelAllOrdersList(string fileTemplatePath, DataTable data, string sheetName, bool isColumnWritten)
        {
            var i = 0;
            var j = 0;
            var count = 0;
            ISheet sheet = null;

            try
            {
                using (FileStream fsTemplate = new FileStream(fileTemplatePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (fileTemplatePath.IndexOf(".xlsx") > 0) // 2007版本
                    {
                        workbook = new XSSFWorkbook(fsTemplate);
                    }
                    else if (fileTemplatePath.IndexOf(".xls") > 0) // 2003版本
                    {
                        workbook = new HSSFWorkbook(fsTemplate);
                    }
                    fsTemplate.Close();
                }
                if (workbook != null)
                {
                    sheet = workbook.GetSheet(sheetName);
                }
                else
                {
                    return -1;
                }

                if (isColumnWritten) //写入DataTable的列名
                {
                    #region 先定义单元格样式
                    //ICellStyle cellStyle = workbook.CreateCellStyle();//声明样式
                    //cellStyle.Alignment = HorizontalAlignment.Center;//水平居中
                    //cellStyle.WrapText = true;
                    //cellStyle.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                    //cellStyle.BorderBottom = BorderStyle.Thin; //下边框
                    //cellStyle.BorderLeft = BorderStyle.Thin;//左边框
                    //cellStyle.BorderTop = BorderStyle.Thin;//上边框
                    //cellStyle.BorderRight = BorderStyle.Thin;//右边框

                    //cellStyle.FillPattern = FillPattern.SolidForeground;//添加背景色必须加这句
                    // //fCellStyle.FillForegroundColor = HSSFColor.Grey50Percent.Index;//设置背景填充色50%的灰色
                    //cellStyle.FillForegroundColor = HSSFColor.Grey50Percent.Index;//设置背景填充色50%的灰色
                    ICellStyle cellStyle = SetFirstTitleStyle();
                    cellStyle.SetFont(SetFontStyle(11));//加入单元格

                    #endregion

                    var row = sheet.CreateRow(0);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Columns[j].ColumnName);
                        row.GetCell(j).CellStyle = cellStyle;
                    }
                    count = 1;
                }
                else
                {
                    count = 2;
                }

                for (i = 0; i < data.Rows.Count; ++i)
                {
                    var row = sheet.CreateRow(count);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        if (data.Columns[j].DataType == typeof(DateTime))
                        {
                            row.CreateCell(j).SetCellValue(data.Rows[i][j].ToString());
                            IDataFormat dataformat = workbook.CreateDataFormat();
                            row.GetCell(j).CellStyle.DataFormat = dataformat.GetFormat("yyyy-MM-dd HH:mm:ss");
                        }
                        else
                        {
                            double f;
                            bool b = double.TryParse(data.Rows[i][j].ToString(), out f);
                            if (b && j != 3 && j != 0)
                            {
                                row.CreateCell(j).SetCellValue(f);
                                IDataFormat dataformat = workbook.CreateDataFormat();
                                row.GetCell(j).CellStyle.DataFormat = dataformat.GetFormat("0.0000");
                            }
                            else
                                row.CreateCell(j).SetCellValue(data.Rows[i][j].ToString());
                        }
                    }
                    ++count;
                }
                //导出Excel报表
                //主意一点的是，ForceFormulaRecalculation是强制要求Excel在打开时重新计算的属性
                sheet.ForceFormulaRecalculation = true;
                using (fs = new FileStream(fileName, FileMode.Create))
                {
                    workbook.Write(fs); //写入到excel
                    fs.Close();
                }

                return count;

            }

            catch (Exception ex)
            {
                LogHelper.Error(ex, "导出所有完成订单报表列表异常！ ");
                return -1;
            }
        }




        /// <summary>
        /// 将DataTable数据导入到excel中
        /// </summary>
        /// <param name="data">要导入的数据</param>
        /// <param name="isColumnWritten">DataTable的列名是否要导入</param>
        /// <param name="sheetName">要导入的excel的sheet的名称</param>
        /// <returns>导入数据行数(包含列名那一行)</returns>
        public int DataTableToExcelSelf(DataTable dtHeader, DataTable data, string sheetName, bool isColumnWritten)
        {
            var i = 0;
            var j = 0;
            var count = 0;
            ISheet sheet = null;

            fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                workbook = new XSSFWorkbook();
            else if (fileName.IndexOf(".xls") > 0) // 2003版本
                workbook = new HSSFWorkbook();

            try
            {
                if (workbook != null)
                {
                    sheet = workbook.CreateSheet(sheetName);
                }
                else
                {
                    return -1;
                }

                if (isColumnWritten) //写入DataTable的列名
                {
                    #region 标题合并单元格，字体居中加粗，先定义单元格样式

                    #region 先定义单元格样式
                    ICellStyle cellStyle = workbook.CreateCellStyle();//声明样式
                    cellStyle.Alignment = HorizontalAlignment.Center;//水平居中
                                                                     //cellStyle.Alignment = HorizontalAlignment.Distributed;//分散对齐会自动换行
                    cellStyle.WrapText = true;
                    cellStyle.VerticalAlignment = VerticalAlignment.Center;//垂直居中

                    IFont font = workbook.CreateFont();//声明字体
                                                       //font.Boldweight = (Int16)FontBoldWeight.Bold;//加粗
                                                       //font.FontHeightInPoints = 18;//字体大小
                    font.FontHeightInPoints = 11;
                    cellStyle.SetFont(font);//加入单元格
                    #endregion


                    #endregion


                    var row = sheet.CreateRow(0);
                    var row1 = sheet.CreateRow(1);
                    row.HeightInPoints = 35;//行高
                    int k = 0;
                    for (j = 0; j < dtHeader.Columns.Count; ++j)
                    {
                        if (j <= 7)
                        {
                            //ICell cell0 = row.CreateCell(j);//创建单元格
                            //cell0.SetCellValue(data.Columns[j].ColumnName);//赋值
                            //cell0.CellStyle = cellStyle;//设置样式
                            row.CreateCell(j).SetCellValue(dtHeader.Columns[j].ColumnName);
                            sheet.AddMergedRegion(new CellRangeAddress(0, 1, j, j));//合并单元格（第几行，到第几行，第几列，到第几列）
                        }
                        else
                        {
                            k++;
                            int startindex = j + ((k - 1) * 4);
                            int endindex = j + k * 4;
                            ICell cell0 = row.CreateCell(j);//创建单元格
                            cell0.SetCellValue(dtHeader.Columns[j].ColumnName);//赋值
                            cell0.CellStyle = cellStyle;//设置样式

                            row1.CreateCell(startindex).SetCellValue("最小值");
                            row1.CreateCell(startindex + 1).SetCellValue("最大值");
                            row1.CreateCell(startindex + 2).SetCellValue("实际值");
                            row1.CreateCell(startindex + 3).SetCellValue("单位");
                            row1.CreateCell(startindex + 4).SetCellValue("状态");
                            sheet.AddMergedRegion(new CellRangeAddress(0, 0, startindex, endindex));//合并单元格（第几行，到第几行，第几列，到第几列）


                        }
                    }

                    //设置第一行，合并单元格
                    //var row1 = sheet.CreateRow(1);
                    //ICell cell1 = row1.CreateCell(0);//创建单元格
                    //cell1.SetCellValue("XXXX年XX月对账单");//赋值
                    //cell1.CellStyle = cellStyle;//设置样式
                    //sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dtBody.Columns.Count - 1));//合并单元格（第几行，到第几行，第几列，到第几列）



                    count = 2;
                }
                else
                {
                    count = 0;
                }

                for (i = 0; i < data.Rows.Count; ++i)
                {
                    var row = sheet.CreateRow(count);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Rows[i][j].ToString());
                    }
                    ++count;
                }
                workbook.Write(fs); //写入到excel
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// 将DataTable数据导入到excel中
        /// </summary>
        /// <param name="data">要导入的数据</param>
        /// <param name="isColumnWritten">DataTable的列名是否要导入</param>
        /// <param name="sheetName">要导入的excel的sheet的名称</param>
        /// <returns>导入数据行数(包含列名那一行)</returns>
        public int DataTableToExcel(DataTable data, string sheetName, bool isColumnWritten)
        {
            var i = 0;
            var j = 0;
            var count = 0;
            ISheet sheet = null;

            fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                workbook = new XSSFWorkbook();
            else if (fileName.IndexOf(".xls") > 0) // 2003版本
                workbook = new HSSFWorkbook();

            try
            {
                if (workbook != null)
                {
                    sheet = workbook.CreateSheet(sheetName);
                }
                else
                {
                    return -1;
                }

                if (isColumnWritten) //写入DataTable的列名
                {
                    #region 先定义单元格样式
                    //ICellStyle cellStyle = workbook.CreateCellStyle();//声明样式
                    //cellStyle.Alignment = HorizontalAlignment.Center;//水平居中
                    //cellStyle.WrapText = true;
                    //cellStyle.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                    //cellStyle.BorderBottom = BorderStyle.Thin; //下边框
                    //cellStyle.BorderLeft = BorderStyle.Thin;//左边框
                    //cellStyle.BorderTop = BorderStyle.Thin;//上边框
                    //cellStyle.BorderRight = BorderStyle.Thin;//右边框

                    //cellStyle.FillPattern = FillPattern.SolidForeground;//添加背景色必须加这句
                    // //fCellStyle.FillForegroundColor = HSSFColor.Grey50Percent.Index;//设置背景填充色50%的灰色
                    //cellStyle.FillForegroundColor = HSSFColor.Grey50Percent.Index;//设置背景填充色50%的灰色
                    ICellStyle cellStyle = SetFirstTitleStyle();
                    cellStyle.SetFont(SetFontStyle(11));//加入单元格

                    #endregion

                    var row = sheet.CreateRow(0);
                    row.HeightInPoints = 45;//行高
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        //cell0.CellStyle = cellStyle;//设置样式

                        row.CreateCell(j).SetCellValue(data.Columns[j].ColumnName);
                        row.GetCell(j).CellStyle = cellStyle;

                    }
                    count = 1;
                }
                else
                {
                    count = 0;
                }

                for (i = 0; i < data.Rows.Count; ++i)
                {
                    var row = sheet.CreateRow(count);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        if (data.Columns[j].DataType == typeof(DateTime))
                        {
                            row.CreateCell(j).SetCellValue(data.Rows[i][j].ToString());
                            IDataFormat dataformat = workbook.CreateDataFormat();
                            row.GetCell(j).CellStyle.DataFormat = dataformat.GetFormat("yyyy-MM-dd HH:mm:ss");
                        }
                        else
                        {
                            row.CreateCell(j).SetCellValue(data.Rows[i][j].ToString());
                        }
                    }
                    ++count;
                }
                workbook.Write(fs); //写入到excel
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return -1;
            }
        }

        /// <summary>
        ///     将excel中的数据导入到DataTable中
        /// </summary>
        /// <param name="sheetName">excel工作薄sheet的名称</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <returns>返回的DataTable</returns>
        public DataTable ExcelToDataTable(string sheetName, bool isFirstRowColumn)
        {
            ISheet sheet = null;
            var data = new DataTable();
            var startRow = 0;
            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                    workbook = new XSSFWorkbook(fs);
                else if (fileName.IndexOf(".xls") > 0) // 2003版本
                    workbook = new HSSFWorkbook(fs);

                if (sheetName != null)
                {
                    sheet = workbook.GetSheet(sheetName);
                    if (sheet == null) //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                    {
                        sheet = workbook.GetSheetAt(0);
                    }
                }
                else
                {
                    sheet = workbook.GetSheetAt(0);
                }
                if (sheet != null)
                {
                    var firstRow = sheet.GetRow(0);
                    int cellCount = firstRow.LastCellNum; //一行最后一个cell的编号 即总的列数

                    if (isFirstRowColumn)
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            var cell = firstRow.GetCell(i);
                            if (cell != null)
                            {
                                cell.SetCellType(CellType.String);
                                var cellValue = cell.StringCellValue;
                                if (!string.IsNullOrEmpty(cellValue))
                                {
                                    var column = new DataColumn(cellValue);
                                    data.Columns.Add(column);
                                }
                            }
                        }
                        startRow = sheet.FirstRowNum + 1;
                    }
                    else
                    {
                        //生成列
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            var column = new DataColumn("列" + i);
                            data.Columns.Add(column);
                        }
                        startRow = sheet.FirstRowNum;
                    }

                    //最后一列的标号
                    var rowCount = sheet.LastRowNum;
                    for (var i = startRow; i <= rowCount; ++i)
                    {
                        var row = sheet.GetRow(i);
                        if (row == null) continue; //没有数据的行默认是null　　　　　　　

                        var dataRow = data.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                        {
                            if (row.GetCell(j) != null) //同理，没有数据的单元格都默认是null
                                dataRow[j] = row.GetCell(j).ToString();
                        }
                        data.Rows.Add(dataRow);
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (fs != null)
                        fs.Close();
                }

                fs = null;
                disposed = true;
            }
        }
    }
}
