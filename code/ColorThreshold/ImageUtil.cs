using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace PoolTracker
{
    class ImageUtil
    {
        public static KeyValuePair<Point, float> findExtremum(Image<Gray, float> image, Image<Gray, byte> mask = null, bool findMax = true)
        {
            KeyValuePair<Point, float> extreme = new KeyValuePair<Point,float>(new Point(-1,-1), findMax ? float.MinValue : float.MaxValue);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {  
                    if (mask.Data[y, x, 0] == 1)
                    {
                        float value = image.Data[y, x, 0];
                        if (findMax)
                        {
                            if (value > extreme.Value) extreme = new KeyValuePair<Point, float>(new Point(x, y), value);
                        }
                        else
                        {
                            if (value < extreme.Value) extreme = new KeyValuePair<Point, float>(new Point(x, y), value);
                        }
                    }
                }
            }
            return extreme;
        }

        public static bool matchHigh(HISTOGRAM_COMP_METHOD comparisonMethod)
        {
            return !(comparisonMethod == HISTOGRAM_COMP_METHOD.CV_COMP_CORREL || comparisonMethod == HISTOGRAM_COMP_METHOD.CV_COMP_INTERSECT);
        }

        public static bool matchHigh(TM_TYPE comparisonMethod)
        {
            return !(
                comparisonMethod == TM_TYPE.CV_TM_SQDIFF || 
                comparisonMethod == TM_TYPE.CV_TM_SQDIFF_NORMED);
        }

        public static Image<Gray, float> backProjectPatchMasked(DenseHistogram hist, Image<Gray, byte>[] planes, Image<Gray, byte> patchMask, HISTOGRAM_COMP_METHOD method, Image<Gray, byte> projectionMask)
        {
            int imgWidth = planes[0].Width;
            int imgHeight = planes[0].Height;

            Image<Gray, float> returnImg = new Image<Gray, float>(imgWidth - patchMask.Width + 1, imgHeight - patchMask.Height + 1, new Gray(matchHigh(method) ? 0 : 255));
            hist.Normalize(1.0);

            Rectangle roi = new Rectangle(0, 0, patchMask.Width, patchMask.Height);
            DenseHistogram model = new DenseHistogram(new int[] { 16, 16 }, hist.Ranges);

            for (int y = 0; y < returnImg.Height; y++)
            {
                for (int x = 0; x < returnImg.Width; x++)
                {
                    if (projectionMask == null || projectionMask.Data[y, x, 0] == 1)
                    {
                        roi.X = x;
                        roi.Y = y;
                        foreach (Image<Gray, byte> plane in planes)
                        {
                            plane.ROI = roi;
                        }

                        model.Calculate<byte>(planes, false, patchMask);
                        model.Normalize(1.0);

                        returnImg.Data[y, x, 0] = (float)CvInvoke.cvCompareHist(model.Ptr, hist.Ptr, method);
                    }
                }
            }
            //Get rid of table roi's
            foreach (Image<Gray, byte> plane in planes)
            {
                plane.ROI = Rectangle.Empty;
            }

            return returnImg;
        }
        
        public static Image<Gray,byte> thresholdAdaptiveMax(Image<Gray, byte> input, int thresholdMax = 255)
        {
            int h_split = 10;
            int v_split = 5;
            int h_stepsize = input.Width / h_split;
            int v_stepsize = input.Height / v_split;

            Image<Gray, Byte> img_out = input.CopyBlank();

            for (int h = 0; h < h_split; h++)
            {
                for (int v = 0; v < v_split; v++)
                {
                    Rectangle ROI = new Rectangle(new Point(h_stepsize * h, v_stepsize * v), new Size(h_stepsize, v_stepsize));
                    input.ROI = ROI;

                    DenseHistogram hist = new DenseHistogram(255, new RangeF(0, 255));
                    hist.Calculate(new Image<Gray, Byte>[] { input }, false, null);

                    float maxValue = 0;
                    float minValue = 0;
                    int[] maxLocation = { 0 };
                    int[] minLocation = { 0 };
                    hist.MinMax(out minValue, out maxValue, out minLocation, out maxLocation);

                    Image<Gray, Byte> thres_h11 = input.ThresholdBinary(new Gray(maxLocation[0] + 10), new Gray(thresholdMax));
                    Image<Gray, Byte> thres_h22 = input.ThresholdBinaryInv(new Gray(maxLocation[0] - 10), new Gray(thresholdMax));

                    img_out.ROI = ROI;
                    thres_h11.Or(thres_h22).CopyTo(img_out);

                }
            }
            img_out.ROI = Rectangle.Empty;
            input.ROI = Rectangle.Empty;

            return img_out;
        }
    }
}
