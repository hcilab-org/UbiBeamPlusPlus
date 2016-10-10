using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.DisplayAPI
{
    /// <summary>
    /// Return a list of surface names to the calling display.
    /// </summary>
    /// <remarks>This is used for ALL displays.  Ensure any state is present within the ProcessRequest function and does not last longer than that.</remarks>
    public class SurfaceList : IRequest
    {
        /// <summary>
        /// The name of this handler.
        /// </summary>
        /// <remarks>This also registers this class instance with the authority.</remarks>
        public static String HandleName = Authority.RegisterRequestHandler("surfacelist", new SurfaceList());

        /// <summary>
        /// Handle a request.
        /// </summary>
        /// <param name="pDisplay">The display which called this function.</param>
        /// <param name="pSurface">The surface which this display is hosted on.</param>
        /// <param name="dArguments">A dictionary of arguments which are passed to the function as parameters.</param>
        /// <returns>True if the request was processed sucessfully.  False if there was an error.</returns>
        public bool ProcessRequest(Display pDisplay, Surface pSurface)
        {
            // Check we have a function to return the results too.
            //if (!dArguments.HasProperty("callback")) throw new Exception("Missing 'callback' argument.");

            // Return a list of surface names.
            /*JSValue[] lSurfaces = (from pSurf in Authority.Surfaces select new JSValue(pSurf.Identifier)).ToArray();

            // If that function is a string.
            var jsValue = dArguments["callback"];
            if (jsValue.IsString)
            {
                pDisplay.AsyncCallGlobalFunction(jsValue.ToString(), lSurfaces);
                return true;
            }*/

            /*
            // If it is a function.
            else if (jsValue.IsObject)
            {
                ((JSObject)jsValue).Invoke("call", lSurfaces);
                return true;
            }
            */

            // Throw the error.
            throw new Exception("Unknown type specified in 'callback' argument.  Expected string.");
        }
    }
}
