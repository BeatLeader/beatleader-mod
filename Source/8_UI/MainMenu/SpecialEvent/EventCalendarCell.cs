using System;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class EventCalendarCell : ReactiveComponent {
        public DateTime Date {
            get => _date;
            set {
                _date = value;
                Refresh();
            }
        }

        public bool Completed {
            get => _completedLabel.Enabled;
            set => _completedLabel.Enabled = value;
        }

        public Color Accent {
            get => _accent;
            set {
                _accent = value;
                Refresh();
            }
        }

        private DateTime _date;
        private Color _accent;

        private Label _dayLabel = null!;
        private Label _completedLabel = null!;
        private Background _background = null!;

        private void Refresh() {
            var now = DateTime.UtcNow.Date;
            var date = _date.Date;

            if (date == now) {
                // Today
                _background.Color = _accent.ColorWithAlpha(0.9f);
                _dayLabel.Color = Color.white;
            } else if (now.AddDays(-1) < date) {
                // Previous days
                _background.Color = (Color.white * 0.1f).ColorWithAlpha(0.5f);
                _dayLabel.Color = Color.white * 0.7f;
            } else {
                // Next days
                _background.Color = Color.white * 0.3f;
                _dayLabel.Color = Color.white;
            }

            _dayLabel.Text = _date.Day.ToString();
        }

        protected override GameObject Construct() {
            return new Background {
                    Children = {
                        new Label {
                                Alignment = TextAlignmentOptions.Capline
                            }
                            .Bind(ref _dayLabel)
                            .AsFlexItem(),

                        new Label {
                                Text = "★",
                                Color = Color.yellow
                            }
                            .Bind(ref _completedLabel)
                            .AsFlexItem(position: new() { top = 0.pt(), left = 0.pt() }),
                    }
                }
                .AsBackground()
                .AsFlexGroup(justifyContent: Justify.Center)
                .AsFlexItem(size: 5f)
                .Bind(ref _background)
                .Use();
        }
    }
}