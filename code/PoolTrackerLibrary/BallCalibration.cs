    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Drawing;
using Emgu.CV.UI;
using System.Windows.Forms;
using System.Diagnostics;

namespace PoolTrackerLibrary
{
    public class BallCalibration
    {
        public BallColor[] calibratableBalls = {BallColor.Cue, BallColor.Black, BallColor.Red, BallColor.Orange, BallColor.Brown, BallColor.Yellow, BallColor.Green, BallColor.Blue, BallColor.Purple};
        public BallColor[] colorBalls = {BallColor.Red, BallColor.Orange, BallColor.Brown, BallColor.Yellow, BallColor.Green, BallColor.Blue };

        public Dictionary<BallColor, Image<Bgr,byte>> ballSamples = new Dictionary<BallColor,Image<Bgr,byte>>();
        public Dictionary<BallColor, Hsv> ballHsv = new Dictionary<BallColor, Hsv>();
        public Dictionary<BallColor, Bgr> ballBgr = new Dictionary<BallColor, Bgr>();

        public Image<Bgr, byte> tableImage;

        public int hueRedLow = 12, hueYellow = 25, hueGreen = 100, hueRedHigh = 210, satWhite = 150, valBlack = 60;
        public int satOrange = 200, satBrown = 225;

        public float whiteRatioInCue = .93F;
        public float ballFactor = .25F;
        public float whiteFactor = .75F;
        public float stripedFactor = .19F;

        public delegate void BallCalibratedHandler(object sender);

        public event BallCalibratedHandler BallCalibrated;

        private const int HUE = 0, SAT = 1, VAL = 2;

        private BallColor[, ,] colorDistanceTable;

        public BallCalibration(PictureBoxExtended imageBox, Image<Bgr, byte> tableImage)
        {
            imageBox.MouseMoveOverImage += imageBox_MouseMove;
            imageBox.Click += imageBox_Click;
            this.tableImage = tableImage.Copy();
        }

        public BallCalibration()
        {
            ballBgr[BallColor.Cue] = new Bgr(255, 255, 255);
            ballBgr[BallColor.Black] = new Bgr(0, 0, 0);
            ballBgr[BallColor.Red] = new Bgr(14, 13, 162);
            ballBgr[BallColor.Orange] = new Bgr(31, 56, 176);
            ballBgr[BallColor.Brown] = new Bgr(26, 38, 111);
            ballBgr[BallColor.Yellow] = new Bgr(47, 190, 245);
            ballBgr[BallColor.Green] = new Bgr(15, 51, 18);
            ballBgr[BallColor.Blue] = new Bgr(18, 5, 0);
            ballBgr[BallColor.Purple] = new Bgr(18, 5, 0);

            //buildDistanceTable();
        }

        void imageBox_MouseMove(object sender, MouseEventArgs e)
        {
            ((Control)sender).Invalidate();
        }

        void imageBox_Click(object sender, EventArgs e)
        {
            PictureBoxExtended imageBox = (PictureBoxExtended)sender;

            Point local = imageBox.MousePositionOnImage;
            setNextValue(local);
        }

        public BallColor nextBall()
        {
            foreach (BallColor ball in calibratableBalls) {
                if (!ballSamples.ContainsKey(ball)) {
                    return ball;
                }
            }
            return BallColor.None;
        }

        public void setNextValue(Point position)
        {
            Image<Bgr, byte> ball = tableImage.Copy(Ball.roiFromCenter(position));

            ballSamples[nextBall()] = ball;
            BallCalibrated(this);
            if (nextBall() == BallColor.None)
            {
                performCalibration();
            }
        }

