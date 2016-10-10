using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UbiDisplays.Vectors;

namespace UbiDisplays
{
    struct Touch
    {
        public int x;
        public int y;
        public int id;

        public Touch(int x, int y, int id)
        {
            this.x = x;
            this.y = y;
            this.id = id;
        }
    }

    public class TouchHand : Hand
    {
        private float width;
        private float height;
        private List<Touch> touchPoints = new List<Touch>();

        public TouchHand(float width, float height)
        {
            this.width = width;
            this.height = height;
        }

        public override Vector3 Position
        {
            get
            {
                int x = -50;
                int y = -50;
                if (touchPoints.Count() > 0)
                {
                    x = touchPoints[0].x;
                    y = touchPoints[0].y;
                }
                return new Vector3(x * 100 / width, y * 100 / height, 0);
            }
        }

        public override FingerPoint GetFinger(int index)
        {
            int x = touchPoints[index].x;
            int y = touchPoints[index].y;
            return new FingerPoint(x * 100 / width, y * 100 / height, touchPoints[index].id);
        }

        public override int FingerCount()
        {
            return touchPoints.Count();
        }

        private int FindTouch(int id)
        {
            for (int i = 0; i < touchPoints.Count(); ++i)
            {
                if (touchPoints[i].id == id) return i;
            }
            return -1;
        }

        public void Down(int x, int y, int id)
        {
            System.Console.WriteLine("Touch down: " + id);
            var touchIndex = FindTouch(id);
            if (touchIndex < 0)
            {
                touchPoints.Add(new Touch(x, y, id));
            }
            else
            {
                // Should never happen
                touchPoints[touchIndex] = new Touch(x, y, id);
            }
        }

        public void Up(int x, int y, int id)
        {
            touchPoints.RemoveAt(FindTouch(id));
        }

        public void Move(int x, int y, int id)
        {
            touchPoints[FindTouch(id)] = new Touch(x, y, id);
        }

        public override bool IsFist()
        {
            return false;
        }
    }
}
