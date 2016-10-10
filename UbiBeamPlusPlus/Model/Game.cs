using UbiBeamPlusPlus.Model.Card;
using UbiBeamPlusPlus.Network;
using UbiBeamPlusPlus.Model.Component.Ability;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UbiBeamPlusPlus.Model.Component.Attack;
using System.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;
using UbiBeamPlusPlus.Input;
using UbiDisplays;
using UbiBeamPlusPlus.UI;

namespace UbiBeamPlusPlus.Model {
    /// <summary>
    /// 
    /// </summary>
    public class Game {

        public enum GameCycle { Menu, BeginTurn, PlayersTurn, CardSelected, UnitSelected, GameOver }

        private const int TimeBetweenAttacks = 500;

        public enum GameMode { Private, Public }
        private GameMode _CurrentGameMode = GameMode.Private;
        public GameMode CurrentGameMode {
            get { return _CurrentGameMode; }
            set { _CurrentGameMode = value; }
        }

        /// <summary>
        /// The Current State of the Game 
        /// </summary>
        private GameCycle _CurrentState;
        public GameCycle CurrentState {
            get { return _CurrentState; }
            set { _CurrentState = value; }
        }

        private Gamefield _Gamefield;
        public Gamefield gamefield {
            get { return _Gamefield; }
            set { _Gamefield = value; }
        }

        private Player[] _Players = new Player[2];
        public Player[] Players {
            get { return _Players; }
            set { _Players = value; }
        }

        private bool Sacrificed = false;

        /// <summary>
        /// Index of the current Player.
        /// </summary>
        private int _CurrentPlayerIndex = 0;
        public int CurrentPlayerIndex {
            get { return _CurrentPlayerIndex; }
        }

        /// <summary>
        /// List containing all imported Cards.
        /// </summary>
        private List<AbstractCard> Cards = new List<AbstractCard>();

        private AbstractCard _SelectedCard;
        /// <summary>
        /// The selected Card of the current Player
        /// </summary>
        public AbstractCard SelectedCard {
            get { return _SelectedCard; }
            set { _SelectedCard = value; }
        }

        private Unit _SelectedUnit;
        private Unit SelectedUnit {
            get { return _SelectedUnit; }
            set { _SelectedUnit = value; }
        }

        private UdpSender Network;

        private UbiHand hand;

        public Game(Gamefield pGamefield, UbiHand hand, GameMode mode, String path, bool TestMode) {
            this.gamefield = pGamefield;
            this.CurrentGameMode = mode;
            this.hand = hand;
            this.Cards = LoadCards(path);

            // set up network connection
            this.Network = UdpSender.GetInstance();
            this.SendNetworkGreeting();

            Player p1 = new Player(), p2 = new Player();
            if (TestMode) {
                p1.AmountRessources = 100;
                p2.AmountRessources = 100;
            }

            StartNewGame(p1, p2);
        }

        /// <summary>
        /// Initialize new Game
        /// </summary>
        /// <param name="pPlayerOne"></param>
        /// <param name="pPlayerTwo"></param>
        public void StartNewGame(Player pPlayerOne, Player pPlayerTwo) {
            //Todo Alle atribute reseten

            // initialize players
            pPlayerOne.Deck = this.getMixedDeck();
            pPlayerTwo.Deck = this.getMixedDeck();

            this.Players[0] = pPlayerOne;
            this.Players[1] = pPlayerTwo;

            DrawCard(1, false);
            DrawCard(1, true);

            DrawCard(0, false);
            DrawCard(0, false);
            DrawCard(0, true);

            _CurrentPlayerIndex = 0;
            Sacrificed = false;

            // set amount of ressources
            Players[CurrentPlayerIndex].RessourceLeft = Players[CurrentPlayerIndex].AmountRessources;

            CurrentState = GameCycle.PlayersTurn;
        }

