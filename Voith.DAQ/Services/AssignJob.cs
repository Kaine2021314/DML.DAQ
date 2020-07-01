using SqlSugar;
using System;
using System.Data;
using System.Threading;
using Voith.DAQ.Common;
using Voith.DAQ.DB;
using Voith.DAQ.Model;

namespace Voith.DAQ.Services
{
    /// <summary>
    /// 下发工作内容，根据操作结果判断哪些步骤需要重新操作；若是托盘新工件进站，则下发所有工步操作内容
    /// </summary>
    class AssignJob
    {
        /// <summary>
        /// 数据库访问对象
        /// </summary>
        private readonly DbContext _db;

        /// <summary>
        /// 当前工站在位的工件信息
        /// </summary>
        private Workpiece _workpiece;

        public AssignJob(Workpiece workpiece)
        {
            _db = new DbContext();
            _workpiece = workpiece;

            Handle();
        }

        private void Handle()
        {
            new Thread(() =>
            {


                /*所有工位数据都在一个DB块，每个工位数据占用1000个字节，
                 根据工位位置确定读取的数据起始位置*/
                var startAddress = _workpiece.StartAddr;
                while (true)
                {
                    try
                    {
                        var datas = PlcHelper.Read<short>(SystemConfig.ControlDB, startAddress + 8, 1);
                        if (datas[0] == 1)
                        {
                            short rcode = 102;

                            PlcHelper.WriteBytes(SystemConfig.DTControlDB, startAddress + 4,
                                SystemConfig.GetProductionTypes(_workpiece.MaterielCode));
                            //PlcHelper.Write<string>(SystemConfig.DTControlDB, startAddress + 40 + 2, _workpiece.SerialNumber);
                            LogHelper.Info($"{_workpiece.StationCode}->{_workpiece.MaterielCode}->{_workpiece.Type1}->{_workpiece.Type2}->" +
                                $"{ _workpiece.SerialNumber}");

                            string sql =
                                $"SELECT COUNT(1) FROM dbo.Formula WHERE StationName = '{_workpiece.StationCode}'";//此处未区分配方号
                            var stepCount = Convert.ToInt32(_db.Db.Ado.GetScalar(sql));

                            switch (_workpiece.StationIndex)
                            {
                                case 61000000:
                                    stepCount = 3;
                                    break;
                                default:
                                    break;
                            }

                            sql =
                                $"SELECT * FROM dbo.QualityData WHERE SerialNumber = '{_workpiece.SerialNumber}' AND StationCode = '{_workpiece.StationCode}' ORDER BY StepNo,ID";
                            var dataList = _db.Db.Ado.GetDataTable(sql);

                            byte[] enableBytes = new byte[stepCount];
                            string ebstr = "";
                            for (int i = 1; i <= stepCount; i++)
                            {
                                enableBytes[i - 1] = 1;
                                foreach (DataRow row in dataList.Rows)
                                {
                                    if (row["StepNo"]?.ToString() == i.ToString())
                                    {
                                        enableBytes[i - 1] = (byte)(Convert.ToInt32(row["CheckResult"]) == 1 ? 2 : 1);
                                        ebstr += enableBytes[i - 1].ToString() + ",";
                                        //goto End;
                                    }
                                }

                                //enableBytes[i - 1] = 1;
                            }

                            PlcHelper.WriteBytes(SystemConfig.DTControlDB, startAddress + 6, enableBytes);
                            LogHelper.Info($"{_workpiece.StationCode}->101 使能信号->{ebstr}");
                            rcode = 101;

                            PlcHelper.Write<short>(SystemConfig.DTControlDB, startAddress + 2, rcode);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                        try
                        {
                            PlcHelper.Write<short>(SystemConfig.DTControlDB, startAddress + 2, 102);
                        }
                        catch { }
                    }

                    Thread.Sleep(300);
                }
            })
            { IsBackground = true }.Start();
        }
    }
}
