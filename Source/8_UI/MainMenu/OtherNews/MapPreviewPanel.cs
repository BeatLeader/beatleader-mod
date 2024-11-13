using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.UI.MainMenu {
    internal class MapPreviewPanel : ReeUIComponentV2 {
        #region UI Components

        [UIValue("preview"), UsedImplicitly] private FeaturedPreviewPanel _previewPanel = null!;

        private void Awake() {
            _previewPanel = Instantiate<FeaturedPreviewPanel>(transform);
        }

        #endregion

        #region Setup

        private MapDetail? _mapDetail;

        public void Setup(MapData mapData) {
            _mapDetail = mapData.song;
            _previewPanel.Setup(_mapDetail.coverImage, _mapDetail.name, _mapDetail.mapper);
        }

        #endregion

        #region Events

        [UIAction("downloadPressed"), UsedImplicitly]
        private async void HandleDownloadButtonClicked() {
            if (_mapDetail == null) return;
            MapDownloadDialog.OpenSongOrDownloadDialog(_mapDetail, Content.transform);
        }

        #endregion
    }
}