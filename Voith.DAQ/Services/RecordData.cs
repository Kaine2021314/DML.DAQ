using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Voith.DAQ.Common;
using Voith.DAQ.DB;
using Voith.DAQ.Model;

namespace Voith.DAQ.Services
{
    /// <summary>
    /// 读取所有质量数据
    /// </summary>
    class RecordData
    {
        /// <summary>
        /// 数据库访问对象
        /// </summary>
        private readonly DbContext _db;

        /// <summary>
        /// 当前工站在位的工件信息
        /// </summary>
        private Workpiece _workpiece;
        bool LastOutstationFlag = true;
        public RecordData(Workpiece workpiece)
        {
            _db = new DbContext();
            _workpiece = workpiece;

            Handle();
        }
        //static DateTime tagdate = DateTime.Now;
        //static int tagcount = 0;
        string lastmark = "";
        public void Handle()
        {
            new Thread(() =>
                {
                    /*所有工位数据都在一个DB块，每个工位数据占用1000个字节，
                     根据工位位置确定读取的数据起始位置*/
                    var startAddress = _workpiece.StartAddr;
                    var DStartAddress = _workpiece.DStartAddr;
                    while (true)
                    {
                        Thread.Sleep(300);
                        try
                        {
                            var datas = PlcHelper.Read<short>(SystemConfig.ControlDB, startAddress + 20, 3);
                            if (datas[2] == 1)
                            {
                                if (string.IsNullOrEmpty(_workpiece.SerialNumber))
                                {
                                    LogHelper.Info($"{_workpiece.StationCode}条码为空");
                                    PlcHelper.Write<short>(SystemConfig.DTControlDB, startAddress + 112, 102);
                                    continue;
                                }
                                else
                                {
                                    //获取追溯数据
                                    var DataBytes = PlcHelper.ReadBytes(SystemConfig.DataDB, DStartAddress, 300);

                                    switch (_workpiece.StationIndex)
                                    {
                                        //case 10:
                                        //    break;
                                        //case 20:
                                        //    break;
                                        //case 30:
                                        //    break;
                                        //case 40:
                                        //    break;
                                        //case 50:
                                        //    break;
                                        //case 60:
                                        //    break;
                                        //case 35:
                                        //    break;
                                        //case 80:
                                        //    break;
                                        default:
                                            var workType = datas[1];//读取数据类型
                                            var stepNo = datas[0];

                                            if (stepNo > 0)
                                            {
                                                StandardRecordData(startAddress, DStartAddress, workType, stepNo);
                                            }
                                            else
                                                PlcHelper.Write<short>(SystemConfig.DTControlDB, startAddress + 112, 102);
                                            break;
                                    }
                                }
                            }

                            //Thread.Sleep(300);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex, $"RecordData");
                            Thread.Sleep(3000);
                        }
                    }
                })
            { IsBackground = true }.Start();
        }

        private void StandardRecordData(int startAddress, int DStartAddress, short workType, short stepNo)
        {
            LogHelper.Info($"标准数据->{_workpiece.StationCode}->{_workpiece.SerialNumber}->{startAddress}->{DStartAddress}->{workType}->{stepNo}");
            //获取质量数据字节数组
            var bytes = PlcHelper.ReadBytes(SystemConfig.DataDB, DStartAddress + (workType - 1) * 50, 50);

            int DataAddr = 0;
            var recordResult = false;

            switch (workType)
            {
                #region 关键条码
                case 1:
                    recordResult = KeyCode(bytes, stepNo, ref DataAddr);//-
                    break;
                #endregion
                #region 拧紧数据
                case 2:
                    recordResult = Tighten(bytes, stepNo, ref DataAddr);//-
                    break;
                #endregion
                #region 压装数据
                case 3:
                    recordResult = Press(bytes, stepNo, ref DataAddr);//-
                    break;
                #endregion
                #region 相机检测数据
                case 4:
                    recordResult = VisionInspection(bytes, stepNo, ref DataAddr);//-
                    break;
                #endregion
            }

            PlcHelper.Write<short>(SystemConfig.DTControlDB, startAddress + 112, (short)(recordResult ? 101 : 102));
        }

