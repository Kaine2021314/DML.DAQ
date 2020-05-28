namespace Voith.DAQ.UI
{
    partial class FrmDataExport
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmDataExport));
            this.skinTabControl1 = new CCWin.SkinControl.SkinTabControl();
            this.tbTiming = new CCWin.SkinControl.SkinTabPage();
            this.btnSelectExportPath = new System.Windows.Forms.Button();
            this.btnStart = new CCWin.SkinControl.SkinButton();
            this.txtExportPath = new System.Windows.Forms.TextBox();
            this.rbMonth = new CCWin.SkinControl.SkinRadioButton();
            this.rbWeek = new CCWin.SkinControl.SkinRadioButton();
            this.rbDay = new CCWin.SkinControl.SkinRadioButton();
            this.rbClass = new CCWin.SkinControl.SkinRadioButton();
            this.tbRegion = new CCWin.SkinControl.SkinTabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.tbSingle = new CCWin.SkinControl.SkinTabPage();
            this.lable1 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnExport = new CCWin.SkinControl.SkinButton();
            this.dtpBegin = new System.Windows.Forms.DateTimePicker();
            this.dtpEnd = new System.Windows.Forms.DateTimePicker();
            this.lable3 = new System.Windows.Forms.Label();
            this.btnQuery = new CCWin.SkinControl.SkinButton();
            this.txtSerialNumber = new System.Windows.Forms.TextBox();
            this.skinTabControl1.SuspendLayout();
            this.tbTiming.SuspendLayout();
            this.tbRegion.SuspendLayout();
            this.tbSingle.SuspendLayout();
            this.SuspendLayout();
            // 
            // skinTabControl1
            // 
            this.skinTabControl1.AnimatorType = CCWin.SkinControl.AnimationType.HorizSlide;
            this.skinTabControl1.BackColor = System.Drawing.Color.DarkGray;
            this.skinTabControl1.CloseRect = new System.Drawing.Rectangle(2, 2, 12, 12);
            this.skinTabControl1.Controls.Add(this.tbTiming);
            this.skinTabControl1.Controls.Add(this.tbRegion);
            this.skinTabControl1.Controls.Add(this.tbSingle);
            this.skinTabControl1.HeadBack = null;
            this.skinTabControl1.ImgTxtOffset = new System.Drawing.Point(0, 0);
            this.skinTabControl1.ItemSize = new System.Drawing.Size(100, 36);
            this.skinTabControl1.Location = new System.Drawing.Point(2, 58);
            this.skinTabControl1.Name = "skinTabControl1";
            this.skinTabControl1.PageArrowDown = ((System.Drawing.Image)(resources.GetObject("skinTabControl1.PageArrowDown")));
            this.skinTabControl1.PageArrowHover = ((System.Drawing.Image)(resources.GetObject("skinTabControl1.PageArrowHover")));
            this.skinTabControl1.PageCloseHover = ((System.Drawing.Image)(resources.GetObject("skinTabControl1.PageCloseHover")));
            this.skinTabControl1.PageCloseNormal = ((System.Drawing.Image)(resources.GetObject("skinTabControl1.PageCloseNormal")));
            this.skinTabControl1.PageDown = ((System.Drawing.Image)(resources.GetObject("skinTabControl1.PageDown")));
            this.skinTabControl1.PageHover = ((System.Drawing.Image)(resources.GetObject("skinTabControl1.PageHover")));
            this.skinTabControl1.PageImagePosition = CCWin.SkinControl.SkinTabControl.ePageImagePosition.Left;
            this.skinTabControl1.PageNorml = null;
            this.skinTabControl1.SelectedIndex = 2;
            this.skinTabControl1.Size = new System.Drawing.Size(796, 381);
            this.skinTabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.skinTabControl1.TabIndex = 0;
            // 
            // tbTiming
            // 
            this.tbTiming.BackColor = System.Drawing.Color.White;
            this.tbTiming.Controls.Add(this.lable1);
            this.tbTiming.Controls.Add(this.btnSelectExportPath);
            this.tbTiming.Controls.Add(this.btnStart);
            this.tbTiming.Controls.Add(this.txtExportPath);
            this.tbTiming.Controls.Add(this.rbMonth);
            this.tbTiming.Controls.Add(this.rbWeek);
            this.tbTiming.Controls.Add(this.rbDay);
            this.tbTiming.Controls.Add(this.rbClass);
            this.tbTiming.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbTiming.Location = new System.Drawing.Point(0, 36);
            this.tbTiming.Name = "tbTiming";
            this.tbTiming.Size = new System.Drawing.Size(796, 345);
            this.tbTiming.TabIndex = 1;
            this.tbTiming.TabItemImage = null;
            this.tbTiming.Text = "定时导出";
            // 
            // btnSelectExportPath
            // 
            this.btnSelectExportPath.Location = new System.Drawing.Point(602, 171);
            this.btnSelectExportPath.Name = "btnSelectExportPath";
            this.btnSelectExportPath.Size = new System.Drawing.Size(57, 28);
            this.btnSelectExportPath.TabIndex = 5;
            this.btnSelectExportPath.Text = "选择";
            this.btnSelectExportPath.UseVisualStyleBackColor = true;
            this.btnSelectExportPath.Click += new System.EventHandler(this.BtnSelectExportPath_Click);
            // 
            // btnStart
            // 
            this.btnStart.BackColor = System.Drawing.Color.Transparent;
            this.btnStart.BaseColor = System.Drawing.Color.Gray;
            this.btnStart.BorderColor = System.Drawing.Color.DarkGray;
            this.btnStart.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btnStart.DownBack = null;
            this.btnStart.DownBaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnStart.Font = new System.Drawing.Font("新宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStart.ForeColor = System.Drawing.Color.Black;
            this.btnStart.Location = new System.Drawing.Point(344, 268);
            this.btnStart.MouseBack = null;
            this.btnStart.Name = "btnStart";
            this.btnStart.NormlBack = null;
            this.btnStart.Size = new System.Drawing.Size(109, 37);
            this.btnStart.TabIndex = 4;
            this.btnStart.Text = "开始";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // txtExportPath
            // 
            this.txtExportPath.Location = new System.Drawing.Point(224, 171);
            this.txtExportPath.Name = "txtExportPath";
            this.txtExportPath.Size = new System.Drawing.Size(372, 28);
            this.txtExportPath.TabIndex = 3;
            // 
            // rbMonth
            // 
            this.rbMonth.AutoSize = true;
            this.rbMonth.BackColor = System.Drawing.Color.Transparent;
            this.rbMonth.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.rbMonth.DownBack = null;
            this.rbMonth.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rbMonth.Location = new System.Drawing.Point(415, 105);
            this.rbMonth.MouseBack = null;
            this.rbMonth.Name = "rbMonth";
            this.rbMonth.NormlBack = null;
            this.rbMonth.SelectedDownBack = null;
            this.rbMonth.SelectedMouseBack = null;
            this.rbMonth.SelectedNormlBack = null;
            this.rbMonth.Size = new System.Drawing.Size(107, 28);
            this.rbMonth.TabIndex = 1;
            this.rbMonth.Text = "按月导出";
            this.rbMonth.UseVisualStyleBackColor = false;
            // 
            // rbWeek
            // 
            this.rbWeek.AutoSize = true;
            this.rbWeek.BackColor = System.Drawing.Color.Transparent;
            this.rbWeek.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.rbWeek.DownBack = null;
            this.rbWeek.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rbWeek.Location = new System.Drawing.Point(415, 42);
            this.rbWeek.MouseBack = null;
            this.rbWeek.Name = "rbWeek";
            this.rbWeek.NormlBack = null;
            this.rbWeek.SelectedDownBack = null;
            this.rbWeek.SelectedMouseBack = null;
            this.rbWeek.SelectedNormlBack = null;
            this.rbWeek.Size = new System.Drawing.Size(107, 28);
            this.rbWeek.TabIndex = 1;
            this.rbWeek.Text = "按周导出";
            this.rbWeek.UseVisualStyleBackColor = false;
            // 
            // rbDay
            // 
            this.rbDay.AutoSize = true;
            this.rbDay.BackColor = System.Drawing.Color.Transparent;
            this.rbDay.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.rbDay.DownBack = null;
            this.rbDay.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rbDay.Location = new System.Drawing.Point(274, 105);
            this.rbDay.MouseBack = null;
            this.rbDay.Name = "rbDay";
            this.rbDay.NormlBack = null;
            this.rbDay.SelectedDownBack = null;
            this.rbDay.SelectedMouseBack = null;
            this.rbDay.SelectedNormlBack = null;
            this.rbDay.Size = new System.Drawing.Size(107, 28);
            this.rbDay.TabIndex = 1;
            this.rbDay.Text = "按天导出";
            this.rbDay.UseVisualStyleBackColor = false;
            // 
            // rbClass
            // 
            this.rbClass.AutoSize = true;
            this.rbClass.BackColor = System.Drawing.Color.Transparent;
            this.rbClass.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.rbClass.DownBack = null;
            this.rbClass.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rbClass.Location = new System.Drawing.Point(274, 42);
            this.rbClass.MouseBack = null;
            this.rbClass.Name = "rbClass";
            this.rbClass.NormlBack = null;
            this.rbClass.SelectedDownBack = null;
            this.rbClass.SelectedMouseBack = null;
            this.rbClass.SelectedNormlBack = null;
            this.rbClass.Size = new System.Drawing.Size(107, 28);
            this.rbClass.TabIndex = 1;
            this.rbClass.Text = "按班导出";
            this.rbClass.UseVisualStyleBackColor = false;
            // 
            // tbRegion
            // 
            this.tbRegion.BackColor = System.Drawing.Color.White;
            this.tbRegion.Controls.Add(this.dtpEnd);
            this.tbRegion.Controls.Add(this.dtpBegin);
            this.tbRegion.Controls.Add(this.btnExport);
            this.tbRegion.Controls.Add(this.label1);
            this.tbRegion.Controls.Add(this.label2);
            this.tbRegion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbRegion.Location = new System.Drawing.Point(0, 36);
            this.tbRegion.Name = "tbRegion";
            this.tbRegion.Size = new System.Drawing.Size(796, 345);
            this.tbRegion.TabIndex = 2;
            this.tbRegion.TabItemImage = null;
            this.tbRegion.Text = "区间导出";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(281, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 18);
            this.label2.TabIndex = 0;
            this.label2.Text = "开始时间";
            // 
            // tbSingle
            // 
            this.tbSingle.BackColor = System.Drawing.Color.White;
            this.tbSingle.Controls.Add(this.lable3);
            this.tbSingle.Controls.Add(this.btnQuery);
            this.tbSingle.Controls.Add(this.txtSerialNumber);
            this.tbSingle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbSingle.Location = new System.Drawing.Point(0, 36);
            this.tbSingle.Name = "tbSingle";
            this.tbSingle.Size = new System.Drawing.Size(796, 345);
            this.tbSingle.TabIndex = 3;
            this.tbSingle.TabItemImage = null;
            this.tbSingle.Text = "单个导出";
            // 
            // lable1
            // 
            this.lable1.AutoSize = true;
            this.lable1.Location = new System.Drawing.Point(138, 176);
            this.lable1.Name = "lable1";
            this.lable1.Size = new System.Drawing.Size(80, 18);
            this.lable1.TabIndex = 6;
            this.lable1.Text = "导出路径";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(281, 147);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "截止时间";
            // 
            // btnExport
            // 
            this.btnExport.BackColor = System.Drawing.Color.Transparent;
            this.btnExport.BaseColor = System.Drawing.Color.Gray;
            this.btnExport.BorderColor = System.Drawing.Color.DarkGray;
            this.btnExport.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btnExport.DownBack = null;
            this.btnExport.DownBaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnExport.Font = new System.Drawing.Font("新宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnExport.ForeColor = System.Drawing.Color.Black;
            this.btnExport.Location = new System.Drawing.Point(344, 256);
            this.btnExport.MouseBack = null;
            this.btnExport.Name = "btnExport";
            this.btnExport.NormlBack = null;
            this.btnExport.Size = new System.Drawing.Size(109, 37);
            this.btnExport.TabIndex = 5;
            this.btnExport.Text = "导出";
            this.btnExport.UseVisualStyleBackColor = false;
            this.btnExport.Click += new System.EventHandler(this.BtnExport_Click);
            // 
            // dtpBegin
            // 
            this.dtpBegin.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpBegin.Location = new System.Drawing.Point(367, 52);
            this.dtpBegin.Name = "dtpBegin";
            this.dtpBegin.Size = new System.Drawing.Size(148, 28);
            this.dtpBegin.TabIndex = 6;
            this.dtpBegin.CloseUp += new System.EventHandler(this.DtpEnd_ValueChanged);
            // 
            // dtpEnd
            // 
            this.dtpEnd.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpEnd.Location = new System.Drawing.Point(367, 140);
            this.dtpEnd.Name = "dtpEnd";
            this.dtpEnd.Size = new System.Drawing.Size(148, 28);
            this.dtpEnd.TabIndex = 6;
            this.dtpEnd.CloseUp += new System.EventHandler(this.DtpEnd_ValueChanged);
            // 
            // lable3
            // 
            this.lable3.AutoSize = true;
            this.lable3.Location = new System.Drawing.Point(178, 108);
            this.lable3.Name = "lable3";
            this.lable3.Size = new System.Drawing.Size(62, 18);
            this.lable3.TabIndex = 9;
            this.lable3.Text = "序列号";
            // 
            // btnQuery
            // 
            this.btnQuery.BackColor = System.Drawing.Color.Transparent;
            this.btnQuery.BaseColor = System.Drawing.Color.Gray;
            this.btnQuery.BorderColor = System.Drawing.Color.DarkGray;
            this.btnQuery.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btnQuery.DownBack = null;
            this.btnQuery.DownBaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnQuery.Font = new System.Drawing.Font("新宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnQuery.ForeColor = System.Drawing.Color.Black;
            this.btnQuery.Location = new System.Drawing.Point(344, 202);
            this.btnQuery.MouseBack = null;
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.NormlBack = null;
            this.btnQuery.Size = new System.Drawing.Size(109, 37);
            this.btnQuery.TabIndex = 8;
            this.btnQuery.Text = "导出";
            this.btnQuery.UseVisualStyleBackColor = false;
            this.btnQuery.Click += new System.EventHandler(this.BtnQuery_Click);
            // 
            // txtSerialNumber
            // 
            this.txtSerialNumber.Location = new System.Drawing.Point(246, 105);
            this.txtSerialNumber.Name = "txtSerialNumber";
            this.txtSerialNumber.Size = new System.Drawing.Size(372, 28);
            this.txtSerialNumber.TabIndex = 7;
            // 
            // FrmDataExport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.skinTabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmDataExport";
            this.Text = "数据导出";
            this.Load += new System.EventHandler(this.FrmDataExport_Load);
            this.skinTabControl1.ResumeLayout(false);
            this.tbTiming.ResumeLayout(false);
            this.tbTiming.PerformLayout();
            this.tbRegion.ResumeLayout(false);
            this.tbRegion.PerformLayout();
            this.tbSingle.ResumeLayout(false);
            this.tbSingle.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private CCWin.SkinControl.SkinTabControl skinTabControl1;
        private CCWin.SkinControl.SkinTabPage tbTiming;
        private CCWin.SkinControl.SkinTabPage tbRegion;
        private CCWin.SkinControl.SkinTabPage tbSingle;
        private System.Windows.Forms.Label label2;
        private CCWin.SkinControl.SkinRadioButton rbMonth;
        private CCWin.SkinControl.SkinRadioButton rbWeek;
        private CCWin.SkinControl.SkinRadioButton rbDay;
        private CCWin.SkinControl.SkinRadioButton rbClass;
        private System.Windows.Forms.TextBox txtExportPath;
        private System.Windows.Forms.Button btnSelectExportPath;
        private CCWin.SkinControl.SkinButton btnStart;
        private System.Windows.Forms.Label lable1;
        private CCWin.SkinControl.SkinButton btnExport;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtpBegin;
        private System.Windows.Forms.DateTimePicker dtpEnd;
        private System.Windows.Forms.Label lable3;
        private CCWin.SkinControl.SkinButton btnQuery;
        private System.Windows.Forms.TextBox txtSerialNumber;
    }
}