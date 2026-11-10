using System;
using System.Windows.Forms;
using ProjectTaskManager.GUI;

namespace ProjectTaskManager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FrmMain());
        }
    }
}
