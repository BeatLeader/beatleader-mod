using System;
using BeatLeader.Models;
using HarmonyLib;
using Zenject;

namespace BeatLeader.UI.Hub {
    [HarmonyPatch]
    internal class ReplayPreviewLoader : IReplayPreviewLoader {
        #region Loader

        [Inject] private readonly SongPreviewPlayer _songPreviewPlayer = null!;
        [Inject] private readonly AudioClipAsyncLoader _audioClipAsyncLoader = null!;
        [Inject] private readonly PerceivedLoudnessPerLevelModel _perceivedLoudnessPerLevelModel = null!;
        [Inject] private readonly BeatmapLevelsModel _beatmapLevelsModel = null!;
        [Inject] private readonly LevelCollectionViewController _levelCollectionViewController = null!;

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
                InvokePatches(level);
            } catch (OperationCanceledException) {
                _currentPreviewedLevelId = null;
            }
        }

        public void StopPreview() {
            _songPreviewPlayer.CrossfadeToDefault();
            _currentPreviewedLevelId = null;
        }

        #endregion

        #region Patches

        private static bool _blockInvocation;
        
        private void InvokePatches(IPreviewBeatmapLevel beatmapLevel) {
            _blockInvocation = true;
            _levelCollectionViewController.HandleLevelCollectionTableViewDidSelectLevel(null, beatmapLevel);
            _blockInvocation = false;
        }

        [HarmonyPatch(typeof(LevelCollectionViewController), nameof(LevelCollectionViewController.HandleLevelCollectionTableViewDidSelectLevel))]
        private static bool DidSelectLevelPrefix() {
            return !_blockInvocation;
        }

        #endregion
    }
}