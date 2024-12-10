using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BeatLeader.API.Methods;
using BeatLeader.Models;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.UI.MainMenu {
    internal class TreeMapsPanel : ReeUIComponentV2 {

        public event Action<bool> ShouldDisplayOtherMapsEvent;

        [UIValue("header"), UsedImplicitly] protected NewsHeader header = null!;
        [UIObject("loading-indicator"), UsedImplicitly] private GameObject _loadingIndicator = null!;

        [UIComponent("image"), UsedImplicitly] private ImageView _image = null!;
        [UIComponent("ornament-tease"), UsedImplicitly] private ImageView _ornamentTease = null!;

        [UIComponent("date-text"), UsedImplicitly] private TMP_Text _dateText = null!;
        [UIComponent("top-text"), UsedImplicitly] private TMP_Text _topText = null!;
        [UIComponent("bottom-text"), UsedImplicitly] private TMP_Text _bottomText = null!;
        [UIComponent("time-text"), UsedImplicitly] private TMP_Text _timeText = null!;

        [UIObject("details-container"), UsedImplicitly] 
        private GameObject _detailsContainer = null!;

        private TreeStatus currentStatus;

        protected virtual void Awake() {
            header = Instantiate<NewsHeader>(transform);
        }

        protected override void OnInitialize() {
            base.OnInitialize();

            header.Setup("Project Tree");
            header.SetBackgroundColor(new Color(0.027f, 0.43f, 0));
            _image.material = BundleLoader.RoundTexture10Material;
            _image.__Refresh();

            _topText.overflowMode = TextOverflowModes.Ellipsis;
            _bottomText.overflowMode = TextOverflowModes.Ellipsis;
        }

        protected override void OnRootStateChange(bool active) {
            if (active) {
                TreeMapRequest.SendRequest();
                TreeMapRequest.AddStateListener(OnRequestStateChanged);
            }
        }

        protected override void OnDispose() {
            TreeMapRequest.RemoveStateListener(OnRequestStateChanged);
        }

        protected void UpdateTimeText(string text) {
            _timeText.SetText(text);
        }

        protected async Task UpdateUI(TreeStatus status) {
            var todaysMap = status.today.song;
            _topText.SetText(todaysMap.name);
            _bottomText.SetText(todaysMap.mapper);

            _dateText.SetText($"December {status.today.startTime.AsUnixTime().Day}th");
            ScheduleBottomText();

            if (!string.IsNullOrEmpty(todaysMap.coverImage)) {
                await _image.SetImageAsync(todaysMap.coverImage);
            }

            await _ornamentTease.SetImageAsync($"https://cdn.assets.beatleader.xyz/project_tree_ornament_preview_1.png");
        }

        private string FormatRemainingTime(TimeSpan span) {
            if (span.TotalHours >= 1) {
                return $"{(int)span.TotalHours} hour{((int)span.TotalHours > 1 ? "s" : "")} left";
            }
            if (span.TotalMinutes >= 1) {
                return $"{(int)span.TotalMinutes} minute{((int)span.TotalMinutes > 1 ? "s" : "")} left";
            }
            return $"{(int)span.TotalSeconds} second{((int)span.TotalSeconds > 1 ? "s" : "")} left";
        }

        private string ScheduleBottomText() {
            var timeSpan = FormatUtils.GetRelativeTime(currentStatus.today.startTime + 60 * 60 * 24);

            var remainingTime = -timeSpan;
            string bottomText = $"{FormatRemainingTime(-timeSpan)}";
            UpdateTimeText(bottomText);

            TimeSpan updateDelay;
            if (remainingTime.TotalHours >= 1) {
                updateDelay = TimeSpan.FromHours(Math.Ceiling(remainingTime.TotalHours)) - remainingTime;
            }
            else if (remainingTime.TotalMinutes >= 1) {
                updateDelay = TimeSpan.FromMinutes(Math.Ceiling(remainingTime.TotalMinutes)) - remainingTime;
            }
            else {
                updateDelay = TimeSpan.FromSeconds(1);
            }

            // Schedule update
            StartCoroutine(UpdateAfterDelay(updateDelay));

            return bottomText;
        }

        private IEnumerator UpdateAfterDelay(TimeSpan delay) {
            yield return new WaitForSeconds((float)delay.TotalSeconds);
            string bottomText = ScheduleBottomText();
            UpdateTimeText(bottomText);
        }

        private void OnRequestStateChanged(API.RequestState state, TreeStatus result, string failReason) {
            switch (state) {
                case API.RequestState.Uninitialized:
                case API.RequestState.Started:
                    default: {
                        _loadingIndicator.SetActive(true);
                        _detailsContainer.gameObject.SetActive(false);
                        break;
                    }
                    case API.RequestState.Failed:
                        _loadingIndicator.SetActive(false);
                        ShouldDisplayOtherMapsEvent.Invoke(true);

                        break;
                    case API.RequestState.Finished: {
                        _loadingIndicator.SetActive(false);
                        _detailsContainer.gameObject.SetActive(true);
                        if (result.today.score != null) {
                            ShouldDisplayOtherMapsEvent.Invoke(true);
                        } else {
                            currentStatus = result;
                            UpdateUI(result);
                        }
                        break;
                    }
            }
        }

        [UIAction("OnButtonPressed"), UsedImplicitly]
        void OnPlayButtonClick() {
            MapDownloadDialog.OpenSongOrDownloadDialog(currentStatus.today.song, Content.transform);
        }
    }
}
