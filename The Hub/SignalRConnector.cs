using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace WpfTest
{
    public class SignalRConnector
    {
        public static HubConnection connection;
        public static IHubProxy hubProxy;
        public static string occupyStatus;

        public SignalRConnector()
        {
            connection = new HubConnection("http://myd-vm06527.hpeswlab.net:1234/SignalR"); //http://myd-vm21045.hpeswlab.net:1234/machines
            Helper.nodeInfo = new Dictionary<string, string>();
            hubProxy = connection.CreateHubProxy("MachineCommunicator");

            Helper.nodeInfo.Add("OS_Version", Helper.OS_Version);
            Helper.nodeInfo.Add("OS_SystemType", Helper.OS_SystemType);
            Helper.nodeInfo.Add("OS_MachineName", Helper.OS_MachineName);
            Helper.nodeInfo.Add("OS_build", Helper.OS_build);
            Helper.nodeInfo.Add("OS_Domain", Helper.OS_Domain);

            Helper.nodeInfo.Add("IE_Version", Helper.IE_Version);
            Helper.nodeInfo.Add("Firefox_Version", Helper.Firefox_Version);
            Helper.nodeInfo.Add("Chrome_Version", Helper.Chrome_Version);
            Helper.nodeInfo.Add("UFT_Version", Helper.UFT_Version);

            Helper.nodeInfo.Add("IPAddress", Helper.IPAddress);
            if (Helper.Occupied)
                occupyStatus = " (Locked)";
            else
                occupyStatus = " (Temp Use)";
            Helper.nodeInfo.Add("CurrentUser", Helper.CurrentUser + occupyStatus);
            Helper.nodeInfo.Add("Comment", Helper.Comment);
        }

        public static void UploadNode()
        {
            try
            {
                connection.Start().Wait();
                hubProxy.Invoke("UploadNode", Helper.nodeInfo);
                connection.Stop();
            }
            catch (Exception e)
            { }
        }

        public static void KeepMachineAlive(object sender, EventArgs e)
        {
            try
            {
                connection.Start().Wait();
                hubProxy.Invoke("KeepAlive", Helper.OS_MachineName, Helper.IPAddress);
                connection.Stop();
            }
            catch (Exception e2)
            { }
        }
    }
}
