using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class SettingsUIView : ReactiveComponent {
        #region Setup

        private ILayoutEditor? _layoutEditor;
        private IReplayWatermark? _watermark;
        private ReplayerUISettings? _settings;
        private QuickSettingsPanel? _quickSettingsPanel;
        private bool _loadingSettings;

        public void Setup(
            ReplayerUISettings settings,
            QuickSettingsPanel quickSettingsPanel,
            ILayoutEditor? layoutEditor,
            IReplayTimeline timeline,
            IReplayWatermark watermark
        ) {
            _settings = settings;
            _watermark = watermark;

            _layoutEditor = layoutEditor;
            _layoutEditorRail.Enabled = layoutEditor != null;

            _loadingSettings = true;

            _watermarkToggle.Toggled = watermark.Enabled;
            _watermarkToggle.Interactable = watermark.CanBeDisabled;

            _autoHideToggle.Toggled = settings.ShowUIOnPause;

            _quickSettingsPanel = quickSettingsPanel;
            _quickSettingsToggle.Toggled = settings.QuickSettingsEnabled;

            _loadingSettings = false;

            ReloadTimelineMarkerToggles(timeline);
        }

        #endregion

        #region Timeline

        private class TimelineMarkerToggle : ReactiveComponent {
            #region Setup

            public void Setup(IReplayTimeline timeline, string markerName) {
                _namedRail.Label.Text = $"Show {markerName} Markers";
                _toggle.OnToggleStateChanged = null;
                _toggle.Toggled = timeline.GetMarkersEnabled(markerName);
                _toggle.OnToggleStateChanged = x => timeline.SetMarkersEnabled(markerName, x);
            }

            #endregion

            #region Construct

            private NamedRail _namedRail = null!;
            private BsToggle _toggle = null!;

            protected override GameObject Construct() {
                return new BsToggle()
                    .Bind(ref _toggle)
                    .InNamedRail("")
                    .Bind(ref _namedRail)
                    .Use();
            }

            #endregion
        }

        private readonly ReactivePool<TimelineMarkerToggle> _markerTogglesPool = new();

        private void ReloadTimelineMarkerToggles(IReplayTimeline timeline) {
            _markerTogglesPool.DespawnAll();

            foreach (var marker in timeline.AvailableMarkers) {
                var toggle = _markerTogglesPool.Spawn();

                toggle.Setup(timeline, marker);
                toggle.AsFlexItem();

                _timelineTogglesContainer.Children.Add(toggle);
            }
        }

        #endregion

        #region Construct

        private Background _timelineTogglesContainer = null!;
        private BsToggle _watermarkToggle = null!;
        private BsToggle _autoHideToggle = null!;
        private BsToggle _quickSettingsToggle = null!;
        private NamedRail _layoutEditorRail = null!;

        [StateGen]
        private Image CreateLayoutEditorButton() {
            var state = Remember(GraphicState.None);

            return new Image {
                    Material = BundleLoader.UIAdditiveGlowMaterial,
                    Sprite = BundleLoader.EditLayoutIcon,
                    sColor = state.MapColorSet(UIStyle.GlowingButtonColorSet)
                }
                .WithPointerEvents(
                    onEnter: _ => state.IsHovered = true,
                    onLeave: _ => state.IsHovered = false,
                    onDown: _ => {
                        HandleLayoutEditorButtonClicked();
                        global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                    }
                )
                .WithHoverScale(1f, 1.2f);
        }

        private static Background CreateContainer(params ILayoutItem[] children) {
            return new Background()
                .With(x => x.Children.AddRange(children))
                .AsBackground(
                    color: new(0.1f, 0.1f, 0.1f, 1f),
                    pixelsPerUnit: 7f
                ).AsFlexGroup(
                    direction: FlexDirection.Column,
                    padding: 2f,
                    justifyContent: Justify.FlexStart,
                    gap: 1f
                ).AsFlexItem();
        }

        private Layout CreateContent() {
            return new Layout {
                Children = {
                    CreateContainer(
                        // Layout editor
                        CreateLayoutEditorButton().AsFlexItem(
                            aspectRatio: 1f,
                            basis: 6f,
                            alignSelf: Align.Center
                        ).InNamedRail("Open Layout Editor").Bind(ref _layoutEditorRail),

                        // Watermark
                        new BsToggle {
                            OnToggleStateChanged = x => {
                                if (_loadingSettings) return;
                                _watermark?.Enabled = x;
                            }
                        }.Bind(ref _watermarkToggle).InNamedRail("Show watermark"),

                        // Auto-hide
                        new BsToggle {
                            OnToggleStateChanged = x => {
                                if (_loadingSettings) return;
                                _settings!.ShowUIOnPause = x;
                            }
                        }.Bind(ref _autoHideToggle).InNamedRail(
                            text: "Show UI on Pause"
                        ).With(x => x.Label.RichText = true),

                        // Quick settings
                        new BsToggle {
                            OnToggleStateChanged = x => {
                                if (_loadingSettings) return;
                                _quickSettingsPanel!.SetShown(x, false);
                                _settings!.QuickSettingsEnabled = x;
                            }
                        }.Bind(ref _quickSettingsToggle).InNamedRail("Show quick settings")
                    ),

                    // Timeline toggles
                    CreateContainer().Bind(ref _timelineTogglesContainer),
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.FlexStart,
                gap: 2f,
                constrainVertical: false
            ).AsFlexItem(
                size: new() { x = 100.pct }
            );
        }

        protected override GameObject Construct() {
            var scrollContext = new ScrollContext();

            return new Layout {
                Children = {
                    new ScrollArea {
                        ScrollContext = scrollContext,
                        ScrollContent = CreateContent(),
                        LineSize = 1f
                    }.AsFlexItem(flexGrow: 1f),

                    new Scrollbar {
                        ScrollContext = scrollContext,
                        HideIfNothingToScroll = true
                    }.AsFlexItem()
                }
            }.AsFlexGroup(gap: 1f).AsFlexItem().Use();
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