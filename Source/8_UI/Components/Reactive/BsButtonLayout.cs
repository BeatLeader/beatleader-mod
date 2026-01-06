using System.Collections.Generic;
using Reactive;
using Reactive.BeatSaber.Components;

namespace BeatLeader.UI.Reactive.Components;

/// A temporary solution to expose children of the <see cref="Reactive.BeatSaber.Components.BsButtonBase"/>
// TODO: remove after reworking bs sdk
internal class BsButtonLayout : BsButtonBase, ILayoutDriver {
    public ICollection<ILayoutItem> Children => _layout.Children;

    public new ILayoutController? LayoutController {
        get => base.LayoutController;
        set => base.LayoutController = value;
    }

    private Layout _layout = null!;

    protected override IEnumerable<IReactiveComponent> ConstructContent() {
        return [new Layout().Bind(ref _layout)];
    }
}