        private Hsv findBallColor(Image<Bgr, byte> ball)
        {
            RangeF range = new RangeF(0, 255);
            Image<Gray,byte>[] hsvPlanes = ball.Convert<Hsv,byte>().Split();
            
            DenseHistogram hist = new DenseHistogram(new int[] { 180, 256}, new RangeF[] { new RangeF(0, 180), range});
            hist.Calculate(new Image<Gray, byte>[] { hsvPlanes[0], hsvPlanes[1] }, false, Ball.getMask());

            float minVal, maxVal;
            int[] maxLoc, minLoc;
            hist.MinMax(out minVal, out maxVal, out minLoc, out maxLoc);
            
            int hue = maxLoc[0];

            int totalSum = 0, count = 0;
            for (int i = 0; i < 180; i++)
            {
                int sum = 0;
                for (int j = satWhite; j < 256; j++)
                {
                    count += (int)hist[i, j];
                    sum += (int)hist[i, j] * j;
                }
                totalSum += sum;    
            }

            int satAvg = totalSum / count;

            return new Hsv(hue, satAvg, 0);
        }

        private Bgr findBgr(Image<Bgr, byte> ball)
        {
            Image<Gray, byte>[] planes = ball.Split();
            int[] maxLoc = new int[3];

            for (int color = 0; color < 3; color++)
            {
                DenseHistogram hist = ImageUtil.makeHist(planes[color], Ball.getMask());
                //maxLoc[color] = ImageUtil.histMaxValue(hist);
                maxLoc[color] = ImageUtil.histMeanValue(hist);
            }

            return new Bgr(maxLoc[0], maxLoc[1], maxLoc[2]);
        }

        private void performCalibration()
        {
            Image<Gray,byte> mask = Ball.getMask();
            Image<Gray, byte>[] hsv = tableImage.Convert<Hsv, byte>().Split();
                
            Bgr white = new Bgr(255, 255, 255);
            foreach (BallColor color in calibratableBalls)
            {  
                ballBgr[color] = findBgr(ballSamples[color]);
                double angle = Util.shortestAngle(ballBgr[color], white);
                Debug.Write(color.ToString() + "color:"+ ballBgr[color].ToString() + "angle: " + angle + Environment.NewLine);
            }
            int[] cueVotes = countVotesForBalls(ballSamples[BallColor.Cue]);

            whiteRatioInCue = (float)cueVotes[0] / Ball.NumPixels;
            Debug.Write("WhiteRatioInCue: " + whiteRatioInCue);
        }

        public BallColor nearestBallColor(Bgr color, bool useDistanceTable = true)
        {
            if (useDistanceTable && colorDistanceTable != null)
            {
                return colorDistanceTable[(int)color.Blue, (int)color.Green, (int)color.Red];
            }
            
            double smallest = double.MaxValue;
            BallColor best = BallColor.None;

            foreach (BallColor ball in calibratableBalls)
            {
                //double dist = Util.shortestAngle(color, ballBgr[ball]);
                double dist = Util.euclideanDistance(color, ballBgr[ball]);
                if (dist < smallest)
                {
                    smallest = dist;
                    best = ball;
                }
            }

            return best;
        }

        public int[] countVotesForBalls(Image<Bgr, byte> image)
        {
            Image<Gray, Byte> mask = Ball.getMask();

            if (ballBgr.Count == 0)
            {
                return null;
            }

            int[] votes = new int[16];

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if (mask.Data[y, x, 0] == 1)
                    {
                        BallColor nearest = nearestBallColor(image[y, x], false);
                        votes[(int)nearest]++;
                    }
                }
            }

            return votes;
        }

        private void buildDistanceTable()
        {
            Stopwatch sw = Util.getWatch();
            colorDistanceTable = new BallColor[256, 256, 256];

            for (int blue = 0; blue < 256; blue++)
            {
                for (int green = 0; green < 256; green++)
                {
                    for (int red = 0; red < 256; red++)
                    {
                        colorDistanceTable[blue, green, red] = nearestBallColor(new Bgr(blue, green, red), false);
                    }
                }
            }
            Util.writeWatch(sw, "Build distance table");
        }
    }
}