        private void BeginTurn() {
            if (CurrentState != GameCycle.BeginTurn) {
                throw new InvalidOperationException();
            }

            // Decrease countdown of units and reset movement Points
            for (int x = 2; x >= 0; x--) {
                for (int y = 0; y < 5; y++) {
                    Unit currentUnit = gamefield.Units[CurrentPlayerIndex, x, y];

                    if (currentUnit != null) {

                        // decrease cooldown if unit is a creature
                        if (currentUnit.Card is Creature) {
                            currentUnit.Countdown--;
                            currentUnit.Move = ((Creature)currentUnit.Card).Move;
                        }                    

                        // always send out unit via network
                        Network.SendUnit(currentUnit, CurrentPlayerIndex);
                    }
                }
            }
            // set amount of ressources 
            Players[CurrentPlayerIndex].RessourceLeft = Players[CurrentPlayerIndex].AmountRessources;
            gamefield.UpdateRessources(CurrentPlayerIndex, Players[CurrentPlayerIndex].RessourceLeft);

            Sacrificed = false;
            DrawCard();
            CurrentState = GameCycle.PlayersTurn;
        }

        /// <summary>
        /// Finish the Turn of the current Player
        /// </summary>
        public void FinishTurn() {
            if (CurrentState != GameCycle.PlayersTurn && CurrentState != GameCycle.CardSelected && CurrentState != GameCycle.UnitSelected) {
                throw new InvalidOperationException();
            }

            gamefield.resetIsSelectAble();
            gamefield.ResetCardHighlighting();
            SelectedCard = null;
            SelectedUnit = null;

            gamefield.DeactivateButtons(CurrentPlayerIndex);

            //attack opponent Units
            BackgroundWorker attackWorker = new BackgroundWorker();
            attackWorker.DoWork += new DoWorkEventHandler(DoAttacking);
            attackWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(PrepareNextTurn);
            attackWorker.RunWorkerAsync();
        }

        /// <summary>
        /// compute attacking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoAttacking(object sender, DoWorkEventArgs e) {
            for (int x = 2; x >= 0; x--) {
                for (int y = 0; y < 5; y++) {
                    int absX = Math.Abs((CurrentPlayerIndex * -2) + x);
                    Unit unit = gamefield.Units[CurrentPlayerIndex, absX, y];
                    if (unit != null && unit.Card is Creature && unit.Countdown <= 0) {
                        Console.WriteLine("Fight!");

                        ((Creature)unit.Card).Attack.PerformAttack(gamefield, CurrentPlayerIndex, absX, y);

                        //reset countdown
                        unit.Countdown = ((Creature)unit.Card).Countdown;

                        Network.SendUnit(unit, CurrentPlayerIndex);

                        // clean up game field
                        gamefield.RefreshGameField();
                        Thread.Sleep(TimeBetweenAttacks);
                    }
                }
            }
        }

        public void PrepareNextTurn(object sender, RunWorkerCompletedEventArgs e) {
            //check Victory condition
            int count = 0;
            for (int i = 0; i < 5; i++) {
                // check if three or more idols of the opponent player are destroyed
                if (gamefield.Idols[(CurrentPlayerIndex + 1) % 2, i] == 0) {
                    if (++count >= 2) {
                        //TODO Nachricht an Brille.
                        CurrentState = GameCycle.GameOver;
                        gamefield.ShowEndOfGameMessage(CurrentPlayerIndex);
                        gamefield.gid = new GameInputDetector(gamefield.window, hand, this);
                        return;
                    }
                }
            }

            //change current Player
            _CurrentPlayerIndex = ++_CurrentPlayerIndex % 2;

            gamefield.ResetButtons(CurrentPlayerIndex);

            gamefield.RefreshGameField();

            CurrentState = GameCycle.BeginTurn;
            BeginTurn();

            // Restart Detection Thread
            gamefield.gid = new GameInputDetector(gamefield.window, hand, this);
        }

