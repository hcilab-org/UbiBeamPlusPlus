using UbiBeamPlusPlus.Model.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UbiBeamPlusPlus.Model.Component.Ability.SpellAbility {
   [Serializable]
    public abstract class SpellAbilityComponent : AbstractAbilityComponent {

        public SpellAbilityComponent(AbilityTrigger Trigger, String Description)
            : base( Trigger, Description) {

        }

    }
}
