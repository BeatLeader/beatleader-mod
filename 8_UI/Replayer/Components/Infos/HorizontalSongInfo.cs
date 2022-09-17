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
        [Inject] private readonly Models.ReplayLaunchData _replayData;

        [UIComponent("song-preview-image")] private Image _songPreviewImage;
        [UIComponent("container")] private RectTransform _container;
        [UIComponent("wrapper")] private RectTransform _wrapper;

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
        protected override HideMode Mode => HideMode.Custom;
        protected override Action<bool> VisibilityController => ChangeVisibility;
        public override string Name => "SongInfo";

        private string _songName;
        private string _songAuthor;

        protected override void OnInitialize()
        {
            IPreviewBeatmapLevel previewBeatmapLevel = _replayData.difficultyBeatmap.level;
            songName = previewBeatmapLevel.songName;
            songAuthor = previewBeatmapLevel.levelAuthorName;
            LoadAndAssignImage(previewBeatmapLevel);
        }
        private async void LoadAndAssignImage(IPreviewBeatmapLevel previewBeatmapLevel)
        {
            _songPreviewImage.sprite = await previewBeatmapLevel.GetCoverImageAsync(new System.Threading.CancellationToken());
        }

        //i really, really hate it, but i was forced because of layout problems on disabled objects
        private void ChangeVisibility(bool visible)
        {
            ContainerCanvasGroup.alpha = visible ? 1 : 0;
            ContainerRect.SetSiblingIndex(visible ? 0 : 1);
        }
    }
}
