using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UbiBeamPlusPlus.Model.Component.Ability.StructureAbility {
    [Serializable]
    public class AdditionalRessources : StructureAbilityComponent {

        private byte _RessourceValue = 0;
        public byte RessourcesValue {
            get { return _RessourceValue; }
            set { _RessourceValue = value; }
        }


        public AdditionalRessources()
            : base(AbilityTrigger.BeginingOfTurn, "When it comes into play, Resources is increased by") {

        }
    }
}
