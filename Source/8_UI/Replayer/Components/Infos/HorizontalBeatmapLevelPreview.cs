using BeatSaberMarkupLanguage.Attributes;
using UnityEngine.UI;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class HorizontalBeatmapLevelPreview : LayoutEditorComponent<HorizontalBeatmapLevelPreview> {
        #region Components

        [UIComponent("song-preview-image"), UsedImplicitly]
        private Image _songPreviewImage = null!;

        #endregion

        #region Name, Author

        [UIValue("song-name")]
        public string? SongName {
            get => _songName;
            private set {
                _songName = value;
                NotifyPropertyChanged();
            }
        }
        [UIValue("song-author")]
        public string? SongAuthor {
            get => _songAuthor;
            private set {
                _songAuthor = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region LayoutComponent

        public override string ComponentName => "Beatmap Preview";
        protected override Vector2 MinSize { get; } = new(0, 24);
        protected override Vector2 MaxSize { get; } = new(int.MaxValue, 24);

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
            var token = new CancellationTokenSource().Token;
            _songPreviewImage.sprite = await level.GetCoverImageAsync(token);
        }

        #endregion
    }
}