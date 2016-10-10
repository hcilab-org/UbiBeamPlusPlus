using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;

namespace UbiDisplays.Model
{
	/// <summary>
	/// The Display class represents (web) content which can be shown on a surface.
	/// </summary>
	/// <remarks>It also manages the rendering of that content (its creation, deletion, etc) and the interface to the API methods.</remarks>
	[Serializable()]
	public class Display : ResourceOwner, IResource
	{
		#region Properties
		/// <summary>
		/// The instruction which loads or unloads the view web link.
		/// </summary>
		public String LoadInstruction { get; set; }

		/// <summary>
		/// This is the current title (i.e. the html 'title' element).
		/// </summary>
		//[NonSerialized]
		public String Title { get; internal set; }

		/// <summary>
		/// A reference to the active surface this view is bound too.
		/// </summary>
		//[NonSerialized]
		public Surface ActiveSurface { get; private set; }

		/// <summary>
		/// Return a reference to the web view which is currently rendering this display.
		/// </summary>
		//[NonSerialized]
		protected Native.NativeDisplay ActiveControl { get; private set; }

		/// <summary>
		/// Get the visual used for rendering.
		/// </summary>
		//[NonSerialized]
		public UIElement Visual
		{
			get
			{
				if (this.ActiveControl == null)
					throw new Exception("Cannot access display visual because it is not yet created.  Did you call Surface_BindToSurface?");
				return this.ActiveControl;
			}
		}

		/// <summary>
		/// Get or set the render resolution for this display.
		/// </summary>
		//[NonSerialized]
		public Point RenderResolution
		{
			get
			{
				return _RenderResolution;
			}
			set
			{
				// Sanity check.
				if (value.X < 2 || value.Y < 2)
					throw new Exception("Bad render resolution.  Must be greater than 2x2.");

				// Store the value.
				_RenderResolution = value;

				// Push an update to the surface if active.  If not it will be done when next created.
				if (ActiveControl != null)
				{
					ActiveControl.Width = value.X;
					ActiveControl.Height = value.Y;
				}
				if (ActiveSurface != null)
				{
					ActiveSurface.Display_SetVisualRenderSize(new Size(value.X, value.Y));
				}
			}
		}
		/// <summary>
		/// The internal render resolution.
		/// </summary>
		private Point _RenderResolution = new Point(800, 600);
		#endregion

		#region Constructor
		/// <summary>
		/// Create a new display.
		/// </summary>
		/// <param name="sLoadInstruction">The load instruction. e.g. http://google.com</param>
		public Display(String sLoadInstruction)
		{
			// The load instruction.
			LoadInstruction = sLoadInstruction;

			// The render resolution.
			RenderResolution = new Point(800, 600);
		}

		/// <summary>
		/// Create a new display.
		/// </summary>
		/// <param name="sLoadInstruction">The load instruction. e.g. http://google.com</param>
		/// <param name="tRenderResolution">The render resolution for this display.</param>
		public Display(String sLoadInstruction, Point tRenderResolution)
			: this(sLoadInstruction)
		{
			// Base class.
			this.RenderResolution = tRenderResolution;
		}
		#endregion

		#region Surface Control Functions
		/// <summary>
		/// Called by a surface to signal that this display has attached to it.
		/// </summary>
		/// <remarks>Here it should create the visual to be shown (if not already existing) and place it on the surface by calling: pSurface.Display_SetVisual(visual).</remarks>
		/// <param name="pSurface">The surface which this display is about to be shown on.</param>
		public void Surface_BindToSurface(Surface pSurface)
		{
			// Error checking.
			if (pSurface == null)
				throw new ArgumentNullException("Cannot bind to a null surface.");

			// If we already have an active surface.
			if (ActiveSurface != null)
				throw new Exception("Display is already bound to a surface.");

			// Store the surface reference.
			ActiveSurface = pSurface;

			// Create the visual to go on the surface.
			ActiveControl = new Native.NativeDisplay(); //CreateRenderable();
			ActiveSurface.Display_SetVisual(ActiveControl);
			ActiveSurface.Display_SetVisualRenderSize(new Size(ActiveControl.Width, ActiveControl.Height));

			// Tell the JS that we have been attached to a surface (i.e. its properties have changed).
			//this.SignalSurfacePropertiesChanged();

			// Bind to spatial update events.
			pSurface.OnSurfacePropertiesUpdated += Surface_OnSpatialPropertiesUpdated;
		}

