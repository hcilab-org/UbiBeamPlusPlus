using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.DisplayAPI
{
	/// <summary>
	/// Open a display on another surface.
	/// </summary>
	/// <remarks>This is used for ALL displays.  Ensure any state is present within the ProcessRequest function and does not last longer than that.</remarks>
	public class OpenDisplay : IRequest
	{
		/// <summary>
		/// The name of this handler.
		/// </summary>
		/// <remarks>This also registers this class instance with the authority.</remarks>
		public static String HandleName = Authority.RegisterRequestHandler("opendisplay", new OpenDisplay());

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
			string sLoad = null; //dArguments.GetValueOrDefault("load", "");
			if (sLoad == null || sLoad == "")
			{
				Log.Write("Cannot open display.  Missing valid 'load' parameter. e.g. 'http://mysite.com'", pDisplay.ToString(), Log.Type.DisplayWarning);
				return false;
			}

			// Find the new surface.
			var pTargetSurface = Authority.FindSurface(""); //dArguments.GetValueOrDefault("target", ""));
			if (pTargetSurface == null)
			{
				Log.Write("Cannot open display on target surface.  Missing valid 'target' parameter.", pDisplay.ToString(), Log.Type.DisplayWarning);
				return false;
			}

			// Do we want to override if occupied?
			var bOverride = false; //dArguments.GetValueOrDefault("override", false);

			// If a display is already on the target surface.
			if (pTargetSurface.ActiveDisplay != null)
			{
				// Do we close it?
				if (bOverride)
				{
					Authority.DeleteDisplay(pTargetSurface.ActiveDisplay);
				}

				// Or do we respect it's right to life!
				else
				{
					Log.Write("Cannot open display on target surface.  Surface is already occupied.", pDisplay.ToString(), Log.Type.DisplayWarning);
					return false;
				}
			}

			// Create the new display.
			Authority.ShowDisplay(new Display(sLoad), pTargetSurface);
			return true;
		}
	}
}
