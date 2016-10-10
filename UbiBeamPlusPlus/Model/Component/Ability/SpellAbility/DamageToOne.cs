using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UbiBeamPlusPlus.Model.Card;

namespace UbiBeamPlusPlus.Model.Component.Ability.SpellAbility {
    /// <summary>
    /// Deal X damage to target unit. 
    /// </summary>
    [Serializable]
    public class DamageToOne : SpellAbilityComponent, EffectUnit {

        public DamageToOne(byte DamageValue)
            : base(AbilityTrigger.PlayCard, "Deal # damage to target Unit") {
            Value = DamageValue;
        }

        /// <summary>
        /// Constructor for XML Serialization
        /// </summary>
        private DamageToOne() : base(AbilityTrigger.PlayCard, "") { }

        public void PerformEffect(Unit Target) {
            if (Target.Health <= Value) {
                Target.Health = 0;
            } else {
                Target.Health -= Value;
            }
        }
    }
}
