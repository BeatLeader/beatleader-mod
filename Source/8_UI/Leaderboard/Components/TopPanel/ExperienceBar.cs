using System.Net;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.UI.Hub;
using BeatLeader.WebRequests;
using BeatSaberMarkupLanguage.Attributes;
using BS_Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        private string _userID;
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
            if (_initialized && _level != 100) {
                if (_isIdle) {
                    _elapsedTime += Time.deltaTime;
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
                }
                else if (_isAnimated) {
                    _elapsedTime2 += Time.deltaTime;
                    _gradientT = Mathf.Clamp01(_elapsedTime2 / _animationDuration);
                    if (_levelUpValue > 0) {
                        _elapsedTime += Time.deltaTime;
                        float t = Mathf.Clamp01(_elapsedTime / _animationDuration * (_levelUpValue + 1));
                        float targetValue = 1 - _expProgress;
                        if (_levelUpCount != 0) {
                            _sessionProgress = Mathf.Lerp(0f, targetValue, t);
                        } else {
                            _sessionProgress = Mathf.Lerp(0f, _targetValue, t);
                        }

                        if (_elapsedTime >= _animationDuration / (_levelUpValue + 1)) {
                            if (_levelUpCount == 0) {
                                _expProgress = 0f;
                                _sessionProgress = _targetValue;
                                _elapsedTime = 0f;
                            } else {
                                _levelUpCount--;
                                SetLevelText(_level++);
                                _expProgress = 0f;
                                _sessionProgress = 0f;
                                _elapsedTime = 0f;
                            }
                        }
                    } else {
                        _elapsedTime += Time.deltaTime;
                        float t = Mathf.Clamp01(_elapsedTime / _animationDuration);
                        _sessionProgress = Mathf.Lerp(0, _targetValue, t);
                    
                        if (_elapsedTime >= _animationDuration)
                        {
                            _sessionProgress = _targetValue;
                        }
                    }
                }

                SetMaterialProperties();
                if (_elapsedTime2 >= _animationDuration) {
                    if (_levelUpValue == 0) {
                        _expProgress += _targetValue;
                    } else {
                        _expProgress = _targetValue;
                    }
                    _targetValue = 0f;
                    _sessionProgress = 0f;
                    _gradientT = 0f;
                    _isAnimated = false;
                }
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
                SceneManager.activeSceneChanged += OnActiveSceneChanged;
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
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
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
            if (enabled && !_initialized) {
                SceneManager.activeSceneChanged += OnActiveSceneChanged;
                PrestigeRequest.StateChangedEvent += OnPrestigeRequestStateChanged;
                SetLevelText(_level);
                SetMaterialProperties();
                _initialized = enabled;
            } else if (_initialized) {
                SceneManager.activeSceneChanged -= OnActiveSceneChanged;
                PrestigeRequest.StateChangedEvent -= OnPrestigeRequestStateChanged;
                LevelText = "";
                NextLevelText = "";
                _initialized = enabled;
            }
        }
        
        private void OnProfileRequestStateChanged(IWebRequest<User> instance, WebRequests.RequestState state, string? failReason) {
            if (!_initialized && state is WebRequests.RequestState.Finished) {
                Player player = instance.Result.player;
                _userID = player.id;
                _level = player.level;
                SetLevelText(_level);
                _requiredExp = CalculateRequiredExperience(player.level, player.prestige);
                _gradientT = 0f;
                _expProgress = player.experience / _requiredExp;
                _isAnimated = false;
                _levelUpCount = 0;
                SetMaterialProperties();
                if (ConfigFileData.Instance.ExperienceBarEnabled) {
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
            LevelText = _level.ToString();
            if (level != 100) {
                NextLevelText = (_level + 1).ToString();
            } else {
                NextLevelText = "Prestige";
            }
        }
        
        private async void OnActiveSceneChanged(Scene previousScene, Scene nextScene) {
            if (_level == 100) return;

            if (previousScene.name == SceneNames.Game && nextScene.name == SceneNames.Menu) {
                if (_isAnimated) {
                    if (_levelUpValue == 0) {
                        _expProgress += _targetValue;
                    } else {
                        _expProgress = _targetValue;
                    }
                    _targetValue = 0f;
                    _sessionProgress = 0f;
                    _gradientT = 0f;
                    _isAnimated = false;
                }

                _isIdle = true;
                _highlight = 0f;
                _elapsedTime = 0f;
                
                SetMaterialProperties();
                await Task.Delay(3000);
                
                var request = PlayerRequest.SendRequest(_userID);
                await request.Join();
                
                if (request.RequestStatusCode == HttpStatusCode.OK) {
                    Player player = request.Result;
                    if (player.level == _level) {
                        float target = player.experience / _requiredExp - _expProgress;
                        if (target > 0) {
                            _targetValue = player.experience / _requiredExp - _expProgress;
                            _levelUpValue = 0;
                        }
                    } else {
                        _levelUpCount = player.level - _level;
                        _levelUpValue = _levelUpCount;
                        _requiredExp = CalculateRequiredExperience(player.level, player.prestige);
                        _targetValue = player.experience / _requiredExp;
                    }
                    
                    _isIdle = false;
                    _highlight = 0f;
                    _elapsedTime = 0f;
                    _elapsedTime2 = 0f;
                    _isAnimated = true;
                    SetMaterialProperties();
                }
            }
        }

        private void OnPrestigeRequestStateChanged(IWebRequest<User> instance, WebRequests.RequestState state, string? failReason) {
            if (state is WebRequests.RequestState.Finished) {
                _level = 0;
                SetLevelText(_level);
                _expProgress = 0f;
                _sessionProgress = 0f;
                _isAnimated = false;
                _levelUpCount = 0;
                SetMaterialProperties();
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
            // _experienceBar.rectTransform.sizeDelta = new Vector2(40f, 3f);
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