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
    public struct Node
    {
        public int id;
        public Point position;
        public List<Point> edges;
    }
  
    public class Graph 
    {
        Node[] nodelist = new Node[0];

        public Graph()
        {
        }

        public int addNode(Point pos)
        {
            int length = nodelist.Length;

            Array.Resize(ref nodelist, length + 1);

            nodelist[length].id = length;
            nodelist[length].position = pos;

            nodelist[length ].edges = new List<Point>();

            return nodelist[length].id;
        }

        public void addEdge(int start, int end, int id)
        {
            nodelist[id].edges.Add( new Point(start, end));
        }
      
    }
}
