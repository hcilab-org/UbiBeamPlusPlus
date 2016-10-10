using System.Windows.Media.Imaging;

namespace BlankTouch
{
	public class Image
	{
		private BitmapImage image;
		private string filename;

		public Image(string filename)
		{
			this.filename = filename;
			image = new System.Windows.Media.Imaging.BitmapImage(new System.Uri(filename, System.UriKind.Relative));
			int a = image.PixelWidth;
		}

		public int Width
		{
			get
			{
				return image.PixelWidth;
			}
		}

		public int Height
		{
			get
			{
				return image.PixelHeight;
			}
		}

		public bool IsOpaque(int x, int y)
		{
			if (x < 0 || y < 0 || x >= image.PixelWidth || y >= image.PixelHeight)
				return false;

			byte[] pixels = new byte[8];
			image.CopyPixels(new System.Windows.Int32Rect(x, y, 1, 1), pixels, image.PixelWidth * 4, 0);
			return pixels[0] > 0;
		}

		public BitmapImage _Image
		{
			get
			{
				return image;
			}
		}
	}
}
