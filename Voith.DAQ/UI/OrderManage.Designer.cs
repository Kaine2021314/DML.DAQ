namespace Voith.DAQ.UI
{
    partial class OrderManage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnCancel = new CCWin.SkinControl.SkinButton();
            this.btnConfirm = new CCWin.SkinControl.SkinButton();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.skinButton1 = new CCWin.SkinControl.SkinButton();
            this.skinButton2 = new CCWin.SkinControl.SkinButton();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.Transparent;
            this.btnCancel.BaseColor = System.Drawing.Color.Gray;
            this.btnCancel.BorderColor = System.Drawing.Color.DarkGray;
            this.btnCancel.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btnCancel.DownBack = null;
            this.btnCancel.Location = new System.Drawing.Point(843, 341);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2);
            this.btnCancel.MouseBack = null;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.NormlBack = null;
            this.btnCancel.Size = new System.Drawing.Size(65, 27);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // btnConfirm
            // 
            this.btnConfirm.BackColor = System.Drawing.Color.Transparent;
            this.btnConfirm.BaseColor = System.Drawing.Color.Green;
            this.btnConfirm.BorderColor = System.Drawing.Color.DarkGreen;
            this.btnConfirm.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btnConfirm.DownBack = null;
            this.btnConfirm.Location = new System.Drawing.Point(751, 341);
            this.btnConfirm.Margin = new System.Windows.Forms.Padding(2);
            this.btnConfirm.MouseBack = null;
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.NormlBack = null;
            this.btnConfirm.Size = new System.Drawing.Size(65, 27);
            this.btnConfirm.TabIndex = 4;
            this.btnConfirm.Text = "状态变更";
            this.btnConfirm.UseVisualStyleBackColor = false;
            this.btnConfirm.Click += new System.EventHandler(this.BtnConfirm_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(11, 51);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(962, 284);
            this.dataGridView1.TabIndex = 5;
            this.dataGridView1.SelectionChanged += new System.EventHandler(this.dataGridView1_SelectionChanged);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(73, 368);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            900,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(82, 21);
            this.numericUpDown1.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 370);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "订单数量";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 344);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "订单头SN";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(73, 341);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(173, 21);
            this.textBox1.TabIndex = 10;
            // 
            // skinButton1
            // 
            this.skinButton1.BackColor = System.Drawing.Color.Transparent;
            this.skinButton1.BaseColor = System.Drawing.Color.Green;
            this.skinButton1.BorderColor = System.Drawing.Color.DarkGreen;
            this.skinButton1.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.skinButton1.DownBack = null;
            this.skinButton1.Location = new System.Drawing.Point(499, 341);
            this.skinButton1.Margin = new System.Windows.Forms.Padding(2);
            this.skinButton1.MouseBack = null;
            this.skinButton1.Name = "skinButton1";
            this.skinButton1.NormlBack = null;
            this.skinButton1.Size = new System.Drawing.Size(65, 27);
            this.skinButton1.TabIndex = 11;
            this.skinButton1.Text = "添加";
            this.skinButton1.UseVisualStyleBackColor = false;
            this.skinButton1.Click += new System.EventHandler(this.skinButton1_Click);
            // 
            // skinButton2
            // 
            this.skinButton2.BackColor = System.Drawing.Color.Transparent;
            this.skinButton2.BaseColor = System.Drawing.Color.Green;
            this.skinButton2.BorderColor = System.Drawing.Color.DarkGreen;
            this.skinButton2.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.skinButton2.DownBack = null;
            this.skinButton2.Location = new System.Drawing.Point(499, 372);
            this.skinButton2.Margin = new System.Windows.Forms.Padding(2);
            this.skinButton2.MouseBack = null;
            this.skinButton2.Name = "skinButton2";
            this.skinButton2.NormlBack = null;
            this.skinButton2.Size = new System.Drawing.Size(65, 27);
            this.skinButton2.TabIndex = 12;
            this.skinButton2.Text = "删除";
            this.skinButton2.UseVisualStyleBackColor = false;
            this.skinButton2.Click += new System.EventHandler(this.skinButton2_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(327, 343);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 14;
            this.label3.Text = "变更数量";
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Location = new System.Drawing.Point(386, 341);
            this.numericUpDown2.Maximum = new decimal(new int[] {
            900,
            0,
            0,
            0});
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(82, 21);
            this.numericUpDown2.TabIndex = 13;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3"});
            this.comboBox1.Location = new System.Drawing.Point(674, 340);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(72, 20);
            this.comboBox1.TabIndex = 15;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(639, 344);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 16;
            this.label4.Text = "状态";
            // 
            // OrderManage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 418);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numericUpDown2);
            this.Controls.Add(this.skinButton2);
            this.Controls.Add(this.skinButton1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnConfirm);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OrderManage";
            this.ShowBorder = false;
            this.ShowDrawIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "订单管理";
            this.Load += new System.EventHandler(this.OrderManage_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private CCWin.SkinControl.SkinButton btnCancel;
        private CCWin.SkinControl.SkinButton btnConfirm;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
        private CCWin.SkinControl.SkinButton skinButton1;
        private CCWin.SkinControl.SkinButton skinButton2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label4;
    }
}