using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using UbiDisplays.Model;
using UbiDisplays.Model.Native;
using UbiDisplays.Vectors;

namespace UbiDisplays
{
	public class Finger
	{
		public static int SmoothingSteps = 4;
		public static float MovingTreshhold = 0.01f;
		public static float StopingTreshhold = 0.002f;

		public float X;
		public float Y;
		public TouchPoint Point;

		private Vector2[] points;
		private Vector2 last;
		private bool moving = true;

		public Finger(float x, float y, TouchPoint point)
		{
			points = new Vector2[SmoothingSteps];
			this.X = x;
			this.Y = y;
			this.Point = point;
			for (int i = 0; i < SmoothingSteps; ++i)
			{
				points[i] = new Vector2(x, y);
			}
			last = new Vector2(x, y);
		}

		public void Set(Vector2 point)
		{
			Vector2 vec = Stabilize(point);
			X = vec.X;
			Y = vec.Y;
		}

		private Vector2 Stabilize(Vector2 point)
		{
			if (SmoothingSteps > points.Length)
			{
				var newpoints = new Vector2[SmoothingSteps];
				for (int i = 0; i < points.Length; ++i) newpoints[i] = points[i];
				for (int i = points.Length; i < SmoothingSteps; ++i) newpoints[i] = point;
				points = newpoints;
			}
			for (int i = 0; i < SmoothingSteps - 1; ++i)
			{
				points[i] = points[i + 1];
			}
			points[SmoothingSteps - 1] = point;
			Vector2 mean = new Vector2();
			for (int i = 0; i < SmoothingSteps; ++i)
			{
				mean += points[i];
			}
			mean /= SmoothingSteps;

			if (moving)
			{
				if ((mean - last).Length < StopingTreshhold)
				{
					moving = false;
					return last;
				}
			}
			else
			{
				if ((mean - last).Length > MovingTreshhold)
				{
					moving = true;
				}
				else return last;
			}

			last = mean;
			return mean;
		}
	}

	public class UbiHand : Hand
	{
		private static UbiHand instance;
		private KinectProcessing processor;
		private static Display display;
		private static float offset;
		private static float height;
        private MouseHand mouse;

		public static float Offset
		{
			get
			{
				return offset;
			}
			set
			{
				offset = value;
                if (display != null)
                {
                    display.SetOffset(offset);
                }
			}
		}

		public static float Height
		{
			get
			{
				return height;
			}
			set
			{
				height = value;
                if (display != null)
                {
                    display.SetHeight(height);
                }
			}
		}

		public static bool Invert;

		public static UbiHand the
		{
			get { return instance; }
		}

		public UbiHand()
		{
            try
            {
            processor = new KinectProcessing();
            Surface.KinectProcessor = processor;
            processor.OnFrameReady += KinectProcessor_OnFrameReady;
            processor.OnKinectStreamingStarted += KinectProcessor_OnKinectStreamingStarted;
            processor.Start(Microsoft.Kinect.KinectSensor.KinectSensors.First());
            Load(true, true, true, "Untitled.ubi");
            instance = this;
            }
            catch (TypeInitializationException e)
            {
                Console.WriteLine("An error with the Kinect occurred. More informations can be found in the stacktrace.");
                Console.WriteLine(e.Message);
            }
		}

        public void AddMouse(float x, float y, float width, float height)
        {
            mouse = new MouseHand(x, y, width, height);
        }

        public KinectProcessing Kinect
        {
            get
            {
                return processor;
            }
        }

		public override Vector3 Position {
			get {
                Vector3 pos;

                if (mouse != null && mouse.FingerCount() > 0) {
                    pos = mouse.Position;
                }

                lock (fingers) {
                    if (fingers.Count == 0) {
                        pos = new Vector3(-1, -1, 0);
                    }

                    pos = new Vector3(fingers[0].X, fingers[0].Y, 0);
                }

                return pos;
			}
		}

        public override FingerPoint GetFinger(int index)
		{
            if (mouse != null && mouse.FingerCount() > 0) {
                if (index == 0)
                    return mouse.GetFinger(0);
                else
                    --index;
            }

            float fingerX=0;
            float fingerY=0;
            int fingerId=0;

            lock (fingers) {
                if (fingers.Count > index) {
                    fingerX = fingers[index].X;
                    fingerY = fingers[index].Y;
                    fingerId = fingers[index].Point.id;
                }
            }

            return new FingerPoint(fingerX, fingerY, fingerId);
		}

		public override int FingerCount() {
            int fingerCount = 0;
            
            if (mouse != null) {
                fingerCount += mouse.FingerCount();
            }

            lock (fingers) {
                fingerCount += fingers.Count;
            }

			return fingerCount;
		}

		public override bool IsFist()
		{
			return FingerCount() > 0;
		}

