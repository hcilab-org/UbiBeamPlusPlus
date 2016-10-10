using UbiBeamPlusPlus.Model.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UbiBeamPlusPlus.Model.Component.Ability.StructureAbility {
   [Serializable]
    public abstract class StructureAbilityComponent : AbstractAbilityComponent {
        
        public StructureAbilityComponent(AbilityTrigger Trigger, String Description)
            : base(Trigger, Description) {

        }
    }
}
