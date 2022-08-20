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
    internal class SongInfo : EditableElement
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
        private bool TextOutOfBounds(RectTransform text)
        {
            return text.sizeDelta.x > _container.sizeDelta.x - _songPreviewImage.rectTransform.sizeDelta.x;
        }
    }
}
