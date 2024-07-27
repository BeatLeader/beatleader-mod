using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal interface IReactiveComponent {
        GameObject Content { get; }
        RectTransform ContentTransform { get; }

        bool IsDestroyed { get; }
        bool IsInitialized { get; }
        bool Enabled { get; set; }

        GameObject Use(Transform? parent);
    }
}