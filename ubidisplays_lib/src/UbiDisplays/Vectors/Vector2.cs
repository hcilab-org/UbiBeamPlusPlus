using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Vectors
{
	public struct Vector2
	{
		public float X;
		public float Y;
		
		public Vector2(float x, float y)
		{
			X = x;
			Y = y;
		}

		public static Vector2 operator +(Vector2 v1, Vector2 v2)
		{
			return new Vector2(v1.X + v2.X, v1.Y + v2.Y);
		}

		public static Vector2 operator -(Vector2 v1, Vector2 v2)
		{
			return new Vector2(v1.X - v2.X, v1.Y - v2.Y);
		}

		public static Vector2 operator *(Vector2 v, float f)
		{
			return new Vector2(v.X * f, v.Y * f);
		}

		public static Vector2 operator *(float f, Vector2 v)
		{
			return new Vector2(v.X * f, v.Y * f);
		}

		public static Vector2 operator /(Vector2 v1, float f)
		{
			return new Vector2(v1.X / f, v1.Y / f);
		}

		public float SquareLength
		{
			get
			{
				return X * X + Y * Y;
			}
		}

		public float Length
		{
			get
			{
				return (float)Math.Sqrt(SquareLength);
			}
			set
			{
				float mul = value / Length;
				X *= mul;
				Y *= mul;
			}
		}


		public void Normalize()
		{
			Length = 1;
		}

		public bool IsZero()
		{
			return X == 0 && Y == 0;
		}

		public static float Dot(Vector2 v1, Vector2 v2)
		{
			return v1.X * v2.X + v1.Y * v2.Y;
		}

		public float Distance(Vector2 v)
		{
			return (this - v).Length;
		}

		public void Invert()
		{
			X = -X;
			Y = -Y;
		}

		public Vector2 Reflect(Vector2 normal)
		{
			normal.Normalize();
			return -2 * Dot(this, normal) * normal + this;
		}

		public double CrossProduct(Vector2 v1, Vector2 v2)
		{
			return (v1.X * v2.Y) - (v1.Y * v2.X);
		}
	}
}
