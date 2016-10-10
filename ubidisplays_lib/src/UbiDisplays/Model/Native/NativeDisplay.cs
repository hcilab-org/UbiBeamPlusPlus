using SlimMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UbiDisplays.Model.DisplayAPI;

namespace UbiDisplays.Model.Native
{
	public class NativeDisplay : Canvas
	{
		private KinectTouch tracker;
		private int x = 0;
		private int y = 0;
		private List<Point> pointList = new List<Point>();
		private LowestPointCube cube;
		public static Surface Surface;

		public NativeDisplay()
		{
			var dict = new Dictionary<string, object>();
			tracker = new KinectTouch(dict, start, stop, update);
		}

		public void start(Display display, Model.Surface surface, float surfaceZOffset, float height)
		{
			NativeDisplay.Surface = surface;
			var pResource = new LowestPointCube(display, surface, newpoints, surfaceZOffset, height);
			cube = pResource;
			display.AttachResource(pResource);
		}

		private void newpoints(Vector3[] points)
		{
			pointList.Clear();
			//System.Console.WriteLine("Points: {0}", points.Count());
			foreach (var point in points)
			{
				pointList.Add(new Point(point.X * 100, point.Y * 100, point.Z * 100));
			}
			tracker.process(pointList);
		}

		public void SetOffset(float offset)
		{
			cube.SetOffset(offset);
		}

		public void SetHeight(float height)
		{
			cube.SetHeight(height);
		}

		private void start(TouchPoint point)
		{
			UbiHand.the.start(point);
		}

		private void stop(TouchPoint point)
		{
			UbiHand.the.stop(point);
		}

		private void update(TouchPoint point)
		{
			UbiHand.the.update(point);
		}

		protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			drawingContext.DrawRectangle(new System.Windows.Media.SolidColorBrush(Color.FromRgb(255, 0, 0)), new System.Windows.Media.Pen(), new System.Windows.Rect(10, 10, 300, 300));
			drawingContext.DrawRectangle(new System.Windows.Media.SolidColorBrush(Color.FromRgb(0, 255, 0)), new System.Windows.Media.Pen(), new System.Windows.Rect(x - 10, y - 10, x + 10, y + 10));
		}
	}
}
