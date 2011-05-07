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
    public enum BallColor
    {
        Cue,
        Yellow,
        Blue,
        Red,
        Purple,
        Orange,
        Green,
        Brown,
        Black,
        YellowWhite,
        RedWhite,
        PurpleWhite,
        OrangeWhite,
        GreenWhite,
        BrownWhite,
        None,
    }

    public class Ball
    {
        public Image<Bgr, byte> patch;
        public Point position;
        public static int ballDia = 26;
        public BallColor color;

        private static int[] histBins = new int[] { 16, 16 };
        private static RangeF[] histRanges = new RangeF[] { new RangeF(0, 180), new RangeF(0, 255) };
        private static int SATURATION_WHITE = 100;
        private static float stripedThreshold = .1F;

        private DenseHistogram hsHist;

        public static int Radius
        {
            get
            {
                return ballDia / 2;
            }
        }

        public static int NumPixels
        {
            get
            {
                return Ball.getMask().CountNonzero()[0];
            }
        }

        public Ball(Image<Bgr, byte> ballPatch, Point pos)
        {
            patch = ballPatch;
            position = pos;
        }

        public Ball(int ballDia)
        {
            //ballDia = ballDia;
        }

        public Ball(Point pos, BallColor color)
        {
            position = pos;
            this.color = color;
        }

        public Brush getBrush()
        {
            switch (color)
            {
                case BallColor.Black:
                    return Brushes.Black;

                case BallColor.Red:
                    return Brushes.Red;

                case BallColor.Green:
                    return Brushes.Green;

                case BallColor.Yellow:
                    return Brushes.Yellow;

                case BallColor.Blue:
                    return Brushes.Blue;
            }
            return Brushes.WhiteSmoke;
        }

        public static Rectangle roiFromCenter(Point centerPos)
        {
            int x = centerPos.X - Radius, y = centerPos.Y - Radius;
            if (x < 0) x = 0;
            if (y < 0) y = 0;

            return new Rectangle(x, y, ballDia, ballDia);
        }

        public static Image<Gray, byte> getMask()
        {
            Image<Gray, byte> ballMask = new Image<Gray, byte>(ballDia, ballDia, new Gray(0));
            ballMask.Draw(new CircleF(new PointF(Radius, Radius), Radius), new Gray(1), -1);

            return ballMask;
        }

        public bool isStriped()
        {
            float whiteRatio = (float)countColors() / (patch.Size.Width * patch.Size.Height);
            return (whiteRatio > stripedThreshold);
        }

        private int countColors()
        {
            Image<Gray, byte>[] patchHsv = patch.Convert<Hsv, byte>().Split();
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