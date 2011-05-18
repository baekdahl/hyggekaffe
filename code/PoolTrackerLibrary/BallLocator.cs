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

        public static int ballDia = 26;                 //Both definded here and in Ball.cs
        public static byte backgroundThreshold;
        private int ballDiaResized;

        public static TM_TYPE templateMatchType = TM_TYPE.CV_TM_SQDIFF;

        public int numberOfBalls;

        public float bestMatch = 0;

        public Image<Gray, byte> backProjectShow;
        private BallCalibration calibration;

        private int totalPixels;


        private int LocateThreshold {
         get {
             
             return (int)((float)totalPixels * calibration.ballFactor);
         }
        }
            
            


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

            Image<Gray, byte> patchMask = Ball.getMask().Resize(resizeFactor, INTER.CV_INTER_CUBIC);
            totalPixels = patchMask.CountNonzero()[0];

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

            Image<Gray, byte> patchMask = Ball.getMask().Resize(resizeFactor, INTER.CV_INTER_CUBIC);
            int totalPixels = patchMask.CountNonzero()[0];

            Rectangle roi = new Rectangle(0, 0, patchMask.Width, patchMask.Height);

            byte backgroundLowThreshold = (byte) (backgroundThreshold - 12);
            byte backgroundHighThreshold = (byte) (backgroundThreshold + 12);

            int[] maxScore = new int[16];
            Point[] maxPosition = new Point[16];

            LinkedList<int[]> candidates = new LinkedList<int[]>(); 

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
                                    else if (value < calibration.valBlack)
                                    {
                                        blackPixels++;
                                    }
                                    else if (saturation < calibration.satWhite)
                                    {
                                        whitePixels++;
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

                        int score = calculateProbabilty(hueHist, satHist, whitePixels, blackPixels); 

                        if (score > LocateThreshold)
                        {
                            int[] newNode = new int[] { score, x, y };
                            LinkedListNode<int[]> node = candidates.First;
                            while (node != null && score < node.Value[0]) //Search list until we find a ball with smaller score or we reach the end of list
                            {
                                node = node.Next;
                            }
                            
                            if (node != null)
                            {
                                candidates.AddBefore(node, newNode);
                            }
                            else //We reached the end of the list. Add the ball there
                            {
                                candidates.AddLast(newNode);
                            }
                        }
                    }
                }
            }

            List<Ball> list = new List<Ball>();

            for (LinkedListNode<int[]> node = candidates.First; node != null; node = node.Next)
            {
                Point pos = new Point(node.Value[1], node.Value[2]);
                if (_tableMatchMask.Data[pos.Y, pos.X, 0] == 1)
                {
                    Ball ball = new Ball(pos, BallColor.None);
                    ball.position.X = (int)(ball.position.X / resizeFactor);
                    ball.position.Y = (int)(ball.position.Y / resizeFactor);
                    ball.imageFromTable(tableImage);

                    ball.findColor();

                    
                    list.Add(ball);
                    if (list.Count == 16)
                    {
                        break;
                    }
                    _tableMatchMask.Draw(new CircleF(pos, ballDiaResized), new Gray(0), -1);
                }
            }

            return list;
        }

        public int calculateProbabilty(int[] hueHist, int[] satHist, int whitePixels, int blackPixels)
        {
            int maxHueDist = 30;
            int hueMean = Util.getMaxIndex(hueHist);
            int satMean = Util.getMean(satHist);

            int start = hueMean - maxHueDist;
            if (start < 1) start += 179;

            int stop = hueMean + maxHueDist;
            if (stop > 179) stop -= 179;

            int score = 0;
            for (int i = start; i != stop; i++)
            {
                if (i == 180) i = 0;

                score += hueHist[i];
            }
            /*
            if (whitePixels > WhiteThreshold)
            {
               return whitePixels;
            }
            else if (blackPixels > BallThreshold)
            {
                return blackPixels;
            }
            else if (score + whitePixels > BallThreshold)
            {
            */

            if (blackPixels > score)
            {
                return blackPixels + whitePixels;
            }
            else
            {
                return score + whitePixels;// + blackPixels;
            }

            
            /*}
           
           else
           {
               return 0;
           }*/
        }
    }
}
