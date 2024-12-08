using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Models {
    public class DailyTreeStatus {
        public MapDetail song { get; set; }
        public Score? score { get; set; }
        public int bundleId { get; set; }
        public long startTime { get; set; }
    }

    public class TreeStatus {
    
        public DailyTreeStatus today { get; set; }
        public DailyTreeStatus[] previousDays { get; set; }
    }
}
