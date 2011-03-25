using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;

using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.VideoSurveillance;


namespace Calib
{
    class Contour
    {
        private Contour(Image<Bgr,Byte> image)
        {
            ContourFind(image);
        }

        public List<Contour<Point>> ContourFind(Image<Bgr,Byte> image)
        {
            Image<Gray,Byte> gray_image = image.Convert<Gray, Byte>();  //Convert to Gray for thresholding

            CvInvoke.cvAdaptiveThreshold(gray_image.Ptr, gray_image.Ptr, //Adptive threshold
                    255, Emgu.CV.CvEnum.ADAPTIVE_THRESHOLD_TYPE.CV_ADAPTIVE_THRESH_MEAN_C, Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY, 301, 10);

            gray_image = gray_image.Dilate(2);    //Dialate
            gray_image = gray_image.Erode(2);     //Erode

            List<Contour<Point>> contourpoint = new List<Contour<Point>>();

            for (Contour<Point> contour = gray_image.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_NONE, RETR_TYPE.CV_RETR_CCOMP); 
                                                                  contour != null; contour = contour.HNext)
            {
                Contour<Point> currentContour = contour.ApproxPoly(contour.Perimeter * 0.05);

                int size = currentContour.BoundingRectangle.Height * currentContour.BoundingRectangle.Width;

                if (currentContour.Convex)
                {
                    contourpoint.Add(currentContour);

                }
            }

            contourpoint.Sort(delegate(Contour<Point> c1, Contour<Point> c2)
            {
                return c1.Area.CompareTo(c2.Area);
            });

            double medianArea = contourpoint[(int)Math.Floor((double)contourpoint.Count / 2)].Area; //Find median size contours

            foreach (Contour<Point> contour in contourpoint)    //Draw contours
            {
                if (Math.Abs(contour.Area - medianArea) > 50)
                {
                    contourpoint.Remove(contour);
                }
            }
            return contourpoint;
        }

    }
}
