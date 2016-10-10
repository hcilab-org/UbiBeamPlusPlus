using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimMath;

namespace UbiDisplays.Model
{
    /// <summary>
    /// An ISpatialQuery is used to check if 3D points belong to a specific region.
    /// </summary>
    /// <remarks>This is used by displays to query regions for interaction. i.e. multi-touch cubes etc.</remarks>
    public interface ISpatialQuery : IResource
    {
        /// <summary>
        /// Determine if a 3D point (from the Kinect sensor) resides within this region.
        /// </summary>
        /// <param name="vPoint">The point to check.</param>
        /// <returns>True if it is, false if it isn't.</returns>
        bool IsContained(Vector3 vPoint);

        /// <summary>
        /// Begin recipt of a new frame of points.
        /// </summary>
        void BeginFrame();

        /// <summary>
        /// End recipt of a new frame of points.  Process them as necessary.
        /// </summary>
        void EndFrame();

        /// <summary>
        /// Insert a point (from the kinect sensor) into the current frame if it resides within this region.
        /// </summary>
        /// <param name="vPoint">The point to insert, if contained.</param>
        /// <returns>True if it was inserted, false it not.</returns>
        bool InsertIfContained(Vector3 vPoint);
    }
}
