using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.DisplayAPI
{
	/// <summary>
	/// Return information about a surface.
	/// </summary>
	/// <remarks>This is used for ALL displays.  Ensure any state is present within the ProcessRequest function and does not last longer than that.</remarks>
	public class SurfaceInfo : IRequest
	{
		/// <summary>
		/// The name of this handler.
		/// </summary>
		/// <remarks>This also registers this class instance with the authority.</remarks>
		public static String HandleName = Authority.RegisterRequestHandler("surfaceinfo", new SurfaceInfo());

		/// <summary>
		/// Handle a request.
		/// </summary>
		/// <param name="pDisplay">The display which called this function.</param>
		/// <param name="pSurface">The surface which this display is hosted on.</param>
		/// <param name="dArguments">A dictionary of arguments which are passed to the function as parameters.</param>
		/// <returns>True if the request was processed sucessfully.  False if there was an error.</returns>
		public bool ProcessRequest(Display pDisplay, Surface pSurface)
		{
			// Get the callback.
			var sCallback = ""; //dArguments.GetValueOrDefault("callback", "");
			if (sCallback == null || sCallback == "")
			{
				Log.Write("Please specify a 'callback' value.", pDisplay.ToString(), Log.Type.DisplayWarning);
				return false;
			}

			// Get the list of surface names we want to find information for.
			var lSurfaces = ""; //dArguments.GetValueOrDefault("surfaces", new JSValue[] { });
			if (lSurfaces == null || lSurfaces.Length == 0)
			{
				Log.Write("Please specify a 'surfaces' value. i.e. ['Surface 0', 'Surface 1']", pDisplay.ToString(), Log.Type.DisplayWarning);
				return false;
			}

			// Create an object to store the information about each surface.
			/*JSObject dOut = new JSObject();
			foreach (var jsSurf in lSurfaces)
			{
				// If it is a string, find the surface.
				var sSurfName = (jsSurf.IsString) ? (String)jsSurf : null;
				if (sSurfName == null)
					continue;

				// Find the surface.
				var pSurf = Authority.FindSurface(sSurfName);
				if (pSurf == null)
					continue;

				// Otherwise, get the information about it.
				var jsSurfObj = new JSObject();
				dOut[pSurf.Identifier] = jsSurfObj;

				// Surface properties.
				jsSurfObj["Name"] = new JSValue(pSurf.Identifier);
				jsSurfObj["Width"] = new JSValue(pSurf.Width);
				jsSurfObj["Height"] = new JSValue(pSurf.Height);
				jsSurfObj["AspectRatio"] = new JSValue(pSurf.AspectRatio);
				jsSurfObj["Angle"] = new JSValue(pSurf.Angle);

				// Add world coordinates.
				var pWorld = new JSObject();
				pWorld["topleft"] = JSExtensions.MakeCoordinate(pSurf.SensorSpace[Surface.TOPLEFT_INDEX]);
				pWorld["topright"] = JSExtensions.MakeCoordinate(pSurf.SensorSpace[Surface.TOPRIGHT_INDEX]);
				pWorld["bottomleft"] = JSExtensions.MakeCoordinate(pSurf.SensorSpace[Surface.BOTTOMLEFT_INDEX]);
				pWorld["bottomright"] = JSExtensions.MakeCoordinate(pSurf.SensorSpace[Surface.BOTTOMRIGHT_INDEX]);
				pWorld["normal"] = JSExtensions.MakeCoordinate(pSurf.Plane.Normal);
				jsSurfObj["World"] = pWorld;

				// Add kinect coordinates.
				var pKinect = new JSObject();
				pKinect["topleft"] = JSExtensions.MakeCoordinate(pSurf.KinectSpace[Surface.TOPLEFT_INDEX]);
				pKinect["topright"] = JSExtensions.MakeCoordinate(pSurf.KinectSpace[Surface.TOPRIGHT_INDEX]);
				pKinect["bottomleft"] = JSExtensions.MakeCoordinate(pSurf.KinectSpace[Surface.BOTTOMLEFT_INDEX]);
				pKinect["bottomright"] = JSExtensions.MakeCoordinate(pSurf.KinectSpace[Surface.BOTTOMRIGHT_INDEX]);
				pKinect["width"] = 320;
				pKinect["height"] = 240;
				jsSurfObj["Kinect"] = pKinect;
			}

			// Dispatch it back on the callback.
			pDisplay.AsyncCallGlobalFunction(sCallback, dOut);*/
			return true;
		}
	}
}
