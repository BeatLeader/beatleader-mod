using BeatLeader.API;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.UI.Hub;
using BeatLeader.WebRequests;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BeatLeader.Components {
    internal class ExperienceBar : ReeUIComponentV2 {
        #region Properties

        private static readonly int GradientTPropertyId = Shader.PropertyToID("_GradientT");
        private static readonly int ExpProgressPropertyId = Shader.PropertyToID("_ExpProgress");
        private static readonly int SessionProgressPropertyId = Shader.PropertyToID("_SessionProgress");
        private static readonly int HighlightPropertyId = Shader.PropertyToID("_Highlight");

        private int _level;
        private float _gradientT;
        private float _expProgress;
        private float _sessionProgress;
        private float _requiredExp;

        private bool _initialized;
        private int _levelUpValue;
        private int _levelUpCount;
        private bool _isIdle;
        private bool _reverse;
        private bool _isAnimated;
        private float _elapsedTime;
        private float _elapsedTime2;
        private readonly float _animationDuration = 3f;
        private float _targetValue;

        private float _highlight;

        #endregion

        #region Animation

        private void Update() {
            if (_initialized && _level != 100 && (_isIdle || _isAnimated)) {
                _elapsedTime += Time.deltaTime;
                if (_isIdle) {
                    // Idle highlight animation, slowly pulses the highlight value
                    float t = Mathf.Clamp01(_elapsedTime / _animationDuration);
                    if (!_reverse) {
                        _highlight = Mathf.Lerp(0f, 1f, t);
                    } else {
                        _highlight = Mathf.Lerp(1f, 0f, t);
                    }

                    if (_elapsedTime >= _animationDuration) {
                        _elapsedTime = 0f;
                        _reverse = !_reverse;
                    }
                } else if (_isAnimated) {
                    // Experience filling the bar animation with wave effect
                    if (_levelUpValue > 0) { // Level up animation
                        _elapsedTime2 += Time.deltaTime;
                        float t = Mathf.Clamp01(_elapsedTime2 * (_levelUpValue + 1) / _animationDuration);
                        float targetValue = 1 - _expProgress;
                        if (_levelUpCount != 0) { // Before final level
                            _sessionProgress = Mathf.Lerp(0f, targetValue, t);
                        } else { // Final level
                            _sessionProgress = Mathf.Lerp(0f, _targetValue, t);
                        }
                        // Consider the number of level ups to speed up the animation
                        if (_elapsedTime2 * (_levelUpValue + 1) >= _animationDuration) {
                            if (_levelUpCount != 0) { // Reset for next level up
                                _levelUpCount--;
                                SetLevelText(++_level);
                                _expProgress = 0f;
                                _sessionProgress = 0f;
                                _elapsedTime2 = 0f;
                            }
                        }
                    } else { // Non-level up animation
                        _gradientT = Mathf.Clamp01(_elapsedTime / _animationDuration);
                        _sessionProgress = Mathf.Lerp(0, _targetValue, _gradientT);
                    }

                    // Forcefully end animation if time exceeded
                    if (_elapsedTime >= _animationDuration) {
                        _level += _levelUpCount; // Add leftover level ups if still existing
                        SetLevelText(_level);
                        _sessionProgress = _targetValue;
                        _isAnimated = false;
                    }
                }

                SetMaterialProperties();
            }
        }

        private void SetMaterialProperties() {
            if (_level == 100) {
                _expProgress = 1;
                _sessionProgress = 0f;
            }

            _materialInstance.SetFloat(GradientTPropertyId, _gradientT);
            _materialInstance.SetFloat(ExpProgressPropertyId, _expProgress);
            _materialInstance.SetFloat(SessionProgressPropertyId, _sessionProgress);
            _materialInstance.SetFloat(HighlightPropertyId, _highlight);
        }

        #endregion

        #region Initialize/Dispose

        protected override void OnInitialize() {
            _initialized = false;
            SetMaterial();
            GlobalSettingsView.ExperienceBarConfigEvent += OnExperienceBarConfigChanged;
            UserRequest.StateChangedEvent += OnProfileRequestStateChanged;
            if (ConfigFileData.Instance.ExperienceBarEnabled) {
                UploadReplayRequest.StateChangedEvent += OnUploadStateChanged;
                PrestigeRequest.StateChangedEvent += OnPrestigeRequestStateChanged;
            } else {
                LevelText = "";
                NextLevelText = "";
                _experienceBar.gameObject.SetActive(false);
            }
        }

        protected override void OnDispose() {
            GlobalSettingsView.ExperienceBarConfigEvent -= OnExperienceBarConfigChanged;
            UserRequest.StateChangedEvent -= OnProfileRequestStateChanged;
            UploadReplayRequest.StateChangedEvent -= OnUploadStateChanged;
            PrestigeRequest.StateChangedEvent -= OnPrestigeRequestStateChanged;
        }

        #endregion

        #region Events

        [UIAction("on-click"), UsedImplicitly]
        private void OnClick() {
            LeaderboardEvents.NotifyPrestigeWasPressed();
        }

        private void OnExperienceBarConfigChanged(bool enabled) {
            _experienceBar.gameObject.SetActive(enabled);
            ResetExperienceBarData();
            if (enabled && !_initialized) {
                UploadReplayRequest.StateChangedEvent += OnUploadStateChanged;
                PrestigeRequest.StateChangedEvent += OnPrestigeRequestStateChanged;
                SetLevelText(_level);
            } else if (!enabled && _initialized) {
                UploadReplayRequest.StateChangedEvent -= OnUploadStateChanged;
                PrestigeRequest.StateChangedEvent -= OnPrestigeRequestStateChanged;
                LevelText = "";
                NextLevelText = "";
            }
            _initialized = enabled;
        }

        private void OnProfileRequestStateChanged(IWebRequest<Player> instance, RequestState state, string? failReason) {
            if (!_initialized && state is RequestState.Finished) {
                Player player = instance.Result;
                _level = player.level;
                _requiredExp = CalculateRequiredExperience(player.level, player.prestige);
                _expProgress = player.experience / _requiredExp;
                ResetExperienceBarData();
                if (ConfigFileData.Instance.ExperienceBarEnabled) {
                    SetLevelText(_level);
                    _experienceBar.gameObject.SetActive(ConfigFileData.Instance.ExperienceBarEnabled);
                    _initialized = true;
                }
            }
        }

        private int CalculateRequiredExperience(int level, int prestige) {
            int requiredExp = 500 + (50 * level);
            if (prestige != 0) {
                requiredExp = (int)Mathf.Round(requiredExp * Mathf.Pow(1.33f, prestige));
            }
            return requiredExp;
        }

        private void SetLevelText(int level) {
            LevelText = level.ToString();
            if (level != 100) {
                NextLevelText = (level + 1).ToString();
            } else {
                NextLevelText = "Prestige";
            }
        }

        private void ResetExperienceBarData(bool refreshVisual = true) {
            _levelUpCount = 0;
            _levelUpValue = 0;
            _targetValue = 0f;
            _sessionProgress = 0f;
            _gradientT = 0f;
            _highlight = 0f;
            _elapsedTime = 0f;
            _elapsedTime2 = 0f;
            _isAnimated = false;
            _isIdle = false;
            if (refreshVisual) {
                SetMaterialProperties();
            }
        }

        private void OnUploadStateChanged(IWebRequest<ScoreUploadResponse> instance, RequestState state, string? failReason) {
            if (_level == 100) return;

            if (state is RequestState.Started) {
                if (_levelUpValue == 0) {
                    _expProgress += _targetValue;
                } else {
                    _expProgress = _targetValue;
                }
                ResetExperienceBarData();
                _isIdle = true;
            }

            if (state is RequestState.Finished) {
                ResetExperienceBarData();

                if (instance.Result.Status != ScoreUploadStatus.Error) {
                    Player player = instance.Result.Score.Player;
                    if (player.level == _level) {
                        _targetValue = player.experience / _requiredExp - _expProgress;
                    } else {
                        _levelUpCount = player.level - _level;
                        _levelUpValue = _levelUpCount;
                        _requiredExp = CalculateRequiredExperience(player.level, player.prestige);
                        _targetValue = player.experience / _requiredExp;
                    }

                    _isAnimated = true;
                }
            }
        }

        private void OnPrestigeRequestStateChanged(IWebRequest<Player> instance, RequestState state, string? failReason) {
            if (state is RequestState.Finished) {
                _level = 0;
                SetLevelText(_level);
                _expProgress = 0f;
                ResetExperienceBarData();
            }
        }

        #endregion

        #region Image & Material

        [UIComponent("experience-bar"), UsedImplicitly]
        private Image _experienceBar;

        private Material _materialInstance;

        private void SetMaterial() {
            _materialInstance = Object.Instantiate(BundleLoader.ExperienceBarMaterial);
            _experienceBar.material = _materialInstance;
        }

        #endregion

        #region LevelText

        private string _levelText = "";

        [UIValue("level-text"), UsedImplicitly]
        public string LevelText {
            get => _levelText;
            set {
                if (_levelText.Equals(value)) return;
                _levelText = value;
                NotifyPropertyChanged();
            }
        }

        private string _nextLevelText = "";

        [UIValue("next-level-text"), UsedImplicitly]
        public string NextLevelText {
            get => _nextLevelText;
            set {
                if (_nextLevelText.Equals(value)) return;
                _nextLevelText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}