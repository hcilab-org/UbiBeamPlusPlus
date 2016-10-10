using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.DisplayAPI
{
	/// <summary>
	/// Close the display on a given surface.  NOTE: This will actually delete the display and free up all its resources etc.
	/// </summary>
	/// <remarks>This is used for ALL displays.  Ensure any state is present within the ProcessRequest function and does not last longer than that.</remarks>
	public class CloseTargetDisplay : IRequest
	{
		/// <summary>
		/// The name of this handler.
		/// </summary>
		/// <remarks>This also registers this class instance with the authority.</remarks>
		public static String HandleName = Authority.RegisterRequestHandler("closetargetdisplay", new CloseTargetDisplay());

		/// <summary>
		/// Handle a request.
		/// </summary>
		/// <param name="pDisplay">The display which called this function.</param>
		/// <param name="pSurface">The surface which this display is hosted on.</param>
		/// <param name="dArguments">A dictionary of arguments which are passed to the function as parameters.</param>
		/// <returns>True if the request was processed sucessfully.  False if there was an error.</returns>
		public bool ProcessRequest(Display pDisplay, Surface pSurface)
		{
			/*
			// Find the new surface.
			var pTargetSurface = Authority.FindSurface(dArguments.GetValueOrDefault("target", ""));
			if (pTargetSurface == null)
			{
				Log.Write("Cannot close display on target surface.  Missing valid 'target' parameter.", pDisplay.ToString(), Log.Type.DisplayWarning);
				return false;
			}
			
			// Close it, if open.
			if (pTargetSurface.ActiveDisplay != null)
			{
				Authority.DeleteDisplay(pTargetSurface.ActiveDisplay);
				return true;
			}
			*/
			// Return false, nothing to do.
			return false;
		}
	}
}
