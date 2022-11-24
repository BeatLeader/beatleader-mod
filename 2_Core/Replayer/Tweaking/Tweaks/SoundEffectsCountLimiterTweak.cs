using BeatLeader.Models;
using BeatLeader.Utils;
using Zenject;

namespace BeatLeader.Replayer.Tweaking {
    internal class SoundEffectsCountLimiterTweak : GameTweak {
        private const int MaximumSoundsCount = 31;
        private static int _spawnedSoundsCount = 0;

        [Inject] private readonly IBeatmapTimeController _beatmapTimeController;

        private readonly HarmonyAutoPatch _cutSoundEffectManagerSpawnPatch = new(new(
            typeof(NoteCutSoundEffectManager).GetMethod(nameof(
                NoteCutSoundEffectManager.HandleNoteWasSpawned), ReflectionUtils.DefaultFlags),
            typeof(SoundEffectsCountLimiterTweak).GetMethod(nameof(
                SpawnNoteCutSoundEffectPrefix), ReflectionUtils.StaticFlags)));

        public override void Initialize() {
            _beatmapTimeController.EarlySongRewindEvent += HandleEarlySongRewind;
        }
        public override void Dispose() {
            _beatmapTimeController.EarlySongRewindEvent -= HandleEarlySongRewind;    
            _cutSoundEffectManagerSpawnPatch.Dispose();
        }

        private void HandleEarlySongRewind(float time) {
            _spawnedSoundsCount = 0;
        }

        private static bool SpawnNoteCutSoundEffectPrefix() {
            if (_spawnedSoundsCount < MaximumSoundsCount) {
                _spawnedSoundsCount++;
                return true;
            }
            return false;
        }
    }
}
