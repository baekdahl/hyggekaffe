using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.VideoSurveillance;

namespace PoolTrackerLibrary
{
    class xmlSaveCalibration
    {
    
        public string filename = "config.xml";
        XmlDocument configXml = new XmlDocument();
                
    public void saveMask(Image<Gray, Byte> mask)
        {
            if(File.Exists(filename)) 
            {
                configXml.Load(filename);

            }
    }











    }
}
