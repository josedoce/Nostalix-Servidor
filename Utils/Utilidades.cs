using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Nostalix_Servidor.Utils
{
    class Utilidades
    {
        //se achar conveniente, mude.
        public static int port = 8082;

        public static string GetFullHost()
        {
            return $"http://{GetLocalIPv4()}:{GetLocalPort()}";
        }
        public static string GetLocalIPv4()
        {
            string localIPv4 = "";

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            localIPv4 = ip.Address.ToString();
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(localIPv4))
                    break;
            }

            return localIPv4;
        }

        internal static int GetLocalPort()
        {
            return port;
        }

    }
}
