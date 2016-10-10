using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.Native
{
	/**
	 * @brief  KalmanDouble is a float value which is filtered with a KalmanFilter.
	 * Usage:
	 *
	 *    var k = new KalmanDouble(start_value)
	 *    k.push(new_value_1)
	 *    k.push(new_value_2)
	 *    var filtered_value = k.get()
	 *
	 * @author John Hardy
	 */
	class KalmanDouble
	{
		private double A = 1.0;	// Factor of real value to previous real value
		private double B = 0.0;	// Factor of real value to real control signal
		private double H = 1.0;	// Factor of measured value to real value
		private double Q = 0.01;
	
		private double P = 1.0;		// 
		public double noise = 0.4;	// Enviromental noise.

		private double value = 0.0;	// The value .
		private double last = 0.0;		// The last value.

		private double LP;
	
		/**
		 * @brief Create a new Kalman filtered double.
		 * @param fStartingValue The starting value for the filter.
		 */
		public KalmanDouble(double fStartingValue = 0)
		{
			this.reset(fStartingValue);
		}
	
		/**
		 * @brief Reset this Kalman filtered double.
		 * @param fStartingValue The starting value for the filter.
		 */
		public void reset(double fStartingValue)
		{
			this.LP = 0.1;
			this.value = fStartingValue;
			this.last = fStartingValue;
		}
	
		/**
		 * @brief Push a value into the filter to modify it by it.
		 * @param fValue The value to filter.
		 */
		public void push(double fValue)
		{
			// time update - prediction
			this.last = this.A * this.last;
			this.LP = this.A * this.LP * this.A + this.Q;
		
			// measurement update - correction
			var K = this.LP * this.H / (this.H * this.LP * this.H + this.noise);
			this.last = this.last + K * (fValue - this.H * this.last);
			this.LP = (1.0 - K * this.H) * this.LP;
		
			// Store the update.
			this.value = this.last;
		}
	
		/**
		 * @brief Return the current filtered value.
		 */
		public double get()
		{
			return this.value;
		}
	}
}
