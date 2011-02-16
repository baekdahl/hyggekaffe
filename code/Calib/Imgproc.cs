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

using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace Calib
{
    public class Imgproc
    {

        public Image<Bgr, Byte> subtractimages(Image<Bgr, Byte> img1, Image<Bgr, Byte> img2)
        {
            Image<Bgr, Byte> imgsub = new Image<Bgr, Byte>(img1.Size);
            int width = img1.Width;
            int height = img1.Height;

            img1 = img1.ConvertScale<Byte>(0.5, 128);
            img2 = img2.ConvertScale<Byte>(0.5, 0);

            for (int i = 0; i < width - 1; i++)
            {
                for (int j = 0; j < height - 1; j++)
                {
                    imgsub[j, i] = new Bgr(img1[j, i].Blue - img2[j, i].Blue, img1[j, i].Green - img2[j, i].Green, img1[j, i].Red - img2[j, i].Red);
                    //Debug.Write("{" + imgsub[j, i].Blue + "{" + imgsub[j, i].Green + "{" + imgsub[j, i].Red + "}\n");
                }

            }
            imgsub = imgsub.ConvertScale<Byte>(2,0);
            
            return imgsub;
        }

        public Image<Bgr, Byte> threshold(Image<Bgr, Byte> imgcalib, int threshold)
        {
            Image<Gray, Single> imgcalibgray = imgcalib.Convert<Gray, Single>();
            imgcalibgray = imgcalibgray.ThresholdBinary(new Gray(threshold), new Gray(255));
            imgcalib = imgcalibgray.Convert<Bgr, Byte>(); 
            return imgcalib;
        }

        public int[] findpositions(Point[] positions)
        {
            int[] minmaxpositions = new int[4];
            int minx = 10000;
            int maxx = 0;
            int miny = 10000;
            int maxy = 0;

            foreach (Point point in positions)
            {
                //Debug.Write(point);
                if(point.X < minx) {minx = point.X;}
                if(point.X > maxx) {maxx = point.X;}
                if(point.Y < miny) {miny = point.Y;}
                if(point.Y > maxy) {maxy = point.Y;}
            }

            Debug.Write(minx + "+" + miny + "+" + maxx + "+" + maxy);
            minmaxpositions[0] = minx;
            minmaxpositions[1] = miny;
            minmaxpositions[2] = maxx;
            minmaxpositions[3] = maxy;
            return minmaxpositions;
        }

        public Point[]  findbgpoints(Image<Bgr,Byte> imgcalib)
        {
            Image<Hsv, Byte> imgcalibhsv = imgcalib.Convert<Hsv, Byte>();
            Point[] positions = new Point[imgcalibhsv.Width * imgcalibhsv.Height];
            int width = imgcalibhsv.Width;
            int height = imgcalibhsv.Height;
            var counth = new Dictionary<int, int>();
            var countv = new Dictionary<int, int>();
            var counts = new Dictionary<int, int>();
            int valueh = 0;
            int valuev = 0;
            int values = 0;

            TextWriter hsvout = new StreamWriter("hsv.txt");

            for (int i = 0; i < width - 1; i++)
            {
                for (int j = 0; j < height - 1; j++)
                {
                    valueh = (int)imgcalibhsv[j, i].Hue;
                    valuev = (int)imgcalibhsv[j, i].Value;
                    values = (int)imgcalibhsv[j, i].Satuation;

                    hsvout.WriteLine("{0};{1};{2};", valueh, valuev, values);

                    if (counth.ContainsKey(valueh))
                    {
                        counth[valueh]++;
                    }
                    else
                    {
                        counth.Add(valueh, 1);
                    }

                    if (countv.ContainsKey(valuev))
                    {
                        countv[valuev]++;
                    }
                    else
                    {
                        countv.Add(valuev, 1);
                    }

                    if (counts.ContainsKey(values))
                    {
                        counts[values]++;
                    }
                    else
                    {
                        counts.Add(values, 1);
                    }

                }
            }

            hsvout.Close();
        
            int mostCommonValueh = 0;
            int highestCounth = 0;
            foreach (KeyValuePair<int, int> pair in counth)
            {
                if (pair.Value > highestCounth)
                {
                    mostCommonValueh = pair.Key;
                    highestCounth = pair.Value;
                }
            }

            int mostCommonValuev = 0;
            int highestCountv = 0;
            foreach (KeyValuePair<int, int> pair in countv)
            {
                if (pair.Value > highestCountv)
                {
                    mostCommonValuev = pair.Key;
                    highestCountv = pair.Value;
                }
            }

            int mostCommonValues = 0;
            int highestCounts = 0;
            foreach (KeyValuePair<int, int> pair in counts)
            {
                if (pair.Value > highestCounts)
                {
                    mostCommonValues = pair.Key;
                    highestCounts = pair.Value;
                }
            }

            Debug.Write("h:"+mostCommonValueh+"s:"+mostCommonValues+"v:"+mostCommonValuev+"\n");


            int count=-1;

            for (int i = 0; i < width - 1; i++)
            {
                for (int j = 0; j < height - 1; j++)
                {
                    if ((Math.Abs(imgcalibhsv[j, i].Hue - mostCommonValueh) <= 15))
                    {
                        if ((Math.Abs(imgcalibhsv[j, i].Value - mostCommonValuev) <= 50))
                        {
                            if ((Math.Abs(imgcalibhsv[j, i].Satuation - mostCommonValues) <= 50))
                            {
                                count++;
                                positions[count].X = i;
                                positions[count].Y = j;
                            }
                        }
                        
                    }
                }
            }

            Array.Resize(ref positions, count);
            return positions;
        }

        public Image<Bgr, Byte> removebackground(Image<Bgr, Byte> imgcalib, Point[] positions)
        {

            Image<Bgr, Byte> imgcalib2 = new Image<Bgr, Byte>(imgcalib.Size);

            foreach (Point point in positions)
            {
              imgcalib[point] = new Bgr(0, 0, 0);
            }
            return imgcalib;
        }
    
    }
}
