using BeatSaberMarkupLanguage.Attributes;
using UnityEngine.UI;
using System.Threading;
using BeatLeader.Models;

namespace BeatLeader.Components {
    internal class HorizontalBeatmapLevelPreview : EditableElement {
        #region Components

        [UIComponent("song-preview-image")]
        private readonly Image _songPreviewImage;

        #endregion

        #region Name, Author

        [UIValue("song-name")]
        public string SongName {
            get => _songName;
            private set {
                _songName = value;
                NotifyPropertyChanged(nameof(SongName));
            }
        }
        [UIValue("song-author")]
        public string SongAuthor {
            get => _songAuthor;
            private set {
                _songAuthor = value;
                NotifyPropertyChanged(nameof(SongAuthor));
            }
        }

        #endregion

        #region Editable

        public override string Name { get; } = "Beatmap Preview";

        public override LayoutMap LayoutMap { get; } = new() {
            layer = 3,
            position = new(0f, 1f),
            anchor = new(0f, 1f)
        };

        #endregion

        #region Setup

        private string _songName;
        private string _songAuthor;

        public void SetBeatmapLevel(BeatmapLevel level) {
            SongName = level.songName;
            SongAuthor = level.songAuthorName;
            LoadAndAssignImage(level);
        }
        private async void LoadAndAssignImage(BeatmapLevel level) {
            _songPreviewImage.sprite = await level.previewMediaData.GetCoverSpriteAsync();
        }

        #endregion
    }
}
