using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimMath;

namespace UbiDisplays.Utilities
{
    /// <summary>
    /// The RatcliffPlane provides the functionality to compute a best for plane or a series of given points.
    /// This is based on the work of John Ratcliff: http://codesuppository.blogspot.com/2006/03/best-fit-plane.html
    /// </summary>
    public class RatcliffPlane
    {
        /// <summary>
        /// Find the distance between a point in 3D space and the closest point on a 3D plane.
        /// This uses the formula:
        /// distance = abs(Ax + By + Cz + d) / sqrt(A*A + B*B + C*C);
        /// </summary>
        /// <param name="vPoint">The point in 3D space.</param>
        /// <param name="tPlane">A plane in 3D space.</param>
        /// <returns>The distance between the point and the closest point on the plane.</returns>
        public static double Distance(Vector3 vPoint, Plane tPlane)
        {
            var n = tPlane.Normal;
            double fDistance = Math.Abs(
                (vPoint.X * n.X) +
                (vPoint.Y * n.Y) +
                (vPoint.Z * n.Z) + tPlane.D) / Math.Sqrt((n.X * n.X) + (n.Y * n.Y) + (n.Z * n.Z));
            return fDistance;
        }

        /// <summary>
        /// Compute a plane which best fits a series of input points.
        /// </summary>
        /// <param name="tPoints">The array of points to analyse.</param>
        /// <returns>The plane of best fit.</returns>
        public static Plane ComputeBestFit(Vector3[] tPoints)
        {
            return RatcliffPlane.ComputeBestFit(tPoints, null);
        }

        /// <summary>
        /// Compute a plane which best fits a series of input points.
        /// </summary>
        /// <param name="tPoints">The array of points to analyse.</param>
        /// <param name="tWeights">The relative weightings to apply to each point.</param>
        /// <returns>The plane of best fit.</returns>
        public static Plane ComputeBestFit(Vector3[] tPoints, float[] tWeights)
        {
            Vector3 kOrigin = new Vector3();
            float w = 1;
            float wtotal = 0;

            // Calculate the weighted center of all the points.
            for (int i = 0, n = tPoints.Length; i < n; ++i)
            {
                w = 1;
                if (tWeights != null)
                    w *= tWeights[i];

                Vector3 p = tPoints[i];
                kOrigin.X += p.X * w;
                kOrigin.Y += p.Y * w;
                kOrigin.Z += p.Z * w;

                wtotal += w;
            }


            float recip = 1.0f / wtotal; // reciprocol of total weighting

            kOrigin.X *= recip;
            kOrigin.Y *= recip;
            kOrigin.Z *= recip;

            float fSumXX = 0;
            float fSumXY = 0;
            float fSumXZ = 0;

            float fSumYY = 0;
            float fSumYZ = 0;
            float fSumZZ = 0;

            //--
            for (int i = 0, n = tPoints.Length; i < n; ++i)
            {
                w = 1;
                if (tWeights != null)
                    w *= tWeights[i];

                // Transform to center and apply vertex weighting.
                Vector3 p = tPoints[i];
                Vector3 kDiff = new Vector3(w * (p.X - kOrigin.X), w * (p.Y - kOrigin.Y), w * (p.Z - kOrigin.Z));

                fSumXX += kDiff.X * kDiff.X; // sum of the squares of the differences.
                fSumXY += kDiff.X * kDiff.Y; // sum of the squares of the differences.
                fSumXZ += kDiff.X * kDiff.Z; // sum of the squares of the differences.

                fSumYY += kDiff.Y * kDiff.Y;
                fSumYZ += kDiff.Y * kDiff.Z;
                fSumZZ += kDiff.Z * kDiff.Z;
            }
            //--

            fSumXX *= recip;
            fSumXY *= recip;
            fSumXZ *= recip;
            fSumYY *= recip;
            fSumYZ *= recip;
            fSumZZ *= recip;

            // Setup an eigensolver.
            Eigen kES = new Eigen();

            kES.mElement[0, 0] = fSumXX;
            kES.mElement[0, 1] = fSumXY;
            kES.mElement[0, 2] = fSumXZ;

            kES.mElement[1, 0] = fSumXY;
            kES.mElement[1, 1] = fSumYY;
            kES.mElement[1, 2] = fSumYZ;

            kES.mElement[2, 0] = fSumXZ;
            kES.mElement[2, 1] = fSumYZ;
            kES.mElement[2, 2] = fSumZZ;

            // compute eigenstuff, smallest eigenvalue is in last position
            kES.DecrSortEigenStuff();

            Vector3 kNormal = new Vector3();
            kNormal.X = kES.mElement[0, 2];
            kNormal.Y = kES.mElement[1, 2];
            kNormal.Z = kES.mElement[2, 2];

            // The minimum energy.
            return new Plane(kNormal, 0 - Vector3.Dot(kNormal, kOrigin));
        }

