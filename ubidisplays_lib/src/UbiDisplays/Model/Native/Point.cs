using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.Native
{
	public class Point
	{
		public double x;
		public double y;
		public double z;
		public Cluster cluster;
		public bool visited;
		public bool noise;

		public Point()
		{
			cluster = null;
			visited = false;
			noise = false;
		}

		public Point(double x, double y, double z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			cluster = null;
			visited = false;
			noise = false;
		}

		public double dimension(int dim)
		{
			switch (dim)
			{
				case 0:
					return x;
				case 1:
					return y;
				case 2:
					return z;
				default:
					throw new Exception("Tried to enter an Unknown dimension.");
			}
		}

		public double dimension(string s)
		{
			switch (s)
			{
				case "x":
					return x;
				case "y":
					return y;
				case "z":
					return z;
				default:
					throw new Exception("Tried to enter an Unknown dimension.");
			}
		}

		public void setDimension(string s, double value)
		{
			switch (s)
			{
				case "x":
					x = value;
					break;
				case "y":
					y = value;
					break;
				case "z":
					z = value;
					break;
				default:
					throw new Exception("Tried to enter an Unknown dimension.");
			}
		}
	}
}