		/// <summary>
		/// Called by a surface to signal that this display should detach from it.
		/// </summary>
		/// <remarks>Here it could destroy the visual to be shown and remove it from the surface by calling: pSurface.Display_SetVisual(null).</remarks>
		/// <param name="pSurface"></param>
		public void Surface_UnbindFromSurface(Surface pSurface)
		{
			// Error checking.
			if (pSurface == null)
				throw new ArgumentNullException("Cannot unbind from a null surface.");

			// Check the surface is our active one.
			if (pSurface != ActiveSurface)
				throw new Exception("Surface to deatch from and stored surface do not match.");

			// Unbind from spatial update events.
			ActiveSurface.OnSurfacePropertiesUpdated -= Surface_OnSpatialPropertiesUpdated;

			// Remove the renderable from the surface.
			ActiveSurface.Display_SetVisual(null);
			ActiveSurface.Display_ResetVisualRenderSize();

			// Destroy the renderable.
			//  n.b. With a few small changes (i.e. not calling this) we could preserve the WebView while not attached to a surface.
			//       this would be good for moving displays without losing state.
			//ActiveControl.Dispose();
			ActiveControl = null;

			// Remove any resources we have attached (i.e. spatial queries).
			this.DeleteResources();

			// Remove the reference to the active surface.
			ActiveSurface = null;
		}

		/// <summary>
		/// Called by the active surface when its spatial properties have been updated.
		/// </summary>
		/// <remarks>This is because we subscribe to the event when we bind/unbind.</remarks>
		private void Surface_OnSpatialPropertiesUpdated(Surface pSurface)
		{
			this.SignalSurfacePropertiesChanged();
		}
		#endregion

		/// <summary>
		/// Tell the display logic (Javascript) that there has been an update to the surface.
		/// </summary>
		public void SignalSurfacePropertiesChanged()
		{
			// If we have no surface or web control, bail.
			if (ActiveSurface == null || ActiveControl == null)
				return;

			// If the web control is not live, skip.
			//if (!ActiveControl.IsProcessCreated)
			//    return;

			/*
			// ASYNC HACK!
			var pSurfaceObject = new JSObject();
			pSurfaceObject["Name"] = new JSValue(ActiveSurface.Identifier);
			pSurfaceObject["Width"] = new JSValue(ActiveSurface.Width);
			pSurfaceObject["Height"] = new JSValue(ActiveSurface.Height);
			pSurfaceObject["AspectRatio"] = new JSValue(ActiveSurface.AspectRatio);
			pSurfaceObject["Angle"] = new JSValue(ActiveSurface.Angle);
			
			ActiveControl.ExecuteJavascript("window.Surface = "+ToJSON(pSurfaceObject)+";");
			//Log.Write("SignalSurfacePropertiesChanged", this.ToString(), Log.Type.AppError);
			*/

			// Create a surface object.
			/*using (JSObject pSurface = ActiveControl.CreateGlobalJavascriptObject(Authority.APIObject_Surface))
			{
				// Say if we couldn't create the object.
				if (pSurface == null)
				{
					Log.Write("Error setting surface properties.  Should be fixed in the next version.", this.ToString(), Log.Type.AppError);
				}

				pSurface["Name"] = new JSValue(ActiveSurface.Identifier);
				pSurface["Width"] = new JSValue(ActiveSurface.Width);
				pSurface["Height"] = new JSValue(ActiveSurface.Height);
				pSurface["AspectRatio"] = new JSValue(ActiveSurface.AspectRatio);
				pSurface["Angle"] = new JSValue(ActiveSurface.Angle);
			}*/

			ActiveControl.start(this, ActiveSurface, UbiHand.Offset, UbiHand.Height);
		}

		#region Control Visual Creation and Managment
		
		/// <summary>
		/// Return a string based representation of this display.
		/// </summary>
		/// <returns>A string representation of this display.  This is just the load instruction.</returns>
		public override string ToString()
		{
			return this.LoadInstruction;
		}

