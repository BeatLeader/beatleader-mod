namespace BeatLeader.Models.AbstractReplay {
    public readonly struct NoteEvent {
        public enum NoteEventType {
            Unknown = -1,
            GoodCut = 0,
            BadCut = 1,
            Miss = 2,
            BombCut = 3,
        }
        
        public NoteEvent(int noteId, 
            float eventTime, 
            float spawnTime,
            NoteEventType eventType, 
            float beforeCutRating,
            float afterCutRating,
            NoteCutInfo noteCutInfo) {
            this.noteId = noteId;
            this.eventTime = eventTime;
            this.spawnTime = spawnTime;
            this.eventType = eventType;
            this.beforeCutRating = beforeCutRating;
            this.afterCutRating = afterCutRating;
            this.noteCutInfo = noteCutInfo;
        }

        public float CutTime => spawnTime - noteCutInfo.timeDeviation;
        
        public readonly int noteId;
        public readonly float eventTime;
        public readonly float spawnTime;
        public readonly NoteEventType eventType;
        public readonly float beforeCutRating;
        public readonly float afterCutRating;
        public readonly NoteCutInfo noteCutInfo;
    }
}