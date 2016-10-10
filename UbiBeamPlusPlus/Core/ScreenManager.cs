using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Threading;

namespace UIKinect.Core
{
        public static class ScreenManager
        {

            /// <summary>
            /// This method returns the working area of the projector
            /// </summary>
            /// <returns></returns>
            public static System.Drawing.Rectangle getProjectorResolution()
            {
                if (isSecondScreenConnected())
                {
                    //As convenience for the developer always choose non-primary Screen
                    int screenID = (System.Windows.Forms.Screen.AllScreens[0].Primary == false) ? 0 : 1;
                    return System.Windows.Forms.Screen.AllScreens[screenID].WorkingArea;
                }
                else
                {
                    return System.Windows.Forms.Screen.AllScreens[0].WorkingArea;
                }
            }

            public static bool isSecondScreenConnected()
            {
                bool ret = false;
                if (getNumberOfScreens() >= 2)
                {
                    ret = true;
                }
                return ret;
            }

            public static int getNumberOfScreens()
            {
                int numScreens = System.Windows.Forms.Screen.AllScreens.Length;
                return numScreens;
            }
    }
}
