namespace BeatLeader.UI.Reactive.Components {
    internal class ColorCircleDialog : DialogComponentBase {
        public ColorCircle ColorCircle { get; } = new();
        
        protected override ILayoutItem ConstructContent() {
            return ColorCircle.AsFlexItem(size: 54f);
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            this.WithSizeDelta(54f, 67f);
            Title = "Select Color";
            ShowCancelButton = false;
        }
    }
}