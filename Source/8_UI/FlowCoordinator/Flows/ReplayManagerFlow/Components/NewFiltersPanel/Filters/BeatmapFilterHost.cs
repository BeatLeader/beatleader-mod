using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using HMUI;
using UnityEngine;
using Dummy = BeatLeader.UI.Reactive.Components.Dummy;

namespace BeatLeader.UI.Hub {
    internal class BeatmapFilterHost {
        public BeatmapFilterHost() {
            _beatmapFilter = new();
            _characteristicFilter = new(_beatmapFilter);
            _difficultyFilter = new(_beatmapFilter, _characteristicFilter);
            Filters = new ListFiltersPanel<IReplayHeader>.IFilter[] {
                _beatmapFilter,
                _characteristicFilter,
                _difficultyFilter
            };
            Init();
        }

        private class BeatmapFilter : ReactiveComponent, ListFiltersPanel<IReplayHeader>.IFilter {
            #region Filter

            public IEnumerable<ListFiltersPanel<IReplayHeader>.IFilter>? DependsOn => null;
            public string FilterName => "Beatmap Filter";

            public event Action? FilterUpdatedEvent;

            private IPreviewBeatmapLevel? _beatmapLevel;

            public bool Matches(IReplayHeader value) {
                var levelId = _beatmapLevel?.levelID;
                return levelId == null || levelId.Replace("custom_level_", "") == value.ReplayInfo.SongHash;
            }

            #endregion

            #region Setup

            public event Action<IPreviewBeatmapLevel?>? BeatmapSelectedEvent {
                add => _beatmapSelector.BeatmapSelectedEvent += value;
                remove => _beatmapSelector.BeatmapSelectedEvent -= value;
            }

            public void Setup(ViewController viewController) {
                _beatmapSelector.Setup(viewController);
            }

            protected override void OnInitialize() {
                _beatmapSelector.Setup(Content.GetComponentInParent<ViewController>());
                _beatmapSelector.BeatmapSelectedEvent += HandleBeatmapSelected;
                this.AsFlexItem(size: new() { x = 52f, y = 18f });
            }

            #endregion

            #region Construct

            private BeatmapSelector _beatmapSelector = null!;

            protected override GameObject Construct() {
                return new Dummy {
                    Children = {
                        new ReeWrapperV2<BeatmapSelector>()
                            .AsFlexItem(grow: 1f)
                            .BindRee(ref _beatmapSelector)
                    }
                }.AsFlexGroup(padding: 1f).Use();
            }

            #endregion

            #region Callbacks

            private void HandleBeatmapSelected(IPreviewBeatmapLevel? level) {
                _beatmapLevel = level;
                FilterUpdatedEvent?.Invoke();
            }

            #endregion
        }

        private class CharacteristicFilter : ReactiveComponent, ListFiltersPanel<IReplayHeader>.IFilter {
            public CharacteristicFilter(BeatmapFilter beatmapFilter) {
                DependsOn = new[] { beatmapFilter };
            }

            #region Filter

            public IEnumerable<ListFiltersPanel<IReplayHeader>.IFilter>? DependsOn { get; }
            public string FilterName => "Characteristic Filter";

            public event Action? FilterUpdatedEvent;

            private BeatmapCharacteristicSO? _characteristic;

            public bool Matches(IReplayHeader value) {
                var characteristic = _characteristic?.serializedName;
                return characteristic == null || value.ReplayInfo.SongMode == characteristic;
            }

            #endregion

            #region Setup

            public event Action<BeatmapCharacteristicSO>? CharacteristicSelectedEvent {
                add => _characteristicPanel.CharacteristicSelectedEvent += value;
                remove => _characteristicPanel.CharacteristicSelectedEvent -= value;
            }

            public void SetBeatmapLevel(IPreviewBeatmapLevel? level) {
                var sets = level?.previewDifficultyBeatmapSets
                    .Select(
                        static x => new DifficultyBeatmapSet(
                            x.beatmapCharacteristic,
                            Array.Empty<IDifficultyBeatmap>()
                        )
                    )
                    .ToArray();
                _characteristicPanel.SetData(sets);
            }

            protected override void OnInitialize() {
                _characteristicPanel.SetData(null);
                _characteristicPanel.CharacteristicSelectedEvent += HandleBeatmapCharacteristicSelected;
                this.AsFlexItem(size: new() { x = 52f, y = 8f });
            }

