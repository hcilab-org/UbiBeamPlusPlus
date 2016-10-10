using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UbiBeamPlusPlus.Model.Card;
using System.Windows.Media;
using UbiBeamPlusPlus.Exceptions;
using System.Windows;
using System.Windows.Shapes;
using UbiBeamPlusPlus.UI;
using System.Windows.Media.Animation;
using UbiBeamPlusPlus.Input;

namespace UbiBeamPlusPlus.Model {


    /// <summary>
    /// Represents the game field to be displayed
    /// hexagon drawing taken from http://www.mycsharp.de/wbb2/thread.php?postid=118368#post118368
    /// </summary>
    public class Gamefield {

        private MainWindow _window;
        public MainWindow window {
            get { return _window; }
            set { _window = value; }
        }

        private GameInputDetector _gid;

        public GameInputDetector gid {
            get { return _gid; }
            set { _gid = value; }
        }

        // constants for the number of rows and columns
        private const int columns = 3;
        private const int rows = 5;

        public const double GameFieldSideRatio = (13.0 / 40.0);
        public const double CardFieldSideRatio = (5.0 / 40.0);

        public double Width {
            get { return this.window.Width; }
            set { this.window.Width = value; }
        }

        public double Height {
            get { return this.window.Height; }
            set { this.window.Height = value; }
        }

        private Unit[, ,] _Units = new Unit[2, 3, 5];
        public Unit[, ,] Units {
            get { return _Units; }
            set { _Units = value; }
        }

        private Boolean[, ,] _IsSelectAble = new Boolean[2, 3, 5];
        public Boolean[, ,] IsSelectAble {
            get { return _IsSelectAble; }
            set { _IsSelectAble = value; }
        }

        private int[,] _Idols = new int[2, 5] { { 7, 7, 7, 7, 7 }, { 7, 7, 7, 7, 7 } };
        public int[,] Idols {
            get { return _Idols; }
            set { _Idols = value; }
        }

        public Gamefield(MainWindow window) {
            this.window = window;
        }

        public Brush GetPlayerBrush(int Player) {
            if (Player == 0) {
                return new SolidColorBrush(Colors.Red);
            } else if (Player == 1) {
                return new SolidColorBrush(Colors.Blue);
            } else {
                throw new IllegalPlayerIndexException("Player index must be 0 or 1!");
            }
        }

        /// <summary>
        /// Returns the coordinates of a hexagon with the given two-dimensional index.
        /// </summary>
        /// <param name="iX">the index of the hexagon in x</param>
        /// <param name="iY">the index of the hexagon in y</param>
        /// <returns></returns>
        protected Point GetHexagonPoint(int iX, int iY, int xBase, int yBase, int side) {
            if (iX < 0 || iY < 0) {
                throw new IllegalIndexException("Indeces must be 0 or greater!");
            }

            int pixelOffsetX = iX * 2 * (xBase + 2);
            int pixelOffsetY = iY * 3 * (yBase + 1);

            if (iY % 2 == side) {
                // odd rows are indented
                pixelOffsetX += xBase + 2;
            }

            return new Point(pixelOffsetX, pixelOffsetY);
        }

        public Polygon GetHexagon(int iX, int iY, int xBase, int yBase, bool inverted) {
            Polygon hexagon = new Polygon();

            PointCollection points = new PointCollection();

            // find out where to draw the hexagon
            Point pt;
            double offsetX, offsetY;
            if (inverted) {
                pt = GetHexagonPoint(iX, iY, xBase, yBase, 1);
                offsetX = pt.X;
            } else {
                pt = GetHexagonPoint(iX, iY, xBase, yBase, 0);
                offsetX = pt.X;
            }

            // vertically center hexagon grid
            offsetY = pt.Y + (this.Height * 0.75 - 16 * yBase) / 2;

            // create seven points (first one occurrs two times)
            points.Add(new Point(offsetX + 0 * xBase, offsetY + 1 * yBase));
            points.Add(new Point(offsetX + 1 * xBase, offsetY + 0 * yBase));
            points.Add(new Point(offsetX + 2 * xBase, offsetY + 1 * yBase));
            points.Add(new Point(offsetX + 2 * xBase, offsetY + 3 * yBase));
            points.Add(new Point(offsetX + 1 * xBase, offsetY + 4 * yBase));
            points.Add(new Point(offsetX + 0 * xBase, offsetY + 3 * yBase));
            points.Add(new Point(offsetX + 0 * xBase, offsetY + 1 * yBase));

            hexagon.Points = points;

            return hexagon;
        }

