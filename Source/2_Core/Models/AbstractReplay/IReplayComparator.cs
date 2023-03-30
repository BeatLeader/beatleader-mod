namespace BeatLeader.Models.AbstractReplay {
    public interface IReplayComparator {
        bool Compare(NoteEvent noteEvent, NoteData noteData);
    }
}