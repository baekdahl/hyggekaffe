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

namespace Calib
{
    public class FindTable
    {
        public FindTable()
        {
            Graph poolGraph = makePoolGraph();
        }

        public Image<Bgr,Byte> ShowImg() {
            string imgstring = "C:\\w.jpg";
            Image<Bgr, Byte> img = new Image<Bgr, Byte>(imgstring);
            return img;
        }

        public Graph makePoolGraph()
        {
            Graph poolGraph = new Graph();

            for (int v = 0; v < 18; v++)
            {
                poolGraph.addNode(new Point(1, 1));
            }

            for (int v = 0; v < 18; v++)
            {
                for(int e = 0; e < 18; e++)
                {
                    poolGraph.addEdge(v, e, v);
                }
            }
            return poolGraph;    
        }

    }
}
