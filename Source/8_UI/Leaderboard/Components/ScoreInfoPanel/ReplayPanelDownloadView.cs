using System;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.Components;

internal class ReplayPanelDownloadView : ReactiveComponent {
    #region Public API

    public float Progress {
        get => _progress.Value;
        set => _progress.Value = value;
    }
    
    public Action? OnDownloadFinished { get; set; }
    
    #endregion

    #region Construct
    
    private ObservableValue<float> _progress = null!;

    protected override GameObject Construct() {
        _progress = Remember(-1f);

        return new Layout {
                Children = {
                    new Label()
                        .AsFlexItem()
                        .Animate(
                            _progress,
                            (x, y) => x.Text = _progress.Value switch {
                                >= 1 => "Done!",
                                < 0 => "Loading...",
                                _ => $"{y}%",
                            }
                        ),

                    new Spinner().AsFlexItem(size: 10.pt()),
                    
                    new BsButton {
                        Text = "Cancel",
                        OnClick = () => OnDownloadFinished?.Invoke()
                    }
                }
            }
            .AsFlexGroup(direction: FlexDirection.Column)
            .AsFlexItem(size: new() { x = 40.pt() })
            .Use();
    }

    #endregion
}