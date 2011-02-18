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
        public iPoint[] removecloseiPoints(iPoint[] list, int threshold)
        {
            iPoint[] sortedlist = new iPoint[list.Length];
            int newlistcount = -1;

            for (int i = 1; i < list.Length; i++)
            {
                bool close = false;
                for (int j = 1; j < list.Length - i; j++)
                {
                    if ((int)Math.Sqrt((Math.Pow((list[j + i].X - list[i].X), 2) + Math.Pow((list[j + i].Y - list[i].Y), 2))) < threshold)
                    {
                        close = true;
                        //Debug.Write( Math.Abs((list[j].X - list[i].X) + (list[j].Y - list[i].Y))+"\n" );
                    }
                }
                if (close == false) { newlistcount++; sortedlist[newlistcount] = list[i]; }                
            }

            Array.Resize(ref sortedlist,newlistcount);

            return sortedlist;
        }

        public iPoint[] sortiPoints(iPoint[] list, int diamondcount)
        {
            iPoint[] sortedlist = new iPoint[diamondcount];

            for (int j = 0; j < sortedlist.Length; j++)
            {
                int max_value = 0;
                int pos_max_value = 0;

                for (int i = 0; i < list.Length; i++)
                {
                    if (list[i].value > max_value)
                    {
                        max_value = list[i].value;
                        pos_max_value = i;
                    }
                }
                sortedlist[j].X = list[pos_max_value].X;
                sortedlist[j].Y = list[pos_max_value].Y;
                list[pos_max_value].value = 0;
            }
            return sortedlist;
        }
        
        public iPoint[] findmaxima(Image<Gray, Byte> img)
        {
            int boxes = 80;

            int width = img.Width;
            int height = img.Height;

            int stepwidth = width / boxes;
            int stepheight = height / boxes;

            iPoint[] maximas = new iPoint[boxes * boxes*2];
            int[] maxiasdifference = new int[boxes * boxes*2];
            
            int count = 0;
            
            for (int i = 0; i < width - stepwidth; i = i + stepwidth)
            {
                for (int j = 0; j < height - stepheight; j = j + stepheight)
                {
                    int max = 0;
                    int value = 0;
                    int meanarealvalue = 0;

                    for (int u = 0; u < stepwidth - 1; u++)
                    {
                        for (int v = 0; v < stepheight - 1; v++)
                        {
                            value = (int)img[j + v, i + u].Intensity;
                            meanarealvalue += value;

                            if (img[j + v, i + u].Intensity > max) 
                            {
                                max = value;
                                maximas[count].Y = j + v;
                                maximas[count].X = i + u; 
                            }
                        }
                    
                    }

                    meanarealvalue = meanarealvalue / (stepheight * stepwidth);
                    maximas[count].value = max - meanarealvalue;
                    count++;
                }
            }

            return maximas;
        }

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
