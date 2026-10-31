using System;
using System.Linq;
using BeatLeader.Dance;
using BeatLeader.Models;
using BeatLeader.Utils;
using JetBrains.Annotations;
using Libraries.HM.HMLib.VR;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using Zenject;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace BeatLeader {
    internal class DanceDreadSummary {
        public bool Died;
        public float HauntedSeconds;
        public float PlayedSeconds;
        public int Escapes;
        public float DeathTime;
        public float LongestStillSeconds;
        public bool Submitted;
    }

    internal class DanceDreadController : IInitializable, ITickable, IDisposable {
        #region Static

        public static bool IsHaunting { get; private set; }
        public static bool IsWarning { get; private set; }
        public static float TrailDroop { get; private set; }
        public static float NoteGray { get; private set; }
        public static float NoteDim { get; private set; }
        public static DanceDreadSummary? LastSummary { get; set; }

        public const double DreadLine = 0.85;

        #endregion

        #region Injection

        [Inject, UsedImplicitly] private readonly DanceModeTracker _tracker = null!;
        [Inject, UsedImplicitly] private readonly BeatmapKey _beatmapKey;
        [Inject, UsedImplicitly] private readonly AudioTimeSyncController _timeSyncController = null!;
        [Inject, UsedImplicitly] private readonly GameEnergyCounter _energyCounter = null!;
        [InjectOptional, UsedImplicitly] private readonly IBpmController? _bpmController;
        [InjectOptional, UsedImplicitly] private readonly MainAudioEffects? _mainAudioEffects;
        [InjectOptional, UsedImplicitly] private readonly AudioManager? _audioManager;
        [InjectOptional, UsedImplicitly] private readonly HapticFeedbackManager? _hapticFeedbackManager;
        [InjectOptional, UsedImplicitly] private readonly IGamePause? _gamePause;
        [InjectOptional, UsedImplicitly] private readonly GameplayCoreSceneSetupData? _sceneSetupData;
        [Inject, UsedImplicitly] private readonly DiContainer _container = null!;

        #endregion

        #region Lifecycle

        private readonly DanceDreadApparitions _apparitions = new();
        private readonly DanceDreadGhost _ghost = new();
        private DanceDreadSummary _summary = null!;
        private bool _eventLevel;
        private bool _flickerAllowed;
        private float _basePitch = 1f;

        public void Initialize() {
            _eventLevel = DanceEvent.IsEventLevel(_beatmapKey);
            var settings = _sceneSetupData?.playerSpecificSettings;
            var effectsFilter = _beatmapKey.difficulty == BeatmapDifficulty.ExpertPlus
                ? settings?.environmentEffectsFilterExpertPlusPreset
                : settings?.environmentEffectsFilterDefaultPreset;
            _flickerAllowed = effectsFilter is null or EnvironmentEffectsFilterPreset.AllEffects;
            if (_eventLevel) _apparitions.Preload();
        }

        private void Start() {
            _summary = new DanceDreadSummary();
            LastSummary = _summary;
            _basePitch = _audioManager?._musicPitch ?? 1f;
            _energyCounter.gameEnergyDidReach0Event += HandleEnergyDidReach0;
            ScoreUtil.ReplayUploadStartedEvent += HandleUploadStarted;
            CreateAudio();
            _ghost.Load(_beatmapKey);
            IsHaunting = true;
        }

        public void Dispose() {
            if (!IsHaunting) return;
            IsHaunting = false;
            IsWarning = false;
            TrailDroop = 0f;
            NoteGray = 0f;
            NoteDim = 0f;
            _apparitions.Dispose();
            _ghost.Dispose();
            _energyCounter.gameEnergyDidReach0Event -= HandleEnergyDidReach0;
            ScoreUtil.ReplayUploadStartedEvent -= HandleUploadStarted;
            DanceDreadLights.Reset();
            if (_audioManager != null && _audioManager._audioMixer != null) _audioManager.musicPitch = _basePitch;
            if (_mainAudioEffects != null && !_mainAudioEffects.enabled) {
                var filter = _mainAudioEffects._audioLowPassFilter;
                filter.cutoffFrequency = MaxCutoff;
                filter.enabled = false;
            }
            if (_vignette != null) Object.Destroy(_vignette);
            RestoreFog();
            if (_audioSource != null) Object.Destroy(_audioSource.gameObject);
            if (_voiceSource != null) Object.Destroy(_voiceSource.gameObject);
            foreach (var whisper in _whisperSources) {
                if (whisper != null) Object.Destroy(whisper.gameObject);
            }
        }

        public void Tick() {
            if (!_eventLevel || _tracker.Live is not { } live) return;
            if (!IsHaunting) Start();
            if (_gamePause?.isPaused ?? false) return;

            UpdateDread(live);
            var dt = Time.deltaTime;
            _shown += (_dread - _shown) * (1f - Mathf.Exp(-dt / EaseSeconds));

            var songTime = _timeSyncController.songTime;
            if (_shown > 0.5f && songTime > _lastSongTime) {
                _summary.HauntedSeconds += songTime - _lastSongTime;
            }
            _summary.PlayedSeconds = Mathf.Max(_summary.PlayedSeconds, songTime);
            _lastSongTime = songTime;

            var lightsOut = Stage(LightsOut);
            UpdateLowPass(dt);
            UpdatePitch();
            UpdateLights(dt);
            UpdateVignette();
            UpdateFog(lightsOut);
            TrailDroop = 0.45f * lightsOut;
            NoteGray = 0.75f * Stage(0.1f);
            NoteDim = 0.5f * lightsOut;
            _apparitions.Update(lightsOut, Stage(TheDark), _stops, NearDeath(), dt);
            _ghost.Update(lightsOut, songTime, _container);
            IsWarning = _shown >= 0.85f;
            UpdateHeartbeat(songTime);
            UpdateWhispers(dt);
            UpdateVoice(dt);
            UpdateRelief();
        }

        private void HandleUploadStarted(Models.Replay.Replay replay, PlayEndData data) {
            _summary.Submitted = true;
        }

        private void HandleEnergyDidReach0() {
            _summary.Died = true;
            _summary.DeathTime = _timeSyncController.songTime;
            Play(DanceDreadSounds.Sting, 0.9f);
            Say("yours", true);
        }

        #endregion

        #region Dread

        private const float EaseSeconds = 1f;
        private const float LightsOut = 0.3f;
        private const float TheDark = 0.55f;
        private const float NearDeathEnergy = 0.12f;

        private float _dread;
        private float _shown;
        private float _lastLiveTime = -1f;
        private float _lastSongTime;
        private int _stops;
        private bool _wasStill;
        private float _stillSeconds;

        private void UpdateDread(DanceLive live) {
            if (live.SongTime == _lastLiveTime) return;
            var step = _lastLiveTime < 0f ? 0f : live.SongTime - _lastLiveTime;
            _lastLiveTime = live.SongTime;
            if (!live.Ready) {
                _dread = 0f;
                return;
            }
            if (live.CountedFraction < DanceScorer.LiveMinCounted) return;
            var still = live.Level < DreadLine;
            if (still) {
                _dread += 0.12f + 0.12f * (float)((DreadLine - live.Level) / DreadLine);
                _stillSeconds += step;
                _summary.LongestStillSeconds = Mathf.Max(_summary.LongestStillSeconds, _stillSeconds);
                if (!_wasStill) _stops++;
            } else {
                _dread -= 0.15f;
                _stillSeconds = 0f;
            }
            _wasStill = still;
            _dread = Mathf.Clamp01(_dread);
        }

        private float Stage(float start, float end = 1f) {
            return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(start, end, _shown));
        }

        private float NearDeath() {
            if (Stage(LightsOut) <= 0f) return 0f;
            return Mathf.InverseLerp(NearDeathEnergy, 0f, _energyCounter.energy);
        }

        #endregion

        #region Music

        private const float MaxCutoff = 22000f;
        private const float MinCutoff = 1500f;

        private float _cutoff = MaxCutoff;
        private float _appliedPitch = 1f;

        private void UpdateLowPass(float dt) {
            if (_mainAudioEffects == null || _mainAudioEffects.enabled) return;
            var filter = _mainAudioEffects._audioLowPassFilter;
            var amount = 0.45f * Stage(0.03f, 0.35f) + 0.55f * Stage(0.35f);
            var target = MaxCutoff * Mathf.Pow(MinCutoff / MaxCutoff, amount);
            _cutoff = Mathf.Exp(Mathf.Lerp(Mathf.Log(_cutoff), Mathf.Log(target), 1f - Mathf.Exp(-dt / 0.3f)));
            filter.enabled = _cutoff < MaxCutoff * 0.95f;
            filter.cutoffFrequency = filter.enabled ? _cutoff : MaxCutoff;
        }

        private void UpdatePitch() {
            if (_audioManager == null) return;
            var pitch = Mathf.Pow(2f, -2f * Stage(0.8f) / 12f);
            if (Mathf.Abs(pitch - _appliedPitch) < 0.001f) return;
            _appliedPitch = pitch;
            _audioManager.musicPitch = _basePitch * pitch;
        }

        #endregion

        #region Lights

        private float _nextFlicker = 3f;
        private float _flickerTimer;
        private float _flickerOff;

        private void UpdateLights(float dt) {
            var amount = Stage(0.2f);
            var dim = 0.8f * amount;
            var dark = Stage(TheDark);
            if (_flickerAllowed && dark > 0f) {
                _flickerTimer += dt;
                if (_flickerOff > 0f) {
                    _flickerOff -= dt;
                    dim = 1f - (1f - dim) * 0.25f;
                } else if (_flickerTimer >= _nextFlicker) {
                    _flickerTimer = 0f;
                    _flickerOff = Random.Range(0.12f, 0.25f);
                    _nextFlicker = Random.value < 0.4f ? 0.35f : Mathf.Lerp(6f, 2f, dark) * Random.Range(0.7f, 1.3f);
                }
            }
            DanceDreadLights.Apply(dim, 0.6f * amount, 0.5f * Stage(0.03f));
        }

        #endregion

        #region Fog

        private const float DreadFogThickness = 6f;
        private const float DreadFogMinAttenuation = 0.02f;

        private BloomFogSO? _bloomFog;
        private BloomFogEnvironmentParams? _dreadFog;
        private BloomFogEnvironmentParams? _savedTransitionFog;
        private float _savedTransition;

        private void UpdateFog(float amount) {
            if (_bloomFog == null) {
                if (amount <= 0f) return;
                _bloomFog = Resources.FindObjectsOfTypeAll<BloomFogSO>().FirstOrDefault();
                if (_bloomFog == null) return;
                _savedTransitionFog = _bloomFog.transitionFogParams;
                _savedTransition = _bloomFog.transition;
                var current = _bloomFog.defaultForParams;
                _dreadFog = ScriptableObject.CreateInstance<BloomFogEnvironmentParams>();
                _dreadFog.attenuation = Mathf.Max(current.attenuation * DreadFogThickness, DreadFogMinAttenuation);
                _dreadFog.offset = current.offset * 0.3f;
                _dreadFog.heightFogStartY = current.heightFogStartY;
                _dreadFog.heightFogHeight = current.heightFogHeight;
                _dreadFog.autoExposureLimit = current.autoExposureLimit;
                _dreadFog.legacyAutoExposure = current.legacyAutoExposure;
                _dreadFog.noteSpawnIntensity = current.noteSpawnIntensity;
                _bloomFog.transitionFogParams = _dreadFog;
            }
            _bloomFog.transition = amount;
        }

        private void RestoreFog() {
            if (_bloomFog != null) {
                _bloomFog.transitionFogParams = _savedTransitionFog;
                _bloomFog.transition = _savedTransition;
            }
            if (_dreadFog != null) Object.Destroy(_dreadFog);
        }

        #endregion

        #region Vignette

        private static Sprite? _vignetteSprite;

        private GameObject? _vignette;
        private Image? _vignetteImage;

        private void UpdateVignette() {
            var alpha = 0.35f * Stage(0.02f, 0.3f) + 0.57f * Stage(TheDark);
            if (_vignette == null) {
                if (alpha <= 0f) return;
                CreateVignette();
            }
            if (_vignetteImage != null) _vignetteImage.color = new Color(0f, 0f, 0f, alpha);
        }

        private void CreateVignette() {
            var camera = Camera.main;
            if (camera == null) return;
            _vignetteImage = DanceDreadApparitions.CreateImage("DanceDreadVignette", VignetteSprite, Vector2.one * 3f, GameResources.UINoGlowMaterial, out _vignette);
            _vignette.GetComponent<Canvas>().sortingOrder = 32000;
            _vignette.transform.SetParent(camera.transform, false);
            _vignette.transform.localPosition = new Vector3(0f, 0f, 0.4f);
        }

        private static Sprite VignetteSprite {
            get {
                if (_vignetteSprite != null) return _vignetteSprite;
                const int size = 256;
                var alpha = new float[size * size];
                for (var y = 0; y < size; y++) {
                    for (var x = 0; x < size; x++) {
                        var dx = (x + 0.5f) / size * 2f - 1f;
                        var dy = (y + 0.5f) / size * 2f - 1f;
                        alpha[y * size + x] = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.33f, 0.75f, Mathf.Sqrt(dx * dx + dy * dy)));
                    }
                }
                _vignetteSprite = DanceDreadApparitions.CreateAlphaSprite(size, size, alpha, new Vector2(0.5f, 0.5f));
                return _vignetteSprite;
            }
        }

        #endregion

        #region Sounds

        private const float HeartbeatSeconds = 0.8f;
        private const float WhisperDistance = 1.8f;

        private AudioSource? _audioSource;
        private HapticPresetSO? _hapticPreset;
        private float _nextBeat = float.NaN;
        private bool _wasHaunted;

        private readonly AudioSource?[] _whisperSources = new AudioSource?[2];
        private readonly float[] _whisperAngles = new float[2];
        private readonly float[] _whisperSpeeds = new float[2];
        private float _whisperTimer;
        private float _nextWhisper = 3f;
        private int _whisperCount;

        private void CreateAudio() {
            _audioSource = new GameObject("DanceDreadAudio").AddComponent<AudioSource>();
            _audioSource.spatialBlend = 0f;
            _audioSource.playOnAwake = false;
            _hapticPreset = ScriptableObject.CreateInstance<HapticPresetSO>();
            _hapticPreset._duration = 0.08f;
            _hapticPreset._frequency = 0.2f;
            _hapticPreset._useAdvancedHapticsOnSupportedPlatforms = false;
        }

        private void Play(AudioClip clip, float volume) {
            if (_audioSource == null || volume <= 0f) return;
            _audioSource.PlayOneShot(clip, volume);
        }

        private void UpdateHeartbeat(float songTime) {
            if (songTime <= 0f) return;
            var beat = _bpmController?.oneBeatDuration ?? 0f;
            var interval = beat > 0.05f ? beat * Mathf.Max(1, Mathf.RoundToInt(HeartbeatSeconds / beat)) : HeartbeatSeconds;
            var following = (Mathf.Floor(songTime / interval) + 1f) * interval;
            if (float.IsNaN(_nextBeat)) _nextBeat = following;
            if (songTime < _nextBeat) return;
            _nextBeat = following;

            var volume = 0.4f * Stage(0.02f, 0.25f) + 0.5f * Stage(0.25f);
            if (volume <= 0.01f) return;
            Play(DanceDreadSounds.Heartbeat, 0.9f * volume);

            var haptics = Stage(LightsOut);
            if (_hapticFeedbackManager == null || _hapticPreset == null || haptics <= 0.01f) return;
            _hapticPreset._strength = 0.7f * haptics;
            _hapticFeedbackManager.PlayHapticFeedback(XRNode.LeftHand, _hapticPreset);
            _hapticFeedbackManager.PlayHapticFeedback(XRNode.RightHand, _hapticPreset);
        }

        private void UpdateRelief() {
            if (_shown >= 0.5f) {
                _wasHaunted = true;
            } else if (_wasHaunted && _shown < 0.1f) {
                _wasHaunted = false;
                _summary.Escapes++;
                Play(DanceDreadSounds.Relief, 0.6f);
                Say("not-yet", true);
            }
        }

        private void UpdateWhispers(float dt) {
            var camera = Camera.main;
            var head = camera != null ? camera.transform.position : new Vector3(0f, 1.7f, 0f);
            for (var i = 0; i < _whisperSources.Length; i++) {
                var source = _whisperSources[i];
                if (source == null || !source.isPlaying) continue;
                _whisperAngles[i] += _whisperSpeeds[i] * dt;
                source.transform.position = AroundHead(head, _whisperAngles[i]);
            }
            var early = Stage(0.15f, 0.4f);
            if (early <= 0f) return;
            _whisperTimer += dt;
            if (_whisperTimer < _nextWhisper) return;
            var dark = Stage(TheDark);
            _whisperTimer = 0f;
            _nextWhisper = Random.Range(4f, 9f) * Mathf.Lerp(1.6f, 0.8f, dark);
            var index = _whisperCount++ % _whisperSources.Length;
            var whisper = _whisperSources[index] ??= CreateSpatialSource("DanceDreadWhisper", 1f);
            _whisperAngles[index] = Random.Range(0f, 360f);
            _whisperSpeeds[index] = Random.Range(30f, 70f) * (Random.value < 0.5f ? -1f : 1f);
            var clips = DanceDreadSounds.Whispers;
            whisper.clip = clips[Random.Range(0, clips.Length)];
            whisper.volume = 0.25f * early + 0.35f * dark;
            whisper.transform.position = AroundHead(head, _whisperAngles[index]);
            whisper.Play();
        }

        private static Vector3 AroundHead(Vector3 head, float angle) {
            return head + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * WhisperDistance;
        }

        private static AudioSource CreateSpatialSource(string name, float minDistance) {
            var source = new GameObject(name).AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = minDistance;
            source.maxDistance = 20f;
            source.dopplerLevel = 0f;
            return source;
        }

        #endregion

        #region Voice

        private static readonly string[] stopLines = { "dont-stop", "we-see-you", "keep-moving" };

        private AudioSource? _voiceSource;
        private float _voiceCooldown;
        private int _voiceStops;
        private bool _wasWarning;
        private bool _spokeInTheDark;
        private string? _lastStopLine;

        private void UpdateVoice(float dt) {
            _voiceCooldown -= dt;
            if (!_spokeInTheDark && Stage(TheDark) > 0.3f) _spokeInTheDark = Say("dance");
            if (IsWarning && !_wasWarning) Say("dance-or-die", true);
            _wasWarning = IsWarning;
            if (_stops == _voiceStops) return;
            _voiceStops = _stops;
            if (Stage(LightsOut) <= 0f) return;
            var index = Random.Range(0, stopLines.Length);
            if (stopLines[index] == _lastStopLine) index = (index + 1) % stopLines.Length;
            if (Say(stopLines[index])) _lastStopLine = stopLines[index];
        }

        private bool Say(string line, bool force = false) {
            if (!force && _voiceCooldown > 0f) return false;
            var clip = DanceDreadSounds.Voice(line);
            var camera = Camera.main;
            if (clip == null || camera == null) return false;
            if (_voiceSource == null) _voiceSource = CreateSpatialSource("DanceDreadVoice", 0.5f);
            var head = camera.transform;
            var back = Vector3.ProjectOnPlane(-head.forward, Vector3.up).normalized;
            var side = Vector3.Cross(Vector3.up, back) * (Random.value < 0.5f ? -0.25f : 0.25f);
            _voiceSource.transform.position = head.position + back * 0.5f + side;
            _voiceSource.clip = clip;
            _voiceSource.Play();
            _voiceCooldown = 8f;
            return true;
        }

        #endregion
    }
}
