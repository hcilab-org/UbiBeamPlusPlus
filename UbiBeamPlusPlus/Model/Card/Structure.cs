using UbiBeamPlusPlus.Model.Component.Ability;
using UbiBeamPlusPlus.Model.Component.Ability.StructureAbility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace UbiBeamPlusPlus.Model.Card {
    [Serializable]
    public class Structure : AbstractCard {

        private byte _Health;
        public byte Health {
            get { return _Health; }
            set { _Health = value; }
        }

        public Structure(int CardID, String Name, byte Cost, byte Health)
            : base(CardID, Name, Cost) {
            this.Health = Health;
        }

        /// <summary>
        /// Constructor for XML Serialization
        /// </summary>
        private Structure() : base(0,"", 0) { }

    }
}
