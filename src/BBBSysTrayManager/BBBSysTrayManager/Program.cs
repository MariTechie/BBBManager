using System;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace BBBSysTrayManager
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var me = Process.GetCurrentProcess();
            var otherMe = Process.GetProcessesByName(me.ProcessName).Where(p => p.Id != me.Id).FirstOrDefault();

            if (otherMe == null)
            {
                Application.Run(new SysTray());
            }
            else
            {
                me.Kill();
            }
        }
    }
}
