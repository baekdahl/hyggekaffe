using System;
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

        Capture cap;

        public ImageProvider(int device = 0)
        {
            cap = new Capture(device);
            setProperties();
            startCapture();
        }

        public ImageProvider(string filename)
        {
            cap = new Capture(filename);
            setProperties();
            startCapture();
        }

        public void startCapture()
        {
            image = cap.QueryFrame();
        }

        public void setProperties(int height = 720, int width = 960) 
        {
            cap.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, height);
            cap.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, width);
        }
        /*
        public List<string> getDevices()
        {
            return devices;
        }
        */

    }
}
