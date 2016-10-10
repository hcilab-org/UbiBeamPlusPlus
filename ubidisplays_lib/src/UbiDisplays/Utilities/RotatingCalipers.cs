using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimMath;

namespace UbiDisplays.Utilities
{
    /// <summary>
    /// Uses the Rotating Calipers technique to compute the minimum bounding box for a 2D point cloud.
    /// Based on the VB source found here: http://www.vb-helper.com/howto_net_find_bounding_rectangle.html
    /// </summary>
    public class RotatingCalipers
    {
        /// <summary>
        /// Find a smallest rectangle that bounds a set of points.
        /// This assumes that the input array contains the points in a convex polygon in counter-clockwise order.
        /// </summary>
        /// <remarks>This assumes that the input array contains the points in a convex polygon in counter-clockwise order.</remarks>
        /// <param name="tInput">An array of points that compose the convex, counter-clockwise polygon.</param>
        /// <param name="bOrder">Do you want to automatically pre-process the data to create the counter-clockwise wound convex-hull?</param>
        /// <returns>An array containing the 4 corners of the corner points of the smallest rectangle that bounds a set of points.  { left edge, bottom edge, right edge, top edge }</returns>
        public static System.Windows.Point[] ComputeMinimumBoundingRectangle(IEnumerable<System.Windows.Point> tInput, bool bOrder)
        {
            // Convert the points to Vector2's.
            List<Vector2> lPoints = new List<Vector2>();
            foreach (var tPoint in tInput)
                lPoints.Add(new Vector2((float)tPoint.X, (float)tPoint.Y));

            // Do the calipers.
            var tOutput = ComputeMinimumBoundingRectangle(lPoints, bOrder);

            // Convert the output back to windows point coordinates.
            System.Windows.Point[] tOutPoints = new System.Windows.Point[tOutput.Length];
            for (int i = 0, n = tOutput.Length; i < n; ++i)
                tOutPoints[i] = new System.Windows.Point(tOutput[i].X, tOutput[i].Y);
            
            // Return the converted.
            return tOutPoints;
        }

