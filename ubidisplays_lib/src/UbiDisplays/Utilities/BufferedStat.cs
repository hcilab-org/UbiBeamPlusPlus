using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Utilities
{
    /// <summary>A windowed average</summary>
    /// <remarks>
    /// This class collects the statistics mean and standard deviation on a running set of n elements.
    /// Adding more than n elements will cause the oldest elements to be overwritten.
    /// This uses O(n) memory and has O(2n) worst case primary time complexity over a set of n elements.
    /// </remarks>
    /// <author>John Hardy</author>
    public class BufferedStat
    {
        /// <summary>
        /// The data buffer.
        /// </summary>
        public double[] tBuffer = null;

        /// <summary>
        /// The write pointer into the buffer.
        /// </summary>
        public int iNext = 0;

        /// <summary>
        /// The number of elements contained in the set.
        /// </summary>
        private int iCount = 0;

        /// <summary>
        /// Do we need to recalculate the mean?
        /// </summary>
        private bool bMeanDirty = false;

        /// <summary>
        /// Do we need to recalculate the variannce?
        /// </summary>
        private bool bVarianceDirty = false;

        /// <summary>
        /// The temp mean.
        /// </summary>
        private double fMean;

        /// <summary>
        /// The temp variance.
        /// </summary>
        private double fVariance;

        /// <summary>
        /// Create a new BufferedStat class with a given number of items which we can store data about.
        /// </summary>
        /// <param name="iBufferSize">The size (i.e. 5 for a running average of 5 items).</param>
        public BufferedStat(int iBufferSize)
        {
            // Create the array.
            tBuffer = new double[iBufferSize];
            this.Clear();
        }

        /// <summary>
        /// Clear all the data in this data set.
        /// </summary>
        public void Clear()
        {
            this.iNext = 0;
            this.iCount = 0;

            this.bMeanDirty = true;
            this.bVarianceDirty = true;
        }

        /// <summary>
        /// Add an item into this data set.
        /// </summary>
        /// <param name="The">value to append.</param>
        public void Append(double x)
        {
            // Compute the count.
            this.iCount = Math.Min(this.iCount + 1, tBuffer.Length);

            // Increment the next counter and wrap-around as necessary.
            this.iNext = (this.iNext + 1) % iCount;

            // Write the data into the array.
            this.tBuffer[this.iNext] = x;

            // Flag us as dirty.
            this.bMeanDirty = true;
            this.bVarianceDirty = true;
        }

        /// <summary>
        /// Return the number of items in the set.
        /// </summary>
        /// <param name="The">number of items in the set.</param>
        public long Count { get { return iCount; } }

        /// <summary>
        /// Returns the mean of this data set.
        /// </summary>
        /// <returns>The mean of this data set</returns>
        public double Mean
        {
            get
            {
                if (bMeanDirty)
                {
                    fMean = 0;
                    for (int i = 0; i < iCount; ++i)
                        fMean += tBuffer[i];
                    fMean /= iCount;

                    bMeanDirty = false;
                }
                return fMean;
            }
        }

        /// <summary>
        /// Returns the variance of this data set.
        /// </summary>
        /// <returns>The variance of this data set</returns>
        public double Variance
        {
            get
            {
                if (bVarianceDirty)
                {
                    if (iCount == 0)
                        throw new Exception("Cannot calculate the variance of 0 values.");

                    var mean = this.Mean;
                    double fSumSquares = 0.0, fSum = 0.0;
                    for (int i = 0; i < iCount; ++i)
                    {
                        double dev = tBuffer[i] - mean;
                        fSumSquares += dev * dev;
                        fSum += dev;
                    }
                    fVariance = (fSumSquares - fSum * fSum / iCount) / iCount;


                    bVarianceDirty = false;
                }
                return fVariance;
            }
        }

        /// <summary>
        /// Returns the standard deviation of this data set.
        /// </summary>
        /// <returns>The standard deviation of this data set</returns>
        public double StandardDeviation { get { return Math.Sqrt(Variance); } }
    }
}
