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
        Image<Bgr, Byte> originalImage;

        /// <summary>
        /// Output from tablecalibrator showing only the table
        /// </summary>
        Image<Bgr, byte> tableImage;
        ImageProvider img;
        TableLocator tab;
        string stream;
        bool game8ball;
        BallCalibration calibration = new BallCalibration();

        public Form2()
        {
            InitializeComponent();
        }

        public void Run() 
        {

            img = new ImageProvider(stream);
            //tab = new TableLocator();

            Application.Idle += new EventHandler(delegate(object sender, EventArgs e)
            {
                originalImage = img.Image;

                if (originalImage != null && tab != null && tabControl1.SelectedIndex == 0)
                {
                    tableImage = tab.getTableImage(originalImage);
                    showImage1(tableImage);

                    if (!tab.isTableOccluded(originalImage))
                    {
                        locateBalls();
                    }
                    else
                    {
                        Debug.Write("Table is occluded\n");
                    }
                }

                if (tabControl1.SelectedIndex == 2)
                {
                    showImageCalibrateInput(originalImage);
                }

            });
        }

        public void locateBalls()
        {

            BallLocator locator = new BallLocator(tableImage, calibration, tab.mask);
            Ball.calibration = calibration; //HACK!

            List<Ball> balls = locator.idBalls();

            foreach (Ball ball in balls)
            {
                drawBallPos(imageBox1.Image.Bitmap, ball);
            }
        }


        public void drawBallPos(Bitmap bitmap, Ball ball)
        {
            Point center = ball.position;

            int radius = BallLocator.ballDia / 2;
            Rectangle boundingRect = new Rectangle(center.X - radius, center.Y - radius, BallLocator.ballDia, BallLocator.ballDia);
            Graphics graphics = Graphics.FromImage(bitmap);
            //Pen myPen = new Pen(ball.isStriped() ? System.Drawing.Color.White : Color.Red, 3);
            Pen myPen = new Pen(Color.White, 3);
            graphics.DrawEllipse(myPen, boundingRect);
            graphics.DrawString(((int)ball.color).ToString(), new Font("Tahoma", 20), ball.getBrush(), ball.position);
            //graphics.DrawEllipse(myPen, new Rectangle(center.X, center.Y, 2, 2));
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
                tab = new TableLocator(originalImage);
                showImageCalibrateFound(tab.getTableImage(originalImage));
                startBallCalibration();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Config.save(tab);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            tab.isTableOccluded(tab.getTableImage(originalImage), 0.99);
        }

        private void tabPage3_Enter(object sender, EventArgs e)
        {
            startBallCalibration();   
        }

        void startBallCalibration()
        {
            if (originalImage != null && tab != null) //If image is ready AND table calibrated a new calibration is started
            {
                Image<Bgr, byte> tableImage = tab.getTableImage(originalImage);
                if (tableImage != null)
                {
                    imageBoxCalib.Image = tableImage.Bitmap;
                    calibration = new BallCalibration(imageBoxCalib, tableImage);
                    calibrateLabel.Text = calibration.nextBall().ToString(); //Tell the user what ball he should click

                    //Register eventhandler to update label when next ball should be clicked
                    calibration.BallCalibrated += new BallCalibration.BallCalibratedHandler(calibration_BallCalibrated);

                }
            }
        }

        void calibration_BallCalibrated(object sender)
        {
            calibrateLabel.Text = Enum.GetName(typeof(BallColor), calibration.nextBall());
        }

        private void imageBoxCalib_MouseMoveOverImage(object sender, MouseEventArgs e)
        {
            Point imagePos = imageBoxCalib.MousePositionOnImage;

            if (imagePos.X < calibration.tableImage.Size.Width - Ball.ballDia && imagePos.Y < calibration.tableImage.Size.Height - Ball.ballDia)
            {
                Image<Bgr, byte> ball = calibration.tableImage.Copy(Ball.roiFromCenter(imagePos));
                imageBoxBallPreview.Image = ball.Copy(Ball.getMask());
            }

        }
    }
}
