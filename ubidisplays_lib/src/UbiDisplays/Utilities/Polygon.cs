using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimMath;

namespace UbiDisplays.Utilities
{
    public class Polygon
    {
        /// <summary>
        /// Determine if a point is located inside of an arbituary polygon.
        /// Based on description here: http://alienryderflex.com/polygon/
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <param name="y">The y-coordinate of the point.</param>
        /// <param name="tCorners">The list of polygon vertices.</param>
        /// <returns>True if the point is contained, false if not.</returns>
        public static bool IsPointInPolygon(double x, double y, Vector2[] tCorners)
        {
            int count = tCorners.Length;
            int i, j = count - 1;

            bool oddNodes = false;

            Vector2 pti;
            Vector2 ptj;

            for (i = 0; i < count; ++i)
            {
                pti = tCorners[i];
                ptj = tCorners[j];

                if ((pti.X <= x || ptj.X <= x) && (pti.Y < y && ptj.Y >= y || ptj.Y < y && pti.Y >= y))
                {
                    if (pti.X + (y - pti.Y) / (ptj.Y - pti.Y) * (ptj.X - pti.X) < x)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }
            return oddNodes;
        }

        /// <summary>
        /// Determine if a point is located inside of an arbituary polygon.
        /// Based on description here: http://alienryderflex.com/polygon/
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <param name="y">The y-coordinate of the point.</param>
        /// <param name="lCorners">The list of polygon vertices.</param>
        /// <returns>True if the point is contained, false if not.</returns>
        public static bool IsPointInPolygon(double x, double y, IList<Vector2> lCorners)
        {
            int count = lCorners.Count;
            int i, j = count - 1;

            bool oddNodes = false;

            Vector2 pti;
            Vector2 ptj;

            for (i = 0; i < count; ++i)
            {
                pti = lCorners[i];
                ptj = lCorners[j];

                if ((pti.X <= x || ptj.X <= x) && (pti.Y < y && ptj.Y >= y || ptj.Y < y && pti.Y >= y))
                {
                    if (pti.X + (y - pti.Y) / (ptj.Y - pti.Y) * (ptj.X - pti.X) < x)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }
            return oddNodes;
        }

        /// <summary>
        /// Determine if a point is located inside of an arbituary polygon.
        /// Based on description here: http://alienryderflex.com/polygon/
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <param name="y">The y-coordinate of the point.</param>
        /// <param name="tCorners">The list of polygon vertices.</param>
        /// <returns>True if the point is contained, false if not.</returns>
        public static bool IsPointInPolygon(double x, double y, System.Windows.Point[] tCorners)
        {
            int count = tCorners.Length;
            int i, j = count - 1;

            bool oddNodes = false;

            System.Windows.Point pti;
            System.Windows.Point ptj;

            for (i = 0; i < count; ++i)
            {
                pti = tCorners[i];
                ptj = tCorners[j];

                if ((pti.X <= x || ptj.X <= x) && (pti.Y < y && ptj.Y >= y || ptj.Y < y && pti.Y >= y))
                {
                    if (pti.X + (y - pti.Y) / (ptj.Y - pti.Y) * (ptj.X - pti.X) < x)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }
            return oddNodes;
        }

        /// <summary>
        /// Determine if a point is located inside of an arbituary polygon.
        /// Based on description here: http://alienryderflex.com/polygon/
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <param name="y">The y-coordinate of the point.</param>
        /// <param name="tCorners">The list of polygon vertices.</param>
        /// <returns>True if the point is contained, false if not.</returns>
        public static bool IsPointInPolygon(double x, double y, IList<System.Windows.Point> lCorners)
        {
            int count = lCorners.Count;
            int i, j = count - 1;

            bool oddNodes = false;

            System.Windows.Point pti;
            System.Windows.Point ptj;

            for (i = 0; i < count; ++i)
            {
                pti = lCorners[i];
                ptj = lCorners[j];

                if ((pti.X <= x || ptj.X <= x) && (pti.Y < y && ptj.Y >= y || ptj.Y < y && pti.Y >= y))
                {
                    if (pti.X + (y - pti.Y) / (ptj.Y - pti.Y) * (ptj.X - pti.X) < x)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }
            return oddNodes;
        }
    }
}
