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
                        try
                        {
                            var datas = PlcHelper.Read<short>(SystemConfig.ControlDB, startAddress + 350, 3);
                            if (datas[2] == 1)
                            {
                                if (_workpiece.SerialNumber == "CALIBRATING")
                                    LogHelper.Info($"{_workpiece.StationCode}正常数据使用标定条码");
                                switch (_workpiece.StationIndex)
                                {
                                    case 25:
                                        OtherRecordData(startAddress, DStartAddress, 1, 1);
                                        break;
                                    case 45:
                                    case 46:
                                        OtherRecordDataQ(startAddress, DStartAddress, 5, 26);
                                        break;
                                    case 70:
                                        OtherRecordData(startAddress, DStartAddress, 6, 1);
                                        break;
                                    case 51:
                                        OtherRecordData(startAddress, DStartAddress, 5, 1);
                                        break;
                                    case 61:
                                        OtherRecordData(startAddress, DStartAddress, 3, 3);//
                                        break;
                                    case 81:
                                    case 82:
                                        OtherRecordDataQ(startAddress, DStartAddress, 5, 25);
                                        break;
                                    default:
                                        var workType = datas[1];//读取数据类型
                                        var stepNo = datas[0];

                                        if (stepNo > 0)
                                        {
                                            StandardRecordData(startAddress, DStartAddress, workType, stepNo);
                                        }
                                        else
                                            PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 354, 102);
                                        break;
                                }
                            }

                            datas = PlcHelper.Read<short>(SystemConfig.ControlDB, startAddress + 364, 3);
                            if (datas[0] == 1)
                            {
                                //_workpiece.SerialNumber = "CALIBRATING";
                                switch (_workpiece.StationIndex)
                                {
                                    case 25:
                                        OtherRecordDataCB(startAddress, DStartAddress, 1, 1);
                                        break;
                                    case 21:
                                        var datas0 = PlcHelper.ReadBytes(SystemConfig.DataDB, 1750, 44);
                                        var recordResult0 = OP20ThreePointDataCB(datas0);
                                        PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 364, (short)(recordResult0 ? 101 : 102));
                                        break;
                                    case 30:
                                        var datas1 = PlcHelper.ReadBytes(SystemConfig.DataDB, 3780, 32);
                                        var recordResult1 = OP30DPTestCB(datas1);
                                        PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 364, (short)(recordResult1 ? 101 : 102));
                                        break;
                                    case 70:
                                        OtherRecordDataCB(startAddress, DStartAddress, 6, 1);
                                        break;
                                }
                            }

                            //var outstation = PlcHelper.ReadBytes(SystemConfig.ControlDB, startAddress, 1);
                            //bool outstationFlag = PlcConvert.GetBitAt(outstation, 0, 2);
                            //if (LastOutstationFlag != outstationFlag && outstationFlag)
                            //{
                            //    var stationdata = PlcHelper.ReadBytes(SystemConfig.ControlDB, startAddress + 4, 26);
                            //    if(!string.IsNullOrEmpty(_workpiece.SerialNumber))
                            //        StationData(stationdata);
                            //    LogHelper.Info($"{_workpiece.StationCode}工位信息记录");
                            //    //_workpiece.SerialNumber = "";//清条码
                            //}
                            //LastOutstationFlag = outstationFlag;

                            //OP061标签打印写入文件
                            if (_workpiece.StationIndex == 61)
                            {
                                var MarkTag = PlcHelper.Read<short>(SystemConfig.ControlDB, startAddress + 370, 3);
                                if (MarkTag[0] == 1)
                                {
                                    short mtr = 102;
                                    string mark = "";
                                    try
                                    {
                                        StreamWriter w = new StreamWriter("60Mark.txt");

                                        DateTime d = DateTime.Now;
                                        GregorianCalendar gc = new GregorianCalendar();
                                        int WeekOfYear = gc.GetWeekOfYear(d, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

                                        string scode = SystemConfig.GetItem(SystemConfig.DFTag, _workpiece.MaterielCode);
                                        if (!string.IsNullOrEmpty(scode))
                                        {
                                            if (SystemConfig.tagdate.Date != DateTime.Now.Date)
                                            {
                                                SystemConfig.tagdate = DateTime.Now;
                                                SystemConfig.tagcount = 1;
                                            }

                                            if (lastmark == _workpiece.SerialNumber)
                                                SystemConfig.tagcount--;

                                            mark = $"{scode} YI93,{d.ToString("yy-MM-dd")} {d.ToString("yyMMdd")}{SystemConfig.tagcount.ToString("000")}," +
                                            $"{_workpiece.SerialNumber},VR115CT 827112";

                                            SystemConfig.tagcount++;

                                            StreamWriter r = new StreamWriter("MarkStatus");
                                            r.WriteLine(SystemConfig.tagdate.ToString("yyyy-MM-dd") + "," + SystemConfig.tagcount);
                                            r.Close();
                                        }
                                        else if (_workpiece.MaterielCode != "")
                                        {
                                            if(_workpiece.MaterielCode != "153008271120CN" &&
                                            _workpiece.MaterielCode != "153007735220CN" &&
                                            _workpiece.MaterielCode != "153007736240CN" &&
                                            _workpiece.MaterielCode != "153008633110CN")
                                            {
                                                mark = $"{_workpiece.SerialNumber},153005346320CN,{d.ToString("yy")}W{WeekOfYear},VR115CT";
                                            }
                                            else
                                                mark = $"{_workpiece.SerialNumber},{_workpiece.MaterielCode},{d.ToString("yy")}W{WeekOfYear},VR115CT";
                                        }

                                        //if (_workpiece.StationIndex == 61)
                                        //{
                                        //    string sql = "INSERT INTO dbo.KeyCodeInfo(SerialNumber,StationCode,KeyCode) " +
                                        //          $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{mark.Replace(","," ")}')";
                                        //    _db.Db.Ado.ExecuteCommand(sql);
                                        //    LogHelper.Info($"61条码数据->{mark}");
                                        //}

                                        lastmark = _workpiece.SerialNumber;
                                        w.WriteLine(mark);
                                        w.Close();
                                        mtr = 101;
                                        LogHelper.Info($"{_workpiece.StationCode} 标签打印写入->{mark}");
                                    }
                                    catch 
                                    {
                                        LogHelper.Info($"{_workpiece.StationCode} 标签打印写入异常->{mark}");
                                    }
                                    PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 370, mtr);
                                }
                            }

                            //外围数据记录
                            if (_workpiece.StationCode == "OP010")
                            {
                                var datas0 = PlcHelper.ReadBytes(SystemConfig.DataDB, 1250, 8);
                                if (datas0[1] == 1)
                                {
                                    var recordResult = OP10GluingData(datas0);
                                    PlcHelper.Write<short>(SystemConfig.DataDB, 1250, (short)(recordResult ? 101 : 102));
                                }

                                datas0 = PlcHelper.ReadBytes(SystemConfig.DataDB, 1270, 28);
                                if (datas0[1] == 1)
                                {
                                    var recordResult = OP10PressXData(datas0);
                                    PlcHelper.Write<short>(SystemConfig.DataDB, 1270, (short)(recordResult ? 101 : 102));
                                }
                            }
                            else if (_workpiece.StationCode == "OP021")
                            {
                                var datas0 = PlcHelper.ReadBytes(SystemConfig.DataDB, 1750, 44);
                                if (datas0[1] == 1)
                                {
                                    var recordResult = OP20ThreePointData(datas0);
                                    PlcHelper.Write<short>(SystemConfig.DataDB, 1750, (short)(recordResult ? 101 : 102));
                                }
                            }
                            else if (_workpiece.StationCode == "OP030")
                            {
                                var datas0 = PlcHelper.ReadBytes(SystemConfig.DataDB, 3750, 16);
                                if (datas0[1] == 1)
                                {
                                    var recordResult = OP30TorqueTest(datas0);
                                    PlcHelper.Write<short>(SystemConfig.DataDB, 3750, (short)(recordResult ? 101 : 102));
                                }

                                datas0 = PlcHelper.ReadBytes(SystemConfig.DataDB, 3780, 32);
                                if (datas0[1] == 1)
                                {
                                    var recordResult = OP30DPTest(datas0);
                                    PlcHelper.Write<short>(SystemConfig.DataDB, 3780, (short)(recordResult ? 101 : 102));
                                }
                            }

                            Thread.Sleep(300);
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

            var recordResult = false;
            switch (workType)
            {
                #region 测量选垫测量值
                case 1:
                    recordResult = Clxdclz(bytes, stepNo);//-
                    break;
                #endregion
                #region 测量选垫值
                case 2:
                    recordResult = Clxdz(bytes, stepNo);//-
                    break;
                #endregion
                #region 气密性测试值
                case 3:
                    recordResult = LeakTest(bytes, stepNo);//-
                    break;
                #endregion
                #region 压装数据
                case 4:
                    recordResult = Press(bytes, stepNo);//-
                    break;
                #endregion
                #region 拧紧数据
                case 5:
                    recordResult = Tighten(bytes, stepNo);//-
                    break;
                #endregion
                #region 注油数据
                case 6:
                    recordResult = Oiling(bytes, stepNo);//-
                    break;
                #endregion
                #region OP021 增加动定轮条码读取
                case 7:
                    //1794 1844
                    recordResult = OP021KeyCode(bytes, stepNo, startAddress);//-
                    break;
                #endregion
                #region 相机检测数据
                case 10:
                    recordResult = VisionInspection(bytes, stepNo);//-
                    break;
                #endregion
            }

            PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 354, (short)(recordResult ? 101 : 102));
        }

        private void OtherRecordData(int startAddress, int DStartAddress, short workType, short count)
        {
            //short stepNo = 0;
            var recordResult = true;
            LogHelper.Info($"其他数据->{_workpiece.StationCode}->{startAddress}->{DStartAddress}->{workType}->{count}");
            for (int i = 1; i <= count; i++)
            {
                //获取质量数据字节数组
                var bytes = PlcHelper.ReadBytes(SystemConfig.DataDB, DStartAddress + (i-1) * 50, 50);

                switch (workType)
                {
                    #region 测量选垫测量值
                    case 1:
                        recordResult &= Clxdclz(bytes, i);//-
                        break;
                    #endregion
                    #region 测量选垫值
                    case 2:
                        recordResult &= Clxdz(bytes, i);//-
                        break;
                    #endregion
                    #region 气密性测试值
                    case 3:
                        recordResult &= LeakTest(bytes, i);//-
                        break;
                    #endregion
                    #region 压装数据
                    case 4:
                        recordResult &= Press(bytes, i);//-
                        break;
                    #endregion
                    #region 拧紧数据
                    case 5:
                        recordResult &= Tighten(bytes, i);//-
                        break;
                    #endregion
                    #region 注油数据
                    case 6:
                        recordResult &= Oiling(bytes, i);//-
                        break;
                    #endregion
                    default:
                        recordResult = false;
                        break;
                }
            }

            if (_workpiece.StationIndex == 61)
            {
                var bytes = PlcHelper.ReadBytes(SystemConfig.DataDB, DStartAddress + 150, 60);
                string KeyCode = System.Text.ASCIIEncoding.ASCII.GetString(bytes).Trim('\0');
                string sql = "INSERT INTO dbo.KeyCodeInfo(SerialNumber,StationCode,KeyCode) " +
                      $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{KeyCode}')";
                _db.Db.Ado.ExecuteCommand(sql);
                LogHelper.Info($"61条码数据->{KeyCode}");
            }

            PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 354, (short)(recordResult ? 101 : 102));
        }

        private void OtherRecordDataCB(int startAddress, int DStartAddress, short workType, short count)
        {
            //short stepNo = 0;
            var recordResult = true;
            LogHelper.Info($"其他数据CB->{_workpiece.StationCode}->{startAddress}->{DStartAddress}->{workType}->{count}");
            for (int i = 1; i <= count; i++)
            {
                //获取质量数据字节数组
                var bytes = PlcHelper.ReadBytes(SystemConfig.DataDB, DStartAddress + (i - 1) * 50, 50);

                switch (workType)
                {
                    #region 测量选垫测量值
                    case 1:
                        recordResult &= ClxdclzCB(bytes, i);//-
                        break;
                    #endregion
                    //#region 测量选垫值
                    //case 2:
                    //    recordResult &= Clxdz(bytes, i);//-
                    //    break;
                    //#endregion
                    //#region 气密性测试值
                    //case 3:
                    //    recordResult &= LeakTest(bytes, i);//-
                    //    break;
                    //#endregion
                    //#region 压装数据
                    //case 4:
                    //    recordResult &= Press(bytes, i);//-
                    //    break;
                    //#endregion
                    //#region 拧紧数据
                    //case 5:
                    //    recordResult &= Tighten(bytes, i);//-
                    //    break;
                    //#endregion
                    #region 注油数据
                    case 6:
                        recordResult &= OilingCB(bytes, i);//-
                        break;
                    #endregion
                    default:
                        recordResult = false;
                        break;
                }
            }
            PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 364, (short)(recordResult ? 101 : 102));
        }

        private void OtherRecordDataQ(int startAddress, int DStartAddress, short workType, short count)
        {
            //short stepNo = 0;
            var recordResult = true;
            LogHelper.Info($"其他数据->{_workpiece.StationCode}->{startAddress}->{DStartAddress}->{workType}->{count}");
            string sql = "";
            for (int i = 1; i <= count; i++)
            {
                //获取质量数据字节数组
                var bytes = PlcHelper.ReadBytes(SystemConfig.DataDB, DStartAddress + (i - 1) * 50, 50);

                 sql += TightenQ(bytes, i);
            }
            if(sql != "")
                recordResult &= _db.Db.Ado.ExecuteCommand(sql) > 0;

            if (string.IsNullOrEmpty(_workpiece.SerialNumber))
                recordResult = false;

            PlcHelper.Write<short>(SystemConfig.ControlDB, startAddress + 354, (short)(recordResult ? 101 : 102));
        }

        private bool Clxdclz(byte[] bytes, int stepNo)
        {
            try
            {
                var ExtremeStatus = PlcConvert.GetIntAt(bytes, 2);
                var MeasureStatus = PlcConvert.GetIntAt(bytes, 4);
                var QualityCode = PlcConvert.GetIntAt(bytes, 6);
                var Program = PlcConvert.GetIntAt(bytes, 8);
                var CalibrationRecord = PlcConvert.GetIntAt(bytes, 10);
                var MasterMeasure = PlcConvert.GetRealAt(bytes, 12);
                var MeasureLimitU = PlcConvert.GetRealAt(bytes, 16);
                var MeasureLimitD = PlcConvert.GetRealAt(bytes, 20);
                var MeasureValue = PlcConvert.GetRealAt(bytes, 24);
                var ThreePointRange = PlcConvert.GetRealAt(bytes, 28);
                var ExtremumValue = PlcConvert.GetRealAt(bytes, 32);
                var V1 = PlcConvert.GetRealAt(bytes, 36);
                var V2 = PlcConvert.GetRealAt(bytes, 40);
                var V3 = PlcConvert.GetRealAt(bytes, 44);

                string sql = "INSERT INTO dbo.CLXDCLZ(SerialNumber,StationCode,StepNo,ExtremeStatus,MeasureStatus,QualityCode," +
                    "Program,CalibrationRecord,MasterMeasure,MeasureLimitU,MeasureLimitD,MeasureValue,ThreePointRange,ExtremumValue,V1,V2,V3)" +
                             $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}',{stepNo},{ExtremeStatus}," +
                             $"{MeasureStatus},{QualityCode},{Program},{CalibrationRecord},{MasterMeasure}," +
                             $"{MeasureLimitU},{MeasureLimitD},{MeasureValue},{ThreePointRange}" +
                             $",{ExtremumValue},{V1},{V2},{V3})";

                _db.Db.Ado.ExecuteCommand(sql);

                int result;
                //if (stationPressureSet != 0 && stationShiftSet != 0 && stationPressureApex != 0 && stationShiftApex != 0 && pressureResult == 1)
                //{
                //    result = 1;
                //}
                //else
                //{
                //    result = 0;
                //}
                result = MeasureStatus == 2 ? 1 : 0;
                sql = "INSERT INTO dbo.QualityData(SerialNumber,StationCode,DataType,DataTableName,StepNo,DataPrimaryKey,CheckResult)" +
                        $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{1}','',{stepNo},'',{result})";
                _db.Db.Ado.ExecuteCommand(sql);

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "测量选垫测量值数据采集出错");
                //MessageBox.Show("测量选垫测量值数据采集出错");
                return false;
            }
        }

        private bool ClxdclzCB(byte[] bytes, int stepNo)
        {
            try
            {
                var ExtremeStatus = PlcConvert.GetIntAt(bytes, 2);
                var MeasureStatus = PlcConvert.GetIntAt(bytes, 4);
                var QualityCode = PlcConvert.GetIntAt(bytes, 6);
                var Program = PlcConvert.GetIntAt(bytes, 8);
                var CalibrationRecord = PlcConvert.GetIntAt(bytes, 10);
                var MasterMeasure = PlcConvert.GetRealAt(bytes, 12);
                var MeasureLimitU = PlcConvert.GetRealAt(bytes, 16);
                var MeasureLimitD = PlcConvert.GetRealAt(bytes, 20);
                var MeasureValue = PlcConvert.GetRealAt(bytes, 24);
                var ThreePointRange = PlcConvert.GetRealAt(bytes, 28);
                var ExtremumValue = PlcConvert.GetRealAt(bytes, 32);
                var V1 = PlcConvert.GetRealAt(bytes, 36);
                var V2 = PlcConvert.GetRealAt(bytes, 40);
                var V3 = PlcConvert.GetRealAt(bytes, 44);

                string sql = "INSERT INTO dbo.CLXDCLZ(SerialNumber,StationCode,StepNo,ExtremeStatus,MeasureStatus,QualityCode," +
                    "Program,CalibrationRecord,MasterMeasure,MeasureLimitU,MeasureLimitD,MeasureValue,ThreePointRange,ExtremumValue,V1,V2,V3)" +
                             $"VALUES('CALIBRATING','{_workpiece.StationCode}',{stepNo},{ExtremeStatus}," +
                             $"{MeasureStatus},{QualityCode},{Program},{CalibrationRecord},{MasterMeasure}," +
                             $"{MeasureLimitU},{MeasureLimitD},{MeasureValue},{ThreePointRange}" +
                             $",{ExtremumValue},{V1},{V2},{V3})";

                _db.Db.Ado.ExecuteCommand(sql);

                int result;
                result = MeasureStatus == 2 ? 1 : 0;
                sql = "INSERT INTO dbo.QualityData(SerialNumber,StationCode,DataType,DataTableName,StepNo,DataPrimaryKey,CheckResult)" +
                        $"VALUES('CALIBRATING','{_workpiece.StationCode}','{1}','',{stepNo},'',{result})";
                _db.Db.Ado.ExecuteCommand(sql);

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "测量选垫测量值数据采集出错CB");
                return false;
            }
        }

        private bool Clxdz(byte[] bytes, int stepNo)
        {
            try
            {
                var Status = PlcConvert.GetIntAt(bytes, 2);
                var QualityCode = PlcConvert.GetIntAt(bytes, 4);
                var Program = PlcConvert.GetIntAt(bytes, 6);
                var CheckResult = PlcConvert.GetRealAt(bytes, 8);
                var CalculateResult = PlcConvert.GetRealAt(bytes, 12);
                var GasketNumber = PlcConvert.GetRealAt(bytes, 16);
                var GapRange = PlcConvert.GetRealAt(bytes, 20);
                var GapValue = PlcConvert.GetRealAt(bytes, 24);

                string sql = "INSERT INTO dbo.CLXDZ(SerialNumber,StationCode,StepNo,Status,QualityCode,Program," +
                   "CheckResult,CalculateResult,GasketNumber,GapRange,GapValue)" +
                            $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}',{stepNo},{Status}," +
                            $"{QualityCode},{Program},{CheckResult},{CalculateResult},{GasketNumber},{GapRange}" +
                            $",{GapValue})";

                _db.Db.Ado.ExecuteCommand(sql);

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
                        $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{2}','',{stepNo},'',{result})";
                _db.Db.Ado.ExecuteCommand(sql);

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "测量选垫值数据采集出错");
                //MessageBox.Show("测量选垫值数据采集出错");
                return false;
            }
        }

        private bool LeakTest(byte[] bytes, int stepNo)
        {
            try
            {
                var Status = PlcConvert.GetIntAt(bytes, 2);
                var QualityCode = PlcConvert.GetIntAt(bytes, 4);
                var Program = PlcConvert.GetIntAt(bytes, 6);
                var UDCP = PlcConvert.GetRealAt(bytes, 8);
                var UDCL = PlcConvert.GetRealAt(bytes, 12);
                var MCP = PlcConvert.GetRealAt(bytes, 16);
                var MCL = PlcConvert.GetRealAt(bytes, 20);

                string sql = "INSERT INTO dbo.LeakTest(SerialNumber,StationCode,StepNo,Status,QualityCode,Program," +
                  "UDCP,UDCL,MCP,MCL)" +
                           $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}',{stepNo},{Status}," +
                           $"{QualityCode},{Program},{UDCP},{UDCL}" +
                           $",{MCP},{MCL})";

                _db.Db.Ado.ExecuteCommand(sql);

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
                        $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{3}','',{stepNo},'',{result})";
                _db.Db.Ado.ExecuteCommand(sql);

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "气密测试数据采集出错");
                //MessageBox.Show("气密测试数据采集出错");
                return false;
            }
        }

        private bool Press(byte[] bytes, int stepNo)
        {
            try
            {
                var Status = PlcConvert.GetIntAt(bytes, 2);
                var QualityCode = PlcConvert.GetIntAt(bytes, 4);
                var Program = PlcConvert.GetIntAt(bytes, 6);
                var PressureSet = PlcConvert.GetRealAt(bytes, 8);
                var PressureLimitationU = PlcConvert.GetRealAt(bytes, 12);
                var PressureValue = PlcConvert.GetRealAt(bytes, 16);
                var PressureLimitationD = PlcConvert.GetRealAt(bytes, 20);
                var MaxPressure = PlcConvert.GetRealAt(bytes, 24);
                var DisplacementSet = PlcConvert.GetRealAt(bytes, 28);
                var DisplacementLimitationU = PlcConvert.GetRealAt(bytes, 32);
                var DisplacementValue = PlcConvert.GetRealAt(bytes, 36);
                var DisplacementLimitationD = PlcConvert.GetRealAt(bytes, 40);
                var MaxDisplacement = PlcConvert.GetRealAt(bytes, 44);

                string sql = "INSERT INTO dbo.Press(SerialNumber,StationCode,StepNo,Status,QualityCode,Program," +
                    "PressureSet,PressureLimitationU,PressureValue,PressureLimitationD,MaxPressure,DisplacementSet," +
                    "DisplacementLimitationU,DisplacementValue,DisplacementLimitationD,MaxDisplacement)" +
                             $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}',{stepNo},{Status}," +
                             $"{QualityCode},{Program},{PressureSet},{PressureLimitationU},{PressureValue}," +
                             $"{PressureLimitationD},{MaxPressure},{DisplacementSet},{DisplacementLimitationU}" +
                             $",{DisplacementValue},{DisplacementLimitationD},{MaxDisplacement})";

                _db.Db.Ado.ExecuteCommand(sql);

                int result;
                //if (stationPressureSet != 0 && stationShiftSet != 0 && stationPressureApex != 0 && stationShiftApex != 0 && pressureResult == 1)
                //{
                //    result = 1;
                //}
                //else
                //{
                //    result = 0;
                //}
                bool b = Program != 0 && PressureValue != 0 && DisplacementValue != 0;
                result = Status == 2 && b ? 1 : 0;
                sql = "INSERT INTO dbo.QualityData(SerialNumber,StationCode,DataType,DataTableName,StepNo,DataPrimaryKey,CheckResult)" +
                        $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{4}','',{stepNo},'',{result})";
                b &= _db.Db.Ado.ExecuteCommand(sql) > 0;

                return b;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "压装数据采集出错");
                //MessageBox.Show("压装数据采集出错");
                return false;
            }
        }

        private bool Tighten(byte[] bytes, int stepNo)
        {
            try
            {
                var Status = PlcConvert.GetIntAt(bytes, 2);
                var QualityCode = PlcConvert.GetIntAt(bytes, 4);
                var Program = PlcConvert.GetIntAt(bytes, 6);
                var Real_TU = PlcConvert.GetRealAt(bytes, 8);
                var Real_T = PlcConvert.GetRealAt(bytes, 12);
                var Real_TD = PlcConvert.GetRealAt(bytes, 16);
                var Real_TT = PlcConvert.GetRealAt(bytes, 20);
                var Real_AU = PlcConvert.GetRealAt(bytes, 24);
                var Real_A = PlcConvert.GetRealAt(bytes, 28);
                var Real_AD = PlcConvert.GetRealAt(bytes, 32);

                if(Status == 0)
                    return true;

                string sql = "INSERT INTO dbo.Tighten(SerialNumber,StationCode,StepNo,Status,QualityCode,Program," +
                    "Real_TU,Real_T,Real_TD,Real_TT,Real_AU,Real_A,Real_AD)" +
                             $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}',{stepNo},{Status}," +
                             $"{QualityCode},{Program},{Real_TU},{Real_T},{Real_TD},{Real_TT}" +
                             $",{Real_AU},{Real_A},{Real_AD})";

                _db.Db.Ado.ExecuteCommand(sql);

                int result;
                //if (stationPressureSet != 0 && stationShiftSet != 0 && stationPressureApex != 0 && stationShiftApex != 0 && pressureResult == 1)
                //{
                //    result = 1;
                //}
                //else
                //{
                //    result = 0;
                //}
                bool b = Program != 0 && Real_T != 0;
                result = Status == 2 &&  b ? 1 : 0;
                sql = "INSERT INTO dbo.QualityData(SerialNumber,StationCode,DataType,DataTableName,StepNo,DataPrimaryKey,CheckResult)" +
                        $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{5}','',{stepNo},'',{result})";
                b &= _db.Db.Ado.ExecuteCommand(sql) > 0;

                return b;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "拧紧数据采集出错");
                //MessageBox.Show("拧紧数据采集出错");
                return false;
            }
        }

        private string TightenQ(byte[] bytes, int stepNo)
        {
            try
            {
                var Status = PlcConvert.GetIntAt(bytes, 2);
                var QualityCode = PlcConvert.GetIntAt(bytes, 4);
                var Program = PlcConvert.GetIntAt(bytes, 6);
                var Real_TU = PlcConvert.GetRealAt(bytes, 8);
                var Real_T = PlcConvert.GetRealAt(bytes, 12);
                var Real_TD = PlcConvert.GetRealAt(bytes, 16);
                var Real_TT = PlcConvert.GetRealAt(bytes, 20);
                var Real_AU = PlcConvert.GetRealAt(bytes, 24);
                var Real_A = PlcConvert.GetRealAt(bytes, 28);
                var Real_AD = PlcConvert.GetRealAt(bytes, 32);

                string sql = "INSERT INTO dbo.Tighten(SerialNumber,StationCode,StepNo,Status,QualityCode,Program," +
                    "Real_TU,Real_T,Real_TD,Real_TT,Real_AU,Real_A,Real_AD)" +
                             $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}',{stepNo},{Status}," +
                             $"{QualityCode},{Program},{Real_TU},{Real_T},{Real_TD},{Real_TT}" +
                             $",{Real_AU},{Real_A},{Real_AD});";

                //_db.Db.Ado.ExecuteCommand(sql);

                int result;
                //if (stationPressureSet != 0 && stationShiftSet != 0 && stationPressureApex != 0 && stationShiftApex != 0 && pressureResult == 1)
                //{
                //    result = 1;
                //}
                //else
                //{
                //    result = 0;
                //}
                bool b = Program != 0 && Real_T != 0;
                result = Status == 2 && b ? 1 : 0;
                sql += "INSERT INTO dbo.QualityData(SerialNumber,StationCode,DataType,DataTableName,StepNo,DataPrimaryKey,CheckResult)" +
                        $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{5}','',{stepNo},'',{result});";
                //_db.Db.Ado.ExecuteCommand(sql);

                return Status == 0 ? "" : sql;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "拧紧数据采集出错Q");
                //MessageBox.Show("拧紧数据采集出错Q");
                return "";
            }
        }

        private bool Oiling(byte[] bytes, int stepNo)
        {
            try
            {
                var Status = PlcConvert.GetIntAt(bytes, 2);
                var QualityCode = PlcConvert.GetIntAt(bytes, 4);
                var Program = PlcConvert.GetIntAt(bytes, 6);
                var CalibrationRecord = PlcConvert.GetIntAt(bytes, 8);
                var Temperature = PlcConvert.GetRealAt(bytes, 10);
                var OilLimitationU = PlcConvert.GetRealAt(bytes, 14);
                var OilLimitationD = PlcConvert.GetRealAt(bytes, 18);
                var OilActual = PlcConvert.GetRealAt(bytes, 22);

                string sql = "INSERT INTO dbo.Oiling(SerialNumber,StationCode,StepNo,Status,QualityCode,Program," +
                    "CalibrationRecord,Temperature,OilLimitationU,OilLimitationD,OilActual)" +
                             $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}',{stepNo},{Status}," +
                             $"{QualityCode},{Program},{CalibrationRecord},{Temperature},{OilLimitationU},{OilLimitationD}" +
                             $",{OilActual})";

                _db.Db.Ado.ExecuteCommand(sql);

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
                        $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{6}','',{stepNo},'',{result})";
                _db.Db.Ado.ExecuteCommand(sql);

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "注油数据采集出错");
                //MessageBox.Show("注油数据采集出错");
                return false;
            }
        }
        private bool OilingCB(byte[] bytes, int stepNo)
        {
            try
            {
                var Status = PlcConvert.GetIntAt(bytes, 2);
                var QualityCode = PlcConvert.GetIntAt(bytes, 4);
                var Program = PlcConvert.GetIntAt(bytes, 6);
                var CalibrationRecord = PlcConvert.GetIntAt(bytes, 8);
                var Temperature = PlcConvert.GetRealAt(bytes, 10);
                var OilLimitationU = PlcConvert.GetRealAt(bytes, 14);
                var OilLimitationD = PlcConvert.GetRealAt(bytes, 18);
                var OilActual = PlcConvert.GetRealAt(bytes, 22);

                string sql = "INSERT INTO dbo.Oiling(SerialNumber,StationCode,StepNo,Status,QualityCode,Program," +
                    "CalibrationRecord,Temperature,OilLimitationU,OilLimitationD,OilActual)" +
                             $"VALUES('CALIBRATING','{_workpiece.StationCode}',{stepNo},{Status}," +
                             $"{QualityCode},{Program},{CalibrationRecord},{Temperature},{OilLimitationU},{OilLimitationD}" +
                             $",{OilActual})";

                _db.Db.Ado.ExecuteCommand(sql);

                int result;
                result = Status == 2 ? 1 : 0;
                sql = "INSERT INTO dbo.QualityData(SerialNumber,StationCode,DataType,DataTableName,StepNo,DataPrimaryKey,CheckResult)" +
                        $"VALUES('CALIBRATING','{_workpiece.StationCode}','{6}','',{stepNo},'',{result})";
                _db.Db.Ado.ExecuteCommand(sql);

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "注油数据采集出错CB");
                return false;
            }
        }
        private bool OP10GluingData(byte[] bytes)
        {
            try
            {
                var Status = PlcConvert.GetIntAt(bytes, 2);
                var GluingTime = PlcConvert.GetRealAt(bytes, 4);

                string sql =
                    "INSERT INTO dbo.OP10GluingData(Status,GluingTime,SerialNumber)" +
                    $"VALUES('{Status}','{GluingTime}','{_workpiece.SerialNumber}')";

                _db.Db.Ado.ExecuteCommand(sql);

                int result = Status == 2 ? 1 : 0;

                sql = "INSERT INTO dbo.QualityData(SerialNumber,StationCode,DataType,DataTableName,StepNo,DataPrimaryKey,CheckResult)" +
                        $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{901}','',{901},'',{result})";
                _db.Db.Ado.ExecuteCommand(sql);

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "OP10GluingData");
                //MessageBox.Show("OP10GluingData");
                return false;
            }
        }

        private bool OP10PressXData(byte[] bytes)
        {
            try
            {
                var Status = PlcConvert.GetIntAt(bytes, 2);
                var Pressure_Limiation_U = PlcConvert.GetRealAt(bytes, 4);
                var Pressure_Value = PlcConvert.GetRealAt(bytes, 8);
                var Pressure_Limiation_D = PlcConvert.GetRealAt(bytes, 12);
                var Displacement_Value = PlcConvert.GetRealAt(bytes, 16);
                var DisplacementLimitationD = PlcConvert.GetRealAt(bytes, 20);
                var DisplacementLimitationU = PlcConvert.GetRealAt(bytes, 24);

                string sql =
                    "INSERT INTO dbo.OP10PressXData(Status,Pressure_Limiation_U,Pressure_Value,Pressure_Limiation_D,Displacement_Value,SerialNumber,DisplacementLimitationD,DisplacementLimitationU)" +
                    $"VALUES('{Status}','{Pressure_Limiation_U}','{Pressure_Value}','{Pressure_Limiation_D}','{Displacement_Value}','{_workpiece.SerialNumber}','{DisplacementLimitationD}','{DisplacementLimitationU}')";

                _db.Db.Ado.ExecuteCommand(sql);

                int result = Status == 2 ? 1 : 0;

                sql = "INSERT INTO dbo.QualityData(SerialNumber,StationCode,DataType,DataTableName,StepNo,DataPrimaryKey,CheckResult)" +
                        $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{902}','',{902},'',{result})";
                _db.Db.Ado.ExecuteCommand(sql);

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "OP10PressXData");
                //MessageBox.Show("OP10PressXData");
                return false;
            }
        }
        private bool OP20ThreePointData(byte[] bytes)
        {
            try
            {
                var ExtremeStatus = PlcConvert.GetIntAt(bytes, 2);
                var MeasureStatus = PlcConvert.GetIntAt(bytes, 4);
                var CalibrationRecord = PlcConvert.GetIntAt(bytes, 6);
                var MasterMeasure = PlcConvert.GetRealAt(bytes, 8);
                var MeasureU = PlcConvert.GetRealAt(bytes, 12);
                var MeasureD = PlcConvert.GetRealAt(bytes, 16);
                var MeasureValue = PlcConvert.GetRealAt(bytes, 20);
                var ThreePointRange = PlcConvert.GetRealAt(bytes, 24);
                var ExtremumValue = PlcConvert.GetRealAt(bytes, 28);
                var V1 = PlcConvert.GetRealAt(bytes, 32);
                var V2 = PlcConvert.GetRealAt(bytes, 36);
                var V3 = PlcConvert.GetRealAt(bytes, 40);

                string sql =
                    "INSERT INTO dbo.OP20ThreePointData(ExtremeStatus,MeasureStatus,CalibrationRecord,MasterMeasure,MeasureU,MeasureD,MeasureValue,ThreePointRange,ExtremumValue,SerialNumber,V1,V2,V3)" +
                    $"VALUES('{ExtremeStatus}','{MeasureStatus}','{CalibrationRecord}','{MasterMeasure}','{MeasureU}','{MeasureD}','{MeasureValue}','{ThreePointRange}','{ExtremumValue}','{_workpiece.SerialNumber}','{V1}','{V2}','{V3}')";

                _db.Db.Ado.ExecuteCommand(sql);

                int result = MeasureStatus == 2 ? 1 : 0;

                sql = "INSERT INTO dbo.QualityData(SerialNumber,StationCode,DataType,DataTableName,StepNo,DataPrimaryKey,CheckResult)" +
                        $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{903}','',{903},'',{result})";
                _db.Db.Ado.ExecuteCommand(sql);

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "OP20ThreePointData");
                //MessageBox.Show("OP20ThreePointData");
                return false;
            }
        }
        private bool OP20ThreePointDataCB(byte[] bytes)
        {
            try
            {
                var ExtremeStatus = PlcConvert.GetIntAt(bytes, 2);
                var MeasureStatus = PlcConvert.GetIntAt(bytes, 4);
                var CalibrationRecord = PlcConvert.GetIntAt(bytes, 6);
                var MasterMeasure = PlcConvert.GetRealAt(bytes, 8);
                var MeasureU = PlcConvert.GetRealAt(bytes, 12);
                var MeasureD = PlcConvert.GetRealAt(bytes, 16);
                var MeasureValue = PlcConvert.GetRealAt(bytes, 20);
                var ThreePointRange = PlcConvert.GetRealAt(bytes, 24);
                var ExtremumValue = PlcConvert.GetRealAt(bytes, 28);
                var V1 = PlcConvert.GetRealAt(bytes, 32);
                var V2 = PlcConvert.GetRealAt(bytes, 36);
                var V3 = PlcConvert.GetRealAt(bytes, 40);

                string sql =
                    "INSERT INTO dbo.OP20ThreePointData(ExtremeStatus,MeasureStatus,CalibrationRecord,MasterMeasure,MeasureU,MeasureD,MeasureValue,ThreePointRange,ExtremumValue,SerialNumber,V1,V2,V3)" +
                    $"VALUES('{ExtremeStatus}','{MeasureStatus}','{CalibrationRecord}','{MasterMeasure}','{MeasureU}','{MeasureD}','{MeasureValue}','{ThreePointRange}','{ExtremumValue}','CALIBRATING','{V1}','{V2}','{V3}')";

                _db.Db.Ado.ExecuteCommand(sql);

                int result = MeasureStatus == 2 ? 1 : 0;

                sql = "INSERT INTO dbo.QualityData(SerialNumber,StationCode,DataType,DataTableName,StepNo,DataPrimaryKey,CheckResult)" +
                        $"VALUES('CALIBRATING','{_workpiece.StationCode}','{903}','',{903},'',{result})";
                _db.Db.Ado.ExecuteCommand(sql);

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "OP20ThreePointDataCB");
                return false;
            }
        }
        private bool OP30TorqueTest(byte[] bytes)
        {
            try
            {
                var Status = PlcConvert.GetIntAt(bytes, 2);
                var TorqueTestLimitU = PlcConvert.GetRealAt(bytes, 4);
                var TorqueTestLimitL = PlcConvert.GetRealAt(bytes, 8);
                var TorqueActual = PlcConvert.GetRealAt(bytes, 12);

                string sql =
                    "INSERT INTO dbo.OP30TorqueTest(Status,TorqueTestLimitU,TorqueTestLimitL,TorqueActual,SerialNumber)" +
                    $"VALUES('{Status}','{TorqueTestLimitU}','{TorqueTestLimitL}','{TorqueActual}','{_workpiece.SerialNumber}')";

                _db.Db.Ado.ExecuteCommand(sql);

                int result = Status == 2 ? 1 : 0;

                sql = "INSERT INTO dbo.QualityData(SerialNumber,StationCode,DataType,DataTableName,StepNo,DataPrimaryKey,CheckResult)" +
                        $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{904}','',{904},'',{result})";
                _db.Db.Ado.ExecuteCommand(sql);

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "OP30TorqueTest");
                //MessageBox.Show("OP30TorqueTest");
                return false;
            }
        }
        private bool OP30DPTest(byte[] bytes)
        {
            try
            {
                var Status = PlcConvert.GetIntAt(bytes, 2);
                var CheckResult = PlcConvert.GetRealAt(bytes, 4);
                var CalculateResult = PlcConvert.GetRealAt(bytes, 8);
                var GasketNumber = PlcConvert.GetRealAt(bytes, 12);
                var GapRange = PlcConvert.GetRealAt(bytes, 16);
                var GapValue = PlcConvert.GetRealAt(bytes, 20);
                var MasterValue = PlcConvert.GetRealAt(bytes, 24);
                var MasterTestValue = PlcConvert.GetRealAt(bytes, 28);

                string sql =
                    "INSERT INTO dbo.OP30DPTest(Status,CheckResult,CalculateResult,GasketNumber,GapRange,GapValue,SerialNumber,MasterValue,MasterTestValue)" +
                    $"VALUES('{Status}','{CheckResult}','{CalculateResult}','{GasketNumber}','{GapRange}" +
                    $"','{GapValue}','{_workpiece.SerialNumber}','{MasterValue}','{MasterTestValue}')";

                _db.Db.Ado.ExecuteCommand(sql);

                int result = Status == 2 ? 1 : 0;

                sql = "INSERT INTO dbo.QualityData(SerialNumber,StationCode,DataType,DataTableName,StepNo,DataPrimaryKey,CheckResult)" +
                        $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{905}','',{905},'',{result})";
                _db.Db.Ado.ExecuteCommand(sql);

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "OP30DPTest");
                //MessageBox.Show("OP30DPTest");
                return false;
            }
        }
        private bool OP30DPTestCB(byte[] bytes)
        {
            try
            {
                var Status = PlcConvert.GetIntAt(bytes, 2);
                var CheckResult = PlcConvert.GetRealAt(bytes, 4);
                var CalculateResult = PlcConvert.GetRealAt(bytes, 8);
                var GasketNumber = PlcConvert.GetRealAt(bytes, 12);
                var GapRange = PlcConvert.GetRealAt(bytes, 16);
                var GapValue = PlcConvert.GetRealAt(bytes, 20);
                var MasterValue = PlcConvert.GetRealAt(bytes, 24);
                var MasterTestValue = PlcConvert.GetRealAt(bytes, 28);

                string sql =
                    "INSERT INTO dbo.OP30DPTest(Status,CheckResult,CalculateResult,GasketNumber,GapRange,GapValue,SerialNumber,MasterValue,MasterTestValue)" +
                    $"VALUES('{Status}','{CheckResult}','{CalculateResult}','{GasketNumber}','{GapRange}" +
                    $"','{GapValue}','CALIBRATING','{MasterValue}','{MasterTestValue}')";

                _db.Db.Ado.ExecuteCommand(sql);

                int result = Status == 2 ? 1 : 0;

                sql = "INSERT INTO dbo.QualityData(SerialNumber,StationCode,DataType,DataTableName,StepNo,DataPrimaryKey,CheckResult)" +
                        $"VALUES('CALIBRATING','{_workpiece.StationCode}','{905}','',{905},'',{result})";
                _db.Db.Ado.ExecuteCommand(sql);

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "OP30DPTestCB");
                return false;
            }
        }

        private bool OP021KeyCode(byte[] bytes, short codetype, int DStartAddress)
        {
            byte[] bytes0;
            string KeyCode = "";

            try
            {
                if (codetype == 2)//定轮
                {
                    bytes0 = PlcHelper.ReadBytes(SystemConfig.DataDB, DStartAddress + 294, 48);
                    KeyCode = Encoding.ASCII.GetString(bytes0).Trim('\0');
                }
                else if (codetype == 3)//动轮
                {
                    bytes0 = PlcHelper.ReadBytes(SystemConfig.DataDB, DStartAddress + 344, 48);
                    KeyCode = Encoding.ASCII.GetString(bytes0).Trim('\0');
                }

                string sql = "INSERT INTO dbo.KeyCodeInfo(SerialNumber,StationCode,KeyCode,CodeType) " +
                          $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{KeyCode}','{codetype}')";
                _db.Db.Ado.ExecuteCommand(sql);
                LogHelper.Info($"21条码数据->{KeyCode}->{codetype}");
                return true;
            }
            catch { return false; }
        }

        private bool VisionInspection(byte[] bytes, int stepNo)
        {
            try
            {
                var Status = PlcConvert.GetIntAt(bytes, 2);
                var QualityCode = PlcConvert.GetIntAt(bytes, 4);
                var Program = PlcConvert.GetIntAt(bytes, 6);

                string sql = "INSERT INTO dbo.VisionInspection(SerialNumber,StationCode,StepNo,Status,QualityCode,Program)" +
                             $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}',{stepNo},{Status}," +
                             $"{QualityCode},{Program})";

                _db.Db.Ado.ExecuteCommand(sql);

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
                        $"VALUES('{_workpiece.SerialNumber}','{_workpiece.StationCode}','{10}','',{stepNo},'',{result})";
                _db.Db.Ado.ExecuteCommand(sql);

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error(exception, "相机检测数据采集出错");
                //MessageBox.Show("注油数据采集出错");
                return false;
            }
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
                if(!string.IsNullOrEmpty(EKSCode) && !string.IsNullOrEmpty(_workpiece.SerialNumber))
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
    }
}
