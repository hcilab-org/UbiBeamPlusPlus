using System;
using System.Windows;

namespace UbiDisplays.Utilities
{
    /// <summary>Homography Matrix.  Transforms 2D-coordinate from space A to space B.  Spaces specified as a two paired rectangles.</summary>
    /// <remarks>
    /// This class is responsible for transforming a 2D-coordinate on a source rectangle onto a that of a destination rectangle.
    /// The transformation is linear and will not take into account bent or curved surfaces (the transformations are affine!).
    /// This is based on the work done by Johnny Lee and can be found here: http://johnnylee.net/projects/wii/
    /// </remarks>
    public class Warper
    {
        private double[] srcMat = new double[16];
        private double[] dstMat = new double[16];
        private double[] warpMat = new double[16];

        private bool bDirty = true;

        /// <summary>
        /// Construct a new warper class.  Initialise with a perfect mapping.
        /// </summary>
        public Warper()
        {
            setIdentity();
        }

        public void setIdentity()
        {
            // Set the source and destination rectangles to be the same.
            Source      = new Point[] { new Point(0.0f, 0.0f), new Point(1.0f, 0.0f), new Point(0.0f, 1.0f), new Point(1.0f, 1.0f) };
            Destination = new Point[] { new Point(0.0f, 0.0f), new Point(1.0f, 0.0f), new Point(0.0f, 1.0f), new Point(1.0f, 1.0f) };

            // Compute the matrix which transforms between the two coordinate spaces.
            ComputeWarp();
        }

        /// <summary>
        /// Get or set the source variables.
        /// </summary>
        public Point[] Source
        {
            get
            {
                return src;
            }
            set
            {
                // Check we have 4 elements.
                if (value == null)
                    throw new ArgumentException();
                if (value.Length != 4)
                    throw new ArgumentException();
                src = value;
                bDirty = true;
            }
        }
        private Point[] src = new Point[4];

        /// <summary>
        /// Get or set the source variables.
        /// </summary>
        public Point[] Destination
        {
            get
            {
                return dst;
            }
            set
            {
                // Check we have 4 elements.
                if (value == null)
                    throw new ArgumentException();
                if (value.Length != 4)
                    throw new ArgumentException();
                dst = value;
                bDirty = true;
            }
        }
        private Point[] dst = new Point[4];

        /*
        /// <summary>
        /// Set the source rectangle we are transforming coordinates from.
        /// </summary>
        /// <param name="x0">Top Left X</param>
        /// <param name="y0">Top Left Y</param>
        /// <param name="x1">Top Right X</param>
        /// <param name="y1">Top Right Y</param>
        /// <param name="x2">Bottom Left X</param>
        /// <param name="y2">Bottom Left Y</param>
        /// <param name="x3">Bottom Right X</param>
        /// <param name="y3">Bottom Right Y</param>
        public void setSource(float x0, float y0, float x1, float y1, float x2, float y2, float x3, float y3)
        {
            // Set the data.
            src[0].X = x0;
            src[0].Y = y0;
            src[1].X = x1;
            src[1].Y = y1;
            src[2].X = x2;
            src[2].Y = y2;
            src[3].X = x3;
            src[3].Y = y3;

            // Flag we need to change.
            bDirty = true;
        }
        

        /// <summary>
        /// Set the destination rectangle we are transforming coordinates onto.
        /// </summary>
        /// <param name="x0">Top Left X</param>
        /// <param name="y0">Top Left Y</param>
        /// <param name="x1">Top Right X</param>
        /// <param name="y1">Top Right Y</param>
        /// <param name="x2">Bottom Left X</param>
        /// <param name="y2">Bottom Left Y</param>
        /// <param name="x3">Bottom Right X</param>
        /// <param name="y3">Bottom Right Y</param>
        public void setDestination(float x0, float y0, float x1, float y1, float x2, float y2, float x3, float y3)
        {
            // Set data.
            dst[0].X = x0;
            dst[0].Y = y0;
            dst[1].X = x1;
            dst[1].Y = y1;
            dst[2].X = x2;
            dst[2].Y = y2;
            dst[3].X = x3;
            dst[3].Y = y3;

            // Flag we need to change.
            bDirty = true;
        }
        */

