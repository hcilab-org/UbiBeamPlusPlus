using UbiBeamPlusPlus.Model.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UbiBeamPlusPlus.Model.Component.Ability.EnchantmentAbility {
    [Serializable]
    public abstract class EnchantmentAbilityComponent : AbstractAbilityComponent {
        public EnchantmentAbilityComponent(AbilityTrigger Trigger, String Description)
            : base(Trigger, Description) {

        }

    }
}
