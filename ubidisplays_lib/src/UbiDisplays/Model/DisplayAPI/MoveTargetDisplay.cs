using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.DisplayAPI
{
	/// <summary>
	/// Move a target display from one surface to another.
	/// </summary>
	/// <remarks>This is used for ALL displays.  Ensure any state is present within the ProcessRequest function and does not last longer than that.</remarks>
	public class MoveTargetDisplay : IRequest
	{
		/// <summary>
		/// The name of this handler.
		/// </summary>
		/// <remarks>This also registers this class instance with the authority.</remarks>
		public static String HandleName = Authority.RegisterRequestHandler("movetargetdisplay", new MoveTargetDisplay());

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
			var pCurrentSurface = Authority.FindSurface("");//dArguments.GetValueOrDefault("source", ""));
			if (pCurrentSurface == null)
			{
				Log.Write("Cannot move target display to target surface.  Missing valid 'source' parameter.", pDisplay.ToString(), Log.Type.DisplayWarning);
				return false;
			}

			// Get the target display.
			var pCurrentDisplay = pCurrentSurface.ActiveDisplay;
			if (pCurrentDisplay == null)
			{
				Log.Write("Cannot move target display to target surface.  Source surface has no display.", pDisplay.ToString(), Log.Type.DisplayWarning);
				return false;
			}

			// Find the new surface.
			var pTargetSurface = Authority.FindSurface("");//dArguments.GetValueOrDefault("dest", ""));
			if (pTargetSurface == null)
			{
				Log.Write("Cannot move target display to target surface.  Missing valid 'dest' parameter.", pDisplay.ToString(), Log.Type.DisplayWarning);
				return false;
			}

			// Check the surface this view is on is not our target.
			if (pTargetSurface == pCurrentSurface)
			{
				Log.Write("Cannot move target display to target surface because it is already there.", pDisplay.ToString(), Log.Type.DisplayWarning);
				return false;
			}

			// If the new surface is occupied, bail.
			if (pTargetSurface.ActiveDisplay != null)
			{
				Log.Write("Cannot move target display to target surface because it already has a display on it.", pDisplay.ToString(), Log.Type.DisplayWarning);
				return false;
			}

			// Do we want to close the display on the destination?
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
					Log.Write("Cannot move target display to target surface.  Surface is already occupied.", pDisplay.ToString(), Log.Type.DisplayWarning);
					return false;
				}
			}

			// Do we want to force a reload of everything with the move.
			var bForceReload = false; //dArguments.GetValueOrDefault("force_reload", false);

			// If we want to force a reload.
			if (bForceReload)
			{
				Authority.DeleteDisplay(pCurrentDisplay);
				Authority.ShowDisplay(new Display(pCurrentDisplay.LoadInstruction, pCurrentDisplay.RenderResolution), pTargetSurface);
				return true;
			}

			// If not.
			else
			{
				// Just detach it from one and move to the other.
				Authority.MoveDisplay(pCurrentDisplay, pTargetSurface);
				return true;
			}
		}
	}
}
