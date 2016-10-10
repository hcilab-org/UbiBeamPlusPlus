using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using SlimMath;
using UbiDisplays.Utilities;

namespace UbiDisplays.Model
{
	/// <summary>
	/// The Surface class represents a region upon which it is possible to render a display.
	/// It has spatial properties and state.
	/// </summary>
	[Serializable()]
	public class Surface : ResourceOwner, IResource
	{
		[NonSerialized]
		public const int TOPLEFT_INDEX = 3;
		[NonSerialized]
		public const int BOTTOMLEFT_INDEX = 2;
		[NonSerialized]
		public const int BOTTOMRIGHT_INDEX = 1;
		[NonSerialized]
		public const int TOPRIGHT_INDEX = 0;
		
		#region App Wide (Hack for local version.. will not be here in distributed version).
		/// <summary>
		/// A reference to the render surface (as if one node exposed lots of surfaces).
		/// </summary>
		//public static Renderer ProjectionRenderer { get; set; }

		/// <summary>
		/// A reference to active the kinect processor.  Global for all surfaces.
		/// </summary>
		public static KinectProcessing KinectProcessor { get; set; }

		/// <summary>
		/// A list of spatial queries.  This is here beacause it is more app related.
		/// </summary>
		public static List<ISpatialQuery> SpatialQueries
		{
			get
			{
				return _SpatialQueries;
			}
		}
		/// <summary>
		/// The inner storage for the spatial query list.
		/// </summary>
		[NonSerialized]
		private static List<ISpatialQuery> _SpatialQueries = new List<ISpatialQuery>();
		#endregion

		#region Unique Identifier
		/// <summary>
		/// Get or set the unique name which represents this surface.
		/// </summary>
		public String Identifier
		{
			get
			{
				return sIdentifier;
			}
			set
			{
				// Try to rename the surface.
				if (!Authority.RenameSurface(sIdentifier, value))
				{
					// The rename failed..
					Log.Write("Renaming surface '" + this.sIdentifier + "' to 'value' failed.", Authority.AUTHORITY_LOG_SOURCE, Log.Type.AppWarning);
				}
			}
		}
		/// <summary>
		/// The internal storage for the unique name which represents this surface.
		/// </summary>
		/// <remarks>This is only set by calling Authority_SetIdentifier</remarks>
		private String sIdentifier = null;
		#endregion

		/// <summary>
		/// The corners of this surface in sensor space.
		/// </summary>
		public Vector3[] SensorSpace;

		/// <summary>
		/// The corners of this surface in projector space.
		/// </summary>
		public Point[] ProjectorSpace;

		/// <summary>
		/// The corners of this surface in kinect image space.
		/// </summary>
		public Point[] KinectSpace;

		#region Computed Spatial Properties
		/// <summary>
		/// Compute the aspect ration of this output display. Width over height.
		/// </summary>
		public double AspectRatio { get; private set; }

		/// <summary>
		/// The (rough) width of this surface in mm.
		/// </summary>
		public float Width { get; private set; }

		/// <summary>
		/// The (rough) height of this surface in mm.
		/// </summary>
		public float Height { get; private set; }

		/// <summary>
		/// The (rough) angle of this surface plane relative to the calibration plane.
		/// </summary>
		public float Angle { get; private set; }

		/// <summary>
		/// The plane of this surface.
		/// </summary>
		public Plane Plane { get; private set; }

		/// <summary>
		/// An event which is fired when the spatial properties (width, height, location, orientation etc) are changed.
		/// </summary>
		public event Action<Surface> OnSurfacePropertiesUpdated;
		#endregion

		/// <summary>
		/// Return a reference to the display which is currently active on this surface.  Returns null if one is not active.
		/// </summary>
		public Display ActiveDisplay { get; private set; }

		/// <summary>
		/// Do we want to attempt to inject multi-touch into all of our displays. N.B. Setting this will force a display reload. EXPERIMENTAL.
		/// </summary>
		public bool AttemptMultiTouchInject
		{
			get
			{
				return bAttemptMultiTouchInject;
			}
			set
			{
				// If the value has changed, force a re-load of any display we have.
				if (bAttemptMultiTouchInject != value)
				{
					if (ActiveDisplay != null)
						ActiveDisplay.Reload(true);
				}
				bAttemptMultiTouchInject = value;
			}
		}

		/// <summary>
		/// Internal storage for the multi-touch injection. EXPERIMENTAL.
		/// </summary>
		private bool bAttemptMultiTouchInject = false;

		/// <summary>
		/// The unique key for the display in the projected display renderer.
		/// </summary>
		private String sProjectionDisplayKey = null;

