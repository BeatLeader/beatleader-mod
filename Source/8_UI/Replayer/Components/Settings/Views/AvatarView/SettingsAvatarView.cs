using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.Replayer.Emulation;
using BeatLeader.UI.BSML_Addons;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Scrollbar = BeatLeader.Components.Scrollbar;

namespace BeatLeader.UI.Replayer {
    [BSMLComponent(Namespace = "Replayer")]
    internal class SettingsAvatarView : ReeUIComponentV3<SettingsAvatarView>, ReplayerSettingsPanel.ISegmentedControlView {
        #region SettingsView

        public ReplayerSettingsPanel.SettingsView Key => ReplayerSettingsPanel.SettingsView.AvatarView;
        public (string, Sprite) Value { get; } = ("Avatar", BundleLoader.AvatarIcon);

        public void SetActive(bool active) {
            Content.SetActive(active);
        }

        public void Setup(Transform? trans) {
            ContentTransform.SetParent(trans, false);
        }

        #endregion

        #region UI Components

        [UIComponent("body-parts-container"), UsedImplicitly]
        private Transform _bodyPartsContainer = null!;

        [UIComponent("models-dropdown"), UsedImplicitly]
        private DropDownListSetting _modelsDropdown = null!;

        [UIComponent("scrollbar"), UsedImplicitly]
        private Scrollbar _scrollbar = null!;

        [UIComponent("scrollable"), UsedImplicitly]
        private ScrollableContainer _scrollable = null!;

        #endregion

        #region UI Values

        [UIValue("body-model"), UsedImplicitly]
        private IVirtualPlayerBodyModel BodyModel {
            get => _bodyModel ?? dummyModel;
            set {
                _bodyModel = value;
                if (_bodyModel == dummyModel) return;
                ReloadBindings();
                ReloadUIControls();
            }
        }

        [UIValue("body-models"), UsedImplicitly]
        private List<object> _bodyModels = new() { dummyModel };

        private static readonly VirtualPlayerBodyModel dummyModel = new("", Array.Empty<VirtualPlayerBodyPartModel>());

        private IVirtualPlayerBodyModel? _bodyModel;
        private VirtualPlayerBodyConfig? _bodyConfig;

        private void ReloadBodyModels() {
            _bodyModels.Clear();
            _bodyModels.AddRange(_bodySpawner!.BodyModels);
            _modelsDropdown.UpdateChoices();
            BodyModel = _bodySpawner!.BodyModels.First();
        }

        [UIAction("format-body-model"), UsedImplicitly]
        private string FormatBodyModel(IVirtualPlayerBodyModel model) {
            return model.Name;
        }

        #endregion

        #region Setup

        private IVirtualPlayerBodySpawner? _bodySpawner;
        private BodySettings _bodySettings = new();

        public void Setup(
            IVirtualPlayerBodySpawner bodySpawner,
            BodySettings? bodySettings = null
        ) {
            _bodySpawner = bodySpawner;
            _bodySettings = bodySettings ?? _bodySettings;
            ReloadBodyModels();
        }

        protected override void OnInitialize() {
            var dropdown = _modelsDropdown.dropdown.transform;
            var dropdownParent = dropdown.parent.gameObject;
            
            var dropdownFitter = dropdownParent.AddComponent<ContentSizeFitter>();
            dropdownFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            dropdownFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var text = dropdownParent.GetComponentInChildren<TMP_Text>();
            text.fontStyle = FontStyles.Normal;
            
            _scrollable.Scrollbar = _scrollbar;
        }

        protected override bool OnValidation() {
            return _bodyConfig is not null && _bodySpawner is not null;
        }

        #endregion

        #region Config

        private record ConfigWithModel(
            IVirtualPlayerBodyPartModel Model,
            VirtualPlayerBodyPartConfig Config
        );

        private record ConfigsGroup(
            string? GroupName,
            IEnumerable<ConfigWithModel> Configs
        );
        
        private readonly Dictionary<IVirtualPlayerBodyModel, IEnumerable<ConfigsGroup>> _configBindings = new();
        private IEnumerable<ConfigsGroup>? _groupedConfigs;

        private void ReloadBindings() {
            ReloadConfig();
            if (!_configBindings.TryGetValue(_bodyModel!, out var groupedConfigs)) {
                Debug.Log("Creating new configs");
                var modelsDict = _bodyModel!.Parts.ToDictionary(static x => x.Id);
                groupedConfigs = _bodyConfig!.BodyParts
                    .Select(x => new ConfigWithModel(modelsDict[x.Id], x))
                    .GroupBy(x => x.Model.Category ?? "Other")
                    .Select(x => new ConfigsGroup(x.Key, x))
                    .ToArray();
                _configBindings[_bodyModel] = groupedConfigs;
            }
            _groupedConfigs = groupedConfigs;
        }

