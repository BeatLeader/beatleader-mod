using BeatLeader.Components;
using BeatLeader.UI.Reactive;
using Reactive;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class BeatmapLevelPreviewEditorComponent : LayoutEditorComponent {
        #region LayoutComponent

        public override string ComponentName => "Beatmap Preview";
        protected override Vector2 MinSize { get; } = new(30, 24);
        protected override Vector2 MaxSize { get; } = new(int.MaxValue, 24);

        #endregion

        #region Setup

        private BeatmapLevelPreview _beatmapLevelPreview = null!;

        public void SetBeatmapLevel(IPreviewBeatmapLevel level) {
            _beatmapLevelPreview.SetBeatmapLevel(level);
        }

        protected override void ConstructInternal(Transform parent) {
            _beatmapLevelPreview = new BeatmapLevelPreview();
            _beatmapLevelPreview.WithRectExpand().Use(parent);
        }

        #endregion
    }
}