using BeatSaberMarkupLanguage.FloatingScreen;
using UnityEngine;

namespace BeatLeader.Replays.UI
{
    public interface IPlaybackViewController
    {
        FloatingScreen floatingScreen { get; }
        GameObject root { get; }

        void Init();
        void Enable();
        void Disable();
    }
}
