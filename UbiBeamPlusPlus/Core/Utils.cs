using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using UbiBeamPlusPlus.UI;

namespace UbiBeamPlusPlus.Core {

    /// <summary>
    /// Class which contains helper methods
    /// </summary>
    public static class Utils {

        /// <summary>
        /// Calculates the angle of an object based on the intial position and the target position
        /// </summary>
        /// <param name="pPositionX"></param>
        /// <param name="pPositionY"></param>
        /// <param name="pFingerPositionX"></param>
        /// <param name="pFingerPositionY"></param>
        /// <returns>Final angle</returns>
        public static double calculateAngle(double pPositionX, double pPositionY, double pFingerPositionX, double pFingerPositionY) {

            double m_dy = pFingerPositionY - pPositionY;
            double m_dx = pFingerPositionX - pPositionX;

            double result = (double)(Math.Atan2(m_dy, m_dx) * 180 / Math.PI) + 90;

            return result;

        }

        /// <summary>
        /// Movement animation of an WPF image
        /// </summary>
        /// <param name="pParent"></param>
        /// <param name="pImage"></param>
        /// <param name="startPoint"></param>
        /// <param name="bezierSegment"></param>
        /// <returns>The storyboard of the animation</returns>
        public static Storyboard animate(MainWindow pParent, System.Windows.Controls.Image pImage, System.Windows.Point startPoint, PolyBezierSegment bezierSegment) {
            NameScope.SetNameScope(pParent, new NameScope());

            TranslateTransform animatedTranslateTransform = new TranslateTransform();

            pParent.RegisterName("AnimatedTranslateTransform", animatedTranslateTransform);

            pImage.RenderTransform = animatedTranslateTransform;

            PathGeometry animationPath = new PathGeometry();
            PathFigure figure = new PathFigure();
            figure.StartPoint = startPoint;
            figure.Segments.Add(bezierSegment);
            animationPath.Figures.Add(figure);

            animationPath.Freeze();

            DoubleAnimationUsingPath translateXAnimation =
               new DoubleAnimationUsingPath();
            translateXAnimation.PathGeometry = animationPath;
            // Timespan in seconds
            // TODO: Calculate animation time instead of using fixed time
            translateXAnimation.Duration = TimeSpan.FromSeconds(5);

            translateXAnimation.Source = PathAnimationSource.X;

            Storyboard.SetTargetName(translateXAnimation, "AnimatedTranslateTransform");
            Storyboard.SetTargetProperty(translateXAnimation,
                new PropertyPath(TranslateTransform.XProperty));

            DoubleAnimationUsingPath translateYAnimation =
                new DoubleAnimationUsingPath();
            translateYAnimation.PathGeometry = animationPath;
            translateYAnimation.Duration = TimeSpan.FromSeconds(5);

            translateYAnimation.Source = PathAnimationSource.Y;

            Storyboard.SetTargetName(translateYAnimation, "AnimatedTranslateTransform");
            Storyboard.SetTargetProperty(translateYAnimation,
                new PropertyPath(TranslateTransform.YProperty));

            Storyboard pathAnimationStoryboard = new Storyboard();
            pathAnimationStoryboard.Children.Add(translateXAnimation);
            pathAnimationStoryboard.Children.Add(translateYAnimation);

            return pathAnimationStoryboard;
        }
    }
}
