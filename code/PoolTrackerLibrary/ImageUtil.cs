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
    class ImageUtil
    {
       public static Image<Gray, Byte> bgrToGray(Image<Bgr, Byte> image)
        {
            return image.Convert<Gray, Byte>();
        }

       public static Image<Gray, Byte> bgrToHue(Image<Bgr, Byte> image) 
       {
           return image.Convert<Hsv, byte>().Split()[0];
       }

       public static DenseHistogram makeHist(Image<Gray, Byte> image) 
       {
            DenseHistogram hist = new DenseHistogram(255, new RangeF(0, 255));  //Make histogram with 255 bins in range from 0-255
            hist.Calculate(new Image<Gray, Byte>[] {  image   }, false, null);  //Calculate the histogram from the image with no accumulation and no mask.
            return hist;                                                        //Return the histogram
       }

       public static int histMaxValue(DenseHistogram hist) {

           float maxValue = 0;
           float minValue = 0;
           int[] maxLocation = { 0 };
           int[] minLocation = { 0 };
           hist.MinMax(out minValue, out maxValue, out minLocation, out maxLocation); //Find maximum and minimum values of histogram and their positions.

           return maxLocation[0];

       }

       public static Image<Gray, Byte> twoSidedThreshold(Image<Gray, Byte> image, int threshold, int deviation = 10, bool rangeToZero = false) 
       {
           Image<Gray, Byte> returnImage = image.Copy();

           byte withinRange = 255;
           byte outsideRange = 0;

           if (rangeToZero)
           {
               withinRange = 0;
               outsideRange = 255;
           }

           
          for (int j = 0; j < image.Width; j++)
            {
                for (int i = 0; i < image.Height; i++)
                {
                    if (image.Data[i,j,0] < threshold+deviation && image.Data[i,j,0] > threshold-deviation)                                               //Pixel within +-10 of maximum value.
                    {
                        returnImage.Data[i, j, 0] = withinRange;                                       //If value is within threshold, set to white.
                    }
                    else 
                    {
                        returnImage.Data[i, j, 0] = outsideRange;                                      //If value is not within threshold, set to black.
                    }
                }
            }

            return returnImage;
        }
  
    }
}
