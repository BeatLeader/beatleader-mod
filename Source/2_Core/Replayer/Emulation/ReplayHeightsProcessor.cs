using System;
using System.Collections.Generic;
using System.Reflection;
using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    public class ReplayHeightsProcessor : MonoBehaviour {
        #region Injection

        [InjectOptional] private readonly PlayerHeightDetector _heightDetector = null!;
        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;
        [Inject] private readonly IBeatmapTimeController _timeController = null!;

        #endregion

        #region Setup

        private static readonly FieldInfo heightChangeEventInfo =
            typeof(PlayerHeightDetector).GetField(
                nameof(PlayerHeightDetector
                    .playerHeightDidChangeEvent), ReflectionUtils.DefaultFlags
            )!;

        private readonly HarmonySilencer _heightDetectorUpdateSilencer = new(
            typeof(PlayerHeightDetector).GetMethod(
                nameof(PlayerHeightDetector
                    .LateUpdate), ReflectionUtils.DefaultFlags
            )!
        );

        private readonly Dictionary<IVirtualPlayer, LinkedList<HeightEvent>?> _cachedEvents = new();

        private LinkedList<HeightEvent>? _heights;
        private LinkedListNode<HeightEvent>? _lastNode;

        private void Start() {
            if (!_heightDetector) {
                Destroy(gameObject);
                return;
            }
            _playersManager.PriorityPlayerWasChangedEvent += HandlePriorityPlayerChanged;
            _timeController.SongWasRewoundEvent += HandleSongWasRewound;
            HandlePriorityPlayerChanged(_playersManager.PriorityPlayer!);
        }

        private void OnDestroy() {
            _playersManager.PriorityPlayerWasChangedEvent -= HandlePriorityPlayerChanged;
            _timeController.SongWasRewoundEvent -= HandleSongWasRewound;
            _heightDetectorUpdateSilencer.Dispose();
        }

        #endregion

        #region Height Handling

        private void LateUpdate() {
            if (_lastNode is null || _lastNode.Value.time > _timeController.SongTime) return;
            HandleHeightChange(_lastNode.Value.height);
            _lastNode = _lastNode.Next;
        }

        private void HandleHeightChange(float height) {
            ((Delegate?)heightChangeEventInfo.GetValue(_heightDetector))?.DynamicInvoke(height);
        }

        #endregion

        #region Callbacks

        private void HandlePriorityPlayerChanged(IVirtualPlayer player) {
            if (!_cachedEvents.TryGetValue(player, out _heights)) {
                _heights = player.Replay.HeightEvents is not { } heights ? null : new(heights);
                _cachedEvents[player] = _heights;
            }
            _lastNode = _heights?.First;
        }

        private void HandleSongWasRewound(float songTime) {
            _lastNode = _heights?.FindNode(x => x.Value.time > songTime) ?? null;
            if (_lastNode?.Previous is { } prev) HandleHeightChange(prev.Value.height);
        }

        #endregion
    }
}