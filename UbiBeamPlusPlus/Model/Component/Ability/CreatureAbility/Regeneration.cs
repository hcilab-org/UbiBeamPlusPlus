using UbiBeamPlusPlus.Model.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UbiBeamPlusPlus.Model.Component.Ability.CreatureAbility {
    /// <summary>
    /// Units with Regeneration heal X Health at the beginning of their turns. 
    /// </summary>
    [Serializable]
    public class Regeneration : CreatureAbilityComponent, EffectUnit {

        public Regeneration(byte pRegeneration)
            : base(AbilityTrigger.BeginingOfTurn, "Units with Regeneration heal # Health at the beginning of their turns.") {

            Value = pRegeneration;
        }

        /// <summary>
        /// Constructor fo XML Serialization
        /// </summary>
        private Regeneration()
            : base(AbilityTrigger.BeginingOfTurn, "") {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Target"></param>
        public void PerformEffect(Unit Target) {
            if (Target.Card is Creature) {
                if (Target.Health + Value <= ((Creature)Target.Card).Health) {
                    Target.Health += Value;
                } else {
                    Target.Health = ((Creature)Target.Card).Health;
                }
            }
            //TODO Code Clone 
            if (Target.Card is Structure) {
                if (Target.Health + Value <= ((Structure)Target.Card).Health) {
                    Target.Health += Value;
                } else {
                    Target.Health = ((Structure)Target.Card).Health;
                }
            }
        }
    }
}