        /// <summary>
        /// An eigen solver.
        /// </summary>
        private class Eigen
        {
            public float[,] mElement = new float[3, 3];
            public float[] m_afDiag = new float[3];
            public float[] m_afSubd = new float[3];
            public bool m_bIsRotation;

            public void DecrSortEigenStuff()
            {
                Tridiagonal(); //diagonalize the matrix.
                QLAlgorithm(); //
                DecreasingSort();
                GuaranteeRotation();
            }

            public void Tridiagonal()
            {
                float fM00 = mElement[0, 0];
                float fM01 = mElement[0, 1];
                float fM02 = mElement[0, 2];
                float fM11 = mElement[1, 1];
                float fM12 = mElement[1, 2];
                float fM22 = mElement[2, 2];

                m_afDiag[0] = fM00;
                m_afSubd[2] = 0;
                if (fM02 != 0.0f)
                {
                    float fLength = (float)Math.Sqrt(fM01 * fM01 + fM02 * fM02);
                    float fInvLength = ((float)1.0) / fLength;
                    fM01 *= fInvLength;
                    fM02 *= fInvLength;
                    float fQ = ((float)2.0) * fM01 * fM12 + fM02 * (fM22 - fM11);
                    m_afDiag[1] = fM11 + fM02 * fQ;
                    m_afDiag[2] = fM22 - fM02 * fQ;
                    m_afSubd[0] = fLength;
                    m_afSubd[1] = fM12 - fM01 * fQ;
                    mElement[0, 0] = 1.0f;
                    mElement[0, 1] = 0.0f;
                    mElement[0, 2] = 0.0f;
                    mElement[1, 0] = 0.0f;
                    mElement[1, 1] = fM01;
                    mElement[1, 2] = fM02;
                    mElement[2, 0] = 0.0f;
                    mElement[2, 1] = fM02;
                    mElement[2, 2] = -fM01;
                    m_bIsRotation = false;
                }
                else
                {
                    m_afDiag[1] = fM11;
                    m_afDiag[2] = fM22;
                    m_afSubd[0] = fM01;
                    m_afSubd[1] = fM12;
                    mElement[0, 0] = 1.0f;
                    mElement[0, 1] = 0.0f;
                    mElement[0, 2] = 0.0f;
                    mElement[1, 0] = 0.0f;
                    mElement[1, 1] = 1.0f;
                    mElement[1, 2] = 0.0f;
                    mElement[2, 0] = 0.0f;
                    mElement[2, 1] = 0.0f;
                    mElement[2, 2] = 1.0f;
                    m_bIsRotation = true;
                }
            }

