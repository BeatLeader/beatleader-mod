namespace BeatLeader.Models
{
    public class Score
    {
        public int id;
        public float accuracy;
        public int baseScore;
        public int modifiedScore;
        public string modifiers;
        public float pp;
        public int rank;
        public int badCuts;
        public int missedNotes;
        public int bombCuts;
        public int wallsHit;
        public int pauses;
        public bool fullCombo;
        public int hmd;
        public int controller;
        public string timeSet;
        public Player player;
        public string replay;
        public string platform;
    }
}
