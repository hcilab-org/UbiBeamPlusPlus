using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UbiDisplays.Vectors;

namespace UbiDisplays
{
	public class MouseHand : Hand
	{
		private int x;
		private int y;
		private bool down;
        private float xleft;
        private float ytop;
		private float width;
		private float height;

		public MouseHand(float xleft, float ytop, float width, float height)
		{
            this.xleft = xleft;
            this.ytop = ytop;
			this.width = width;
			this.height = height;
		}

		public override Vector3 Position
		{
			get { return new Vector3((x - xleft) * 100 / width, (y - ytop) * 100 / height, 0); }
		}

		public override FingerPoint GetFinger(int index)
		{
			return new FingerPoint((x - xleft) * 100 / width, (y - ytop) * 100 / height, -1);
		}

		public override int FingerCount()
		{
			return (down && x >= xleft && y >= ytop && x <= xleft + width && y <= ytop + height) ? 1 : 0;
		}

		public void MouseDown(int x, int y)
		{
			this.x = x;
			this.y = y;
			down = true;
		}

		public void MouseUp(int x, int y)
		{
			this.x = x;
			this.y = y;
			down = false;
		}

		public void MouseMove(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public override bool IsFist()
		{
			return down;
		}
	}
}
