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
        public Point position;
        public static int ballDia = 26;
        public BallColor color;
        public int score;

        public static BallCalibration calibration;

        private int whitePixels, blackPixels;
        private int[] hueHist, satHist;
        private int hueMean, satMean;

        public static int BallThreshold
        {
            get
            {
                return (int)((double)Ball.NumPixels * calibration.ballFactor);
            }
        }

        public static int WhiteThreshold
        {
            get
            {
                return (int)((double)BallThreshold * calibration.whiteFactor);
            }
        }
        public static int StripedThreshold
        {
            get
            {
                return (int)((double)BallThreshold * calibration.stripedFactor);
            }
        }


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


        public Ball(Point pos, int whitePixels, int blackPixels, int[] hueHist, int[] satHist)
        {
            this.whitePixels = whitePixels;
            this.blackPixels = blackPixels;
            this.hueHist = hueHist;
            this.satHist = satHist;
            this.position = pos;

            this.color = identifyBall();
        }

        public BallColor identifyBall()
        {
            int maxHueDist = 30;
            hueMean = Util.getMaxIndex(hueHist);
            satMean = Util.getMean(satHist);

            int start = hueMean - maxHueDist;
            if (start < 1) start += 179;

            int stop = hueMean + maxHueDist;
            if (stop > 179) stop -= 179;

            score = 0;
            for (int i = start; i != stop; i++)
            {
                if (i == 180) i = 0;

                score += hueHist[i];
            }

            if (whitePixels > WhiteThreshold)
            {
                score = whitePixels;
                return BallColor.Red; 
            }
            else if (blackPixels > BallThreshold)
            {
                score = blackPixels;
                return BallColor.Red;
                
            }
            else if (score + whitePixels > BallThreshold)
            {
                score = score + whitePixels;
                return BallColor.Red;
                //BallColor color = calibration.ballHsv.Count != 0 ? findColor() : BallColor.Red;

                if (whitePixels > StripedThreshold)
                {
                    return color + 8;
                }
                else
                {
                    return color;
                }  
            }
            else
            {
                return BallColor.None;
            }
        }

        private BallColor findColor()
        {
            double smallest = double.MaxValue;
            BallColor best = BallColor.None;

            foreach (BallColor ball in calibration.colorBalls)
            {
                double dist = ImageUtil.hsDist(new Hsv(hueMean, satMean, 0), calibration.ballHsv[ball]);
                if (dist < smallest)
                {
                    smallest = dist;
                    best = ball;
                }
            }

            return best;
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
    }
}