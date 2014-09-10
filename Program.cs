using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DNSAera
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
        static void OnProcessExit(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
