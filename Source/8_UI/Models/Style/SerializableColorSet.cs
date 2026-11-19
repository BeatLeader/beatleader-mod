using BeatLeader.Models;
using Reactive.Components;

namespace BeatLeader {
    /// <summary>
    /// A simple serializable color set for general purposes.
    /// </summary>
    internal record struct SerializableColorSet {
        public SerializableColor ActiveColor { get; set; }
        public SerializableColor HoveredColor { get; set; }
        public SerializableColor NotInteractableColor { get; set; }
        public SerializableColor Color { get; set; }

        public ColorSet ToColorSet() {
            return new ColorSet {
                Color = Color,
                HoveredColor = HoveredColor,
                ActiveColor = ActiveColor,
                HoveredActiveColor = ActiveColor,
                NotInteractableColor = NotInteractableColor
            };
        }
    }
}