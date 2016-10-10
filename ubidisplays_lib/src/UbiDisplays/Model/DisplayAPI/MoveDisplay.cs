using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.DisplayAPI
{
	/// <summary>
	/// Move this display to another surface.
	/// </summary>
	/// <remarks>This is used for ALL displays.  Ensure any state is present within the ProcessRequest function and does not last longer than that.</remarks>
	public class MoveDisplay : IRequest
	{
		/// <summary>
		/// The name of this handler.
		/// </summary>
		/// <remarks>This also registers this class instance with the authority.</remarks>
		public static String HandleName = Authority.RegisterRequestHandler("movedisplay", new MoveDisplay());

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
			var pTargetSurface = Authority.FindSurface("");//dArguments.GetValueOrDefault("target", ""));
			if (pTargetSurface == null)
			{
				Log.Write("Cannot move display to target surface.  Missing valid 'target' parameter.", pDisplay.ToString(), Log.Type.DisplayWarning);
				return false;
			}

			// Check the surface this view is on is not our target.
			if (pTargetSurface == pDisplay.ActiveSurface)
			{
				Log.Write("Cannot move display to target surface because it is already there.", pDisplay.ToString(), Log.Type.DisplayWarning);
				return false;
			}

			// If the new surface is occupied, bail.
			if (pTargetSurface.ActiveDisplay != null)
			{
				Log.Write("Cannot move display to target surface because it already has a display on it.", this.ToString(), Log.Type.DisplayWarning);
				return false;
			}

			// Do we want to force a reload of everything with the move.
			var bForceReload = false; // dArguments.GetValueOrDefault("force_reload", false);

			// If we want to force a reload.
			if (bForceReload)
			{
				Authority.DeleteDisplay(pDisplay);
				Authority.ShowDisplay(new Display(pDisplay.LoadInstruction, pDisplay.RenderResolution), pTargetSurface);
				return true;
			}

			// If not.
			else
			{
				// Just detach it from one and move to the other.
				Authority.MoveDisplay(pDisplay, pTargetSurface);
				return true;
			}
		}
	}
}
