using BeatLeader.Interop;
using BeatLeader.Replayer.Emulation;
using Zenject;

namespace BeatLeader.Replayer.Tweaking {
    internal class PatchesLoaderTweak : GameTweak {
        [Inject] private readonly VRControllersProvider _controllersProvider;
        [Inject] private readonly BeatmapObjectManager _beatmapObjectManager;

        public override void Initialize() {
            RaycastBlocker.EnableBlocker = true;
            Cam2Interop.SetHeadTransform(_controllersProvider.Head.transform);

            _beatmapObjectManager.noteWasDespawnedEvent += HandleNoteWasDespawned;
        }
        public override void Dispose() {
            RaycastBlocker.EnableBlocker = false;
            Cam2Interop.SetHeadTransform(null);

            _beatmapObjectManager.noteWasDespawnedEvent -= HandleNoteWasDespawned;
        }

        private void HandleNoteWasDespawned(NoteController controller) {
            CustomNotesInterop.TryDespawnCustomObject(controller);
        }
    }
}