            #endregion

            #region Construct

            private BeatmapCharacteristicPanel _characteristicPanel = null!;

            protected override GameObject Construct() {
                return new Dummy {
                    Children = {
                        new ReeWrapperV2<BeatmapCharacteristicPanel>()
                            .AsFlexItem(grow: 1f)
                            .BindRee(ref _characteristicPanel)
                    }
                }.AsFlexGroup(padding: 1f).Use();
            }

            #endregion

            #region Callbacks

            private void HandleBeatmapCharacteristicSelected(BeatmapCharacteristicSO characteristic) {
                _characteristic = characteristic;
                FilterUpdatedEvent?.Invoke();
            }

            #endregion
        }

        private class DifficultyFilter : ReactiveComponent, ListFiltersPanel<IReplayHeader>.IFilter {
            public DifficultyFilter(
                BeatmapFilter beatmapFilter,
                CharacteristicFilter characteristicFilter
            ) {
                DependsOn = new ListFiltersPanel<IReplayHeader>.IFilter[] { beatmapFilter, characteristicFilter };
            }

            #region Filter

            public IEnumerable<ListFiltersPanel<IReplayHeader>.IFilter>? DependsOn { get; }
            public string FilterName => "Difficulty Filter";

            public event Action? FilterUpdatedEvent;

            private BeatmapDifficulty? _beatmapDifficulty;

            public bool Matches(IReplayHeader value) {
                var diff = _beatmapDifficulty;
                return !diff.HasValue || value.ReplayInfo.SongDifficulty == diff.Value.ToString();
            }

            #endregion

            #region Setup

            private IPreviewBeatmapLevel? _beatmapLevel;
            private BeatmapCharacteristicSO? _characteristic;

            public void SetBeatmapLevel(IPreviewBeatmapLevel? level) {
                _beatmapLevel = level;
                if (_characteristic != null) {
                    SetCharacteristicCharacteristic(_characteristic);
                }
            }

            public void SetCharacteristicCharacteristic(
                BeatmapCharacteristicSO characteristic
            ) {
                _characteristic = characteristic;
                var sets = _beatmapLevel?.previewDifficultyBeatmapSets
                    .FirstOrDefault(x => x.beatmapCharacteristic == characteristic)?
                    .beatmapDifficulties.Select(
                        x => new CustomDifficultyBeatmap(
                            null,
                            null,
                            x,
                            0,
                            0,
                            0,
                            0,
                            null,
                            null
                        )
                    ).ToArray();
                _difficultyPanel.SetData(sets);
            }

            protected override void OnInitialize() {
                _difficultyPanel.SetData(null);
                _difficultyPanel.DifficultySelectedEvent += HandleDifficultySelected;
                this.AsFlexItem(size: new() { x = 52f, y = 8f });
            }

            #endregion

            #region Construct

            private BeatmapDifficultyPanel _difficultyPanel = null!;

            protected override GameObject Construct() {
                return new Dummy {
                    Children = {
                        new ReeWrapperV2<BeatmapDifficultyPanel>()
                            .AsFlexItem(grow: 1f)
                            .BindRee(ref _difficultyPanel)
                    }
                }.AsFlexGroup(padding: 1f).Use();
            }

            #endregion

            #region Callbacks

            private void HandleDifficultySelected(BeatmapDifficulty difficulty) {
                _beatmapDifficulty = difficulty;
                FilterUpdatedEvent?.Invoke();
            }

            #endregion
        }

        public IEnumerable<ListFiltersPanel<IReplayHeader>.IFilter> Filters { get; }

        private readonly BeatmapFilter _beatmapFilter;
        private readonly CharacteristicFilter _characteristicFilter;
        private readonly DifficultyFilter _difficultyFilter;

        public void Setup(ViewController viewController) {
            _beatmapFilter.Setup(viewController);
        }

        private void Init() {
            _beatmapFilter.BeatmapSelectedEvent += _characteristicFilter.SetBeatmapLevel;
            _beatmapFilter.BeatmapSelectedEvent += _difficultyFilter.SetBeatmapLevel;
            _characteristicFilter.CharacteristicSelectedEvent += _difficultyFilter.SetCharacteristicCharacteristic;
        }
    }
}