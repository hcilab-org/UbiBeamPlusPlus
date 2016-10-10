using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.DisplayAPI
{
    /// <summary>
    /// Swap this display with one on another surface.
    /// </summary>
    /// <remarks>This is used for ALL displays.  Ensure any state is present within the ProcessRequest function and does not last longer than that.</remarks>
    public class SwapDisplay : IRequest
    {
        /// <summary>
        /// The name of this handler.
        /// </summary>
        /// <remarks>This also registers this class instance with the authority.</remarks>
        public static String HandleName = Authority.RegisterRequestHandler("swapdisplay", new SwapDisplay());

        /// <summary>
        /// Handle a request.
        /// </summary>
        /// <param name="pDisplay">The display which called this function.</param>
        /// <param name="pSurface">The surface which this display is hosted on.</param>
        /// <param name="dArguments">A dictionary of arguments which are passed to the function as parameters.</param>
        /// <returns>True if the request was processed sucessfully.  False if there was an error.</returns>
        public bool ProcessRequest(Display pDisplay, Surface pSurface)
        {
            // Find the new surface.
            var pTargetSurface = Authority.FindSurface(""); //dArguments.GetValueOrDefault("target", ""));
            if (pTargetSurface == null)
            {
                Log.Write("Cannot swap display to target surface.  Missing valid 'target' parameter.", pDisplay.ToString(), Log.Type.DisplayWarning);
                return false;
            }

            // Check the surface this view is on is not our target.
            if (pTargetSurface == pDisplay.ActiveSurface)
            {
                Log.Write("Cannot swap display to target surface because it is already there.", pDisplay.ToString(), Log.Type.DisplayWarning);
                return false;
            }

            // If the target surface has a display, get a reference and remove it.
            Display pOtherView = pTargetSurface.ActiveDisplay;
            if (pOtherView != null)
                Authority.RemoveDisplay(pOtherView);

            // Remove this display from this surface and put it on the target surface.
            Authority.RemoveDisplay(pDisplay);
            Authority.ShowDisplay(pDisplay, pTargetSurface);

            // Now put the other display on the original surface.
            if (pOtherView != null)
                Authority.ShowDisplay(pOtherView, pSurface);

            // Boom.
            return true;
        }
    }
}
