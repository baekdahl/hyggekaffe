using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;

using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.VideoSurveillance;


namespace PoolTrackerLibrary
{
    public class TableLocator
    {
        public Rectangle ROI;
        public double angle;
        public Image<Gray, Byte> mask;
        public double maskarea;
        public int histMaxValue;
        public double maskperimeter;

        public TableLocator()
        {
            //Config.load(this);
        }

        public TableLocator(Image<Bgr,Byte> input_image)
        {
            Image<Bgr, Byte> image = input_image.Copy();

            Image<Gray, Byte> hue = ImageUtil.bgrToHue(image);                              //Convert image to HSV
            DenseHistogram hist = ImageUtil.makeHist(hue);                                  //Make histogram of the hue
            histMaxValue = ImageUtil.histMaxValue(hist);

            Image<Gray, Byte> imageClothID = ImageUtil.twoSidedThreshold(hue, histMaxValue);//Set pixels close to the maximum value to 255, otherwise 0.
            imageClothID = median(imageClothID, 3);                                        //Run median filter
            MCvBox2D clothBox = findClothBox(imageClothID);                                 //Find bounding box of cloth
            angle = findTableAngle(clothBox);                                               //Find angle of cloth (and table).

            Image<Bgr, Byte> rotimage = image.Rotate(angle, new Bgr(255, 255, 255));        //Rotate image and create rotimage
            imageClothID = imageClothID.Rotate(angle, new Gray(255));                       //Rotate image with cloth identified.
            MCvBox2D clothBox2 = findClothBox(imageClothID);                                //Find rotated box
            ROI = clothBoxToROI(clothBox2);                                                 //Convert box to rectangle ROI

        }

        public Image<Bgr, Byte> getTableImage(Image<Bgr, Byte> input_image)
        {
            Image<Bgr, Byte> returnImage = input_image.Copy();
            returnImage = input_image.Rotate(angle, new Bgr(255, 255, 255));
            
            if (mask != null & ROI.Height < input_image.Height & ROI.Width < input_image.Width)
            {
                mask.ROI = ROI;
                returnImage = input_image.Rotate(angle, new Bgr(255, 255, 255)).Copy(ROI);
            }

            return returnImage;
        }

        public bool isTableOccluded(Image<Bgr, Byte> input_image, double area_threshold=0.99, double perimeter_threshold=1.04)
        {
            bool occluded = false;
            double[] currentMask = findBiggestContour(input_image);


            if (currentMask[0] / maskarea <= area_threshold | currentMask[1] / maskperimeter > perimeter_threshold)
            {
                occluded = true;
            }
            
            return occluded;
        }

        private static Image<Gray, Byte> median(Image<Gray, Byte> image, int kernel)
        {
            return image = image.SmoothMedian(kernel);
        }

        private MCvBox2D findClothBox(Image<Gray, Byte> image)
        {
            MCvBox2D clothBox = new MCvBox2D();

            for (Contour<Point> contour = image.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_LIST); contour != null; contour = contour.HNext)  //Iterate through contours
            {
                Contour<Point> currentContour = contour.ApproxPoly(contour.Perimeter * 0.001);                                           //Approximate a polygon with certain precision.

                if (currentContour.Area > (image.Height * image.Width) / 2 && currentContour.Area < (image.Height * image.Width) / 1.01)  //Image.Size/2 < Contour < Image.Size/1.1
                {
                    if (maskperimeter==0)
                    {
                        maskperimeter = contour.Perimeter;
                        maskarea = contour.Area;
                    }

                    mask = image.CopyBlank();
                    mask.Draw(contour, new Gray(255), -1);
                    clothBox = currentContour.GetMinAreaRect();                                                                          //Get minimum rectangle that fits on contour.
                }
            }

            return clothBox;
        }

        private double[] findBiggestContour(Image<Bgr, Byte> image)
        {
            double returnArea=0;
            double returnPerimeter=0;

            Image<Gray, Byte> img = ImageUtil.bgrToHue(image).Resize(1, INTER.CV_INTER_AREA);
            img = ImageUtil.twoSidedThreshold(img, histMaxValue);
            img = median(img, 3);

            for (Contour<Point> contour = img.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_LIST); contour != null; contour = contour.HNext)  //Iterate through contours
            {

                if (contour.Area > img.Width * img.Height * 0.5)
                {
                    returnArea = contour.Area;
                    returnPerimeter = contour.Perimeter;
                    Image<Gray, Byte> imgtemp = img.CopyBlank();
                    imgtemp.Draw(contour, new Gray(255), -1);
                }
            }

            return new double[] {returnArea, returnPerimeter};
        }

        private static double findTableAngle(MCvBox2D box)
        {
            PointF[] pts = box.GetVertices();
            List<LineSegment2D> linelist = new List<LineSegment2D>();

            linelist.Add(new LineSegment2D(new Point((int)pts[0].X, (int)pts[0].Y), new Point((int)pts[1].X, (int)pts[1].Y)));
            linelist.Add(new LineSegment2D(new Point((int)pts[1].X, (int)pts[1].Y), new Point((int)pts[2].X, (int)pts[2].Y)));
            linelist.Add(new LineSegment2D(new Point((int)pts[2].X, (int)pts[2].Y), new Point((int)pts[3].X, (int)pts[3].Y)));
            linelist.Add(new LineSegment2D(new Point((int)pts[3].X, (int)pts[3].Y), new Point((int)pts[0].X, (int)pts[0].Y)));

            linelist.Sort(delegate(LineSegment2D l1, LineSegment2D l2)
            {
                return l1.Length.CompareTo(l2.Length);
            });

            LineSegment2D horizontalLine = new LineSegment2D(new Point(0, 0), new Point(100, 0));

            double angle = Math.Abs(horizontalLine.GetExteriorAngleDegree(linelist[3]));

            if (Double.IsNaN(angle)) { angle = 0; }
            if (angle > 90) { angle = 180 - angle; }

            return angle;

        }

        private static Rectangle clothBoxToROI(MCvBox2D box)
        {
            Rectangle ROI = box.MinAreaRect();
            return ROI;
        }
        
    }
}