        /// <summary>
        /// Change Selected Card
        /// </summary>
        /// <param name="player">the index of the player, whose card was selected</param>
        /// <param name="CardFieldIndex">Index of the selected CardField</param>
        public void SelectCard(int player, int CardFieldIndex) {
            if (CurrentState == GameCycle.PlayersTurn || CurrentState == GameCycle.UnitSelected || CurrentState == GameCycle.CardSelected) {
                this.SelectedUnit = null;
                AbstractCard newSelectedCard = null;

                // check whether card can be selected
                if (player == CurrentPlayerIndex && (newSelectedCard = Players[CurrentPlayerIndex].getCardAt(CardFieldIndex)) != null) {

                    SoundPlayer.getInstance().playClick();
                    gamefield.HighlightCard(CurrentPlayerIndex, CardFieldIndex);

                    if (SelectedCard != null && SelectedCard.Equals(newSelectedCard)) {
                        // deselect last selected card
                        this.SelectedCard = null;

                        this.CurrentState = GameCycle.PlayersTurn;
                        gamefield.resetIsSelectAble();
                        gamefield.ResetCardHighlighting();
                    } else {
                        this.SelectedCard = Players[CurrentPlayerIndex].getCardAt(CardFieldIndex);

                        // check if the current Player have enough Ressource to Play the selected Card
                        if (this.Players[CurrentPlayerIndex].RessourceLeft >= this.SelectedCard.RessourceCost) {
                            gamefield.selectAbleFieldsToPlayCard(this.SelectedCard, CurrentPlayerIndex);
                        }

                        // change State
                        this.CurrentState = GameCycle.CardSelected;
                    }

                    gamefield.RefreshGameField();

                } else {
                    Console.WriteLine("No Card to select!");
                }
            }
        }

        /// <summary>
        /// Handle the selection of the field at the given given HexField position
        /// </summary>
        /// <param name="PlayerSide"></param>
        /// <param name="XPos"></param>
        /// <param name="YPos"></param>
        public void SelectField(int PlayerSide, int XPos, int YPos) {

            switch (CurrentState) {
                case GameCycle.PlayersTurn:
                    Unit unit = gamefield.Units[PlayerSide, XPos, YPos];
                    if (unit != null) {
                        if (PlayerSide == CurrentPlayerIndex && unit.Card is Creature) {

                            // Show fields the selected Unit can move to.
                            gamefield.selectabelFieldsForMoving(PlayerSide, XPos, YPos);
                            gamefield.RefreshGameField();
                        }

                        this.SelectedUnit = unit;
                        CurrentState = GameCycle.UnitSelected;
                    }
                    break;

                case GameCycle.UnitSelected:
                    // try to move the Selected Unit to the Selected HexField

                    if (gamefield.MoveUnitTo(SelectedUnit, PlayerSide, XPos, YPos)) {
                        SoundPlayer.getInstance().playClick();
                        this.SelectedUnit.Move -= 1;
                    }

                    this.SelectedUnit = null;
                    this.gamefield.resetIsSelectAble();
                    this.CurrentState = GameCycle.PlayersTurn;
                    break;

                case GameCycle.CardSelected:

                    //if (this.SelectedCard != null
                    //    && this.Players[CurrentPlayerIndex].RessourceLeft >= this.SelectedCard.RessourceCost) {

                    if (gamefield.PlayCardtAt(this.SelectedCard, PlayerSide, XPos, YPos)) {
                        SoundPlayer.getInstance().playClick();
                        Unit Target = gamefield.Units[PlayerSide, XPos, YPos];

                        //Perform Abilities which are triggered when the Card is Played
                        foreach (AbstractAbilityComponent ability in SelectedCard.Abilities) {
                            if (ability.Trigger == AbstractAbilityComponent.AbilityTrigger.PlayCard && ability is EffectUnit) {
                                ((EffectUnit)ability).PerformEffect(Target);
                                if (Target.Health <= 0) {
                                    int markerId = Target.MarkerID;
                                    gamefield.Units[PlayerSide, XPos, YPos] = null;
                                    Unit.ReAddMarkerId(markerId);
                                }
                            }
                        }
                        this.Players[CurrentPlayerIndex].RessourceLeft -= this.SelectedCard.RessourceCost;
                        gamefield.UpdateRessources(CurrentPlayerIndex, this.Players[CurrentPlayerIndex].RessourceLeft);
                        this.Players[CurrentPlayerIndex].RemoveCard(SelectedCard);

                        this.SelectedCard = null;
                        this.gamefield.resetIsSelectAble();
                        this.CurrentState = GameCycle.PlayersTurn;

                        Network.SendCards(CurrentPlayerIndex, Players[CurrentPlayerIndex].getCards());
                        Network.SendUnit(Target, PlayerSide);

                        gamefield.ResetCardHighlighting();
                    }
                    //}
                    break;
            }
        }

