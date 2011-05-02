using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.Diagnostics;

namespace PoolTrackerLibrary
{
    public class Ball
    {
        public Image<Bgr, byte> patch;
        public Point position;
        public int ballDia;

        private static int[] histBins = new int[] { 16, 16 };
        private static RangeF[] histRanges = new RangeF[] { new RangeF(0, 180), new RangeF(0, 255) };
        private static int SATURATION_WHITE = 100;
        private static float stripedThreshold = .1F;

        private DenseHistogram hsHist;

        public Ball(Image<Bgr,byte> ballPatch, Point pos)
        {
            patch = ballPatch;
            position = pos;
        }

        public Image<Gray, byte> getMask()
        {
            float ballRadius = ballDia / 2;
            Image<Gray, byte> ballMask = new Image<Gray, byte>(ballDia, ballDia, new Gray(0));
            ballMask.Draw(new CircleF(new PointF(ballRadius, ballRadius), ballRadius), new Gray(1), -1);

            return ballMask;
        }

        public bool isStriped()
        {  
            float whiteRatio = (float)countColors()/(patch.Size.Width*patch.Size.Height);
            return (whiteRatio > stripedThreshold);
        }

        private int countColors()
        {
            Image<Gray,byte>[] patchHsv = patch.Convert<Hsv,byte>().Split();
            int whitePixels = 0;

            for (int x = 0; x < patch.Width; x++)
            {
                for (int y = 0; y < patch.Height; y++)
                {
                    if (patchHsv[1].Data[y, x, 0] < SATURATION_WHITE)
                    {
                        whitePixels++;
                    }
                }
            }
            Debug.Write("White pixels: " + whitePixels); 
            return whitePixels;
        }
    }
}