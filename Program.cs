using System;
using System.Windows.Forms;

namespace DataVisualizationApp
{
    internal static class Program
    {
        ///

        /// Ӧ�ó��������ڵ㡣
        ///

        [STAThread]
        static void Main()
        {
            // ���ø�DPI֧��
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            // �����Ӿ���ʽ
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ����ȫ���쳣����
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            try
            {
                // ����������
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ӧ�ó�������ʧ�ܣ�{ex.Message}", "��������",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        ///

        /// ����UI�߳��쳣
        ///

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show($"Ӧ�ó���������{e.Exception.Message}\n\n��ϸ��Ϣ��{e.Exception.StackTrace}",
            "�������", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        ///

        /// ������UI�߳��쳣
        ///

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception? ex = e.ExceptionObject as Exception;
            MessageBox.Show($"Ӧ�ó�����δ�����Ĵ���{ex?.Message}\n\n��ϸ��Ϣ��{ex?.StackTrace}",
            "���ش���", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
