using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.Native
{
	public class TouchTracker
	{
		/**
		 * @brief The number of frames to wait before we consider a tracker as active and working.
		 * Default = 2.  Setting this to 1 makes it instantly start the tracker. 
		 * The lower this number, the faster it will react to a finger.  However, it also makes it more succeptibe to noise.
		 */
		public int FramePersistance = 2;
	
		/** @brief The number of trackers we have ever had active.  Gives each 'touch' an id. */
		public int TrackerCount = 0;
	
		/** @brief The number of milliseconds to wait before removing the touch point if no data. */
		int TouchRemoveTime = 100;
	
		/** @brief The kinect point voxel width in pixels. This is set when updateSurface() is called. */
		public double DebugVoxelWidth = 2;
	
		/** @brief The kinect point voxel height in pixels. This is set when updateSurface() is called. */
		public double DebugVoxelHeight = 2;
	
		/** @brief The size of a kinect voxel. Default = 5mm. */
		double KinectVoxel = 0.005;
	
		/** @brief The distance at which we can be sure the inputs are not the same (from frame to frame).  Given in meters. */
		double DistanceCutOff = 0.07;
	
		/** @brief The size of the largest finger (in meters).  Anything larger will not be detected as a finger. */
		double LargestFinger  = 0.04;
	
		/** The debug colours for the touch points. */
		List<string> DebugColours = new List<string>(){"red", "green", "blue", "yellow", "orange", "lime"};
	
		/** @brief Is debug mode enabled or not. */
		bool _Debug = false;
	
		/** @brief A layer that sits atop everything which we can draw debug data onto. */
		Div _DebugLayer = null;
		public Div _DebugLayerStatic = null;
	
		/** @brief A list of tracked touch points. */
		List<TouchPoint> _Tracked = new List<TouchPoint>();
	
		public delegate void Dispatcher(TouchPoint point);
		/** @brief A function that is called with all the newly created touch points. */
		public Dispatcher DispatchStart = null;
	
		/** @brief A function that is called with all the old touch points. */
		public Dispatcher DispatchStop  = null;
	
		/** @brief A function that is called with all the updated touch points. */
		public Dispatcher DispatchMove  = null;

		/**
		 * @brief Create a new touch tracker.
		 */
		public TouchTracker(bool debug = false, bool trails = false, Dispatcher start = null, Dispatcher stop = null, Dispatcher update = null) {
			this.debug(debug);
			this.trails(trails);
		
			// Parse out handlers.
			this.DispatchStart = start;
			this.DispatchStop = stop;
			this.DispatchMove = update;
		
			// Setup the surface.
			this.updateSurface();
		}

		/**
		 * @brief Converts x pixels to meters.
		 * @param f The number of pixels.
		 * @param axis "w" or "h".
		 * @return f pixels in meters.
		 */
		private static double Convert_Pixels2Meters(double f, string axis) {
			if (axis == "w") return (f * NativeDisplay.Surface.Width) / window.innerWidth;
			if (axis == "h") return (f * NativeDisplay.Surface.Height) / window.innerHeight;
			return 0;
		}

		/**
		 * @brief Converts x meters to pixels.
		 * @param f The number of meters.
		 * @param axis "w" or "h".
		 * @return f meters in pixels.
		 */
		private static double Convert_Meters2Pixels(double f, string axis) {
			if (axis == "w") return (f * (window.innerWidth / NativeDisplay.Surface.Width));
			if (axis == "h") return (f * (window.innerHeight / NativeDisplay.Surface.Height));
			return 0;
		}
	
		/**
		 * @brief Process a set of points and dispatch touch points.
		 * @param lPoints Points in the format: [{x:3, y:1, z:3}, {x:1, y:2, z:3}]
		 */
		public void process(List<Point> lPoints) {
			// Update the voxel sizes.
			this.updateSurface();
		
			// Hide everything on the debug layer.
			if (this._DebugLayer != null)
			{
				// this._DebugLayer.empty();
				while (this._DebugLayer.hasChildNodes()) {
					this._DebugLayer.removeChild(this._DebugLayer.lastChild);
				}
			}
		
			// Perform the DB scan.
			// The idea is to get the smallest distance (~2.5) with the largest minpoints (~5). ~3 works well.
			// 3 * 100 = 300 BECAUSE we are not in 0-1 space but 0-100 space.
			var fBest = (300.0 * this.DebugVoxelWidth) / window.innerWidth;  // 300 here is the scale factor on the voxel size.
		
			var lClusters = dbScan(lPoints, fBest, 5, true); // eps = 2.5 // this.DebugVoxelWidth * 0.25
			var lTracked = this._Tracked;
		
			// Classify the fingers from clusters.
            List<Cluster> lFingers = new List<Cluster>();
			for (var i = 0; i < lClusters.Count; ++i) {
			
				// Phase 1: Volume (disregard very large clusters)
				Double fVolume = lClusters[i].volume();
			
				// Make it correct size for voxel.
				Double fEstX = 100 * Convert_Meters2Pixels(this.LargestFinger, "w") / window.innerWidth;
                Double fEstY = 100 * Convert_Meters2Pixels(this.LargestFinger, "h") / window.innerWidth; // TODO check innerHeight?
                Double fEst = fEstX * fEstY;
				// 10 * 100 = 1000 BECAUSE we are not in 0-1 space but 0-100 space.
				//var fEst = ((1000 * this.DebugVoxelWidth * this.DebugVoxelWidth) / $(document).width());
				//$("#surf_name").text(Math.round(fVolume, 5) + ", " + Math.round(fEst, 5));
				if (fVolume > fEst) // fVolume > 60
					continue;
			
				// Disregard those with a given s.
				// TODO: Use residuals to detect "tightness" of the line.
				// http://en.wikipedia.org/wiki/File:Correlation_examples2.svg
				//var vSlope = Math.abs(lClusters[i].slope());
				//console.log(vSlope);
				//if (vSlope > 1.0)
				//	continue;
			
				// Now, this cluster passed the classifer so accept it as a finger!
				lFingers.Add(lClusters[i]);
			}
		
			// Generate a list of ranked matches. FIX: Distance cut off.
            List<Rank> lRanks = new List<Rank>();
			for (var i = 0; i < lTracked.Count; ++i) {
				var pTracker = lTracked[i];
				for (var j = 0; j < lFingers.Count; ++j) {
					var pCluster = lFingers[j];
				
					// Rank the two based on distance.
					var fRank = Math.Pow(pCluster.centerx() - pTracker.centerx(), 2) +  Math.Pow(pCluster.centery() - pTracker.centery(), 2);
				
					// TODO: Store the pair if the distance is not crazy. * 10 * 100 = 1000 BECAUSE we are not in 0-1 space but 0-100 space.
					//var fDist = 1000 * (this.DebugVoxelWidth / $(document).width()) ;//30;
					var fDist = 100 * Convert_Meters2Pixels(this.DistanceCutOff, "w") / window.innerWidth;
					if (fRank < (fDist * fDist)) // Square distance.
						lRanks.Add(new Rank(pCluster, pTracker, fRank));
				}
			}
		
			// Sort the rank table.
			lRanks.Sort((Rank a, Rank b) => {
				if (a.rank < b.rank) return -1;
				if (a.rank > b.rank) return 1;
				return 0;
			});
		
			// Flag all the trackers and clusters as unassigned.
			for (var i = 0; i < lTracked.Count; ++i) lTracked[i].assigned = false;
			for (var i = 0; i < lFingers.Count; ++i) lFingers[i].assigned = false;
		
			// Match up the best ones.
			for (var i = 0; i < lRanks.Count; ++i) {
				// Skip if the tracker or the cluster has been assigned.
				if (lRanks[i].cluster.assigned == true || lRanks[i].tracker.assigned == true)
					continue;
			
				// Assign the best match! woo!
				lRanks[i].tracker.consume(lRanks[i].cluster);
				lRanks[i].cluster.assigned = true;
				lRanks[i].tracker.assigned = true;
			}
		
			// If we have clusters left over, make them into touch points.
			for (var i = 0; i < lFingers.Count; ++i) {
				if (lFingers[i].assigned == false)
				{
					// Make a new tracker.
					var pTracker = new TouchPoint(this, lFingers[i]);
					lTracked.Add(pTracker);
				
				}
			}
		
			// If we have touch points left, remove them.
			var lRemove = new List<TouchPoint>();
			for (var i = 0; i < lTracked.Count; ++i)
			{
				if (lTracked[i].assigned == false)
				{
					// Suggest this tracker should die as it didn't recieve an update this frame.
					if (lTracked[i].consumeNothing())
						lRemove.Add(lTracked[i]);
				}
			}
		
			// Remove them from the array.
			for (var i = 0; i < lRemove.Count; ++i)
			{
				lRemove[i].remove();
				lTracked.Remove(lRemove[i]);
			}
		
			// Draw debug data for all the trackers.
			if (this._DebugLayer != null)
			{
				/**/
				for (var i = 0; i < lTracked.Count; ++i)
				{
					var pTouch = lTracked[i];
				
					var points = pTouch.lastcluster.points;
					for (var iPt = 0; iPt < points.Count; ++iPt)
					{
						var el = new Div();
						el.style.position = "absolute";
						el.style.width = this.DebugVoxelWidth + "px";
						el.style.height = this.DebugVoxelHeight + "px";
						el.style.backgroundColor = (pTouch.stronglock) ? pTouch.colour : "#CCC";
						el.style.opacity = "0.3";
					
						el.style.left = points[iPt].x + "%";
						el.style.top = points[iPt].y + "%";
					
						this._DebugLayer.appendChild(el);
					}
				}
			
			}
		}
	
		/**
		 * @brief Enable or disable debug mode.
		 * @param bEnabled Do we want to enable or disable debug mode.
		 */
		public void debug(bool bEnabled) {
			// Set the variable.
			this._Debug = bEnabled;
		
			// Create a debug layer if needed.
			if (this._Debug)
			{
				this._DebugLayerStatic = new Div();
				//document.body.appendChild(this._DebugLayerStatic);
			
				this._DebugLayerStatic.style.width= "100%";
				this._DebugLayerStatic.style.height= "100%";
				this._DebugLayerStatic.style.top= "0";
				this._DebugLayerStatic.style.left= "0";
				this._DebugLayerStatic.style.zIndex = "999";
				this._DebugLayerStatic.style.pointerEvents = "none";
				this._DebugLayerStatic.style.pointerEvents = "none";
				this._DebugLayerStatic.style.position= "absolute";
			}
			// Otherwise destroy it.
			else
			{
				//if (this._DebugLayerStatic != null)
				//	document.body.removeChild(this._DebugLayerStatic);
				this._DebugLayerStatic = null;
			}
		}
	
		/**
		 * @brief Enable or disable debug mode.
		 * @param bEnabled Do we want to enable or disable debug mode.
		 */
		public void trails(bool bEnabled) {
			// Create a debug layer if needed.
			if (bEnabled)
			{
				this._DebugLayer = new Div();
				//document.body.appendChild(this._DebugLayer);
			
				this._DebugLayer.style.width= "100%";
				this._DebugLayer.style.height= "100%";
				this._DebugLayer.style.top= "0";
				this._DebugLayer.style.left= "0";
				this._DebugLayer.style.zIndex = "999";
				this._DebugLayer.style.pointerEvents= "none";
				this._DebugLayer.style.position= "absolute";
			}
			// Otherwise destroy it.
			else
			{
				//if (this._DebugLayer != null)
				//	document.body.removeChild(this._DebugLayer);
				this._DebugLayer = null;
			}
		}
	
		/**
		 * @brief Update this touch tracker to respect the new Surface.XX settings and page resolution.
		 */
		public void updateSurface() {
			if (NativeDisplay.Surface == null) return;
			this.DebugVoxelWidth = this.KinectVoxel * (window.innerWidth / NativeDisplay.Surface.Width);
			this.DebugVoxelHeight = this.KinectVoxel * (window.innerHeight / NativeDisplay.Surface.Height);
		}
	
		/**
		 * @brief Return the colour at the start of the debug colour list, and cycle the list.
		 */
		public string _nextColour() {
			var colour = this.DebugColours.Splice(0, 1)[0];
			this.DebugColours.Add(colour);
			return colour;
		}
		
		/**
		 * @brief The dbScan algorithm (density-based spatial clustering of applications with noise)
		 * clusters points based on density.
		 * @see http://en.wikipedia.org/wiki/DBSCAN
		 *
		 * Usage: 
		 *    var clusters = dbScan(lData, DebugVoxelWidth * 0.25, 5, true)
		 *    for (var i = 0; i < clusters.length; ++i) {
		 *          console.log(clusters[i].centerx) // centerx, volume, slope, intercept, aspectratio, volume, radius
		 *    }
		 * 
		 * @author John Hardy
		 * @param data A list of data points [{x:3, y:4}, {x:1, y:1}, {x:5, y:0}]
		 * @param eps The scanning distance threshold for each cluster.
		 * @param minpts The minimum number of points required to be part of a cluster (the yellow ones on the wikipedia page top diagram)
		 * @param editdata Controls if we copy the data or not.  If true, the data will be copied.  False will edit in place (and add meta-data) e.g. data[i].noise, data[i].cluster, data[i].visited etc
		 * @return A list of clusters. Clusters are defined by the Cluster type within.
		 */
		public List<Cluster> dbScan(List<Point> data, double eps, int minpts, bool editdata = false) {
			this.eps = eps;
			this.minpts = minpts;
			// Create variables that store the number of 
			var lClusters = new List<Cluster>();
			var bWriteToData = editdata;
		
			// If we don't want to modify the current collection.
			var data2 = data;
			if (!bWriteToData) {
				data2.Clear();
				for (var i = 0; i < data.Count; i++)
				{
					Point p = new Point();
					p.x = data[i].x;
					p.y = data[i].y;
					p.cluster = null;
					data2.Add(p);
				}
			}
	
			// Build a kd-tree of points.
			var dimensions = new List<string>{"x", "y"};
			tree = new kdTree(data2, treeDistance, dimensions);
	
			// For each unvisited point P in dataset D.
			for (var i = 0; i < data2.Count; ++i) {
				// Bail if the point is visited.
				if (data2[i].visited)
					continue;
		
				// Mark it as visited.
				data2[i].visited = true;
		
				// Return all points within P's neigbourhood.
				var n = regionQuery(data2[i]);
		
				// If we have less than minpts neighbours, then flag as noise.
				if (n.Count < minpts)
				{
					data2[i].noise = true;
				}
		
				// Otherwise, generate us a new cluster.
				else
				{
					var cluster = new Cluster();
					lClusters.Add(cluster);
					expandCluster(data2[i], n, cluster);
				}
			}
	
			// Return all the clusters we generated.
			return lClusters;
		}

		private kdTree tree;
		private double eps;
		private int minpts;

		// Define a euclidan distance function for comparing epsilons.
		private double treeDistance(Point a, Point b){
			return Math.Pow(a.x - b.x, 2) +  Math.Pow(a.y - b.y, 2);
		}

		// Work out the n nearest points to our points (n = 100, but you can pick whatever).
		List<Point> regionQuery(Point dPoint) {
			// Query the kd-tree for the nearest n points. n = 100 by default
			var lNearest = tree.nearest(dPoint, 100, eps * eps); // TODO: treedistance uses squared distance.
		
			// Discard the distance information returned by the tree.
			var lPoints = new List<Point>();
			for (var i = 0; i < lNearest.Count; ++i)
				lPoints.Add(lNearest[i]._0);
		
			// Return the points.
			return lPoints;
		}

		// Expand a cluster by checking all the neigbours of neigbours.  Minesweeper stack based for speed.
		void expandCluster(Point dPoint, List<Point> lNeighbours, Cluster pCluster) {
			// Add the point to the cluster.
			pCluster.points.Add(dPoint);
			dPoint.cluster = pCluster;
		
			// For each neighbour.
			while (lNeighbours.Count > 0)
			{
				var dNPoint = lNeighbours[lNeighbours.Count - 1];
				lNeighbours.RemoveAt(lNeighbours.Count - 1);
				// If we have not visited this neigbour yet.
				if (!dNPoint.visited)
				{
					// Mark it as visited.
					dNPoint.visited = true;
				
					// Add its neigbours into the cluster, if they have enough neigbours.
					var n = regionQuery(dNPoint);
					if (n.Count >= minpts)
					{
						// Put them onto the stack, if they are not already visited.
						lNeighbours = new List<Point>(lNeighbours.Concat(n.FindAll((Point pt) => { return !pt.visited; })));
					}
				}
			
				// If we are not a member of a cluster yet, make us a member.
				if (dNPoint.cluster == null)
				{
					pCluster.addPoint(dNPoint);
					dNPoint.cluster = pCluster;
				}
			}
		}
	}
}
