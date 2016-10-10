using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UbiBeamPlusPlus.Model.Component;
using UbiBeamPlusPlus.Model.Component.Ability;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UbiBeamPlusPlus.Model.Card {

    [Serializable()]
    public abstract class AbstractCard {

        private int _CardID = 0;
        public int CardID {
            get { return _CardID; }
            set { _CardID = value; }
        }

        /// <summary>
        /// Name of the Card. Should be Unic
        /// </summary>
        private String _CardName;
        public String CardName {
            set { _CardName = value; }
            get { return _CardName; }
        }

        /// <summary>
        /// Resource Cost to play this Card
        /// </summary>
        private byte _RessourceCost;
        public byte RessourceCost {
            get { return _RessourceCost; }
            set { _RessourceCost = value; }
        }

        [XmlArray("Abilities")]
        [XmlArrayItem("AbstractAbilityComponent")]
        private List<AbstractAbilityComponent> _Abilities = new List<AbstractAbilityComponent>();
        public List<AbstractAbilityComponent> Abilities {
            get { return _Abilities; }
            set { _Abilities = value; }
        }

        public AbstractCard(int CardID,String CardName, byte RessourceCost) {
            this.CardID = CardID;
            this.CardName = CardName;
            this.RessourceCost = RessourceCost;
        }


        public AbstractCard Clone() {
            AbstractCard objResult = null;
            using (MemoryStream ms = new MemoryStream()) {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, this);

                ms.Position = 0;
                objResult = (AbstractCard) bf.Deserialize(ms);
            }
            return objResult;
        }
    }
}
