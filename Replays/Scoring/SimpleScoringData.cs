using BeatLeader.Models;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace BeatLeader.Replays.Scoring
{
    public class SimpleScoringData
    {
        public readonly NoteData noteData;
        public readonly NoteEvent noteEvent;
        public readonly Quaternion worldRotation;
        public readonly Quaternion inverseWorldRotation;
        public readonly Quaternion noteRotation;
        public readonly Vector3 notePosition;
        public readonly bool scoreAsync;

        public SimpleScoringData() { }
        public SimpleScoringData(NoteController noteController, NoteEvent noteEvent, bool scoreAsync)
        {
            this.noteData = noteController.noteData;
            this.noteEvent = noteEvent;
            this.worldRotation = noteController.worldRotation;
            this.inverseWorldRotation = noteController.inverseWorldRotation;
            this.noteRotation = noteController.noteTransform.localRotation;
            this.notePosition = noteController.noteTransform.localPosition;
            this.scoreAsync = scoreAsync;
        }
        public SimpleScoringData(NoteData noteData, NoteEvent noteEvent, Quaternion worldRotation, Quaternion inverseWorldRotation, Quaternion noteRotation, Vector3 notePosition, bool scoreAsync)
        {
            this.noteData = noteData;
            this.noteEvent = noteEvent;
            this.worldRotation = worldRotation;
            this.inverseWorldRotation = inverseWorldRotation;
            this.noteRotation = noteRotation;
            this.notePosition = notePosition;
            this.scoreAsync = scoreAsync;
        }
    }
}
