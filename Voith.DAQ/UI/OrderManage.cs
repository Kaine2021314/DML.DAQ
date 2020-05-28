using CCWin;
using SqlSugar;
using System;
using System.Collections.Generic;
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
    public partial class OrderManage : Skin_Color
    {
        private static readonly DbContext Db = new DbContext();
        int count = 0;
        int id = 0;
        public OrderManage()
        {
            InitializeComponent();

            DataGridView dgv0 = dataGridView1;
            dgv0.AllowUserToResizeColumns = true;
            dgv0.AllowUserToResizeRows = false;
            dgv0.RowTemplate.ReadOnly = true;
            dgv0.AllowUserToAddRows = false;
            dgv0.RowHeadersVisible = false;
            dgv0.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                List<GoodsOrder> gl = Db.GoodsOrderDb.AsQueryable().Where(it => it.ID >= id).OrderBy(it => it.ID, OrderByType.Asc).ToList();

                if (gl[selectRow].OrderStatus == 1 && comboBox1.Text == "0")
                {
                    if (MessageBox.Show("将状态为1的订单,状态改为0，可能会造成数据异常，是否继续？",
                        "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                        return;
                }

                //if (gl[selectRow].OrderStatus == 0 || gl[selectRow].OrderStatus == 1)
                //{
                gl[selectRow].OrderStatus = int.Parse(comboBox1.Text);
                    Db.GoodsOrderDb.Update(gl[selectRow]);
                //}
                GetGoodsOrder();
            }
            catch { }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        GoodsOrder goodsOrder0;//最后一条
        GoodsOrder goodsOrder;//订单头
        private void OrderManage_Load(object sender, EventArgs e)
        {
            GetGoodsOrder();
        }

        void GetGoodsOrder()
        {
            try
            {
                goodsOrder = Db.GoodsOrderDb.AsQueryable().Where(it => it.OrderStatus == 0).OrderBy(it => it.ID, OrderByType.Asc).First();
                if (goodsOrder != null)
                {
                    goodsOrder = Db.GoodsOrderDb.AsQueryable().Where(it => it.ID < (goodsOrder.ID + 1) && it.HeadOrder == 1).OrderBy(it => it.ID, OrderByType.Desc).First();
                    goodsOrder0 = Db.GoodsOrderDb.AsQueryable().Where(it => it.OrderStatus == 0).OrderBy(it => it.ID, OrderByType.Desc).First();
                    dataGridView1.DataSource = Db.GoodsOrderDb.AsQueryable().Where(it => it.ID >= goodsOrder.ID).OrderBy(it => it.ID, OrderByType.Asc).ToDataTable();

                    textBox1.Text = goodsOrder.SerialNumber;
                    numericUpDown1.Value = goodsOrder.Count;

                    count = goodsOrder.Count;
                    id = goodsOrder.ID;
                }
                else
                {
                    dataGridView1.DataSource = null;
                    dataGridView1.Rows.Clear();

                    textBox1.Text = "-";
                    numericUpDown1.Value = 0;

                    count = 0;
                    id = 0;
                    numericUpDown2.Value = 0;
                }
            }
            catch { }
        }

        private void skinButton1_Click(object sender, EventArgs e)
        {
            try
            {
                if (goodsOrder == null || goodsOrder0 == null)
                {
                    return;
                }

                int acount = (int)numericUpDown2.Value;

                for (int i = 0; i < acount; i++)
                {
                    GoodsOrder order = new GoodsOrder
                    {
                        MaterielCode = goodsOrder0.MaterielCode,
                        ProductionOrderCode = goodsOrder0.ProductionOrderCode,
                        SerialNumber = (Convert.ToInt32(goodsOrder0.SerialNumber) + i + 1).ToString(),//sn + (lastNo + i).ToString(),
                        OrderStatus = 0,
                        CheckResult = 0,
                        LocalTime = DateTime.Now,
                        Type1 = goodsOrder0.Type1,
                        Type2 = goodsOrder0.Type2,

                        PalletCode = "-",
                        ProductType = goodsOrder0.ProductType,
                    };
                    Db.GoodsOrderDb.Insert(order);
                }
                goodsOrder.Count += acount;
                count = goodsOrder.Count;
                Db.GoodsOrderDb.Update(goodsOrder);
                GetGoodsOrder();

                //dataGridView1.DataSource = Db.GoodsOrderDb.AsQueryable().Where(it => it.ID >= id && it.OrderStatus != 3).OrderBy(it => it.ID, OrderByType.Asc).ToDataTable();
            }
            catch { }
        }

        private void skinButton2_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult d = MessageBox.Show("是否要删除部分未使用订单项？","警告",MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (d != DialogResult.Yes)
                    return;

                if (goodsOrder == null || goodsOrder0 == null)
                {
                    return;
                }

                var goodsOrderL = Db.GoodsOrderDb.AsQueryable().Where(it => it.OrderStatus == 0).OrderBy(it => it.ID, OrderByType.Desc);
                int scount = (int)numericUpDown2.Value;
                int rcount = goodsOrderL.Count();
                if (scount > rcount)
                {
                    //提示数量错误
                }
                else if (scount <= rcount)
                {
                    List<GoodsOrder> gl = goodsOrderL.ToList();
                    for (int i = 0; i < scount; i++)
                    {
                        //gl[i].OrderStatus = 3;
                        //Db.GoodsOrderDb.Update(gl[i]);
                        Db.GoodsOrderDb.Delete(gl[i]);
                    }
                    goodsOrder.Count -= scount;
                    count = goodsOrder.Count;
                    Db.GoodsOrderDb.Update(goodsOrder);
                    GetGoodsOrder();
                }
                //dataGridView1.DataSource = Db.GoodsOrderDb.AsQueryable().Where(it => it.ID >= id && it.OrderStatus != 3).OrderBy(it => it.ID, OrderByType.Asc).ToDataTable();
            }
            catch { }
        }

        int selectRow = 0;
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1.Rows[i].Selected)
                    {
                        selectRow = i;
                        comboBox1.Text = dataGridView1.Rows[i].Cells["OrderStatus"].Value.ToString();
                    }
                }
            }
            catch { }
        }
    }
}
