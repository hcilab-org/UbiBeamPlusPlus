using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Utilities
{
    /// <summary>A running average</summary>
    /// <remarks>
    /// This class collects the statistics mean and standard deviation on a set over time.
    /// This uses O(1) memory and has O(1) primary time complexity over a set of n elements.
    /// </remarks>
    /// <author>John Hardy</author>
    public class RunningStat
    {
        /// <summary>
        /// The item count.
        /// </summary>
        private long iCount = 0;

        /// <summary>
        /// Are we beyond the first item.
        /// </summary>
        private bool bStarted = false;

        /// <summary>
        /// The last mean.
        /// </summary>
        private double fLastMean = 0.0;

        /// <summary>
        /// The Current mean.
        /// </summary>
        private double fMean = 0.0;

        /// <summary>
        /// The Current Variance.
        /// </summary>
        private double fVar = 0.0;

        /// <summary>
        /// Create a new data set and init the varaibles.
        /// </summary>
        public RunningStat()
        {
        }

        /// <summary>
        /// Clear all the data in this data set.
        /// </summary>
        public void Clear()
        {
            this.iCount = 0;
        }
	
        /// <summary>
        /// Add an item into this data set.
        /// </summary>
        /// <param name="The">value to append.</param>
        public void Append(double x)
        {
            // Update the count.
            ++iCount;

            // If we have started collecting.
            if (bStarted)
            {
                fLastMean = fMean;
			    fMean += (x - fMean) / iCount;
			    fVar  += (x - fLastMean) * (x - fMean);
            }
            else
            {
                fMean = 0.0;
                fVar = 0.0;
                bStarted = true;
            }
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
        public double Mean { get { return fMean; } }
	
        /// <summary>
        /// Returns the variance of this data set.
        /// </summary>
        /// <returns>The variance of this data set</returns>
        public double Variance { get { return fVar / (double)iCount; } }

        /// <summary>
        /// Returns the standard deviation of this data set.
        /// </summary>
        /// <returns>The standard deviation of this data set</returns>
        public double StandardDeviation  { get { return Math.Sqrt(Variance); } }
    }
}
