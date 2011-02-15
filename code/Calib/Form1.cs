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

    public partial class Form1 : Form
    {
        Image<Bgr, Byte> imgcalib;
        Point[] positions = new Point[0];

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fpath = openFileDialog1.FileName.ToString();

                imgcalib = new Image<Bgr, Byte>(fpath);
                Image<Bgr, Byte> imgcalib2 = new Image<Bgr, Byte>("C:\\Users\\Simon\\Desktop\\pool\\Picture 1wob.jpg");
               
                int height = imgcalib.Height;
                int width = imgcalib.Width;

                imgcalib = imgcalib.Resize(width/2, height/2, INTER.CV_INTER_AREA, false);
                imgcalib2 = imgcalib2.Resize(width / 2, height / 2, INTER.CV_INTER_AREA, false);
                pictureBox1.Image = imgcalib.ToBitmap();
                pictureBox2.Image = imgcalib.ToBitmap();

                pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
                pictureBox2.SizeMode = PictureBoxSizeMode.AutoSize;
/*
                Imgproc imgproc = new Imgproc();
                positions = imgproc.findbgpoints(imgcalib);
                int[] minmaxpositions = imgproc.findpositions(positions);
                imgcalib = imgproc.removebackground(imgcalib, positions);

                imgcalib.ROI = new Rectangle(minmaxpositions[0], minmaxpositions[1], minmaxpositions[2] - minmaxpositions[0], minmaxpositions[3] - minmaxpositions[1]);
                Image<Bgr, Byte> cropimg = imgcalib.Copy();*/
                
                
                Imgproc imgproc = new Imgproc();
                Image<Bgr, Byte> imgcalib3 = imgproc.subtractimages(imgcalib, imgcalib2);

                pictureBox1.Image = imgcalib3.ToBitmap();

            }
        }

    }    
}

