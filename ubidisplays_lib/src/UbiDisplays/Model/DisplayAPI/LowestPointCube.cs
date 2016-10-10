
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimMath;

namespace UbiDisplays.Model.DisplayAPI
{
	/// <summary>
	/// Create a lowest point cube and attach it as a display resource.
	/// </summary>
	/// <remarks>This is used for ALL displays.  Ensure any state is present within the ProcessRequest function and does not last longer than that.</remarks>
	public class LowestPointCubeHandler : IRequest
	{
		/// <summary>
		/// The name of this handler.
		/// </summary>
		/// <remarks>This also registers this class instance with the authority.</remarks>
		public static String HandleName = Authority.RegisterRequestHandler("kinectlowestpointcube", new LowestPointCubeHandler());

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
				//var pResource = new LowestPointCube(pDisplay, dArguments);
				//pDisplay.AttachResource(pResource);
				//Log.Write("LowestPointCube created relative to '" + pResource.RelativeSurface.Identifier + "'.", pDisplay.ToString(), Log.Type.DisplayInfo); 
				return true;
			}
			catch (Exception e)
			{
				Log.Write("Error creating LowestPointCube: " + e.Message, pDisplay.ToString(), Log.Type.DisplayWarning);
				return false;
			}
		}
	}


	/// <summary>
	/// A spatial query which passes a list of the N lowest points in a cuboid onto the owning display.
	/// </summary>
	public class LowestPointCube : Cuboid
	{
		/// <summary>
		/// The current pixel frame.
		/// </summary>
		private Vector3[] tFrame = null;

		/// <summary>
		/// The number of points we have processed in this frame.
		/// </summary>
		private int iFrameCounter = 0;

		/// <summary>
		/// The limit on the number of points which we are able to accept per-frame.
		/// </summary>
		/// <remarks>This is useful to make sure we don't kill the application.</remarks>
		private int iPointLimit = 200;

		/// <summary>
		/// Do we want to send empty data frames to the display?
		/// </summary>
		protected bool bSendEmptyFrames = true;

		/// <summary>
		/// Do we want to send empty frames if we have more than one in a row.
		/// </summary>
		protected bool bSendEmptySuccessiveFrames = true;

		/// <summary>
		/// Construct a new Kinect Trigger Space Cuboid based on some arguments.
		/// </summary>
		/// <param name="pSurface"></param>
		/// <param name="dArguments"></param>
		public LowestPointCube(Display pView, Surface surface, Cuboid.PointsDelegate callback, float surfaceZOffset, float height)
			: base(pView, surface, callback, surfaceZOffset, height)
		{
			// Store the point limit.
			//iPointLimit = Math.Max(0, dArguments.GetValueOrDefault("point_limit", iPointLimit));
			//bSendEmptyFrames = dArguments.GetValueOrDefault("sendemptyframes", bSendEmptyFrames);
			//bSendEmptySuccessiveFrames = dArguments.GetValueOrDefault("sendemptysucessiveframes", bSendEmptyFrames);
		}

		/// <summary>
		/// Start recieving new points.
		/// </summary>
		public override void BeginFrameImpl()
		{
			// Ensure we have a frame ready.
			if (tFrame == null || tFrame.Length != iPointLimit)
			{
				tFrame = new Vector3[iPointLimit];
				
			}
			iFrameCounter = 0;
		}

		/// <summary>
		/// Signal that we have recieved a full frame of points.
		/// </summary>
		public override void EndFrameImpl()
		{
			// Bail if we don't have a frame or an array.
			if (tFrame == null || this.ParentDisplay == null)
				return;

			// Create a sorted array of points.
			// Obvs. this could be written better with an index buffer for large volumes of data.. Another time though.
			Vector3[] tNew = new Vector3[iFrameCounter];
			Array.Copy(tFrame, tNew, iFrameCounter);
			Array.Sort(tFrame, (a, b) => {
				return Utilities.RatcliffPlane.Distance(a, SurfacePlane) < Utilities.RatcliffPlane.Distance(b, SurfacePlane) ? -1 : 1;
			});

			// Transform them into the relative surface space of this cuboid - converting to JSValue while we are at it.
			Vector3[] tOutput = new Vector3[iFrameCounter];
			for (int i = 0; i < iFrameCounter; ++i)
			{
				var vTransformed = Vector3.TransformCoordinate(tNew[i], Transform);
				tOutput[i] = vTransformed;// new JSValue[] {
				//    vTransformed.X,
				//    vTransformed.Y,
				//    vTransformed.Z
				//};
			}

			// If we send empty frames OR our frame is not empty OR we have already skipped 2 or more empty.
			if (bSendEmptyFrames || tOutput.Length > 0 || (tOutput.Length == 0 && iEmptyFramesIgnored >= 1 && bSendEmptySuccessiveFrames))
			{
				// Send the frame.
				this.sCallback(tOutput);
				//ParentDisplay.AsyncCallGlobalFunction(this.sCallback, new JSValue[] { tOutput });
				iEmptyFramesIgnored = 0;
			}
			else if (bSendEmptySuccessiveFrames)
			{
				// Say that we skipped the frame.
				iEmptyFramesIgnored++;
			}
		}
		private int iEmptyFramesIgnored = 0;

		/// <summary>
		/// Insert a new point into this trigger frame if it is contained.
		/// </summary>
		/// <param name="vPoint"></param>
		/// <returns></returns>
		public override bool InsertIfContained(Vector3 vPoint)
		{
			// If we are already full, do not add more.
			if (iFrameCounter >= iPointLimit)
				return false;

			if (IsContained(vPoint))
			{
				tFrame[iFrameCounter++] = vPoint;
				return true;
			}

			return false;
		}
	}
}