        /// <summary>
        /// Current Player draw two new Cards from his deck.
        /// </summary>
        public void SacrificeCardToDrawCards() {

            if (CurrentState == GameCycle.CardSelected && !Sacrificed) {
                //remove selected Card from Hand
                this.Players[CurrentPlayerIndex].RemoveCard(SelectedCard);

                this.SelectedCard = null;
                this._Gamefield.ResetCardHighlighting();
                this.gamefield.resetIsSelectAble();

                // play sound
                SoundPlayer.getInstance().playClick();

                //Draw two new Cards from Deck
                DrawCard(CurrentPlayerIndex, false);
                DrawCard();

                Sacrificed = true;
                gamefield.DectivateSacrifice(CurrentPlayerIndex);
                CurrentState = GameCycle.PlayersTurn;
            } else {
                Console.WriteLine("Can't sacrifice Card for new Cards");
            }
        }

        /// <summary>
        /// the current player gain a extra ressource and increase his maximum resource count
        /// </summary>
        public void SacrificeCardForRessource() {
            if (CurrentState == GameCycle.CardSelected && !Sacrificed) {
                this.Players[CurrentPlayerIndex].RemoveCard(SelectedCard);

                this.SelectedCard = null;
                this._Gamefield.ResetCardHighlighting();
                this.gamefield.resetIsSelectAble();

                this.Players[CurrentPlayerIndex].RessourceLeft++;
                this.Players[CurrentPlayerIndex].AmountRessources++;

                Sacrificed = true;
                gamefield.DectivateSacrifice(CurrentPlayerIndex);
                CurrentState = GameCycle.PlayersTurn;

                SoundPlayer.getInstance().playClick();

                Network.SendCards(CurrentPlayerIndex, Players[CurrentPlayerIndex].getCards());

                gamefield.UpdateRessources(CurrentPlayerIndex, Players[CurrentPlayerIndex].RessourceLeft);
            } else {
                Console.WriteLine("Can't sacrifice Card for Ressource");
            }
        }

        /// <summary>
        ///  Draw Card for the current Player.
        /// </summary>
        private void DrawCard() {
            DrawCard(CurrentPlayerIndex, true);
        }

        /// <summary>
        /// Draw Card for the given Player. If the Deck of the Player is empty it will create a new one.
        /// </summary>
        /// <param name="PlayerIndex"></param>
        /// <param name="update">if true send changes via network</param>
        private void DrawCard(int PlayerIndex, bool update) {
            // If Deck of current Player is Empty create new one
            if (Players[PlayerIndex].Deck.Count == 0) {
                Players[PlayerIndex].Deck = getMixedDeck();
            }

            // Draw new Card from Deck
            if (Players[PlayerIndex].Deck.Count > 0) {
                Players[PlayerIndex].AddCard(Players[PlayerIndex].Deck[0]);
                Players[PlayerIndex].Deck.RemoveAt(0);

                // cards have changed, send out cards via network
                if (update) {
                    Network.SendCards(PlayerIndex, Players[PlayerIndex].getCards());
                }
            } else {
                Console.WriteLine("Player " + CurrentPlayerIndex + " has no more cards to draw!");
            }

        }


