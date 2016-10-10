using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UbiBeamPlusPlus.Model.Component.Ability.StructureAbility {
    
    /// <summary>
    /// Creatures you control have +X Attack.
    /// </summary>
    [Serializable]
    public class AdditionalDamage : StructureAbilityComponent {

        private int _AdditionalAttack = 0;
        public int AdditionalAttackValue {
            get { return _AdditionalAttack; }
            set { _AdditionalAttack = value; }
        }

        public AdditionalDamage(int pAdditionalAttackValue)
            : base(AbilityTrigger.PlayCard, "Creatures you control have " + pAdditionalAttackValue + " Attack. ") {

            this.AdditionalAttackValue = pAdditionalAttackValue;
        }

    }
}
