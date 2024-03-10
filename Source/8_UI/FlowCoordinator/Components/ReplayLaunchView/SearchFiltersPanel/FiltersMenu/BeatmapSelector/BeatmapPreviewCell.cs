using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using static BeatmapLevelSO;

namespace BeatLeader.Components {
    internal class BeatmapPreviewCell : ReeUIComponentV2 {
        #region Dummies

        private class NotSelectedPreviewBeatmapLevel : BeatmapLevel {

            public NotSelectedPreviewBeatmapLevel(bool hasPrecalculatedData, string levelID, string songName, string songSubName, string songAuthorName, string[] allMappers, string[] allLighters, float beatsPerMinute, float integratedLufs, float songTimeOffset, float previewStartTime, float previewDuration, float songDuration, PlayerSensitivityFlag contentRating, IPreviewMediaData previewMediaData, IReadOnlyDictionary<(BeatmapCharacteristicSO, BeatmapDifficulty), BeatmapBasicData> beatmapBasicData) : base(hasPrecalculatedData, levelID, songName, songSubName, songAuthorName, allMappers, allLighters, beatsPerMinute, integratedLufs, songTimeOffset, previewStartTime, previewDuration, songDuration, contentRating, previewMediaData, beatmapBasicData) {
            }

            public string? levelID { get; } = string.Empty;
            public string? songName { get; } = "Click to select";
            public string? songSubName { get; } = null;
            public string? songAuthorName { get; } = "Unknown";
            public string? levelAuthorName { get; } = null;

            public float beatsPerMinute { get; } = 0;
            public float songTimeOffset { get; } = -1;
            public float shuffle { get; } = -1;
            public float shufflePeriod { get; } = -1;
            public float previewStartTime { get; } = -1;
            public float previewDuration { get; } = -1;
            public float songDuration { get; } = 0;

            public EnvironmentInfoSO? environmentInfo { get; } = null;
            public EnvironmentInfoSO? allDirectionsEnvironmentInfo { get; } = null;
            public EnvironmentInfoSO[]? environmentInfos { get; } = null;
            public IReadOnlyList<PreviewDifficultyBeatmapSet>? previewDifficultyBeatmapSets { get; } = null;

            public PlayerSensitivityFlag contentRating { get; } = PlayerSensitivityFlag.Unknown;

            public Task<Sprite> GetCoverImageAsync(CancellationToken cancellationToken) => Task.FromResult(BundleLoader.UnknownIcon);
        }

        private class DummyTableCellOwner : ITableCellOwner {
            public TableViewSelectionType selectionType { get; } = TableViewSelectionType.Single;
            public bool canSelectSelectedCell { get; } = false;
            public int numberOfCells { get; } = 0;
        }

        #endregion

        #region Prefab

        private static LevelListTableCell LevelTableCellPrefab =>
            _levelTableCellPrefab ? _levelTableCellPrefab! : _levelTableCellPrefab = Resources
                .FindObjectsOfTypeAll<LevelCollectionTableView>().First()
                .GetField<LevelListTableCell, LevelCollectionTableView>("_levelCellPrefab");

        private static LevelListTableCell? _levelTableCellPrefab;

        #endregion

        #region BlockPresses

        public bool BlockPresses {
            get => !Touchable.enabled;
            set => Touchable.enabled = !value;
        }

        #endregion

        #region Events

        public event Action? PressedEvent;

        #endregion

        #region UI Components

        private Touchable Touchable => _touchable ? _touchable :
            throw new UninitializedComponentException();

        [UIComponent("container"), UsedImplicitly]
        private Transform _container = null!;

        private static readonly NotSelectedPreviewBeatmapLevel defaultPreviewBeatmapLevel = new(false, "", "", "", "", new string[]{ }, new string[]{ }, 0, 0, 0, 0, 0, 0, PlayerSensitivityFlag.Unknown, null, null);
        private Touchable _touchable = null!;
        private LevelListTableCell _cell = null!;

        #endregion

        #region Init

        protected override void OnInitialize() {
            _cell = Instantiate(LevelTableCellPrefab, _container, false);
            _touchable = _cell.GetComponent<Touchable>();
            _cell.TableViewSetup(new DummyTableCellOwner(), 0);
            _cell.selectionDidChangeEvent += HandleSelectionDidChange;
            SetData(null);
        }

        #endregion

        #region SetData

        public void SetData(BeatmapLevel? previewBeatmapLevel) {
            previewBeatmapLevel ??= defaultPreviewBeatmapLevel;
            if (_cell == null) throw new UninitializedComponentException();
            _cell.SetDataFromLevelAsync(previewBeatmapLevel, false, false, false);
        }

        #endregion

        #region Callbacks

        private void HandleSelectionDidChange(SelectableCell cell, SelectableCell.TransitionType transition, object obj) {
            if (!cell.selected) return;
            PressedEvent?.Invoke();
            cell.SetSelected(false, SelectableCell.TransitionType.Instant, null, true);
        }

        #endregion
    }
}