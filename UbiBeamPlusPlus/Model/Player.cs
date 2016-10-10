using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UbiBeamPlusPlus.Model.Card;

namespace UbiBeamPlusPlus.Model {

    /// <summary>
    /// Represents a player playing the game.
    /// </summary>
    public class Player {

        /// <summary>
        /// The default name for a player
        /// </summary>
        private const String DefaultName = "Player";

        /// <summary>
        /// Name of the Player
        /// </summary>
        private String _Name = "";
        public String Name {
            get { return _Name; }
            set { _Name = value; }
        }

        /// <summary>
        /// Amount of Ressources this Player have for each Round.
        /// </summary>
        private int _AmountRessources = 0;
        public int AmountRessources {
            get { return _AmountRessources; }
            set { _AmountRessources = value; }
        }

        /// <summary>
        /// Amount of Ressource the Player have left for this round
        /// </summary>
        private int _RessourceLeft = 0;
        public int RessourceLeft {
            get { return _RessourceLeft; }
            set { _RessourceLeft = value; }
        }

        private List<AbstractCard> _Deck = new List<AbstractCard>();
        internal List<AbstractCard> Deck {
            get { return _Deck; }
            set { _Deck = value; }
        }


        private List<AbstractCard> _Cards = new List<AbstractCard>();
        /// </summary>
        /// Cards the Player have in his Hand
        /// </summary>
        //public List<AbstractCard> Cards {
        //    get { return _Cards; }
        //    set { _Cards = value; }
        //}

        private int _CardPage = 0;
        /// <summary>
        /// Indicates if the cards 0 to 4 or 5 to 9 is shown.
        /// </summary>
        public int CardPage {
            get { return _CardPage; }
            set { _CardPage = value; }
        }

        /// <summary>
        /// Creates a new Player with default name and empty card deck.
        /// </summary>
        public Player()
            : this(DefaultName, new List<AbstractCard>()) {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="playerDeck"></param>
        public Player(String playerName, List<AbstractCard> playerDeck) {
            this.Name = playerName;
            this._Deck = playerDeck;

        }

        /// <summary>
        /// Return Card at the given CardFieldIndex. Return null if the Index out of bound.
        /// </summary>
        /// <param name="CardFieldIndex"></param>
        /// <returns></returns>
        public AbstractCard getCardAt(int CardFieldIndex) {
            int CardIndex = this.CardPage * 5 + CardFieldIndex;
            if (CardIndex < _Cards.Count) {
                return _Cards[CardIndex];
            } else {
                return null;
            }
        }

        public void AddCard(AbstractCard card) {
            _Cards.Add(card);
        }

        public bool RemoveCard(AbstractCard card) {
           return _Cards.Remove(card);
        }

        public List<AbstractCard> getCards() {
            if(CardPage == 0){
                return _Cards.GetRange(0, _Cards.Count);
            } else {
                if (_Cards.Count > 5) {
                    return _Cards.GetRange(5, _Cards.Count - 5);
                }
                return new List<AbstractCard>();
            }
        }
    }
}
