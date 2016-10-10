using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Media;

namespace UbiDisplays.Model.DisplayAPI
{
	/// <summary>
	/// Play a sound at a specified file.  This is deprecated - use HTML5 audio tags where possible.
	/// </summary>
	/// <remarks>This is used for ALL displays.  Ensure any state is present within the ProcessRequest function and does not last longer than that.</remarks>
	public class PlaySound : IRequest
	{
		/// <summary>
		/// The name of this handler.
		/// </summary>
		/// <remarks>This also registers this class instance with the authority.</remarks>
		public static String HandleName = Authority.RegisterRequestHandler("playsound", new PlaySound());

		/// <summary>
		/// Handle a request.
		/// </summary>
		/// <param name="pDisplay">The display which called this function.</param>
		/// <param name="pSurface">The surface which this display is hosted on.</param>
		/// <param name="dArguments">A dictionary of arguments which are passed to the function as parameters.</param>
		/// <returns>True if the request was processed sucessfully.  False if there was an error.</returns>
		public bool ProcessRequest(Display pDisplay, Surface pSurface)
		{
			// Check we have a sound file.
			var sSound = ""; //dArguments.GetValueOrDefault("file", "");
			if (sSound == null || sSound == "")
			{
				Log.Write("Cannot play sound.  Are you missing a 'file' parameter.", pDisplay.ToString(), Log.Type.DisplayWarning);
				return false;
			}

			// Attempt to play it.
			try
			{
				SoundPlayer pSound = new SoundPlayer(sSound);
				pSound.Play();
				return true;
			}
			
			// Log warnings.
			catch (Exception e)
			{
				Log.Write("Cannot play sound. " + e.Message, pDisplay.ToString(), Log.Type.DisplayWarning);
				return false;
			}
		}
	}
}
