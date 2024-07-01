using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using HMUI;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class BeatmapFilterPanel : ReactiveComponent, IPanelListFilter<IReplayHeaderBase> {
        #region Filter

        public IEnumerable<IPanelListFilter<IReplayHeaderBase>>? DependsOn => null;
        public string FilterName => "Beatmap Filter";
        public IPreviewBeatmapLevel? BeatmapLevel { get; private set; }

        public event Action? FilterUpdatedEvent;

        public bool Matches(IReplayHeaderBase value) {
            var levelId = BeatmapLevel?.levelID;
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

        private BeatLeader.Components.BeatmapSelector _beatmapSelector = null!;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new ReeWrapperV2<BeatLeader.Components.BeatmapSelector>()
                        .AsFlexItem(grow: 1f)
                        .BindRee(ref _beatmapSelector)
                }
            }.AsFlexGroup(padding: 1f).Use();
        }

        #endregion

        #region Callbacks

        private void HandleBeatmapSelected(IPreviewBeatmapLevel? level) {
            BeatmapLevel = level;
            FilterUpdatedEvent?.Invoke();
        }

        #endregion
    }
}