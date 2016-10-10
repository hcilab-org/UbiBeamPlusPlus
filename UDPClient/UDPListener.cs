using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDPClient {

    /// <summary>
    /// Simple UDP multicast client for testing purposes. Code was taken from 
    /// https://msdn.microsoft.com/de-de/library/tst0kwb1%28v=vs.110%29.aspx
    /// </summary>
    class UdpListener {

        private const int listenPort = 11000;

        private static void StartListener() {
            bool done = false;

            UdpClient client = new UdpClient();
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);
            client.Client.Bind(groupEP);

            IPAddress MulticastAddress = IPAddress.Parse("239.0.0.222");
            client.JoinMulticastGroup(MulticastAddress);

            Console.WriteLine("Joined multicast group!");
            Console.WriteLine("Waiting for messages...");

            try {
                while (!done) {
                    byte[] receivedBytes = client.Receive(ref groupEP);
                    Console.WriteLine("{0}: {1}", groupEP.ToString(), 
                        Encoding.ASCII.GetString(receivedBytes, 0, receivedBytes.Length));
                }
            } catch(Exception e) {
                Console.WriteLine(e.ToString());
            } finally {
                client.Close();
            }
        }
        
        static void Main(string[] args) {
            StartListener();
        }
    }
}
