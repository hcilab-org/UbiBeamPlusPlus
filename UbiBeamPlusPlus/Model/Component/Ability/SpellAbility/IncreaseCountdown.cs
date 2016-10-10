using UbiBeamPlusPlus.Model.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UbiBeamPlusPlus.Model.Component.Ability.SpellAbility {
    /// <summary>
    /// Increase Countdown of Target Unit by X
    /// </summary>
    [Serializable]
    public class IncreaseCountdown : SpellAbilityComponent, EffectUnit {

        public IncreaseCountdown(byte _Value)
            : base(AbilityTrigger.PlayCard, "Increase Countdown of Target Unit by #") {
            Value = _Value;
        }

        /// <summary>
        /// Constructor for XML Serialization
        /// </summary>
        private IncreaseCountdown()
            : base(AbilityTrigger.PlayCard, "") {

        }

        public void PerformEffect(Unit Target) {
            if (Target.Card is Creature) {
                Target.Countdown += Value;
            }
        }

    }
}
