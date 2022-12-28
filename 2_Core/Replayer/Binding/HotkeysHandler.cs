using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Binding {
    public class HotkeysHandler : MonoBehaviour {
        [Inject] private readonly DiContainer _container = null!;

        public IList<GameHotkey> Hotkeys { get; } = new List<GameHotkey> {
            new LayoutEditorHotkey(),
            new HideCursorHotkey(),
            new PauseHotkey(),
            new RewindBackwardHotkey(),
            new RewindForwardHotkey()
        };

        private void Awake() {
            foreach (var item in Hotkeys) {
                _container.Inject(item);
            }
        }

        private void Update() {
            foreach (var item in Hotkeys) {
                try {
                    if (Input.GetKeyDown(item.Key))
                        item.OnKeyDown();
                    else if (Input.GetKeyUp(item.Key))
                        item.OnKeyUp();
                } catch (Exception ex) {
                    Plugin.Log.Error($"[HotkeysHandler] Error during attempting to perform {item.GetType().Name} hotkey!\r\n{ex}");
                }
            }
        }
    }
}
