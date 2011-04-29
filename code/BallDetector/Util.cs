using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace PoolTracker
{
    class Util
    {
        public static void writeWatch(Stopwatch sw, string text)
        {
            sw.Stop();
            Debug.Write(text + ": " + sw.ElapsedMilliseconds + "ms" + Environment.NewLine);
        }
    }
}
