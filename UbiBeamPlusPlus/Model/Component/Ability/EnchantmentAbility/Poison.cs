using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UbiBeamPlusPlus.Model.Component.Ability.EnchantmentAbility {
    /// <summary>
    /// Enchanted creature becomes poisoned for X rounds. 
    /// </summary>
    [Serializable]
    public class Poison : EnchantmentAbilityComponent {

        public Poison(int RoundsCount)
            : base(AbilityTrigger.BeginingOfTurn, "Enchanted creature becomes poisoned for " + RoundsCount + " rounds") {

        }

    }
}