		/// <summary>
		/// A reference to the projected display (internal object in the renderer).
		/// </summary>
		//private Renderer.Display pProjectionDisplay = null;

		/// <summary>
		/// The control which we render surface content into.
		/// </summary>
		//private SurfaceView pSurfaceContent = null;

		/// <summary>
		/// Has this surface been deleted.
		/// </summary>
		private bool bDeleted = false;

		/// <summary>
		/// An event which is raised when this is deleted.
		/// </summary>
		public event Action<IResource> OnDeleted;

		/// <summary>
		/// An event which is raised once this surface has changed display.
		/// </summary>
		public event Action<Surface> OnDisplayChanged;

		/// <summary>
		/// Show or hide the debug image.
		/// </summary>
		public bool ShowDebug
		{
			get
			{
				return true; //pSurfaceContent.ShowDebug;
			}
			set
			{
				//pSurfaceContent.ShowDebug = value;
			}
		}

		/// <summary>
		/// Get or set the opacity of the content control.
		/// </summary>
		public double ContentOpacity
		{
			get
			{
				return 1.0; //pSurfaceContent.Opacity;
			}
			set
			{
				//pSurfaceContent.Opacity = value;
			}
		}


		#region Constructor
		/// <summary>
		/// Construct a new surface.
		/// </summary>
		/// <param name="sIdentifier">The unique identifier for this surface.  Cannot be null.  If this is not unique, it will not be checked until the surface is registered with an authority.</param>
		public Surface(String sIdentifier)
		{
			// If the identifier is not valid, throw an error.
			if (sIdentifier == null || sIdentifier.Length == 0)
				throw new Exception("Surface identifier cannot be empty.");

			// Otherwise set the value.  HOWEVER.. this is not authorised by the authority.  It will be checked on registration.
			this.Authority_SetIdentifier(sIdentifier);

			// Load the debug for this surface.
			//pDebugImageControl = new Image();
			//pDebugImageControl.Source = new BitmapImage(new Uri("pack://application:,,,/UbiDisplays;component/Interface/Images/DebugImage.png"));

			// Generate a key to access the projected surface.
			sProjectionDisplayKey = System.Guid.NewGuid().ToString();

			// A surface control.
			/*pSurfaceContent= new SurfaceView();
			pSurfaceContent.ShowDebug = true;

			// Create a projected display for this surface.
			pProjectionDisplay = ProjectionRenderer.AddDisplay(sProjectionDisplayKey, pSurfaceContent);//, pDebugImageControl);
			pProjectionDisplay.Visible = true;*/
		}
		#endregion

		#region Deletion Pattern
		/// <summary>
		/// Call this to tell the surface to delete.  This works by calling Authority.DeleteSurface.
		/// </summary>
		public void Delete()
		{
			// Try to remove us from the authority.  This should take care of most things.
			try
			{
				Authority.DeleteSurface(this);
			}
			catch (Exception e)
			{
				Log.Write("Error deleting surface '" + this.Identifier + "'. " + e.Message, Authority.AUTHORITY_LOG_SOURCE, Log.Type.AppWarning);
			}
		}

		/// <summary>
		/// Determine if this surface has been deleted.
		/// </summary>
		public bool IsDeleted()
		{
			return bDeleted;
		}
		#endregion

		#region Authority Handlers
		/// <summary>
		/// A function which is called by the authority to set the surface name.
		/// </summary>
		/// <param name="sValue">The new name for this surface.</param>
		/// <remarks>This is not checked.  Ensure that only the authority calls this otherwise it will lead to undefined behaviour.  If you want to change the surface name use the property 'Surface.Identifier = "whatever"'</remarks>
		protected internal void Authority_SetIdentifier(String sValue)
		{
			// Set the value.
			sIdentifier = sValue;

			// Say we have updated a property.
			if (OnSurfacePropertiesUpdated != null)
				OnSurfacePropertiesUpdated(this);
		}

		/// <summary>
		/// Called by the authority to signal that this surface has been deleted.
		/// </summary>
		internal void Authority_Delete()
		{
			// Remove the projection display.
			/*if (pProjectionDisplay != null)
			{
				pProjectionDisplay.Visible = false;
				ProjectionRenderer.RemoveDisplay(pProjectionDisplay);
				pProjectionDisplay = null;
			}*/

			// Remove the reference to the active display.
			if (ActiveDisplay != null)
			{
				Authority_DetachDisplay(ActiveDisplay);
				throw new Exception("Error deleting surface.  Display still active.");
			}

			// Free any resources.
			this.DeleteResources();

			// Remove the debug image.
			/*if (pSurfaceContent != null)
			{
				pSurfaceContent.Children.Clear();
				pSurfaceContent = null;
			}*/
			//pDebugImageControl = null;

			// Set the deleted flag.
			bDeleted = true;

			// Deletion event.
			if (OnDeleted != null)
				OnDeleted(this);
		}