		/// <summary>
		/// Reload the display and its content.  Reloading will also delete any other resources we have.
		/// </summary>
		/// <param name="bHard">True if we want to remove and then re-create the webcontrol.  False if we just want to do a web-refresh.</param>
		/// <param name="bIgnoreCache">If this is just a web-refresh, do we want to ignore the cache.</param>
		public void Reload(bool bHard, bool bIgnoreCache = true)
		{
			// If it is active.
			if (ActiveControl == null)
				return;

			// If it is on a surface.
			if (ActiveSurface == null)
				return;

			// If it is a hard reset.
			if (bHard)
			{
				// Remove the control and re-add it.
				ActiveSurface.Display_SetVisual(null);
				//ActiveControl.Dispose();
				ActiveControl = null;
				
				this.DeleteResources();
				ActiveControl = new Native.NativeDisplay(); //CreateRenderable();
				ActiveSurface.Display_SetVisual(ActiveControl);
			}
			else
			{
				this.DeleteResources();
				//ActiveControl.Reload(bIgnoreCache);
			}
		}
		#endregion

		public void SetOffset(float offset)
		{
			if (ActiveControl != null) ActiveControl.SetOffset(offset);
		}

		public void SetHeight(float height)
		{
			if (ActiveControl != null) ActiveControl.SetHeight(height);
		}

		#region Deletion Pattern
		/// <summary>
		/// A flag which says if this display has been deleted or not.
		/// </summary>
		[NonSerialized]
		private bool bDeleted = false;

		/// <summary>
		/// An event which is raised when this is deleted.
		/// </summary>
		public event Action<IResource> OnDeleted;

		/// <summary>
		/// Call this to tell the display to delete.  This works by calling Authority.DeleteDisplay.
		/// </summary>
		public void Delete()
		{
			// Try to remove us from the authority.  This should take care of most things.
			try
			{
				Authority.DeleteDisplay(this);
			}
			catch (Exception e)
			{
				Log.Write("Error deleting display '" + this.ToString() + "'. " + e.Message, Authority.AUTHORITY_LOG_SOURCE, Log.Type.AppWarning);
			}
		}

		/// <summary>
		/// Determine if this surface has been deleted.
		/// </summary>
		public bool IsDeleted()
		{
			return bDeleted;
		}

		/// <summary>
		/// Called by the authority to signal that this surface has been deleted.
		/// </summary>
		internal void Authority_Delete()
		{
			// If we are still attached to a surface, throw an error.
			if (ActiveSurface != null)
				throw new Exception("Cannot delete display while still attached to surface.");

			// Remove the web control.
			if (ActiveControl != null)
			{
				//ActiveControl.Dispose();
				ActiveControl = null;
			}
			
			// Free up any other resources we may have created.
			this.DeleteResources();

			// And'were done - set the deleted flag.
			bDeleted = true;

			// Say we are deleted.
			if (OnDeleted != null)
				OnDeleted(this);
		}
		#endregion

		#region Helpers for Awesomium ASYNC function use.
		/// <summary>
		/// Encodes a string to be represented as a string literal. The format
		/// is essentially a JSON string.
		/// 
		/// The string returned includes outer quotes 
		/// Example Output: "Hello \"Rick\"!\r\nRock on"
		/// </summary>
		/// <remarks>Found here: http://stackoverflow.com/questions/806944/escape-quote-in-c-sharp-for-javascript-consumption </remarks>
		/// <param name="s">The string to encode.</param>
		/// <returns>The encoded string.</returns>
		private static string EncodeJsString(string s)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("\"");
			foreach (char c in s)
			{
				switch (c)
				{
					case '\"':
						sb.Append("\\\"");
						break;
					case '\\':
						sb.Append("\\\\");
						break;
					case '\b':
						sb.Append("\\b");
						break;
					case '\f':
						sb.Append("\\f");
						break;
					case '\n':
						sb.Append("\\n");
						break;
					case '\r':
						sb.Append("\\r");
						break;
					case '\t':
						sb.Append("\\t");
						break;
					default:
						int i = (int)c;
						if (i < 32 || i > 127)
						{
							sb.AppendFormat("\\u{0:X04}", i);
						}
						else
						{
							sb.Append(c);
						}
						break;
				}
			}
			sb.Append("\"");

