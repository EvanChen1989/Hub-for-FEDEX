using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Cassia;
using System.Xml;
using System.IO;

namespace WpfTest
{
    public class Helper
    {
        public static string OS_Version;
        public static string OS_SystemType;
        public static string OS_build;
        public static string OS_MachineName;
        public static string OS_Domain;

        public static string UFT_Version;
        public static string IE_Version;
        public static string Chrome_Version;
        public static string Firefox_Version;
        public static string IPAddress;
        public static string CurrentUser = "Nobody";
        public static string Comment = "Comment:";

        public static Dictionary<string, string> nodeInfo;

        public static string OS_Info_String;
        public static string UFT_Info_String;
        public static string Browser_Info_String;

        public static bool Occupied = false;
        public static string UserDisplay = "";


        public static void GetOSInfo()
        {
            RegistryKey OSRegKey = null;
            OSRegKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion");
            OS_Version = (String)OSRegKey.GetValue("ProductName") + " " + (String)OSRegKey.GetValue("CSDVersion");
            
            OS_MachineName = System.Net.Dns.GetHostName();
            OS_build = (String)OSRegKey.GetValue("CurrentBuild");
            System.Net.IPAddress[] addressList = System.Net.Dns.GetHostAddresses(OS_MachineName);
            foreach (IPAddress ip in addressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    IPAddress = ip.ToString();
            }
            if (Environment.Is64BitOperatingSystem)
                OS_SystemType = "64bit";
            else
                OS_SystemType = "32bit";

            OS_Domain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            if (OS_Domain == String.Empty)
                OS_Domain = "N/A";

            OS_Info_String = "OSVersion: " + OS_Version + " " + OS_SystemType + "\n" + "OSbuild: " + OS_build + "\nMachineName: " + OS_MachineName + "\nDomain: " + OS_Domain +"\nIPAddress: " + IPAddress;

            if (Occupied == false)
            {
                TerminalServicesManager sessionManager = new TerminalServicesManager();
                
                if (sessionManager.CurrentSession.ConnectionState.ToString() != "Active")
                    Helper.CurrentUser = "Nobody";
                else
                    Helper.CurrentUser = sessionManager.CurrentSession.ClientName;
            }          
        }

        public static void SaveSessionStatus()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode user, locked, comment;
            if (!File.Exists("PreviousSession.xml"))
            {
                XmlElement root = xmlDoc.CreateElement("PreviousSession");
                xmlDoc.AppendChild(root);
                user = xmlDoc.CreateElement("User");
                root.AppendChild(user);
                locked = xmlDoc.CreateElement("Locked");
                root.AppendChild(locked);
                comment = xmlDoc.CreateElement("Comment");
                root.AppendChild(comment);
            }
            else
            {
                xmlDoc.Load("PreviousSession.xml");
                user = xmlDoc.SelectSingleNode("//User");
                locked = xmlDoc.SelectSingleNode("//Locked");
                comment = xmlDoc.SelectSingleNode("//Comment");
            }
            user.InnerText = CurrentUser;
            locked.InnerText = Occupied.ToString();
            comment.InnerText = Comment;
            xmlDoc.Save("PreviousSession.xml");
        }

        public static void GetUFTVersion()
        {
            try
            {
                RegistryKey regKey;
                regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Mercury Interactive\\QuickTest Professional\\CurrentVersion");
                string UFT_major = regKey.GetValue("DisplayName").ToString();
                string UFT_build = regKey.GetValue("build").ToString();
                UFT_Version = UFT_major + "." + UFT_build;
            }
            catch (Exception)
            {
                UFT_Version = "Not Installed";
            }

            UFT_Info_String = "UFTVersion: " + UFT_Version;
        }

        public static void GetBrowserVersion()
        {
            RegistryKey IERegKey;
            IERegKey = Registry.LocalMachine.OpenSubKey("software\\Microsoft\\Internet Explorer");
            IE_Version = (String)IERegKey.GetValue("svcUpdateVersion");

            try
            {
                RegistryKey ChromeRegKey;
                ChromeRegKey = Registry.CurrentUser.OpenSubKey("software\\Google\\Chrome\\BLBeacon");
                Chrome_Version = (String)ChromeRegKey.GetValue("version");
            }
            catch (Exception)
            {
                Chrome_Version = "Not Installed";
            }

            try
            {
                string key;
                if (OS_SystemType == "32bit")
                    key = "Software\\mozilla.org\\Mozilla";
                else
                    key = "Software\\Wow6432Node\\mozilla.org\\Mozilla";

                RegistryKey FireFoxRegKey;
                FireFoxRegKey = Registry.LocalMachine.OpenSubKey(key);
                Firefox_Version = (String)FireFoxRegKey.GetValue("CurrentVersion");
            }
            catch (Exception)
            {
                Firefox_Version = "Not Installed";
            }

            Browser_Info_String = "IE: " + IE_Version + "\n" + "Chrome: " + Chrome_Version + "\n" + "Firefox: " + Firefox_Version + "\n";
        }

        public static void Set_Bootup()
        {
            string path = System.Windows.Forms.Application.ExecutablePath;
            RegistryKey rk = Registry.LocalMachine;
            RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            rk2.SetValue("HubAutoLaunch", path);
            rk2.Close();
            rk.Close();
        }

        public static void StartTimers()
        {
            System.Windows.Threading.DispatcherTimer keepAliveTimer = new System.Windows.Threading.DispatcherTimer();
            keepAliveTimer.Tick += new System.EventHandler(SignalRConnector.KeepMachineAlive);
            keepAliveTimer.Interval = new System.TimeSpan(0, 15, 0);
            keepAliveTimer.Start();

            System.Windows.Threading.DispatcherTimer scannerTimer = new System.Windows.Threading.DispatcherTimer();
            scannerTimer.Tick += new System.EventHandler(ReScanMachineInfo);
            scannerTimer.Interval = new System.TimeSpan(0, 1, 0);
            scannerTimer.Start();
        }

        public static void ReScanMachineInfo(object obj, EventArgs e)
        {
            GetBrowserVersion();
            GetOSInfo();
            GetUFTVersion();

            foreach (KeyValuePair<string, string> keyPair in Helper.nodeInfo)
            {
                Helper helper = new Helper();
                string currentValue = helper.GetType().GetField(keyPair.Key).GetValue(helper).ToString();
                string previousValue;
                Helper.nodeInfo.TryGetValue(keyPair.Key, out previousValue);
                if (previousValue != currentValue)
                {
                    SignalRConnector connector = new SignalRConnector();
                    SignalRConnector.UploadNode();
                    break;
                }
            }
        }

        public static void LoadPreviousSession()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode user, locked, comment;
            try
            {
                xmlDoc.Load("PreviousSession.xml");
                user = xmlDoc.SelectSingleNode("//User");
                locked = xmlDoc.SelectSingleNode("//Locked");
                comment = xmlDoc.SelectSingleNode("//Comment");

                Helper.CurrentUser = user.InnerText;
                Helper.Occupied = Convert.ToBoolean(locked.InnerText);
                Helper.Comment = comment.InnerText;
            }
            catch (Exception e)
            { };
        }
    }
}
