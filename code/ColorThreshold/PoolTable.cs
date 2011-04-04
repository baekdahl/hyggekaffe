using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using Emgu.CV.CvEnum;
using System.Diagnostics;

namespace PoolTracker
{
    class PoolTable
    {
        //Histogram comparison options:
        public static HISTOGRAM_COMP_METHOD comparisonMethod = HISTOGRAM_COMP_METHOD.CV_COMP_INTERSECT;
        public static int[] histBins = new int[] { 16, 16 };
        public static RangeF[] histRanges = new RangeF[] { new RangeF(0, 180), new RangeF(0, 255) };
        
        private Image<Bgr, byte> _tableImage;
        private Image<Hsv, byte> _tableImageHsv;
        private Image<Gray, byte>[] _tablePlanes;
        public Image<Gray, byte> _tableMatchMask;
        public static int ballDia = 24;
        

        public float bestMatch = 0;

        public Image<Gray, byte> backProjectShow;

        public PoolTable(Image<Bgr, byte> tableImage)
        {
            _tableImage = tableImage;
            _tableImageHsv = tableImage.Convert<Hsv, byte>();
            _tablePlanes = _tableImageHsv.Split();
            _tableMatchMask = ImageUtil.thresholdAdaptiveMax(_tablePlanes[0], 1);
            _tableMatchMask = new Image<Gray, byte>(tableImage.Width, tableImage.Height, new Gray(1));
        }

        public Point findBall(Image<Bgr, byte> ballImage)
        {
            return findBall(ballImage.Convert<Hsv,Byte>());
        }

        public Point findBall(Image<Hsv, byte> ballImage)
        { 
            DenseHistogram ballHist = new DenseHistogram(histBins, histRanges);
            Image<Gray, byte>[] ballPlanes = ballImage.Split();

            float ballRadius = ballDia/2;
            Image<Gray, byte> ballMask = new Image<Gray, byte>(ballDia, ballDia, new Gray(0));

            ballMask.Draw(new CircleF(new PointF(ballRadius, ballRadius), ballRadius), new Gray(1), -1);

            ballHist.Calculate<byte>(new Image<Gray, byte>[] { ballPlanes[0], ballPlanes[1] }, false, ballMask);

            //Image<Gray, float> backProjection = ballHist.BackProjectPatch<byte>(new Image<Gray, byte>[] { _tablePlanes[0], _tablePlanes[1] }, new Size(35, 35), comparisonMethod, 1);
            Image<Gray, float> backProjection = ImageUtil.backProjectPatchMasked(ballHist, new Image<Gray, byte>[] { _tablePlanes[0], _tablePlanes[1] }, ballMask, comparisonMethod, _tableMatchMask);
           
            double[] min, max;
            Point[] minLoc, maxLoc;
            Point ballPos;

            backProjection.MinMax(out min, out max, out minLoc, out maxLoc);

            ballPos = ImageUtil.matchHigh(comparisonMethod) ? maxLoc[0] : minLoc[0];
            bestMatch = ImageUtil.matchHigh(comparisonMethod) ? (float)max[0] : (float)min[0];
            
            backProjectShow = backProjection.Convert<Gray, byte>();
            backProjectShow._EqualizeHist();

            _tableMatchMask.Draw(new CircleF(ballPos, ballDia - 5), new Gray(0), -1);

            return ballPos;

        }

        private Size backProjectSize()
        {
            return new Size(_tableImage.Width - ballDia + 1, _tableImage.Height - ballDia + 1);
        }   

        private DenseHistogram backgroundHist()
        {
            Ball backgroundBall = new Ball(new Image<Hsv, byte>("balls/bg.jpg"), ballDia);
            return backgroundBall.histogram;
        }

