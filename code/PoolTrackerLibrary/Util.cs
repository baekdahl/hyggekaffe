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
    }
}
