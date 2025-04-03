using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using HMUI;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BeatLeaderSettingsViewController : ViewController {
        #region Injection

        [Inject] private readonly MenuTransitionsHelper _menuTransitionsHelper = null!;
        [Inject] private readonly IReplayFileManager _replayFileManager = null!;
        [Inject] private readonly BeatLeaderHubTheme _hubTheme = null!;

        #endregion

        #region Construct

        private void Awake() {
            new Dummy {
                Children = {
                    new TextSegmentedControl<string> {
                            CellConstructCallback = cell => {
                                cell.Label.FontStyle |= FontStyles.Italic;
                            },
                            Items = {
                                { "GLOBAL", "Global" },
                                { "UI", "UI" },
                                { "REPLAYS", "Replays" },
                            },
                        }
                        .AsFlexItem(grow: 1f)
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
                                .With(x => x.Setup(_menuTransitionsHelper)),

                            ["UI"] = new UISettingsView()
                                .WithRectExpand()
                                .With(x => x.Setup(_hubTheme)),

                            ["REPLAYS"] = new ReplaySettingsView()
                                .WithRectExpand()
                                .With(x => x.Setup(_replayFileManager))
                        }
                    }.AsFlexItem(grow: 1f, size: new() { x = 90f })
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                alignItems: Align.Center,
                gap: 6f
            ).WithRectExpand().Use(transform);
        }

        #endregion
    }
}