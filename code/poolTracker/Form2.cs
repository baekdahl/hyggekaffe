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
using poolTracker.Properties;

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
        List<Dictionary<BallColor, Ball>> ballsarray = new List<Dictionary<BallColor, Ball>>();
        BallCalibration calibration = new BallCalibration();

        private static int lastTick;
        private static int lastFrameRate;
        private static int frameRate;

        public static int CalculateFrameRate()
        {
            if (System.Environment.TickCount - lastTick >= 1000)
            {
                lastFrameRate = frameRate;
                frameRate = 0;
                lastTick = System.Environment.TickCount;
            }
            frameRate++;
            return lastFrameRate;
        }

        public Form2()
        {
            InitializeComponent();
        }

        public void Run() 
        {
            bool occluded = true;
            img = new ImageProvider(stream);
            
            tab = new TableLocator();
            loadBallCalibration();

            Ball.ballDia = (int)(26 * 0.95);
            BallLocator.ballDia = (int)(26 * 0.95);

            Application.Idle += new EventHandler(delegate(object sender, EventArgs e)
            {
                originalImage = img.Image;
                this.Text = "PoolTracker running with FPS:" + CalculateFrameRate().ToString();

                if (originalImage != null && tab != null && tabControl1.SelectedIndex == 0)
                {
                    tableImage = tab.getTableImage(originalImage);
                    
                    showImage1(tableImage);
                    imageBox5.Image = tableImage.Copy();
                    imageBox6.Image = tableImage.CopyBlank();

                    if (!tab.isTableOccluded(originalImage))//==occluded)
                    {
                       // occluded = !occluded;
                        locateBalls();
                    }
                    
                    else
                    {
                        Debug.Write("Table is occluded\n");
                    }
                }

                if (tabControl1.SelectedIndex == 2)
                {
                    Thread.Sleep(100);
                    showImageCalibrateInput(originalImage);
                }

            });
        }
        /*
        public bool stillBalls(int distance_threshold = 13, int frames = 3)
        {
            if (ballsarray.Count < frames) { return true; }

            foreach (Dictionary<BallColor, Ball> currentFrameBalls in ballsarray)
            {

                double x_distance = ballsarray[balls.count];
                double y_distance = Math.Abs(ballsarray[ballsarray.Count-1][i].position.X - ballsarray[ballsarray.Count-n][i].position.X);

                   /if (x_distance > distance_threshold) 
                    { 
                        return false; 
                    }
                    if (y_distance > distance_threshold) 
                    { 
                        return false; 
                    }

            }
            return true;
        }
        */

        public void locateBalls()
        {
            BallLocator locator = new BallLocator(tableImage, calibration, tab.mask);
            Ball.calibration = calibration; //HACK!

            List<Ball> balls = locator.idBalls();
            Dictionary<BallColor, Ball> addToArray = new Dictionary<BallColor, Ball>();

            foreach (Ball ball in balls)
            {
                drawBallPos(imageBox1.Image.Bitmap, ball);
                drawBallCopy(imageBox6.Image.Bitmap, ball);
                //addToArray.Add(ball.color, ball);
            }
            
            //ballsarray.Add(addToArray);
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

        public void drawBallCopy(Bitmap bitmap, Ball ball)
        {
            Point center = ball.position;

            int radius = BallLocator.ballDia / 2;
            Rectangle boundingRect = new Rectangle(center.X - radius, center.Y - radius, BallLocator.ballDia, BallLocator.ballDia);
            Graphics graphics = Graphics.FromImage(bitmap);

            Pen myPen = new Pen(Color.White, 3);
            graphics.FillEllipse(Brushes.White, boundingRect);

            Bitmap ballImage = (Bitmap)Resources.ResourceManager.GetObject("_" + ((int)ball.color).ToString());
            if (ballImage != null)
            {
                graphics.DrawImage(ballImage, Ball.roiFromCenter(ball.position));
            }
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
                //startBallCalibration();
            }
        }

        private void SaveSettings_Click(object sender, EventArgs e)
        {
             Config.save(tab);
             Config.save(BallColor.Cue, calibration.ballBgr[BallColor.Cue]);
             Config.save(BallColor.Black, calibration.ballBgr[BallColor.Black]);
             Config.save(BallColor.Red, calibration.ballBgr[BallColor.Red]);
             Config.save(BallColor.Orange, calibration.ballBgr[BallColor.Orange]);
             Config.save(BallColor.Brown, calibration.ballBgr[BallColor.Brown]);
             Config.save(BallColor.Yellow, calibration.ballBgr[BallColor.Yellow]);
             Config.save(BallColor.Green, calibration.ballBgr[BallColor.Green]);
             Config.save(BallColor.Blue, calibration.ballBgr[BallColor.Blue]);
             Config.save(BallColor.Purple, calibration.ballBgr[BallColor.Purple]);
       }

        private void tabPage3_Enter(object sender, EventArgs e)
        {
           // startBallCalibration();   
        }

        private void loadBallCalibration()
        {
            calibration.ballBgr[BallColor.Cue] = Config.load(BallColor.Cue);
            calibration.ballBgr[BallColor.Black] = Config.load(BallColor.Black);
            calibration.ballBgr[BallColor.Red] = Config.load(BallColor.Red);
            calibration.ballBgr[BallColor.Orange] = Config.load(BallColor.Orange);
            calibration.ballBgr[BallColor.Brown] = Config.load(BallColor.Brown);
            calibration.ballBgr[BallColor.Yellow] = Config.load(BallColor.Yellow);
            calibration.ballBgr[BallColor.Green] = Config.load(BallColor.Green);
            calibration.ballBgr[BallColor.Blue] = Config.load(BallColor.Blue);
            calibration.ballBgr[BallColor.Purple] = Config.load(BallColor.Purple);

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

        private void button1_Click(object sender, EventArgs e)
        {
            startBallCalibration();
        }
    }
}
