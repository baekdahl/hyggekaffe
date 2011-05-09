using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

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
    }
}
