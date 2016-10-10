using UbiBeamPlusPlus.Model.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UbiBeamPlusPlus.Model.Component.Ability {
    public interface EffectUnit {

        void PerformEffect(Unit Target);
    }
}
