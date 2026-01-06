using System;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components;

/// <summary>
/// Represents a <see cref="ReplayPanel"/> button added by an external source.
/// </summary>
[PublicAPI]
public class ReplayPanelExtraButton {
    /// <summary>
    /// A text displayed on the button when expanded.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// A placeholder icon displayed when the button is collapsed.
    /// </summary>
    public required Sprite Icon { get; init; }

    /// <summary>
    /// A callback to be called when the button is pressed.
    /// </summary>
    public required Action Callback { get; init; }

    /// <summary>
    /// An accent color of the button's text and icon.
    /// </summary>
    public Color? AccentColor { get; init; }

    /// <summary>
    /// Determines whether the button is clickable or not.
    /// </summary>
    public bool Clickable {
        get;
        set {
            field = value;
            ClickableChangedEvent?.Invoke(this, value);
        }
    }

    internal event Action<ReplayPanelExtraButton, bool>? ClickableChangedEvent;

    internal void InvokeCallback() {
        Callback.Invoke();
    }
}