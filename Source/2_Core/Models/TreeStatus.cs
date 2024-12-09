namespace BeatLeader.Models {
    public class DailyTreeStatus {
        public MapDetail song;
        public Score? score;
        public int bundleId;
        public long startTime;
    }

    public class TreeStatus {
        public DailyTreeStatus today;
        public DailyTreeStatus[] previousDays;
    }
}