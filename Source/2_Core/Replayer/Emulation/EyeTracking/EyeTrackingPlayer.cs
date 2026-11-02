using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using HMUI;
using UnityEngine;
using Zenject;
using EyeTrackingData = BeatLeader.Models.Replay.EyeTrackingData;

namespace BeatLeader.Replayer.Emulation {
    internal class EyeTrackingPlayer : MonoBehaviour {
        private const float BeamStart = 0.08f;
        private const float BeamLength = 6f;
        private const float BeamWidth = 0.024f;
        private const float RingDistance = 3f;
        private const float RingSize = 0.11f;
        private const float RingThickness = 0.27f;
        private const float MinOpenness = 0.02f;

        private static readonly Color color = new(1f, 0.85f, 0.4f);

        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;
        [Inject] private readonly IBeatmapTimeController _timeController = null!;
        [Inject] private readonly ICameraController _cameraController = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly ReplayerExtraObjectsProvider _extraObjects = null!;

        public static bool HasData(IReplay replay) {
            return replay.CustomData.ContainsKey(EyeTrackingData.CustomDataKey);
        }

        #region Playback

        private readonly Dictionary<IReplay, EyeTrackingTrack?> _tracks = new();

        private void LateUpdate() {
            var track = GetTrack(_playersManager.PrimaryPlayer.Replay);
            var isEnabled = _launchData.Settings.BodySettings.RequireConfig<BasicBodySettings>().EyeTrackingEnabled;
            if (track == null || !isEnabled ||!track.TryGetGaze(_timeController.SongTime, out var headPosition, out var direction, out var openness) || openness <= MinOpenness) {
                _canvas.enabled = false;
                return;
            }

            _canvas.enabled = true;
            transform.localPosition = headPosition;
            transform.localRotation = Quaternion.LookRotation(direction);
            _beam.color = color * openness;
            _ring.color = color * openness;
            TurnBeamToCamera();
        }

        private EyeTrackingTrack? GetTrack(IReplay replay) {
            if (!_tracks.TryGetValue(replay, out var track)) {
                track = replay.CustomData.TryGetValue(EyeTrackingData.CustomDataKey, out var bytes) && EyeTrackingData.Decode(bytes) is { } data ?
                    new EyeTrackingTrack(data, replay.PlayerMovementFrames) :
                    null;
                _tracks[replay] = track;
            }
            return track;
        }

        #endregion

        #region Visuals

        private Canvas _canvas = null!;
        private ImageView _beam = null!;
        private ImageView _ring = null!;

        private void Awake() {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.enabled = false;
            transform.SetParent(_extraObjects.VRGameCore, false);

            _beam = CreateImage("Beam", CreateBeamTexture(), new(BeamLength, BeamWidth), new(0f, 0.5f));
            _beam.rectTransform.localPosition = new(0f, 0f, BeamStart);
            _ring = CreateImage("Ring", CreateRingTexture(), new(RingSize, RingSize), new(0.5f, 0.5f));
            _ring.rectTransform.localPosition = new(0f, 0f, RingDistance);
        }

        private void OnDestroy() {
            foreach (var image in new[] { _beam, _ring }) {
                Destroy(image.sprite.texture);
                Destroy(image.sprite);
            }
        }

        // The beam is flat, so it's turned around its axis to face the camera
        private void TurnBeamToCamera() {
            var camera = _cameraController.Camera;
            if (camera == null) camera = Camera.main;
            if (camera == null) return;

            var axis = transform.forward;
            var facing = Vector3.ProjectOnPlane(_beam.rectTransform.position - camera.transform.position, axis);
            if (facing.sqrMagnitude < 1e-6f) return;
            _beam.rectTransform.rotation = Quaternion.LookRotation(facing, Vector3.Cross(facing, axis));
        }

        private ImageView CreateImage(string name, Texture2D texture, Vector2 size, Vector2 pivot) {
            var image = new GameObject(name).AddComponent<ImageView>();
            image.rectTransform.SetParent(transform, false);
            image.rectTransform.sizeDelta = size;
            image.rectTransform.pivot = pivot;
            image.sprite = Sprite.Create(texture, new(0f, 0f, texture.width, texture.height), new(0.5f, 0.5f));
            image.material = GameResources.UINoGlowAdditiveMaterial;
            return image;
        }

        private static Texture2D CreateBeamTexture() {
            return CreateTexture(128, 32, static (u, v) => {
                var halfWidth = Mathf.Lerp(1f, 0.25f, u);
                var edge = Mathf.Clamp01((1f - Mathf.Abs(2f * v - 1f) / halfWidth) * 2f);
                return (1f - u) * (1f - u) * edge;
            });
        }

        private static Texture2D CreateRingTexture() {
            return CreateTexture(64, 64, static (u, v) => {
                var radius = new Vector2(2f * u - 1f, 2f * v - 1f).magnitude;
                return Mathf.Clamp01((1f - radius) * 32f) * Mathf.Clamp01((radius - 1f + RingThickness) * 32f);
            });
        }

        private static Texture2D CreateTexture(int width, int height, Func<float, float, float> getIntensity) {
            var pixels = new Color[width * height];
            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    var intensity = getIntensity((x + 0.5f) / width, (y + 0.5f) / height);
                    pixels[y * width + x] = new(intensity, intensity, intensity, intensity);
                }
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true) {
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return texture;
        }

        #endregion
    }
}