        /// <summary>
        /// Find a smallest rectangle that bounds a set of points.
        /// This assumes that the input array contains the points in a convex polygon in counter-clockwise order.
        /// </summary>
        /// <remarks>This assumes that the input array contains the points in a convex polygon in counter-clockwise order.</remarks>
        /// <param name="tInput">An array of points that compose the convex, counter-clockwise polygon.</param>
        /// <param name="bOrder">Do you want to automatically pre-process the data to create the counter-clockwise wound convex-hull?</param>
        /// <returns>An array containing the 4 corners of the corner points of the smallest rectangle that bounds a set of points.  { left edge, bottom edge, right edge, top edge }</returns>
        public static Vector2[] ComputeMinimumBoundingRectangle(IEnumerable<Vector2> tInput, bool bOrder)
        {
            #region Ensure points form a convex counter-clockwise wound polygon.
            // Accept inputs.
            Vector2[] tVertices = null;// tInput;
            int iPointCount = -1;// tVertices.Length;

            // Ensure the points form a convex counter-clockwise wound polygon.
            if (bOrder)
            {
                //tVertices = Melkman.ComputeConvexHull(tInput);
                tVertices = GrahamScan.FindConvexHull(tInput).ToArray();
                tVertices = tVertices.Reverse().ToArray() ;
                iPointCount = tVertices.Length;
            }
            else
            {
                tVertices = tInput.ToArray();
                iPointCount = tVertices.Length;
            }
            #endregion

            #region Setup Variables
            // The four caliper control points.
            // { left edge, bottom edge, right edge, top edge } { xmin, ymax, xmax, ymin }
            var tControlPoints = new int[4];

            // Some key variables.
            int iCurrentControlPoint = -1;             // The current control point.
            int iRectanglesExamined = 0;              // The number of rectangles we have examined.
            float fBestArea = float.MaxValue; // The best rectangle area so far.
            Vector2[] tBestRectangle = null;           // The best rectangle points so far.

            // Store which edges we have checked.  Initially all false.
            var tEdgesChecked = new bool[iPointCount];
            for (int i = 0; i < iPointCount; ++i)
                tEdgesChecked[i] = false;
            #endregion

            #region Determine Initial Control Points
            float minx = tVertices[0].X;
            float maxx = minx;
            float miny = tVertices[0].Y;
            float maxy = miny;

            // Compute initial control points based on the AABB of the hull.
            for (int i = 1; i <= iPointCount - 1; i++)
            {
                if (minx > tVertices[i].X) { minx = tVertices[i].X; tControlPoints[0] = i; }
                if (maxx < tVertices[i].X) { maxx = tVertices[i].X; tControlPoints[2] = i; }
                if (miny > tVertices[i].Y) { miny = tVertices[i].Y; tControlPoints[3] = i; }
                if (maxy < tVertices[i].Y) { maxy = tVertices[i].Y; tControlPoints[1] = i; }
            }
            #endregion

            while (true)
            {
                #region Select Control Points
                // Find the next point on an edge.
                float xmindx = 0; float xmindy = 0;
                float ymaxdx = 0; float ymaxdy = 0;
                float xmaxdx = 0; float xmaxdy = 0;
                float ymindx = 0; float ymindy = 0;
                FindDelta(ref xmindx, ref xmindy, tControlPoints[0], tVertices);
                FindDelta(ref ymaxdx, ref ymaxdy, tControlPoints[1], tVertices);
                FindDelta(ref xmaxdx, ref xmaxdy, tControlPoints[2], tVertices);
                FindDelta(ref ymindx, ref ymindy, tControlPoints[3], tVertices);

                // Switch so we can look for the smallest opposite/adjacent ratio.
                float xminopp = xmindx; float xminadj = xmindy;
                float ymaxopp = -ymaxdy; float ymaxadj = ymaxdx;
                float xmaxopp = -xmaxdx; float xmaxadj = -xmaxdy;
                float yminopp = ymindy; float yminadj = -ymindx;

                // Pick initial values that will make every point an improvement.
                float fBestOpposite = 1;
                float fBestAdjacent = 0;
                iCurrentControlPoint = 0; // HACK This should say -1.

                // Attempt the select the next best control point.
                if (xminopp >= 0 && xminadj >= 0)
                {
                    if (xminopp * fBestAdjacent < fBestOpposite * xminadj) { fBestOpposite = xminopp; fBestAdjacent = xminadj; iCurrentControlPoint = 0; }
                }
                if (ymaxopp >= 0 && ymaxadj >= 0)
                {
                    if (ymaxopp * fBestAdjacent < fBestOpposite * ymaxadj) { fBestOpposite = ymaxopp; fBestAdjacent = ymaxadj; iCurrentControlPoint = 1; }
                }
                if (xmaxopp >= 0 && xmaxadj >= 0)
                {
                    if (xmaxopp * fBestAdjacent < fBestOpposite * xmaxadj) { fBestOpposite = xmaxopp; fBestAdjacent = xmaxadj; iCurrentControlPoint = 2; }
                }
                if (yminopp >= 0 && yminadj >= 0)
                {
                    if (yminopp * fBestAdjacent < fBestOpposite * yminadj) { fBestOpposite = yminopp; fBestAdjacent = yminadj; iCurrentControlPoint = 3; }
                }

                // Throw an error if one was not selected.
                if (iCurrentControlPoint == -1)
                    throw new Exception("Unable to determine best control point for rectangle sample: " + iRectanglesExamined + ".  Are the input points part of a counter-clockwise wound convex hull?");
                #endregion

                #region Compute the tangent for the current edge.
                // See which point has the current edge.
                int i1 = tControlPoints[iCurrentControlPoint];
                int i2 = (i1 + 1) % iPointCount;
                float dx = tVertices[i2].X - tVertices[i1].X;
                float dy = tVertices[i2].Y - tVertices[i1].Y;

                // Make dx and dy work for the first line.
                switch (iCurrentControlPoint)
                {
                    case 0: break;                                              // Nothing to do.
                    case 1: { float temp = dx; dx = -dy; dy = temp; } break;    // dx = -dy, dy = dx
                    case 2: { dx = -dx; dy = -dy; } break;                      // dx = -dx, dy = -dy
                    case 3: { float temp = dx; dx = dy; dy = -temp; } break;    // dx = dy, dy = -dx
                }
                #endregion

                #region Compute a new bounding rectangle based on this edge.
                // Compute the newest area.
                Vector2[] pts = new Vector2[4];
                float fArea = CheckBoundingRectangle(ref pts,
                    tVertices[tControlPoints[0]].X, tVertices[tControlPoints[0]].Y, dx, dy,
                    tVertices[tControlPoints[1]].X, tVertices[tControlPoints[1]].Y, dy, -dx,
                    tVertices[tControlPoints[2]].X, tVertices[tControlPoints[2]].Y, -dx, -dy,
                    tVertices[tControlPoints[3]].X, tVertices[tControlPoints[3]].Y, -dy, dx);
                #endregion

                #region Select if better, mark the edge as done and quit the loop if complete.
                // If our area is better than the best we are aware of, store it.
                if (fArea < fBestArea)
                {
                    fBestArea = fArea;
                    tBestRectangle = pts;
                }

                // This says we are done with using this edge.
                tControlPoints[iCurrentControlPoint] = (tControlPoints[iCurrentControlPoint] + 1) % iPointCount;

                // Remember that we have checked this edge.
                tEdgesChecked[tControlPoints[iCurrentControlPoint]] = true;

                // See if we have checked all possible bounding rectangles.
                iRectanglesExamined += 1;

                // If we have finished checking all possible rectangles, we can quit the loop.
                if (iRectanglesExamined >= tVertices.Length)
                    break;
                #endregion
            }

            return tBestRectangle;
        }

