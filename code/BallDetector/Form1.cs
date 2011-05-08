using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading;
using PoolTrackerLibrary;

namespace PoolTracker
{
    public partial class Form1 : Form
    {
        ImageProvider imageProvider;
        Image<Bgr, byte> cameraImage;

        TableLocator tableLocator = null;

        EventHandler captureHandler;
        BallCalibration calibration;

        public Form1()
        {
            InitializeComponent();
            calibration = new BallCalibration();
        }
        public void locateBalls()
        {

            BallLocator locator = new BallLocator(cameraImage, calibration, tableLocator.mask);
            List<Ball> balls = locator.findBalls(16);

            imageBoxTable.Image = cameraImage;
            imageBox2.Image = tableLocator.mask;

            locator.initialMatchMask._EqualizeHist();
            imageBox3.Image = locator.initialMatchMask;

            imageBox1.Image = cameraImage.Copy(); // Make a copy to draw the balls on

            foreach (Ball ball in balls)
            {
                drawBallPos(imageBox1.Image.Bitmap, ball);
            }
       }


        public void drawBallPos(Bitmap bitmap, Ball ball)
        {
            Point center = ball.position;

            int radius = BallLocator.ballDia/2;
            Rectangle boundingRect = new Rectangle(center.X-radius, center.Y-radius, BallLocator.ballDia, BallLocator.ballDia); 
            Graphics graphics = Graphics.FromImage(bitmap);
            //Pen myPen = new Pen(ball.isStriped() ? System.Drawing.Color.White : Color.Red, 3);
            Pen myPen = new Pen(Color.Red, 3);
            graphics.DrawEllipse(myPen, boundingRect);
            graphics.DrawString(((int)ball.color).ToString(), new Font("Tahoma", 20), ball.getBrush(), ball.position);
            //graphics.DrawEllipse(myPen, new Rectangle(center.X, center.Y, 2, 2));
        }

        private void startCapture(string filename = "")
        {
            if (File.Exists(filename))
            {
                imageProvider = new ImageProvider(filename);
            }
            else
            {
                imageProvider = new ImageProvider();
            }
        }

        private void runFrame()
        {
            imageProvider.startCapture();

            if (tableLocator == null)
            {
                tableLocator = new TableLocator(imageProvider.image);
            }

            cameraImage = tableLocator.getTableImage(imageProvider.image);

            locateBalls();
        }

        private void runOffline()
        {
            cameraImage = new Image<Bgr, byte>("tables/fracam.jpg");
            locateBalls();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            //startCapture();
            startCapture("../../video/Video 7.wmv");
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            runButton.Enabled = false;
            runFrame();
            runButton.Enabled = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                captureHandler = new EventHandler(Application_Idle);
                Application.Idle += captureHandler;
            }
            else
            {
                Application.Idle -= captureHandler;
            }
        }

        void Application_Idle(object sender, EventArgs e)
        {
            imageProvider.startCapture();

            if (tableLocator == null)
            {
                tableLocator = new TableLocator(imageProvider.image);
            }

            cameraImage = tableLocator.getTableImage(imageProvider.image);

            locateBalls();
        }

        private void imageBoxCalib_Click(object sender, EventArgs e)
        {

        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (tabControl1.SelectedIndex == 1 && cameraImage != null)
            {
                imageBoxCalib.Image = cameraImage;
                calibration = new BallCalibration(imageBoxCalib, cameraImage);
                calibrateLabel.Text = Enum.GetName(typeof(BallColor), calibration.nextBall());

                calibration.BallCalibrated += new BallCalibration.BallCalibratedHandler(calibration_BallCalibrated);
                pixelsInBall.Text = calibration.ballFactor.ToString();
                stripedPixels.Text = calibration.stripedFactor.ToString();
                cueWhite.Text = calibration.whiteFactor.ToString();
            }
        }

        void calibration_BallCalibrated(object sender)
        {
            calibrateLabel.Text = Enum.GetName(typeof(BallColor), calibration.nextBall()) ;
        }

        private void calibrateLabel_Click(object sender, EventArgs e)
        {

        }

        private void calibChanged(object sender, EventArgs e)
        {
            if (pixelsInBall.Text.Length > 0)
            {
                calibration.ballFactor = float.Parse(pixelsInBall.Text);
                calibration.stripedFactor = float.Parse(stripedPixels.Text);
                calibration.whiteFactor = float.Parse(cueWhite.Text);
            }

        }

        
    }
}
