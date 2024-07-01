using UnityEngine;

namespace BeatLeader.Models.AbstractReplay {
    public interface ITablePlayer : IPlayer {
        Color AccentColor { get; }
    }
}