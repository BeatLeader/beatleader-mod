using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class BeatmapPreviewCell : ReeUIComponentV2 {
        #region Dummies

        private class NotSelectedPreviewBeatmapLevel : BeatmapLevel {
            private class PreviewMediaData : IPreviewMediaData {
                public Task<Sprite> GetCoverSpriteAsync(CancellationToken cancellationToken) => Task.FromResult(BundleLoader.UnknownIcon);

                public Task<AudioClip> GetPreviewAudioClip(CancellationToken cancellationToken) => throw new NotImplementedException();

                public void UnloadPreviewAudioClip() { }
            }

            public NotSelectedPreviewBeatmapLevel() :
                base(
                    false,
                    string.Empty,
                    "Click to select",
                    string.Empty,
                    "Unknown",
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    0,
                    0,
                    0,
                    -1,
                    -1,
                    0,
                    PlayerSensitivityFlag.Safe,
                    new PreviewMediaData(),
                    null
                ) { }
        }

        private class DummyTableCellOwner : ITableCellOwner {
            public TableViewSelectionType selectionType { get; } = TableViewSelectionType.Single;
            public bool canSelectSelectedCell { get; } = false;
            public int numberOfCells { get; } = 0;
        }

        #endregion

        #region Prefab

        private static LevelListTableCell LevelTableCellPrefab => _levelTableCellPrefab ? _levelTableCellPrefab! : _levelTableCellPrefab = Resources
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

        private static readonly NotSelectedPreviewBeatmapLevel defaultPreviewBeatmapLevel = new();
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