        private bool StationData(byte[] bytes)
        {
            try
            {
                var WorkingTime = PlcConvert.GetDIntAt(bytes, 0);
                var EKSLevel = PlcConvert.GetIntAt(bytes, 4);
                var EKSCode = Encoding.ASCII.GetString(bytes, 8, 18).Trim('\0').Trim();

                string sql = "INSERT INTO dbo.StationData(SerialNumber,StationCode,EKS,EKSLevel,WorkingTime)" +
                             $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{EKSCode}',{EKSLevel},{WorkingTime});";
                if (!string.IsNullOrEmpty(EKSCode) && !string.IsNullOrEmpty(_workpiece.SerialNumber))
                    _db.Db.Ado.ExecuteCommand(sql);

                //int result;
                //if (stationPressureSet != 0 && stationShiftSet != 0 && stationPressureApex != 0 && stationShiftApex != 0 && pressureResult == 1)
                //{
                //    result = 1;
                //}
                //else
                //{
                //    result = 0;
                //}
                //result = Status == 2 ? 1 : 0;
                //sql = "INSERT INTO dbo.QualityData(SerialNumber,StationCode,DataType,DataTableName,StepNo,DataPrimaryKey,CheckResult)" +
                //        $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{6}','',{stepNo},'',{result})";
                //_db.Db.Ado.ExecuteCommand(sql);

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "工位数据记录出错");
                //MessageBox.Show("注油数据采集出错");
                return false;
            }
        }

