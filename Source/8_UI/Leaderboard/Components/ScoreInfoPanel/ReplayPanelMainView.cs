using System;
using BeatLeader.UI;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.Components;

internal class ReplayPanelMainView : ReactiveComponent {
    #region Extra Buttons

    private readonly ReactivePool<ReplayPanelExtraButton, ExpandoButton> _buttons = new();

    public void AddExtraButton(ReplayPanelExtraButton extraButton) {
        var button = _buttons.Spawn(extraButton);

        button.Icon = extraButton.Icon;
        button.Text = extraButton.Text;
        button.Color = extraButton.AccentColor ?? UIStyle.SecondaryTextColor;

        _expandoGroup.Children.Add(button);
    }

    public void RemoveExtraButton(ReplayPanelExtraButton extraButton) {
        var button = _buttons.Get(extraButton);
        
        _expandoGroup.Children.Remove(button);
        _buttons.Despawn(button);
    }

    #endregion

    #region Public API

    public Action? OnDownloadClicked { get; set; }
    public Action? OnPlayClicked { get; set; }

    public bool MainButtonsClickable {
        get => _mainButtonsClickable.Value;
        set => _mainButtonsClickable.Value = value;
    }

    #endregion

    #region Construct

    private ObservableValue<bool> _mainButtonsClickable = null!;
    private ExpandoGroup _expandoGroup = null!;

    protected override GameObject Construct() {
        _mainButtonsClickable = Remember(false);

        return new Layout {
                Children = {
                    new ReeWrapperV2<ReplayerSettingsPanel>()
                        .AsFlexItem(size: new() { y = 8.pt() }),

                    new ExpandoGroup {
                            Children = {
                                new ExpandoButton {
                                    Text = "Watch",
                                    Icon = BundleLoader.Sprites.plusIcon
                                }.Animate(_mainButtonsClickable, (x, y) => x.Clickable = y),

                                new ExpandoButton {
                                    Text = "Download",
                                    Icon = BundleLoader.Sprites.homeIcon
                                }.Animate(_mainButtonsClickable, (x, y) => x.Clickable = y)
                            }
                        }
                        .AsFlexItem(
                            size: new() { x = 100.pct() },
                            margin: new() { left = 2.pt(), right = 2.pt() }
                        )
                        .AsFlexGroup()
                        .Bind(ref _expandoGroup)
                }
            }
            .AsFlexGroup(direction: FlexDirection.Column, justifyContent: Justify.SpaceEvenly)
            .AsFlexItem(size: new() { x = 50.pt() })
            .Use();
    }

    #endregion
}