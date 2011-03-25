using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace PoolTracker
{
    class Ball
    {
        public static int ballDia = 26;

        public DenseHistogram histogram;
        public Point position;

        public Ball(Image<Hsv, byte> image, int ballDia)
        {
            histogram = new DenseHistogram(PoolTable.histBins, PoolTable.histRanges);
            Image<Gray, byte>[] ballPlanes = image.Split();
  
            histogram.Calculate<byte>(new Image<Gray, byte>[] { ballPlanes[0], ballPlanes[1] }, false, getMask());
        }

        public Ball(DenseHistogram hist, Point pos)
        {
            histogram = new DenseHistogram(PoolTable.histBins,PoolTable.histRanges);
            hist.Copy(histogram);
            position = pos;
        }

        public static Image<Gray, byte> getMask()
        {
            float ballRadius = ballDia / 2;
            Image<Gray, byte> ballMask = new Image<Gray, byte>(ballDia, ballDia, new Gray(0));
            ballMask.Draw(new CircleF(new PointF(ballRadius, ballRadius), ballRadius), new Gray(1), -1);

            return ballMask;
        }
    }
}
