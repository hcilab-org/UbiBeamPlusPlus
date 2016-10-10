using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.Native
{
	/**
	 * @brief A TouchPoint manages a tracked touch point.  This accounts for
	 * smoothing, position, strong locks (i.e. how sure are we that it exists)
	 * and debugging data.
	 * @author John Hardy
	 */
	public class TouchPoint
	{
		public bool assigned = false;
		private TouchTracker processor;
		public string colour;
		private double avgx;
		private KalmanDouble smoothx;
		private double avgy;
		private KalmanDouble smoothy;
		public Cluster lastcluster;
		public bool stronglock;
		public int id;
		private int iFrameCount;
		private DateTime lastupdate;
		private int iDeathTracker;
		private Div _visual;
		private Div _label;

		public TouchPoint(TouchTracker processor, Cluster cluster)
		{
			this.processor = processor;                   // The TouchTracker that created us.
			this.colour = this.processor._nextColour();   // The debug colour for this point.
	
			this.avgx = cluster.centerx();                // The raw cluster x.
			this.smoothx = new KalmanDouble(cluster.centerx());  // The smoothed cluster x.
			this.smoothx.noise = 0.05;
	
			this.avgy = cluster.centery();                // The raw cluster y.
			this.smoothy = new KalmanDouble(cluster.centery());  // The smoothed cluster y.
			this.smoothy.noise = 0.05;
	
			this.lastcluster = cluster;                   // The reference to the last cluster of points.
			this.stronglock = false;                      // Is this touchpoint definately not just noise.
	
			this.id = this.processor.TrackerCount++;      // The id for this touch point.
			this.iFrameCount = 0;                         // The number of touch frames it has processed.
	
			this.lastupdate = DateTime.Now;               // The last time it recieved an update.
	
			this.iDeathTracker = 0;

			// Add a visual and label to the debug trackers layer.
			this._visual = null;
			this._label = null;
		}
	
		/** @brief The raw center of this touch point on the x axis. */
		public double centerx() {
			return this.avgx;//.get();
		}
	
		/** @brief The raw center of this touch point on the y axis. */
		public double centery() {
			return this.avgy;//.get();
		}
	
		/** @brief The raw center of this touch point on the x axis. */
		public double x() {
			return this.smoothx.get() / 100; // put us back into 0-1 space!
		}
	
		/** @brief The raw center of this touch point on the y axis. */
		public double y() {
			return this.smoothy.get() / 100;// put us back into 0-1 space!
		}
	
		/**
		 * @brief Tell this touch point to consume a cluster.
		 */
		public void consume(Cluster cluster) {
		
			// Update the number of frames this point has processed.
			this.iFrameCount++;
			//this.lastupdate = new Date().getTime();
		
			// Update the tracker by this cluster.
			this.avgx = cluster.centerx();
			this.smoothx.push(cluster.centerx());
		
			this.avgy = cluster.centery();
			this.smoothy.push(cluster.centery());
		
			// Store the last cluster.
			this.lastcluster = cluster;
		
			this.iDeathTracker = 0;
		
			// If we are a strong lock.
			if (this.iFrameCount == this.processor.FramePersistance)
			{
				// If we have had x updates, set us to a strong tracker.
				this.stronglock = true;
			
				// Dispatch the start signal.
				if (this.processor.DispatchStart != null) {
					this.processor.DispatchStart(this);
				}
			
				// Add some visuals to the static debug layer.
				if (this.processor._DebugLayerStatic != null)
				{
					// Add us a new visual which we can move around.
					this._visual = new Div();
					this.processor._DebugLayerStatic.appendChild(this._visual);
				
					//this._visual.style.zIndex = "999";
					this._visual.style.pointerEvents= "none";
					this._visual.style.position= "absolute";
				
					this._visual.style.left = this.centerx() + "%";
					this._visual.style.top  = this.centery() + "%";
					this._visual.style.width = this.processor.DebugVoxelWidth * 2 + "px";
					this._visual.style.height = this.processor.DebugVoxelHeight * 2 + "px";
					this._visual.style.marginTop = -this.processor.DebugVoxelHeight + "px";
					this._visual.style.marginLeft = -this.processor.DebugVoxelWidth + "px";
				
					this._visual.style.borderRadius = this.processor.DebugVoxelWidth * 2 + "px";
					this._visual.style.backgroundColor = this.colour;
					this._visual.style.opacity = "0.8";
					this._visual.style.border="1px solid white";
				
					// And the label.
					this._label = null; /*document.createElement("div");
					this._label.innerHTML = this.id;
					this.processor._DebugLayerStatic.appendChild(this._label);
				
					this._label.style.pointerEvents= "none";
					this._label.style.position= "absolute";
					this._label.style.left = this.centerx() + "%";
					this._label.style.top  = this.centery() + "%";
					this._label.style.marginTop = "-40px";
					this._label.style.color = "yellow";
				
					/*
					this._label = $("<div>").css({
							position: "absolute",
							left : this.centerx() + "%",
							top  : this.centery() + "%",
							"margin-top" : "-40px",
							color: "yellow",
					}).appendTo(this.processor._DebugLayerStatic).text(this.id);
					*/
				}
			}
		
			// Don't dispatch move updates unless we have a strong lock.
			if (this.stronglock) {
				if (this.processor.DispatchMove != null) {
					this.processor.DispatchMove(this);
				}
			}
		
			// If debugging is enabled.
			if (this.processor._DebugLayerStatic != null) {
				// Update the visuals.
				if (this._visual != null) {
					this._visual.style.left = this.smoothx.get() + "%";
					this._visual.style.top = this.smoothy.get() + "%";
					/*this._visual.css({
						left : this.smoothx.get() + "%",
						top  : this.smoothy.get() + "%",
					});*/
				}
				if (this._label != null) {
					this._label.style.left = this.smoothx.get() + "%";
					this._label.style.top = this.smoothy.get() + "%";
					/*
					this._label.css({
						left : this.smoothx.get() + "%",
						top  : this.smoothy.get() + "%",
					});*/
				}
			}
		}
	
		/**
		 * @brief Check to see if this touch point should be deleted.
		 * @return True to flag for deletion, false to keep alive.
		 */
		public bool consumeNothing() {
			// If we have four consecutive frames with NO input, consider us dead.
			if (this.iDeathTracker >= 4)
				return true;
			this.iDeathTracker++;
			return false;
			// Work out the time since the last input.
			//var elapsed = new Date().getTime() - this.lastupdate;
			//if (elapsed > this.processor.TouchRemoveTime)
			//	return true;
			//return false;
		
			// Suggest the tracker should die.
			//return false; // keep it alive.
			//return true; // kill it off
		}
	
		/**
		 * @brief Remove this touch point, fire events and remove visual debug.
		 */
		public void remove() {
			// Don't dispatch if we are not a strong lock.
			if (this.stronglock) {
				if (this.processor.DispatchStop != null) {
					this.processor.DispatchStop(this);
				}
			}
		
			// Remove debug visuals.
			if (this._visual != null)
				this.processor._DebugLayerStatic.removeChild(this._visual);
				//this._visual.remove();
		
			if (this._label != null)
				this.processor._DebugLayerStatic.removeChild(this._label);
				//this._label.remove();
		}
	
		/*
		this.drawCluster = function() {
			// For each point in this cluster, draw it.
			var points = this.lastcluster.points;
			for (var iPt = 0; iPt < points.length; ++iPt)
			{
				$("<div>").addClass("debugSpot").css({
					left : points[iPt].x + "%",
					top  : points[iPt].y + "%",
					width: TouchPoint_DebugVoxelWidth + "px",
					height: TouchPoint_DebugVoxelHeight + "px",
					"background-color": (this.stronglock) ? this.colour : "#CCC",
					opacity : "0.3",
				}).appendTo(pLayer);
			}
		}
		*/
	}
}
