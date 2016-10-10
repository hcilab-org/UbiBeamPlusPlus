using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UbiBeamPlusPlus.Model.Card;

namespace UbiBeamPlusPlus.Model.Component.Ability.EnchantmentAbility {

    /// <summary>
    /// Enchanted unit gets +X Attack.
    /// </summary>
   [Serializable]
    public class AdditionalDamage : EnchantmentAbilityComponent, EffectUnit {

        public AdditionalDamage(byte damagaValue)
            : base(AbilityTrigger.PlayCard, "Enchanted unit gets +# Attack.") {
            Value = damagaValue;
        }

        /// <summary>
        /// Constructor for XML Serialization
        /// </summary>
        private AdditionalDamage()
            : base(AbilityTrigger.PlayCard, "") {
        }

        public void PerformEffect(Unit Target) {
            if (Target.Card is Creature) {
                Target.Damage += Value;
            }
        }

    }
}
