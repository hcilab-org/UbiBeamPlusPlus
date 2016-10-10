using UbiBeamPlusPlus.Model.Component.Ability.CreatureAbility;
using UbiBeamPlusPlus.Model.Component.Ability.EnchantmentAbility;
using UbiBeamPlusPlus.Model.Component.Ability.SpellAbility;
using UbiBeamPlusPlus.Model.Component.Ability.StructureAbility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UbiBeamPlusPlus.Model.Component.Ability {
    
    [XmlInclude(typeof(Haste))]
    [XmlInclude(typeof(EnchantmentAbility.AdditionalDamage))]
    [XmlInclude(typeof(AdditionalHealth))]
    [XmlInclude(typeof(DamageToOne))]
    [XmlInclude(typeof(SpellAbility.Healing))]
    [XmlInclude(typeof(IncreaseCountdown))]
    [XmlInclude(typeof(Regeneration))]
    [Serializable]
    public abstract class AbstractAbilityComponent {

        /// <summary>
        /// Ability is triggered
        /// Defending: when the Unit with this ability is attacked.
        /// Attacking: when the Unit with this ability is attacking an other Unit.
        /// PlayCard: when the Card with this Ability is played.
        /// Countdown: when the Countdown is 0.
        /// Targeted: when the Unit with this ability is the Target of an enchantment or Spell.
        /// BeginingOfTurn: at the beginning of their turns.
        /// </summary>
        public enum AbilityTrigger { Defending, PlayCard, Attacking, Countdown, BeginingOfTurn, Targeted}

        private String _Description ="No Description";
        public String Description {
            get { return _Description.Replace("#","" + Value); }
            set { _Description = value; }
        }

        private AbilityTrigger _Trigger;
        public AbilityTrigger Trigger {
            get { return _Trigger; }
            set { _Trigger = value; }
        }

        private String _Name;
        public String Name {
            get { return this.GetType().Name; }
        }

        private byte _Value = 0;
        public byte Value {
            get { return _Value; }
            set { _Value = value; }
        }


        public AbstractAbilityComponent(AbilityTrigger Trigger, String Description) {
            this.Trigger = Trigger;
            this.Description = Description;
        }
    }
}
