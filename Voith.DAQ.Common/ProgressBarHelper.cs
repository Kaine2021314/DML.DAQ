using CCWin.SkinControl;
using System.Windows.Forms;

namespace Voith.DAQ.Common
{
    public class ProgressBarHelper
    {
        private readonly Form _progressBar;
        private readonly Form _ownerForm;

        /// <summary>
        /// 实例化一个遮罩层
        /// </summary>
        /// <param name="owner">遮罩层所属窗体</param>
        /// <param name="showTxt">进度条显示的文字</param>
        public ProgressBarHelper(IWin32Window owner, string showTxt = null)
        {
            _ownerForm = (Form)owner;
            _progressBar = new Form
            {
                Width = _ownerForm.Width,
                Height = _ownerForm.Height,
                Location = _ownerForm.Location,
                FormBorderStyle = FormBorderStyle.None,
                Opacity = 0.65,
                ShowInTaskbar = false
            };
            SkinProgressIndicator progressIndicator = new SkinProgressIndicator();
            progressIndicator.Text = showTxt;
            progressIndicator.ShowText = !string.IsNullOrWhiteSpace(showTxt);
            progressIndicator.Left = _ownerForm.Width / 2 - progressIndicator.Width / 2;
            progressIndicator.Top = _ownerForm.Height / 2 - progressIndicator.Height / 2;
            _progressBar.Controls.Add(progressIndicator);
            progressIndicator.Start();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        /// <summary>
        /// 显示遮罩层
        /// </summary>
        public void Show()
        {
            try
            {
                //_progressBar.Show(_ownerForm);

                //_progressBar.Location = _ownerForm.Location;
            }
            catch { }
        }

        /// <summary>
        /// 关闭遮罩层
        /// </summary>
        public void Close()
        {
            _progressBar.Close();
        }
    }
}
