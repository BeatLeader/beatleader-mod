using System.Collections.Generic;

namespace BeatLeader.UI.Replayer {
    internal record AvatarPartConfigsGroup(
        string? GroupName,
        IEnumerable<AvatarPartConfigWithModel> Configs
    );
}