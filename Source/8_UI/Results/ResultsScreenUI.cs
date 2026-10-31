using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BeatLeader.API;
using BeatLeader.Dance;
using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Components;
using JetBrains.Annotations;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.Results;
using BeatLeader.UI.MainMenu;
using Reactive.BeatSaber;
using TMPro;
using UnityEngine;
using Zenject;

namespace BeatLeader.ViewControllers {
    internal class ResultsScreenUI : ReeUIComponentV2 {
        #region Setup

        [UIValue("voting-button"), UsedImplicitly]
        private VotingButton _votingButton;

        [UIValue("replay-button")]
        private ReplayButton _replayButton;

        private SpecialEventResultsPanel _eventResultsPanel = null!;
        private DanceEventResultsPanel _danceResultsPanel = null!;
        private BeatLeaderNewsViewController? _newsViewController = null;

        private void Awake() {
            _newsViewController = Resources.FindObjectsOfTypeAll<BeatLeaderNewsViewController>().FirstOrDefault();
            if (_newsViewController != null) {
                _newsViewController.HappeningEvent.ValueChangedEvent += HandleEventStatusUpdated;
            }

            _eventResultsPanel = new();
            _eventResultsPanel.Use(transform);
            _eventResultsPanel.ContentTransform.localPosition = new(0f, 80f);

            _danceResultsPanel = new();
            _danceResultsPanel.Use(transform);
            _danceResultsPanel.ContentTransform.localPosition = new(0f, 80f);
            _danceResultsPanel.Enabled = false;

            _votingButton = ReeUIComponentV2.Instantiate<VotingButton>(transform, false);
            _replayButton = ReeUIComponentV2.Instantiate<ReplayButton>(transform, false);
            _replayButton.ReplayButtonClickedEvent += HandleReplayButtonClicked;
            LeaderboardEvents.VotingWasPressedEvent += PresentVotingModal;
        }

        protected override void OnDestroy() {
            _replayButton.ReplayButtonClickedEvent -= HandleReplayButtonClicked;
            LeaderboardEvents.VotingWasPressedEvent -= PresentVotingModal;
        }

        public void Setup(BeatmapLevel level, ResultsViewController results) {
            _replayButton.Interactable = ReplayManager.LastPlayedReplay is not null;

            var danceResult = DanceModeTracker.LastResult;
            var dread = danceResult != null ? DanceDreadController.LastSummary : null;
            SetupDanceScore(danceResult);
            SetupVerdict(dread);
            SetupTombstone(dread);
            SetupBanner(results, dread);
            WaitForEventPoints(dread is { Submitted: true } ? DanceEvent.Current : null);

            if (_newsViewController != null) {
                var evt = _newsViewController.HappeningEvent.Value;
                var hash = CustomLevelLoader.kCustomLevelPrefixId + evt?.today?.song.hash.ToUpper();

                if (level.levelID == hash) {
                    _newsViewController.RefreshEventsCache();
                    _eventResultsPanel.Enabled = true;
                } else {
                    _eventResultsPanel.Enabled = false;
                }
            } else {
                _eventResultsPanel.Enabled = false;
            }
        }

        #endregion

        #region Dance Score

        private string _danceText = "";

        [UIValue("dance-text"), UsedImplicitly]
        private string DanceText {
            get => _danceText;
            set {
                if (_danceText.Equals(value)) return;
                _danceText = value;
                NotifyPropertyChanged();
            }
        }

        private bool _danceTextActive;

        [UIValue("dance-text-active"), UsedImplicitly]
        private bool DanceTextActive {
            get => _danceTextActive;
            set {
                if (_danceTextActive.Equals(value)) return;
                _danceTextActive = value;
                NotifyPropertyChanged();
            }
        }

        private void SetupDanceScore(DanceResult? result) {
            DanceTextActive = result != null;
            if (result == null) return;
            if (!result.Valid) {
                DanceText = "Dance score: not enough play to judge";
                return;
            }
            var score = Math.Round(result.Score * 1_000_000).ToString("N0", CultureInfo.InvariantCulture);
            var voteShare = result.VoteShare.ToString("P0", CultureInfo.InvariantCulture);
            DanceText = $"Dance score <color=#FFD700>{score}</color>  <color=#AAAAAA>~{voteShare} of players would call this dancing</color>";
        }

