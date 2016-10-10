using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using UbiDisplays.Vectors;

namespace BlankTouch
{
	public class Painter
	{
		private DrawingContext context;
		private Color myColor;
		private System.Windows.Media.Color nativeColor;
		private Font myFont;
		private float x;
		private float y;
		private float width;
		private float height;

		public Painter(float x, float y, float width, float height)
		{
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
			myFont = new Font("Arial", new FontStyle(false, false, false), 0.02f);
		}


		public void DrawImage(Image image, float dx, float dy, float dw, float dh, float opacity = 1.0f)
		{
			context.PushOpacity(opacity);
			context.DrawImage(image._Image, new System.Windows.Rect(x + dx * width, y + dy * height, dw * width, dh * height));
			context.Pop();
		}

		public void DrawImage(Image image, float sx, float sy, float sw, float sh, float dx, float dy, float dw, float dh, float opacity = 1.0f)
		{
			context.PushOpacity(opacity);
			context.DrawImage(image._Image, new System.Windows.Rect(x + dx * width, y + dy * width, dw * width, dh * height));
			context.Pop();
		}

		public void DrawImage(ImageSource image, float dx, float dy, float dw, float dh, float opacity = 1.0f)
		{
			context.PushOpacity(opacity);
			context.DrawImage(image, new System.Windows.Rect(x + dx * width, y + dy * width, dw * width, dh * height));
			context.Pop();
		}

		public void DrawImage(ImageSource image, float sx, float sy, float sw, float sh, float dx, float dy, float dw, float dh, float opacity = 1.0f)
		{
			context.PushOpacity(opacity);
			context.DrawImage(image, new System.Windows.Rect(x + dx * width, y + dy * width, dw * width, dh * height));
			context.Pop();
		}

		public void DrawString(string text, float x, float y)
		{
			if (text != null)
			{
				text.Replace(' ', (char)160); // Non-breaking space 
				System.Windows.Media.FormattedText fText = new System.Windows.Media.FormattedText(text,
					System.Globalization.CultureInfo.GetCultureInfo("en-us"), System.Windows.FlowDirection.LeftToRight,
					new System.Windows.Media.Typeface(Font.Name), MainWindow.Height * Font.Size, new System.Windows.Media.SolidColorBrush(nativeColor));
				if (Font.Style.IsBold()) fText.SetFontWeight(System.Windows.FontWeights.Bold);
				if (Font.Style.IsItalic()) fText.SetFontStyle(System.Windows.FontStyles.Italic);
				if (Font.Style.IsUnderlined()) fText.SetTextDecorations(System.Windows.TextDecorations.Underline);
				context.DrawText(fText, new System.Windows.Point(this.x + x * width, this.y + y * height));
			}
		}

		public Font Font
		{
			get
			{
				return myFont;
			}
			set
			{
				myFont = value;
			}
		}

		public Color Color
		{
			get
			{
				return myColor;
			}
			set
			{
				myColor = value;
				nativeColor = System.Windows.Media.Color.FromArgb((byte)myColor.Ab, (byte)myColor.Rb, (byte)myColor.Gb, (byte)myColor.Bb);
			}
		}

		public void DrawRect(float x, float y, float width, float height, int thickness)
		{
			context.DrawRectangle(null, new System.Windows.Media.Pen(new System.Windows.Media.SolidColorBrush(nativeColor), thickness), new System.Windows.Rect(this.x + x * this.width, this.y + y * this.height, width * this.width, height * this.height));
		}

		public void DrawRect(float x, float y, float width, float height)
		{
			context.DrawRectangle(null, new System.Windows.Media.Pen(new System.Windows.Media.SolidColorBrush(nativeColor), 1), new System.Windows.Rect(this.x + x * this.width, this.y + y * this.height, width * this.width, height * this.height));
		}

		public void FillRect(float x, float y, float width, float height)
		{
			context.DrawRectangle(new System.Windows.Media.SolidColorBrush(nativeColor), new System.Windows.Media.Pen(), new System.Windows.Rect(this.x + x * this.width, this.y + y * this.height, width * this.width, height * this.height));
		}

		public void DrawLine(float x1, float y1, float x2, float y2, int thickness)
		{
			context.DrawLine(new System.Windows.Media.Pen(new System.Windows.Media.SolidColorBrush(nativeColor), thickness), new System.Windows.Point(x + x1 * width, y + y1 * height), new System.Windows.Point(x + x2 * width, y + y2 * height));
		}

