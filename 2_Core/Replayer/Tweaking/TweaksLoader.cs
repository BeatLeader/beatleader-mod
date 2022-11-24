using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Tweaking {
    public class TweaksLoader : MonoBehaviour {
        private enum ActionType {
            Initialize,
            LateInitialize,
            Dispose,
            Inject
        }

        public readonly List<GameTweak> tweaks = new() {
            new SmoothCameraTweak(),
            new InputSystemTweak(),
            new EventsSubscriberTweak(),
            new GarbageDisablerTweak(),
            new PatchesLoaderTweak(),
            new MethodsSilencerTweak(),
            new ModifiersTweak(),
            new RoomOffsetsTweak(),
            new SoundEffectsCountLimiterTweak()
        }; //not static to dispose the garbage inside tweaks

        [Inject] private readonly DiContainer _container;

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
            foreach (var item in tweaks) {
                if (!item.CanBeInstalled) continue;
                try {
                    switch (action) {
                        case ActionType.Inject:
                            _container.Inject(item);
                            break;
                        case ActionType.Initialize:
                            item.Initialize();
                            break;
                        case ActionType.LateInitialize:
                            item.LateInitialize();
                            break;
                        case ActionType.Dispose:
                            item.Dispose();
                            break;
                    }
                } catch (Exception ex) {
                    Plugin.Log.Error($"[TweaksLoader] Error during attempting to perform {action} on {item.GetType().Name} tweak! \r\n {ex}");
                }
            }
        }
    }
}
