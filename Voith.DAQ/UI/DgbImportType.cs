using CCWin;
using System;
using System.Windows.Forms;
using Voith.DAQ.Common;
using Voith.DAQ.Services;

namespace Voith.DAQ.UI
{
    /// <summary>
    /// 配方导入类型选择对话框
    /// </summary>
    public partial class DgbImportType : Skin_Color
    {
        private static FormulaImportType _formulaImportType = FormulaImportType.Cancel;
        public DgbImportType()
        {
            InitializeComponent();
        }
        static string _filePath = "";
        static ProgressBarHelper _progressBar;
        public static FormulaImportType Show(string filePath, ProgressBarHelper progressBar)
        {
            DgbImportType dgbImportType = new DgbImportType();
            //{
            //    //lbFormulaPath = {Text = filePath,AutoSize = false,Dock = DockStyle.Fill}
            //};
            
            _filePath = filePath;
            _progressBar = progressBar;

            dgbImportType.ShowDialog();
            return _formulaImportType;
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (rbCover.Checked)
            {
                _formulaImportType = FormulaImportType.Cover;
            }

            if (rbAddition.Checked)
            {
                _formulaImportType = FormulaImportType.Addition;
            }

            AssignFormula af = new AssignFormula(new Model.Workpiece());
            af.FormulaImport(_filePath, _progressBar, _formulaImportType);

            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void DgbImportType_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            var form = (Form)sender;
            var senderButton = form.ActiveControl.Name;
            if (senderButton != "btnConfirm")
            {
                _formulaImportType = FormulaImportType.Cancel;
            }

        }
    }

    /// <summary>
    /// 配方导入类型枚举
    /// </summary>
    public enum FormulaImportType
    {
        /// <summary>
        /// 覆盖导入
        /// </summary>
        Cover,

        /// <summary>
        /// 增量导入
        /// </summary>
        Addition,

        /// <summary>
        /// 取消
        /// </summary>
        Cancel
    }
}
