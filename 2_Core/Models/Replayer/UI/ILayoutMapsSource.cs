using BeatLeader.Components;

namespace BeatLeader.Models {
    internal interface ILayoutMapsSource {
        bool TryRequestLayoutMap(EditableElement element, out LayoutMap map);
        void OverrideLayoutMap(EditableElement element, LayoutMap map);
    }
}
