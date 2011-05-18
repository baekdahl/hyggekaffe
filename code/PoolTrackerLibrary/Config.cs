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
using System.Diagnostics;
using System.Xml.Serialization;
using System.Drawing;

namespace PoolTrackerLibrary
{
    public class Config
    {
    
        public static string filename = "config.xml";
        static XmlDocument configXml = new XmlDocument();

        public static void save(TableLocator tab)
        {
            tab.mask.Save("mask.png");

            if (File.Exists(filename))
            {
                configXml.Load(filename);
                XmlNodeList nodelist = configXml.GetElementsByTagName("Table");
                nodelist[0].ChildNodes[0].InnerText = tab.angle.ToString();
                nodelist[0].ChildNodes[1].InnerText = tab.maskarea.ToString();
                nodelist[0].ChildNodes[2].InnerText = tab.histMaxValue.ToString();
                nodelist[0].ChildNodes[3].InnerText = tab.maskperimeter.ToString();
                
                nodelist[0].ChildNodes[4].ChildNodes[0].InnerText = tab.ROI.X.ToString();
                nodelist[0].ChildNodes[4].ChildNodes[1].InnerText = tab.ROI.Y.ToString();
                nodelist[0].ChildNodes[4].ChildNodes[2].InnerText = tab.ROI.Width.ToString();
                nodelist[0].ChildNodes[4].ChildNodes[3].InnerText = tab.ROI.Height.ToString();
                                
                configXml.Save(filename);
            }
        }

        public static void save(BallColor ballname, Bgr color) 
        {
            if (File.Exists(filename))
            {
                configXml.Load(filename);
                XmlNodeList nodelist = configXml.GetElementsByTagName("Ball");

                for (int i = 0; i < nodelist[0].ChildNodes.Count; i++) 
                {
                    if(nodelist[0].ChildNodes[i].Name==ballname.ToString()) 
                    {
                        nodelist[0].ChildNodes[i].ChildNodes[0].InnerText = color.Blue.ToString();
                        nodelist[0].ChildNodes[i].ChildNodes[1].InnerText = color.Green.ToString();
                        nodelist[0].ChildNodes[i].ChildNodes[2].InnerText = color.Red.ToString();
                    }
                }

                configXml.Save(filename);
            }

        }

        public static void save(int ballsize)
        {
            if (File.Exists(filename))
            {
                configXml.Load(filename);
                XmlNodeList nodelist = configXml.GetElementsByTagName("Ball");
                nodelist[0].ChildNodes[0].InnerText = ballsize.ToString();
                configXml.Save(filename);

            }


        }

        public static void load(TableLocator tab)
        {
            tab.mask = new Image<Gray, Byte>("mask.png");
            if (File.Exists(filename))
            {
                configXml.Load(filename);
                XmlNodeList nodelist = configXml.GetElementsByTagName("Table");

                tab.angle = Convert.ToDouble(nodelist[0].ChildNodes[0].InnerText);
                tab.maskarea = Convert.ToDouble(nodelist[0].ChildNodes[1].InnerText);
                tab.histMaxValue = Convert.ToInt16(nodelist[0].ChildNodes[2].InnerText);
                tab.maskperimeter = Convert.ToDouble(nodelist[0].ChildNodes[3].InnerText);

                int x = Convert.ToInt16(nodelist[0].ChildNodes[4].ChildNodes[0].InnerText);
                int y = Convert.ToInt16(nodelist[0].ChildNodes[4].ChildNodes[1].InnerText);
                int width = Convert.ToInt16(nodelist[0].ChildNodes[4].ChildNodes[2].InnerText);
                int height = Convert.ToInt16(nodelist[0].ChildNodes[4].ChildNodes[3].InnerText);

                tab.ROI = new Rectangle(x, y, width, height);               
            }

        }

        public static Bgr load(BallColor ballname) 
        {
            Bgr returncolor = new Bgr();
            if (File.Exists(filename))
            {
                configXml.Load(filename);
                XmlNodeList nodelist = configXml.GetElementsByTagName("Ball");

                for(int i=0; i < nodelist[0].ChildNodes.Count; i++) 
                {
                    if(nodelist[0].ChildNodes[i].Name==ballname.ToString()) 
                    {
                        returncolor.Blue = Convert.ToDouble(nodelist[0].ChildNodes[i].ChildNodes[0].InnerText);
                        returncolor.Green = Convert.ToDouble(nodelist[0].ChildNodes[i].ChildNodes[1].InnerText);
                        returncolor.Red = Convert.ToDouble(nodelist[0].ChildNodes[i].ChildNodes[2].InnerText);
                    }
                }

                
            }
            return returncolor;
        }

        public static int load()
        {
            int ballsize = 0;
            if (File.Exists(filename))
            {
                configXml.Load(filename);
                XmlNodeList nodelist = configXml.GetElementsByTagName("Ball");
                ballsize = Convert.ToInt16(nodelist[0].ChildNodes[0].InnerText);
            }
            return ballsize;

        }
    }
}
