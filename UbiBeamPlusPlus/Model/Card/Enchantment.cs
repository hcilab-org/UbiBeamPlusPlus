using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UbiBeamPlusPlus.Model.Component.Ability.EnchantmentAbility;
using System.Xml.Serialization;
using UbiBeamPlusPlus.Model.Component.Ability;

namespace UbiBeamPlusPlus.Model.Card {
    [Serializable]
    public class Enchantment : AbstractCard {

        public Enchantment(int CardID, String Name, byte Cost)
            : base(CardID, Name, Cost) {

        }

        /// <summary>
        /// Constructor for XML Serialization
        /// </summary>
        private Enchantment() : base(0, "", 0) { }
    }
}
