using UbiBeamPlusPlus.Model.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UbiBeamPlusPlus.Model.Component.Ability.CreatureAbility {
    /// <summary>
    /// Blocks X amount of combat damage.
    /// </summary>
    [Serializable]
    class Armor : CreatureAbilityComponent {

        public Armor(byte pArmorValue)
            : base(AbilityTrigger.Defending, "Blocks # amount of combat damage.") {
            Value = pArmorValue;
        }

        public void performEffect(Gamefield gameField, Unit unit) {
            throw new NotImplementedException();
        }
    }
}
