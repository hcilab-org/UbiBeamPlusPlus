using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UbiBeamPlusPlus.Model.Component.Ability.EnchantmentAbility {

    /// <summary>
    /// Adds X extra damage when enchanted unit receives damage. 
    /// </summary>
    [Serializable]
    public class Curse : EnchantmentAbilityComponent {

        
        public Curse(byte pAdditionalDamage)
            : base(AbilityTrigger.Defending, "Adds # extra damage when enchanted unit receives damage.") {
            
            Value = pAdditionalDamage;
        }

        public void PerformEffect(Gamefield _Gamefield) {
            throw new NotImplementedException();
        }

        public void RemoveEffect(Gamefield _Gamefield) {
            throw new NotImplementedException();
        }
    }
}
