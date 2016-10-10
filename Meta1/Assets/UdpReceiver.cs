using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UdpReceiver : MonoBehaviour {
    // manages GameObjects
    MarkerManager manager;

    // receiving Thread
    Thread receiveThread;

    // UdpClient object
    UdpClient client;

    //port to receive data
    private int port = 11000;

    private int player;
    private int otherPlayer;

    /*
     * Initialization of the UdpReceiver.
     */
    public void Start() {
        //get instanz from MarkerManager
        manager = GetComponent<MarkerManager>();
        //get players
        if (manager.player1) {
            player = 1;
            otherPlayer = 0;
        } else {
            player = 0;
            otherPlayer = 1;
        }

        //create new thread to receive data
        receiveThread = new Thread(
            new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    /*
     * Receive data with a new thread.
     * 
     * Possible patterns:
     * player1:marker2:card1:2,4,6
     * player1:1,2,3,4,5
     * show1:1,1
     */
    private void ReceiveData() {
        client = new UdpClient();

        // receive data from port
        IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, port);
        client.Client.Bind(groupEP);

        //join multicast group
        IPAddress MulticastAddress = IPAddress.Parse("239.0.0.222");
        client.JoinMulticastGroup(MulticastAddress);

        Debug.Log("Joined multicast group!");

        while (true) {

            try {
                byte[] data = client.Receive(ref groupEP);

                // convert received bytes with UTF8 to a string
                string text = Encoding.UTF8.GetString(data);

                Debug.Log("Received message: " + text);

                if (text.Equals("Hello Network!")) {
                    //start new game
                    manager.startNewGame();
                    
                } else {
                        //split massage at ':'
                        string[] textArray = text.Split(new char[] { ':' });

                        string[] values;

                        switch (textArray.Length) {
                            //player1:1,2,3,4,5
                            // -1 no card at this place
                            //show1:0,1
                            case 2:
                                if (textArray[0].Contains("show")) {
                                    int showCardsFromPlayer = int.Parse(textArray[0].Substring(4));
                                    if (showCardsFromPlayer == 0 || showCardsFromPlayer == 1) {
                                        //get other values, split bei ','
                                        values = textArray[1].Split(new char[] { ',' });

                                        if (values.Length == 2) {
                                            int showPlayerValues = int.Parse(values[0]);
                                            int visibleValues = int.Parse(values[1]);
                                            if ((showPlayerValues == 0 || showPlayerValues == 1) && (visibleValues == 0 || visibleValues == 1)) {
                                                bool visible = false;
                                                if (visibleValues == 1) {
                                                    visible = true;
                                                }
                                                manager.showValues(showCardsFromPlayer, showPlayerValues, visible);
                                            } else {
                                                Debug.Log("No correct message!");
                                            }
                                        } else {
                                            Debug.Log("No correct message!");
                                        }
                                    } else {
                                        Debug.Log("No valid playerID!");
                                    }
                                } else {
                                    //get player id
                                    int playerID = int.Parse(textArray[0].Substring(6));
                                    Debug.Log("Player: " + playerID);

                                    //own cards?
                                    if (player == playerID) {
                                        //get other values, split bei ','
                                        values = textArray[1].Split(new char[] { ',' });

                                        if (values.Length <= manager.getMaxCards()) {
                                            int[] cards = new int[values.Length];

                                            //get all cards
                                            for (int i = 0; i < values.Length; i++) {
                                                cards[i] = int.Parse(values[i]);
                                                Debug.Log("Card: " + cards[i]);
                                            }
                                            //show cards with meta
                                            manager.showCards(cards);
                                        } else {
                                            Debug.Log("No valid number of cards!");
                                        }
                                    } else {
                                        //is playerID valid?
                                        if (playerID == otherPlayer) {
                                            Debug.Log("Cards from other player.");
                                        } else {
                                            Debug.Log("No valid playerID!");
                                        }
                                    }
                                }
                                break;
                            //player1:marker2:card1:2,4,6
                            case 4:
                                //get marker id
                                int markerID = int.Parse(textArray[1].Substring(6));

                                //check if markerID is valid for model
                                if (markerID < 2 || markerID > manager.getMaxMarkerID()) {
                                    Debug.Log("No valid markerID!");
                                    break;
                                }

                                //get player
                                int playerID2 = int.Parse(textArray[0].Substring(6));

                                //get model/card id
                                int cardID = int.Parse(textArray[2].Substring(4));

                                //check if cardID exists
                                if (manager.existsCardIDForModel(cardID)) {

                                    Debug.Log("PlayerID: " + playerID2);
                                    Debug.Log("MarkerID: " + markerID);
                                    Debug.Log("CardID: " + cardID);

                                    //get other values
                                    values = textArray[3].Split(new char[] { ',' });

                                    if (values.Length == 3) {
                                        int[] info = new int[3];
                                        //get all values
                                        for (int i = 0; i < values.Length; i++) {
                                            info[i] = int.Parse(values[i]);
                                            Debug.Log("Info: " + info[i]);
                                        }
                                        //add marker to meta
                                        manager.addModel(playerID2, markerID, cardID, info);
                                    } else {
                                        Debug.Log("No correct message!");
                                    }
                                } else {
                                    Debug.Log("No valid cardID!");
                                }
                                break;
                            default:
                                Debug.Log("No correct message!");
                                break;
                        }
                    }

            } catch (Exception err) {
                print(err.ToString());
            }
        }
    }

    /*
     * Close client and thread.
     */
    void OnDisable() {
        if (receiveThread != null)
            receiveThread.Abort();

        client.Close();
    }
}