        Random random = new Random();
        /// <summary>
        /// Create and return a new mixed Card Deck.
        /// </summary>
        /// <returns>A mixed List of Cards</returns>
        private List<AbstractCard> getMixedDeck() {
            List<AbstractCard> AllCards = new List<AbstractCard>();
            List<AbstractCard> Deck = new List<AbstractCard>();

            foreach (AbstractCard card in Cards) {
                AllCards.Add(card.Clone());
                AllCards.Add(card.Clone());
                AllCards.Add(card.Clone());
            }

            // mix Cards
            while (AllCards.Count != 0) {
                int r = random.Next(AllCards.Count);
                Deck.Add(AllCards[r]);
                AllCards.RemoveAt(r);
            }
            Console.WriteLine("");
            return Deck;
        }

        /// <summary>
        /// Load Cards from the choosen Folder
        /// </summary>
        private List<AbstractCard> LoadCards(String path) {
            List<AbstractCard> Cards = new List<AbstractCard>();

            // load Cards
            if (path != null && path.Length != 0) {

                System.IO.DirectoryInfo ParentDirectory = new System.IO.DirectoryInfo(path);
                //Import Creature
                foreach (System.IO.FileInfo file in ParentDirectory.GetFiles("*_Creature.xml")) {
                    Cards.Add(LoadXML<Creature>(file.FullName));
                }
                //Import Structure
                foreach (System.IO.FileInfo file in ParentDirectory.GetFiles("*_Structure.xml")) {
                    Cards.Add(LoadXML<Structure>(file.FullName));
                }
                //Import Creature
                foreach (System.IO.FileInfo file in ParentDirectory.GetFiles("*_Spell.xml")) {
                    Cards.Add(LoadXML<Spell>(file.FullName));
                }
                //Import Enchantment
                foreach (System.IO.FileInfo file in ParentDirectory.GetFiles("*_Enchantment.xml")) {
                    Cards.Add(LoadXML<Enchantment>(file.FullName));
                }
            } else {
                // create dummy cards
                for (int i = 0; i < 5; i++) {
                    Creature dummy = new Creature(i, "Creature" + i, 1, 2, 3, 1);
                    dummy.Attack = new MeleeAttackComponent();
                    Cards.Add(dummy);
                }
            }
            if (Cards.Count < 7) {
                MessageBox.Show("Only " + Cards.Count + " Cards was found!");
            }

            return Cards;
        }

        /// <summary>
        /// Load an object from an xml file
        /// </summary>
        /// <param name="FileName">Xml file name</param>
        /// <returns>The object created from the xml file</returns>
        private T LoadXML<T>(string FileName) where T : AbstractCard {
            using (var stream = System.IO.File.OpenRead(FileName)) {
                var serializer = new XmlSerializer(typeof(T));
                return serializer.Deserialize(stream) as T;
            }
        }

        /// <summary>
        /// Sends out a greeting message to test network connection.
        /// </summary>
        public void SendNetworkGreeting() {
            Network.sendMessage("Hello Network!");

            Thread.Sleep(200);
        }

        /// <summary>
        /// Returns whether given card is selected.
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="CardFieldIndex"></param>
        /// <returns></returns>
        public bool IsCardSelected(int playerIndex, int CardFieldIndex) {
            if (CurrentState == GameCycle.CardSelected && CurrentPlayerIndex == playerIndex) {
                AbstractCard card = Players[playerIndex].getCardAt(CardFieldIndex);
                if (card != null && card.Equals(SelectedCard)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Toggle CardPage of the currentPlayer 
        /// </summary>
        /// <param name="PlayerIndex"></param>
        public void changeCardPage(int PlayerIndex) {
            this.Players[PlayerIndex].CardPage = (this.Players[PlayerIndex].CardPage + 1) % 2;
            gamefield.resetIsSelectAble();
            SelectedCard = null;
            gamefield.ResetCardHighlighting();
            UdpSender.GetInstance().SendCards(PlayerIndex, this.Players[PlayerIndex].getCards());
        }
    }
}
