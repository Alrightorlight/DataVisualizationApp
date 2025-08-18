using System;
using System.Windows.Forms;

namespace DataVisualizationApp
{
    internal static class Program
    {
        ///

        /// 应用程序的主入口点。
        ///

        [STAThread]
        static void Main()
        {
            // 启用高DPI支持
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            // 启用视觉样式
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 设置全局异常处理
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            try
            {
                // 启动主窗体
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"应用程序启动失败：{ex.Message}", "致命错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        ///

        /// 处理UI线程异常
        ///

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show($"应用程序发生错误：{e.Exception.Message}\n\n详细信息：{e.Exception.StackTrace}",
            "程序错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        ///

        /// 处理非UI线程异常
        ///

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show($"应用程序发生未处理的错误：{ex?.Message}\n\n详细信息：{ex?.StackTrace}",
            "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
