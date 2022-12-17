using BeatLeader.Interop;
using BeatLeader.Models;
using BeatLeader.Replayer.Emulation;
using Zenject;

namespace BeatLeader.Replayer.Tweaking {
    internal class InteropsLoaderTweak : GameTweak {
        [Inject] private readonly IVirtualPlayersManager _playersManager;
        [Inject] private readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] private readonly BeatmapTimeController _beatmapTimeController;

        public override void Initialize() {
            HandlePriorityPlayerChanged(_playersManager.PriorityPlayer);
            _playersManager.PriorityPlayerWasChangedEvent += HandlePriorityPlayerChanged;
            _beatmapObjectManager.noteWasDespawnedEvent += HandleNoteWasDespawned;
            _beatmapTimeController.SongWasRewoundEvent += HandleSongWasRewound;
        }
        public override void Dispose() {
            Cam2Interop.SetHeadTransform(null);
            _playersManager.PriorityPlayerWasChangedEvent -= HandlePriorityPlayerChanged;
            _beatmapObjectManager.noteWasDespawnedEvent -= HandleNoteWasDespawned;
            _beatmapTimeController.SongWasRewoundEvent -= HandleSongWasRewound;
        }

        private void HandlePriorityPlayerChanged(VirtualPlayer player) {
            Cam2Interop.SetHeadTransform(player.ControllersProvider.Head.transform);
        }
        private void HandleNoteWasDespawned(NoteController controller) {
            CustomNotesInterop.TryDespawnCustomObject(controller);
        }
        private void HandleSongWasRewound(float time) {
            NoodleExtensionsInterop.RequestReprocess();
        }
    }
}
