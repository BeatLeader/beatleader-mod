using System.Collections.Generic;
using System.Linq;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.BSML_Addons;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.UI.Replayer {
    [BSMLComponent(Namespace = "Replayer")]
    internal class SettingsCameraView : ReeUIComponentV3<SettingsCameraView>, ReplayerSettingsPanel.ISegmentedControlView {
        #region SegmentedControlView

        public ReplayerSettingsPanel.SettingsView Key => ReplayerSettingsPanel.SettingsView.CameraView;
        public (string, Sprite) Value { get; } = ("Camera", BundleLoader.CameraIcon);

        void ISegmentedControlView.SetActive(bool active) {
            Content.SetActive(active);
        }

        void ISegmentedControlView.Setup(Transform? trans) {
            ContentTransform.SetParent(trans, false);
        }

        #endregion

        #region UI Components

        [UIComponent("fov-slider"), UsedImplicitly]
        private NormalizedSlider _fovSlider = null!;

        [UIComponent("camera-view-setting"), UsedImplicitly]
        private ListSetting _cameraViewSetting = null!;

        [UIComponent("camera-view-params-container"), UsedImplicitly]
        private Transform _cameraViewParamsContainer = null!;

        [UIObject("camera-view-params-empty-text"), UsedImplicitly]
        private GameObject _cameraViewParamsEmptyTextObject = null!;

        #endregion

        #region UI Values

        [UIValue("camera-view-choices"), UsedImplicitly]
        private List<object> _cameraViewSettingsOptions = new();

        #endregion

        #region CameraParams

        public interface ICameraViewParams {
            bool SupportsCameraView(ICameraView cameraView);
            void Setup(Transform? transform, ICameraView? cameraView);
        }

        public readonly List<ICameraViewParams> cameraViewParams = new();
        private readonly Dictionary<ICameraView, ICameraViewParams> _cachedCameraParams = new();

        private ICameraViewParams? _selectedCameraParams;

        public void ReloadCameraViewParams() {
            ShowCameraParamsOrText(null, null);
            _cachedCameraParams.Clear();
            foreach (var viewParams in cameraViewParams) {
                viewParams.Setup(null, null);
                var view = CameraViews.FirstOrDefault(x => viewParams.SupportsCameraView(x));
                if (view is not null) _cachedCameraParams[view] = viewParams;
            }
            RefreshCameraParams();
        }

        private void RefreshCameraParams() {
            if (_selectedCameraView is null) {
                ShowCameraParamsOrText(null, null);
                return;
            }
            var view = _selectedCameraView;
            var viewParams = AcquireCameraParams(view);
            ShowCameraParamsOrText(viewParams, view);
        }

        private ICameraViewParams? AcquireCameraParams(ICameraView view) {
            _cachedCameraParams.TryGetValue(view, out var viewParams);
            return viewParams;
        }

        private void ShowCameraParamsOrText(ICameraViewParams? viewParams, ICameraView? cameraView) {
            _cameraViewParamsEmptyTextObject.SetActive(viewParams is null);
            _selectedCameraParams?.Setup(null, null);
            viewParams?.Setup(_cameraViewParamsContainer, cameraView);
            _selectedCameraParams = viewParams;
        }

        #endregion

        #region CameraViews

        private IReadOnlyList<ICameraView> CameraViews => _cameraController!.Views;

        private ICameraView? SelectedCameraView {
            set {
                _selectedCameraView = value;
                if (value is null) return;
                _cameraController!.SetView(value);
            }
        }

        private ICameraView? _selectedCameraView;

        private void RefreshCameraViews() {
            ValidateAndThrow();
            _cameraViewSettingsOptions.AddRange(CameraViews);
            _selectedCameraView = CameraViews.FirstOrDefault();
            _cameraViewSetting.Value = _selectedCameraView;
            RefreshCameraParams();
        }

        [UIAction("camera-view-format"), UsedImplicitly]
        private string FormatCameraView(ICameraView? view) {
            return view?.Name ?? "None";
        }

        #endregion

        #region Setup

        private ICameraController? _cameraController;

        public void Setup(
            ICameraController cameraController,
            ReplayerCameraSettings cameraSettings
        ) {
            _cameraController = cameraController;
            _fovSlider.ValueRange = new(cameraSettings.MinCameraFOV, cameraSettings.MaxCameraFOV);
            _fovSlider.Value = cameraController.Camera.fieldOfView;
            RefreshCameraViews();
        }

        protected override void OnInitialize() {
            _cameraViewSetting.text.fontStyle = FontStyles.Normal;
            var dummyNavigation = new Navigation { mode = Navigation.Mode.None };
            _cameraViewSetting.decButton.navigation = dummyNavigation;
            _cameraViewSetting.incButton.navigation = dummyNavigation;
        }

        protected override bool OnValidation() {
            return _cameraController is not null;
        }

        #endregion

        #region Camera Management

        private void SetFOV(float fov) {
            ValidateAndThrow();
            _cameraController!.Camera.fieldOfView = fov;
        }

        #endregion

        #region Callbacks

        [UIAction("fov-slider-value-change"), UsedImplicitly]
        private void HandleFOVSliderValueChanged(float value) {
            SetFOV(value);
        }

        [UIAction("camera-view-value-change"), UsedImplicitly]
        private void HandleCameraViewChanged(ICameraView view) {
            SelectedCameraView = view;
            RefreshCameraParams();
        }

        #endregion
    }
}