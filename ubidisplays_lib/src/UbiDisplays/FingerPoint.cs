using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays
{
    public class FingerPoint
    {
        private float x;
        private float y;
        private int id;

        public float X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        public float Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }

        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public FingerPoint(float pX, float pY, int id)
        {
            x = pX;
            y = pY;
            this.id = id;
        }
    }
}
