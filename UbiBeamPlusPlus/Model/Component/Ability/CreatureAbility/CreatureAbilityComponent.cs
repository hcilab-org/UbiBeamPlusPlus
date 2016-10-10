using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UbiBeamPlusPlus.Model.Card;

namespace UbiBeamPlusPlus.Model.Component.Ability.CreatureAbility {
   [Serializable]
    public abstract class CreatureAbilityComponent : AbstractAbilityComponent {

        public CreatureAbilityComponent(AbilityTrigger Trigger, String Description)
            : base(Trigger, Description) {

        }
    }
}
