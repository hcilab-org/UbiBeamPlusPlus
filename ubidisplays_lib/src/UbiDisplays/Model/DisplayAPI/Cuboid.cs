
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimMath;

namespace UbiDisplays.Model.DisplayAPI
{ 
	/// <summary>
	/// The Cuboid class represents the most simple type of trigger.
	/// It is a cuboid in 3D space which notifies a display about parts of the Kinect point cloud which lie within it.
	/// </summary>
	public abstract class Cuboid : ISpatialQuery, IResource
	{
		// TODO: Add support for non-relative coordiantes. Perhaps specify it in Kinect coordiantes? :)

		#region Properties
		/// <summary>
		/// The matrix which converts worldspace points into relative surface coordinates.
		/// </summary>
		public Matrix Transform;

		/// <summary>
		/// The matrix which converts relative surface coordinates back into worldspace points.
		/// </summary>
		public Matrix Inverse;

		/// <summary>
		/// Several planes which define the bounding box of the cuboid in 3D space.
		/// </summary>
		private Plane Back, Front, Top, Bottom, Close, Far;


		/// <summary>
		/// The display which created this trigger space.
		/// </summary>
		public Display ParentDisplay { get; protected set; }

		/// <summary>
		/// A reference to the surface we are set as relative too.
		/// </summary>
		public Surface RelativeSurface { get; protected set; }

		/// <summary>
		/// The plane which defines the projected surface of this trigger point.
		/// </summary>
		public Plane SurfacePlane { get; protected set; }

		/// <summary>
		/// Has this trigger space been deleted.
		/// </summary>
		private bool bDeleted = false;

		/// <summary>
		/// An event which is raised when this is deleted.
		/// </summary>
		public event Action<IResource> OnDeleted;
		#endregion

		#region Arguments
		/// <summary>
		/// The callback function to call on the display.
		/// </summary>
		protected PointsDelegate sCallback = null;

		/// <summary>
		/// The minimum offset from the surface plane to accept points.
		/// </summary>
		protected float fSurfaceZOffset = 0.015f;

		/// <summary>
		/// The height of this tracker.
		/// </summary>
		protected float fHeight = 0.1f;

		/// <summary>
		/// A mutex which prevents us from being deleted during a begin/end frame call.
		/// </summary>
		protected System.Threading.Mutex mFrameLock = new System.Threading.Mutex();
		#endregion

		Vector3 BL;
		Vector3 BR;
		Vector3 TL;
		Vector3 TR;

		public delegate void PointsDelegate(Vector3[] points);

		/// <summary>
		/// Construct a new Kinect Trigger Space Cuboid based on some arguments.
		/// </summary>
		/// <param name="pSurface"></param>
		/// <param name="dArguments"></param>
		public Cuboid(Display pView, Surface surface, PointsDelegate callback, float surfaceZOffset, float height)
		{
			// Get the min-depth touch depth and cuboid height.
			fSurfaceZOffset = surfaceZOffset;
            //fSurfaceZOffset = 0.07f;
			fHeight = height;

			// Get the surface we are capturing input from.
			this.SetRelativeSurface(surface);//dArguments.GetValueOrDefault("relativeto", ""));

			// Get the callback.
			this.SetCallbackFunctionName(callback);//dArguments.GetValueOrDefault("callback", ""));

			// Store the view.
			ParentDisplay = pView;

			// Push us onto a list of spatial queries in the Kinect processing thread.
			Surface.SpatialQueries.Add(this);
		}

		public void SetOffset(float offset)
		{
			fSurfaceZOffset = offset;
			calculateCorners();
		}

		public void SetHeight(float height)
		{
			fHeight = height;
		}

		/// <summary>
		/// Set the name of the callback function to call on the view.
		/// </summary>
		/// <param name="sCallback">The name of the function to invoke.  This cannot be null.</param>
		public void SetCallbackFunctionName(PointsDelegate sCallback)
		{
			this.sCallback = sCallback;
			if (this.sCallback == null) // || this.sCallback == "")
				throw new Exception("Cuboid requires a valid 'callback' function.");
		}

