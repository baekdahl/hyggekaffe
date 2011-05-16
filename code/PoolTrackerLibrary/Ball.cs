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
        public DenseHistogram hist;

        public static BallCalibration calibration;

        private int whitePixels, blackPixels;
        private int[] hueHist, satHist;
        private int hueMean, satMean;
        private Image<Bgr, byte> image;

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
                return (int)((double)Ball.NumPixels * calibration.whiteFactor);
            }
        }
        public static int StripedThreshold
        {
            get
            {
                return (int)((double)Ball.NumPixels * calibration.stripedFactor);
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

            this.color = calculateProbabilty();
        }

        public void imageFromTable(Image <Bgr,byte> tableImage)
        {
            this.image = tableImage.Copy(roiFromCenter(position));
        }

        public BallColor calculateProbabilty()
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
                return BallColor.Cue; 
            }
            else if (blackPixels > BallThreshold)
            {
                score = blackPixels;
                return BallColor.Black;
                
            }
            else if (score + whitePixels > BallThreshold)
            {
                score = score + whitePixels;

                BallColor color = BallColor.Red;

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

        public BallColor findColor()
        {
            if (calibration.ballBgr.Count == 0)
            {
                return BallColor.Red;
            }
            int[] votes = new int[16];

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    BallColor nearest = calibration.nearestBallColor(image[y, x], false);
                    votes[(int)nearest]++;
                }
            }

            int maxIndex = 0, max = 0;
            for (int i = 1; i < votes.Length; i++)
            {
                if (votes[i] > max)
                {
                    max = votes[i];
                    maxIndex = i;
                }
            }
            this.color = (BallColor)maxIndex;

            return this.color;
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

        public static DenseHistogram histogram(Image<Bgr,byte> image, Point pos)
        {
            image.ROI = (pos == Point.Empty) ? Rectangle.Empty : roiFromCenter(pos);
            
            RangeF range = new RangeF(0,255);
            DenseHistogram hist = new DenseHistogram(new int[] {256, 256, 256}, new RangeF[] { range, range, range });
            hist.Calculate(image.Split(), false, null);

            image.ROI = Rectangle.Empty;
            return hist;
        }

        public static Image<Gray, byte> getMask()
        {
            Image<Gray, byte> ballMask = new Image<Gray, byte>(ballDia, ballDia, new Gray(0));
            ballMask.Draw(new CircleF(new PointF(Radius, Radius), Radius), new Gray(1), -1);

            return ballMask;
        }
    }
}