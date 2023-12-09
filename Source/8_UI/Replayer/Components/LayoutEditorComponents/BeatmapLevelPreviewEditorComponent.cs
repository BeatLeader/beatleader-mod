using UnityEngine;

namespace BeatLeader.Components {
    internal class BeatmapLevelPreviewEditorComponent : LayoutEditorComponent<BeatmapLevelPreviewEditorComponent> {
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
        
        protected override GameObject ConstructInternal(Transform parent) {
            _beatmapLevelPreview = BeatmapLevelPreview.Instantiate(parent);
            return _beatmapLevelPreview.Content!;
        }

        #endregion
    }
}