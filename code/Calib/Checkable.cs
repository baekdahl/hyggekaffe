using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using vflibcs;
using System.Diagnostics;

namespace Calib
{
    class Checkable : IContextCheck
    {
        int x = 0;
        int y = 0;
        
        public bool FCompatible(IContextCheck icc)
        {
            Checkable crap = (Checkable)icc;
            Debug.Write(crap.x);
            return false;
        }

        public Checkable(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static void find()
        {

        }

    }
}
