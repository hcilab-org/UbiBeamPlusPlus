using System;
using System.Windows.Media;
using UbiDisplays.Vectors;

namespace UbiDisplays
{
	public abstract class Hand
	{
		public abstract Vector3 Position { get; }

		public abstract FingerPoint GetFinger(int index);

		public abstract int FingerCount();

		public virtual bool IsFist()
		{
			return (new Vector3((float)GetFinger(0).X, (float)GetFinger(0).Y, 0) - Position).Length < 50;
		}

		public virtual void Update()
		{

		}

		public virtual ImageSource GetImage()
		{
			return null;
		}

		public virtual void Stop()
		{

		}
	}
}
