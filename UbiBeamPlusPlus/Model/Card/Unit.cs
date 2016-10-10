using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UbiBeamPlusPlus.Model.Card {
    /// <summary>
    /// Represent a Unit on the Gamefield that be can a Creature or Structure.
    /// </summary>
    public class Unit {

        /// <summary>
        /// The separator used for the attributes when sending via network
        /// </summary>
        private const String AttributeSeparator = ",";

        public const int NumberOfMarkers = 20;

        private static Stack<Int32> markerIds = new Stack<int>();

        /// <summary>
        /// static code block for marker id stack initialization
        /// </summary>
        static Unit() {
            for (int id=2; id < NumberOfMarkers; id++) {
                markerIds.Push(id);
            }
        }

        public static bool IdsLeft() {
            return markerIds.Count > 0;
        }

        /// <summary>
        /// The Card which belong to the Unit
        /// </summary>
        private readonly AbstractCard _Card;
        public AbstractCard Card {
            get { return _Card; }
        }

        /// <summary>
        /// List of Entchantments 
        /// </summary>
        private List<Enchantment> _Entchantments = new List<Enchantment>();
        public List<Enchantment> Entchantments {
            get { return _Entchantments; }
        }

        /// <summary>
        /// Current Health of the Unit
        /// </summary>
        private int _Health;
        public int Health {
            get { return _Health; }
            set { _Health = value; }
        }

        /// <summary>
        /// Current Countdown of the Unit. If the Countdown is 0 this Unit will attack at the and of this Turn.
        /// </summary>
        private int _Countdown;
        public int Countdown {
            get { return _Countdown; }
            set { _Countdown = value; }
        }

        /// <summary>
        /// Current Damage this Unit will deal to a Enemy if its Attacking him.
        /// </summary>
        private int _Damage;
        public int Damage {
            get { return _Damage; }
            set { _Damage = value; }
        }

        /// <summary>
        /// Remaining distance this Unit can move.
        /// </summary>
        private int _Move = 1;
        public int Move {
            get { return _Move; }
            set { _Move = value; }
        }


        /// <summary>
        /// MarkerID for 3D model of this Unit.  
        /// </summary>
        private int _MarkerID;
        public int MarkerID {
            get { return _MarkerID; }
            set { _MarkerID = value; }
        }

        public Unit(Creature Card) {
            this._Card = Card;
            this.Health = Card.Health;
            this.Countdown = Card.Countdown;
            this.Damage = Card.Damage;
            this.MarkerID = markerIds.Pop();
        }

        public Unit(Structure Card) {
            _Card = Card;
            this.MarkerID = markerIds.Pop();
            this.Health = Card.Health;
        }

        /// <summary>
        /// Returns an attribute string containing health, countdown and damage of this Unit (not 
        /// bthe card!).
        /// </summary>
        /// <returns></returns>
        public String GetAttributeString() {
            StringBuilder attributes = new StringBuilder();
            attributes.Append(this.Health);
            attributes.Append(AttributeSeparator);
            attributes.Append(this.Countdown);
            attributes.Append(AttributeSeparator);
            attributes.Append(this.Damage);
            return attributes.ToString();
        }

        public static void ReAddMarkerId(int markerId) {
            markerIds.Push(markerId);
        }

    }
}