        /// <summary>
        /// Compute the new 'warp' matrix based on the source and destination rectangles.
        /// </summary>
        public void ComputeWarp()
        {
            computeQuadToSquare(src[0].X, src[0].Y, src[1].X, src[1].Y, src[2].X, src[2].Y, src[3].X, src[3].Y, srcMat);
            computeSquareToQuad(dst[0].X, dst[0].Y, dst[1].X, dst[1].Y, dst[2].X, dst[2].Y, dst[3].X, dst[3].Y, dstMat);
            multMats(srcMat, dstMat, warpMat);

            // Remove our flag so that we are not in need of update again until something changes.
            bDirty = false;
        }

        /// <summary>
        /// A helper function to multiply two matracies.
        /// </summary>
        /// <param name="srcMat">Source matrix as a 16-float flattened 4x4 matrix.</param>
        /// <param name="dstMat">Destination matrix as a 16-float flattened 4x4 matrix.</param>
        /// <param name="resMat">Result matrix as a 16-float flattened 4x4 matrix.</param>
        public void multMats(double[] srcMat, double[] dstMat, double[] resMat)
        {
            // DSTDO/CBB: could be faster, but not called often enough to matter
            for (int r = 0; r < 4; r++)
            {
                int ri = r * 4;
                for (int c = 0; c < 4; c++)
                {
                    resMat[ri + c] = (srcMat[ri] * dstMat[c] +
                              srcMat[ri + 1] * dstMat[c + 4] +
                              srcMat[ri + 2] * dstMat[c + 8] +
                              srcMat[ri + 3] * dstMat[c + 12]);
                }
            }
        }

        public void computeSquareToQuad(double x0, double y0, double x1, double y1, double x2, double y2, double x3, double y3, double[] mat)
        {
            double dx1 = x1 - x2, dy1 = y1 - y2;
            double dx2 = x3 - x2, dy2 = y3 - y2;
            double sx = x0 - x1 + x2 - x3;
            double sy = y0 - y1 + y2 - y3;
            double g = (sx * dy2 - dx2 * sy) / (dx1 * dy2 - dx2 * dy1);
            double h = (dx1 * sy - sx * dy1) / (dx1 * dy2 - dx2 * dy1);
            double a = x1 - x0 + g * x1;
            double b = x3 - x0 + h * x3;
            double c = x0;
            double d = y1 - y0 + g * y1;
            double e = y3 - y0 + h * y3;
            double f = y0;

            mat[0] = a; mat[1] = d; mat[2] = 0; mat[3] = g;
            mat[4] = b; mat[5] = e; mat[6] = 0; mat[7] = h;
            mat[8] = 0; mat[9] = 0; mat[10] = 1; mat[11] = 0;
            mat[12] = c; mat[13] = f; mat[14] = 0; mat[15] = 1;
        }

        public void computeQuadToSquare(double x0, double y0, double x1, double y1, double x2, double y2, double x3, double y3, double[] mat)
        {
            computeSquareToQuad(x0, y0, x1, y1, x2, y2, x3, y3, mat);

            // invert through adjoint

            double a = mat[0], d = mat[1],       /* ignore */            g = mat[3];
            double b = mat[4], e = mat[5],       /* 3rd col*/            h = mat[7];
            /* ignore 3rd row */
            double c = mat[12], f = mat[13];

            double A = e - f * h;
            double B = c * h - b;
            double C = b * f - c * e;
            double D = f * g - d;
            double E = a - c * g;
            double F = c * d - a * f;
            double G = d * h - e * g;
            double H = b * g - a * h;
            double I = a * e - b * d;

            // Probably unnecessary since 'I' is also scaled by the determinant,
            //   and 'I' scales the homogeneous coordinate, which, in turn,
            //   scales the X,Y coordinates.
            // Determinant  =   a * (e - f * h) + b * (f * g - d) + c * (d * h - e * g);
            double idet = 1.0f / (a * A + b * D + c * G);

            mat[0] = A * idet; mat[1] = D * idet; mat[2] = 0; mat[3] = G * idet;
            mat[4] = B * idet; mat[5] = E * idet; mat[6] = 0; mat[7] = H * idet;
            mat[8] = 0; mat[9] = 0; mat[10] = 1; mat[11] = 0;
            mat[12] = C * idet; mat[13] = F * idet; mat[14] = 0; mat[15] = I * idet;
        }

