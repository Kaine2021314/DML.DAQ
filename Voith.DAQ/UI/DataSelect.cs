using CCWin;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Voith.DAQ.Common;
using Voith.DAQ.DB;
using Voith.DAQ.Model;
using Voith.DAQ.Services;

namespace Voith.DAQ.UI
{
    /// <summary>
    /// 配方导入类型选择对话框
    /// </summary>
    public partial class DataSelect : Skin_Color
    {
        private static readonly DbContext Dbc = new DbContext();
        int count = 0;
        int id = 0;
        public DataSelect()
        {
            InitializeComponent();

            for (int i = 1; i < 9; i++)
            {
                DataGridView dgv0 = (DataGridView)Controls.Find("dataGridView" + i, true)[0];
                dgv0.AllowUserToResizeColumns = true;
                dgv0.AllowUserToResizeRows = false;
                dgv0.RowTemplate.ReadOnly = true;
                dgv0.AllowUserToAddRows = false;
                dgv0.RowHeadersVisible = false;
                dgv0.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            }
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                string SN = textBox1.Text;
                string PN = textBox2.Text;
                string dateStart = dateTimePicker1.Value.ToString();
                string dateEnd = dateTimePicker2.Value.ToString();

                string Condition = " 1=1";
                if (checkBox1.Checked)
                    Condition += $" and OnLineTime between '{dateStart}' and '{dateEnd}'";
                //else if (!string.IsNullOrEmpty(PN))
                //    Condition += $" and ProductionOrderCode='{PN}'";
                else if (!string.IsNullOrEmpty(SN))
                    Condition += $" and SerialNumber='{SN}'";

                if (Condition == " 1=1")
                    return;

                List<string> SNLst = new List<string>();
                string SNs = "";
                string sql = $"SELECT * FROM GoodsOrder WHERE {Condition} ORDER BY ID";
                var dataList = Dbc.Db.Ado.GetDataTable(sql);
                foreach (DataRow r in dataList.Rows)
                {
                    SNLst.Add(r["SerialNumber"].ToString());
                    SNs += $"'{r["SerialNumber"]}',";
                }
                SNs = SNs.TrimEnd(',');

                string StationCode = textBox3.Text;
                string c2 = "";
                if (!string.IsNullOrEmpty(StationCode))
                    c2 += $" and StationCode='{StationCode}'";
                string StepNo = textBox4.Text;
                string c3 = "";
                if (!string.IsNullOrEmpty(StepNo))
                    c3 += $" and StepNo='{StepNo}'";

                if (SNs != "")
                {
                    sql = $"SELECT * FROM QualityData WHERE SerialNumber in ({SNs}) ORDER BY SerialNumber,LocalTime";
                    dataGridView1.DataSource = Dbc.Db.Ado.GetDataTable(sql);

                    sql = $"SELECT * FROM Tighten WHERE SerialNumber in ({SNs}){c2}{c3} ORDER BY SerialNumber,LocalTime";
                    dataGridView2.DataSource = Dbc.Db.Ado.GetDataTable(sql);
                    sql = $"SELECT * FROM Press WHERE SerialNumber in ({SNs}){c2}{c3} ORDER BY SerialNumber,LocalTime";
                    dataGridView3.DataSource = Dbc.Db.Ado.GetDataTable(sql);
                    sql = $"SELECT * FROM CLXDCLZ WHERE SerialNumber in ({SNs}){c2}{c3} ORDER BY SerialNumber,LocalTime";
                    dataGridView4.DataSource = Dbc.Db.Ado.GetDataTable(sql);
                    sql = $"SELECT * FROM OP20ThreePointData WHERE SerialNumber in ({SNs}) ORDER BY SerialNumber,LocalTime";
                    dataGridView5.DataSource = Dbc.Db.Ado.GetDataTable(sql);
                    sql = $"SELECT * FROM OP10GluingData WHERE SerialNumber in ({SNs}) ORDER BY SerialNumber,LocalTime";
                    dataGridView6.DataSource = Dbc.Db.Ado.GetDataTable(sql);
                    sql = $"SELECT * FROM OP30DPTest WHERE SerialNumber in ({SNs}) ORDER BY SerialNumber,LocalTime";
                    dataGridView7.DataSource = Dbc.Db.Ado.GetDataTable(sql);
                    sql = $"SELECT * FROM OP30TorqueTest WHERE SerialNumber in ({SNs}) ORDER BY SerialNumber,LocalTime";
                    dataGridView8.DataSource = Dbc.Db.Ado.GetDataTable(sql);
                    sql = $"SELECT * FROM LeakTest WHERE SerialNumber in ({SNs}) ORDER BY SerialNumber,LocalTime";
                    dataGridView9.DataSource = Dbc.Db.Ado.GetDataTable(sql);
                }

                GetColumns();
            }
            catch { }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        GoodsOrder goodsOrder;
        private void OrderManage_Load(object sender, EventArgs e)
        {
            try
            {
                //goodsOrder = Db.GoodsOrderDb.AsQueryable().Where(it => it.OrderStatus == 0).OrderBy(it => it.ID, OrderByType.Asc).First();
                //goodsOrder = Db.GoodsOrderDb.AsQueryable().Where(it => it.ID < goodsOrder.ID && it.HeadOrder == 1).OrderBy(it => it.ID, OrderByType.Desc).First();
                //dataGridView1.DataSource = Db.GoodsOrderDb.AsQueryable().Where(it => it.ID >= goodsOrder.ID && it.OrderStatus != 3).OrderBy(it => it.ID, OrderByType.Asc).ToDataTable();

                //textBox1.Text = goodsOrder.SerialNumber;

                //count = goodsOrder.Count;
                //id = goodsOrder.ID;
            }
            catch { }
        }

