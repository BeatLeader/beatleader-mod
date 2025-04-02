namespace BeatLeader.Models.AbstractReplay {
    public interface IReplayNoteComparator {
        bool Compare(NoteEvent noteEvent, NoteData noteData);
    }
}