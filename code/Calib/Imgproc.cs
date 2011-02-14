using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace Calib
{
    public class Imgproc
    {
        public Image<Bgr, Byte> threshold(Image<Bgr, Byte> imgcalib, int threshold)
        {
            Image<Gray, Single> imgcalibgray = imgcalib.Convert<Gray, Single>();
            imgcalibgray = imgcalibgray.ThresholdBinary(new Gray(threshold), new Gray(255));
            imgcalib = imgcalibgray.Convert<Bgr, Byte>(); 
            return imgcalib;
        }

        public int[] findpositions(Image<Bgr, Byte> imgcalib)
        {
            int[] whitepositions = new int[4];

            int width = imgcalib.Width;
            int height = imgcalib.Height;
            int minx = 255;
            int miny = 255;
            int maxx = 0;
            int maxy = 0;

            for (int i = 0; i < width - 1; i++)
            {
                for (int j = 0; j < height - 1; j++)
                {
                    if (imgcalib[j, i].Red == 255 && imgcalib[j, i].Blue == 255 && imgcalib[j, i].Green == 255)
                    {
                        if (j < minx) { minx = j; }
                        if (j > maxx) { maxx = j; }
                        if (i < miny) { miny = i; }
                        if (i > maxy) { maxy = i; }
                    }
                }
            }

            whitepositions[0] = minx;
            whitepositions[1] = miny;
            whitepositions[2] = maxx;
            whitepositions[3] = maxy;

            return whitepositions;
        }

        public Image<Bgr,Byte> backgremove(Image<Bgr,Byte> imgcalib)
        {
            Image<Hsv, Byte> imgcalibhsv = imgcalib.Convert<Hsv, Byte>();

            int width = imgcalibhsv.Width;
            int height = imgcalibhsv.Height;

            var counth = new Dictionary<int, int>();
            var countv = new Dictionary<int, int>();
            var counts = new Dictionary<int, int>();
            int valueh = 0;
            int valuev = 0;
            int values = 0;

            for (int i = 0; i < width - 1; i++)
            {
                for (int j = 0; j < height - 1; j++)
                {
                    valueh = (int)imgcalibhsv[j, i].Hue;
                    valuev = (int)imgcalibhsv[j, i].Value;
                    values = (int)imgcalibhsv[j, i].Satuation;

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

            for (int i = 0; i < width - 1; i++)
            {
                for (int j = 0; j < height - 1; j++)
                {
                    if (Math.Abs(imgcalibhsv[j, i].Hue - mostCommonValueh) <= 5)
                    {
                        if (Math.Abs(imgcalibhsv[j, i].Value - mostCommonValuev) <= 50)
                        {
                            if (Math.Abs(imgcalibhsv[j, i].Satuation - mostCommonValues) <= 50)
                            {
                                imgcalib[j, i] = new Bgr(255, 255, 255);
                            }
                        }
                        
                    }
                }
            }

            return imgcalib;
        }
    
    
    }
}
