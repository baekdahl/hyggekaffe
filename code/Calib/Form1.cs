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
    public struct iPoint
    {
        public int X, Y, value;
    }

    public struct calibImage
    {
        public Image<Bgr, Byte> org_img;
        public Image<Bgr, Byte> wob_img;
        public Image<Bgr, Byte> cropped_img;
        public Image<Gray, Byte> thresholded;
        public Image<Gray, Byte> threshold_diamond;
        public List<Contour<Point>> contourpoint;
        public Rectangle rect;
        public double rot_angle;
    }

    public partial class Form1 : Form
    {
        Point[] positions = new Point[0];

        public Form1()
        {
            InitializeComponent();
            
            DirectoryInfo di = new DirectoryInfo("c://hyggekaffe//pics//randx");
            FileInfo[] rgFiles = di.GetFiles("*.jpg");
            foreach (FileInfo fi in rgFiles)
            {
                calibImage imageStruct = new calibImage();

                imageStruct.org_img = new Image<Bgr, Byte>(fi.FullName);
                imageStruct.org_img = imageStruct.org_img.Resize(0.5, INTER.CV_INTER_NN);
                imageStruct.org_img = removebackground(imageStruct.org_img);

                imageStruct.threshold_diamond = imageStruct.org_img.Convert<Gray, Byte>();  //Convert to Gray for thresholding

                CvInvoke.cvAdaptiveThreshold(imageStruct.threshold_diamond.Ptr, imageStruct.threshold_diamond.Ptr, //Adptive threshold
                        255, Emgu.CV.CvEnum.ADAPTIVE_THRESHOLD_TYPE.CV_ADAPTIVE_THRESH_MEAN_C, Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY, 201, 10);

                imageStruct.threshold_diamond = imageStruct.threshold_diamond.Dilate(1);    //Dialate
                imageStruct.threshold_diamond = imageStruct.threshold_diamond.Erode(1);     //Erode
             
                imageStruct.contourpoint = contourfind2(imageStruct.threshold_diamond);     //Find contours in image

                double medianArea = imageStruct.contourpoint[(int)Math.Floor((double)imageStruct.contourpoint.Count / 2)].Area; //Find median size contours

                foreach (Contour<Point> contour in imageStruct.contourpoint)    //Draw contours
                {
                    if (Math.Abs(contour.Area - medianArea) < 5)
                    {
                        Debug.Write("\n"+contour.Area+"\n");
                        imageStruct.org_img.Draw(contour, new Bgr(0, 255, 255), 3);
                        Debug.Write(contour.Area / Math.Pow(contour.Perimeter, 2));
                    }
                }

                imageStruct.org_img.Save("c://hyggekaffe//pics//rand2//" + fi.Name);    //Save ouput image

                pictureBox2.Image = imageStruct.threshold_diamond.ToBitmap();
                pictureBox1.Image = imageStruct.org_img.ToBitmap();
                
            }
        }

        private List<Contour<Point>> contourfind2(Image<Gray,Byte> img) {

            List<Contour<Point>> contourpoint = new List<Contour<Point>>();

            for (Contour<Point> contour = img.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_NONE, RETR_TYPE.CV_RETR_CCOMP); contour != null; contour = contour.HNext)
            {
                Contour<Point> currentContour = contour.ApproxPoly(contour.Perimeter * 0.05);

                int size = currentContour.BoundingRectangle.Height*currentContour.BoundingRectangle.Width;

                if (currentContour.Convex) 
                {
                    contourpoint.Add(currentContour);
                    
                }
                Debug.Write("\n"+contourpoint.Count+"\n");
            }

            contourpoint.Sort(delegate(Contour<Point> c1, Contour<Point> c2)
            {
                return c1.Area.CompareTo(c2.Area);
            });


            return contourpoint;
        }

        private calibImage countourfind(calibImage imageStruct)
        {
            List<Contour<Point>> contour_array = new List<Contour<Point>>();

            for (Contour<Point> contour = imageStruct.thresholded.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_EXTERNAL); contour != null; contour = contour.HNext)
            {
                Contour<Point> currentContour = contour.ApproxPoly(contour.Perimeter * 0.05);
                contour_array.Add(currentContour);
            }
            
            contour_array.Sort(delegate(Contour<Point> c1, Contour<Point> c2)
            {
                return c1.Area.CompareTo(c2.Area);
            });
            contour_array.Reverse();

            foreach(Contour<Point> currentContour in contour_array) {

                Point[] pts = currentContour.ToArray();

                if (currentContour.Total == 4)
                {
                    LineSegment2D[] edges = PointCollection.PolyLine(pts, true);
                    bool isRectangle = true;
                    
                    for (int i = 0; i < edges.Length; i++)
                    {
                        double angle = Math.Abs(edges[(i + 1) % edges.Length].GetExteriorAngleDegree(edges[i]));
                        
                        if (angle < 85 || angle > 95)
                        {
                            isRectangle = false;
                            break;
                        }
                    }

                    if (isRectangle)
                    {

                        if (currentContour.Area > (imageStruct.thresholded.Height * imageStruct.thresholded.Width) / 3)
                        {
                                double x = currentContour.BoundingRectangle.X;
                                double y = currentContour.BoundingRectangle.Y;

                                int padRail = (int)Math.Round(imageStruct.thresholded.Width * 0.1);
                                int padSize = (int)Math.Round(imageStruct.thresholded.Width * 0.15);
                                
                                Rectangle rect = new Rectangle((int)x - padRail, (int)y - padRail,
                                                       currentContour.BoundingRectangle.Width + padSize, currentContour.BoundingRectangle.Height + padSize);

                                imageStruct.rect = rect;
                        }
                        return imageStruct;
                    }
                }
            }
            return imageStruct;
        }

        private Image<Bgr,Byte> removebackground_thres(Image<Bgr,Byte> img)
        {
            Imgproc imgproc = new Imgproc();

            Point[] bgpos = imgproc.findbgpoints(img, 5, 255);
            Image<Bgr, Byte> imgnew = imgproc.removebackground_thres(img, bgpos);
            return imgnew;

        }

        private Image<Bgr, Byte> removebackground(Image<Bgr, Byte> img)
        {
            Imgproc imgproc = new Imgproc();

            Point[] bgpos = imgproc.findbgpoints(img, 5, 255);
            Image<Bgr, Byte> imgnew = imgproc.removebackground(img, bgpos);
            return imgnew;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Capture capture = new Capture(); //create a camera captue
            Image<Bgr, Byte> img = capture.QueryFrame();
        }

    }    
}

