using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.Native
{
	/**
	 * @brief A class which adds multi-touch to a page.
	 * @author John Hardy
	 * @date 18th August 2012
	 * Usage:
	 *   var mt = new KinectTouch({
	 *       point_limit : 200,         // The number of points we are allowed to process.
	 *       surface_zoffset : 0.01,    // The offset from the surface (in meters) at which to start capturing data.
	 *       height : 0.01,             // The distance from the surface offset (in meters) at which to stop capturing data.
	 *       relativeto : Surface.Name, // The surface you want to take multi-touch input from.
	 *   });
	 *   mt.updateSurface() // <-- all this if you have new surface dimensions or render resolution
	 */
	class KinectTouch
	{
		public static KinectTouch _KinectTouchInstance;

		/** @brief Handle the data passed in from the Kinect. */
		public static void KinectMultiTouch_HandlePointCloud(double[][] lPoints) {
			// Transform data into the format for the dbscan.
			var lData = new List<Point>();
			for (var i = 0; i < lPoints.Count(); ++i)
			{
				Point p = new Point();
				p.x = Math.Abs(lPoints[i][0]) * 100;
				p.y = Math.Abs(lPoints[i][1]) * 100;
				p.z = Math.Abs(lPoints[i][2]) * 100;
				lData.Add(p);
			}
	
			// Process it.
			_KinectTouchInstance.process(lData);
		}

		private long kLastLoop;
		private double fFps;
		private long fFrameTime;
		private TouchTracker processor;

		/**
		 * @brief Create a new Kinect touch tracker.
		 */
		public KinectTouch(Dictionary<string, object> dArgs, TouchTracker.Dispatcher start, TouchTracker.Dispatcher stop, TouchTracker.Dispatcher update)
		{
			// If we are already created, die.
			if (_KinectTouchInstance != null)
				throw new Exception("Error");
			_KinectTouchInstance = this;
			/*
			// Process arguments.
			var dArguments = new Dictionary<string, object>();
			dArguments.Add("point_limit", 200);
			dArguments.Add("height", 0.010);
			dArguments.Add("relativeto", Surface.Name);
			dArguments.Add("surface_zoffset", 0.015);
			dArguments.Add("callback", "KinectMultiTouch_HandlePointCloud");
		
			// Override anything with our arguments.
			foreach (var key in dArgs.Keys)
				dArguments[key] = dArgs[key];
			*/
			// Variable.
			this.kLastLoop = DateTime.Now.Ticks;
			this.fFps = 0;
			this.fFrameTime = 0;
		
			// Generate a new touch tracker to process the data.
			this.processor = new TouchTracker(dArgs.ContainsKey("debug") ? (bool)dArgs["debug"] : false, dArgs.ContainsKey("trails") ? (bool)dArgs["trails"] : false, start, stop, update);
				//start   : function(touch){ MultiTouch.inject(MultiTouch.START, touch.id, touch.x(), touch.y(), touch); },
				//stop    : function(touch){ MultiTouch.inject(MultiTouch.END,   touch.id, touch.x(), touch.y(), touch); },
				//update  : function(touch){ MultiTouch.inject(MultiTouch.MOVE,  touch.id, touch.x(), touch.y(), touch); },
		
			// Subscript to point cloud data.
			//Authority.request("KinectLowestPointCube", dArguments);
		}
	
		/**
		 * @brief Enable or disable debug mode.
		 */
		public void enableDebug(bool bEnabled) { this.processor.debug(bEnabled); }
	
		/**
		 * @brief Update the touch tracker if we have new surface dimensions.
		 */
		public void updateSurface()
		{
			this.processor.updateSurface();
		}
	
		public void process(List<Point> points) {
			// Work out the time since the last process.
			var kLoop = DateTime.Now.Ticks;
			if (kLoop - this.kLastLoop > 0) this.fFps = 1000 * 10000 / (kLoop - this.kLastLoop);
			this.kLastLoop = kLoop;
		
			// Process the points.
			var start = DateTime.Now.Ticks;
			this.processor.process(points);
			var stop = DateTime.Now.Ticks;
			this.fFrameTime = stop - start;
		}
	
		/**
		 * @brief Get the number of touch frames processed per second.
		 */
		double fps() {
			return this.fFps;
		}
	
		/**
		 * @brief Get how many ms it took to process the last frame of touch points.
		 */
		double processTime() {
			return this.fFrameTime;
		}
	}
}
