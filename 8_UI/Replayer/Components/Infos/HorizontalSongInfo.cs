using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using UnityEngine.UI;
using UnityEngine;
using Zenject;

namespace BeatLeader.Components
{
    internal class HorizontalSongInfo : EditableElement
    {
        [Inject] private readonly PauseMenuManager.InitData _pauseMenuInitData;

        [UIComponent("song-preview-image")] private Image _songPreviewImage;
        [UIComponent("container")] private RectTransform _container;
        [UIComponent("wrapper")] private RectTransform _wrapper;
        [UIComponent("song-name-text")] private RectTransform _songNameText;
        [UIComponent("song-author-text")] private RectTransform _songAuthorText;

        [UIValue("song-name")] private string songName
        {
            get => _songName;
            set
            {
                _songName = value;
                NotifyPropertyChanged(nameof(songName));
            }
        }
        [UIValue("song-author")] private string songAuthor
        {
            get => _songAuthor;
            set
            {
                _songAuthor = value;
                NotifyPropertyChanged(nameof(songAuthor));
            }
        }

        protected override RectTransform ContainerRect => _container;
        protected override RectTransform WrapperRect => _wrapper;
        protected override HideMode Mode => HideMode.Hierarchy;
        public override string Name => "SongInfo";

        private string _songName;
        private string _songAuthor;

        protected override void OnInitialize()
        {
            IPreviewBeatmapLevel previewBeatmapLevel = _pauseMenuInitData.previewBeatmapLevel;
            songName = previewBeatmapLevel.songName;
            songAuthor = previewBeatmapLevel.levelAuthorName;
            LoadAndAssignImage(previewBeatmapLevel);
        }
        private async void LoadAndAssignImage(IPreviewBeatmapLevel previewBeatmapLevel)
        {
            _songPreviewImage.sprite = await previewBeatmapLevel.GetCoverImageAsync(new System.Threading.CancellationToken());
        }
        private void TextOutOfBounds()
        {
            Debug.Log("text: " + _songNameText.sizeDelta.x);
            Debug.Log("container: " + _container.sizeDelta.x);
            Debug.Log("picture: " + _songPreviewImage.rectTransform.sizeDelta.x);
            Debug.Log("difference: " + (_container.sizeDelta.x - _songPreviewImage.rectTransform.sizeDelta.x));
        }
    }
}
