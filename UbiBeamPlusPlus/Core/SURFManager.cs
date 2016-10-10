using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UIKinect.Core
{
    class SURFManager
    {
        private static SURFManager instance;


        public static SURFManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SURFManager();
                }
                return instance;
            }
        }

        public static List<Point> getDescriptorPoints(Image<Gray, Byte> grayFrame)
        {
            List<Point> ret = new List<Point>();
            VectorOfKeyPoint observedKeyPoints;
            long  matchTime = 0;
 
            if (grayFrame != null)
            {
                FindKeypoints(grayFrame, out matchTime, out observedKeyPoints);

                foreach (Emgu.CV.Structure.MKeyPoint point in observedKeyPoints.ToArray())
                {
                    ret.Add(new Point((int)point.Point.X, (int)point.Point.Y));
                }
            }
            // Console.WriteLine("Matchtime: " + matchTime + " Features: " + ret.Count);
            return ret;
        }


        public static void FindKeypoints(Image<Gray, byte> observedImage, out long matchTime, out VectorOfKeyPoint observedKeyPoints)
        {
            int k = 2;
            double uniquenessThreshold = 0.8;
            SURFDetector surfCPU = new SURFDetector(500, false);
            Stopwatch watch;
            watch = Stopwatch.StartNew();

            observedKeyPoints = new VectorOfKeyPoint();
            Matrix<float> observedDescriptors = surfCPU.DetectAndCompute(observedImage, null, observedKeyPoints);
            watch.Stop();
            matchTime = watch.ElapsedMilliseconds;
        }
    }
}
