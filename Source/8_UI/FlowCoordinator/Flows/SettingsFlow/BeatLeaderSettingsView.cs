using HMUI;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BeatLeaderSettingsView : ReactiveComponent {

        private GlobalSettingsView _globalSettingsView;
        private UISettingsView _uiSettingsView;
        private ReplaySettingsView _replaySettingsView;

        protected override GameObject Construct() {
            return new Layout {
                Children = {
                    new TextSegmentedControl<string> {
                            WhenCellSpawned = (_, cell) => {
                                cell.FontStyle |= FontStyles.Italic;
                            },
                            Items = {
                                { "GLOBAL", "Global" },
                                { "UI", "UI" },
                                { "REPLAYS", "Replays" },
                            },
                        }
                        .AsFlexItem(flexGrow: 1f)
                        .Export(out var control)
                        //bg
                        .InBackground(
                            color: UIStyle.ControlColorSet.Color,
                            skew: UIStyle.Skew
                        )
                        .AsFlexGroup()
                        .AsFlexItem(size: new() { y = 8f, x = 50f }),
                    //
                    new KeyedContainer<string> {
                        Control = control,
                        Items = {
                            ["GLOBAL"] = new GlobalSettingsView()
                                .WithRectExpand()
                                .Bind(ref _globalSettingsView),

                            ["UI"] = new UISettingsView()
                                .WithRectExpand()
                                .Bind(ref _uiSettingsView),

                            ["REPLAYS"] = new ReplaySettingsView()
                                .WithRectExpand()
                                .Bind(ref _replaySettingsView)
                        }
                    }.AsFlexItem(flexGrow: 1f, size: new() { x = 90f })
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