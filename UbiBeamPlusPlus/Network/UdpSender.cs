using UbiBeamPlusPlus.Model;
using UbiBeamPlusPlus.Model.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UbiBeamPlusPlus.Network {

    /// <summary>
    /// Sends out multicast messages using the UDP protocoll.
    /// </summary>
    class UdpSender {

        private const int DefaultPortNumber = 11000;

        private const String Separator = ":";

        private int PortNumber;

        private static UdpSender sender;

        public static UdpSender GetInstance() {
            if (sender == null) {
                sender = new UdpSender();
            }

            return sender;
        }

        /// <summary>
        /// Creates a new UDPSender using the default port.
        /// </summary>
        private UdpSender()
            : this(DefaultPortNumber) {
        }

        /// <summary>
        /// Creates a new UDPSender using the given port number.
        /// </summary>
        /// <param name="pPortNumber">the port number to use</param>
        private UdpSender(int pPortNumber) {
            this.PortNumber = pPortNumber;
        }

        public void sendMessage(String messageToSend) {
            UdpClient udpClient = new UdpClient();
            Console.WriteLine("UDP Client created!");

            IPAddress MulticastAddress = IPAddress.Parse("239.0.0.222");
            udpClient.JoinMulticastGroup(MulticastAddress);
            Console.WriteLine("Joined multicast group.");

            IPEndPoint ep = new IPEndPoint(MulticastAddress, PortNumber);

            byte[] sendBuffer = Encoding.ASCII.GetBytes(messageToSend);
            udpClient.Send(sendBuffer, sendBuffer.Length, ep);
            Console.WriteLine("Sent message to multicast group.");
        }

        public void SendCards(int playerIndex, List<AbstractCard> cardsToSend) {

            StringBuilder message = new StringBuilder();
            message.Append("player" + playerIndex + ":");

            bool first = true;

            for (int i = 0; i < 5; i++) {
                if (first) {
                    first = false;
                } else {
                    message.Append(",");
                }
                if (i < cardsToSend.Count) {
                    message.Append(cardsToSend[i].CardID);
                } else {
                    message.Append(-1);
                }
            }

            sendMessage(message.ToString());
        }

        /// <summary>
        /// Sends out the new units marker and card id and its attributes via network.
        /// </summary>
        /// <param name="newUnit"></param>
        /// <param name="PlayerIndex"></param>
        public void SendUnit(Unit newUnit, int PlayerIndex) {
            StringBuilder message = new StringBuilder();

            message.Append("player" + PlayerIndex);
            message.Append(Separator);
            message.Append("marker" + newUnit.MarkerID);
            message.Append(Separator);
            message.Append("card" + newUnit.Card.CardID);
            message.Append(Separator);
            message.Append(newUnit.GetAttributeString());

            sendMessage(message.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="PlayerIndex"></param>
        /// <param name="Mode"></param>
        /// <param name="IsVisible"></param>
        public void setUnitInfoVisible(int PlayerIndex, Game.GameMode Mode, Boolean IsVisible ) {
            String message = "show"+PlayerIndex+":";
            int visible = IsVisible ? 1:0;
            switch (Mode) {
                case Game.GameMode.Private:
                    message += "0," + visible;
                    break;
                case Game.GameMode.Public:
                    message += "1," + visible;
                    break;
            }
            sendMessage(message);
        }

    }
}