		public void DrawLine(float x1, float y1, float x2, float y2)
		{
			context.DrawLine(new System.Windows.Media.Pen(new System.Windows.Media.SolidColorBrush(nativeColor), 1), new System.Windows.Point(x + x1 * width, y + y1 * height), new System.Windows.Point(x + x2 * width, y + y2 * height));
		}
		
		public void DrawTriangle(Vector2 p1, Vector2 p2, Vector2 p3)
		{
			DrawTriangle(x + p1.X * width, y + p1.Y * height, x + p2.X * width, y + p2.Y * height, x + p3.X * width, y + p3.Y * height);
		}

		public void DrawTriangle(float x1, float y1, float x2, float y2, float x3, float y3, int thickness)
		{
			DrawLine(x1, y1, x2, y2, thickness);
			DrawLine(x2, y2, x3, y3, thickness);
			DrawLine(x3, y3, x1, y1, thickness);

		}

		public void DrawTriangle(float x1, float y1, float x2, float y2, float x3, float y3)
		{
			DrawLine(x1, y1, x2, y2);
			DrawLine(x2, y2, x3, y3);
			DrawLine(x3, y3, x1, y1);
		}

		public void FillTriangle(Vector2 p1, Vector2 p2, Vector2 p3)
		{
			FillTriangle(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
		}

		public void FillTriangle(float x1, float y1, float x2, float y2, float x3, float y3)
		{
			PathFigure myPathFigure = new PathFigure();
			myPathFigure.StartPoint = new Point(x + x1 * width, y + y1 * height);
			myPathFigure.Segments.Add(new LineSegment(new Point(x + x2 * width, y + y2 * height), false));
			myPathFigure.Segments.Add(new LineSegment(new Point(x + x3 * width, y + y3 * height), false));
			
			PathGeometry myPathGeometry = new PathGeometry();
			myPathGeometry.Figures.Add(myPathFigure);

			context.DrawGeometry(new System.Windows.Media.SolidColorBrush(nativeColor), new System.Windows.Media.Pen(), myPathGeometry);
		}

		public void DrawCircle(Vector2 position, float radius)
		{
			DrawCircle(position.X, position.Y, radius);
		}

		public void DrawCircle(float x, float y, float radius, float thickness)
		{
			context.DrawEllipse(null, new System.Windows.Media.Pen(new System.Windows.Media.SolidColorBrush(nativeColor), thickness), new Point(this.x + x * width, this.y + y * height), radius * width, radius * height);
		}

		public void DrawCircle(float x, float y, float radius)
		{
			context.DrawEllipse(null, new System.Windows.Media.Pen(new System.Windows.Media.SolidColorBrush(nativeColor), 1), new Point(this.x + x * width, this.y + y * height), radius * width, radius * height);
		}

		public void FillCircle(Vector2 position, float radius)
		{
			FillCircle(position.X, position.Y, radius);
		}

		public void FillCircle(float x, float y, float radius)
		{
			context.DrawEllipse(new System.Windows.Media.SolidColorBrush(nativeColor), new System.Windows.Media.Pen(), new Point(this.x + x * width, this.y + y * height), radius * width, radius * height);
		}

		public void DrawStar(int num_points, float x1, float y1, float width, float height, int thickness)
		{
			Rect bounds = new Rect(x1, y1, width, height);
			//Vector2[] points = StarPoints(num_points, bounds);
			Vector2[] allPoints = AllStarPoints(num_points, bounds);

			float x2, y2;

			x1 = allPoints[0].X;
			y1 = allPoints[0].Y;

			for (int i = 1; i < allPoints.Length; i++)
			{
				x2 = allPoints[i].X;
				y2 = allPoints[i].Y;

				DrawLine(x1, y1, x2, y2, thickness);

				x1 = x2;
				y1 = y2;
			}
			x2 = allPoints[0].X;
			y2 = allPoints[0].Y;

			DrawLine(x1, y1, x2, y2, thickness);
		}

		public void DrawStar(int num_points, float x1, float y1, float width, float height)
		{
			Rect bounds = new Rect(x1, y1, width, height);
			//Vector2[] points = StarPoints(num_points, bounds);
			Vector2[] allPoints = AllStarPoints(num_points, bounds);
			
			float x2, y2;

			x1 = allPoints[0].X;
			y1 = allPoints[0].Y;

			for (int i = 1; i < allPoints.Length; i++)
			{
				x2 = allPoints[i].X;
				y2 = allPoints[i].Y;

				DrawLine(x1, y1, x2, y2);

				x1 = x2;
				y1 = y2;
			}

			x2 = allPoints[0].X;
			y2 = allPoints[0].Y;

			DrawLine(x1, y1, x2, y2);
		}

		public void FillStar(int num_points, float x, float y, float width, float height)
		{
			Rect bounds = new Rect(x, y, width, height);
			//Vector2[] points = StarPoints(num_points, bounds);
			Vector2[] allPoints = AllStarPoints(num_points, bounds);
			
			PathFigure myPathFigure = new PathFigure();
			myPathFigure.StartPoint = new Point(this.x + allPoints[0].X * this.width, this.y + allPoints[0].Y * this.height);

			for (int i = 1; i < allPoints.Length; i++)
			{
				myPathFigure.Segments.Add(new LineSegment(new Point(this.x + allPoints[i].X * this.width, this.y + allPoints[i].Y * this.height), false));
			}
			
			PathGeometry myPathGeometry = new PathGeometry();
			myPathGeometry.Figures.Add(myPathFigure);

			context.DrawGeometry(new System.Windows.Media.SolidColorBrush(nativeColor), new System.Windows.Media.Pen(), myPathGeometry);
		}

		public Vector2[] AllStarPoints(int num_points, Rect bounds)
		{
			Vector2[] points = StarPoints(num_points, bounds);
			Vector2[] allPoints = new Vector2[num_points * 2];
			allPoints[0] = points[0];
			allPoints[1] = IntersectionPoint(points[0], points[1], points[2], points[3]);
			allPoints[2] = points[3];
			allPoints[3] = IntersectionPoint(points[0], points[1], points[3], points[4]);
			allPoints[4] = points[1];
			allPoints[5] = IntersectionPoint(points[1], points[2], points[3], points[4]);
			allPoints[6] = points[4];
			allPoints[7] = IntersectionPoint(points[0], points[4], points[1], points[2]);
			allPoints[8] = points[2];
			allPoints[9] = IntersectionPoint(points[2], points[3], points[0], points[4]);
			return allPoints;
		}

// From http://blog.csharphelper.com/2010/02/16/draw-a-star-in-c.aspx

		// Return PointFs to define a star.
		private Vector2[] StarPoints(int num_points, Rect bounds)
		{
			// Make room for the points.
			Vector2[] pts = new Vector2[num_points];

			double rx = bounds.Width / 2;
			double ry = bounds.Height / 2;
			double cx = bounds.X + rx;
			double cy = bounds.Y + ry;

			// Start at the top.
			double theta = -Math.PI / 2;
			double dtheta = 4 * Math.PI / num_points;
			for (int i = 0; i < num_points; i++)
			{
				pts[i] = new Vector2(
					(float)(cx + rx * Math.Cos(theta)),
					(float)(cy + ry * Math.Sin(theta)));
				theta += dtheta;
			}

			return pts;
		}
//****************************************************************************************************//


// from http://stackoverflow.com/questions/4543506/algorithm-for-intersection-of-2-lines
		public Vector2 IntersectionPoint(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
		{
				
			float a1 = p2.Y-p1.Y; 
			float b1 = p1.X-p2.X;
			float c1 = a1 * p1.X + b1 * p1.Y;
			float a2 = p4.Y - p3.Y;
			float b2 = p3.X - p4.X;
			float c2 = a2 * p3.X + b2 * p3.Y;

			float delta = a1 * b2 - a2 * b1;
			if (delta == 0)
				throw new ArgumentException("Lines are parallel");

			float x = (b2 * c1 - b1 * c2) / delta;
			float y = (a1 * c2 - a2 * c1) / delta;

			return new Vector2(x, y);
		}
//****************************************************************************************************//

		/*public void DrawShape(Vector2[] points)
		{
			float x1,y1,x2, y2;

			x1 = points[0].X;
			y1 = points[0].Y;

			for (int i = 1; i < points.Length; i++)
			{
				x2 = points[i].X;
				y2 = points[i].Y;

				DrawLine(x1, y1, x2, y2);

				x1 = x2;
				y1 = y2;
			}

			x2 = points[0].X;
			y2 = points[0].Y;

			DrawLine(x1, y1, x2, y2);
		}*/

		//public void DrawVideo(Video video, float x, float y, float width, float height)
		//{
		//	context.DrawVideo(video._Player, new System.Windows.Rect(this.x + x * this.width, this.y + y * this.height, width * this.width, height * this.height));
		//}

		public DrawingContext _Context
		{
			set
			{
				context = value;
			}
		}
	}
}
