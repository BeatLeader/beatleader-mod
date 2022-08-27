using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Utils
{
    public static class LayerMaskHelper
    {
        public static int LayersToBit(IEnumerable<int> collection)
        {
            int mask = 0;
            foreach (int i in collection)
                mask |= 1 << i;
            return mask;
        }
    }
}
