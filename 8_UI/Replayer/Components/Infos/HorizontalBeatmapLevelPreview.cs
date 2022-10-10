using System;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine.UI;
using UnityEngine;
using System.Threading;

namespace BeatLeader.Components
{
    internal class HorizontalBeatmapLevelPreview : EditableElement
    {
        #region Components

        [UIComponent("song-preview-image")] private readonly Image _songPreviewImage;
        [UIComponent("container")] private readonly RectTransform _container;
        [UIComponent("wrapper")] private readonly RectTransform _wrapper;

        #endregion

        #region Name, Author

        [UIValue("song-name")] public string SongName
        {
            get => _songName;
            private set
            {
                _songName = value;
                NotifyPropertyChanged(nameof(SongName));
            }
        }
        [UIValue("song-author")] public string SongAuthor
        {
            get => _songAuthor;
            private set
            {
                _songAuthor = value;
                NotifyPropertyChanged(nameof(SongAuthor));
            }
        }

        #endregion

        #region Editable

        protected override RectTransform ContainerRect => _container;
        protected override RectTransform WrapperRect => _wrapper;
        protected override HideMode Mode => HideMode.Custom;
        protected override Action<bool> VisibilityController => ChangeVisibility;
        public override string Name => "Song Info";

        #endregion

        #region Setup

        private string _songName;
        private string _songAuthor;

        public void SetBeatmapLevel(IPreviewBeatmapLevel level)
        {
            SongName = level.songName;
            SongAuthor = level.levelAuthorName;
            LoadAndAssignImage(level);
        }
        private async void LoadAndAssignImage(IPreviewBeatmapLevel level)
        {
            var token = new CancellationTokenSource().Token;
            _songPreviewImage.sprite = await level.GetCoverImageAsync(token);
        }

        //i really, really hate it, but i was forced because of layout problems on disabled objects
        private void ChangeVisibility(bool visible)
        {
            ContainerCanvasGroup.alpha = visible ? 1 : 0;
            ContainerRect.SetSiblingIndex(visible ? 0 : 1);
        }

        #endregion
    }
}