        //DML 1
        private bool KeyCode(byte[] bytes, int stepNo, ref int DStartAddress)
        {
            switch (_workpiece.StationIndex)
            {
                case 10:
                    break;
                case 20:
                    break;
                case 30:
                    break;
                case 40:
                    break;
                case 50:
                    break;
                case 60:
                    break;
                case 35:
                    break;
                case 80:
                    break;
            }

            bool b;
            try
            {
                var Status = PlcConvert.GetIntAt(bytes, DStartAddress + 2);
                var temp = new byte[44];//50 -2-2-2
                Array.Copy(bytes, 6, temp, 0, 44);
                var KeyCode = Encoding.ASCII.GetString(temp).Trim('\0');
                string sql = "INSERT INTO dbo.KeyCodeInfo(SerialNumber,StationCode,KeyCode,Status) " +
                          $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{KeyCode}','{Status}')";
                b = _db.Db.Ado.ExecuteCommand(sql) > 0;
                LogHelper.Info($"{_workpiece.StationCode}->{_workpiece.SerialNumber}->条码数据->{KeyCode}->{Status}");

                int result;
                //if (stationPressureSet != 0 && stationShiftSet != 0 && stationPressureApex != 0 && stationShiftApex != 0 && pressureResult == 1)
                //{
                //    result = 1;
                //}
                //else
                //{
                //    result = 0;
                //}
                result = Status == 2 ? 1 : 0;
                sql = "INSERT INTO dbo.QualityData(SerialNumber,StationCode,DataType,DataTableName,StepNo,DataPrimaryKey,CheckResult)" +
                        $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{1}','',{stepNo},'',{result})";
                b &= _db.Db.Ado.ExecuteCommand(sql) > 0;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "条码数据采集出错");
                //MessageBox.Show("注油数据采集出错");
                b = false;
            }
            return b;
        }
        //DML 4
        private bool VisionInspection(byte[] bytes, int stepNo, ref int DStartAddress)
        {
            switch (_workpiece.StationIndex)
            {
                case 10:
                    break;
                case 20:
                    DStartAddress = 200;
                    break;
                case 30:
                    break;
                case 40:
                    break;
                case 50:
                    break;
                case 60:
                    break;
                case 35:
                    DStartAddress = 150;
                    break;
                case 80:
                    break;
            }

            bool b;
            try
            {
                var Status = PlcConvert.GetIntAt(bytes, DStartAddress + 2);
                var QualityCode = PlcConvert.GetIntAt(bytes, DStartAddress + 4);
                var Program = PlcConvert.GetIntAt(bytes, DStartAddress + 6);

                string sql = "INSERT INTO dbo.VisionInspection(SerialNumber,StationCode,StepNo,Status,QualityCode,Program)" +
                             $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}',{stepNo},{Status}," +
                             $"{QualityCode},{Program})";

                b = _db.Db.Ado.ExecuteCommand(sql) > 0;

                int result;
                //if (stationPressureSet != 0 && stationShiftSet != 0 && stationPressureApex != 0 && stationShiftApex != 0 && pressureResult == 1)
                //{
                //    result = 1;
                //}
                //else
                //{
                //    result = 0;
                //}
                result = Status == 2 ? 1 : 0;
                sql = "INSERT INTO dbo.QualityData(SerialNumber,StationCode,DataType,DataTableName,StepNo,DataPrimaryKey,CheckResult)" +
                        $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{4}','',{stepNo},'',{result})";
                b &= _db.Db.Ado.ExecuteCommand(sql) > 0;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "相机检测数据采集出错");
                //MessageBox.Show("注油数据采集出错");
                b = false;
            }

            return b;
        }
        //DML 3
        private bool Press(byte[] bytes, int stepNo, ref int DStartAddress)
        {
            switch (_workpiece.StationIndex)
            {
                case 10:
                    DStartAddress = 150;
                    break;
                case 20:
                    break;
                case 30:
                    break;
                case 40:
                    break;
                case 50:
                    break;
                case 60:
                    break;
                case 35:
                    break;
                case 80:
                    break;
            }

            bool b;
            try
            {
                var Status = PlcConvert.GetIntAt(bytes, DStartAddress + 2);
                var QualityCode = PlcConvert.GetIntAt(bytes, DStartAddress + 4);
                var Program = PlcConvert.GetIntAt(bytes, DStartAddress + 6);
                var PressureSet = PlcConvert.GetRealAt(bytes, DStartAddress + 8);
                var PressureLimitationU = PlcConvert.GetRealAt(bytes, DStartAddress + 12);
                var PressureValue = PlcConvert.GetRealAt(bytes, DStartAddress + 16);
                var PressureLimitationD = PlcConvert.GetRealAt(bytes, DStartAddress + 20);
                var MaxPressure = PlcConvert.GetRealAt(bytes, DStartAddress + 24);
                var DisplacementSet = PlcConvert.GetRealAt(bytes, DStartAddress + 28);
                var DisplacementLimitationU = PlcConvert.GetRealAt(bytes, DStartAddress + 32);
                var DisplacementValue = PlcConvert.GetRealAt(bytes, DStartAddress + 36);
                var DisplacementLimitationD = PlcConvert.GetRealAt(bytes, DStartAddress + 40);
                var MaxDisplacement = PlcConvert.GetRealAt(bytes, DStartAddress + 44);

                string sql = "INSERT INTO dbo.Press(SerialNumber,StationCode,StepNo,Status,QualityCode,Program," +
                    "PressureSet,PressureLimitationU,PressureValue,PressureLimitationD,MaxPressure,DisplacementSet," +
                    "DisplacementLimitationU,DisplacementValue,DisplacementLimitationD,MaxDisplacement)" +
                             $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}',{stepNo},{Status}," +
                             $"{QualityCode},{Program},{PressureSet},{PressureLimitationU},{PressureValue}," +
                             $"{PressureLimitationD},{MaxPressure},{DisplacementSet},{DisplacementLimitationU}" +
                             $",{DisplacementValue},{DisplacementLimitationD},{MaxDisplacement})";

                b = _db.Db.Ado.ExecuteCommand(sql) > 0;

                int result;
                //if (stationPressureSet != 0 && stationShiftSet != 0 && stationPressureApex != 0 && stationShiftApex != 0 && pressureResult == 1)
                //{
                //    result = 1;
                //}
                //else
                //{
                //    result = 0;
                //}
                b = Program != 0 && PressureValue != 0 && DisplacementValue != 0;
                result = Status == 2 && b ? 1 : 0;
                sql = "INSERT INTO dbo.QualityData(SerialNumber,StationCode,DataType,DataTableName,StepNo,DataPrimaryKey,CheckResult)" +
                        $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{3}','',{stepNo},'',{result})";
                b &= _db.Db.Ado.ExecuteCommand(sql) > 0;

                
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "压装数据采集出错");
                //MessageBox.Show("压装数据采集出错");
                b = false;
            }

            //DStartAddress += 50;
            return b;
        }
        //DML 2
        private bool Tighten(byte[] bytes, int stepNo, ref int DStartAddress)
        {
            switch (_workpiece.StationIndex)
            {
                case 10:
                    break;
                case 20:
                    DStartAddress = 150;
                    break;
                case 30:
                    break;
                case 40:
                    DStartAddress = 150;
                    break;
                case 50:
                    break;
                case 60:
                    DStartAddress = 150;
                    break;
                case 35:
                    break;
                case 80:
                    break;
            }

            bool b;
            try
            {
                var Status = PlcConvert.GetIntAt(bytes, DStartAddress + 2);
                var QualityCode = PlcConvert.GetIntAt(bytes, DStartAddress + 4);
                var Program = PlcConvert.GetIntAt(bytes, DStartAddress + 6);
                var Real_TU = PlcConvert.GetRealAt(bytes, DStartAddress + 8);
                var Real_T = PlcConvert.GetRealAt(bytes, DStartAddress + 12);
                var Real_TD = PlcConvert.GetRealAt(bytes, DStartAddress + 16);
                var Real_TT = PlcConvert.GetRealAt(bytes, DStartAddress + 20);
                var Real_AU = PlcConvert.GetRealAt(bytes, DStartAddress + 24);
                var Real_A = PlcConvert.GetRealAt(bytes, DStartAddress + 28);
                var Real_AD = PlcConvert.GetRealAt(bytes, DStartAddress + 32);

                if (Status == 0)
                    return true;

                string sql = "INSERT INTO dbo.Tighten(SerialNumber,StationCode,StepNo,Status,QualityCode,Program," +
                    "Real_TU,Real_T,Real_TD,Real_TT,Real_AU,Real_A,Real_AD)" +
                             $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}',{stepNo},{Status}," +
                             $"{QualityCode},{Program},{Real_TU},{Real_T},{Real_TD},{Real_TT}" +
                             $",{Real_AU},{Real_A},{Real_AD})";

                b = _db.Db.Ado.ExecuteCommand(sql) > 0;

                int result;
                //if (stationPressureSet != 0 && stationShiftSet != 0 && stationPressureApex != 0 && stationShiftApex != 0 && pressureResult == 1)
                //{
                //    result = 1;
                //}
                //else
                //{
                //    result = 0;
                //}
                b = Program != 0 && Real_T != 0;
                result = Status == 2 && b ? 1 : 0;
                sql = "INSERT INTO dbo.QualityData(SerialNumber,StationCode,DataType,DataTableName,StepNo,DataPrimaryKey,CheckResult)" +
                        $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{2}','',{stepNo},'',{result})";
                b &= _db.Db.Ado.ExecuteCommand(sql) > 0;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "拧紧数据采集出错");
                //MessageBox.Show("拧紧数据采集出错");
                b = false;
            }

            //DStartAddress += 50;
            return b;
        }
    }
}
