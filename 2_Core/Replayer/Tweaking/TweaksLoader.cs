using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Tweaking
{
    public class TweaksLoader : MonoBehaviour
    {
        private enum ActionType
        {
            Initialize,
            LateInitialize,
            Dispose,
            Inject
        }

        private readonly List<GameTweak> _tweaks = new()
        {
            new SmoothCameraTweak(),
            new InputSystemTweak(),
            new EventsSubscriberTweak(),
            new GarbageDisablerTweak(),
            new PatchesLoaderTweak(),
            new MethodsSilencerTweak(),
            new ModifiersTweak(),
            new RoomOffsetsTweak(),
        }; //not static to dispose the garbage inside tweaks

        [Inject] private readonly DiContainer _container;

        private void Awake()
        {
            PerformActions(ActionType.Inject);
            PerformActions(ActionType.Initialize);
        }
        private void Start()
        {
            PerformActions(ActionType.LateInitialize);
        }
        private void OnDestroy()
        {
            PerformActions(ActionType.Dispose);
        }

        private void PerformActions(ActionType action)
        {
            foreach (var item in _tweaks)
            {
                if (!item.CanBeInstalled) continue;

                try
                {
                    switch (action)
                    {
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
                }
                catch (Exception ex)
                {
                    Plugin.Log.Error($"[TweaksLoader] Error during attempting to perform {action} on {item.GetType().Name} tweak! \r\n {ex}");
                }
            }
        }
    }
}
