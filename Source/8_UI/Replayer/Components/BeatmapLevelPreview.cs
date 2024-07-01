using BeatSaberMarkupLanguage.Attributes;
using UnityEngine.UI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class BeatmapLevelPreview : ReeUIComponentV3<BeatmapLevelPreview> {
        #region Components

        [UIComponent("song-preview-image"), UsedImplicitly]
        private Image _songPreviewImage = null!;

        [UIComponent("flex-group"), UsedImplicitly]
        private FlexContainer _flexGroup = null!;
        
        #endregion

        #region Name, Author

        [UIValue("song-name"), UsedImplicitly]
        public string? SongName {
            get => _songName;
            private set {
                _songName = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("song-author"), UsedImplicitly]
        public string? SongAuthor {
            get => _songAuthor;
            private set {
                _songAuthor = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Setup

        private string? _songName;
        private string? _songAuthor;

        public void SetBeatmapLevel(IPreviewBeatmapLevel level) {
            SongName = level.songName;
            SongAuthor = level.levelAuthorName;
            LoadAndAssignImage(level);
        }

        private async void LoadAndAssignImage(IPreviewBeatmapLevel level) {
            _songPreviewImage.sprite = await level.GetCoverImageAsync(default);
        }

        protected override void OnInitialize() {
            _songPreviewImage.material = BundleLoader.RoundTextureMaterial;
            var flexItem = _songPreviewImage.gameObject.AddComponent<FlexItem>();
            flexItem.MinSize = Vector2.one * 20;
            flexItem.MaxSize = Vector2.one * 20;
        }

        protected override void OnRectDimensionsChange() {
            _flexGroup.SetDirty();
        }

        #endregion
    }
}