		/// <summary>
		/// Set the surface which we want to update
		/// </summary>
		/// <param name="sSurface"></param>
		public void SetRelativeSurface(Surface sSurface)
		{
			// If we have a current surface, remove it.
			if (RelativeSurface != null)
			{
				RelativeSurface.OnSurfacePropertiesUpdated -= Surface_OnPropertiesChanged;
				RelativeSurface = null;
			}

			// Find the surface.
			var pSurface = sSurface; // Authority.FindSurface(sSurface);
			if (pSurface == null)
				throw new Exception("Cannot create Cuboid because it has no relative surface.");

			// Store the surface and listen for changes.
			RelativeSurface = pSurface;
			RelativeSurface.OnSurfacePropertiesUpdated += Surface_OnPropertiesChanged;

			// Setup based on bounding corners.
			this.SetBoundingCorners(pSurface.SensorSpace[Surface.BOTTOMLEFT_INDEX], pSurface.SensorSpace[Surface.TOPLEFT_INDEX], pSurface.SensorSpace[Surface.BOTTOMRIGHT_INDEX], pSurface.SensorSpace[Surface.TOPRIGHT_INDEX]);
		}

		/// <summary>
		/// Handle the surface we are relative to being updated.
		/// </summary>
		/// <param name="pSurface"></param>
		private void Surface_OnPropertiesChanged(Surface pSurface)
		{
			// Check the passed surface is the relative surface!
			if (pSurface != RelativeSurface)
			{
				throw new Exception("Somehow cuboid is subscribed to the wrong surface.");
			}

			// Update based on the surface properties.
			if (pSurface != null)
			{
				// The surface bounding corners.
				this.SetBoundingCorners(pSurface.SensorSpace[Surface.BOTTOMLEFT_INDEX], pSurface.SensorSpace[Surface.TOPLEFT_INDEX], pSurface.SensorSpace[Surface.BOTTOMRIGHT_INDEX], pSurface.SensorSpace[Surface.TOPRIGHT_INDEX]);
				//this.SetBoundingCorners(pSurface.SensorSpace[Surface.BOTTOMLEFT_INDEX], pSurface.SensorSpace[Surface.BOTTOMRIGHT_INDEX], pSurface.SensorSpace[Surface.TOPLEFT_INDEX], pSurface.SensorSpace[Surface.TOPRIGHT_INDEX]);
			}
		}

		/// <summary>
		/// Unsubscribe to the delete button.
		/// </summary>
		public void Delete()
		{
			// Lock.
			mFrameLock.WaitOne();

			// Unbind and remove reference to the surface.
			if (this.RelativeSurface != null)
				this.RelativeSurface.OnSurfacePropertiesUpdated -= Surface_OnPropertiesChanged;
			this.RelativeSurface = null;

			// Remove reference to the display.
			if (ParentDisplay != null)
				ParentDisplay = null;

			// Remove us from the list of spatial queries to process.
			Surface.SpatialQueries.Remove(this);

			// Set the flag.
			this.bDeleted = true;

			// Unlock.
			mFrameLock.ReleaseMutex();

			// Deletion event.
			if (OnDeleted != null)
				OnDeleted(this);
		}

		/// <summary>
		/// Has this resource been deleted or not.
		/// </summary>
		/// <returns></returns>
		public bool IsDeleted()
		{
			return bDeleted;
		}

		/// <summary>
		/// Update the bounding corners of the surface corners of this 3D cuboid with specific corners.
		/// </summary>
		/// <param name="vBottomLeft">What the space understands as the bottom left corner of the surface.</param>
		/// <param name="vBottomRight">What the space understands as the bottom right corner of the surface.</param>
		/// <param name="vTopLeft">What the space understands as the top left corner of the surface.</param>
		/// <param name="vTopRight">What the space understands as the top right corner of the surface.</param>
		public void SetBoundingCorners(Vector3 vBottomLeft, Vector3 vBottomRight, Vector3 vTopLeft, Vector3 vTopRight)
		{
			// Set the corners.
			BL = vBottomLeft;
			BR = vBottomRight;
			TL = vTopLeft;
			TR = vTopRight;

			calculateCorners();
		}

