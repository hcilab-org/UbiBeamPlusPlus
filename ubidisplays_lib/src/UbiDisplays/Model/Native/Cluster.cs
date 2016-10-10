using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.Native
{
	public class Cluster
	{
		public bool assigned = false;
		// List of points.
		public List<Point> points = new List<Point>();
		
		// AABB.
		private double minx = 999999999;
		private double miny = 999999999;
		private double maxx = -999999999;
		private double maxy = -999999999;
		
		// Running average (we don't know how many points there will be).
		private KnuthVariance avgx = new KnuthVariance();
		private KnuthVariance avgy = new KnuthVariance();
		
		// Variables for the line of best fit.
		private double sigmaX = 0;
		private double sigmaY = 0;
		private double sigmaX2 = 0;
		private double sigmaXY = 0;
		
		public void addPoint(Point p) {
			// Add it to the list of points.
			this.points.Add(p);
			
			// Store the AABB.
			this.minx = Math.Min(p.x, this.minx);
			this.miny = Math.Min(p.y, this.miny);
			this.maxx = Math.Max(p.x, this.maxx);
			this.maxy = Math.Max(p.y, this.maxy);
			
			// Compute some averages for it etc.
			this.avgx.push(p.x);
			this.avgy.push(p.y);
			
			// Line of best fit.
			this.sigmaX += p.x;
			this.sigmaY += p.y;
			this.sigmaX2 += p.x*p.x;
			this.sigmaXY += p.x*p.y;
		}
		
		/** @brief Calculate the slope of all the points in this cluster. */
		public double slope() {
			return ((this.points.Count * this.sigmaXY) - (this.sigmaX * this.sigmaY)) / ((this.points.Count * this.sigmaX2) - (this.sigmaX * this.sigmaX));
		}
		
		/** @brief Calculate the intercept of all the points in this cluster. */
		public double intercept() {
			return (this.sigmaY - (this.slope() * this.sigmaX)) / this.points.Count;
		}
		
		/** @brief Calculate the aspect ratio of all the points in this cluster. */
		public double aspectratio() {
			if (this.points.Count == 1)
				return 1.0;
			return (this.maxx - this.minx) / (this.maxy - this.miny);
		}
		
		/** @brief Calculate the volume taken up by all the points in this cluster. */
		public double volume() {
			var w = (this.maxx - this.minx);
			var h = (this.maxy - this.miny);
			return w * h;
		}
		
		/** @brief Calculate the average x-value of all the points in this cluster. */
		public double centerx() {
			return this.avgx.mean();
		}
		
		/** @brief Calculate the average y-value of all the points in this cluster. */
		public double centery() {
			return this.avgy.mean();
		}
		
		/** @brief Calculate the radius of all the points in this cluster. */
		public double radius() {
			// Get the center point.
			double x = this.centerx();
			double y = this.centerx();
			
			// Compute square distance to max.
			var fMin = Math.Abs(Math.Pow(this.minx - x, 2) +  Math.Pow(this.miny - y, 2));
			
			// Compute square distance to min.
			var fMax = Math.Abs(Math.Pow(this.maxx - x, 2) +  Math.Pow(this.maxy - y, 2));
			
			// Return the length of the largest square distance.
			return Math.Sqrt(Math.Max(fMax, fMin));
		}
	}
}
