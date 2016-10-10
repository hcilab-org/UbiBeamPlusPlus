using UbiBeamPlusPlus.Model.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UbiBeamPlusPlus.Model.Component.Ability.EnchantmentAbility {
    /// <summary>
    /// Enchanted unit is healed by X Health each round.
    /// </summary>
    [Serializable]
    public class Healing : EnchantmentAbilityComponent, EffectUnit {

        public Healing(byte HealthValue)
            : base(AbilityTrigger.BeginingOfTurn, "Enchanted unit is healed by # Health each round.") {
                Value = HealthValue;
        }

        public void PerformEffect(Unit Target) {
            if (Target.Card is Creature && Target.Health < ((Creature) Target.Card).Health) {
                Target.Health += Value;
            }
        }
    }
}
