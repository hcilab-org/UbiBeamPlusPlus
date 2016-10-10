using UbiBeamPlusPlus.Model.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UbiBeamPlusPlus.Model.Component.Ability.CreatureAbility {
    /// <summary>
    /// Unit comes in play with Countdown set to 0. 
    /// </summary>
    [Serializable]
    public class Haste : CreatureAbilityComponent, EffectUnit {

        public Haste()
            : base(AbilityTrigger.PlayCard, "Unit comes in play with Countdown set to 0.") {

        }

        public void PerformEffect(Unit Target) {
            if (Target.Card is Creature) {
                Target.Countdown = 0;
            }
        }
    }
}
