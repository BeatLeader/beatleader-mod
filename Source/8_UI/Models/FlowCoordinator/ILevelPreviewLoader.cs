using BeatLeader.Models;

namespace BeatLeader.UI.Hub {
    internal interface IReplayPreviewLoader {
        void LoadPreview(IReplayHeaderBase header);
        void StopPreview();
    }
}