using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UbiDisplays;
using UbiBeamPlusPlus.UI;
using System.Windows.Controls;
using UbiBeamPlusPlus.Model;

namespace UbiBeamPlusPlus.Input {

    /// <summary>
    /// Detects user input on the game field
    /// </summary>
    public class GameInputDetector {

        private MainWindow parentWindow;
        private Hand detectedHand;
        private Game game;
        private Thread DetectionThread;

        private bool readInput = true;

        protected static double m_FingerX = 0;
        protected static double m_FingerY = 0;

        public GameInputDetector(MainWindow pParent, Hand pHand, Game pGame) {
            this.parentWindow = pParent;
            this.detectedHand = pHand;
            this.game = pGame;

            Console.WriteLine("GameInputDetector created!");

            this.DetectionThread = new Thread(() => this.handleInput());
            this.DetectionThread.Start();
        }

        /// <summary>
        /// Handles Touch and mouse inputs.
        /// </summary>
        public void handleInput() {
            System.Windows.Point position = new System.Windows.Point(m_FingerX, m_FingerY);

            while (readInput) {

                for (int i = 0; i < detectedHand.FingerCount(); i++) {
                    try {
                        parentWindow.Dispatcher.Invoke((Action)(() => {
                            // calculate absolute position
                            position = new System.Windows.Point((detectedHand.GetFinger(0).X / 100.0) * parentWindow.Width,
                                                                (detectedHand.GetFinger(0).Y / 100.0) * parentWindow.Height);

                            // only if position is different
                            if ((Math.Abs(position.X - m_FingerX) > 40 || Math.Abs(position.Y - m_FingerY) > 40)
                                && position.X > 0 && position.Y > 0) {
                                Console.WriteLine("Detected Hand at x:" + position.X + " y:" + position.Y);

                                int player = 0;

                                int cardIndex = -1;
                                Tuple<int, int> hexIndices = new Tuple<int, int>(-1, -1);
                                int buttonIndex = -1;

                                // check for each player
                                while (player < MainWindow.NumberOfPlayers) {

                                    // check card fields
                                    cardIndex = parentWindow.CheckCards(position, player);

                                    if (cardIndex != -1) {
                                        break;
                                    }

                                    // check hexagon grid
                                    hexIndices = parentWindow.CheckHexField(position, player);

                                    if (hexIndices.Item1 != -1 && hexIndices.Item2 != -1) {
                                        break;
                                    }

                                    // TODO checkButtons

                                    buttonIndex = parentWindow.CheckButtons(position, player);

                                    if (buttonIndex != -1) {
                                        break;
                                    }

                                    player++;
                                }

                                if (cardIndex != -1) {
                                    Console.WriteLine("Clicked Card " + (cardIndex + 1) + " of Player " + player);
                                    game.SelectCard(player, cardIndex);
                                } else if (hexIndices.Item1 != -1 && hexIndices.Item2 != -1) {
                                    game.SelectField(player, hexIndices.Item1, hexIndices.Item2);
                                    Console.WriteLine("Hex " + hexIndices.Item1 + "," + hexIndices.Item2 + " of Player" + player);
                                } else if (buttonIndex != -1 && player == game.CurrentPlayerIndex) {
                                    Console.WriteLine("Button " + buttonIndex + " of Player " + player + " pressed!");
                                    switch (buttonIndex) {
                                        case 0:
                                            SoundPlayer.getInstance().playClick();
                                            game.FinishTurn();
                                            readInput = false;
                                            break;
                                        case 1:
                                            game.SacrificeCardForRessource();
                                            break;
                                        case 2:
                                            game.SacrificeCardToDrawCards();
                                            break;
                                    }

                                } else {
                                    Console.WriteLine("Nothing");
                                }

                                // always show detected position
                                parentWindow.moveTouchPos(position.X, position.Y);

                                // save the position
                                m_FingerX = position.X;
                                m_FingerY = position.Y;
                            }

                        }));
                    } catch (TaskCanceledException) {
                        readInput = false;
                    }
                }

            } // end of detection loop

            Console.WriteLine("End detection");
        }

        private bool isInsideCardZone(System.Windows.Point position) {
            // TODO check for different players
            return position.X < game.gamefield.Width * Gamefield.CardFieldSideRatio
                && position.Y > game.gamefield.Height * 0.1 && position.Y < game.gamefield.Height * 0.9;
        }

        /// <summary>
        /// Listener waiting for a completed animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void animation_completed(object sender, EventArgs e) {
            // Wait until thread is finished
            DetectionThread.Join();
            // Reinstantiate thread for restarting it
            DetectionThread = new Thread(() => this.handleInput());
            DetectionThread.Start();
        }

        /// <summary>
        /// Stops the detection thread.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void stopDetection() {
            readInput = false;
        }

    }
}