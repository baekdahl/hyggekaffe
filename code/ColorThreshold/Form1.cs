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

namespace PoolTracker
{
    public partial class Form1 : Form
    {
        Capture capture = new Capture();
        Image<Bgr, byte> cameraImage;

        public Form1()
        {
            InitializeComponent();            
        }

        public void findBalls()
        {
            Image<Bgr, byte> tableImg = new Image<Bgr, byte>("tables/3.jpg");

            PoolTable table = new PoolTable(tableImg);
            imageBoxTable.Image = tableImg;

            Point ballPos;
            //foreach (string filename in Directory.GetFiles("balls", "*.jpg"))
            for (int i=1; i <= 16; i++)
            {
                if (i == 16) i = 0;
                string filename = "balls/" + i + ".jpg";
                logMessage("Matching: " + filename);
                Image<Bgr, byte> ball = new Image<Bgr, byte>(filename);
                ballPos = table.findBall(ball);
                imageBox1.Image = table.backProjectShow;
                
                Image<Gray, Byte> tableMatchMask = table._tableMatchMask.Copy();
                tableMatchMask._EqualizeHist();
                imageBox2.Image = tableMatchMask;

                logMessage("Score: " + table.bestMatch + Environment.NewLine);
                
                drawBallPos(imageBoxTable.Image.Bitmap, new Rectangle(ballPos.X, ballPos.Y, table.ballDia, table.ballDia));
                
                this.Refresh();
                Application.DoEvents();

                if (i == 0) break; 
            }
        }

        public void locateBalls()
        {
            Image<Bgr, byte> tableImg = new Image<Bgr, byte>("tables/1.jpg");

            PoolTable table = new PoolTable(tableImg);
            imageBoxTable.Image = tableImg;

            Point ballPos;
            //foreach (string filename in Directory.GetFiles("balls", "*.jpg"))
            for (int i = 1; i <= 16; i++)
            {
                if (i == 16) i = 0;
                string filename = "balls/bg.jpg";
                logMessage("Matching: " + filename);
                Image<Bgr, byte> ball = new Image<Bgr, byte>(filename);
                ballPos = table.findBall(ball);
                imageBox1.Image = table.backProjectShow;

                Image<Gray, Byte> tableMatchMask = table._tableMatchMask.Copy();
                tableMatchMask._EqualizeHist();
                imageBox2.Image = tableMatchMask;

                logMessage("Score: " + table.bestMatch + Environment.NewLine);

                drawBallPos(imageBoxTable.Image.Bitmap, new Rectangle(ballPos.X, ballPos.Y, table.ballDia, table.ballDia));

                this.Refresh();
                Application.DoEvents();

                if (i == 0) break;
            }
        }

        public void locateBalls2()
        {
            imageBoxTable.Image = cameraImage;

            Image<Bgr, byte> tableImg = cameraImage.Copy(new Rectangle(new Point(40, 280), new Size(780, 380)));
            imageBox1.Image = tableImg;

            PoolTable table = new PoolTable(tableImg);

            List<Ball> balls = table.findBalls(16);

            Image<Gray, Byte> tableMatchMask = table._tableMatchMask.Copy();
            tableMatchMask._EqualizeHist();
            imageBox2.Image = tableMatchMask;

            foreach (Ball ball in balls)
            {
                drawBallPos(imageBox1.Image.Bitmap, new Rectangle(ball.position.X, ball.position.Y, table.ballDia, table.ballDia));
            }
       }


        public void drawBallPos(Bitmap bitmap, Rectangle rect)
        {
            Graphics graphics = Graphics.FromImage(bitmap);
            Pen myPen = new Pen(System.Drawing.Color.Red, 3);
            graphics.DrawEllipse(myPen, rect);
        }

        public void logMessage(string message)
        {
            textBoxLog.AppendText(message + Environment.NewLine);
            textBoxLog.ScrollToCaret();
        }

        private void startCapture()
        {
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, 960);
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, 1280);
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS, 100);
            Application.Idle += new EventHandler(delegate(object sender, EventArgs e)
            {  //run this until application closed (close button click on image viewer)
                cameraImage = capture.QueryFrame(); //draw the image obtained from camera
                locateBalls2();
            });
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            startCapture();
        }
    }
}
