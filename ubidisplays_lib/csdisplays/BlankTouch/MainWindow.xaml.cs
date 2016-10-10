using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using UbiDisplays;
using UbiDisplays.Vectors;

namespace BlankTouch
{
	public abstract class HandApplication
	{
		protected Hand hand;

		public HandApplication(Hand hand)
		{
			this.hand = hand;
		}

		public abstract void Draw(Painter painter);

		public abstract void Update();
	}

	class BlankTouchApplication : HandApplication
	{
		public BlankTouchApplication(Hand hand) : base(hand)
		{

		}

		public override void Draw(Painter painter)
		{
			painter.Color = Color.fromBytes(255, 255, 0, 80);
			float radius = 0.02f;
			for (int i = 0; i < hand.FingerCount(); ++i)
			{
				painter.FillCircle(new Vector2((100.0f + hand.GetFinger(i).X) / 100.0f, (100.0f + hand.GetFinger(i).Y) / 100.0f), radius);
			}
		}

		public override void Update()
		{
			hand.Update();
		}
	}

	class WpfCanvas : System.Windows.Controls.Canvas
	{
		private Painter painter;

		public WpfCanvas(float x, float y, float width, float height)
		{
			painter = new Painter(x, y, width, height);
		}

		protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			painter._Context = drawingContext;
			MainWindow.application.Draw(painter);
		}
	}

	public partial class MainWindow : Window
	{
		public static HandApplication application; 
		private Hand hand;
		private WpfCanvas canvas;

		public static new float Width;
		public static new float Height;

		public MainWindow()
		{
			InitializeComponent();

			UbiHand.Offset = 0.03f;
			UbiHand.Height = 0.01f;
			UbiHand.Invert = true;

			hand = new UbiHand();
			//hand = new MouseHand(width, height);

			if (hand is UbiHand)
			{
				float xmin = 1000;
				float xmax = -1000;
				float ymin = 1000;
				float ymax = -1000;
				var document = XDocument.Load("Untitled.ubi");
				foreach (var point in document.Root.Element("surface").Element("projector").Elements("point"))
				{
					var p = PointFromString(point.Value);
					if (p.X < xmin) xmin = (float)p.X;
					if (p.X > xmax) xmax = (float)p.X;
					if (p.Y < ymin) ymin = (float)p.Y;
					if (p.Y > ymax) ymax = (float)p.Y;
				}
				System.Drawing.Rectangle workingArea = System.Windows.Forms.Screen.AllScreens[1].WorkingArea;
				Width = (xmax - xmin) * workingArea.Width;
				Height = (ymax - ymin) * workingArea.Height;
				canvas = new WpfCanvas(xmin * workingArea.Width, ymin * workingArea.Height, Width, Height);
				UbiDisplays.Model.Native.window.innerWidth = (int)Width;
				UbiDisplays.Model.Native.window.innerHeight = (int)Height;
			}
			else
			{
				//Width = 1500;
				//Height = 900;

				Width = 1024;
				Height = 768;

				//UbiDisplays.Model.Native.window.innerWidth = (int)Width;
				//UbiDisplays.Model.Native.window.innerHeight = (int)Height;

				canvas = new WpfCanvas(0, 0, Width, Height);
				canvas.Width = Width;
				canvas.Height = Height;
			}
			canvas.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));

			AddChild(canvas);
			RenderOptions.SetBitmapScalingMode(canvas, BitmapScalingMode.NearestNeighbor);

			this.SizeToContent = SizeToContent.WidthAndHeight;

			if (System.Windows.Forms.SystemInformation.MonitorCount > 1)
			{
				WindowStartupLocation = WindowStartupLocation.Manual;
				System.Drawing.Rectangle workingArea = System.Windows.Forms.Screen.AllScreens[1].WorkingArea;
				Left = workingArea.Left;
				Top = workingArea.Top;
				base.Width = workingArea.Width;
				base.Height = workingArea.Height;
				canvas.Width = workingArea.Width;
				canvas.Height = workingArea.Height;
				//WindowState = WindowState.Maximized;
				WindowStyle = WindowStyle.None;
				AllowsTransparency = true;
				Topmost = true;
			}

			// Go fullscreen
			//WindowStyle = System.Windows.WindowStyle.None;
			//AllowsTransparency = true;
			//WindowState = System.Windows.WindowState.Maximized;

			application = new BlankTouchApplication(hand);

			Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
			CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
			Closing += MainWindow_Closing;
		}

		void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (hand is UbiHand)
			{
				var ubiHand = (UbiHand)hand;
				double min = 10000000;
				double max = -10000000;
				double mean = 0;
				for (int i = 0; i < ubiHand.times.Count; ++i)
				{
					double value = ubiHand.times[i];
					mean += value;
					if (value < min) min = value;
					if (value > max) max = value;
				}
				mean /= ubiHand.times.Count;
				
				string[] lines = new string[ubiHand.times.Count];
				for (int i = 0; i < ubiHand.times.Count; ++i)
				{
					lines[i] = ubiHand.times[i].ToString();
				}
				System.IO.File.WriteAllLines(@"C:\Users\Robert\EAPValues.txt", lines);
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (System.Windows.Forms.SystemInformation.MonitorCount > 1) WindowState = WindowState.Maximized;
		}

		void CompositionTarget_Rendering(object sender, System.EventArgs e)
		{
			application.Update();
			canvas.InvalidateVisual();
			InvalidateVisual();
		}

		private static System.Windows.Point PointFromString(String sString)
		{
			// Check we have 3 spaces.
			var tCoords = sString.Split(' ');
			if (tCoords.Length == 2)
				return new System.Windows.Point(float.Parse(tCoords[0]), float.Parse(tCoords[1]));
			throw new Exception("Cannot parse Point from string '" + sString + "'.");
		}

		protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);
			if (hand is MouseHand) (hand as MouseHand).MouseDown((int)e.GetPosition(canvas).X, (int)e.GetPosition(canvas).Y);
		}

		protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseUp(e);
			if (hand is MouseHand) (hand as MouseHand).MouseUp((int)e.GetPosition(canvas).X, (int)e.GetPosition(canvas).Y);
		}

		protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (hand is MouseHand) (hand as MouseHand).MouseMove((int)e.GetPosition(canvas).X, (int)e.GetPosition(canvas).Y);
		}
	}
}
