using System;
using System.Collections;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class EventPreviewPanel : ListCell<PlatformEvent> {
        public Action<PlatformEvent>? ButtonAction { get; set; }
        public Action<PlatformEvent>? BackgroundAction { get; set; }

        private ObservableValue<string> _bottomText = null!;

        protected override void OnDestroy() {
            StopAllCoroutines();
            base.OnDestroy();
        }

        protected override GameObject Construct() {
            _bottomText = Remember(string.Empty);
            
            return new BackgroundButton {
                Colors = new StateColorSet {
                    States = {
                        GraphicState.None.WithColor(Color.clear),
                        GraphicState.Active.WithColor(Color.white.ColorWithAlpha(0.53f)),
                        GraphicState.Hovered.WithColor(new Color(0, 0, 0, 0.5f + 0.4f))
                    }
                },
                Image = {
                    Sprite = BeatSaberResources.Sprites.background,
                    PixelsPerUnit = 10,
                    Skew = UIStyle.Skew
                },
                OnClick = () => BackgroundAction?.Invoke(Item),
                Children = {
                    new Image {
                        Sprite = BundleLoader.UnknownIcon,
                        Material = BundleLoader.RoundTextureMaterial,
                        Skew = UIStyle.Skew
                    }.AsFlexItem(
                        size: new() { x = 10, y = 10 }
                    ).Animate(ObservableItem, (img, item) => {
                        if (item == null) return;
                        img.WithWebSource(item.image);
                    }),
                    new Layout {
                        Children = {
                            new Label {
                                FontSize = 4f,
                                Overflow = TextOverflowModes.Ellipsis,
                                Alignment = TextAlignmentOptions.Left
                            }.AsFlexItem().Animate(ObservableItem, (lbl, item) => {
                                if (item == null) return;
                                lbl.Text = item.name;
                            }),
                            new Label {
                                FontSize = 3f,
                                Overflow = TextOverflowModes.Ellipsis,
                                Alignment = TextAlignmentOptions.Left,
                                RichText = true, // For color tags
                            }.AsFlexItem().Animate(_bottomText, (lbl, text) => lbl.Text = text)
                        }
                    }.AsFlexGroup(
                        direction: FlexDirection.Column
                    ).AsFlexItem(
                        margin: new() { left = 1 }, 
                        flexGrow: 1
                    ),
                    new BsButton {
                        Skew = UIStyle.Skew,
                        OnClick = () => ButtonAction?.Invoke(Item)
                    }.AsFlexItem(
                        size: new() { x = 12, y = 8 }
                    ).Animate(ObservableItem, (btn, item) => {
                        if (item == null) return;
                        btn.Text = "Details";
                    })
                }
            }.AsFlexGroup(
                direction: FlexDirection.Row,
                gap: 1f,
                padding: 1f,
                alignItems: Align.Center,
                constrainHorizontal: false
            ).AsFlexItem().Use();
        }

        #region Timer Logic

        protected override void OnInit(PlatformEvent item) {
            base.OnInit(item);

            if (item == null) {
                StopAllCoroutines();
                return;
            }
            UpdateBottomText(item);
        }

        private void UpdateBottomText(PlatformEvent item) {
            StopAllCoroutines();

            var timeSpan = FormatUtils.GetRelativeTime(item.endDate);
            var remainingTime = timeSpan;
            
            if (timeSpan < TimeSpan.Zero) {
                _bottomText.Value = $"<color=#88FF88>{FormatRemainingTime(-timeSpan)}";
                remainingTime = -timeSpan;
            } else {
                var date = FormatUtils.GetRelativeTimeString(timeSpan, false);
                _bottomText.Value = $"<color=#884444>Ended {date}";
                return; // No need to schedule update if it has ended
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
                updateDelay = TimeSpan.FromSeconds(Math.Ceiling(remainingTime.TotalSeconds)) - remainingTime;
                if (updateDelay <= TimeSpan.Zero) updateDelay = TimeSpan.FromSeconds(1);
            }

            StartCoroutine(UpdateAfterDelay(updateDelay, item));
        }

        private IEnumerator UpdateAfterDelay(TimeSpan delay, PlatformEvent item) {
            yield return new WaitForSeconds((float)delay.TotalSeconds);
            UpdateBottomText(item);
        }

        private static string FormatRemainingTime(TimeSpan span) {
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

        #endregion
    }
} 