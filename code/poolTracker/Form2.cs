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

using PoolTrackerLibrary;

namespace PoolTracker
{
    public partial class Form2 : Form
    {
        Image<Bgr, Byte> image;
        ImageProvider img;
        TableLocator tab;
        string stream;
        bool game8ball;

        public Form2()
        {
            InitializeComponent();
        }

        public void Run() 
        {

            img = new ImageProvider();
            //tab = new TableLocator();

            Application.Idle += new EventHandler(delegate(object sender, EventArgs e)
            {
                img.startCapture();
                image = img.image;

                if (tab != null)
                {
                    showImage1(tab.getTableImage(image));
                    Debug.Write("Table is occluded:" + tab.isTableOccluded(image)+"\n");
                }

                if (tabControl1.TabIndex==2)
                {
                    showImageCalibrateInput(image);
                }

            });
        }

        public void showBalls()
        {
            richTextBox1.Clear();
            
            Bitmap ballimg = new Bitmap("balls//1.jpg");
            Clipboard.SetDataObject(ballimg);
            DataFormats.Format format = DataFormats.GetFormat (DataFormats.Bitmap);
            richTextBox1.Paste(format);

            ballimg = new Bitmap("balls//2.jpg");
            Clipboard.SetDataObject(ballimg);
            format = DataFormats.GetFormat(DataFormats.Bitmap);
            richTextBox1.Paste(format);


        }

        public void showImage1(Image<Bgr, Byte> img)
        {
            imageBox1.Image = img;
        }

        public void showImageCalibrateInput(Image<Bgr, Byte> img)
        {
            imageBox3.Image = img;
        }
        
        public void showImageCalibrateFound(Image<Bgr, Byte> img)
        {
            imageBox2.Image = img;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("PoolTracker by Hyggekaffe 2011", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            stream = openFileDialog1.FileName;
            Run();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter =
            "Videos (*.AVI;*.MPG;*.WMV)|*.AVI;*.MPG;*.WMV|" +
            "All files (*.*)|*.*";

            openFileDialog1.FileName = "";
            openFileDialog1.ShowDialog();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            game8ball = true;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            game8ball = false;
        }

        private void calibratebutton_Click(object sender, EventArgs e)
        {
            if (img != null)
            {
                tab = new TableLocator(image);
                showImageCalibrateFound(tab.getTableImage(image));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Config.save(tab);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            tab.isTableOccluded(tab.getTableImage(image), 0.99);
        }

    }
}