			return sb.ToString();
		}
		#endregion
	}
}


/* // NOTE: THIS NEEDS TO BE ASYNC!
// Create and acquire a global Javascript object - this will persist for the lifetime of the web-view.
using (JSObject pSurfaceObject = ActiveControl.CreateGlobalJavascriptObject(Authority.APIObject_Surface))
{
	// Handle requests for Authority.request.
	pSurfaceObject["Name"] = new JSValue(ActiveSurface.Identifier);
	pSurfaceObject["Width"] = new JSValue(ActiveSurface.Width);
	pSurfaceObject["Height"] = new JSValue(ActiveSurface.Height);
	pSurfaceObject["AspectRatio"] = new JSValue(ActiveSurface.AspectRatio);
	pSurfaceObject["Angle"] = new JSValue(ActiveSurface.Angle);

	// Add world coordinates.
	var pWorld = new JSObject();
	pWorld["topleft"]       = MakeCoordinate(ActiveSurface.SensorSpace[Surface.TOPLEFT_INDEX]);
	pWorld["topright"]      = MakeCoordinate(ActiveSurface.SensorSpace[Surface.TOPRIGHT_INDEX]);
	pWorld["bottomleft"]    = MakeCoordinate(ActiveSurface.SensorSpace[Surface.BOTTOMLEFT_INDEX]);
	pWorld["bottomright"]   = MakeCoordinate(ActiveSurface.SensorSpace[Surface.BOTTOMRIGHT_INDEX]);
	pWorld["normal"]        = MakeCoordinate(ActiveSurface.Plane.Normal);
	pSurfaceObject["World"] = pWorld;

	// Add kinect coordinates.
	var pKinect = new JSObject();
	pKinect["topleft"]       = MakeCoordinate(ActiveSurface.KinectSpace[Surface.TOPLEFT_INDEX]);
	pKinect["topright"]      = MakeCoordinate(ActiveSurface.KinectSpace[Surface.TOPRIGHT_INDEX]);
	pKinect["bottomleft"]    = MakeCoordinate(ActiveSurface.KinectSpace[Surface.BOTTOMLEFT_INDEX]);
	pKinect["bottomright"]   = MakeCoordinate(ActiveSurface.KinectSpace[Surface.BOTTOMRIGHT_INDEX]);
	pKinect["width"]         = 320;
	pKinect["height"]        = 240;
	pSurfaceObject["Kinect"] = pKinect;
}
*/

/*
// Do the processing in another thread.
Parallel.Invoke(() =>
{
	// Get the function.
	var pJSValue = ActiveControl.ExecuteJavascriptWithResult(sFunction);
	if (pJSValue.IsObject)
	{
		// Repack the arguments.
		JSValue[] tArgs = new JSValue[tArguments.Length + 1];
		Array.Copy(tArguments, 0, tArgs, 1, tArguments.Length);
		tArgs[0] = pJSValue;

		// Invoke the function.
		var pObject = (JSObject)pJSValue;
		pObject.Invoke("call", pObject, tArguments);
	}
});

			
// Do the processing in another thread.
BackgroundWorker pWorker = new BackgroundWorker();
pWorker.DoWork += (object pSender, DoWorkEventArgs eWork) =>
{
	// Get the function.
	var pJSValue = ActiveControl.ExecuteJavascriptWithResult(sFunction);
	if (pJSValue.IsObject)
	{
		// Repack the arguments.
		JSValue[] tArgs = new JSValue[tArguments.Length + 1];
		Array.Copy(tArguments, 0, tArgs, 1, tArguments.Length);
		tArgs[0] = pJSValue;

		// Invoke the function.
		var pObject = (JSObject)pJSValue;
		pObject.Invoke("call", pObject, tArguments);
	}
};
//pWorker.RunWorkerCompleted += (object pSender, RunWorkerCompletedEventArgs eWork) =>
//{
//};
// Perform the processing.
pWorker.RunWorkerAsync();
*/