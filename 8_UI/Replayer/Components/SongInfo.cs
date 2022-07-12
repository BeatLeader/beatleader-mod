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
    internal class SongInfo : ReeUIComponentV2
    {
        [UIValue("song-name")]
        private string songName
        {
            get => _songName;
            set
            {
                _songName = value;
                NotifyPropertyChanged(nameof(songName));
            }
        }

        [UIValue("song-author")]
        private string songAuthor
        {
            get => _songAuthor;
            set
            {
                _songAuthor = value;
                NotifyPropertyChanged(nameof(songAuthor));
            }
        }

        [UIComponent("container")]
        private RectTransform _container;

        [UIComponent("song-preview-image")] 
        private Image _songPreviewImage;

        [UIComponent("background")]
        private Image _background;

        [Inject] 
        private readonly PauseMenuManager.InitData _pauseMenuInitData;

        private string _songName;
        private string _songAuthor;

        public RectTransform Root => _container;

        protected override void OnInitialize()
        {
            HorizontalLayoutGroup horizontalGroup = _background.gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontalGroup.childForceExpandWidth = false;
            horizontalGroup.childAlignment = TextAnchor.MiddleLeft;

            IPreviewBeatmapLevel previewBeatmapLevel = _pauseMenuInitData.previewBeatmapLevel;
            songName = previewBeatmapLevel.songName;
            songAuthor = previewBeatmapLevel.levelAuthorName;
            Task.Run(() => LoadAndAssignImage(previewBeatmapLevel));
        }
        private async void LoadAndAssignImage(IPreviewBeatmapLevel previewBeatmapLevel)
        {
            _songPreviewImage.sprite = await previewBeatmapLevel.GetCoverImageAsync(new System.Threading.CancellationToken());
        }
    }
}
