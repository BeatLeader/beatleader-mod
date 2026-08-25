using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Binding {
    public class HotkeysHandler : MonoBehaviour {
        [Inject] private readonly DiContainer _container = null!;

        public IList<GameHotkey> Hotkeys { get; } = new List<GameHotkey> {
            new LayoutEditorModeHotkey(),
            new HideCursorHotkey(),
            new PauseHotkey(),
            new RewindBackwardHotkey(),
            new RewindForwardHotkey(),
            new SpeedDownHotkey(),
            new SpeedUpHotkey()
        };

        private void Awake() {
            foreach (var item in Hotkeys) {
                _container.Inject(item);
            }
        }

        private void Update() {
            foreach (var item in Hotkeys) {
                try {
                    foreach (var key in item.Keys) {
                        if (Input.GetKeyDown(key)) {
                            item.OnKeyDown();
                            break;
                        }
                        if (Input.GetKeyUp(key)) {
                            item.OnKeyUp();
                            break;
                        }
                    }
                } catch (Exception ex) {
                    Plugin.Log.Error($"[HotkeysHandler] Error during attempting to perform {item.GetType().Name} hotkey!\r\n{ex}");
                }
            }
        }
    }
}