        private DenseHistogram getBallHistogram(Point position)
        {
            Rectangle roi = new Rectangle(position, new Size(ballDia, ballDia));
            DenseHistogram hist = new DenseHistogram(histBins, histRanges);

            foreach (Image<Gray, byte> plane in _tablePlanes)
            {
                plane.ROI = roi;
            }
            //hist.Calculate(new Image<Gray, byte>[] { _tablePlanes[0], _tablePlanes[1] }, false, Ball.getMask());
            return hist;
        }

        public List<Ball> findBalls(int numberOfBalls)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Image<Gray, byte>[] planes = { _tablePlanes[0], _tablePlanes[1] };
            //Image<Gray, float> projection = ImageUtil.backProjectPatchMasked(backgroundHist(), planes, Ball.getMask(), comparisonMethod, _tableMatchMask);
            Image<Gray, float> projection = adaptiveBallDetect(_tablePlanes[0]);
            backProjectShow = projection.Convert<Gray, byte>();
            backProjectShow._EqualizeHist();
            sw.Stop();
            Debug.Write("ProjTime:" + sw.ElapsedMilliseconds + Environment.NewLine);


            bool matchHigh = ImageUtil.matchHigh(comparisonMethod);
            List<Ball> returnList = new List<Ball>();
            Point coordDiff = new Point(_tablePlanes[0].Size - projection.Size);
            _tableMatchMask = new Image<Gray, byte>(projection.Width, projection.Height, new Gray(1));
            //_tableMatchMask.ROI = new Rectangle(coordDiff.X / 2, coordDiff.Y / 2, projection.Width, projection.Height);
            for (int i=0; i < numberOfBalls; i++) {
                
                KeyValuePair<Point, float> extremum = ImageUtil.findExtremum(projection, _tableMatchMask, false);

                Point ballInTableCoords = new Point(extremum.Key.X + coordDiff.X / 2, extremum.Key.Y + coordDiff.Y / 2);

                returnList.Add(new Ball(getBallHistogram(ballInTableCoords), ballInTableCoords));
                
                //Mark area matched in mask
                _tableMatchMask.Draw(new CircleF(extremum.Key, ballDia - 5), new Gray(0), -1);
            }
            return returnList;
        }

        public Image<Gray,float> adaptiveBallDetect(Image<Gray, byte> input)
        {
            int h_split = 10;
            int v_split = 5;
            int h_stepsize = input.Width / h_split;
            int v_stepsize = input.Height / v_split;
            
            Image<Gray, byte> template = new Image<Gray,byte>(ballDia+2, ballDia+2);

            Image<Gray, float> img_out = new Image<Gray,float>(input.Size.Width-template.Width+1, input.Size.Height-template.Height+1);

            for (int h = 0; h < h_split; h++)
            {
                for (int v = 0; v < v_split; v++)
                {
                    Rectangle ROI = new Rectangle(new Point(h_stepsize * h, v_stepsize * v), new Size(h_stepsize, v_stepsize));
                    input.ROI = ROI;
                    img_out.ROI = ROI;

                    DenseHistogram hist = new DenseHistogram(255, new RangeF(0, 255));
                    hist.Calculate(new Image<Gray, Byte>[] { input }, false, null);

                    float maxValue = 0;
                    float minValue = 0;
                    int[] maxLocation = { 0 };
                    int[] minLocation = { 0 };
                    hist.MinMax(out minValue, out maxValue, out minLocation, out maxLocation);

                    //ROI.Offset(-ballDia, -ballDia);
                    ROI.Width += ballDia+1;
                    ROI.Height += ballDia+1;
                    input.ROI = ROI;
                    
                    template.Draw(new CircleF(new PointF(ballDia/2, ballDia/2), ballDia/2), new Gray(maxLocation[0]), -1);
                    Image<Gray, float> match = input.MatchTemplate(template, TM_TYPE.CV_TM_CCOEFF);

                    //img_out.ROI.Width = match.Width;
                    //img_out.ROI.Height = match.Height;
                    match.CopyTo(img_out);
                }
            }
            img_out.ROI = Rectangle.Empty;
            input.ROI = Rectangle.Empty;

            return img_out;
        }
    }
}
