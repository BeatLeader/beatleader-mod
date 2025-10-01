using System;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.UI.MainMenu {
    internal class EventCalendarCell : ReactiveComponent {
        public DateTime Date {
            get => _date;
            set {
                _date = value;
                Refresh();
            }
        }

        public bool Selected {
            get => _selected;
            set {
                _selected = value;
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
        
        public Action<EventCalendarCell>? OnClick { get; set; }

        private DateTime _date;
        private Color _accent;
        private Color _bgColor;
        private bool _selected;

        private Label _dayLabel = null!;
        private Label _completedLabel = null!;
        private Background _background = null!;
        private PointerEventsHandler _pointerHandler = null!;
        
        private void Refresh() {
            var now = DateTime.UtcNow.Date;
            var date = _date.Date;

            if (_selected) {
                _bgColor = _accent.ColorWithAlpha(0.9f);
                _dayLabel.Color = Color.white;
                _pointerHandler.enabled = true;
            } else if (date <= now) {
                // Previous days or today if not selected
                _bgColor = Color.white * 0.3f;
                _dayLabel.Color = Color.white;
                _pointerHandler.enabled = true;
            } else {
                // Next days
                _bgColor = (Color.white * 0.1f).ColorWithAlpha(0.5f);
                _dayLabel.Color = Color.white * 0.7f;
                _pointerHandler.enabled = false;
            }

            _dayLabel.Text = _date.Day.ToString();
            
            RefreshBackgroundColor();
        }

        private void RefreshBackgroundColor() {
            _background.Color = _pointerHandler.IsHovered ? _bgColor * 1.3f : _bgColor;
        }
        
        private void HandleBackgroundEvents(PointerEventsHandler _, PointerEventData _1) {
            RefreshBackgroundColor();
            
            if (_pointerHandler.IsPressed) {
                OnClick?.Invoke(this);
            }
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
                                Color = Color.yellow,
                                FontSize = 3f
                            }
                            .Bind(ref _completedLabel)
                            .AsFlexItem(position: new() { top = (-1.3f).pt(), right = (-0.8f).pt() }),
                    }
                }
                .WithNativeComponent(out _pointerHandler)
                .With(_ => _pointerHandler.PointerUpdatedEvent += HandleBackgroundEvents)
                .AsBackground()
                .AsFlexGroup(justifyContent: Justify.Center)
                .AsFlexItem(size: 5f)
                .Bind(ref _background)
                .Use();
        }
    }
}