        private void ReloadConfig() {
            var config = _bodySettings.GetConfigByNameOrNull(_bodyModel!.Name);
            config ??= new VirtualPlayerBodyConfig(_bodyModel);
            _bodySettings.AddOrUpdateConfig(config);
            
            if (_bodyConfig is not null) {
                _bodyConfig.ConfigUpdatedEvent -= HandleConfigUpdated;
            }
            _bodyConfig = config;
            _bodyConfig.ConfigUpdatedEvent += HandleConfigUpdated;
        }

        #endregion

        #region BodyPartsCategory

        [BSMLComponent(Suppress = true)]
        private class BodyPartsCategory : ReeUIComponentV3<BodyPartsCategory> {
            #region UI Components

            [UIComponent("container"), UsedImplicitly]
            private Transform _container = null!;

            [UIComponent("text"), UsedImplicitly]
            private TMP_Text _text = null!;

            #endregion

            #region Setup

            private SettingsAvatarView _settingsAvatarView = null!;
            private ConfigsGroup _configsGroup = null!;

            public void Setup(
                SettingsAvatarView avatarView,
                ConfigsGroup configsGroup
            ) {
                _text.text = configsGroup.GroupName;
                _settingsAvatarView = avatarView;
                _configsGroup = configsGroup;
                Content.SetActive(true);
                LoadParts();
            }

            public void Release() {
                Content.SetActive(false);
                ClearParts();
            }

            #endregion

            #region Parts

            private readonly List<BodyPartControl> _parts = new();

            private void LoadParts() {
                foreach (var (model, partConfig) in _configsGroup.Configs) {
                    var part = _settingsAvatarView.GetBodyPart(model, partConfig);
                    part.ContentTransform.SetParent(_container, false);
                    _parts.Add(part);
                }
            }

            private void ClearParts() {
                foreach (var part in _parts) {
                    _settingsAvatarView.ReleaseBodyPart(part);
                }
                _parts.Clear();
            }

            #endregion
        }

        private readonly List<BodyPartsCategory> _categories = new();
        private readonly Stack<BodyPartsCategory> _reusableCategories = new();

        private void ReloadUIControls() {
            ValidateAndThrow();
            ClearCategories();
            foreach (var grouped in _groupedConfigs!) {
                AddCategory(grouped);
            }
        }

        private void AddCategory(ConfigsGroup configsGroup) {
            if (!_reusableCategories.TryPop(out var category)) {
                category = BodyPartsCategory.Instantiate(_bodyPartsContainer);
            }
            category!.Setup(this, configsGroup);
            _categories.Add(category);
        }

        private void ClearCategories() {
            foreach (var category in _categories) {
                category.Release();
                _reusableCategories.Push(category);
            }
            _categories.Clear();
        }

        #endregion

        #region BodyPartPool

        private readonly Stack<BodyPartControl> _reusableParts = new();

        private BodyPartControl GetBodyPart(
            IVirtualPlayerBodyPartModel model,
            VirtualPlayerBodyPartConfig partConfig
        ) {
            if (!_reusableParts.TryPop(out var partControl)) {
                partControl = BodyPartControl.Instantiate(ContentTransform);
            }
            partControl!.Setup(model, partConfig);
            return partControl;
        }

        private void ReleaseBodyPart(BodyPartControl control) {
            _reusableParts.Push(control);
        }

        #endregion

        #region BodyPartControl

        [BSMLComponent(Suppress = true)]
        private class BodyPartControl : ReeUIComponentV3<BodyPartControl> {
            #region UI Components

            [UIComponent("toggle"), UsedImplicitly]
            private ToggleSetting _toggleSetting = null!;

            [UIComponent("alpha-selector"), UsedImplicitly]
            private AlphaSelector _alphaSelector = null!;

            #endregion

            #region UI Values

            [UIValue("toggle-value"), UsedImplicitly]
            private bool Enabled {
                get => _enabled;
                set {
                    _enabled = value;
                    _partConfig!.Active = value;
                }
            }

            private bool _enabled;

            #endregion

            #region Setup

            private VirtualPlayerBodyPartConfig? _partConfig;

            public void Setup(
                IVirtualPlayerBodyPartModel model,
                VirtualPlayerBodyPartConfig partConfig
            ) {
                _partConfig = partConfig;
                _toggleSetting.Value = partConfig.Active;
                _toggleSetting.Text = model.Name;
                _alphaSelector.Interactable = model.HasAlphaSupport;
            }

            #endregion

            #region Callbacks

            [UIAction("alpha-change"), UsedImplicitly]
            private void HandleAlphaChanged(float alpha) {
                _partConfig!.Alpha = alpha;
            }

            #endregion
        }

        #endregion

        #region Callbacks

        private void HandleConfigUpdated() {
            _bodySpawner!.ApplyModelConfig(_bodyModel!, _bodyConfig!);
        }

        #endregion
    }
}