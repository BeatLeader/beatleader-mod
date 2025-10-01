﻿using System.Collections.Generic;
using BeatLeader.Models.AbstractReplay;

namespace BeatLeader.Models {
    public interface IReplayBeatmapData {
        IReadOnlyCollection<NoteData> NoteDatas { get; }

        int FindNoteDataForEvent(NoteEvent noteEvent, int startIndex, out NoteData? noteData);
    }
}