		/// <summary>
		/// Called by the authority to attach the specified display to this surface.
		/// </summary>
		/// <param name="pDisplay">The display we want to attach.</param>
		internal void Authority_AttachDisplay(Display pDisplay)
		{
			// If this surface already has a display, raise an error.
			if (ActiveDisplay != null)
				throw new Exception("Surface already has display attached.");

			/* Uncomment this if you want transient projection displays.
			// Check we don't already have a projection display.
			if (pProjectionDisplay != null)
				throw new Exception("Surface already has a projection display.");

			// Create a projected display for this surface.
			pProjectionDisplay = ProjectionRenderer.AddDisplay(sProjectionDisplayKey, pDebugImageControl);
			pProjectionDisplay.Visible = false;
			*/

			// Update the projection with our known spatial properties.
			this.UpdateProjectedImage();

			// Tell this display to create its renderable.
			pDisplay.Surface_BindToSurface(this);

			// Set the active display pointer.
			ActiveDisplay = pDisplay;

			// Recompute our spatial properties just to be sure. 
			this.RecomputeSpatialProperties();

			// Raise the event for display change.
			if (OnDisplayChanged != null)
				OnDisplayChanged(this);
		}

		/// <summary>
		/// Called by the Authority to tell this surface to detach its current display.
		/// </summary>
		/// <param name="pDisplay">The display to detach.  Must be the current display.</param>
		internal void Authority_DetachDisplay(Display pDisplay)
		{
			// If this surface already has a display, raise an error.
			if (ActiveDisplay == null)
				throw new Exception("Surface does not have a display attached.");

			// Check the specified display is the display we want to unbind.
			if (pDisplay != ActiveDisplay)
				throw new Exception("Surface cannot remove specified display because a different one is attached.");

			// Signal the display to unbind from this.
			ActiveDisplay.Surface_UnbindFromSurface(this);

			// Remove the reference to the rendered control.
			//   n.b. this will already likely have been called by Surface_UnbindFromSurface
			Display_SetVisual(null);
			Display_ResetVisualRenderSize();

			// Set our opacity back to fully shown (defensive incase the display does not put it back).
			ContentOpacity = 1.0;

			// Uncomment this if we want transient projection surfaces.
			/*
			// Remove the projection surface from the projection.
			ProjectionRenderer.RemoveDisplay(pProjectionDisplay);
			*/

			// Set the active display pointer.
			ActiveDisplay = null;

			// Raise the event for display change.
			if (OnDisplayChanged != null)
				OnDisplayChanged(this);
		}
		#endregion

		#region Display Handlers
		/// <summary>
		/// Called a Display (when this calls Displays.Surface_BindToSurface) to set the visual they want to render on this surface.
		/// </summary>
		/// <param name="pVisual">A reference to the visual they want to render on the surface.  If null then remove the active one.</param>
		internal void Display_SetVisual(UIElement pVisual)
		{
			// Check we have a projection surface.
			//if (pProjectionDisplay == null)
			//	throw new Exception("Cannot set surface visual because there is not a corresponding projection surface.");

			/*
			// Set the content pointer.
			pProjectionDisplay.Content = pVisual;
			*/

			// Place us on the surface content.
			//pSurfaceContent.Children.Clear();
			//if (pVisual != null)
			//	pSurfaceContent.Children.Add(pVisual);
		}

		/// <summary>
		/// Can be called by a display to change the render size of this control.
		/// </summary>
		/// <remarks>Only works with sizes larger than 20px in each dimension.</remarks>
		/// <param name="pSize">The size to render to.</param>
		internal void Display_SetVisualRenderSize(Size pSize)
		{
			if (pSize.Width > 20 && pSize.Height > 20)
			{
				//pSurfaceContent.Width = pSize.Width;
				//pSurfaceContent.Height = pSize.Height;
			}
		}

		/// <summary>
		/// Can be called by a display to change the render size of this control.
		/// </summary>
		/// <remarks>This wills set the render size of this surface back to something suitable. i.e. 800x600</remarks>
		internal void Display_ResetVisualRenderSize()
		{
			//pSurfaceContent.Width = 800;
			//pSurfaceContent.Height = 600;
		}
		#endregion