            public bool QLAlgorithm()
            {
                const int iMaxIter = 32;

                for (int i0 = 0; i0 < 3; i0++)
                {
                    int i1;
                    for (i1 = 0; i1 < iMaxIter; i1++)
                    {
                        int i2;
                        for (i2 = i0; i2 <= (3 - 2); i2++)
                        {
                            float fTmp = Math.Abs(m_afDiag[i2]) + Math.Abs(m_afDiag[i2 + 1]);
                            if (Math.Abs(m_afSubd[i2]) + fTmp == fTmp)
                                break;
                        }
                        if (i2 == i0)
                        {
                            break;
                        }

                        float fG = (m_afDiag[i0 + 1] - m_afDiag[i0]) / (((float)2.0) * m_afSubd[i0]);
                        float fR = (float)Math.Sqrt(fG * fG + (float)1.0);
                        if (fG < (float)0.0)
                        {
                            fG = m_afDiag[i2] - m_afDiag[i0] + m_afSubd[i0] / (fG - fR);
                        }
                        else
                        {
                            fG = m_afDiag[i2] - m_afDiag[i0] + m_afSubd[i0] / (fG + fR);
                        }
                        float fSin = (float)1.0, fCos = (float)1.0, fP = (float)0.0;
                        for (int i3 = i2 - 1; i3 >= i0; i3--)
                        {
                            float fF = fSin * m_afSubd[i3];
                            float fB = fCos * m_afSubd[i3];
                            if (Math.Abs(fF) >= Math.Abs(fG))
                            {
                                fCos = fG / fF;
                                fR = (float)Math.Sqrt(fCos * fCos + (float)1.0);
                                m_afSubd[i3 + 1] = fF * fR;
                                fSin = ((float)1.0) / fR;
                                fCos *= fSin;
                            }
                            else
                            {
                                fSin = fF / fG;
                                fR = (float)Math.Sqrt(fSin * fSin + (float)1.0);
                                m_afSubd[i3 + 1] = fG * fR;
                                fCos = ((float)1.0) / fR;
                                fSin *= fCos;
                            }
                            fG = m_afDiag[i3 + 1] - fP;
                            fR = (m_afDiag[i3] - fG) * fSin + ((float)2.0) * fB * fCos;
                            fP = fSin * fR;
                            m_afDiag[i3 + 1] = fG + fP;
                            fG = fCos * fR - fB;
                            for (int i4 = 0; i4 < 3; i4++)
                            {
                                fF = mElement[i4, i3 + 1];
                                mElement[i4, i3 + 1] = fSin * mElement[i4, i3] + fCos * fF;
                                mElement[i4, i3] = fCos * mElement[i4, i3] - fSin * fF;
                            }
                        }
                        m_afDiag[i0] -= fP;
                        m_afSubd[i0] = fG;
                        m_afSubd[i2] = (float)0.0;
                    }
                    if (i1 == iMaxIter)
                    {
                        return false;
                    }
                }
                return true;
            }

            public void DecreasingSort()
            {
                //sort eigenvalues in decreasing order, e[0] >= ... >= e[iSize-1]
                for (int i0 = 0, i1; i0 <= 3 - 2; i0++)
                {
                    // locate maximum eigenvalue
                    i1 = i0;
                    float fMax = m_afDiag[i1];
                    int i2;
                    for (i2 = i0 + 1; i2 < 3; i2++)
                    {
                        if (m_afDiag[i2] > fMax)
                        {
                            i1 = i2;
                            fMax = m_afDiag[i1];
                        }
                    }

                    if (i1 != i0)
                    {
                        // swap eigenvalues
                        m_afDiag[i1] = m_afDiag[i0];
                        m_afDiag[i0] = fMax;
                        // swap eigenvectors
                        for (i2 = 0; i2 < 3; i2++)
                        {
                            float fTmp = mElement[i2, i0];
                            mElement[i2, i0] = mElement[i2, i1];
                            mElement[i2, i1] = fTmp;
                            m_bIsRotation = !m_bIsRotation;
                        }
                    }
                }
            }

            public void GuaranteeRotation()
            {
                if (!m_bIsRotation)
                {
                    // change sign on the first column
                    for (int iRow = 0; iRow < 3; iRow++)
                    {
                        mElement[iRow, 0] = -mElement[iRow, 0];
                    }
                }
            }
        }
    }
}
