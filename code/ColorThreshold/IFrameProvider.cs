using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;

namespace PoolTracker
{
    interface IFrameProvider
    {
        Image<Bgr, byte> getFrame();
    }
}
