using System.Collections.Generic;

namespace BeatLeader.Models {
    public class DailyTreeStatus {
        public MapDetail song;
        public Score? score;
        public int bundleId;
        public long startTime;
    }

    public class BonusOrnament {
        public Score? score { get; set; }
        public int bundleId { get; set; }
        public string description { get; set; }
    }

    public class TreeStatus {
        public DailyTreeStatus? today;
        public DailyTreeStatus[] previousDays;
        public BonusOrnament[]? bonusOrnaments { get; set; }

        public List<(int, string)> GetOrnamentIds() {
            var result = new List<(int, string)>();
            if (today?.score != null) {
                result.Add((today.bundleId, null));
            }
            foreach (var prevDay in previousDays) {
                if (prevDay.score != null) {
                    result.Add((prevDay.bundleId, null));
                }
            }
            if (bonusOrnaments != null) {
                foreach (var bonus in bonusOrnaments) {
                    result.Add((bonus.bundleId, bonus.description));
                }
            }
            return result;
        }
    }
}