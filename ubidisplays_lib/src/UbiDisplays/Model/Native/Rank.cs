using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.Native
{
	class Rank
	{
		public Cluster cluster;
		public TouchPoint tracker;
		public double rank;

		public Rank(Cluster cluster, TouchPoint tracker, double rank)
		{
			this.cluster = cluster;
			this.tracker = tracker;
			this.rank = rank;
		}
	}
}
