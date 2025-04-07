using BeatLeader.Models;

namespace BeatLeader.UI.Hub {
    internal interface IReplayPreviewLoader {
        void LoadPreview(IReplayHeader header);
        void StopPreview();
    }
}