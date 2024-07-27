using System;

namespace BeatLeader.Models {
    [Flags]
    public enum BodyNode {
        Head = 1,
        LeftHand = 2,
        RightHand = 4,
        Unknown = 8
    }
}