using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Emgu.CV.Structure;

namespace PoolTrackerLibrary
{
    /// <summary>
    /// General purpose helper functions for the pooltracker program
    /// </summary>
    class Util
    {
        /// <summary>
        /// Stops a <see cref="System.Diagnostics.Stopwatch"/> instance and writes a line in debuglog along with the time. <s
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="text"></param>
        public static void writeWatch(Stopwatch sw, string text)
        {
            sw.Stop();
            Debug.Write(text + ": " + sw.ElapsedMilliseconds + "ms" + Environment.NewLine);
        }

        public static Stopwatch getWatch()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            return sw;
        }

        public static int getMaxIndex(int[] array)
        {
            int maxVal = array[0], maxIndex=0;

            for (int i = 1; i < array.Length; i++ )
            {
                if (array[i] > maxVal)
                {
                    maxIndex = i;
                    maxVal = array[i];
                }
            }
            return maxIndex;
        }

        public static int getMean(int[] array)
        {
            int sum = 0, count = 0;
            for (int i = 0; i < 256; i++)
            {
                count += (int)array[i];
                sum += (int)array[i] * i;
            }
            return count == 0 ? 0 : sum / count;
        }

        public static double vectorLength(double[] vector)
        {
            return Math.Sqrt(Math.Pow(vector[0],2) + Math.Pow(vector[1],2) + Math.Pow(vector[2],2));
        }

        public static double shortestAngle(Bgr color1, Bgr color2)
        {
            return shortestAngle(new double[] { color1.Blue, color1.Green, color1.Red }, new double[] { color2.Blue, color2.Green, color2.Red });
        }

        public static double shortestAngle(double[] vector1, double[] vector2)
        {
            double dotProduct = vector1[0] * vector2[0] + vector1[1] * vector2[1] + vector1[2] * vector2[2];

            double result = Math.Sqrt(1-Math.Pow(dotProduct / (vectorLength(vector1)*vectorLength(vector2)),2));

            return result;

        }

        public static double euclideanDistance(Bgr c1, Bgr c2)
        {
            return Math.Sqrt(Math.Pow(c1.Blue - c2.Blue, 2) + Math.Pow(c1.Green - c2.Green, 2) + Math.Pow(c1.Red - c2.Red, 2));
        }
    }
}