        #endregion

        #region Verdict

        private string _verdictText = "";

        [UIValue("verdict-text"), UsedImplicitly]
        private string VerdictText {
            get => _verdictText;
            set {
                if (_verdictText.Equals(value)) return;
                _verdictText = value;
                NotifyPropertyChanged();
            }
        }

        private bool _verdictActive;

        [UIValue("verdict-active"), UsedImplicitly]
        private bool VerdictActive {
            get => _verdictActive;
            set {
                if (_verdictActive.Equals(value)) return;
                _verdictActive = value;
                NotifyPropertyChanged();
            }
        }

        private void SetupVerdict(DanceDreadSummary? dread) {
            VerdictActive = dread != null;
            if (dread == null) return;
            var haunted = dread.HauntedSeconds < 1f
                ? "never haunted"
                : $"haunted for {dread.HauntedSeconds.ToString("F0", CultureInfo.InvariantCulture)} s";
            VerdictText = $"<color=#C9A0FF><i>{FormatVerdict(dread)}</i></color>   <color=#AAAAAA>{haunted}</color>";
        }

        private static string FormatVerdict(DanceDreadSummary dread) {
            if (dread.Died) {
                return dread.Escapes > 0
                    ? "R.I.P. You fought back, but the ghosts won"
                    : "R.I.P. The ghosts took you without a fight";
            }
            if (dread.HauntedSeconds < 1f) return "Not a single ghost dared to come near";
            var hauntedShare = dread.PlayedSeconds > 0f ? dread.HauntedSeconds / dread.PlayedSeconds : 0f;
            return dread.Escapes > 0 && hauntedShare < 0.25f
                ? "The ghosts tried, you danced them away"
                : "You made it out... barely";
        }

        #endregion

        #region Tombstone

        private string _tombstoneText = "";

        [UIValue("tombstone-text"), UsedImplicitly]
        private string TombstoneText {
            get => _tombstoneText;
            set {
                if (_tombstoneText.Equals(value)) return;
                _tombstoneText = value;
                NotifyPropertyChanged();
            }
        }

        private bool _tombstoneActive;

        [UIValue("tombstone-active"), UsedImplicitly]
        private bool TombstoneActive {
            get => _tombstoneActive;
            set {
                if (_tombstoneActive.Equals(value)) return;
                _tombstoneActive = value;
                NotifyPropertyChanged();
            }
        }

        [UIComponent("tombstone-image"), UsedImplicitly]
        private HMUI.ImageView _tombstoneImage = null!;

        private static Sprite? _tombstoneSprite;

        private void SetupTombstone(DanceDreadSummary? dread) {
            TombstoneActive = dread is { Died: true };
            if (!TombstoneActive) return;
            _tombstoneSprite ??= DanceDreadApparitions.LoadSprite("tombstone.png", new Vector2(0.5f, 0.5f));
            _tombstoneImage.sprite = _tombstoneSprite;
            var death = TimeSpan.FromSeconds(dread!.DeathTime);
            TombstoneText = $"died {(int)death.TotalMinutes}:{death.Seconds:00}\n<size=80%>still for {dread.LongestStillSeconds:F0} s</size>";
        }

        #endregion

        #region Dance Event Points

        private PlatformEvent? _pointsEvent;

        private void WaitForEventPoints(PlatformEvent? evt) {
#pragma warning disable CS0618 // Type or member is obsolete
            UploadReplayRequest.StateChangedEvent -= HandleUploadStateChanged;
            _pointsEvent = evt;
            _danceResultsPanel.Enabled = evt != null;
            if (evt == null) return;
            _danceResultsPanel.SetEvent(evt);
            _danceResultsPanel.SetLoading();
            UploadReplayRequest.StateChangedEvent += HandleUploadStateChanged;
#pragma warning restore CS0618
        }

