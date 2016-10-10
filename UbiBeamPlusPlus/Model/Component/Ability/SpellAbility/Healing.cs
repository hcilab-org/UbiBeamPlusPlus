using UbiBeamPlusPlus.Model.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UbiBeamPlusPlus.Model.Component.Ability.SpellAbility {
    /// <summary>
    /// Heal target unit by X Health.
    /// </summary>
    [Serializable]
    public class Healing : SpellAbilityComponent,EffectUnit {

        public Healing(byte _HealingValue)
            : base(AbilityTrigger.PlayCard, "Heal target unit by # Health") {
                Value = _HealingValue;
        }

        /// <summary>
        /// Constructor for XML Serialization
        /// </summary>
        private Healing() : base(AbilityTrigger.PlayCard, "") { }

        public void PerformEffect(Unit Target) {
            Target.Health += Value;
        }
    }
}
