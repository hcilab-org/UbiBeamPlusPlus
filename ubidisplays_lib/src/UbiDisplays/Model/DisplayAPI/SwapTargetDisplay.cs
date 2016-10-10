using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.DisplayAPI
{
    /// <summary>
    /// Swap a display on a target surface with another target surface.
    /// </summary>
    /// <remarks>This is used for ALL displays.  Ensure any state is present within the ProcessRequest function and does not last longer than that.</remarks>
    public class SwapTargetDisplay : IRequest
    {
        /// <summary>
        /// The name of this handler.
        /// </summary>
        /// <remarks>This also registers this class instance with the authority.</remarks>
        public static String HandleName = Authority.RegisterRequestHandler("swaptargetdisplay", new SwapTargetDisplay());

        /// <summary>
        /// Handle a request.
        /// </summary>
        /// <param name="pDisplay">The display which called this function.</param>
        /// <param name="pSurface">The surface which this display is hosted on.</param>
        /// <param name="dArguments">A dictionary of arguments which are passed to the function as parameters.</param>
        /// <returns>True if the request was processed sucessfully.  False if there was an error.</returns>
        public bool ProcessRequest(Display pDisplay, Surface pSurface)
        {
            // Get the first target.
            var pTarget1 = Authority.FindSurface(""); //dArguments.GetValueOrDefault("target1", ""));
            if (pTarget1 == null)
            {
                Log.Write("SwapTargetDisplay: Missing valid 'target1' parameter.", pDisplay.ToString(), Log.Type.DisplayWarning);
                return false;
            }

            // Get the first target.
            var pTarget2 = Authority.FindSurface(""); //dArguments.GetValueOrDefault("target2", ""));
            if (pTarget2 == null)
            {
                Log.Write("SwapTargetDisplay: Missing valid 'target2' parameter.", pDisplay.ToString(), Log.Type.DisplayWarning);
                return false;
            }

            // Check the surface this view is on is not our target.
            if (pTarget1 == pTarget2)
            {
                Log.Write("SwapTargetDisplay: Surface targets are the same.", pDisplay.ToString(), Log.Type.DisplayWarning);
                return false;
            }

            // Store references to the displays.
            Display pDisplay1 = pTarget1.ActiveDisplay;
            Display pDisplay2 = pTarget2.ActiveDisplay;

            // Remove them both from surfaces.
            if (pDisplay1 != null) Authority.RemoveDisplay(pDisplay1);
            if (pDisplay2 != null) Authority.RemoveDisplay(pDisplay2);

            // Re-open them on the other surfaces.
            if (pDisplay1 != null) Authority.ShowDisplay(pDisplay1, pTarget2);
            if (pDisplay2 != null) Authority.ShowDisplay(pDisplay2, pTarget1);

            // Boom.
            return true;
        }
    }
}
