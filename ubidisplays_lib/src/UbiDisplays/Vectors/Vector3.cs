using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Vectors
{
	public struct Vector3
	{
		public float X;
		public float Y;
		public float Z;

		public Vector3(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static Vector3 operator +(Vector3 v1, Vector3 v2)
		{
			return new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
		}

		public static Vector3 operator -(Vector3 v1, Vector3 v2)
		{
			return new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
		}

		public static Vector3 operator *(Vector3 v1, float f)
		{
			return new Vector3(v1.X * f, v1.Y * f, v1.Z * f);
		}

		public static Vector3 operator /(Vector3 v1, float f)
		{
			return new Vector3(v1.X / f, v1.Y / f, v1.Z / f);
		}

		public float SquareLength
		{
			get
			{
				return X * X + Y * Y + Z * Z;
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
				Z *= mul;
			}
		}


		public void Normalize()
		{
			Length = 1;
		}

		public bool IsZero()
		{
			return X == 0 && Y == 0 && Z == 0;
		}

		public static float Dot(Vector3 v1, Vector3 v2)
		{
			return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
		}

		public static Vector3 Cross(Vector3 v1, Vector3 v2)
		{
			float x = v1.Y * v2.Z - v1.Z * v2.Y;
			float y = v1.Z * v2.X - v1.X * v2.Z;
			float z = v1.X * v2.Y - v1.Y * v2.X;
			return new Vector3(x, y, z);
		}

		public float Distance(Vector3 v)
		{
			return (this - v).Length;
		}

		public void Invert()
		{
			X = -X;
			Y = -Y;
			Z = -Z;
		}
	}
}
