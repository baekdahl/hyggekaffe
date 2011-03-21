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
    
    public partial class Form1 : Form
    {
        Point[] positions = new Point[0];

        public Form1()
        {
            InitializeComponent();
            /*
            DirectoryInfo di = new DirectoryInfo("c://hyggekaffe//pics//rand");
            FileInfo[] rgFiles = di.GetFiles("*.jpg");
            foreach (FileInfo fi in rgFiles)
            {
                 countourfind(fi.FullName, fi.Name);
            }
            */

        }

        private int countourfind(string filefull, string filename)
        {
            Image<Bgr, Byte> img = new Image<Bgr, Byte>(filefull);
            img = img.Resize(img.Width / 3, img.Height / 3, INTER.CV_INTER_AREA);

            Image<Bgr,Byte> imgwouback = removebackround(img);

            Image<Gray, Byte> imgthres = imgwouback.Convert<Gray, Byte>().SmoothGaussian(5, 5, 2, 2);

            List<Contour<Point>> contour_array = new List<Contour<Point>>();

            for (Contour<Point> contour = imgthres.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_NONE, RETR_TYPE.CV_RETR_EXTERNAL); contour != null; contour = contour.HNext)
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

                imgthres.Draw(currentContour, new Gray(150), 2);
                Point[] pts = currentContour.ToArray();

                if (currentContour.Total == 4)
                {
                    LineSegment2D[] edges = PointCollection.PolyLine(pts, true);
                    bool isRectangle = true;
                    
                    for (int i = 0; i < edges.Length; i++)
                    {
                        double angle = Math.Abs(edges[(i + 1) % edges.Length].GetExteriorAngleDegree(edges[i]));
                        
                        if (angle < 80 || angle > 100)
                        {
                            isRectangle = false;
                            break;
                        }
                    }

                    if (isRectangle)
                    {

                        if (currentContour.Area > (img.Height*img.Width)/3) {
                        
                            LineSegment2D norm_line = new LineSegment2D(new Point(0, 5), new Point(img.Width, 5));
                            imgthres.Draw(norm_line, new Gray(255), 2);

                            double min_angle = 360;

                            foreach(LineSegment2D edge in edges) {
                                double rot_angle = edge.GetExteriorAngleDegree(norm_line);
                                if (Math.Abs(rot_angle) < Math.Abs(min_angle)) { min_angle = rot_angle; }
                            }
                            
                            double x = currentContour.BoundingRectangle.X;
                            double y = currentContour.BoundingRectangle.Y;

                            double newx = x * Math.Cos(min_angle / (180 * Math.PI)) - y * Math.Sin(min_angle / (180 * Math.PI));
                            double newy = y * Math.Cos(min_angle / (180 * Math.PI)) + x * Math.Sin(min_angle / (180 * Math.PI));

                            int padRail = (int)Math.Round(img.Width * 0.025);
                            int padSize = (int)Math.Round(img.Width * 0.05);
                            

                            Rectangle rect = new Rectangle((int)newx - padRail, (int)newy - padRail,
                                                 currentContour.BoundingRectangle.Width + padSize, currentContour.BoundingRectangle.Height + padSize);

                            imgthres = imgthres.Rotate(min_angle, new Gray(0));
                            img = img.Rotate(min_angle, new Bgr(255, 255, 255));

                            img.ROI = rect;
                            
                            Image<Bgr, Byte> imgsave = img.Clone();

                            imgsave.ToBitmap().Save("c:\\hyggekaffe\\pics\\rand2\\" + filename);
                            pictureBox1.Image = imgthres.ToBitmap();
                        }
                        return 0;
                       
                    }

                }

            }
            return 0;
        }

        private void countourfind(Image<Bgr,Byte> img)
        {
            img = img.Resize(img.Width / 3, img.Height / 3, INTER.CV_INTER_AREA);

            Image<Bgr, Byte> imgwouback = removebackround(img);

            Image<Gray, Byte> imgthres = imgwouback.Convert<Gray, Byte>().SmoothGaussian(5, 5, 2, 2);

            List<Contour<Point>> contour_array = new List<Contour<Point>>();

            for (Contour<Point> contour = imgthres.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_NONE, RETR_TYPE.CV_RETR_EXTERNAL); contour != null; contour = contour.HNext)
            {
                Contour<Point> currentContour = contour.ApproxPoly(contour.Perimeter * 0.05);
                contour_array.Add(currentContour);
            }


            contour_array.Sort(delegate(Contour<Point> c1, Contour<Point> c2)
            {
                return c1.Area.CompareTo(c2.Area);
            });
            contour_array.Reverse();

            foreach (Contour<Point> currentContour in contour_array)
            {

                imgthres.Draw(currentContour, new Gray(150), 2);
                Point[] pts = currentContour.ToArray();

                if (currentContour.Total == 4)
                {
                    LineSegment2D[] edges = PointCollection.PolyLine(pts, true);
                    bool isRectangle = true;

                    for (int i = 0; i < edges.Length; i++)
                    {
                        double angle = Math.Abs(edges[(i + 1) % edges.Length].GetExteriorAngleDegree(edges[i]));

                        if (angle < 80 || angle > 100)
                        {
                            isRectangle = false;
                            break;
                        }
                    }

                    if (isRectangle)
                    {

                        if (currentContour.Area > (img.Height * img.Width) / 3)
                        {

                            LineSegment2D norm_line = new LineSegment2D(new Point(0, 5), new Point(img.Width, 5));
                            imgthres.Draw(norm_line, new Gray(255), 2);

                            double min_angle = 360;

                            foreach (LineSegment2D edge in edges)
                            {
                                double rot_angle = edge.GetExteriorAngleDegree(norm_line);
                                if (Math.Abs(rot_angle) < Math.Abs(min_angle)) { min_angle = rot_angle; }
                            }

                            double x = currentContour.BoundingRectangle.X;
                            double y = currentContour.BoundingRectangle.Y;

                            double newx = x * Math.Cos(min_angle / (180 * Math.PI)) - y * Math.Sin(min_angle / (180 * Math.PI));
                            double newy = y * Math.Cos(min_angle / (180 * Math.PI)) + x * Math.Sin(min_angle / (180 * Math.PI));

                            int padRail = (int)Math.Round(img.Width * 0.025);
                            int padSize = (int)Math.Round(img.Width * 0.05);


                            Rectangle rect = new Rectangle((int)newx - padRail, (int)newy - padRail,
                                                 currentContour.BoundingRectangle.Width + padSize, currentContour.BoundingRectangle.Height + padSize);

                            imgthres = imgthres.Rotate(min_angle, new Gray(0));
                            img = img.Rotate(min_angle, new Bgr(255, 255, 255));

                            img.ROI = rect;

                            Image<Bgr, Byte> imgsave = img.Clone();
                            
                            pictureBox1.Image = img.ToBitmap();
                        }

                    }

                }

            }
        }

        private Image<Bgr,Byte> removebackround(Image<Bgr,Byte> img)
        {
            Imgproc imgproc = new Imgproc();

            Point[] bgpos = imgproc.findbgpoints(img, 5);
            Image<Bgr, Byte> imgnew = imgproc.removebackground(img, bgpos);
            imgproc.testtest(bgpos);

            pictureBox1.Image = imgnew.ToBitmap();

            return imgnew;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Capture capture = new Capture(); //create a camera captue
            Image<Bgr, Byte> img = capture.QueryFrame();
            countourfind(img);
        }

    }    
}

