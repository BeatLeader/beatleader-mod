namespace BeatLeader.Components {
    internal interface ILayoutComponent {
        string ComponentName { get; }
        ref LayoutData LayoutData { get; }

        void Setup(ILayoutComponentHandler? handler);
        void ApplyLayoutData(bool notify = true);
        void LoadLayoutData();
        
        void OnEditorModeChanged(LayoutEditorMode mode);
        void OnSelectedComponentChanged(ILayoutComponent? component);
    }
}