		List<Finger> fingers = new List<Finger>();

		private void RemoveFinger(TouchPoint point) {
            lock (fingers) {
                for (int i = 0; i < fingers.Count; ++i) {
                    if (fingers[i].Point == point) {
                        fingers.Remove(fingers[i]);
                        return;
                    }
                }
            }

		}

		private Finger FindFinger(TouchPoint point) {
            lock (fingers) {
                for (int i = 0; i < fingers.Count; ++i) {
                    if (fingers[i].Point == point) {
                        return fingers[i];
                    }
                }
            }

			return null;
		}

		public void start(TouchPoint point)
		{
			//System.Console.WriteLine("X: {0} Y: {1}", point.x(), point.y());
			float x = transformX(point);
			float y = transformY(point);
            lock (fingers) {
                fingers.Add(new Finger(x, y, point));
            }
			//System.Console.WriteLine("x: {0} y: {1}", x, y);
			//System.Console.WriteLine("Finger added.");
		}

		public void stop(TouchPoint point)
		{
			//System.Console.WriteLine("X: {0} Y: {1}", point.x(), point.y());
			float x = transformX(point);
			float y = transformY(point);
			RemoveFinger(point);
			//System.Console.WriteLine("x: {0} y: {1}", x, y);
			//System.Console.WriteLine("Finger removed.");
		}

		public void update(TouchPoint point)
		{
			//System.Console.WriteLine("X: {0} Y: {1}", point.x(), point.y());
			
			float x = transformX(point);
			float y = transformY(point);
			Finger finger = FindFinger(point);
			if (finger != null)
			{
				finger.Set(new Vector2(x, y));
			}

			//x = transformX(point);
			//y = transformY(point);
			
			//System.Console.WriteLine("x: {0} y: {1}", x, y);
		}

		private float transformX(TouchPoint point)
		{
			if (Invert) return (float)(1.0 - point.x() * 100.0);
			else return (float)(point.x() * 100.0);
		}

		private float transformY(TouchPoint point)
		{
			if (Invert) return (float)(1.0 - point.y() * -100.0);
			else return (float)(point.y() * -100.0);
		}

		private void KinectProcessor_OnKinectStreamingStarted(KinectProcessing obj)
		{
			
		}

		public List<double> times = new List<double>();
		System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

		private void KinectProcessor_OnFrameReady(KinectProcessing obj)
		{
			watch.Reset();
			watch.Start();
			///TimeSpan begin = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime;
			
			// If we have a valid image.
			//if (pKinectVideoTarget == null)
			//	return;

			// For each spatial query, begin a new frame.
			var tQueries = UbiDisplays.Model.Surface.SpatialQueries.ToArray();
			foreach (var pQuery in tQueries)
				pQuery.BeginFrame();

			// Wipe all the debug pixels back to transparent.
			//Array.Clear(tKinectVideoDebugBackBuffer, 0, tKinectVideoDebugBackBuffer.Length);

			// Perform the actual processing.
			processor.QueryPointCloud(tQueries);//, tKinectVideoDebugBackBuffer);

			// End the frame.
			foreach (var pQuery in tQueries)
				pQuery.EndFrame();

			///TimeSpan end = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime;
			///Console.WriteLine("Measured time: " + (end - begin).TotalMilliseconds + " ms.");

			watch.Stop();
			times.Add(watch.Elapsed.TotalMilliseconds);

			//if (MainWindow.recording) motionVideo.VideoWriter.SendData(obj.tColourPixels);
		}

		private void Calibrate(System.Windows.Point[] tProjector, System.Windows.Point[] tKinect)
		{
			// Map the kinect (pixel space).
			//pHomography.Source = tKinect;
			//pHomography.Destination = tProjector;

			// Compute the matrix.
			//pHomography.ComputeWarp();

			// Compute the calibration plane (which goes into the KinectProcessor for relative rendering).
			processor.CalibrationPlane = processor.ImageCoordinatesToBestFitPlane(tKinect);

			// Say we are calibrated.
			//this.bCalibrated = true;

			// Try to move us to the next step
			//TryCompleteStep2();
		}

