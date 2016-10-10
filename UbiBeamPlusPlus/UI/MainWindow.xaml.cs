using UbiBeamPlusPlus;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using UbiDisplays;
using UbiDisplays.Vectors;
using UbiBeamPlusPlus.Core;
using System.Windows.Media.Animation;
using System.Threading;
using System.Runtime.CompilerServices;
using UbiBeamPlusPlus.Exceptions;
using UbiBeamPlusPlus.Input;
using UbiBeamPlusPlus.Model;
using UbiBeamPlusPlus.Network;
using UbiBeamPlusPlus.Model.Card;

namespace UbiBeamPlusPlus.UI {

    /// <summary>
    /// Represents the main window of the game, showing two hexagon grids, card zones, idols, 
    /// player names and life points.
    /// </summary>
    public partial class MainWindow : Window {

        private UbiHand m_Hand;

        public static float m_Width;
        public static float m_Height;

        public static MainWindow m_Instance = null;

        private System.Windows.Point m_StartPoint = new System.Windows.Point(0, 0);

        public const int NumberOfPlayers = 2;
        public const int NumberOfIdols = 5;
        public const int NumberOfCards = 5;
        public const int NumberOfHexRows = 5;
        public const int NumberOfHexColumns = 3;
        public const int MinMarkerIndex = 2;

        public const int BorderThicknessDefault = 1;
        public const int BorderThicknessActive = 4;

        public const int TouchDotSize = 10;

        // Directories
        public const String ResourcesDir = "Resources/";
        public const String LogDir = "logs/";

        private const String PedalLogFileType = "csv";

        private const int IdolXMargin = 5;

        private Game game;

        private double MainRowRatio = 0.75;

        private bool[, ,] isFieldSelected = new bool[NumberOfPlayers, NumberOfHexColumns, NumberOfHexRows];

        // Brushes
        private SolidColorBrush borderBrush;
        private SolidColorBrush borderBrushActive;
        private SolidColorBrush selectedBrush;
        private SolidColorBrush backgroundBrush;
        private SolidColorBrush attackBrush = new SolidColorBrush(Colors.Red);

        // Images
        private BitmapImage[] markerBitmap = new BitmapImage[Unit.NumberOfMarkers];
        private VisualBrush grassBrush = new VisualBrush();
        private ImageBrush[] imgIdol = new ImageBrush[NumberOfPlayers];
        private ImageBrush[] imgDamagedIdol1 = new ImageBrush[NumberOfPlayers];
        private ImageBrush[] imgDamagedIdol2 = new ImageBrush[NumberOfPlayers];
        private ImageBrush[] imgDestroyedIdol = new ImageBrush[NumberOfPlayers];

        // skip turn images
        private ImageBrush[] imgSkip = new ImageBrush[NumberOfPlayers];
        private ImageBrush[] imgSkipDeactivated = new ImageBrush[NumberOfPlayers];

        // card change images
        private ImageBrush[] imgChange = new ImageBrush[NumberOfPlayers];
        private ImageBrush[] imgChangeDeactivated = new ImageBrush[NumberOfPlayers];

        // sacrifice images
        private ImageBrush[] imgSacrifice = new ImageBrush[NumberOfPlayers];
        private ImageBrush[] imgSacrificeDeactivated = new ImageBrush[NumberOfPlayers];

        // GUI Elements
        private StackPanel[] cardZone = new StackPanel[NumberOfPlayers];
        private StackPanel[] idols = new StackPanel[NumberOfPlayers];

        private Polygon[, ,] hexFields = new Polygon[NumberOfPlayers, NumberOfHexColumns, NumberOfHexRows];
        private Rectangle[,] idolRect = new Rectangle[NumberOfPlayers, NumberOfIdols];
        private Rectangle[,] cardRect = new Rectangle[NumberOfPlayers, NumberOfCards];

        // control panel
        private WrapPanel[] gameControls = new WrapPanel[NumberOfPlayers];

        // ingame buttons
        private Rectangle[] sacrificeZone = new Rectangle[NumberOfPlayers];
        private Rectangle[] cardChangeZone = new Rectangle[NumberOfPlayers];
        private Rectangle[] skipTurnZone = new Rectangle[NumberOfPlayers];

        // GUI labels
        private Label[] lblRessources = new Label[NumberOfPlayers];

        // dot showing detected touch position
        private System.Windows.Shapes.Ellipse touchPos;

        private double cardZoneWidth = 0.0;
        private double cardZoneHeight = 0.0;

