using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlankTouch
{
	public class FontStyle
	{
		private bool bold;
		private bool italic;
		private bool underlined;

		public FontStyle(bool bold, bool italic, bool underlined)
		{
			this.bold = bold;
			this.italic = italic;
			this.underlined = underlined;
		}

		public bool IsBold()
		{
			return bold;
		}

		public bool IsItalic()
		{
			return italic;
		}

		public bool IsUnderlined()
		{
			return underlined;
		}
	}

	public class Font
	{
		public Font(string name, FontStyle style, float size)
		{
			this.Name = name;
			this.Style = style;
			this.Size = size;
		}

		public string Name;
		public FontStyle Style;
		public float Size;

		private System.Windows.Media.FormattedText GetFormat(string text = "ABC")
		{
			System.Windows.Media.FormattedText fText = new System.Windows.Media.FormattedText(text,
					System.Globalization.CultureInfo.GetCultureInfo("en-us"), System.Windows.FlowDirection.LeftToRight,
					new System.Windows.Media.Typeface(Name), MainWindow.Height * Size, System.Windows.Media.Brushes.Black);
			if (Style.IsBold()) fText.SetFontWeight(System.Windows.FontWeights.Bold);
			if (Style.IsItalic()) fText.SetFontStyle(System.Windows.FontStyles.Italic);
			if (Style.IsUnderlined()) fText.SetTextDecorations(System.Windows.TextDecorations.Underline);
			return fText;
		}

		public float Height
		{
			get
			{
				return (float)GetFormat().Height / MainWindow.Height;
			}
		}

		public float WidthOf(string str)
		{
			return (float)GetFormat(str).WidthIncludingTrailingWhitespace / MainWindow.Width;
		}

		public float BaselinePosition
		{
			get
			{
				return (float)GetFormat().Baseline / MainWindow.Height;
			}
		}
	}
}
