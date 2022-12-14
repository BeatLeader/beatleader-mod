using BeatLeader.Interop;
using Zenject;

namespace BeatLeader.Replayer.Tweaking {
    internal class InteropsLoaderTweak : GameTweak {
        [Inject] private readonly ReplayerControllersManager _controllersManager;
        [Inject] private readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] private readonly BeatmapTimeController _beatmapTimeController;

        public override void Initialize() {
            Cam2Interop.SetHeadTransform(_controllersManager.Head.transform);

            _beatmapObjectManager.noteWasDespawnedEvent += HandleNoteWasDespawned;
            _beatmapTimeController.SongRewindEvent += HandleSongWasRewinded;
        }
        public override void Dispose() {
            Cam2Interop.SetHeadTransform(null);

            _beatmapObjectManager.noteWasDespawnedEvent -= HandleNoteWasDespawned;
            _beatmapTimeController.SongRewindEvent -= HandleSongWasRewinded;
        }

        private void HandleNoteWasDespawned(NoteController controller) {
            CustomNotesInterop.TryDespawnCustomObject(controller);
        }
        private void HandleSongWasRewinded(float time) {
            NoodleExtensionsInterop.RequestReprocess();
        }
    }
}