		/// <summary>
		/// Set the spatial properties of this display to reflect the arguments passed.
		/// </summary>
		/// <param name="tProjector">The four corners of the projection.</param>
		/// <param name="tSensor">The four corners in 3D space.</param>
		/// <param name="tKinectImage">The four corners in the Kinect video feeed.</param>
		public void SetSpatialProperties(Point[] tProjector, Vector3[] tSensor, Point[] tKinectImage)
		{
			// Store the values.
			this.ProjectorSpace = tProjector;
			this.SensorSpace = tSensor;
			this.KinectSpace = tKinectImage;

			// Update the projected image.
			this.UpdateProjectedImage();

			// Recompute the spatial properties.  This will also update any attached displays etc.
			this.RecomputeSpatialProperties();
		}

		/// <summary>
		/// Update the projected image to match the spatial properties of this surface.
		/// </summary>
		private void UpdateProjectedImage()
		{
			// Throw an error if the projection display is missing.
			//if (pProjectionDisplay == null)
			//	throw new Exception("Cannot update the projected image without a valid projected display!");

			// Update the homography..
			// FROM  = left edge, bottom edge, right edge, top edge
			// TO    = bottomleft, topleft, bottomright, topright
			//pProjectionDisplay.SetHomographyFrom2DPoints(new Point[]
			//{
			//	ProjectorSpace[BOTTOMLEFT_INDEX], // bl
			//	ProjectorSpace[TOPLEFT_INDEX], //tl
			//	ProjectorSpace[BOTTOMRIGHT_INDEX], //br
			//	ProjectorSpace[TOPRIGHT_INDEX] //tr
				/*
				ProjectorSpace[TOPLEFT_INDEX], // bl
				ProjectorSpace[TOPRIGHT_INDEX], //tl
				ProjectorSpace[BOTTOMLEFT_INDEX], //br
				ProjectorSpace[BOTTOMRIGHT_INDEX] //tr
				*/
			//});
		}

		/// <summary>
		/// This method will force an update of the spatial properties.  It will raise the OnSpatialPropertiesUpdated event.
		/// </summary>
		/// <remarks>If this surface hosts a view, it will be notified.</remarks>
		public void RecomputeSpatialProperties()
		{
			// Estimate the surface plane from the four corners.
			Plane  = RatcliffPlane.ComputeBestFit(SensorSpace);

			// Compute the aspect ratio (w/h) and physical size.
			Width  = Vector3.Distance(SensorSpace[BOTTOMLEFT_INDEX], SensorSpace[TOPLEFT_INDEX]); // bottom left - top left
			Height = Vector3.Distance(SensorSpace[BOTTOMRIGHT_INDEX], SensorSpace[BOTTOMLEFT_INDEX]); // bottom right - bottom left

			// Compute the aspect ratio.
			AspectRatio = this.Width / this.Height;

			// Compute angle relative to the world.
			// The angle between the (surface plane) and the (calibration plane).
			var tCalibrationPlane = Surface.KinectProcessor.CalibrationPlane;
			float fDot = Vector3.Dot(tCalibrationPlane.Normal, Plane.Normal);
			Angle = (float)Math.Acos(fDot / (tCalibrationPlane.Normal.Length * Plane.Normal.Length));

			// Raise the updated event.
			if (OnSurfacePropertiesUpdated != null)
				OnSurfacePropertiesUpdated(this);
		}


		/// <summary>
		/// Rotate this surface clockwise.
		/// </summary>
		/// <param name="bClockwise">Do we want to rotate it clockwise or anti-clockwise.</param>
		public void RotateSurface(bool bClockwise)
		{
			// Swap the buffers around.
			RotateArray<Vector3>(this.SensorSpace, bClockwise);
			RotateArray<Point>(this.KinectSpace, bClockwise);
			RotateArray<Point>(this.ProjectorSpace, bClockwise);

			// Update the spatial properties of this surface to reflect the change.
			SetSpatialProperties(this.ProjectorSpace, this.SensorSpace, this.KinectSpace);
		}

		/// <summary>
		/// Helper method to rotate an array in a given direction.
		/// </summary>
		/// <typeparam name="T">The generic array type.</typeparam>
		/// <param name="src">The source array.</param>
		/// <param name="bClockwise">True to rotate it clockwise ([0,1,2]->[1,2,0]) or false to rotate anti-clockwise ([0,1,2]->[2,0,1]).</param>
		private void RotateArray<T>(T[] src, bool bClockwise)
		{
			if (bClockwise)
			{
				T tmp = src[0];
				for (int i = 0; i < (src.Length - 1); ++i)
				{
					src[i] = src[i + 1];
				}
				src[src.Length - 1] = tmp;
			}
			else
			{
				T tmp = src[(src.Length - 1)];
				for (int i = (src.Length - 1); i > 0; --i)
				{
					src[i] = src[i - 1];
				}
				src[0] = tmp;
			}
		}
	}
}
