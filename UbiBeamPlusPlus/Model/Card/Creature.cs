using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using UbiBeamPlusPlus.Model.Component.Attack;
using UbiBeamPlusPlus.Model.Component.Ability.CreatureAbility;
using System.Xml.Serialization;
using UbiBeamPlusPlus.Model.Component.Ability;

namespace UbiBeamPlusPlus.Model.Card {
    /// <summary>
    /// Card Type which can be used to summon a unit/Creature on the Gamefield.
    /// </summary>
    [Serializable]
    public class Creature : AbstractCard {

        private byte _Health = 0;
        public byte Health {
            get { return _Health; }
            set { _Health = value; }
        }


        private byte _Countdown = 0;
        public byte Countdown {
            get { return _Countdown; }
            set { _Countdown = value; }
        }

        private byte _Damage = 0;
        public byte Damage {
            get { return _Damage; }
            set { _Damage = value; }
        }

        /// <summary>
        /// Distance a Unit can Move on the Gamefield
        /// </summary>
        private byte _Move = 1;
        public byte Move {
            get { return _Move; }
            set { _Move = value; }
        }

        private AttackComponent _Attack;
        public AttackComponent Attack {
            get { return _Attack; }
            set { _Attack = value; }
        }

        public Creature(int CardID, String name, byte cost, byte health, byte damage, byte countdown)
            : base(CardID, name, cost) {
            this.Countdown = countdown;
            this.Health = health;
            this.Damage = damage;
        }

        /// <summary>
        /// Constructor for XML Serialization
        /// </summary>
        private Creature() : base(0, "", 0) { }

    }
}
