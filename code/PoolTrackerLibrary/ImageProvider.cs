﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;

using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.VideoSurveillance;

namespace PoolTrackerLibrary
{
    public class ImageProvider
    {
        public Image<Bgr, Byte> image;
        private Thread _thread;
        private bool autoRun = false;

        Capture cap;

        /// <summary>
        /// Returns the latest captured frame from the stream. If nescesarry a new frame is captured before the image is returned
        /// </summary>
        public Image<Bgr, byte> Image
        {
            get
            {
                if (!autoRun)
                {
                    captureFrame();
                }
                return image;
            }
        }

        public void captureFrame()
        {
            Rectangle crop = new Rectangle(98, 0, 1168, 670);
            image = cap.QueryFrame(); //.Resize(0.5, INTER.CV_INTER_LINEAR);  //Resize to half - saves tons of computationtime.
            if (image != null)
            {
                image.ROI = crop;
                //image = image.Resize(0.5, INTER.CV_INTER_CUBIC);
            }
            
        }

        public ImageProvider(int device = 0)
        {
            cap = new Capture(device);
            autoRun = true;
            setProperties();
            startCapture();
        }

        public ImageProvider(string filename)
        {
            cap = new Capture(filename);
            autoRun = false;
            //startCapture();
        }

        public void startCapture()
        {
            if (autoRun)
            {
                StartProcess();
            }
        }

        public void StartProcess()
        {
            //we need to create a new thread for our process
            _thread = new System.Threading.Thread(capture);
            //set the thread to run in the background
            _thread.IsBackground = true;
            _thread.Priority = ThreadPriority.Highest;
            //start our thread
            _thread.Start();
        }

        private void capture()
        {
            while (true)
            {
                if (image != null)
                {
                    lock (image)
                    {
                        image = cap.QueryFrame();
                    }
                }
                else
                {
                    image = cap.QueryFrame();
                }
                Thread.Sleep(100);
            }
        }

        public void setProperties(int height = 720, int width = 960) 
        {
            cap.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, height);
            cap.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, width);
        }

    }
}
