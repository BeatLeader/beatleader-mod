namespace BeatLeader.Models {
    public interface ICameraView {
        bool Update { get; }
        string Name { get; }

        void ProcessView(ICameraControllerBase cameraController);
    }
}
