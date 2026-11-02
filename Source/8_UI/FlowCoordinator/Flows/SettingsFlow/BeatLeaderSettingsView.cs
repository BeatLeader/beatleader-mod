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
                Children = {
                    new BsSegmentedControl<string> {
                            Items = new Dictionary<string, BsSegmentedControlItem> {
                                ["GLOBAL"] = new("Global", null),
                                ["UI"] = new("UI", null),
                                ["REPLAYS"] = new("Replays", null)
                            },
                            Key = "GLOBAL",
                            OnKeyChanged = x => _viewContainer.Key = x
                        }
                        .AsFlexItem(size: new() { y = 8f, x = 50f }),
                    //
                    new KeyedContainer<string> {
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
                    }.AsFlexItem(flexGrow: 1f, size: new() { x = 90f }).Bind(ref _viewContainer)
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                alignItems: Align.Center,
                gap: 6f
            ).WithRectExpand().Use();
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