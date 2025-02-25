using BeatLeader.Components;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class SettingsUIView : ReactiveComponent {
        #region Setup

        private ILayoutEditor? _layoutEditor;
        private IReplayTimeline? _timeline;
        private IReplayWatermark? _watermark;
        private ReplayerUISettings? _settings;
        private QuickSettingsPanel? _quickSettingsPanel;

        public void Setup(
            ReplayerUISettings settings,
            QuickSettingsPanel quickSettingsPanel,
            ILayoutEditor? layoutEditor,
            IReplayTimeline timeline,
            IReplayWatermark watermark
        ) {
            _settings = settings;
            _timeline = timeline;
            _watermark = watermark;

            _layoutEditor = layoutEditor;
            _layoutEditorRail.Enabled = layoutEditor != null;

            _watermarkToggle.SetActive(watermark.Enabled, false, true);
            _watermarkToggle.Interactable = watermark.CanBeDisabled;

            _autoHideToggle.SetActive(settings.ShowUIOnPause, false, true);

            _quickSettingsPanel = quickSettingsPanel;
            _quickSettingsToggle.SetActive(settings.QuickSettingsEnabled, false, true);

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
                    var active = _timeline.GetMarkersEnabled(_markerName);
                    _toggle.SetActive(active, false, true);
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
        private Toggle _autoHideToggle = null!;
        private Toggle _quickSettingsToggle = null!;
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
                            Colors = UIStyle.ButtonColorSet,
                            OnClick = HandleLayoutEditorButtonClicked,
                            Image = {
                                Material = BundleLoader.UIAdditiveGlowMaterial,
                                Sprite = BundleLoader.EditLayoutIcon
                            }
                        }.WithScaleAnimation(1f, 1.2f).AsFlexItem(
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
                        ).Bind(ref _watermarkToggle).InNamedRail("Show watermark"),

                        //auto-hide
                        new Toggle().WithListener(
                            x => x.Active,
                            x => {
                                _settings!.ShowUIOnPause = x;
                            }
                        ).Bind(ref _autoHideToggle).InNamedRail(
                            text: "Show UI on Pause"
                        ).With(x => x.Label.RichText = true),

                        //quick settings
                        new Toggle().WithListener(
                            x => x.Active,
                            x => {
                                _quickSettingsPanel!.SetShown(x, false);
                                _settings!.QuickSettingsEnabled = x;
                            }
                        ).Bind(ref _quickSettingsToggle).InNamedRail("Show quick settings")
                    ),
                    //timeline toggles
                    CreateContainer(1f).Bind(ref _timelineTogglesContainer),
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

        private void HandleLayoutEditorButtonClicked() {
            _layoutEditor!.Mode = LayoutEditorMode.Edit;
        }

        #endregion
    }
}