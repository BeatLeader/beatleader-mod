using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using BeatLeader.Utils;
using UnityEngine;
using Dummy = BeatLeader.UI.Reactive.Components.Dummy;
using FlexDirection = BeatLeader.UI.Reactive.Yoga.FlexDirection;
using ImageButton = BeatLeader.UI.Reactive.Components.ImageButton;

namespace BeatLeader.UI.Replayer {
    internal class SettingsUIView : ReactiveComponent {
        #region Setup

        private ILayoutEditor? _layoutEditor;
        private IReplayTimeline? _timeline;
        private IReplayWatermark? _watermark;
        private IBeatmapTimeController? _timeController;

        public void Setup(
            IBeatmapTimeController timeController,
            ILayoutEditor? layoutEditor,
            IReplayTimeline timeline,
            IReplayWatermark watermark
        ) {
            if (_timeController != null) {
                _timeController.SongSpeedWasChangedEvent -= HandleTimeControllerSpeedChanged;
            }
            _timeController = timeController;
            _layoutEditor = layoutEditor;
            _timeline = timeline;
            _watermark = watermark;
            _watermarkToggle.SetActive(watermark.Enabled, false, true);
            _watermarkToggle.Interactable = watermark.CanBeDisabled;
            _layoutEditorRail.Enabled = layoutEditor != null;

            _timeController.SongSpeedWasChangedEvent += HandleTimeControllerSpeedChanged;
            HandleTimeControllerSpeedChanged(timeController.SongSpeedMultiplier);
            ReloadTimelineMarkerToggles();
        }

        #endregion

        #region Timeline

        private class TimelineMarkerToggle : ReactiveComponent {
            #region Setup

            private IReplayTimeline? _timeline;
            private string? _markerName;

            public void Setup(IReplayTimeline? timeline, string? markerName) {
                _timeline = timeline;
                _markerName = markerName;
                if (_timeline != null && _markerName != null) {
                    _namedRail.Label.Text = $"Show {markerName} Markers";
                    _toggle.SetActive(_timeline.GetMarkersEnabled(_markerName), false, true);
                }
            }

            #endregion

            #region Construct

            private NamedRail _namedRail = null!;
            private Toggle _toggle = null!;

            protected override GameObject Construct() {
                return new Toggle()
                    .WithListener(
                        x => x.Active,
                        x => _timeline?.SetMarkersEnabled(_markerName!, x)
                    )
                    .Bind(ref _toggle)
                    .InNamedRail("")
                    .Bind(ref _namedRail)
                    .Use();
            }

            #endregion
        }

        private readonly ReactivePool<TimelineMarkerToggle> _markerTogglesPool = new();

        private void ReloadTimelineMarkerToggles() {
            _markerTogglesPool.DespawnAll();
            foreach (var marker in _timeline!.AvailableMarkers) {
                var toggle = _markerTogglesPool.Spawn();
                toggle.Setup(_timeline, marker);
                toggle.AsFlexItem();
                _timelineTogglesContainer.Children.Add(toggle);
            }
        }

        #endregion

        #region Construct

        private Image _timelineTogglesContainer = null!;
        private Toggle _watermarkToggle = null!;
        private Slider _speedSlider = null!;
        private NamedRail _layoutEditorRail = null!;

        protected override GameObject Construct() {
            static Image CreateContainer(float gap, params ILayoutItem[] children) {
                return new Image()
                    .With(x => x.Children.AddRange(children))
                    .AsBackground(
                        color: new(0.1f, 0.1f, 0.1f, 1f),
                        pixelsPerUnit: 7f
                    ).AsFlexGroup(
                        direction: FlexDirection.Column,
                        padding: 2f,
                        justifyContent: Justify.FlexStart,
                        gap: gap
                    ).AsFlexItem();
            }

            return new Dummy {
                Children = {
                    CreateContainer(
                        2f,
                        //layout editor
                        new ImageButton {
                            Colors = UIStyle.SecondaryButtonColorSet,
                            GrowOnHover = true,
                            Image = {
                                Sprite = BundleLoader.EditLayoutIcon
                            }
                        }.WithClickListener(HandleLayoutEditorButtonClicked).AsFlexItem(
                            aspectRatio: 1f,
                            basis: 6f,
                            alignSelf: Align.Center
                        ).InNamedRail("Open Layout Editor").Bind(ref _layoutEditorRail),
                        //watermark
                        new Toggle().WithListener(
                            x => x.Active,
                            x => {
                                if (_watermark == null) return;
                                _watermark.Enabled = x;
                            }
                        ).Bind(ref _watermarkToggle).InNamedRail("Show watermark")
                    ),
                    //timeline toggles
                    CreateContainer(1f).Bind(ref _timelineTogglesContainer),
                    //speed
                    CreateContainer(
                        2f, //speed
                        new Slider {
                            ValueRange = new() {
                                Start = 20f,
                                End = 200f
                            },
                            ValueFormatter = x => $"{x}%",
                            ValueStep = 5f
                        }.WithListener(
                            x => x.Value,
                            x => _timeController?.SetSpeedMultiplier(x / 100f)
                        ).Bind(ref _speedSlider).InNamedRail("Playback Speed")
                    )
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.FlexStart,
                gap: 2f
            ).Use();
        }

        protected override void OnInitialize() {
            _layoutEditorRail.Enabled = false;
        }

        #endregion

        #region Callbacks

        private void HandleTimeControllerSpeedChanged(float speed) {
            _speedSlider.SetValueSilent(speed * 100f);
        }

        private void HandleLayoutEditorButtonClicked() {
            _layoutEditor!.SetEditorActive(true);
        }

        #endregion
    }
}