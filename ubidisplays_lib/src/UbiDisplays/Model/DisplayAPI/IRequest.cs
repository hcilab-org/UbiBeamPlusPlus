using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.DisplayAPI
{
    /// <summary>
    /// Each class which defines IRequest defines functions that can be called by the Javascript running in a display.
    /// </summary>
    public interface IRequest
    {
        /// <summary>
        /// Handle a request.
        /// </summary>
        /// <param name="pDisplay">The display which called this function.</param>
        /// <param name="pSurface">The surface which this display is hosted on.</param>
        /// <param name="dArguments">A dictionary of arguments which are passed to the function as parameters.</param>
        /// <returns>True if the request was processed sucessfully.  False if there was an error.</returns>
        bool ProcessRequest(Display pDisplay, Surface pSurface);
    }
}