        /// <summary>
        /// Creates a new main window initializing components, parsing the ".ubi" file and creating
        /// and drawing the game field.
        /// </summary>
        public MainWindow(Game.GameMode mode, String path, float xFactor, float yFactor, Boolean TestMode) {
            InitializeComponent();

            Console.WriteLine("Mode: " + mode + ", Path: " + path + ", x: " + xFactor + ", y: " + yFactor);

            // set margin and size
            this.Top = 0;
            this.Left = 0;
            this.Width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
            this.Height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;

            // configure Ubi Hand ?
            UbiHand.Offset = 0.03f;
            UbiHand.Height = 0.01f;
            UbiHand.Invert = true;

            this.m_Hand = new UbiHand();

            m_Hand.AddMouse(0f, 0f, (float)this.Width, (float)this.Height);

            // Prepare Hand and parse *.ubi file
            if (m_Hand is UbiHand) {
                float xmin = 1000;
                float xmax = -1000;
                float ymin = 1000;
                float ymax = -1000;
                var document = XDocument.Load("Untitled.ubi");
                foreach (var point in document.Root.Element("surface").Element("projector").Elements("point")) {
                    var p = PointFromString(point.Value);
                    if (p.X < xmin)
                        xmin = (float)p.X;
                    if (p.X > xmax)
                        xmax = (float)p.X;
                    if (p.Y < ymin)
                        ymin = (float)p.Y;
                    if (p.Y > ymax)
                        ymax = (float)p.Y;
                }
                // TODO: Parse right display from the *.ubi file
                System.Drawing.Rectangle workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
                m_Width = (xmax - xmin) * workingArea.Width;
                m_Height = (ymax - ymin) * workingArea.Height;
                UbiDisplays.Model.Native.window.innerWidth = (int)m_Width;
                UbiDisplays.Model.Native.window.innerHeight = (int)m_Height;
            } else {
                m_Width = 1024;
                m_Height = 768;

                this.Width = m_Width;
                this.Height = m_Height;
            }

            if (System.Windows.Forms.SystemInformation.MonitorCount > 1) {
                WindowStartupLocation = WindowStartupLocation.Manual;
                // TODO: Parse right display from the *.ubi file
                System.Drawing.Rectangle workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
                Left = workingArea.Left;
                Top = workingArea.Top;
                base.Width = workingArea.Width;
                base.Height = workingArea.Height;
                WindowStyle = WindowStyle.None;
                AllowsTransparency = true;
                Topmost = true;
            }

            Closing += MainWindow_Closing;
            m_Instance = this;

            // create game and gamefield
            game = new Game(new Gamefield(this), m_Hand, mode, path, TestMode);
            String date = DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss");
            System.IO.Directory.CreateDirectory(LogDir);
            PedalLoggerPath = LogDir + "PedalLogger " + date;

            // set up brushes
            borderBrush = new SolidColorBrush(Colors.LightGray);
            borderBrushActive = new SolidColorBrush(Colors.White);
            selectedBrush = new SolidColorBrush(Colors.Orange);
            backgroundBrush = new SolidColorBrush(Colors.Black);

            BitmapImage grassImg = new BitmapImage(new Uri(ResourcesDir + "grass.png", System.UriKind.Relative));
            grassBrush = new VisualBrush(new Image { Source = grassImg });
            grassBrush.Stretch = Stretch.UniformToFill;

            for (int i = 0; i < Unit.NumberOfMarkers; i++) {
                String markerDirectory;
                if (i < 10) {
                    markerDirectory = ResourcesDir + "markers/meta_marker_0" + i + ".png";
                } else {
                    markerDirectory = ResourcesDir + "markers/meta_marker_" + i + ".png";
                }
                markerBitmap[i] = new BitmapImage(new Uri(markerDirectory, System.UriKind.Relative));
            }

            for (int player = 0; player < NumberOfPlayers; player++) {
                BitmapImage idolImage = new BitmapImage(new System.Uri(ResourcesDir + "idol.png", System.UriKind.Relative));
                imgIdol[player] = new ImageBrush(idolImage);
                imgIdol[player].RelativeTransform = (player == 0) ? new RotateTransform(90, 0.5, 0.5) : new RotateTransform(270, 0.5, 0.5);

                // to avoid distortion
                imgIdol[player].Stretch = Stretch.Uniform;

                BitmapImage idolDamagedImage1 = new BitmapImage(new System.Uri(ResourcesDir + "idol_damaged_0.png", System.UriKind.Relative));
                imgDamagedIdol1[player] = new ImageBrush(idolDamagedImage1);
                imgDamagedIdol1[player].RelativeTransform = (player == 0) ? new RotateTransform(90, 0.5, 0.5) : new RotateTransform(270, 0.5, 0.5);
                imgDamagedIdol1[player].Stretch = Stretch.Uniform;

                BitmapImage idolDamagedImage2 = new BitmapImage(new System.Uri(ResourcesDir + "idol_damaged_1.png", System.UriKind.Relative));
                imgDamagedIdol2[player] = new ImageBrush(idolDamagedImage2);
                imgDamagedIdol2[player].RelativeTransform = (player == 0) ? new RotateTransform(90, 0.5, 0.5) : new RotateTransform(270, 0.5, 0.5);
                imgDamagedIdol2[player].Stretch = Stretch.Uniform;

                BitmapImage idolDestroyedImage = new BitmapImage(new System.Uri(ResourcesDir + "idol_destroyed.png", System.UriKind.Relative));
                imgDestroyedIdol[player] = new ImageBrush(idolDestroyedImage);
                imgDestroyedIdol[player].RelativeTransform = (player == 0) ? new RotateTransform(90, 0.5, 0.5) : new RotateTransform(270, 0.5, 0.5);
                imgDestroyedIdol[player].Stretch = Stretch.Uniform;
            }

            // initialize images for buttons
            BitmapImage skipImage = new BitmapImage(new Uri(ResourcesDir + "NextPlayer.png", System.UriKind.Relative));
            BitmapImage skipImageDeactivated = new BitmapImage(new Uri(ResourcesDir + "NextPlayer_grey.png", System.UriKind.Relative));

            for (int player = 0; player < NumberOfPlayers; player++) {
                imgSkip[player] = new ImageBrush(skipImage);
                imgSkip[player].RelativeTransform = (player == 0) ? new RotateTransform(90, 0.5, 0.5) : new RotateTransform(270, 0.5, 0.5);
                imgSkip[player].Stretch = Stretch.Uniform;

                imgSkipDeactivated[player] = new ImageBrush(skipImageDeactivated);
                imgSkipDeactivated[player].RelativeTransform = (player == 0) ? new RotateTransform(90, 0.5, 0.5) : new RotateTransform(270, 0.5, 0.5);
                imgSkipDeactivated[player].Stretch = Stretch.Uniform;
            }

            BitmapImage changeImage = new BitmapImage(new Uri(ResourcesDir + "TwoCards.png", System.UriKind.Relative));
            BitmapImage changeImageDeactivated = new BitmapImage(new Uri(ResourcesDir + "TwoCards_grey.png", System.UriKind.Relative));

            for (int player = 0; player < NumberOfPlayers; player++) {
                imgChange[player] = new ImageBrush(changeImage);
                imgChange[player].RelativeTransform = (player == 0) ? new RotateTransform(90, 0.5, 0.5) : new RotateTransform(270, 0.5, 0.5);
                imgChange[player].Stretch = Stretch.Uniform;

                imgChangeDeactivated[player] = new ImageBrush(changeImageDeactivated);
                imgChangeDeactivated[player].RelativeTransform = (player == 0) ? new RotateTransform(90, 0.5, 0.5) : new RotateTransform(270, 0.5, 0.5);
                imgChangeDeactivated[player].Stretch = Stretch.Uniform;
            }

            BitmapImage sacrificeImage = new BitmapImage(new Uri(ResourcesDir + "Ressource.png", System.UriKind.Relative));
            BitmapImage sacrificeImageDeactivated = new BitmapImage(new Uri(ResourcesDir + "Ressource_grey.png", System.UriKind.Relative));

            for (int player = 0; player < NumberOfPlayers; player++) {
                imgSacrifice[player] = new ImageBrush(sacrificeImage);
                imgSacrifice[player].RelativeTransform = (player == 0) ? new RotateTransform(90, 0.5, 0.5) : new RotateTransform(270, 0.5, 0.5);
                imgSacrifice[player].Stretch = Stretch.Uniform;

                imgSacrificeDeactivated[player] = new ImageBrush(sacrificeImageDeactivated);
                imgSacrificeDeactivated[player].RelativeTransform = (player == 0) ? new RotateTransform(90, 0.5, 0.5) : new RotateTransform(270, 0.5, 0.5);
                imgSacrificeDeactivated[player].Stretch = Stretch.Uniform;
            }

            // initialize marker boolean array, at the beginning no marker is shown
            for (int player = 0; player < NumberOfPlayers; player++) {
                for (int iY = 0; iY < NumberOfHexRows; iY++) {
                    for (int iX = 0; iX < NumberOfHexColumns; iX++) {
                        isFieldSelected[player, iX, iY] = false;
                    }
                }
            }

            Draw();

            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        /// <summary>
        /// This method is called when main window is closed. This also stops the detection thread 
        /// so the program can end properly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (m_Hand is UbiHand) {
                var ubiHand = (UbiHand)m_Hand;
                double min = 10000000;
                double max = -10000000;
                double mean = 0;
                for (int i = 0; i < ubiHand.times.Count; ++i) {
                    double value = ubiHand.times[i];
                    mean += value;
                    if (value < min)
                        min = value;
                    if (value > max)
                        max = value;
                }
                mean /= ubiHand.times.Count;

                string[] lines = new string[ubiHand.times.Count];
                for (int i = 0; i < ubiHand.times.Count; ++i) {
                    lines[i] = ubiHand.times[i].ToString();
                }
            }

