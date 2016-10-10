using UbiBeamPlusPlus.Model.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UbiBeamPlusPlus.Model.Component.Attack {

    [XmlInclude(typeof(MeleeAttackComponent))]
    [Serializable]
    public abstract class AttackComponent {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_GameField"></param>
        /// <param name="currentPlayer"></param>
        /// <param name="unit">The Unit which is Attacking</param>
        public abstract void PerformAttack(Gamefield gameField, int CurrentPlayer, int PosX, int PosY);
    }
}
