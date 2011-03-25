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
using vflibcs;

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
        Image<Bgr, Byte> camera_image;
        Rectangle imgRect = new Rectangle(new Point(38, 168), new Size(541, 278));

        public Form1()
        {
            InitializeComponent();
            generatePoolGraph();
            /*
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
                
            }*/
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
            test();
        }

        private void test()
        {
            Capture capture = new Capture(); //create a camera capture
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, 960/2);
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, 1280/2);
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS, 100);


            Application.Idle += new EventHandler(delegate(object sender, EventArgs e)
            {  //run this until application closed (close button click on image viewer)
                Image<Bgr, Byte> img = capture.QueryFrame(); //draw the image obtained from camera
                camera_image = img.Copy(imgRect);
                //pictureBox1.Image = img.Resize(0.5, INTER.CV_INTER_NN).ToBitmap();
                test3();
            });
        }

        private void test2()
        {
            calibImage imageStruct = new calibImage();

            imageStruct.org_img = camera_image;
            //imageStruct.org_img = imageStruct.org_img.Resize(1, INTER.CV_INTER_NN);
            //imageStruct.org_img = removebackground(imageStruct.org_img);

            imageStruct.threshold_diamond = imageStruct.org_img.Convert<Gray, Byte>();  //Convert to Gray for thresholding

            CvInvoke.cvAdaptiveThreshold(imageStruct.threshold_diamond.Ptr, imageStruct.threshold_diamond.Ptr, //Adptive threshold
                    255, Emgu.CV.CvEnum.ADAPTIVE_THRESHOLD_TYPE.CV_ADAPTIVE_THRESH_MEAN_C, Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY, 301, 10);

            imageStruct.threshold_diamond = imageStruct.threshold_diamond.Dilate(2);    //Dialate
            imageStruct.threshold_diamond = imageStruct.threshold_diamond.Erode(2);     //Erode

            imageStruct.contourpoint = contourfind2(imageStruct.threshold_diamond);     //Find contours in image

            double medianArea = imageStruct.contourpoint[(int)Math.Floor((double)imageStruct.contourpoint.Count / 2)].Area; //Find median size contours

            foreach (Contour<Point> contour in imageStruct.contourpoint)    //Draw contours
            {
                if (Math.Abs(contour.Area - medianArea) < 50)
                {
                    Debug.Write("\n" + contour.Area + "\n");
                    imageStruct.org_img.Draw(contour, new Bgr(0, 255, 255), 3);
                    Debug.Write(contour.Area / Math.Pow(contour.Perimeter, 2));
                }
            }

            //pictureBox4.Image = 
            imageBox1.Image = imageStruct.threshold_diamond;
            imageBox2.Image = imageStruct.org_img;

        }

        private void test3() {

            Image<Gray,Byte>[] img_split = camera_image.Convert<Hsv,Byte>().Split();

            img_split[0] = img_split[0].Resize(1, INTER.CV_INTER_NN);

            int h_split = 10;
            int v_split = 5;
            int h_stepsize = img_split[0].Width / h_split;
            int v_stepsize = img_split[0].Height / v_split;

            Image<Gray,Byte> img_out = img_split[0].CopyBlank();

            for (int h = 0; h < h_split; h++)
            {
                for (int v = 0; v < v_split; v++)
                {
                    Rectangle ROI = new Rectangle(new Point(h_stepsize * h, v_stepsize * v), new Size(h_stepsize, v_stepsize));
                    img_split[0].ROI = ROI;

                    DenseHistogram hist = new DenseHistogram(255, new RangeF(0, 255));
                    hist.Calculate(new Image<Gray, Byte>[] { img_split[0] }, false, null);

                    float maxValue = 0;
                    float minValue = 0;
                    int[] maxLocation = { 0 };
                    int[] minLocation = { 0 };
                    hist.MinMax(out minValue, out maxValue, out minLocation, out maxLocation);

                    Image<Gray, Byte> thres_h11 = img_split[0].ThresholdBinary(new Gray(maxLocation[0] + 10), new Gray(255));
                    Image<Gray, Byte> thres_h22 = img_split[0].ThresholdBinaryInv(new Gray(maxLocation[0] - 10), new Gray(255));
                    
                    img_out.ROI = ROI;
                    thres_h11.Or(thres_h22).CopyTo(img_out);

                }
            }
            img_out.ROI = Rectangle.Empty;
            img_split[0].ROI = Rectangle.Empty;

            imageBox1.Image = img_split[0];
            imageBox2.Image = camera_image;
            imageBox3.Image = img_out;
            
            
        }
        
        private void generatePoolGraph()
        {
            Graph poolGraph = new Graph();
            poolGraph.InsertNode(new Checkable(2, 3));
            poolGraph.InsertNode(new Checkable(2, 3));
            poolGraph.InsertNode(new Checkable(2, 3));

            poolGraph.InsertEdge(0, 1);
            poolGraph.InsertEdge(1, 2);
            poolGraph.InsertEdge(2, 0);

            Graph foundGraph = new Graph();
            foundGraph.InsertNode(new Checkable(2, 3));
            foundGraph.InsertNode(new Checkable(2, 3));
            foundGraph.InsertNode(new Checkable(2, 3));

            foundGraph.InsertEdge(0, 1);
            foundGraph.InsertEdge(1, 2);
            foundGraph.InsertEdge(2, 0);

            VfState vfs = new VfState(poolGraph, foundGraph, false, true, true);
            bool fIsomorphic = vfs.FMatch();
            if (fIsomorphic)
            {
                foreach (FullMapping fm in vfs.Mappings)
                {
                    int[] mapping1to2 = fm.arinodMap1To2;
                    int[] mapping2to1 = fm.arinodMap2To1;
                }
            }
            
        }

    }    
}