        private void HandleUploadStateChanged(WebRequests.IWebRequest<ScoreUploadResponse> instance, WebRequests.RequestState state, string? failReason) {
            if (state is not (WebRequests.RequestState.Finished or WebRequests.RequestState.Failed) || _pointsEvent == null) return;
#pragma warning disable CS0618
            UploadReplayRequest.StateChangedEvent -= HandleUploadStateChanged;
#pragma warning restore CS0618
            var response = state == WebRequests.RequestState.Finished ? instance.Result : null;
            switch (response?.Status) {
                case ScoreUploadStatus.Uploaded when response.Score != null:
                    DanceEventStatusRequest.Send(_pointsEvent.id, response.Score.id).StateChangedEvent += HandleEventStatus;
                    break;
                case ScoreUploadStatus.NonPB:
                    _danceResultsPanel.SetResult("Not your best on this map");
                    break;
                case ScoreUploadStatus.Attempt:
                    _danceResultsPanel.SetResult("Only cleared runs count for the event");
                    break;
                default:
                    _danceResultsPanel.SetResult("Couldn't send this run");
                    break;
            }
        }

        private void HandleEventStatus(WebRequests.IWebRequest<DanceEventStatus> instance, WebRequests.RequestState state, string? failReason) {
            switch (state) {
                case WebRequests.RequestState.Finished when instance.Result?.score != null:
                    var status = instance.Result;
                    var standing = status.player is { } player
                        ? $"#{player.rank} with {player.points.ToString("F1", CultureInfo.InvariantCulture)}"
                        : null;
                    _danceResultsPanel.SetResult(FormatEventResult(status), standing);
                    break;
                case WebRequests.RequestState.Finished:
                case WebRequests.RequestState.Failed:
                    _danceResultsPanel.SetResult("The event standings didn't load");
                    break;
            }
        }

        internal static string FormatEventResult(DanceEventStatus status) {
            var score = status.score!;
            if (!score.eligible) return "Can't count, too little was judged";
            var best = status.rules?.countedMaps ?? 5;
            var counted = score.counted ? $"in your best {best}" : $"not in your best {best}";
            return $"<color=#FF9A1F>{score.points.ToString("F1", CultureInfo.InvariantCulture)} points</color>, {counted}";
        }

        #endregion

        #region Dance or Die Banner

        private readonly List<(TMP_Text text, string original, Behaviour? localizer)> _overriddenBannerTexts = new();

        private void SetupBanner(ResultsViewController results, DanceDreadSummary? dread) {
            RestoreBanner();
            if (dread == null) return;
            OverrideBanner(results._failedBannerGo, "DIED");
            OverrideBanner(results._clearedBannerGo, "SURVIVED");
        }

        private void OverrideBanner(GameObject? banner, string text) {
            if (banner == null) return;
            foreach (var label in banner.GetComponentsInChildren<TMP_Text>(true)) {
                var localizer = label.GetComponents<Behaviour>().FirstOrDefault(x => x.GetType().Name.StartsWith("Localized"));
                if (localizer != null) localizer.enabled = false;
                _overriddenBannerTexts.Add((label, label.text, localizer));
                label.text = text;
            }
        }

        private void RestoreBanner() {
            foreach (var (text, original, localizer) in _overriddenBannerTexts) {
                if (text == null) continue;
                text.text = original;
                if (localizer != null) localizer.enabled = true;
            }
            _overriddenBannerTexts.Clear();
        }

        #endregion

        #region Callbacks

        private void HandleEventStatusUpdated(PlatformEventStatus? status) {
            if (status != null) {
                _eventResultsPanel.SetEvent(status);
                _eventResultsPanel.Enabled = true;
            } else {
                _eventResultsPanel.Enabled = false;
            }
        }

        private void HandleReplayButtonClicked() {
            ReplayerMenuLoader.Instance!.StartLastReplayAsync().RunCatching();
        }

        private void PresentVotingModal() {
            ReeModalSystem.OpenModal<VotingPanel>(transform, 0);
        }

        #endregion
    }
}