using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PoolTrackerLibrary;
using Emgu.CV.Structure;
using Emgu.CV;

namespace ColorCalibrator
{
    public partial class Form1 : Form
    {
        //ImageProvider provider = new ImageProvider("../../../../video/Video 7.wmv");
        ImageProvider provider = new ImageProvider();
 
        public Form1()
        {
            InitializeComponent();

            Application.Idle += new EventHandler(Application_Idle);
        }

        void Application_Idle(object sender, EventArgs e)
        {
            provider.startCapture();
            imageBox1.Image = provider.image;
            histogramBox1.ClearHistogram();
            histogramBox1.GenerateHistograms(provider.image.Convert<Hsv,byte>(), 255);
            histogramBox1.Refresh();
        }

        private void histogramBox1_Load(object sender, EventArgs e)
        {

        }

        private void histogramBox1_Load_1(object sender, EventArgs e)
        {

        }
    }
}