		private void calculateCorners()
		{
			if (BL == null) return;

			// Extrapolation distance.
			float fDistance = -(fSurfaceZOffset + fHeight);

			// Query all the points in a given region.
			if (this.RelativeSurface == null)
				throw new Exception("Setting the bounding corners requires a surface.");

			// Slow (use all data within the surface region to compute the plane).
			// SurfacePlane = Surface.KinectProcessor.ImageCoordinatesToBestFitPlane(this.RelativeSurface.KinectSpace);

			// Fast (just use corners to compute the plane.
			SurfacePlane = Utilities.RatcliffPlane.ComputeBestFit(new Vector3[] { BL, BR, TL, TR });
			SurfacePlane.Normalize();

			// Check the plane is facing upwards.
			if (SurfacePlane.Normal.Z < 0)
			{
				SurfacePlane *= -1;
				//SurfacePlane = new Plane(SurfacePlane.Normal, SurfacePlane.D * -1);
			}

			// Extrapolate the cube based on the normal of the plane.
			Vector3 BLZ = BL + (SurfacePlane.Normal * fDistance);
			Vector3 BRZ = BR + (SurfacePlane.Normal * fDistance);
			Vector3 TLZ = TL + (SurfacePlane.Normal * fDistance);
			Vector3 TRZ = TR + (SurfacePlane.Normal * fDistance);

			// Create a scaling matrix to transform values between 0 and 1 reative to the display.
			var mScale = Matrix.Scaling(
				1f / Vector3.Distance(BL, BR),
				1f / Vector3.Distance(BL, TL),
				1f / Vector3.Distance(BL, BLZ));

			// Create a transformation matrix (and inverse) which allows going to and from surface space.
			Transform = Matrix.LookAtRH(BL, BL + (SurfacePlane.Normal * 100f), Vector3.Normalize(TL - BL)) * mScale;
			Inverse = Matrix.Invert(Transform);

			//             top       back                .. clockwise windings ..
			//         TR------TRz
			//         /|      /|           - Back Plane    <= (BL, TL, TLz, BLz)
			//       TL-+----TLz|           - Front Plane   <= (BR, TR, TRz, BRz)
			// close  | BR----|-BRz  far    
			//        |/      |/            - Top Plane     <= (TL, TR, TRz, TLz)
			//       BL------BLz            - Bottom Plane  <= (BL, BR, BRz, BLz)
			//         bottom
			//                              - Close Plane   <= (BL, TL, TR, BR)
			//   font                       - Far   Plane   <= (BLz, TLz, TRz, BRz)
			// 

			Back = new Plane(BL, TL, TLZ);
			Front = new Plane(BR, TR, TRZ);

			Top = new Plane(TL, TR, TRZ);
			Bottom = new Plane(BL, BR, BRZ);

			Close = new Plane(BL, TL, TR);
			Far = new Plane(BLZ, TLZ, TRZ);
		}

		/// <summary>
		/// Check to see if a world sensor point is contained in this trigger space.
		/// </summary>
		/// <param name="vPoint"></param>
		/// <returns></returns>
		public bool IsContained(Vector3 vPoint)
		{
			// vPoint DOT (plane normal) = 0  ... The point lies on the plane.
			// vPoint DOT (plane normal) > 0  ... The point lies on the same side as the normal vector.
			// vPoint DOT (plane normal) < 0  ... The point lies on the opposite side of the normal vector.

		   

			// Check if the point is contained within this OOBB.
			return (Plane.DotCoordinate(Close, vPoint) > this.fSurfaceZOffset //  0.015
				&& Plane.DotCoordinate(Far, vPoint) < 0

				&& Plane.DotCoordinate(Back, vPoint) < 0
				&& Plane.DotCoordinate(Front, vPoint) > 0

				&& Plane.DotCoordinate(Top, vPoint) < 0
				&& Plane.DotCoordinate(Bottom, vPoint) > 0);
		}

		/// <summary>
		/// Begin recipt of a new frame of points.
		/// </summary>
		public void BeginFrame()
		{
			mFrameLock.WaitOne();
			BeginFrameImpl();
			mFrameLock.ReleaseMutex();
		}

		/// <summary>
		/// End recipt of a new frame of points.  Process them as necessary.
		/// </summary>
		public void EndFrame()
		{
			mFrameLock.WaitOne();
			EndFrameImpl();
			mFrameLock.ReleaseMutex();
		}

		/// <summary>
		/// Begin recipt of a new frame of points.
		/// </summary>
		public abstract void BeginFrameImpl();

		/// <summary>
		/// End recipt of a new frame of points.  Process them as necessary.
		/// </summary>
		public abstract void EndFrameImpl();

		/// <summary>
		/// Insert a point (from the kinect sensor) into the current frame if it is contained.
		/// </summary>
		/// <param name="vPoint">The point to insert, if contained.</param>
		/// <returns>True if it was inserted, false it not.</returns>
		public abstract bool InsertIfContained(Vector3 vPoint);
	}
}
