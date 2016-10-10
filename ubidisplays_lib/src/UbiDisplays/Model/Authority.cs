using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace UbiDisplays.Model
{
    /// <summary>
    /// The Authority class is responsible for managing the surfaces, displays, spatial regions etc which are
    /// available to the components of this toolkit.
    /// </summary>
    /// <remarks>It is an abstract class because there is only one authority for all displays and surfaces.</remarks>
    public abstract class Authority
    {

        #region Properties
        /// <summary>
        /// The log source string for this authority.
        /// </summary>
        public const String AUTHORITY_LOG_SOURCE = "Authority";

        /// <summary>
        /// Mutex to make safe the adding and removal of surfaces, displays etc.
        /// </summary>
        private static Mutex mControl = new Mutex();

        /// <summary>
        /// A list of surfaces available to the authority.
        /// </summary>
        private static List<Surface> _Surfaces = new List<Surface>();

        /// <summary>
        /// A list of displays available to the authority.
        /// </summary>
        private static List<Display> _ActiveDisplays = new List<Display>();

        /// <summary>
        /// Returns a read only wrapper of all the active surfaces.
        /// </summary>
        public static IEnumerable<Surface> Surfaces
        {
            get { return _Surfaces.AsReadOnly(); }
        }

        /// <summary>
        /// Return a read only wrapper of all the active displays.
        /// </summary>
        public static IEnumerable<Display> Displays
        {
            get { return _ActiveDisplays.AsReadOnly(); }
        }

        /// <summary>
        /// An event which is called when the surface list changes.
        /// </summary>
        public static event Action OnSurfaceListChanged;
        #endregion

        #region Surface Managment
        /// <summary>
        /// Register a surface to this authority.
        /// </summary>
        /// <param name="pSurface">The surface object to register.</param>
        /// <returns>The surface that was registered.</returns>
        public static Surface RegisterSurface(Surface pSurface)
        {
            // Acquire the mutex.
            mControl.WaitOne();

            // Confirm we have a valid UUID within the authority.
            foreach (var s in _Surfaces)
            {
                if (s == pSurface)
                {
                    mControl.ReleaseMutex();
                    Log.Write("Surface '" + pSurface.Identifier + "' is already registerd.", AUTHORITY_LOG_SOURCE, Log.Type.DisplayWarning);
                    throw new Exception("Surface is already registered to the authority.");
                }
                if (s.Identifier.Equals(pSurface.Identifier))
                {
                    mControl.ReleaseMutex();
                    Log.Write("Surface with name '" + pSurface.Identifier + "' already exists.", AUTHORITY_LOG_SOURCE, Log.Type.DisplayWarning);
                    throw new Exception("Surface name already registered.");
                }
            }

            // Add it to the list.
            _Surfaces.Add(pSurface);

            // Say its ok.
            Log.Write("Surface '" + pSurface.Identifier + "' registered succesfully.", AUTHORITY_LOG_SOURCE, Log.Type.AppInfo);

            // Release the mutex.
            mControl.ReleaseMutex();

            // Raise the surface list changed event.
            if (OnSurfaceListChanged != null)
                OnSurfaceListChanged();

            // Return it cos we are done.
            return pSurface;
        }

        /// <summary>
        /// Unregister a surface from an authority and delete it.
        /// </summary>
        /// <remarks>This will also delete any display currently active on the surface.</remarks>
        /// <param name="pSurface">A reference to the surface object.</param>
        /// <returns>The surface that was removed.  Null if nothing happened.</returns>
        public static void DeleteSurface(Surface pSurface)
        {
            // Acquire the mutex.
            mControl.WaitOne();

            // Try to remove it.  Bail on failure.
            if (!_Surfaces.Remove(pSurface))
            {
                mControl.ReleaseMutex();
                return;
            }

            // Tell it to remove its display (if it has one).
            var pDisplay = pSurface.ActiveDisplay;
            if (pDisplay != null)
            {
                // Delete the display (calls Authority.DeleteDisplay(pDisplay); internally).
                pDisplay.Delete();
            }

            // Delete the surface.
            pSurface.Authority_Delete();
            
            // Say its ok.
            Log.Write("Surface '" + pSurface.Identifier + "' unregistered succesfully.", AUTHORITY_LOG_SOURCE, Log.Type.AppInfo);

            // Release the mutex.
            mControl.ReleaseMutex();

            // Raise the surface list changed event.
            if (OnSurfaceListChanged != null)
                OnSurfaceListChanged();
        }

        /// <summary>
        /// Look up a surface by name.
        /// </summary>
        /// <param name="Identifier">The name of the surface to locate.</param>
        /// <returns>The surface that was located.  If no surface matches, null is returned.</returns>
        public static Surface FindSurface(String Identifier)
        {
            // Acquire the mutex.
            //mControl.WaitOne();

            // Confirm we have a valid UUID within the authority.
            var pSurface = _FindSurface(Identifier);

            // Release the mutex.
            //mControl.ReleaseMutex();

            // Return null if one was not found.
            return pSurface;
        }

        /// <summary>
        /// Look up a surface by name.
        /// </summary>
        /// <remarks>This version does not wait for a lock.</remarks>
        /// <param name="Identifier">The name of the surface to locate.</param>
        /// <returns>The surface that was located.  If no surface matches, null is returned.</returns>
        private static Surface _FindSurface(String Identifier)
        {
            // Check we have a good surface id.
            if (Identifier == "")
                return null;

            // Confirm we have a valid UUID within the authority.
            var lSurface = Surfaces;
            foreach (var s in lSurface)
            {
                if (s.Identifier == Identifier)
                {
                    return s;
                }
            }

            // Return null if one was not found.
            return null;
        }

        /// <summary>
        /// Change the name of an existing surface.
        /// </summary>
        /// <param name="sCurrentIdentifier">The current name of the surface.  Used to find the surface to change.</param>
        /// <param name="sNewIdentifier">The new name of the surface.</param>
        /// <returns>True if the name change was successful, false if not.</returns>
        public static bool RenameSurface(String sCurrentIdentifier, String sNewIdentifier)
        {
            // Acquire the mutex.
            mControl.WaitOne();

            // Find the first surface.
            var pTarget = _FindSurface(sCurrentIdentifier);
            if (pTarget == null)
            {
                mControl.ReleaseMutex();
                return false;
            }

            // If the other one exists already.
            if (_FindSurface(sNewIdentifier) != null)
            {
                mControl.ReleaseMutex();
                return false;
            }

            // Update the name, and boom, done!
            pTarget.Authority_SetIdentifier(sNewIdentifier);

            // Release the mutex.
            mControl.ReleaseMutex();

            // Raise the surface list changed event.
            //if (OnSurfaceListChanged != null)
            //    OnSurfaceListChanged();
            
            return true;
        }
        #endregion

        #region Display Management
        /// <summary>
        /// Show a display on a given surface.
        /// </summary>
        /// <remarks>This will throw exceptions if this is not possible.  Ensure neither the display is already open or the surface is occupied.</remarks>
        /// <param name="pDisplay">The display to show.</param>
        /// <param name="pSurface">The surface to show it on.</param>
        public static void ShowDisplay(Display pDisplay, Surface pSurface)
        {
            // Check we have good data.
            if (pDisplay == null) throw new ArgumentNullException("Cannot show null display.");
            if (pSurface == null) throw new ArgumentNullException("Cannot show display on a null surface.");
            if (pDisplay.IsDeleted()) throw new ArgumentNullException("Cannot show a deleted display.");
            if (pSurface.IsDeleted()) throw new ArgumentNullException("Cannot show display on a deleted surface.");

            // Check neither the display or surface are occupied.
            if (pDisplay.ActiveSurface != null) throw new Exception("Cannot show this display because it is already active somewhere else.");
            if (pSurface.ActiveDisplay != null) throw new Exception("Cannot show a display on this surface because it is currently occupied.");

            // Attach it to the surface.
            pSurface.Authority_AttachDisplay(pDisplay);
            Authority._ActiveDisplays.Add(pDisplay);

            // Write a log message.
            Log.Write("Attached display '"+pDisplay.ToString()+"' to surface '"+pSurface.Identifier+"'", AUTHORITY_LOG_SOURCE, Log.Type.DisplayInfo);
        }

        /// <summary>
        /// Move a display from one surface to another.
        /// </summary>
        /// <param name="pDisplay">The display we want to move.</param>
        /// <param name="pNewSurface">The surface we want to move it too.</param>
        public static void MoveDisplay(Display pDisplay, Surface pNewSurface)
        {
            // Check we have good data.
            if (pDisplay == null) throw new ArgumentNullException("Cannot move a null display.");
            if (pNewSurface == null) throw new ArgumentNullException("Cannot move display to a null surface.");
            if (pDisplay.IsDeleted()) throw new ArgumentNullException("Cannot move a deleted display.");
            if (pNewSurface.IsDeleted()) throw new ArgumentNullException("Cannot move display to a deleted surface.");

            // Drop if the new and old are the same.
            if (pDisplay.ActiveSurface == pNewSurface)
                return;
            
            // Check we can move to the new surface.
            if (pNewSurface.ActiveDisplay != null)
                throw new Exception("Cannot move a display to the new surface because it is currently occupied.");

            // Remove the view from the current surface.
            var pOldSurface = pDisplay.ActiveSurface;
            if (pOldSurface != null)
                pDisplay.ActiveSurface.Authority_DetachDisplay(pDisplay);

            // Attach it to the new one.
            pNewSurface.Authority_AttachDisplay(pDisplay);

            // Write a log message.
            Log.Write("Display moved from '" + pOldSurface.ToString()+ "' to '"+pNewSurface.ToString()+"'.", AUTHORITY_LOG_SOURCE, Log.Type.DisplayInfo);
        }

        /// <summary>
        /// Remove a display from its current surface. Use with care.
        /// </summary>
        /// <remarks>N.B You must call 'pDisplay.Delete()' when you are finished with it if you don't put it back on a surface.</remarks>
        /// <param name="pDisplay">The display we want to remove from its surface.</param>
        public static void RemoveDisplay(Display pDisplay)
        {
            // Check we have good data.
            if (pDisplay == null) throw new ArgumentNullException("Cannot remove a null display.");
            if (pDisplay.IsDeleted()) throw new ArgumentNullException("Cannot remove a deleted display.");

            // Remove the view from the current surface.
            var pOldSurface = pDisplay.ActiveSurface;
            if (pOldSurface != null)
                pDisplay.ActiveSurface.Authority_DetachDisplay(pDisplay);

            // Remove it from the list of displays.
            Authority._ActiveDisplays.Remove(pDisplay);

            // Write a log message.
            Log.Write("Display '"+pDisplay.ToString()+"' removed.", AUTHORITY_LOG_SOURCE, Log.Type.DisplayInfo);
        }

        /// <summary>
        /// Delete a display.
        /// </summary>
        /// <param name="pDisplay">The display we want to delete.</param>
        public static void DeleteDisplay(Display pDisplay)
        {
            // Check we have good data.
            if (pDisplay == null) throw new ArgumentNullException("Cannot delete a null display.");
            if (pDisplay.IsDeleted()) throw new ArgumentNullException("Cannot delete a deleted display.");

            // Remove it from its surface, if attached.
            var pSurface = pDisplay.ActiveSurface;
            if (pSurface != null)
            {
                pSurface.Authority_DetachDisplay(pDisplay);
            }

            // Release any resources created for it.
            pDisplay.Authority_Delete();

            // Remove it from the list of displays.
            Authority._ActiveDisplays.Remove(pDisplay);

            // Log the happening.  That is a weird film btw.
            Log.Write("Display '"+pDisplay.ToString()+"' deleted.", AUTHORITY_LOG_SOURCE, Log.Type.DisplayInfo);
        }
        #endregion

        #region Request Processing
        /// <summary>
        /// The Authority API object name.
        /// </summary>
        public const String APIObject_Authority = "Authority";

        /// <summary>
        /// The Surface API object name.
        /// </summary>
        public const String APIObject_Surface = "Surface";

        /// <summary>
        /// The Display API object name.
        /// </summary>
        public const String APIObject_Display = "Display";

        /// <summary>
        /// The Authority.request function name.
        /// </summary>
        public const String APIObject_AuthorityRequest = "request";

        /// <summary>
        /// The Authority.call function name.
        /// </summary>
        public const String APIObject_AuthorityCall = "call";

        /// <summary>
        /// The Authority.log function name.
        /// </summary>
        public const String APIObject_AuthorityLog = "log";

        /// <summary>
        /// Thread saftey for the request table.
        /// </summary>
        private static Mutex mHandlerLock = new Mutex();

        /// <summary>
        /// The table of request handlers.
        /// </summary>
        private static Dictionary<String, DisplayAPI.IRequest> dRequestHandlers = new Dictionary<String, DisplayAPI.IRequest>();

        /// <summary>
        /// Process a display request.  For example the calling JS would look like: Authority.request(handlername, somedata).
        /// </summary>
        /// <param name="pDisplay">The display which called this api function.</param>
        /// <param name="pSurface">The surface which this display is hosted on.</param>
        /// <param name="sRequestHandler">The name of the request handler.</param>
        /// <param name="dArguments">The table of arguments which were given in the data parameter.</param>
        /// <returns>True if the request was sucessfully handled.  False if not.</returns>
        public static bool ProcessRequest(Display pDisplay, Surface pSurface, String sRequestHandler)
        {
            // Check the display and surface are valid.
            if (pDisplay == null) throw new ArgumentNullException("Cannot process a display API request without a display.");
            if (pSurface == null) throw new ArgumentNullException("Cannot process a display API request without a surface.");

            // Check we have a valid request handler.
            if (sRequestHandler == null || sRequestHandler.Length == 0)
            {
                Log.Write("Cannot process a display API request without a handler name.", pDisplay.ToString(), Log.Type.DisplayWarning);
                return false;
            }

            // Make the request handler lower case.
            sRequestHandler = sRequestHandler.ToLower();

            // Search the bound request handlers.
            DisplayAPI.IRequest pHandler = null;
            if (dRequestHandlers.TryGetValue(sRequestHandler, out pHandler))
            {
                // If one is found, process the request and return the success condition.
                return pHandler.ProcessRequest(pDisplay, pSurface);
            }

            // No handler for request.
            Log.Write("Authority could not find handler for request '" + sRequestHandler + "'.", pDisplay.ToString(), Log.Type.DisplayWarning);
            return false;
        }

        /// <summary>
        /// Process a remote method invocation request from one display to another hosted on another surface.
        /// </summary>
        /// <param name="pDisplay">The display which called this api function.</param>
        /// <param name="pSurface">The surface which this display is hosted on.</param>
        /// <param name="sTargetSurface">The target surface which contains the display we want to call the function on.</param>
        /// <param name="sRemoteFunction">The name of the function on the active display on the target surface we want to call.</param>
        /// <param name="lArguments">A list of arguments to pass to that function.</param>
        /// <returns></returns>
        public static bool ProcessRMICall(Display pDisplay, Surface pSurface, String sTargetSurface, String sRemoteFunction)
        {
            // Check the display and surface are valid.
            if (pDisplay == null) throw new ArgumentNullException("Cannot process a display API request without a display.");
            if (pSurface == null) throw new ArgumentNullException("Cannot process a display API request without a surface.");

            // Check we have a valid request handler.
            if (sTargetSurface == null || sTargetSurface.Length == 0)
            {
                Log.Write("Cannot process a cross-surface RMI call without a target surface name.", pDisplay.ToString(), Log.Type.DisplayWarning);
                return false;
            }

            // Check for a valid remote function.
            if (sRemoteFunction == null || sRemoteFunction.Length == 0)
            {
                Log.Write("Cannot process a cross-surface RMI call without a target function name.", pDisplay.ToString(), Log.Type.DisplayWarning);
                return false;
            }

            // Attempt to find the target surface.
            var pTargetSurface = Authority.FindSurface(sTargetSurface);
            if (pTargetSurface == null)
            {
                Log.Write("Surface '"+sTargetSurface+"' not found.  Cannot process a cross-surface RMI call without a valid target surface.", pDisplay.ToString(), Log.Type.DisplayWarning);
                return false;
            }

            // Check there is a display on the surface.
            if (pTargetSurface.ActiveDisplay != null)
            {
                return true;
            }

            // Otherwise we didn't do it.
            Log.Write("Could not process a cross-surface RMI call because the target surface did not have an active display.", pDisplay.ToString(), Log.Type.DisplayWarning);
            return false;
        }

        /// <summary>
        /// Register a handler name with a new handler instance.
        /// </summary>
        /// <param name="sHandlerName">The name of the request handler we want the API to use as a look up.</param>
        /// <param name="kHandlerInstance">The instance responsible for handling all these requests.</param>
        /// <returns>The normalised name of the handler.  This is practically just a lower case version of sHandlerName.</returns>
        /// <remarks>This is threadsafe.</remarks>
        public static String RegisterRequestHandler(String sHandlerName, DisplayAPI.IRequest kHandlerInstance)
        {
            // Check we have valid data.
            if (sHandlerName == null || sHandlerName == "") throw new ArgumentNullException("Invalid handler name.");

            // Transform the handler into lower case for the lookups.
            sHandlerName = sHandlerName.ToLower();

            // Add us to the table of registered items.
            mHandlerLock.WaitOne();
            dRequestHandlers[sHandlerName] = kHandlerInstance;
            mHandlerLock.ReleaseMutex();

            // Return the name.
            return sHandlerName;
        }

        /// <summary>
        /// Remove a request handler name and instance.
        /// </summary>
        /// <param name="sHandlerName">The name of the handler we want to remove.</param>
        /// <returns>True if it was removed, false if not because it did not exist.</returns>
        /// <remarks>This is threadsafe.</remarks>
        public static bool DeleteRequestHandler(String sHandlerName)
        {
            // Check we have valid data.
            if (sHandlerName == null || sHandlerName == "") return false;

            // Try and remove it from the list.
            mHandlerLock.WaitOne();
            var bResult = dRequestHandlers.Remove(sHandlerName);
            mHandlerLock.ReleaseMutex();
            return bResult;
        }

        /// <summary>
        /// Hackily load the plugins which will be used Authority.request().
        /// </summary>
        /// <remarks>Each IRequest class in UbiDisplays.Model.DisplayAPI which inherits the IRequest interface will have its HandleName property accessed.</remarks>
        public static void LoadPlugins()
        {
            // Load all the plugins in the authority.
            var tTypes = System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(
                t => String.Equals(t.Namespace, "UbiDisplays.Model.DisplayAPI", StringComparison.Ordinal)
                ).ToArray();

            // For each type we found.
            for (int i = 0; i < tTypes.Length; ++i)
            {
                // Skip it if it is not an IRequest.
                if (!tTypes[i].GetInterfaces().Contains(typeof(UbiDisplays.Model.DisplayAPI.IRequest)))
                    continue;

                // Try to load up the HandleName.
                var pField = tTypes[i].GetField("HandleName");

                // If it is null, skip it.
                if (pField == null)
                {
                    Log.Write("IRequest missing HandleName: " + tTypes[i].Name + ".", "Application", Log.Type.AppError);
                    continue;
                }

                // Say we have it.
                try
                {
                    Log.Write("IRequest: " + tTypes[i].Name + " loaded for '" + ((String)pField.GetValue(null)) + "'.", "Application", Log.Type.AppInfo);
                }
                catch (Exception)
                {
                    Log.Write("IRequest Error: " + tTypes[i].Name + ".", "Application", Log.Type.AppError);
                }
            }
        }
        #endregion
    }
}
