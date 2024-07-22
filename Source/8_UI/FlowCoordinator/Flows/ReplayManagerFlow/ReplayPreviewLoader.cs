using System;
using BeatLeader.Models;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class ReplayPreviewLoader : IReplayPreviewLoader {
        [Inject] private readonly SongPreviewPlayer _songPreviewPlayer = null!;
        [Inject] private readonly AudioClipAsyncLoader _audioClipAsyncLoader = null!;
        [Inject] private readonly PerceivedLoudnessPerLevelModel _perceivedLoudnessPerLevelModel = null!;
        [Inject] private readonly BeatmapLevelsModel _beatmapLevelsModel = null!;

        private string? _currentPreviewedLevelId;

        public async void LoadPreview(IReplayHeaderBase header) {
            var hash = header.ReplayInfo.SongHash;
            if (_currentPreviewedLevelId == hash) return;
            //attempting to load
            var level = _beatmapLevelsModel.GetLevelPreviewForLevelId(hash);
            level ??= _beatmapLevelsModel.GetLevelPreviewForLevelId(CustomLevelLoader.kCustomLevelPrefixId + hash);
            //returning if failed
            if (level == null) return;
            _currentPreviewedLevelId = hash;
            try {
                var preview = await _audioClipAsyncLoader.LoadPreview(level);
                var loudnessCorrection = _perceivedLoudnessPerLevelModel.GetLoudnessCorrectionByLevelId(level.levelID);
                _songPreviewPlayer.CrossfadeTo(
                    preview,
                    loudnessCorrection,
                    level.previewStartTime,
                    level.previewDuration,
                    () => _audioClipAsyncLoader.UnloadPreview(level)
                );
            } catch (OperationCanceledException) {
                _currentPreviewedLevelId = null;
            }
        }

        public void StopPreview() {
            _songPreviewPlayer.CrossfadeToDefault();
            _currentPreviewedLevelId = null;
        }
    }
}