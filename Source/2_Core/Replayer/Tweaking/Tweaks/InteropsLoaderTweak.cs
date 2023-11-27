using BeatLeader.Interop;
using BeatLeader.Models;
using Zenject;

namespace BeatLeader.Replayer.Tweaking {
    internal class InteropsLoaderTweak : GameTweak {
        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;
        [Inject] private readonly IBeatmapTimeController _beatmapTimeController = null!;
        [Inject] private readonly BeatmapObjectManager _beatmapObjectManager = null!;

        public override void Initialize() {
            HandlePriorityPlayerChanged(_playersManager.PriorityPlayer!);
            _playersManager.PriorityPlayerWasChangedEvent += HandlePriorityPlayerChanged;
            _beatmapObjectManager.noteWasDespawnedEvent += HandleNoteWasDespawned;
            _beatmapTimeController.SongWasRewoundEvent += HandleSongWasRewound;
        }
        public override void Dispose() {
            Cam2Interop.HeadTransform = null;
            _playersManager.PriorityPlayerWasChangedEvent -= HandlePriorityPlayerChanged;
            _beatmapObjectManager.noteWasDespawnedEvent -= HandleNoteWasDespawned;
            _beatmapTimeController.SongWasRewoundEvent -= HandleSongWasRewound;
        }

        private void HandlePriorityPlayerChanged(IVirtualPlayer player) {
            Cam2Interop.HeadTransform = player.ControllersProvider.Head.transform;
        }
        
        private void HandleNoteWasDespawned(NoteController controller) {
            CustomNotesInterop.TryDespawnCustomObject(controller);
        }
        
        private void HandleSongWasRewound(float time) {
            NoodleExtensionsInterop.RequestReprocess();
        }
    }
}
