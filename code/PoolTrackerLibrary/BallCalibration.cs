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
        public BallColor[] calibratableBalls = {BallColor.Cue, BallColor.Black, BallColor.Red, BallColor.Orange, BallColor.Brown, BallColor.Yellow, BallColor.Green, BallColor.Blue};
        public BallColor[] colorBalls = {BallColor.Red, BallColor.Orange, BallColor.Brown, BallColor.Yellow, BallColor.Green, BallColor.Blue };

        public Dictionary<BallColor, Image<Bgr,byte>> ballSamples = new Dictionary<BallColor,Image<Bgr,byte>>();
        public Dictionary<BallColor, Hsv> ballHsv = new Dictionary<BallColor, Hsv>();

        public Image<Bgr, byte> tableImage;

        public int hueRedLow = 12, hueYellow = 25, hueGreen = 100, hueRedHigh = 210, satWhite = 40, valBlack = 50;
        public int satOrange = 200, satBrown = 225;

        public float ballFactor = .3F;
        public float whiteFactor = 1.0F;
        public float stripedFactor = .3F;

        public delegate void BallCalibratedHandler(object sender);

        public event BallCalibratedHandler BallCalibrated;

        private const int HUE = 0, SAT = 1, VAL = 2;

        public BallCalibration(ImageBox imageBox, Image<Bgr, byte> tableImage)
        {
            imageBox.Paint += imageBox_Paint;
            imageBox.MouseMove += imageBox_MouseMove;
            imageBox.Click += imageBox_Click;
            this.tableImage = tableImage.Copy();
        }

        public BallCalibration()
        {
        }

        void imageBox_Paint(object sender, PaintEventArgs e)
        {
            ImageBox imageBox = (ImageBox)sender;

            Point local = imageBox.PointToClient(Cursor.Position);
            int ballRadius = 10;
            local.X -= ballRadius;
            local.Y -= ballRadius;
            local.X = (int)(local.X / imageBox.ZoomScale);
            local.Y = (int)(local.Y / imageBox.ZoomScale);

            e.Graphics.FillEllipse(Brushes.Red, local.X , local.Y , 20, 20);
        }

        void imageBox_MouseMove(object sender, MouseEventArgs e)
        {
            ((ImageBox)sender).Invalidate();
        }

        void imageBox_Click(object sender, EventArgs e)
        {
            ImageBoxExtended imageBox = (ImageBoxExtended)sender;

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

        private int findMaxColor(Image<Bgr,byte> image, int channel)
        {
            Image<Gray, byte> mask = Ball.getMask();
            int ballPixels = mask.CountNonzero()[0];
            int threshold = (int)((float)ballPixels * 0.4);

            DenseHistogram hist = make1DHist(image, channel);

            int pixels = 0;
            for (int i = 0; i < 255; i++)
            {
                pixels += (int)hist[i];
                if (pixels > threshold)
                {
                    return i;
                }
            }

            return 255;
        }

        private DenseHistogram make1DHist(Image<Bgr, byte> image, int channel)
        {
            Image<Gray, byte> mask = Ball.getMask();
            DenseHistogram hist = new DenseHistogram(255, new RangeF(0, 255));
            hist.Calculate(new Image<Gray, byte>[] { image.Convert<Hsv, byte>().Split()[channel] }, false, mask);
            return hist;
        }

        private int getAverageInRange(Image<Bgr, byte> image, int channel, int start, int end)
        {
            DenseHistogram hist = make1DHist(image, channel);

            int sum = 0, number = 0;
            for (int i = start; i < end; i++)
            {
                number += (int)hist[i];
                sum += (int)hist[i] * i;        
            }
           
            return number > 0 ? sum / number : 0;
        }

        private void performCalibration()
        {
            Image<Gray,byte> mask = Ball.getMask();
            Image<Gray, byte>[] hsv = tableImage.Convert<Hsv, byte>().Split();

            valBlack = findMaxColor(ballSamples[BallColor.Black], VAL);
            satWhite = findMaxColor(ballSamples[BallColor.Cue], SAT);

            foreach (BallColor color in colorBalls)
            {  
                ballHsv[color] = findBallColor(ballSamples[color]);
            }
        }
    }
}
