using System.Linq;
using IPA.Utilities;
using UnityEngine;

namespace BeatLeader.Replayer.Emulation {
    public class NoteControllerEmulator : NoteController {
        public override NoteData noteData => _noteData;
        public NoteCutInfo CutInfo { get; private set; }

        private NoteController _prefab;
        private NoteData _noteData;

        protected override void Awake() {
            _prefab = Resources.FindObjectsOfTypeAll<BeatmapObjectsInstaller>()
                .FirstOrDefault().GetField<GameNoteController, BeatmapObjectsInstaller>("_normalBasicNotePrefab");
            _noteMovement = _prefab.GetField<NoteMovement, NoteController>("_noteMovement");
            _noteTransform = transform;
        }
        public void Setup(NoteData noteData, NoteCutInfo cutInfo) {
            _noteData = noteData;
            CutInfo = cutInfo;
        }

        #region Garbage

        public override void ManualUpdate() { }
        protected override void OnDestroy() { }
        protected override void HiddenStateDidChange(bool _) { }
        public override void Pause(bool _) { }

        #endregion
    }
}