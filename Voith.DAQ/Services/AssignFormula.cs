using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Voith.DAQ.Common;
using Voith.DAQ.DB;
using Voith.DAQ.Model;
using Voith.DAQ.UI;

namespace Voith.DAQ.Services
{
    /// <summary>
    /// 下发配方（如果工位类型一样，PLC将不会重新请求下发配方）
    /// </summary>
    class AssignFormula
    {
        /// <summary>
        /// 数据库访问对象
        /// </summary>
        private readonly DbContext _db;

        /// <summary>
        /// 当前工站在位的工件信息
        /// </summary>
        private Workpiece _workpiece;

        public AssignFormula(Workpiece workpiece)
        {
            _db = new DbContext();
            _workpiece = workpiece;

            Handle();
        }

        /// <summary>
        /// 下发配方线程
        /// </summary>
        public void Handle()
        {
            new Thread(() =>
            {
                while (true)
                {
                    /*所有工位数据都在一个DB块，每个工位数据占用500个字节，
                     根据工位位置确定读取的数据起始位置*/
                    try
                    {
                        var startAddress = _workpiece.StartAddr + 240;//配方地址

                        var signals = PlcHelper.Read<short>(SystemConfig.ControlDB, startAddress, 2);
                        if (signals[1] == 1)
                        {
                            var formulaNo = signals[0].ToString("D3");
                            var formulas = _db.FormulaDb
                                .GetList(it => it.FormulaNum == formulaNo && it.StationName == _workpiece.StationCode)
                                .OrderBy(it => it.WorkStep).ToList();

                            byte[] workContent = new byte[500 + formulas.Count * 300];

                            int i = 0;
                            foreach (var formula in formulas)
                            {
                                if (formula.OperationTypeId >= 100)
                                    break;

                                int index = 500 + i * 300;
                                i++;
                                BitConverter.GetBytes(formula.OperationTypeId).Reverse().ToArray().CopyTo(workContent, index);
                                BitConverter.GetBytes(formula.Parameter1).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter2).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter3).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter4).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter5).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter6).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter7).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter8).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter9).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter10).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter11).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter12).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter13).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter14).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter15).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter16).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter17).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter18).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter19).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter20).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter21).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter22).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter23).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter24).Reverse().ToArray().CopyTo(workContent, index += 2);
                                BitConverter.GetBytes(formula.Parameter25).Reverse().ToArray().CopyTo(workContent, index += 2);
                                Encoding.Default.GetBytes(formula.FeatureCode1).Reverse().ToArray().CopyTo(workContent, index += 20);
                                Encoding.Default.GetBytes(formula.FeatureCode2).Reverse().ToArray().CopyTo(workContent, index += 20);
                                Encoding.Default.GetBytes(formula.FeatureCode3).Reverse().ToArray().CopyTo(workContent, index += 20);
                                Encoding.Default.GetBytes(formula.FeatureCode4).Reverse().ToArray().CopyTo(workContent, index += 20);
                                Encoding.Default.GetBytes(formula.FeatureCode5).Reverse().ToArray().CopyTo(workContent, index += 20);
                                Encoding.Default.GetBytes(formula.FeatureCode6).Reverse().ToArray().CopyTo(workContent, index += 20);
                                Encoding.Default.GetBytes(formula.FeatureCode7).Reverse().ToArray().CopyTo(workContent, index += 20);
                                Encoding.Default.GetBytes(formula.FeatureCode8).Reverse().ToArray().CopyTo(workContent, index += 20);
                            }

                            if (formulas.Count > 0)
                            {
                                PlcHelper.WriteBytes(_workpiece.DBAddr1, 0, workContent);//写入配方数据
                                PlcHelper.Write(_workpiece.DBAddr1, 0, Convert.ToInt16(formulaNo));//写入配方编号
                                PlcHelper.Write(_workpiece.DBAddr1, 2, (short)i);// 写入工艺总步数
                                PlcHelper.Write(1045, startAddress + 2, (short)101);//写入配方下发完成信号
                                LogHelper.Info($"{_workpiece.StationCode}->配方下发 101->{formulas.Count}->{formulaNo}");
                            }
                            else
                            {
                                PlcHelper.Write(1045, startAddress + 2, (short)102);
                                LogHelper.Info($"{_workpiece.StationCode}->配方102->{formulaNo}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        PlcHelper.Write(1045, _workpiece.StartAddr, (short)102);//XRU配方下发失败信号
                        LogHelper.Error(ex, "配方下发线程出错：");
                    }

                    Thread.Sleep(300);
                }
            })
            { IsBackground = true }.Start();
        }

        /// <summary>
        /// 导入配方
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="progressBar">进度条</param>
        /// <param name="importType">导入方式</param>
        public void FormulaImport(string path, ProgressBarHelper progressBar, FormulaImportType importType = FormulaImportType.Cancel)
        {
            if (importType == FormulaImportType.Cancel)
            {
                return;
            }

            try
            {
                var formulaList = new List<Formula>();

                var ds = ExcelHelper.ExcelToDataSetFormPath(true, path);

                foreach (DataTable dt in ds.Tables)
                {
                    Console.WriteLine(dt.TableName + "------------------");
                    if (dt.TableName.Contains("Sheet"))
                    {
                        //跳过空表
                        continue;
                    }
                    foreach (DataRow row in dt.Rows)
                    {
                        if (!row.Table.Columns.Contains("配方号") || !row.Table.Columns.Contains("工步") ||
                            string.IsNullOrWhiteSpace(row["配方号"]?.ToString()) ||
                            string.IsNullOrWhiteSpace(row["工步"]?.ToString()))
                        {
                            //跳过空行
                            continue;
                        }

                        Formula formula = new Formula();
                        foreach (DataColumn column in dt.Columns)
                        {
                            Console.WriteLine(column.ColumnName);
                            switch (column.ColumnName.Trim())
                            {
                                case "配方号":
                                    formula.FormulaNum = row[column.ColumnName]?.ToString();
                                    break;
                                case "工步":
                                    formula.WorkStep = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "操作类型":
                                    formula.OperationTypeContent = row[column.ColumnName]?.ToString();
                                    break;
                                case "工作描述(中文)":
                                    formula.ActionDescription = row[column.ColumnName]?.ToString();
                                    break;
                                case "代码":
                                    formula.OperationTypeId = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数1":
                                    formula.Parameter1 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数2":
                                    formula.Parameter2 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数3":
                                    formula.Parameter3 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数4":
                                    formula.Parameter4 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数5":
                                    formula.Parameter5 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数6":
                                    formula.Parameter6 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数7":
                                    formula.Parameter7 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数8":
                                    formula.Parameter8 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数9":
                                    formula.Parameter9 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数10":
                                    formula.Parameter10 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数11":
                                    formula.Parameter11 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数12":
                                    formula.Parameter12 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数13":
                                    formula.Parameter13 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数14":
                                    formula.Parameter14 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数15":
                                    formula.Parameter15 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数16":
                                    formula.Parameter16 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数17":
                                    formula.Parameter17 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数18":
                                    formula.Parameter18 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数19":
                                    formula.Parameter19 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数20":
                                    formula.Parameter20 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数21":
                                    formula.Parameter21 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数22":
                                    formula.Parameter22 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数23":
                                    formula.Parameter23 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数24":
                                    formula.Parameter24 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "参数25":
                                    formula.Parameter25 = Convert.ToInt16(string.IsNullOrEmpty(row[column.ColumnName].ToString()) ?
                                        "0" : row[column.ColumnName].ToString());
                                    break;
                                case "特征码1":
                                    formula.FeatureCode1 = row[column.ColumnName]?.ToString();
                                    break;
                                case "特征码2":
                                    formula.FeatureCode2 = row[column.ColumnName]?.ToString();
                                    break;
                                case "特征码3":
                                    formula.FeatureCode3 = row[column.ColumnName]?.ToString();
                                    break;
                                case "特征码4":
                                    formula.FeatureCode4 = row[column.ColumnName]?.ToString();
                                    break;
                                case "特征码5":
                                    formula.FeatureCode5 = row[column.ColumnName]?.ToString();
                                    break;
                                case "特征码6":
                                    formula.FeatureCode6 = row[column.ColumnName]?.ToString();
                                    break;
                                case "特征码7":
                                    formula.FeatureCode7 = row[column.ColumnName]?.ToString();
                                    break;
                                case "特征码8":
                                    formula.FeatureCode8 = row[column.ColumnName]?.ToString();
                                    break;
                            }
                        }

                        formula.StationName = dt.TableName;
                        formulaList.Add(formula);
                        if (formula.OperationTypeId == 100)
                            break;
                    }
                }


                if (formulaList.Count > 0)
                {
                    var result = _db.Db.Ado.UseTran(() =>
                            {
                                //判断是否为覆盖导入
                                if (importType == FormulaImportType.Cover)
                                {
                                    //删除原有配方
                                    _db.FormulaDb.Delete(a => true);
                                }

                                //批量插入
                                var insertCount = _db.FormulaDb.AsInsertable(formulaList.ToArray()).ExecuteCommand();
                            });
                    if (result.IsSuccess)
                    {
                        _db.Db.Ado.CommitTran();
                        MessageBox.Show(@"配方导入成功！");
                    }
                    else
                    {
                        _db.Db.Ado.RollbackTran();
                        MessageBox.Show(@"导入配方失败！");
                    }
                }
                else
                {
                    MessageBox.Show(@"没有检索到配方内容！");
                }


            }
            catch (System.Exception e)
            {
                LogHelper.Error(e, "导入配方出错：");
                MessageBox.Show(@"导入配方失败！");
            }
            finally
            {
                progressBar.Close();
            }
        }
    }
}
