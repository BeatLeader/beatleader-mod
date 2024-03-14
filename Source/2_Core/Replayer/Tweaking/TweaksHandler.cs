using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

        [Inject] private readonly DiContainer _container = null!;

        public IList<GameTweak> Tweaks => _tweaks;

        private readonly ObservableCollection<GameTweak> _tweaks = new() {
            new AudioTimeSyncControllerTweak(),
            new InputSystemTweak(),
            new GarbageDisablerTweak(),
            new InteropsLoaderTweak(),
            new MethodsSilencerTweak(),
            new ModifiersTweak(),
            new RaycastBlockerTweak(),
            new RoomOffsetsTweak(),
            new ReplayFailTweak(),
            new ReplayFinishTweak(),
            new JumpDistanceTweak(),
            new ScoringTweak(),
            new SiraFPFCTweak()
        };

        #region Installation

        private void Awake() {
            HandleAddedTweaks(_tweaks, false);
        }

        private void Start() {
            HandleAddedTweaks(_tweaks);
            _tweaks.CollectionChanged += HandleCollectionChanged;
        }

        private void OnDestroy() {
            HandleRemovedTweaks(_tweaks);
        }

        private void PerformAction(ActionType action, IEnumerable<GameTweak> tweaks) {
            foreach (var tweak in tweaks) {
                if (action != ActionType.Inject && !tweak.CanBeInstalled) continue;
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
                    Plugin.Log.Error($"[TweaksLoader] Error during attempting to perform {action} on {tweak.GetType().Name} tweak!\r\n{ex}");
                }
            }
        }

        #endregion

        #region Collection

        private void HandleAddedTweaks(IEnumerable tweaks, bool late = true) {
            var items = (IEnumerable<GameTweak>)tweaks;
            PerformAction(ActionType.Inject, items);
            PerformAction(late ? ActionType.LateInitialize : ActionType.Initialize, items);
        }

        private void HandleRemovedTweaks(IEnumerable tweaks) {
            PerformAction(ActionType.Dispose, (IEnumerable<GameTweak>)tweaks);
        }

        private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    HandleAddedTweaks(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Remove:
                    HandleRemovedTweaks(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    HandleRemovedTweaks(e.OldItems);
                    HandleAddedTweaks(e.NewItems);
                    break;
            }
        }

        #endregion
    }
}
