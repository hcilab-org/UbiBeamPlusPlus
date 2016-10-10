using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.Native
{
	/**
	 * @brief KnuthVariance is a continuious fixed-memory running average.
	 * Based on this: http://www.johndcook.com/standard_deviation.html
	 * Usage:
	 *
	 *    var k = new MovingVariance(length)
	 *    k.push(new_value_1)
	 *    k.push(new_value_2)
	 *    var mean = k.mean()
	 *    var std = k.std()
	 *
	 * @author John Hardy
	 */
	class KnuthVariance
	{
		private int n = 0;		// The number of values considered.
	
		private double _oldM = 0;	// The old mean.
		private double _newM = 0;	// The new mean.
		private double _oldS = 0;	// The old variance.
		private double _newS = 0;	// The new variance.
	
		/**
		 * @brief Create a new running variance tracker.
		 */
		public KnuthVariance()
		{
			this.reset();
		}
	
		/**
		 * @brief Reset this running variance tracker.
		 */
		public void reset()
		{
			this.n = 0;
		}
	
		/**
		 * @brief Push a value so that it is accounted for in this variance tracker.
		 * @param x The value to account for.
		 */
		public void push(double x)
		{
			this.n++;
		
			// See Knuth TAOCP vol 2, 3rd edition, page 232
			if (this.n == 1)
			{
				this._oldM = this._newM = x;
				this._oldS = 0.0;
			}
			else
			{
				this._newM = this._oldM + (x - this._oldM) / this.n;
				this._newS = this._oldS + (x - this._oldM) * (x - this._newM);
			
				// set up for next iteration
				this._oldM = this._newM; 
				this._oldS = this._newS;
			}
		}
	
		/**
		 * @brief Return the number of values used to compute this average.
		 * @return An integer.
		 */
		public int numvalues() { return this.n; }
	
		/**
		 * @brief Return the mean of the values pushed to this running average.
		 * @return A float.
		 */
		public double mean() { return (this.n > 0) ? this._newM : 0.0; }
	
		/**
		 * @brief Return the mean of the values pushed to this running average.
		 * @return A float.
		 */
		public double variance() { return ( (this.n > 1) ? this._newS / (this.n - 1) : 0.0 ); }
	
		/**
		 * @brief Return the mean of the values pushed to this running average.
		 * @return A float.
		 */
		public double std() { return Math.Sqrt(this.variance()); }
	}
}
