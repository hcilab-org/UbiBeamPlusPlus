using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimMath;

namespace UbiDisplays.Utilities
{
    /// <summary>
    /// Find a convex hull from a set of points using the Graham Scan algorythim.
    /// </summary>
    /// <remarks>This works on 2D floating point vectors.</remarks>
    public abstract class GrahamScan
    {
        /// <summary>
        /// Find convex hull for the given set of input points.
        /// </summary>
        /// <remarks>See this for how it works: http://softsurfer.com/Archive/algorithm_0109/algorithm_0109.htm </remarks>
        /// <param name="lPoints">The set of input points.</param>
        /// <returns>The points which form the outside of the convex hull.</returns>
        public static List<Vector2> FindConvexHull(IEnumerable<Vector2> lPoints)
        {
            // If we have no points, return nothing.
            if (lPoints == null)
                return null;

            // Convert the input point list to our comparable class.
            List<GrahamPointData> lPts = new List<GrahamPointData>();
            foreach (Vector2 Vector2 in lPoints)
            {
                lPts.Add(new GrahamPointData(Vector2));
            }

            // If we don't have enough data, bail.
            if (lPts.Count <= 1)
                return null;

            // Find the point with lowest X and lowest Y.
            int iStartIndex = 0;
            var pStart = lPts[0];
            for (int i = 1, n = lPts.Count; i < n; i++)
            {
                // If it is is further left pr it is the same left but lower down.
                if ((lPts[i].X < pStart.X) || ((lPts[i].X == pStart.X) && (lPts[i].Y < pStart.Y)))
                {
                    pStart = lPts[i];
                    iStartIndex = i;
                }
            }

            // Remove that point from the array.
            lPts.RemoveAt(iStartIndex);

            // Compute the delta, distance and tangent of each point to our start point.
            for (int i = 0, n = lPts.Count; i < n; i++)
            {
                var dx = lPts[i].X - pStart.X;
                var dy = lPts[i].Y - pStart.Y;
                lPts[i].Distance = dx * dx + dy * dy;
                lPts[i].K = (dx == 0) ? double.PositiveInfinity : dy / dx;
            }

            // Sort points by angle and distance.
            lPts.Sort();

            // Create a list to contain the convex hull and add the first corner.
            var lHull = new List<GrahamPointData>();
            lHull.Add(pStart);

            // Add our first point.
            lHull.Add(lPts[0]);
            lPts.RemoveAt(0);

            // Setup pointers.
            var pPrev = lHull[0];
            var pLast = lHull[1];
            
            // While we still have points to process.
            while (lPts.Count != 0)
            {
                // Bring the next point into scope.
                var pCurrent = lPts[0];

                // Remove from the process list if the point has the same tangent or it is in the same place.
                if ((pCurrent.K == pLast.K) || (pCurrent.Distance == 0))
                {
                    lPts.RemoveAt(0);
                    continue;
                }

                // If we can accept our current point (i.e. it is to the left of both the last and the previous).
                if ((pCurrent.X - pPrev.X) * (pLast.Y - pCurrent.Y) - (pLast.X - pCurrent.X) * (pCurrent.Y - pPrev.Y) < 0)
                {
                    // Add the point to the hull and remove it from the processing list.
                    lPts.RemoveAt(0);
                    lHull.Add(pCurrent);

                    // Update the references.
                    pPrev = pLast;
                    pLast = pCurrent;
                }

                // Otherwise 
                else
                {
                    // Remove the last points from the hull.
                    lHull.RemoveAt(lHull.Count - 1);

                    // Update the references.
                    pLast = pPrev;
                    pPrev = lHull[lHull.Count - 2];
                }
            }

            // Convert points back into a format we know and love.
            List<Vector2> lOutput = new List<Vector2>();
            foreach (GrahamPointData pt in lHull)
                lOutput.Add(pt.ToVector2());
            return lOutput;
        }

        /// <summary>
        /// A graham scan datapoint which sorts on tangent and Euclidian distance.
        /// </summary>
        private class GrahamPointData : IComparable
        {
            public double X;
            public double Y;
            public double K;
            public double Distance;

            /// <summary>
            /// Create a new comparison.
            /// </summary>
            /// <param name="Vector2"></param>
            public GrahamPointData(Vector2 Vector2)
            {
                X = Vector2.X;
                Y = Vector2.Y;

                K = 0;
                Distance = 0;
            }

            /// <summary>
            /// Set the values for this point from a vector2.
            /// </summary>
            /// <param name="tVector2"></param>
            public void fromVector2(Vector2 tVector2)
            {
                this.X = tVector2.X;
                this.Y = tVector2.Y;
            }

            /// <summary>
            /// Compare this to another object.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public int CompareTo(object obj)
            {
                GrahamPointData another = (GrahamPointData)obj;

                return (K < another.K) ? -1 : (K > another.K) ? 1 :
                    ((Distance > another.Distance) ? -1 : (Distance < another.Distance) ? 1 : 0);
            }

            /// <summary>
            /// Extract a Vector2 from this point.
            /// </summary>
            /// <returns></returns>
            public Vector2 ToVector2()
            {
                return new Vector2((float)X, (float)Y);
            }
        }

    }
}
