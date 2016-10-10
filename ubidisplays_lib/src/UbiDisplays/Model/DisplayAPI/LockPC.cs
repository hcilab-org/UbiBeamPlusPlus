using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace UbiDisplays.Model.DisplayAPI
{
    /// <summary>
    /// Lock this computer.
    /// </summary>
    /// <remarks>This is used for ALL displays.  Ensure any state is present within the ProcessRequest function and does not last longer than that.</remarks>
    public class LockPC : IRequest
    {
        [DllImport("user32")]
        public static extern void LockWorkStation();

        /// <summary>
        /// The name of this handler.
        /// </summary>
        /// <remarks>This also registers this class instance with the authority.</remarks>
        public readonly static String HandleName = Authority.RegisterRequestHandler("lockpc", new LockPC());

        /// <summary>
        /// Handle a request.
        /// </summary>
        /// <param name="pDisplay">The display which called this function.</param>
        /// <param name="pSurface">The surface which this display is hosted on.</param>
        /// <param name="dArguments">A dictionary of arguments which are passed to the function as parameters.</param>
        /// <returns>True if the request was processed sucessfully.  False if there was an error.</returns>
        public bool ProcessRequest(Display pDisplay, Surface pSurface)
        {
            try
            {
                LockWorkStation();
                return true;
            }
            catch
            {
                Log.Write("Error locking workstation.", pDisplay.ToString(), Log.Type.DisplayWarning);
                return false;
            }
        }
    }
}
