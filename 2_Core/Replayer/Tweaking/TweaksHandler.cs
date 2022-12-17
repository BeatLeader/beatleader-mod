using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Tweaking {
    public class TweaksHandler : MonoBehaviour {
        private enum ActionType {
            Initialize,
            LateInitialize,
            Dispose,
            Inject
        }

        [Inject] private readonly DiContainer _container;

        public IReadOnlyList<GameTweak> Tweaks { get; } = new List<GameTweak> {
            new AudioTimeSyncControllerTweak(),
            new SmoothCameraTweak(),
            new InputSystemTweak(),
            new GarbageDisablerTweak(),
            new InteropsLoaderTweak(),
            new MethodsSilencerTweak(),
            new ModifiersTweak(),
            new RaycastBlockerTweak(),
            new RoomOffsetsTweak()
        }; 
        
        private void Awake() {
            PerformAction(ActionType.Inject);
            PerformAction(ActionType.Initialize);
        }
        
        private void Start() {
            PerformAction(ActionType.LateInitialize);
        }
        
        private void OnDestroy() {
            PerformAction(ActionType.Dispose);
        }

        private void PerformAction(ActionType action) {
            foreach (var tweak in Tweaks) {
                if (!tweak.CanBeInstalled) continue;
                try {
                    switch (action) {
                        case ActionType.Inject:
                            _container.Inject(tweak);
                            break;
                        case ActionType.Initialize:
                            tweak.Initialize();
                            break;
                        case ActionType.LateInitialize:
                            tweak.LateInitialize();
                            break;
                        case ActionType.Dispose:
                            tweak.Dispose();
                            break;
                    }
                } catch (Exception ex) {
                    Plugin.Log.Error($"[TweaksLoader] Error during attempting to perform {action} on {tweak.GetType().Name} tweak! \r\n {ex}");
                }
            }
        }
    }
}
