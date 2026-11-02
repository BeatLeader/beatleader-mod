using System.Collections.Generic;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class BeatLeaderSettingsView : ReactiveComponent {
        private GlobalSettingsView _globalSettingsView = null!;
        private UISettingsView _uiSettingsView = null!;
        private ReplaySettingsView _replaySettingsView = null!;
        private KeyedContainer<string> _viewContainer = null!;

        protected override GameObject Construct() {
            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    AlignItems = Align.Center,
                    Gap = 6f
                },

                Children = {
                    new BsSegmentedControl<string> {
                        FlexItem = {
                            Size = new() { y = 8f, x = 50f }
                        },

                        Items = new Dictionary<string, BsSegmentedControlItem> {
                            ["GLOBAL"] = new("Global", null),
                            ["UI"] = new("UI", null),
                            ["REPLAYS"] = new("Replays", null)
                        },
                        Key = "GLOBAL",
                        OnKeyChanged = x => _viewContainer.Key = x
                    },
                    //
                    new KeyedContainer<string> {
                        FlexItem = {
                            FlexGrow = 1f,
                            Size = new() { x = 90f }
                        },

                        Views = new Dictionary<string, IReactiveComponent> {
                            ["GLOBAL"] = new GlobalSettingsView()
                                .WithRectExpand()
                                .Bind(ref _globalSettingsView),

                            ["UI"] = new UISettingsView()
                                .WithRectExpand()
                                .Bind(ref _uiSettingsView),

                            ["REPLAYS"] = new ReplaySettingsView()
                                .WithRectExpand()
                                .Bind(ref _replaySettingsView)
                        },
                        Key = "GLOBAL"
                    }.Bind(ref _viewContainer)
                }
            }.WithRectExpand().Use();
        }

        public void Setup(BeatLeaderHubTheme hubTheme, MenuTransitionsHelper? menuTransitionsHelper) {
            _globalSettingsView.Setup(menuTransitionsHelper);
            _uiSettingsView.Setup(hubTheme);
        }

        public void CancelSelection() {
            _globalSettingsView.CancelSelection();
            _uiSettingsView.CancelSelection();
            _replaySettingsView.CancelSelection();
        }
    }
}