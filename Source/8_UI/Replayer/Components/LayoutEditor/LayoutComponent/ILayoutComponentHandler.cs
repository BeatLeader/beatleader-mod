using UnityEngine;

namespace BeatLeader.Components {
    internal interface ILayoutComponentHandler {
        Vector2 PointerPosition { get; }
        RectTransform? AreaTransform { get; }

        void OnSelect(ILayoutComponent component);
        Vector2 OnMove(ILayoutComponent component, Vector2 destination);
        Vector2 OnResize(ILayoutComponent component, Vector2 destination);
    }
}