        /// <summary>
        /// Compute a rectangle from the intersection of control points and their tangents.
        /// </summary>
        /// <param name="pts">The points of the intersected rectangle.</param>
        /// <returns>The area of the rectangle returned</returns>
        private static float CheckBoundingRectangle(ref Vector2[] pts, float px0, float py0, float dx0, float dy0, float px1, float py1, float dx1, float dy1, float px2, float py2, float dx2, float dy2, float px3, float py3, float dx3, float dy3)
        {
            // Compute our intersection points - they form our corners.
            FindIntersection(px0, py0, px0 + dx0, py0 + dy0, px1, py1, px1 + dx1, py1 + dy1, ref pts[0]);
            FindIntersection(px1, py1, px1 + dx1, py1 + dy1, px2, py2, px2 + dx2, py2 + dy2, ref pts[1]);
            FindIntersection(px2, py2, px2 + dx2, py2 + dy2, px3, py3, px3 + dx3, py3 + dy3, ref pts[2]);
            FindIntersection(px3, py3, px3 + dx3, py3 + dy3, px0, py0, px0 + dx0, py0 + dy0, ref pts[3]);

            // Get the area of the bounding rectangle.
            float vx0 = pts[0].X - pts[1].X;
            float vy0 = pts[0].Y - pts[1].Y;
            float len0 = (float)Math.Sqrt(vx0 * vx0 + vy0 * vy0);

            float vx1 = pts[1].X - pts[2].X;
            float vy1 = pts[1].Y - pts[2].Y;
            float len1 = (float)Math.Sqrt(vx1 * vx1 + vy1 * vy1);

            // Return the area of this rectangle.
            return len0 * len1;
        }

        /// <summary>
        /// Find the slope of the edge from point i to point i+1.
        /// This automatically wraps around (hence the need for counter-clockwise wound input).
        /// </summary>
        /// <param name="dx">The variable to store the change in X into.</param>
        /// <param name="dy">The variable to store the change in Y into.</param>
        /// <param name="i">The point index.</param>
        /// <param name="tPoints">The point array.</param>
        private static void FindDelta(ref float dx, ref float dy, int i, Vector2[] tPoints)
        {
            int i2 = (i + 1) % tPoints.Length;
            dx = tPoints[i2].X - tPoints[i].X;
            dy = tPoints[i2].Y - tPoints[i].Y;
        }

        /// <summary>
        /// Find the point of intersection between two lines.
        /// </summary>
        /// <param name="X1">Line 1 p1 X coordinate</param>
        /// <param name="Y1">Line 1 p1 Y coordinate</param>
        /// <param name="X2">Line 1 p2 X coordinate</param>
        /// <param name="Y2">Line 1 p2 Y coordinate</param>
        /// <param name="A1">Line 2 p1 X coordinate</param>
        /// <param name="B1">Line 2 p1 Y coordinate</param>
        /// <param name="A2">Line 2 p2 X coordinate</param>
        /// <param name="B2">Line 2 p2 Y coordinate</param>
        /// <param name="intersect">The point to save the intersection into.</param>
        /// <returns>False if no intersection, True if one occurs.</returns>
        private static bool FindIntersection(float X1, float Y1, float X2, float Y2, float A1, float B1, float A2, float B2, ref Vector2 intersect)
        {
            float dx = X2 - X1;
            float dy = Y2 - Y1;
            float da = A2 - A1;
            float db = B2 - B1;
            float s = 0;
            float t = 0;

            // If the segments are parallel, return False.
            if (Math.Abs(da * dy - db * dx) < 0.001)
                return false;

            // Find the point of intersection.
            s = (dx * (B1 - Y1) + dy * (X1 - A1)) / (da * dy - db * dx);
            t = (da * (Y1 - B1) + db * (A1 - X1)) / (db * dx - da * dy);
            intersect.X = X1 + t * dx;
            intersect.Y = Y1 + t * dy;
            return true;
        }
    }
}
