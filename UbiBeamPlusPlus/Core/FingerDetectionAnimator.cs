using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UbiDisplays;
using UbiBeamPlusPlus.UI;

namespace UbiBeamPlusPlus.Core {
    /// <summary>
    /// Detects Fingers and triggers animations when a finger touches the surface
    /// </summary>
    class FingerDetectionAnimator {

        protected static float m_FingerX = 0;
        protected static float m_FingerY = 0;

        private System.Windows.Point m_StartPoint;
        private bool m_AnimationControl = false;
        private Thread m_HandThread;
        private Hand m_Hand;
        private System.Windows.Controls.Image m_Image;
        private MainWindow m_Parent;

        public FingerDetectionAnimator(MainWindow pParent, Hand pHand, System.Windows.Controls.Image pImage) {
            this.m_Parent = pParent;
            this.m_Hand = pHand;
            this.m_Image = pImage;

            m_HandThread = new Thread(() => this.calculateFinger(true, new System.Windows.Point(m_FingerX, m_FingerY)));
            m_HandThread.Start();
        }

        /// <summary>
        /// Calculate the fingerposition and trigger the animation to the touched position
        /// </summary>
        /// <param name="pAnimationControl"></param>
        /// <param name="pStartPoint"></param>
        public void calculateFinger(bool pAnimationControl, System.Windows.Point pStartPoint) {
            m_AnimationControl = pAnimationControl;

            while (m_AnimationControl) {
                // Calculating relative position of the fingers in range of 0.0 - 1.0
                for (int i = 0; i < m_Hand.FingerCount(); ++i) {
                    m_FingerX = ((100.0f + m_Hand.GetFinger(0).X) / 100.0f) * System.Windows.Forms.Screen.AllScreens[0].WorkingArea.Width;
                    m_FingerY = ((100.0f + m_Hand.GetFinger(0).Y) / 100.0f) * System.Windows.Forms.Screen.AllScreens[0].WorkingArea.Height;
                    m_AnimationControl = false;
                }
                if (m_FingerX != pStartPoint.X && m_FingerY != pStartPoint.Y) {
                    this.triggerAnimation(m_Parent, m_Image);
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void triggerAnimation(MainWindow pParent, System.Windows.Controls.Image pImage) {
            pParent.Dispatcher.Invoke((Action)(() => {
                // Preparing a polybeziersegment
                // TODO: Maybe a non-interpolated list can be taken into account
                PolyBezierSegment bezierSegment = new PolyBezierSegment();
                bezierSegment.Points.Add(new System.Windows.Point(m_FingerX / 3, m_FingerY / 3));
                bezierSegment.Points.Add(new System.Windows.Point((m_FingerX / 3) * 2, (m_FingerY / 3) * 2));
                bezierSegment.Points.Add(new System.Windows.Point(m_FingerX, m_FingerY));
                Storyboard storyBoard = Utils.animate(pParent, pImage, m_StartPoint, bezierSegment);
                storyBoard.Completed += new EventHandler(animation_completed);
                storyBoard.Begin(pParent);
                Console.WriteLine("X: " + m_FingerX);
                Console.WriteLine("Y: " + m_FingerY);
            }));
            m_StartPoint.X = m_FingerX;
            m_StartPoint.Y = m_FingerY;
        }

        /// <summary>
        /// Listener waiting for a completed animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void animation_completed(object sender, EventArgs e) {
            // Wait until thread is finished
            m_HandThread.Join();
            // Reinstantiate thread for restarting it
            m_HandThread = new Thread(() => this.calculateFinger(true, new System.Windows.Point(m_FingerX, m_FingerY)));
            m_HandThread.Start();
        }

    }
}