		private void Load(bool bCalibration, bool bSurfaces, bool bDisplays, String sFile)
		{
			// Open the XML file and parse out the data we are interested in.
			var pDocument = XDocument.Load(sFile);

			if (bCalibration)
			{
				// Parse the surfaces from the file.
				var dCalibration = (from item in pDocument.Root.Elements("calibration")
									select new
									{
										MonitorDevice = item.Element("monitor").Value,
										KinectDevice = item.Element("kinect").Value,

										KinectImage = (from pt in item.Element("kinectimage").Elements("point") select PointFromString(pt.Value)).ToArray(),
										ProjectedImage = (from pt in item.Element("projectedimage").Elements("point") select PointFromString(pt.Value)).ToArray(),
									}).FirstOrDefault();

				Calibrate(dCalibration.ProjectedImage, dCalibration.KinectImage);
			}

			if (bSurfaces)
			{
				// Parse the surfaces from the file.
				var lSurfaces = from item in pDocument.Root.Elements("surface")
								select new
								{
									Identifier = item.Element("name").Value,
									InjectMT = (item.Element("inject_multitouch").Value.ToLower() == "true") ? true : false,

									Projector = (from pt in item.Element("projector").Elements("point") select PointFromString(pt.Value)).ToArray(),
									Sensor = (from pt in item.Element("sensorspace").Elements("point") select Vector3FromString(pt.Value)).ToArray(),
									KinectImage = (from pt in item.Element("image").Elements("point") select PointFromString(pt.Value)).ToArray(),
								};

				// For each surface, register one with the authority.
				foreach (var dSurfaceData in lSurfaces)
				{
					// Check the surface name is good.
					if (dSurfaceData.Identifier == null || dSurfaceData.Identifier == "")
					{
						Log.Write("Cannot import surface.  Surface is missing a name.", "Application", Log.Type.AppWarning);
						continue;
					}

					// If the name is already taken, bail.
					if (Authority.FindSurface(dSurfaceData.Identifier) != null)
					{
						Log.Write("Cannot import surface '" + dSurfaceData.Identifier + "'.  Surface with the same name already exists.", "Application", Log.Type.AppWarning);
						continue;
					}

					// Check we have valid data.
					if ((dSurfaceData.Projector == null || dSurfaceData.Projector.Length != 4) ||
						(dSurfaceData.Sensor == null || dSurfaceData.Sensor.Length != 4) ||
						(dSurfaceData.KinectImage == null || dSurfaceData.KinectImage.Length != 4))
					{
						Log.Write("Cannot import surface '" + dSurfaceData.Identifier + "'.  It does not contain valid data.", "Application", Log.Type.AppWarning);
						continue;
					}

					// Create the surface.
					var pSurface = new UbiDisplays.Model.Surface(dSurfaceData.Identifier);
					pSurface.AttemptMultiTouchInject = dSurfaceData.InjectMT;
					pSurface.SetSpatialProperties(dSurfaceData.Projector, dSurfaceData.Sensor, dSurfaceData.KinectImage);
					Authority.RegisterSurface(pSurface);
				}
			}

			if (bDisplays)
			{
				// For each display in the surface file, attach it to the surface.
				var lDisplays = from item in pDocument.Root.Elements("display")
								select new
								{
									SurfaceName = item.Element("surfacename").Value,
									LoadInstruction = item.Element("loadinstruction").Value,
									Resolution = PointFromString(item.Element("resolution").Value),
								};

				// Create the displays.
				foreach (var dDisplayData in lDisplays)
				{
					// Find the surface to place it on.
					var pSurface = Authority.FindSurface(dDisplayData.SurfaceName);
					if (pSurface == null)
					{
						Log.Write("Cannot import display '" + dDisplayData.LoadInstruction + "'.  Could not find host surface '" + dDisplayData.SurfaceName + "'.", "Application", Log.Type.AppWarning);
						continue;
					}

					// Create the display.
					var pDisplay = new Display(dDisplayData.LoadInstruction, dDisplayData.Resolution);
					display = pDisplay;

					// Disable debug mode on the surface.
					pSurface.ShowDebug = false;

					// Show the display.
					Authority.ShowDisplay(pDisplay, pSurface);
				}
			}
		}
        //TODO: Fix this!
		private static System.Windows.Point PointFromString(String sString)
		{
			// Check we have 3 spaces.
			var tCoords = sString.Split(' ');
			if (tCoords.Length == 2)
				return new System.Windows.Point(float.Parse(tCoords[0]), float.Parse(tCoords[1]));
			throw new Exception("Cannot parse Point from string '" + sString + "'.");
		}
		
		private static SlimMath.Vector3 Vector3FromString(String sString)
		{
			// Check we have 3 spaces.
			var tCoords = sString.Split(' ');
			if (tCoords.Length == 3)
				return new SlimMath.Vector3(float.Parse(tCoords[0]), float.Parse(tCoords[1]), float.Parse(tCoords[2]));
			throw new Exception("Cannot parse Vector3 from string '" + sString + "'.");
		}

        public void MouseDown(int x, int y)
        {
            if (mouse != null) mouse.MouseDown(x, y);
        }

        public void MouseUp(int x, int y)
        {
            if (mouse != null) mouse.MouseUp(x, y);
        }

        public void MouseMove(int x, int y)
        {
            if (mouse != null) mouse.MouseMove(x, y);
        }
	}
}
