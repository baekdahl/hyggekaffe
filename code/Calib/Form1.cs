using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

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
                imgcalib = imgcalib.Resize(640, 480, INTER.CV_INTER_AREA, false);
                change_Pix1(imgcalib);
                pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;

                imgcalib = imgcalib.SmoothGaussian(5, 5, 2, 2);
                Image<Gray, Byte> imgcanny = imgcalib.Convert<Gray, Byte>().Canny(new Gray(100), new Gray(80));
                imgcanny._Dilate(2);
                imgcanny._Erode(2);

                pictureBox1.Image = imgcanny.ToBitmap();


            }
        }

        public void change_Pix1(Image<Bgr, Byte> imagename)
        {
            pictureBox1.Image = imagename.ToBitmap();
        }

    }    
}

