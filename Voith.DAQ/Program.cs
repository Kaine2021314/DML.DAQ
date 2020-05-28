using System;
using System.Windows.Forms;
using Voith.DAQ.Common;
using Voith.DAQ.UI;

namespace Voith.DAQ
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new FrmMain());

            string MutexT = Application.ProductName;
            try
            {
                JsonConfigHelper config = new JsonConfigHelper("Config.json");
                MutexT = config["PlcIpAddress"].ToString();
            }
            catch { }

            bool createNew;
            using (System.Threading.Mutex mutex = new System.Threading.Mutex(true, MutexT, out createNew))
            {
                if (createNew)
                {
                    Application.Run(new FrmMain());
                }
                else
                {
                    MessageBox.Show("程序已启动，请勿重复打开");
                    System.Threading.Thread.Sleep(1000);
                    System.Environment.Exit(1);
                }
            }
        }
    }
}
