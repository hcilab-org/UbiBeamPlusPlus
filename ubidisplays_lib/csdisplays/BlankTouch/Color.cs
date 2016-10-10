using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlankTouch
{
	public struct Color
	{
		// Creates a new Color object from a packed 32 bit ARGB value.
		public static Color fromValue(uint value)
		{
			var color = new Color();
			color.myValue = value;
			return color;
		}

		// Creates a new Color object from components in the range 0 - 255.
		public static Color fromBytes(int r, int g, int b, int a = 255)
		{
			var color = new Color();
			color.myValue = ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | (uint)b;
			return color;
		}

		// Creates a new Color object from components in the range 0 - 1.
		public static Color fromFloats(float r, float g, float b, float a = 1)
		{
			var color = new Color();
			color.myValue = ((uint)(a * 255) << 24) | ((uint)(r * 255) << 16) | ((uint)(g * 255) << 8) | (uint)(b * 255);
			return color;
		}

		// Contains a byte representing the red color component.
		public int Rb
		{
			get
			{
				return getRedByte();
			}
		}

		// Contains a byte representing the green color component.
		public int Gb
		{
			get
			{
				return getGreenByte();
			}
		}

		// Contains a byte representing the blue color component.
		public int Bb
		{
			get
			{
				return getBlueByte();
			}
		}

		// Contains a byte representing the alpha color component (more exactly the opacity component - a value of 0 is fully transparent).
		public int Ab
		{
			get { return getAlphaByte(); }
		}

		public float R
		{
			get { return getRed(); }
		}

		public float G
		{
			get { return getGreen(); }
		}
		public float B
		{
			get { return getBlue(); }
		}
		public float A
		{
			get { return getAlpha(); }
		}

		private uint myValue;

		public uint Value
		{
			get { return myValue; }
		}

		private int getRedByte()
		{
			return (int)((Value & 0x00ff0000) >> 16);
		}

		private int getGreenByte()
		{
			return (int)((Value & 0x0000ff00) >> 8);
		}

		private int getBlueByte()
		{
			return (int)(Value & 0x000000ff);
		}

		private int getAlphaByte()
		{
			return (int)((Value & 0xff000000) >> 24);
		}

		private float getRed()
		{
			return getRedByte() / 255.0f;
		}

		private float getGreen()
		{
			return getGreenByte() / 255.0f;
		}

		private float getBlue()
		{
			return getBlueByte() / 255.0f;
		}

		private float getAlpha()
		{
			return getAlphaByte() / 255.0f;
		}
	}
}
