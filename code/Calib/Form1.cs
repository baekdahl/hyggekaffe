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
        Image<Bgr, Byte> img1;
        Image<Bgr, Byte> img2;
        Point[] positions = new Point[0];

        public Form1()
        {
            InitializeComponent();
            findblobtest();
            //templatetest();
        }

        private void findblobtest()
        {
            Image<Gray, Byte> img = new Image<Gray, Byte>("C:\\hyggekaffe\\pool\\new\\Picture 2.jpg");
            img = img.Resize(img.Width / 2, img.Height / 2, INTER.CV_INTER_AREA, false);
    
            pictureBox1.Image = img.ToBitmap();
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;

            Imgproc imgproc = new Imgproc();
            iPoint[] maximas = imgproc.findmaxima(img);

            iPoint[] sortedlist = imgproc.sortiPoints(maximas,100);
            iPoint[] newsortedlist = imgproc.removecloseiPoints(sortedlist, 50);

            foreach (iPoint points in sortedlist)
            {
                //img.Draw(new CircleF(new Point(points.X, points.Y), 5), new Gray(0), 2);
            }
            
            foreach (iPoint points in newsortedlist)
            {
                img.Draw(new CircleF(new Point(points.X, points.Y), 5), new Gray(255), 2);                
            }

            pictureBox1.Image = img.ToBitmap();

            
        }


        private void templatetest()
        {
            Image<Gray, Byte> template = new Image<Gray, Byte>("C:\\hyggekaffe\\pool\\new\\ball.jpg");
            Image<Gray, Byte> table = new Image<Gray, Byte>("C:\\hyggekaffe\\pool\\new\\pooltable2.jpg");

            Image<Gray, float> match = table.MatchTemplate(template, TM_TYPE.CV_TM_CCOEFF);

            pictureBox2.Image = table.ToBitmap();
            pictureBox1.Image = match.ToBitmap();


        }

        private void button1_Click(object sender, EventArgs e)
        {
           
                img1 = new Capture().QueryFrame();

                int height = img1.Height;
                int width = img1.Width;

                img1 = img1.Resize(width/2, height/2, INTER.CV_INTER_AREA, false);
                pictureBox1.Image = img1.ToBitmap();
                pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            /*
                pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
                pictureBox2.SizeMode = PictureBoxSizeMode.AutoSize;

                Imgproc imgproc = new Imgproc();
                positions = imgproc.findbgpoints(imgcalib);
                int[] minmaxpositions = imgproc.findpositions(positions);
                imgcalib = imgproc.removebackground(imgcalib, positions);

                imgcalib.ROI = new Rectangle(minmaxpositions[0], minmaxpositions[1], minmaxpositions[2] - minmaxpositions[0], minmaxpositions[3] - minmaxpositions[1]);
                Image<Bgr, Byte> cropimg = imgcalib.Copy();
            */    
           // }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            img2 = new Capture().QueryFrame();
            
            int height = img2.Height;
            int width = img2.Width;
            
            img2 = img2.Resize(width / 2, height / 2, INTER.CV_INTER_AREA, false);
            
            Imgproc imgproc = new Imgproc();
            Image<Bgr, Byte> imgsub = imgproc.subtractimages(img2, img1);
            pictureBox2.Image = img2.ToBitmap();
            pictureBox2.SizeMode = PictureBoxSizeMode.AutoSize;

        }

    }    
}

