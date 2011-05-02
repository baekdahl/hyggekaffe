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
        bool busy = false;

        TableLocator tableLocator = null;

        public Form1()
        {
            InitializeComponent();            
        }
        public void locateBalls()
        {
            imageBoxTable.Image = cameraImage;

            Image<Bgr, byte> tableImg = cameraImage;
            imageBox1.Image = tableImg;

            BallLocator table = new BallLocator(tableImg, 16, tableLocator.mask);

            List<Ball> balls = table.findBalls(16);

            imageBox2.Image = tableLocator.mask;

            table.initialMatchMask._EqualizeHist();
            imageBox3.Image = table.initialMatchMask;


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
            Pen myPen = new Pen(ball.isStriped() ? System.Drawing.Color.White : Color.Red, 3);
            graphics.DrawEllipse(myPen, boundingRect);
            graphics.DrawEllipse(myPen, new Rectangle(center.X, center.Y, 2, 2));
        }

        public void logMessage(string message)
        {
            textBoxLog.AppendText(message + Environment.NewLine);
            textBoxLog.ScrollToCaret();
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

            Application.Idle += new EventHandler(delegate(object sender, EventArgs e)
            {
                imageProvider.startCapture();
                
                if (tableLocator == null) {
                    tableLocator = new TableLocator(imageProvider.image);
                }

                cameraImage = tableLocator.getTableImage(imageProvider.image);

                locateBalls();
                
            });
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            //startCapture(); //Online
            startCapture("../../video/Video 3.wmv");
        }
    }
}
