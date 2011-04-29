using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace PoolTrackerLibrary
{
    public class Ball
    {
        public DenseHistogram histogram;
        public Point position;
        public int ballDia;

        private static int[] histBins = new int[] { 16, 16 };
        private static RangeF[] histRanges = new RangeF[] { new RangeF(0, 180), new RangeF(0, 255) };

        public Ball(Image<Hsv, byte> image, int ballDia)
        {
            this.ballDia = ballDia;
            histogram = new DenseHistogram(histBins, histRanges);
            Image<Gray, byte>[] ballPlanes = image.Split();

            histogram.Calculate<byte>(new Image<Gray, byte>[] { ballPlanes[0], ballPlanes[1] }, false, getMask());
        }

        public Ball(DenseHistogram hist, Point pos)
        {
            histogram = new DenseHistogram(histBins, histRanges);
            hist.Copy(histogram);
            position = pos;
        }

        public Image<Gray, byte> getMask()
        {
            float ballRadius = ballDia / 2;
            Image<Gray, byte> ballMask = new Image<Gray, byte>(ballDia, ballDia, new Gray(0));
            ballMask.Draw(new CircleF(new PointF(ballRadius, ballRadius), ballRadius), new Gray(1), -1);

            return ballMask;
        }
    }
}