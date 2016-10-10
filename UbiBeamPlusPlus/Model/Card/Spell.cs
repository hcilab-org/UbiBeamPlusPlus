using UbiBeamPlusPlus.Model.Component.Ability;
using UbiBeamPlusPlus.Model.Component.Ability.SpellAbility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UbiBeamPlusPlus.Model.Card {
    [Serializable]
    public class Spell : AbstractCard {

        public Spell(int CardID, String Name, byte Cost)
            : base(CardID, Name, Cost) {
        }

        /// <summary>
        /// Constructor for XML Serialization
        /// </summary>
        private Spell() : base(0,"", 0) { }

    }
}
