using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reactive;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.UI.Reactive.Components;

/// <summary>
/// A layout group that expands the hovered object and collapses others.
/// Useful for placing multiple buttons in small spaces.
/// </summary>
[PublicAPI]
public class ExpandoGroup : ReactiveComponent, ILayoutDriver {
    #region LayoutDriver

    public ICollection<ILayoutItem> Children => _layout.Children;

    public ILayoutController? LayoutController {
        get => _layout.LayoutController;
        set => _layout.LayoutController = value;
    }

    #endregion

    #region Props

    /// <summary>
    /// Either relative or fixed width/height of the hovered button.
    /// </summary>
    public YogaValue ExpandedButtonSize { get; set; }

    #endregion

    #region Construct

    private Layout _layout = null!;

    protected override GameObject Construct() {
        return new Layout()
            .WithNativeComponent(out PointerEventsHandler eventsHandler)
            .With(_ => {
                    eventsHandler.PointerEnterEvent += HandlePointerEntered;
                    eventsHandler.PointerExitEvent += HandlePointerLeft;
                }
            )
            .Use();
    }

    #endregion

    #region Callbacks

    private void HandlePointerEntered(PointerEventsHandler _, PointerEventData data) {
        var hovered = data.hovered.FirstOrDefault(x => x);
    }

    private void HandlePointerLeft(PointerEventsHandler _, PointerEventData data) { }

    #endregion
}