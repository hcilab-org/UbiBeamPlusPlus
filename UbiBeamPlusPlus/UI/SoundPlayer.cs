using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UbiBeamPlusPlus.UI {

    public class SoundPlayer {
        
        private static SoundPlayer _instance;

        private System.Media.SoundPlayer player;

        private SoundPlayer() {
            this.player = new System.Media.SoundPlayer();
            this.player.SoundLocation = MainWindow.ResourcesDir + "sounds/click.wav";
            this.player.Load();
        }

        public static SoundPlayer getInstance() {
            if (_instance == null) {
                _instance = new SoundPlayer();
            }

            return _instance;
        }
    
        /// <summary>
        /// Plays a click sound in a separate thread
        /// </summary>
        public void playClick() {
            new Thread(() => {
                player.Play();
            }).Start();
        }
    }

}