        /// <summary>
        /// Move the given Unit to the given Hexfield Position
        /// </summary>
        /// <param name="unit">The Unit which is moved</param>
        /// <param name="PlayerSide"></param>
        /// <param name="XPos"></param>
        /// <param name="YPos"></param>
        public bool MoveUnitTo(Unit pUnit, int PlayerSide, int XPos, int YPos) {
            if (IsSelectAble[PlayerSide, XPos, YPos] && pUnit != null) {
                //remove given Unit from old position
                for (int p = 0; p < 2; p++) {
                    for (int x = 0; x < 3; x++) {
                        for (int y = 0; y < 5; y++) {
                            var currentUnit = Units[p, x, y];
                            if (currentUnit != null && currentUnit.Equals(pUnit)) {
                                Units[p, x, y] = null;
                            }
                        }
                    }
                }
                // set given Unit to the new Position
                Units[PlayerSide, XPos, YPos] = pUnit;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Play the given Card and summoning a unit or enchant a Unit at the given hex-Field on the gamefield
        ///  
        /// </summary>
        /// <param name="pUnit"></param>
        /// <param name="PlayerSide"></param>
        /// <param name="XPos"></param>
        /// <param name="YPos"></param>
        public bool PlayCardtAt(AbstractCard Card, int PlayerSide, int XPos, int YPos) {

            bool successful = false;

            if (IsSelectAble[PlayerSide, XPos, YPos] && Card != null) {
                if (Card is Creature && Unit.IdsLeft()) {
                    //summoning Creature
                    Units[PlayerSide, XPos, YPos] = new Unit((Creature)Card);
                    successful = true;
                } else if (Card is Structure && Unit.IdsLeft()) {
                    //summoning Structure
                    Units[PlayerSide, XPos, YPos] = new Unit((Structure)Card);
                    successful = true;
                } else if (Card is Enchantment) {
                    //Enchant the Unit at the given Hex-Field    
                    Units[PlayerSide, XPos, YPos].Entchantments.Add((Enchantment)Card);
                    successful = true;
                } else if (Card is Spell) {
                    successful = true;
                }
            }
            return successful;
        }

        /// <summary>
        /// Set all Values in IsSelectAble to false
        /// </summary>
        public void resetIsSelectAble() {
            for (int p = 0; p < 2; p++) {
                for (int x = 0; x < 3; x++) {
                    for (int y = 0; y < 5; y++) {
                        IsSelectAble[p, x, y] = false;
                    }
                }
            }

            window.RefreshGameFields();
        }

        /// <summary>
        /// tag all Fields which are selectable for the given Card. Set IsSelectable Values to true if 
        /// the card is selectable to play the given Card.
        /// </summary>
        /// <param name="Card"></param>
        /// <param name="CurrentPlayerIndex"></param>
        public void selectAbleFieldsToPlayCard(AbstractCard Card, int CurrentPlayerIndex) {
            resetIsSelectAble();
            for (int p = 0; p < 2; p++) {
                for (int x = 0; x < 3; x++) {
                    for (int y = 0; y < 5; y++) {
                        Unit unit = Units[p, x, y];
                        if ((Card is Creature || Card is Structure) && unit == null && p == CurrentPlayerIndex) {
                            // all empty fields on the currentPLayers side are selectable
                            IsSelectAble[p, x, y] = true;

                        } else if ((Card is Enchantment || Card is Spell) && unit != null) {
                            // all units are selectable
                            IsSelectAble[p, x, y] = true;
                        }
                    }
                }
            }
            window.RefreshGameFields();
        }

        /// <summary>
        /// Set the IsSelectAble Values of the Neighbours of the given Hex Field to true
        /// </summary>
        /// <param name="PlayerSide"></param>
        /// <param name="XPos"></param>
        /// <param name="YPos"></param>
        public void selectabelFieldsForMoving(int PlayerSide, int XPos, int YPos) {
            // check if values are inbound and if the Unit have enough Movement points
            if (PlayerSide >= 0 && PlayerSide < 2 && XPos >= 0 && XPos < 3 && YPos >= 0 && YPos < 5 && Units[PlayerSide, XPos, YPos].Move > 0) {

                SoundPlayer.getInstance().playClick();

                //left Neighbor
                if (XPos - 1 >= 0 && Units[PlayerSide, XPos - 1, YPos] == null)
                    IsSelectAble[PlayerSide, XPos - 1, YPos] = true;

                //right Neighbor
                if (XPos + 1 < 3 && Units[PlayerSide, XPos + 1, YPos] == null)
                    IsSelectAble[PlayerSide, XPos + 1, YPos] = true;

                if (YPos % 2 == 1 && PlayerSide == 0 || YPos % 2 == 0 && PlayerSide == 1) { // Even Row
                    //Left upper Neighbor
                    if (XPos - 1 >= 0 && YPos - 1 >= 0 && Units[PlayerSide, XPos - 1, YPos - 1] == null)
                        IsSelectAble[PlayerSide, XPos - 1, YPos - 1] = true;

                    //Left upper Neighbor
                    if (YPos - 1 >= 0 && Units[PlayerSide, XPos, YPos - 1] == null)
                        IsSelectAble[PlayerSide, XPos, YPos - 1] = true;

                    //Right lower Neighbor
                    if (XPos - 1 >= 0 && YPos + 1 < 5 && Units[PlayerSide, XPos - 1, YPos + 1] == null)
                        IsSelectAble[PlayerSide, XPos - 1, YPos + 1] = true;

                    // Right lower Neighbor
                    if (YPos + 1 < 5 && Units[PlayerSide, XPos, YPos + 1] == null)
                        IsSelectAble[PlayerSide, XPos, YPos + 1] = true;

                } else { // Odd Row
                    //Left upper Neighbor
                    if (YPos - 1 >= 0 && Units[PlayerSide, XPos, YPos - 1] == null)
                        IsSelectAble[PlayerSide, XPos, YPos - 1] = true;

                    //Right upper Neighbor
                    if (XPos + 1 < 3 && YPos - 1 >= 0 && Units[PlayerSide, XPos + 1, YPos - 1] == null)
                        IsSelectAble[PlayerSide, XPos + 1, YPos - 1] = true;

                    //Left lower Neighbor
                    if (YPos + 1 < 5 && Units[PlayerSide, XPos, YPos + 1] == null)
                        IsSelectAble[PlayerSide, XPos, YPos + 1] = true;

                    // Right lower Neighbor
                    if (XPos + 1 < 3 && YPos + 1 < 5 && Units[PlayerSide, XPos + 1, YPos + 1] == null)
                        IsSelectAble[PlayerSide, XPos + 1, YPos + 1] = true;

                }
            }
        }

        /// <summary>
        /// Reset the Card Highlighting of all CardFields 
        /// </summary>
        public void ResetCardHighlighting() {
            window.ResetCardBorders();
        }

        /// <summary>
        /// toggle highlighting of the Card on the given indey.
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="CardFieldIndex"></param>
        public void HighlightCard(int playerIndex, int CardFieldIndex) {
            window.ToggleCardHighlight(playerIndex, CardFieldIndex);
            Console.WriteLine("Highlighted card " + CardFieldIndex + " of player " + playerIndex);
        }

        public void RefreshGameField() {
            window.Dispatcher.Invoke((Action)(() => {
                window.RefreshGameFields();
            }));
        }

        public void UpdateRessources(int playerIndex, int res) {
            window.updateRessources(playerIndex, res);
        }

        public void HighlightFight(int player, int row, int attackerColumn, int attackedColumn) {
            window.Dispatcher.Invoke((Action)(() => {
                window.HighlightRow(player, row, attackedColumn);
                window.HighlightField((player + 1) % 2, attackerColumn, row);
            }));
        }

        public void stopDetection() {
            if (gid != null) {
                gid.stopDetection();
                gid = null;
            }
        }

        public void ShowEndOfGameMessage(int winner) {
            window.ShowEndOfGameMessage(winner);
        }

        public void DeactivateButtons(int playerIndex) {
            window.DisableButtons(playerIndex);
        }

        public void DectivateSacrifice(int playerIndex) {
            window.DisableSacrificeButtons(playerIndex);
        }

        public void ResetButtons(int playerIndex) {
            window.ResetButtons(playerIndex);
        }
    }
}