        /// <summary>
        /// CPK=MIN(CPU,CPL)
        /// CPU=(规格上限-平均值)/3倍标准偏差
        /// CPL=(平均值-规格下限)/3倍标准偏差
        /// </summary>
        double GetCPK(double[] dataListArray, double USL, double LSL)
        {
            double rAverage = GetAverage(dataListArray);

            //Dictionary<int, double> d2 = new Dictionary<int, double>();
            //d2.Add(2, 1.128);
            //d2.Add(3, 1.693);
            //d2.Add(4, 2.059);
            //d2.Add(5, 2.326);
            //d2.Add(6, 2.534);
            //d2.Add(7, 2.704);
            //d2.Add(8, 2.847);
            //d2.Add(9, 2.97);
            //d2.Add(10, 3.078);
            //d2.Add(11, 3.173);
            //d2.Add(12, 3.258);
            //d2.Add(13, 3.336);
            //d2.Add(14, 3.407);
            //d2.Add(15, 3.472);
            //d2.Add(16, 3.532);
            //d2.Add(17, 3.588);
            //d2.Add(18, 3.64);
            //d2.Add(19, 3.689);
            //d2.Add(20, 3.735);
            //d2.Add(21, 3.778);
            //d2.Add(22, 3.819);
            //d2.Add(23, 3.858);
            //d2.Add(24, 3.895);
            //d2.Add(25, 3.931);
            
            double standardVariance_st = StDev(dataListArray);
            double CPU = (USL - rAverage) / (3 * standardVariance_st);
            double CPL = (rAverage - LSL) / (3 * standardVariance_st);
            double CPK = CPU > CPL ? CPL : CPU;

            return CPK;
        }

        /// <summary>
        /// 标准差
        /// </summary>
        public double StDev(double[] arrData)
        {
            double xSum = 0F;
            double xAvg = 0F;
            double sSum = 0F;
            double tmpStDev = 0F;
            int arrNum = arrData.Length;
            for (int i = 0; i < arrNum; i++)
            {
                xSum += arrData[i];
            }
            xAvg = xSum / arrNum;
            for (int j = 0; j < arrNum; j++)
            {
                sSum += ((arrData[j] - xAvg) * (arrData[j] - xAvg));
            }
            tmpStDev = Math.Sqrt(sSum / (arrNum - 1));
            return tmpStDev;
        }
        /// <summary>
        /// 极差
        /// </summary>
        public double GetRange(double[] src)
        {
            double result = 0;
            if (src != null && src.Length > 0)
            {
                result = src.Max() - src.Min(); //(from mm in src select mm).Max() - (from mm in src select mm).Min();
            }
            result = (double)Math.Round(result, 6);
            return result;
        }
        /// <summary>
        /// 平均值
        /// </summary>
        public static double GetAverage(double[] src)
        {
            double result = 0;
            if (src != null && src.Length > 0)
            {
                for (int i = 0; i < src.Length; i++)
                {
                    result += src[i];
                }
                result /= src.Length;
            }
            result = Math.Round(result, 4);
            return result;
        }

        private void skinButton1_Click(object sender, EventArgs e)
        {
            try
            {
                label8.Text = "CPK:-";
                if (dgv == null)
                    return;
                List<double> datalistArr = new List<double>();
                for (int i = 0; i < dgv.Rows.Count; i++)
                {
                    double v = 0;
                    bool b = double.TryParse(dgv.Rows[i].Cells[comboBox1.SelectedIndex].Value.ToString(), out v);
                    if (b)
                        datalistArr.Add(v);
                }
                label8.Text = "CPK:" + GetCPK(datalistArr.ToArray(), (double)numericUpDown4.Value, (double)numericUpDown3.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetColumns();
        }


        DataGridView dgv = null;
        void GetColumns()
        {
            try
            {
                int selectIndex = tabControl1.SelectedIndex + 1;

                dgv = (DataGridView)Controls.Find("dataGridView" + selectIndex, true)[0];
                comboBox1.Items.Clear();
                List<object> itmes = new List<object>();
                for (int i = 0; i < dgv.Columns.Count; i++)
                {
                    if (dgv.Columns[i].GetType() == typeof(DataGridViewTextBoxColumn))
                        itmes.Add(dgv.Columns[i].Name.ToString());
                }
                comboBox1.Items.AddRange(itmes.ToArray());
                if (comboBox1.Items.Count > 0)
                    comboBox1.SelectedIndex = 0;
            }
            catch { }
        }
    }
}
