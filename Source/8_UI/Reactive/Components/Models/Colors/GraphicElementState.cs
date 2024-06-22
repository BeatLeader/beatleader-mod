namespace BeatLeader.UI.Reactive {
    internal struct GraphicElementState {
        public GraphicElementState(bool hovered, bool active, bool interactable) {
            this.hovered = hovered;
            this.active = active;
            this.interactable = interactable;
        }

        public bool hovered;
        public bool active;
        public bool interactable;
    }
}