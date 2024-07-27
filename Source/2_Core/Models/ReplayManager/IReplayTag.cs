using System;
using UnityEngine;

namespace BeatLeader.Models {
    public interface IReplayTag {
        Color Color { get; }
        string Name { get; }

        event Action? TagUpdatedEvent;
    }
}