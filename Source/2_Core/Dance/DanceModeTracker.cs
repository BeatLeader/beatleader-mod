using System;
using System.Collections.Generic;
using System.Reflection;
using BeatLeader.Dance;
using BeatLeader.Models.Replay;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BeatLeader {
    internal class DanceModeTracker : IInitializable, ITickable, IDisposable {
        #region Static

        public static bool IsRunning { get; private set; }

        public static bool IsDraining { get; private set; }

        public static DanceResult? LastResult { get; set; }

        #endregion

        #region Injection

        [Inject, UsedImplicitly] private readonly IReadonlyBeatmapData _beatmapData = null!;
        [Inject, UsedImplicitly] private readonly BeatmapKey _beatmapKey;
        [Inject, UsedImplicitly] private readonly AudioTimeSyncController _timeSyncController = null!;
        [Inject, UsedImplicitly] private readonly AudioTimeSyncController.InitData _timeSyncInitData = null!;
        [Inject, UsedImplicitly] private readonly GameEnergyCounter _energyCounter = null!;

        #endregion

        #region Lifecycle

        private DanceScorer? _scorer;
        private float _lastSongTime;

        internal DanceLive? Live => _scorer?.Live;

        public void Initialize() {
            if (_beatmapKey.beatmapCharacteristic.serializedName != "Standard") {
                Plugin.Log.Info("Dance Mode only scores Standard maps");
                return;
            }
            var audioClip = _timeSyncInitData.audioClip;
            var songLength = audioClip != null ? audioClip.length : 0f;
            _scorer = new DanceScorer(GetNoteTimes(_beatmapData), GetWalls(_beatmapData), _timeSyncInitData.timeScale, songLength);
            _lastSongTime = _timeSyncController.songTime;
            IsRunning = true;
        }

        public void Dispose() {
            IsRunning = false;
            IsDraining = false;
        }

        public void Tick() {
            if (_scorer == null) return;
            var songTime = _timeSyncController.songTime;
            var deltaTime = songTime - _lastSongTime;
            _lastSongTime = songTime;

            var live = _scorer.Live;
            IsDraining = live.ShouldDrain;
            if (IsDraining && deltaTime > 0f) {
                _energyCounter._nextFrameEnergyChange -= (float)(DanceScorer.SuggestedDrainPerSecond * deltaTime);
            }
            RefreshHud(live);
        }

        #endregion

        #region Recorder

        public void AddFrame(Frame frame) {
            if (_scorer == null) return;
            var head = frame.head;
            var left = frame.leftHand.position;
            var right = frame.rightHand.position;
            _scorer.AddFrame(
                frame.time,
                head.position.x, head.position.y, head.position.z,
                head.rotation.x, head.rotation.y, head.rotation.z, head.rotation.w,
                left.x, left.y, left.z,
                right.x, right.y, right.z
            );
        }

        public void AddPause(float songTime) {
            _scorer?.AddPause(songTime);
        }

        public void Finish() {
            if (_scorer == null) return;
            LastResult = _scorer.Finish();
            Plugin.Log.Info(LastResult.Valid
                ? $"Dance score: {LastResult.Score:F4}, vote share: {LastResult.VoteShare:F3}"
                : "Dance score: not enough play to judge");
        }

        #endregion

        #region Beatmap

        internal static List<float> GetNoteTimes(IReadonlyBeatmapData beatmapData) {
            var times = new List<float>();
            foreach (var note in beatmapData.GetBeatmapDataItems<NoteData>(0)) {
                if (note.gameplayType == NoteData.GameplayType.Bomb || IsFake(GetCustomData(note))) continue;
                times.Add(note.time);
            }
            foreach (var slider in beatmapData.GetBeatmapDataItems<SliderData>(0)) {
                if (slider.sliderType != SliderData.Type.Burst || IsFake(GetCustomData(slider))) continue;
                AddChainLinkTimes(times, slider);
            }
            return times;
        }

        private static void AddChainLinkTimes(List<float> times, SliderData chain) {
            for (var i = 1; i < chain.sliceCount; i++) {
                times.Add(Mathf.LerpUnclamped(chain.time, chain.tailTime, (float)i / (chain.sliceCount - 1)));
            }
        }

        internal static List<DanceWall> GetWalls(IReadonlyBeatmapData beatmapData) {
            var walls = new List<DanceWall>();
            foreach (var obstacle in beatmapData.GetBeatmapDataItems<ObstacleData>(0)) {
                float x = FromMappingExtensions(obstacle.lineIndex), y = (int)obstacle.lineLayer;
                float width = obstacle.width >= 1000 ? (obstacle.width - 1000) / 1000f : obstacle.width, height = obstacle.height;

                var customData = GetCustomData(obstacle);
                if (customData != null) {
                    if (IsFake(customData) || IsUninteractable(customData)) continue;
                    if (TryGetPoint(customData, "coordinates", "_position", out var px, out var py) && px.HasValue && py.HasValue) {
                        x = px.Value + 2;
                        y = py.Value;
                    }
                    if (TryGetPoint(customData, "size", "_scale", out var sw, out var sh)) {
                        width = sw ?? width;
                        height = sh ?? height;
                    }
                }
                if (obstacle.duration < 0 || width <= 0 || height <= 0) continue;
                walls.Add(DanceWall.FromGrid(obstacle.time, obstacle.duration, x, y, width, height));
            }
            return walls;
        }

        private static float FromMappingExtensions(float value) {
            if (value >= 1000) return (value - 1000) / 1000;
            if (value <= -1000) return (value + 1000) / 1000;
            return value;
        }

        private static readonly Dictionary<Type, PropertyInfo?> customDataProperties = new();

        private static IDictionary<string, object?>? GetCustomData(BeatmapDataItem item) {
            var type = item.GetType();
            if (!customDataProperties.TryGetValue(type, out var property)) {
                property = type.GetProperty("customData", BindingFlags.Instance | BindingFlags.Public);
                customDataProperties[type] = property;
            }
            return property?.GetValue(item) as IDictionary<string, object?>;
        }

        private static bool IsFake(IDictionary<string, object?>? customData) {
            return customData != null && (Is(customData, "NE_fake", true) || Is(customData, "_fake", true));
        }

        private static bool IsUninteractable(IDictionary<string, object?> customData) {
            return Is(customData, "uninteractable", true) || Is(customData, "_interactable", false);
        }

        private static bool Is(IDictionary<string, object?> customData, string key, bool value) {
            return customData.TryGetValue(key, out var obj) && obj is bool b && b == value;
        }

        private static bool TryGetPoint(IDictionary<string, object?> customData, string key, string v2Key, out float? x, out float? y) {
            x = y = null;
            if (!customData.TryGetValue(key, out var obj) && !customData.TryGetValue(v2Key, out obj)) return false;
            if (obj is not IList<object?> list || list.Count < 2) return false;
            x = ToFloat(list[0]);
            y = ToFloat(list[1]);
            return true;
        }

        private static float? ToFloat(object? obj) {
            return obj is IConvertible convertible ? convertible.ToSingle(null) : null;
        }

        #endregion

        #region HUD

        private const string LabelText = "<size=60%>DANCE</size> ";
        private const string WarningLabelText = "<size=60%><color=#E61A1A>DANCE... OR DIE</color></size> ";
        private const string DrainColor = "#FF5050";
        private const string DreadColor = "#FF9A1F";

        private TextMeshProUGUI? _levelText;
        private float _hudSongTime = -1f;

        private void RefreshHud(DanceLive live) {
            if (live.SongTime == _hudSongTime && _levelText != null) return;
            _hudSongTime = live.SongTime;
            if (_levelText == null) {
                var scoreUI = Object.FindAnyObjectByType<ScoreUIController>();
                if (scoreUI == null) return;
                _levelText = CopyScoreText(scoreUI._scoreText, "DanceLevelText");
                scoreUI._scoreText.enabled = false;
            }
            if (!live.Ready) {
                _levelText.text = LabelText + "...";
                return;
            }
            var level = $"{live.Level * 100:F0}%";
            var label = DanceDreadController.IsWarning ? WarningLabelText : LabelText;
            var color = live.ShouldDrain ? DrainColor
                : DanceDreadController.IsHaunting && live.Level < DanceDreadController.DreadLine ? DreadColor
                : null;
            _levelText.text = label + (color != null ? $"<color={color}>{level}</color>" : level);
        }

        private static TextMeshProUGUI CopyScoreText(TextMeshProUGUI scoreText, string name) {
            var copy = Object.Instantiate(scoreText, scoreText.transform.parent, false);
            copy.name = name;
            for (var i = copy.transform.childCount - 1; i >= 0; i--) {
                Object.DestroyImmediate(copy.transform.GetChild(i).gameObject);
            }
            foreach (var component in copy.GetComponents<Component>()) {
                if (component is UnityEngine.Transform or CanvasRenderer or TMP_Text) continue;
                Object.DestroyImmediate(component);
            }
            copy.enabled = true;
            copy.richText = true;
            copy.textWrappingMode = TextWrappingModes.NoWrap;
            return copy;
        }

        #endregion
    }
}
