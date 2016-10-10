using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIKinect.Core
{
    public class PointTuple<Point, Int, Int2>
    {

        public PointTuple(Point pPoint, int pLife, int pAge) 
        {
            KeyPoint = pPoint;
            Life = pLife;

        }

        public Point KeyPoint { get; set; }
        public int Life { get; set; }
        public int Age { get; set; }


    }
}
