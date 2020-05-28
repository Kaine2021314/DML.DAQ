namespace Voith.DAQ.UI
{
    partial class DgbImportType
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
            this.rbCover = new CCWin.SkinControl.SkinRadioButton();
            this.rbAddition = new CCWin.SkinControl.SkinRadioButton();
            this.btnCancel = new CCWin.SkinControl.SkinButton();
            this.btnConfirm = new CCWin.SkinControl.SkinButton();
            this.SuspendLayout();
            // 
            // rbCover
            // 
            this.rbCover.AutoSize = true;
            this.rbCover.BackColor = System.Drawing.Color.Transparent;
            this.rbCover.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.rbCover.DownBack = null;
            this.rbCover.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rbCover.Location = new System.Drawing.Point(222, 81);
            this.rbCover.MouseBack = null;
            this.rbCover.Name = "rbCover";
            this.rbCover.NormlBack = null;
            this.rbCover.SelectedDownBack = null;
            this.rbCover.SelectedMouseBack = null;
            this.rbCover.SelectedNormlBack = null;
            this.rbCover.Size = new System.Drawing.Size(107, 28);
            this.rbCover.TabIndex = 1;
            this.rbCover.TabStop = true;
            this.rbCover.Text = "覆盖导入";
            this.rbCover.UseVisualStyleBackColor = false;
            // 
            // rbAddition
            // 
            this.rbAddition.AutoSize = true;
            this.rbAddition.BackColor = System.Drawing.Color.Transparent;
            this.rbAddition.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.rbAddition.DownBack = null;
            this.rbAddition.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rbAddition.Location = new System.Drawing.Point(222, 155);
            this.rbAddition.MouseBack = null;
            this.rbAddition.Name = "rbAddition";
            this.rbAddition.NormlBack = null;
            this.rbAddition.SelectedDownBack = null;
            this.rbAddition.SelectedMouseBack = null;
            this.rbAddition.SelectedNormlBack = null;
            this.rbAddition.Size = new System.Drawing.Size(107, 28);
            this.rbAddition.TabIndex = 1;
            this.rbAddition.TabStop = true;
            this.rbAddition.Text = "增量导入";
            this.rbAddition.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.Transparent;
            this.btnCancel.BaseColor = System.Drawing.Color.Gray;
            this.btnCancel.BorderColor = System.Drawing.Color.DarkGray;
            this.btnCancel.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btnCancel.DownBack = null;
            this.btnCancel.Location = new System.Drawing.Point(309, 266);
            this.btnCancel.MouseBack = null;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.NormlBack = null;
            this.btnCancel.Size = new System.Drawing.Size(97, 40);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // btnConfirm
            // 
            this.btnConfirm.BackColor = System.Drawing.Color.Transparent;
            this.btnConfirm.BaseColor = System.Drawing.Color.Green;
            this.btnConfirm.BorderColor = System.Drawing.Color.DarkGreen;
            this.btnConfirm.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btnConfirm.DownBack = null;
            this.btnConfirm.Location = new System.Drawing.Point(144, 266);
            this.btnConfirm.MouseBack = null;
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.NormlBack = null;
            this.btnConfirm.Size = new System.Drawing.Size(97, 40);
            this.btnConfirm.TabIndex = 4;
            this.btnConfirm.Text = "提交";
            this.btnConfirm.UseVisualStyleBackColor = false;
            this.btnConfirm.Click += new System.EventHandler(this.BtnConfirm_Click);
            // 
            // DgbImportType
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(551, 343);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.rbAddition);
            this.Controls.Add(this.rbCover);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DgbImportType";
            this.ShowBorder = false;
            this.ShowDrawIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "请选择导入方式";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DgbImportType_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private CCWin.SkinControl.SkinRadioButton rbCover;
        private CCWin.SkinControl.SkinRadioButton rbAddition;
        private CCWin.SkinControl.SkinButton btnCancel;
        private CCWin.SkinControl.SkinButton btnConfirm;
    }
}