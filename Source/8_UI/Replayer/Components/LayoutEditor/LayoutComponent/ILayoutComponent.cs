namespace BeatLeader.Components {
    internal interface ILayoutComponent {
        string ComponentName { get; }
        ref LayoutData LayoutData { get; }

        void Setup(ILayoutComponentHandler? handler);
        void ApplyLayoutData();
        
        void OnEditorModeChanged(LayoutEditorMode mode);
        void OnSelectedComponentChanged(ILayoutComponent? component);
    }
}