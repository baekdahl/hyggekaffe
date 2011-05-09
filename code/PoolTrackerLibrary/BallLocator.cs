using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

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

        private static double resizeFactor = 1;

        public double locateThreshold = 100;
        public static int ballDia = 26;
        public static byte backgroundThreshold;
        private int ballDiaResized;

        public static TM_TYPE templateMatchType = TM_TYPE.CV_TM_SQDIFF;

        public int numberOfBalls;

        public float bestMatch = 0;

        public Image<Gray, byte> backProjectShow;
        private BallCalibration calibration;



        private byte findClothHue()
        {
            DenseHistogram hist = ImageUtil.makeHist(tableImage.Convert<Hsv, byte>()[0]);
            return (byte) ImageUtil.histMaxValue(hist);
        }

        public BallLocator(Image<Bgr, byte> tableImage, BallCalibration calibration, Image<Gray, byte> mask = null)
        {
            ballDiaResized = (int)((double)ballDia * resizeFactor);
            _tableMatchMask = ImageUtil.thresholdAdaptiveMax(tableImage.Convert<Hsv, byte>().Split()[0], 1);

            if (mask != null)
            {
                mask = mask.Erode(30);

                _tableMatchMask = _tableMatchMask.And(mask);
            }

            this.calibration = calibration;
            this.tableImage = tableImage;
            tableImageSmall = tableImage.Resize(resizeFactor, INTER.CV_INTER_CUBIC);
            _tableMatchMask = _tableMatchMask.Resize(resizeFactor, INTER.CV_INTER_CUBIC);

            _tableImageHsv = tableImageSmall.Convert<Hsv, byte>();
            _tablePlanes = _tableImageHsv.Split();

            initialMatchMask = _tableMatchMask.Copy();

            backgroundThreshold = findClothHue();

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

        public List<Ball> idBalls()
        {
            Image<Gray, byte>[] planes = tableImageSmall.Convert<Hsv,byte>().Split();
            int imgWidth = planes[0].Width;
            int imgHeight = planes[0].Height;

            Image<Gray, byte> patchMask = Ball.getMask();
            int totalPixels = patchMask.CountNonzero()[0];

            Rectangle roi = new Rectangle(0, 0, patchMask.Width, patchMask.Height);

            byte backgroundLowThreshold = (byte) (backgroundThreshold - 10);
            byte backgroundHighThreshold = (byte) (backgroundThreshold + 10);

            int[] maxScore = new int[16];
            Point[] maxPosition = new Point[16];

            LinkedList<Ball> candidates = new LinkedList<Ball>(); 

            int patchHalf = (patchMask.Width - 1) / 2;

            for (int y = patchHalf; y < imgHeight - (patchHalf + 1); y++)
            {
                for (int x = patchHalf; x < imgWidth - (patchHalf + 1); x++)
                {
                    if (_tableMatchMask.Data[y, x, 0] == 1)
                    {
                        int whitePixels = 0, blackPixels = 0;
                        int[] pixels = new int[16];
                        int[] hueHist = new int[180], satHist = new int[256];

                        int maskY = 0;
                        for (int patchY = y - patchHalf; patchY < y + patchHalf; patchY++)
                        {
                            int maskX = 0;
                            for (int patchX = x - patchHalf; patchX < x + patchHalf; patchX++)
                            {
                                if (patchMask.Data[maskY, maskX, 0] == 1)
                                {
                                    byte hue = planes[0].Data[patchY, patchX, 0];
                                    byte saturation = planes[1].Data[patchY, patchX, 0];
                                    byte value = planes[2].Data[patchY, patchX, 0];

                                    if (hue > backgroundLowThreshold && hue < backgroundHighThreshold)
                                    {
                                        continue;
                                    }
                                    else if (saturation < calibration.satWhite)
                                    {
                                        whitePixels++;
                                    }
                                    else if (value < calibration.valBlack)
                                    {
                                        blackPixels++;
                                    }
                                    else
                                    {
                                        hueHist[hue]++;
                                        satHist[saturation]++;
                                    }
                                }
                                maskX++;
                            }
                            maskY++;
                        }

                        Ball ball = new Ball(new Point(x, y), whitePixels, blackPixels, hueHist, satHist);

                        if (ball.color != BallColor.None)
                        {
                            LinkedListNode<Ball> node = candidates.First;
                            while (node != null && ball.score < node.Value.score) //Search list until we find a ball with smaller score or we reach the end of list
                            {
                                node = node.Next;
                            }
                            
                            if (node != null)
                            {
                                candidates.AddBefore(node, ball);
                            }
                            else //We reached the end of the list. Add the ball there
                            {
                                candidates.AddLast(ball);
                            }
                        }
                    }
                }
            }

            List<Ball> list = new List<Ball>();

            for (LinkedListNode<Ball> node = candidates.First; node != null; node = node.Next)
            {
                Point pos = node.Value.position;
                if (_tableMatchMask.Data[pos.Y, pos.X, 0] == 1)
                {
                    list.Add(node.Value);
                    if (list.Count == 16)
                    {
                        break;
                    }
                    _tableMatchMask.Draw(new CircleF(pos, ballDiaResized), new Gray(0), -1);
                }
            }

            return list;
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
