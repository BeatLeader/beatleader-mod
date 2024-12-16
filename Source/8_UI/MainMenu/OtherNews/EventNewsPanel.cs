using System;
using System.Collections;
using System.Collections.Generic;
using BeatLeader.API.Methods;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class EventNewsPanel : AbstractNewsPanel {
        #region UI Components

        [UIComponent("empty-text"), UsedImplicitly] private TextMeshProUGUI _emptyText = null!;

        [UIObject("loading-indicator"), UsedImplicitly] private GameObject _loadingIndicator = null!;

        protected virtual void Awake() {
            header = Instantiate<NewsHeader>(transform);
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            header.Setup("BeatLeader Events");
        }

        protected override void OnRootStateChange(bool active) {
            if (active) {
                PlatformEventsRequest.SendRequest();
                PlatformEventsRequest.AddStateListener(OnRequestStateChanged);
            } else {
                PlatformEventsRequest.RemoveStateListener(OnRequestStateChanged);
            }
        }

        #endregion

        #region Events

        private void OnRequestStateChanged(API.RequestState state, Paged<PlatformEvent> result, string failReason) {
            switch (state) {
                case API.RequestState.Uninitialized:
                case API.RequestState.Started:
                default: {
                    _loadingIndicator.SetActive(true);
                    _emptyText.gameObject.SetActive(false);
                    DisposeList();
                    break;
                }
                case API.RequestState.Failed:
                    _loadingIndicator.SetActive(false);
                    _emptyText.gameObject.SetActive(true);
                    _emptyText.text = "<color=#ff8888>Failed to load";
                    DisposeList();
                    break;
                case API.RequestState.Finished: {
                    _loadingIndicator.SetActive(false);

                    if (result.data is { Count: > 0 }) {
                        _emptyText.gameObject.SetActive(false);
                        PresentList(result.data);
                    } else {
                        _emptyText.gameObject.SetActive(true);
                        _emptyText.text = "There is no events";
                        DisposeList();
                    }

                    break;
                }
            }
        }

        #endregion

        #region List

        private readonly List<FeaturedPreviewPanel> _list = new List<FeaturedPreviewPanel>();

        private void PresentList(IEnumerable<PlatformEvent> items) {
            DisposeList();

            foreach (var item in items) {
                var component = Instantiate<FeaturedPreviewPanel>(transform);
                component.ManualInit(mainContainer);
                SetupFeaturePreview(component, item);
                _list.Add(component);
            }

            MarkScrollbarDirty();
        }

        private string FormatRemainingTime(TimeSpan span) {
            if (span.TotalDays >= 1) {
                return $"Ongoing! {(int)span.TotalDays} day{((int)span.TotalDays > 1 ? "s" : "")} left";
            }
            if (span.TotalHours >= 1) {
                return $"Ongoing! {(int)span.TotalHours} hour{((int)span.TotalHours > 1 ? "s" : "")} left";
            }
            if (span.TotalMinutes >= 1) {
                return $"Ongoing! {(int)span.TotalMinutes} minute{((int)span.TotalMinutes > 1 ? "s" : "")} left";
            }
            return $"Ongoing! {(int)span.TotalSeconds} second{((int)span.TotalSeconds > 1 ? "s" : "")} left";
        }

        private string ScheduleBottomText(FeaturedPreviewPanel panel, PlatformEvent item) {
            string bottomText;
            var timeSpan = FormatUtils.GetRelativeTime(item.endDate);
            var remainingTime = timeSpan;
            if (timeSpan < TimeSpan.Zero) {
                bottomText = $"<color=#88FF88>{FormatRemainingTime(-timeSpan)}";
                
                // Calculate time until next significant change
                remainingTime = -timeSpan;
                
            } else {
                var date = FormatUtils.GetRelativeTimeString(timeSpan, false);
                bottomText = $"<color=#884444>Ended {date}";
            }

            TimeSpan updateDelay;
            if (remainingTime.TotalDays >= 1) {
                updateDelay = TimeSpan.FromDays(Math.Ceiling(remainingTime.TotalDays)) - remainingTime;
            }
            else if (remainingTime.TotalHours >= 1) {
                updateDelay = TimeSpan.FromHours(Math.Ceiling(remainingTime.TotalHours)) - remainingTime;
            }
            else if (remainingTime.TotalMinutes >= 1) {
                updateDelay = TimeSpan.FromMinutes(Math.Ceiling(remainingTime.TotalMinutes)) - remainingTime;
            }
            else {
                updateDelay = TimeSpan.FromSeconds(1);
            }

            // Schedule update
            StartCoroutine(UpdateAfterDelay(updateDelay, panel, item));

            return bottomText;
        }

        private void SetupFeaturePreview(FeaturedPreviewPanel panel, PlatformEvent item) {
            
            string bottomText = ScheduleBottomText(panel, item);
            panel.Setup(item.image, item.name, bottomText, "Details", ButtonAction);
            return;

            void ButtonAction() {
                ReeModalSystem.OpenModal<EventDetailsDialog>(Content.transform, item);
            }
        }

        private IEnumerator UpdateAfterDelay(TimeSpan delay, FeaturedPreviewPanel panel, PlatformEvent item) {
            yield return new WaitForSeconds((float)delay.TotalSeconds);
            string bottomText = ScheduleBottomText(panel, item);
            panel.UpdateBottomText(bottomText);
        }

        private void DisposeList() {
            foreach (var post in _list) {
                Destroy(post.gameObject);
            }

            _list.Clear();
            MarkScrollbarDirty();
        }

        #endregion
    }
}