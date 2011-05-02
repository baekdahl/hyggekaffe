﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace PoolTrackerLibrary
{
    public class BallLocator
    {
        //Histogram comparison options:
        public static HISTOGRAM_COMP_METHOD comparisonMethod = HISTOGRAM_COMP_METHOD.CV_COMP_INTERSECT;
        public static int[] histBins = new int[] { 16, 16 };
        public static RangeF[] histRanges = new RangeF[] { new RangeF(0, 180), new RangeF(0, 255) };

        private Image<Bgr, byte> tableImage;
        private Image<Bgr, byte> tableImageSmall;
        private Image<Hsv, byte> _tableImageHsv;
        private Image<Gray, byte>[] _tablePlanes;
        public Image<Gray, byte> _tableMatchMask;
        public Image<Gray, byte> initialMatchMask;

        private static double resizeFactor = .4;

        public double locateThreshold = 100;
        public static int ballDia = 26;
        private int ballDiaResized;

        public static TM_TYPE templateMatchType = TM_TYPE.CV_TM_SQDIFF;

        public int numberOfBalls;


        public float bestMatch = 0;

        public Image<Gray, byte> backProjectShow;

        public BallLocator(Image<Bgr, byte> tableImage, int numberOfBalls = 16, Image<Gray, byte> mask = null)
        {
            ballDiaResized = (int)((double)ballDia * resizeFactor);

            _tableMatchMask = ImageUtil.thresholdAdaptiveMax(tableImage.Convert<Hsv, byte>().Split()[0], 1);

            if (mask != null)
            {
                mask = mask.Erode(50);

                _tableMatchMask = _tableMatchMask.And(mask);
            }

            this.numberOfBalls = numberOfBalls;
            this.tableImage = tableImage;
            tableImageSmall = tableImage.Resize(resizeFactor, INTER.CV_INTER_CUBIC);
            _tableMatchMask = _tableMatchMask.Resize(resizeFactor, INTER.CV_INTER_CUBIC);

            _tableImageHsv = tableImageSmall.Convert<Hsv, byte>();
            _tablePlanes = _tableImageHsv.Split();


            //_tableMatchMask = _tableMatchMask.Erode(1);
            initialMatchMask = _tableMatchMask.Copy();
        }

        private Image<Bgr,byte> getBallPatch(Point position)
        {
            position.X -= ballDia / 2;
            position.Y -= ballDia / 2;

            tableImage.ROI = new Rectangle(position, new Size(ballDia, ballDia));
            Image<Bgr, byte> patch = tableImage.Copy();
            tableImage.ROI = Rectangle.Empty;

            return patch;
        }

        public List<Ball> findBalls(int numberOfBalls)
        {
            Stopwatch sw = Stopwatch.StartNew();

            Image<Gray, float> projection = adaptiveBallDetect(tableImageSmall);
            backProjectShow = new Image<Gray, byte>(projection.Size);

            Util.writeWatch(sw, "Projtime");

            bool matchHigh = ImageUtil.matchHigh(comparisonMethod);
            List<Ball> returnList = new List<Ball>();
            Point coordDiff = new Point(_tablePlanes[0].Size - projection.Size);

            _tableMatchMask.ROI = new Rectangle(coordDiff.X / 2, coordDiff.Y / 2, projection.Width, projection.Height);

            sw = Stopwatch.StartNew();
            for (int i = 0; i < numberOfBalls; i++)
            {

                KeyValuePair<Point, float> extremum = ImageUtil.findExtremum(projection, _tableMatchMask, !ImageUtil.matchHigh(templateMatchType));
                if (extremum.Value < locateThreshold)
                {
                    break;
                }
                //Debug.Write("Ball " + i + ": " + extremum.Value);

                Point ballInTableCoords = new Point(extremum.Key.X + coordDiff.X / 2, extremum.Key.Y + coordDiff.Y / 2);

                ballInTableCoords.X = (int)((double)ballInTableCoords.X / resizeFactor);
                ballInTableCoords.Y = (int)((double)ballInTableCoords.Y / resizeFactor);

                returnList.Add(new Ball(getBallPatch(ballInTableCoords), ballInTableCoords));

                _tableMatchMask.Draw(new CircleF(extremum.Key, ballDiaResized), new Gray(0), -1);
            }
            Util.writeWatch(sw, "BallLocate");

            return returnList;
        }

        /// <summary>
        ///     Detects where balls are located in an image of the pooltable. The balls locations can then be extracted by finding maximal points of the returned image.
        /// </summary>
        /// <param name="input">An image containing pool balls on the cloth</param>
        /// <returns>Image where large values are more likely to be balls</returns>
        public Image<Gray, float> adaptiveBallDetect(Image<Bgr, byte> input)
        {
            int h_split = 1;
            int v_split = 1;
            int h_stepsize = input.Width / h_split;
            int v_stepsize = input.Height / v_split;

            Image<Bgr, byte> template = new Image<Bgr, byte>(ballDiaResized - 2, ballDiaResized - 2, new Bgr(0, 0, 0));
            Image<Gray, float> img_out = new Image<Gray, float>(input.Size.Width - template.Width + 1, input.Size.Height - template.Height + 1);
            Image<Gray, byte>[] inputPlanes = input.Split();

            for (int h = 0; h < h_split; h++)
            {
                for (int v = 0; v < v_split; v++)
                {
                    Rectangle ROI = new Rectangle(new Point(h_stepsize * h, v_stepsize * v), new Size(h_stepsize, v_stepsize));
                    input.ROI = ROI;
                    img_out.ROI = ROI;

                    RangeF histRange = new RangeF(0, 255);
                    DenseHistogram hist = new DenseHistogram(new int[] { 255, 255, 255 }, new RangeF[] { histRange, histRange, histRange });
                    hist.Calculate(inputPlanes, false, null);

                    float maxValue = 0;
                    float minValue = 0;
                    int[] maxLocation = { 0 };
                    int[] minLocation = { 0 };
                    hist.MinMax(out minValue, out maxValue, out minLocation, out maxLocation);

                    Bgr bgColor = new Bgr(maxLocation[0], maxLocation[1], maxLocation[2]);

                    ROI.Width += ballDiaResized + 1;
                    ROI.Height += ballDiaResized + 1;
                    input.ROI = ROI;

                    template = new Image<Bgr, byte>(template.Width, template.Height, bgColor);

                    Image<Gray,Byte>[] templatePlanes = template.Split();

                    Image<Gray, float> match = ImageUtil.matchTemplateMasked(inputPlanes, templatePlanes, (Image<Gray, byte>)null, _tableMatchMask);

                    match.CopyTo(img_out);
                }
            }
            img_out.ROI = Rectangle.Empty;
            input.ROI = Rectangle.Empty;

            return img_out;
        }
    }
}
