using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UbiBeamPlusPlus.Model.Component.Ability.CreatureAbility {
    /// <summary>
    /// Cannot be targeted by opponent Spells or Enchantments.
    /// </summary>
   [Serializable]
    class Ward : CreatureAbilityComponent {

        public Ward()
            : base(AbilityTrigger.Targeted, "Cannot be targeted by opponent Spells or Enchantments.") {

        }

    }
}