        /// <summary>
        /// Return a reference to the array which is the warp matrix.
        /// </summary>
        /// <returns>An array reference of a 4x4 matrix represented as a flattened 16-float array.</returns>
        public double[] getWarpMatrix()
        {
            // If our matrix is out of date, recompute it.
            if (bDirty)
                ComputeWarp();

            return warpMat;
        }

        /// <summary>
        /// Transform a point from one rectangle onto another using this class's warp matrix.
        /// </summary>
        /// <param name="pSrc">Source point.</param>
        /// <returns>Destination point.</returns>
        public Point Transform(Point pSrc)
        {
            // If our matrix is out of date, recompute it.
            if (bDirty)
                ComputeWarp();

            // Compute the coordinate transform.
            double dstX = 0, dstY = 0;
            Warper.warp(warpMat, pSrc.X, pSrc.Y, ref dstX, ref dstY);
            return new Point(dstX, dstY);
        }

        /// <summary>
        /// Transform a point from one rectangle onto another using this class's warp matrix.
        /// </summary>
        /// <param name="srcX">Source point, X coordinate.</param>
        /// <param name="srcY">Source point, Y coordinate.</param>
        /// <param name="dstX">Destination point, X coordinate.</param>
        /// <param name="dstY">Destination point, Y coordinate.</param>
        public void Transform(double srcX, double srcY, ref double dstX, ref double dstY)
        {
            // If our matrix is out of date, recompute it.
            if (bDirty)
                ComputeWarp();

            // Compute the coordinate transform.
            Warper.warp(warpMat, srcX, srcY, ref dstX, ref dstY);
        }

        /// <summary>
        /// Transform a point from one rectangle onto another using this class's warp matrix.
        /// </summary>
        /// <param name="srcX">Source point, X coordinate.</param>
        /// <param name="srcY">Source point, Y coordinate.</param>
        /// <param name="dstX">Destination point, X coordinate.</param>
        /// <param name="dstY">Destination point, Y coordinate.</param>
        public void Transform(double srcX, double srcY, ref double dstX, ref double dstY, double fScale)
        {
            // If our matrix is out of date, recompute it.
            if (bDirty)
                ComputeWarp();

            // Compute the coordinate transform.
            Warper.warp(warpMat, srcX, srcY, ref dstX, ref dstY);
            dstX *= fScale;
            dstY *= fScale;
        }

        /// <summary>
        /// Transform a point from one rectangle onto another using this class's warp matrix.
        /// </summary>
        /// <param name="srcX">Source point, X coordinate.</param>
        /// <param name="srcY">Source point, Y coordinate.</param>
        /// <returns>The transformed point.</returns>
        public Point Transform(double srcX, double srcY)
        {
            // If our matrix is out of date, recompute it.
            if (bDirty)
                ComputeWarp();

            // Compute the coordinate transform.
            double dstX = 0;
            double dstY = 0;
            Warper.warp(warpMat, srcX, srcY, ref dstX, ref dstY);
            return new Point(dstX, dstY);
        }

        /// <summary>
        /// Use a given matrix to transform a point on one rectangle onto another.
        /// </summary>
        /// <param name="mat">A reference to the array which describes the matrix we want to use.</param>
        /// <param name="srcX">Source point, X coordinate.</param>
        /// <param name="srcY">Source point, Y coordinate.</param>
        /// <param name="dstX">Destination point, X coordinate.</param>
        /// <param name="dstY">Destination point, Y coordinate.</param>
        public static void warp(double[] mat, double srcX, double srcY, ref double dstX, ref double dstY)
        {
            double[] result = new double[4];
            double z = 0;
            result[0] = (srcX * mat[0] + srcY * mat[4] + z * mat[8] + 1 * mat[12]);
            result[1] = (srcX * mat[1] + srcY * mat[5] + z * mat[9] + 1 * mat[13]);
            result[2] = (srcX * mat[2] + srcY * mat[6] + z * mat[10] + 1 * mat[14]);
            result[3] = (srcX * mat[3] + srcY * mat[7] + z * mat[11] + 1 * mat[15]);
            dstX = result[0] / result[3];
            dstY = result[1] / result[3];
        }
    }
}
