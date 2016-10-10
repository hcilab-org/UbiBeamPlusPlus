using UbiBeamPlusPlus.Model.Card;
using UbiBeamPlusPlus.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UbiBeamPlusPlus.Model.Component.Attack {
    [Serializable()]
    public class MeleeAttackComponent : AttackComponent {

        private const int AttackDuration = 2000;
        private const int NumberOfBlinks = 3;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_GameField"></param>
        /// <param name="currentPlayerIndex">Index of the Current Player </param>
        /// <param name="PosX">X Field Position of the attacking Unit</param>
        /// <param name="PosY">Y Field Position of the attacking Unit</param>
        public override void PerformAttack(Gamefield _GameField, int currentPlayerIndex, int PosX, int PosY) {
            Unit Attacker = _GameField.Units[currentPlayerIndex, PosX, PosY];

            int opponentIndex = (currentPlayerIndex + 1) % 2;
            bool AttackIdol = true;

            for (int x = 0; x < 3; x++) {
                int absX = Math.Abs((currentPlayerIndex * -2) + x);
                Unit Defender = _GameField.Units[opponentIndex, absX, PosY];

                if (Defender != null) {

                    // show attack
                    showAttack(_GameField, PosX, PosY, opponentIndex, absX);

                    // Deal damage to Defender
                    if (Attacker.Damage >= Defender.Health) {
                        Defender.Health = 0;

                        _GameField.Units[(currentPlayerIndex + 1) % 2, absX, PosY] = null;
                        Unit.ReAddMarkerId(Defender.MarkerID);

                        //TODO: Sterbe animation starten
                    } else {
                        Defender.Health -= Attacker.Damage;
                    }

                    UdpSender.GetInstance().SendUnit(Defender, opponentIndex);
                    AttackIdol = false;
                    break;
                }
            }

            //Deal Damage to Idol
            if (AttackIdol) {
                int x = opponentIndex == 1 ? 2 : 0;
                showAttack(_GameField, PosX, PosY, opponentIndex, x);

                // Deal Damage
                if (Attacker.Damage >= _GameField.Idols[(currentPlayerIndex + 1) % 2, PosY]) {
                    _GameField.Idols[(currentPlayerIndex + 1) % 2, PosY] = 0;
                } else {
                    _GameField.Idols[(currentPlayerIndex + 1) % 2, PosY] -= Attacker.Damage;

                }
            }
        }

        private static void showAttack(Gamefield _GameField, int PosX, int PosY, int opponent, int x) {
            for (int blink = 0; blink < NumberOfBlinks; blink++) {
                _GameField.HighlightFight(opponent, PosY, PosX, x);
                Thread.Sleep(AttackDuration / (NumberOfBlinks * 2));
                if (blink < (NumberOfBlinks - 1)) {
                    _GameField.RefreshGameField();
                    Thread.Sleep(AttackDuration / (NumberOfBlinks * 2));
                }
            }
        }
    }
}