            // stop detection thread
            game.gamefield.stopDetection();
        }

        /// <summary>
        /// This method is called after the main window is fully loaded and the gamefield drawn.
        /// Creates a GameInputDetector for this window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {

            game.gamefield.gid = new GameInputDetector(this, m_Hand, game);
        }

        private static System.Windows.Point PointFromString(String sString) {
            // Check we have 3 spaces
            var tCoords = sString.Split(' ');
            if (tCoords.Length == 2) {
                return new System.Windows.Point(float.Parse(tCoords[0]), float.Parse(tCoords[1]));
            }
            throw new Exception("Cannot parse Point from string '" + sString + "'.");
        }

        /// <summary>
        /// This method is called when the user clicks anywhere on the main window. Triggers the 
        /// MouseDown() method of the Hand at the clicked position.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            m_Hand.MouseDown((int)(e.GetPosition(this).X), (int)(e.GetPosition(this).Y));
        }

        /// <summary>
        /// Checks whether a given MouseButtonEvent is inside one of the card fields, and prints 
        /// out the card and player indices.
        /// </summary>
        public int CheckCards(Point p, int player) {
            Point relativePos = TranslatePoint(p, cardZone[player]);

            double CardZoneWidth = cardZone[player].ActualWidth;
            double CardZoneHeight = cardZone[player].ActualHeight;

            if (relativePos.X >= 0 && relativePos.Y >= 0
                && relativePos.X < CardZoneWidth && relativePos.Y < CardZoneHeight) {
                return (int)relativePos.Y / ((int)CardZoneHeight / NumberOfCards);
            }

            return -1;
        }

        private Boolean IsPressed_D1 = false;
        private Boolean IsPressed_D2 = false;
        private Boolean IsPressed_D3 = false;
        private Boolean IsPressed_D4 = false;

        private String PedalLoggerPath;

        /// <summary>
        /// This method is called when the user Press a Key. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key.Equals(Key.D1) && !IsPressed_D1) { // show Unit info from Player 0
                IsPressed_D1 = true;
                UdpSender.GetInstance().setUnitInfoVisible(0, game.CurrentGameMode, true);
                PedalLogger(Key.D1, 1);

            } else if (e.Key.Equals(Key.D2) && !IsPressed_D2) { // show Unit info from Player 1
                IsPressed_D2 = true;
                UdpSender.GetInstance().setUnitInfoVisible(1, game.CurrentGameMode, true);
                PedalLogger(Key.D2, 1);

            } else if (e.Key.Equals(Key.D3) && !IsPressed_D3) { // Change CardPage of player 0
                IsPressed_D3 = true;
                game.changeCardPage(0);

            } else if (e.Key.Equals(Key.D4) && !IsPressed_D4) { // Change CardPage of player 1
                IsPressed_D4 = true;
                game.changeCardPage(1);

            } else if (e.Key.Equals(Key.Escape) && game.CurrentState == Game.GameCycle.GameOver) {
                //Close Application if Game is Over
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// This method is called when the user release a Key.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key.Equals(Key.D1) && IsPressed_D1) {
                IsPressed_D1 = false;
                UdpSender.GetInstance().setUnitInfoVisible(0, game.CurrentGameMode, false);
                PedalLogger(Key.D1, 0);

            } else if (e.Key.Equals(Key.D2) && IsPressed_D2) {
                IsPressed_D2 = false;
                UdpSender.GetInstance().setUnitInfoVisible(1, game.CurrentGameMode, false);
                PedalLogger(Key.D2, 0);

            } else if (e.Key.Equals(Key.D3) && IsPressed_D3) {
                IsPressed_D3 = false;

            } else if (e.Key.Equals(Key.D4) && IsPressed_D4) {
                IsPressed_D4 = false;
            }     
        }

        /// <summary>
        /// Save Key Action with a Time Stamp in a log File
        /// </summary>
        /// <param name="key"></param>
        /// <param name="KeyAction"> 1 when KeyDown, 0 when KeyUp</param>
        private void PedalLogger(Key key, int KeyAction){
            try {
                String path = PedalLoggerPath + "_"  + key.ToString() + "." + PedalLogFileType;
                System.IO.File.AppendAllText(path, KeyAction + ";" + DateTime.Now.ToString("HH:mm:ss:fff")  + Environment.NewLine);
            } catch (Exception e) {
                Console.WriteLine("Error while writing Pedal Log");
                Console.WriteLine(e.StackTrace);
            }
        }

        /// <summary>
        /// Returns the indices of a hex field matching the given point.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public Tuple<int, int> CheckHexField(Point p, int player) {

            IInputElement hit = this.InputHitTest(p);

            for (int iY = 0; iY < NumberOfHexRows; iY++) {
                for (int iX = 0; iX < NumberOfHexColumns; iX++) {
                    Polygon currentHex = hexFields[player, iX, iY];

                    // TODO use a real hexagon check
                    if (currentHex.Equals(hit)) {
                        return new Tuple<int, int>(iX, iY);
                    }
                }
            }

            return new Tuple<int, int>(-1, -1);
        }

        /// <summary>
        /// Check whether given position is over one of the three buttons of the also given player.
        /// </summary>
        /// <param name="p">the position to check</param>
        /// <param name="player">the player for which buttons should be checked</param>
        /// <returns></returns>
        public int CheckButtons(Point p, int player) {
            IInputElement hit = this.InputHitTest(p);

            if (skipTurnZone[player].Equals(hit)) {
                return 0;
            } else if (sacrificeZone[player].Equals(hit)) {
                return 1;
            } else if (cardChangeZone[player].Equals(hit)) {
                return 2;
            } else {
                return -1;
            }
        }

        /// <summary>
        /// This method is called when the user resizes the main window. Redraws the whole game 
        /// field to match the new window size.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateSize(object sender, RoutedEventArgs e) {
            game.gamefield.Width = this.Width;
            game.gamefield.Height = this.Height;
            LayoutRoot.Children.Clear();
            Draw();
        }

        /// <summary>
        /// Draws game fields, card fields and prints out player names and life points.
        /// </summary>
        private void Draw() {
            Console.WriteLine("Width: " + this.Width + ", Height: " + this.Height);

            // compute the base lengths for the hexagons
            int xBase = (int)((this.Width * Gamefield.GameFieldSideRatio / 4.0) - (BorderThicknessActive));
            int yBase = (int)(((this.Height * MainRowRatio) / 15.0) - (BorderThicknessActive));

            Console.WriteLine("xBase: " + xBase + ", yBase: " + yBase);

            // ensure right aspect ratio for hexagons
            xBase = Math.Min(xBase, (int)(yBase * (7f / 4f)));
            yBase = Math.Min(yBase, (int)(xBase * (4f / 7f)));

            Console.WriteLine("xBase: " + xBase + ", yBase: " + yBase);

            LayoutRoot.Background = backgroundBrush;

            // draw game fields for player 0 and 1
            DrawGameFieldForPlayer(0, xBase, yBase);
            DrawGameFieldForPlayer(1, xBase, yBase);

            cardZoneWidth = this.Width * Gamefield.CardFieldSideRatio - 2;
            cardZoneHeight = this.Height * MainRowRatio;

            DrawCardFieldForPlayer(0, cardZoneWidth, cardZoneHeight);
            DrawCardFieldForPlayer(1, cardZoneWidth, cardZoneHeight);

            DrawIdolsForPlayer(0, xBase, yBase);
            DrawIdolsForPlayer(1, xBase, yBase);

            DrawButtons();

            drawTouchDot();
        }
        /// <summary>
        /// Draws a touch dot on the screen.
        /// </summary>
        private void drawTouchDot() {
            Canvas touchCanvas = new Canvas();

            touchPos = new System.Windows.Shapes.Ellipse();
            touchCanvas.Children.Add(touchPos);
            Canvas.SetLeft(touchPos, 0);
            Canvas.SetTop(touchPos, 0);

            touchPos.Width = TouchDotSize;
            touchPos.Height = TouchDotSize;
            touchPos.Fill = new SolidColorBrush(Colors.Lime);

            LayoutRoot.Children.Add(touchCanvas);
        }

        /// <summary>
        /// Moves the touch dot to the given position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void moveTouchPos(double x, double y) {
            Canvas.SetLeft(touchPos, x - (TouchDotSize / 2));
            Canvas.SetTop(touchPos, y - (TouchDotSize / 2));
        }

        /// <summary>
        /// Draws the hexagon grid for the given player with also given base lengths in x and y 
        /// directions.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="xBase"></param>
        /// <param name="yBase"></param>
        private void DrawGameFieldForPlayer(int player, int xBase, int yBase) {
            if (player > 1 || player < 0) {
                throw new IllegalPlayerIndexException("Player must be 0 or 1!");
            }

            for (int iY = 0; iY < 5; iY++) {
                for (int iX = 0; iX < 3; iX++) {

                    if (player == 0) {
                        hexFields[player, iX, iY] = game.gamefield.GetHexagon(iX, iY, xBase, yBase, false);

                        Grid.SetRow(hexFields[player, iX, iY], 1);
                        Grid.SetColumn(hexFields[player, iX, iY], 2);
                    } else {
                        hexFields[player, iX, iY] = game.gamefield.GetHexagon(iX, iY, xBase, yBase, true);

                        Grid.SetRow(hexFields[player, iX, iY], 1);
                        Grid.SetColumn(hexFields[player, iX, iY], 3);
                    }

                    if (game.gamefield.IsSelectAble[player, iX, iY]) {
                        // set selectable border
                        hexFields[player, iX, iY].StrokeThickness = BorderThicknessActive;
                        hexFields[player, iX, iY].Stroke = selectedBrush;
                    } else {
                        // set up border
                        if (player == game.CurrentPlayerIndex) {
                            hexFields[player, iX, iY].StrokeThickness = BorderThicknessActive;
                            hexFields[player, iX, iY].Stroke = borderBrushActive;
                        } else {
                            hexFields[player, iX, iY].StrokeThickness = BorderThicknessDefault;
                            hexFields[player, iX, iY].Stroke = borderBrush;
                        }
                    }

                    // show markers where units are set.
                    Unit currentUnit = game.gamefield.Units[player, iX, iY];
                    if (currentUnit != null) {
                        hexFields[player, iX, iY].Fill = GetMarkerBrush(currentUnit.MarkerID, player);
                    } else {
                        hexFields[player, iX, iY].Fill = grassBrush;
                    }

                    // add hexagon to UI
                    LayoutRoot.Children.Add(hexFields[player, iX, iY]);
                }
            }
        }

        /// <summary>
        /// Draws the card field for a given player.
        /// </summary>
        /// <param name="player">the index of the player whose card field should be drawn</param>
        /// <param name="width">the width of the card field</param>
        /// <param name="height">the height of the card field</param>
        private void DrawCardFieldForPlayer(int player, double width, double height) {
            if (player < 0 || player > 2) {
                throw new IllegalPlayerIndexException("Player index must be 0 or 1!");
            }

            cardZone[player] = new StackPanel();
            Grid.SetRow(cardZone[player], 1);

            if (player == 0) {
                Grid.SetColumn(cardZone[player], 0);
            } else {
                Grid.SetColumn(cardZone[player], 5);
            }

            double rectHeight = height / NumberOfCards;

            for (int i = 0; i < NumberOfCards; i++) {
                cardRect[player, i] = new Rectangle();

                // set up size
                cardRect[player, i].Width = width;
                cardRect[player, i].Height = height / NumberOfCards;

                Grid.SetRow(cardRect[player, i], 1);

                cardRect[player, i].Stroke = borderBrush;

                if (i == 2) {
                    cardRect[player, i].Fill = GetMarkerBrush(player, player);
                }

                // add card to cardzone
                cardZone[player].Children.Add(cardRect[player, i]);
            }

            LayoutRoot.Children.Add(cardZone[player]);
        }

        /// <summary>
        /// Prints out the given amount of ressources for an also given player.
        /// </summary>
        /// <param name="player">the index of the player whose ressources should be printed</param>
        /// <param name="ressources">the ressources to be printed</param>
        private void PrintRessources(int player, int ressources) {
            if (player < 0 || player > 1) {
                throw new IllegalPlayerIndexException("Player index must be 0 or 1!");
            }

            lblRessources[player] = new Label();

            lblRessources[player].Width = this.Width * Gamefield.CardFieldSideRatio;
            lblRessources[player].Height = this.Height * 0.1;

            lblRessources[player].Content = ressources;
            lblRessources[player].Foreground = borderBrush;
            lblRessources[player].Background = backgroundBrush;
            lblRessources[player].FontSize = 50;

            if (player == 0) {
                lblRessources[player].HorizontalAlignment = HorizontalAlignment.Right;
                lblRessources[player].VerticalAlignment = VerticalAlignment.Top;
                lblRessources[player].LayoutTransform = new RotateTransform(90);
                Grid.SetRow(lblRessources[player], 0);
                Grid.SetColumn(lblRessources[player], 0);
            } else {
                lblRessources[player].HorizontalAlignment = HorizontalAlignment.Left;
                lblRessources[player].VerticalAlignment = VerticalAlignment.Bottom;
                lblRessources[player].LayoutTransform = new RotateTransform(270);
                Grid.SetRow(lblRessources[player], 2);
                Grid.SetColumn(lblRessources[player], 5);
            }

            gameControls[player].Children.Add(lblRessources[player]);
        }

        /// <summary>
        /// Updates the ressources to be shown for the given player.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="ressources"></param>
        public void updateRessources(int player, int ressources) {
            // avoid null pointer when called before initialization
            if (lblRessources[player] != null) {
                lblRessources[player].Content = ressources;
            }
        }

        private double GetButtonSize() {
            return Math.Min((this.Height * 0.1), ((2.0 / 40.0) * Width) - (2 * BorderThicknessDefault));
        }

        public void DrawButtons() {
            for (int player = 0; player < NumberOfPlayers; player++) {
                gameControls[player] = new WrapPanel();

                if (player == 0) {
                    Grid.SetColumn(gameControls[player], 0);
                    Grid.SetRow(gameControls[player], 0);
                } else {
                    Grid.SetColumn(gameControls[player], 5);
                    Grid.SetRow(gameControls[player], 2);
                }
                LayoutRoot.Children.Add(gameControls[player]);

                // order depends on player
                if (player == 0) {
                    drawCardChangeZone(player);
                    DrawSacrificeZone(player);
                    drawSkipTurnZone(player);
                    PrintRessources(player, 0);
                } else {
                    DrawSacrificeZone(player);
                    drawCardChangeZone(player);
                    PrintRessources(player, 0);
                    drawSkipTurnZone(player);
                }

            }
        }

        private void DrawSacrificeZone(int playerIndex) {
            sacrificeZone[playerIndex] = new Rectangle();

            double size = GetButtonSize();

            sacrificeZone[playerIndex].Width = size;
            sacrificeZone[playerIndex].Height = size;

            sacrificeZone[playerIndex].Fill = imgSacrifice[playerIndex];
            sacrificeZone[playerIndex].Stroke = borderBrush;

            int row = (playerIndex == 0) ? 2 : 0;
            int column = (playerIndex == 0) ? 1 : 4;

            Grid.SetRow(sacrificeZone[playerIndex], row);
            Grid.SetColumn(sacrificeZone[playerIndex], column);

            LayoutRoot.Children.Add(sacrificeZone[playerIndex]);
        }

        private void drawCardChangeZone(int playerIndex) {
            cardChangeZone[playerIndex] = new Rectangle();

            double size = GetButtonSize();

            cardChangeZone[playerIndex].Width = size;
            cardChangeZone[playerIndex].Height = size;

            cardChangeZone[playerIndex].Fill = imgChange[playerIndex];
            cardChangeZone[playerIndex].Stroke = borderBrush;

            int row = (playerIndex == 0) ? 2 : 0;
            int column = (playerIndex == 0) ? 0 : 5;

            Grid.SetRow(cardChangeZone[playerIndex], row);
            Grid.SetColumn(cardChangeZone[playerIndex], column);

            LayoutRoot.Children.Add(cardChangeZone[playerIndex]);
        }

        private void drawSkipTurnZone(int playerIndex) {
            skipTurnZone[playerIndex] = new Rectangle();

            double size = GetButtonSize();

            skipTurnZone[playerIndex].Width = size;
            skipTurnZone[playerIndex].Height = size;

            skipTurnZone[playerIndex].Fill = imgSkip[playerIndex];
            skipTurnZone[playerIndex].Stroke = borderBrush;

            gameControls[playerIndex].Children.Add(skipTurnZone[playerIndex]);
        }

        /// <summary>
        /// Draws the idols of a given player on the game field.
        /// </summary>
        /// <param name="Player">the index of the player whose idols should be printed</param>
        /// <param name="xBase">the base length in x direction</param>
        /// <param name="yBase">the base length in y direction</param>
        private void DrawIdolsForPlayer(int Player, double xBase, double yBase) {
            // create a stack panel for the idols
            idols[Player] = new StackPanel();

            Grid.SetRow(idols[Player], 1);

            if (Player == 0) {
                Grid.SetColumn(idols[Player], 1);
                idols[Player].HorizontalAlignment = HorizontalAlignment.Left;
            } else {
                Grid.SetColumn(idols[Player], 4);
                idols[Player].HorizontalAlignment = HorizontalAlignment.Right;
            }

            double IdolWidth = xBase - (2 * IdolXMargin);
            double IdolHeight = IdolWidth;
            double IdolYMargin = ((this.Height * MainRowRatio / NumberOfIdols) - IdolHeight) / 2.0;

            Console.WriteLine("Idol y margin: " + IdolYMargin);

            for (int i = 0; i < NumberOfIdols; i++) {

                Rectangle currentIdol = new Rectangle();
                idolRect[Player, i] = currentIdol;

                currentIdol.Width = IdolWidth;
                currentIdol.Height = IdolHeight;

                currentIdol.Margin = new Thickness(IdolXMargin, IdolYMargin, IdolXMargin, IdolYMargin);

                currentIdol.Stroke = borderBrush;

                currentIdol.Fill = getIdolImage(Player, i);
                idols[Player].Children.Add(currentIdol);
            }

            LayoutRoot.Children.Add(idols[Player]);
        }

        public void RefreshGameFields() {
            for (int player = 0; player < NumberOfPlayers; player++) {
                for (int iX = 0; iX < NumberOfHexColumns; iX++) {
                    for (int iY = 0; iY < NumberOfHexRows; iY++) {
                        // show markers where units are set.
                        Unit currentUnit = game.gamefield.Units[player, iX, iY];
                        if (currentUnit != null) {
                            hexFields[player, iX, iY].Fill = GetMarkerBrush(currentUnit.MarkerID, player);
                        } else {
                            hexFields[player, iX, iY].Fill = grassBrush;
                        }

                        // highlight selectable fields using different Border
                        if (game.gamefield.IsSelectAble[player, iX, iY]) {
                            hexFields[player, iX, iY].StrokeThickness = BorderThicknessActive;
                            hexFields[player, iX, iY].Stroke = selectedBrush;
                        } else {

                            if (player == game.CurrentPlayerIndex) {
                                hexFields[player, iX, iY].StrokeThickness = BorderThicknessActive;
                                hexFields[player, iX, iY].Stroke = borderBrushActive;
                            } else {
                                hexFields[player, iX, iY].StrokeThickness = BorderThicknessDefault;
                                hexFields[player, iX, iY].Stroke = borderBrush;
                            }
                        }
                    }
                }
                // refresh idols
                for (int i = 0; i < NumberOfIdols; i++) {
                    idolRect[0, i].Fill = getIdolImage(0, i);
                    idolRect[1, i].Fill = getIdolImage(1, i);
                }
            }
        }

        private ImageBrush getIdolImage(int PlayerIndex, int i) {
            if (game.gamefield.Idols[PlayerIndex, i] > 5) {
                return imgIdol[PlayerIndex];
            } else if (game.gamefield.Idols[PlayerIndex, i] > 3) {
                return imgDamagedIdol1[PlayerIndex];
            } else if (game.gamefield.Idols[PlayerIndex, i] > 0) {
                return imgDamagedIdol2[PlayerIndex];
            }else{
                return imgDestroyedIdol[PlayerIndex];
            }
        }

        public void HighlightRow(int player, int row, int column) {
            if (player == 0) {
                for (int iX = 2; iX >= column; iX--) {
                    hexFields[player, iX, row].StrokeThickness = BorderThicknessActive;
                    hexFields[player, iX, row].Fill = attackBrush;
                }
            } else {
                for (int iX = 0; iX <= column; iX++) {
                    hexFields[player, iX, row].StrokeThickness = BorderThicknessActive;
                    hexFields[player, iX, row].Fill = attackBrush;
                }

            }
        }

        public void HighlightField(int player, int x, int y) {
            hexFields[player, x, y].StrokeThickness = BorderThicknessActive;
            hexFields[player, x, y].Stroke = attackBrush;
        }

        /// <summary>
        /// Highlights card of given player with given index by changing its border.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="CardFieldIndex"></param>
        public void ToggleCardHighlight(int player, int CardFieldIndex) {
            // unselect all other cards
            ResetCardBorders();

            if (game.IsCardSelected(player, CardFieldIndex)) {
                cardRect[player, CardFieldIndex].StrokeThickness = BorderThicknessDefault;
                cardRect[player, CardFieldIndex].Stroke = borderBrush;
            } else {
                cardRect[player, CardFieldIndex].StrokeThickness = BorderThicknessActive;
                cardRect[player, CardFieldIndex].Stroke = game.gamefield.GetPlayerBrush(player);
            }
        }

        /// <summary>
        /// Reset the Card Highlighting of all CardFields 
        /// </summary>
        public void ResetCardBorders() {
            for (int player = 0; player < NumberOfPlayers; player++) {
                for (int card = 0; card < NumberOfCards; card++) {
                    cardRect[player, card].StrokeThickness = BorderThicknessDefault;
                    cardRect[player, card].Stroke = borderBrush;
                }
            }
        }

        public VisualBrush GetMarkerBrush(int markerId, int playerIndex) {
            if (markerId > Unit.NumberOfMarkers) {
                throw new IllegalIndexException("marker id must be between 0 and" + (Unit.NumberOfMarkers - 1) + " !");
            }

            VisualBrush markerBrush = null;
            BitmapImage bimg = markerBitmap[markerId];

            if (markerId < 2) {
                markerBrush = (markerId == 0) ? GetRotatedBrush(bimg, 270) : GetRotatedBrush(bimg, 90);

                markerBrush.Stretch = Stretch.Uniform;
            } else {
                if (playerIndex == 0) {
                    markerBrush = GetRotatedBrush(bimg, 90);
                } else {
                    markerBrush = GetRotatedBrush(bimg, 270);
                }
                markerBrush.Stretch = Stretch.UniformToFill;
            }

            return markerBrush;

        }

        public VisualBrush GetRotatedBrush(BitmapImage bimg, int angle) {
            var rotatedImage = new TransformedBitmap(bimg, new RotateTransform(angle));
            var image = new Image { Source = rotatedImage };

            RenderOptions.SetCachingHint(image, CachingHint.Cache);
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
            return new VisualBrush(image);
        }

        public void ShowEndOfGameMessage(int winner) {
            Label[] lblEndOfGame = new Label[NumberOfPlayers];

            for (int i = 0; i < NumberOfPlayers; i++) {
                lblEndOfGame[i] = new Label();

                // set up size
                lblEndOfGame[i].Width = cardZoneHeight - (BorderThicknessDefault * 2) - 40;
                lblEndOfGame[i].Height = cardZoneWidth - (BorderThicknessDefault * 2);

                Console.WriteLine("Height: " + cardZoneHeight);

                // set up colors
                lblEndOfGame[i].Background = new SolidColorBrush(Colors.Black);
                lblEndOfGame[i].Foreground = new SolidColorBrush(Colors.White);

                lblEndOfGame[i].FontSize = 100;
                lblEndOfGame[i].HorizontalContentAlignment = HorizontalAlignment.Center;

                if (i == 0) {
                    Grid.SetColumn(lblEndOfGame[i], 0);
                    lblEndOfGame[i].LayoutTransform = new RotateTransform(90);
                } else {
                    Grid.SetColumn(lblEndOfGame[i], 5);
                    lblEndOfGame[i].LayoutTransform = new RotateTransform(270);
                }

                if (i == winner) {
                    lblEndOfGame[i].Content = "Winner!";
                } else {
                    lblEndOfGame[i].Content = "Loser!";
                }

                Grid.SetRow(lblEndOfGame[i], 1);
                LayoutRoot.Children.Add(lblEndOfGame[i]);
            }
        }

        public void DisableSacrificeButtons(int playerIndex) {
            sacrificeZone[playerIndex].Fill  = imgSacrificeDeactivated[playerIndex];
            cardChangeZone[playerIndex].Fill  = imgChangeDeactivated[playerIndex];
        }

        public void DisableButtons(int playerIndex) {
            skipTurnZone[playerIndex].Fill = imgSkipDeactivated[playerIndex];
            DisableSacrificeButtons(playerIndex);
        }

        public void ResetButtons(int playerIndex) {
            skipTurnZone[playerIndex].Fill = imgSkip[playerIndex];
            sacrificeZone[playerIndex].Fill = imgSacrifice[playerIndex];
            cardChangeZone[playerIndex].Fill = imgChange[playerIndex];
        }

    }
}
