namespace BeatLeader.Models
{
    internal class Score
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
        public string timeSet;
        public Player player;
        public string replay;
    }
}
