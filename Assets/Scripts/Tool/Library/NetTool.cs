using System;
using System.Net;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Net.NetworkInformation;

namespace Kun.Tool
{
    public static class NetTool
    {
        public static string GetLocalIP ()
        {
            var hostName = Dns.GetHostName ();

            var ip = Dns.GetHostEntry (hostName).AddressList.ToList ().Find (ipAddress => ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString ();

            return ip;
        }

        public static string GetMacAddress()
        {
            List<NetworkInterface> nics = NetworkInterface.GetAllNetworkInterfaces().ToList().FindAll(nic => nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet);

            StringBuilder stringBuilder = new StringBuilder();

            if (nics.Count > 0)
            {
                return nics[0].GetPhysicalAddress().ToString();
            }
            else
            {
                LoggerRouter.Error ("不具有網卡");
                return "";
            }
        }

        public static string GetMacAddress(string ip)
        {
            IPAddress targetIp = IPAddress.Parse(ip);

            List<NetworkInterface> nics = NetworkInterface.GetAllNetworkInterfaces().ToList().FindAll(nic => nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet);

            StringBuilder stringBuilder = new StringBuilder();

            if (nics.Count > 0)
            {
                foreach (NetworkInterface nic in nics)
                {
                    IPInterfaceProperties properties = nic.GetIPProperties();

                    foreach (UnicastIPAddressInformation unicastAddress in properties.UnicastAddresses)
                    {
                        if (unicastAddress.Address.Equals(targetIp))
                        {
                            return nic.GetPhysicalAddress().ToString();
                        }
                    }
                }

                LoggerRouter.Error ("找不到該MacAddress");
                return "";
            }
            else
            {
                LoggerRouter.Error ("不具有網卡");
                return "";
            }
        }

        public static bool IsPortInUse (int port)
        {
            var props = IPGlobalProperties.GetIPGlobalProperties ();

            // 獲取當前所有的 TCP 監聽端口
            var tcpListeners = props.GetActiveTcpListeners ();

            // 獲取當前所有的 UDP 監聽端口
            var udpListeners = props.GetActiveUdpListeners ();

            // 檢查 TCP 和 UDP 是否有該端口被占用
            return tcpListeners.Any (l => l.Port == port) || udpListeners.Any (l => l.Port == port);
        }
    }
}
