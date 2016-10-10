using UbiBeamPlusPlus.Model.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UbiBeamPlusPlus.Model.Component.Ability.EnchantmentAbility {
    /// <summary>
    /// Enchanted unit gets +X Health.
    /// </summary>
   [Serializable]
    public class AdditionalHealth : EnchantmentAbilityComponent, EffectUnit {

        public AdditionalHealth(byte HealthValue)
            : base(AbilityTrigger.PlayCard, "Enchanted unit gets +# Health.") {
            Value = HealthValue;
        }

        /// <summary>
        /// Constructor for XML Serialization
        /// </summary>
        private AdditionalHealth() : base(AbilityTrigger.PlayCard, "") { }

        public void PerformEffect(Unit Target) {
            if (Target.Card is Creature || Target.Card is Structure) {
                Target.Health += Value;
            }
        }
    }
}
