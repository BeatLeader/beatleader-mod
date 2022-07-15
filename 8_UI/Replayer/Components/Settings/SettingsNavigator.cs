using System;
using System.Linq;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using BeatLeader.Replays.Movement;
using BeatLeader.Replays;
using BeatLeader.Utils;
using UnityEngine.XR;
using UnityEngine;
using Zenject;
using HMUI;

namespace BeatLeader.Components
{
    internal class SettingsNavigator : ReeUIComponentV2WithContainer
    {
        #region Components

        [UIObject("container")] private GameObject _container;
        [UIObject("main-view")] private GameObject _mainView;
        [UIObject("body-settings-view")] private GameObject _bodySettingsView;
        [UIObject("playback-settings-view")] private GameObject _playbackSettingsView;
        [UIObject("camera-settings-view")] private GameObject _cameraSettingsView;
        [UIObject("navigation-panel")] private GameObject _navigationPanel;
        [UIValue("speed-controller")] private SpeedController _speedController;

        private ModalView _modal;

        #endregion

        #region Setup

        protected override void OnInstantiate()
        {
            _speedController = InstantiateInContainer<SpeedController>(Container, transform);
            _cameraViewValues = new List<object>(_cameraController.poseProviders.Select(x => x.name));

            _movementLerp = ReplayerConfig.instance.MovementLerp;
            _fieldOfView = ReplayerConfig.instance.CameraFOV;
            _cameraView = ReplayerConfig.instance.CameraView;
        }
        protected override void OnInitialize()
        {
            _bodySettingsView.SetActive(false);
            _playbackSettingsView.SetActive(false);
            _cameraSettingsView.SetActive(false);
            _navigationPanel.SetActive(false);
            _showHead = false;
            _modal = _container.GetComponentInParentHierarchy<ModalView>();
            _modal.blockerClickedEvent += DismissCurrentView; 
        }

        #endregion

        #region Logic

        [Inject] private readonly LayoutEditor _layoutEditor;

        [UIAction("editor-button-clicked")] private void EnterOrExitEditor()
        {
            _layoutEditor.SetEditModeEnabled(true);
            _modal.Hide(true);
        }

        #endregion

        #region Views

        private GameObject _currentView;
        private bool _chillingInSubMenu;

        private void PresentView(GameObject view)
        {
            if (_chillingInSubMenu) return;
            view.SetActive(true);
            _navigationPanel.SetActive(true);
            _mainView.SetActive(false);
            _currentView = view;
            _chillingInSubMenu = true;
        }
        private void DismissView(GameObject view)
        {
            if (!_chillingInSubMenu) return;
            view.SetActive(false);
            _navigationPanel.SetActive(false);
            _mainView.SetActive(true);
            _currentView = null;
            _chillingInSubMenu = false;
        }

        [UIAction("dismiss-current-view")] private void DismissCurrentView() => DismissView(_currentView);
        [UIAction("present-playback-settings-view")] private void PresentPlaybackSettingsView() => PresentView(_playbackSettingsView);
        [UIAction("present-body-settings-view")] private void PresentBodySettingsView() => PresentView(_bodySettingsView);
        [UIAction("present-camera-settings-view")] private void PresentCameraSettingsView() => PresentView(_cameraSettingsView);

        #endregion

        #region Settings

        [Inject] private readonly VRControllersMovementEmulator _movementEmulator;
        [Inject] private readonly VRControllersManager _controllersManager;
        [Inject] private readonly ReplayerCameraController _cameraController;

        [UIValue("camera-view-values")] private List<object> _cameraViewValues;
        [UIValue("camera-view")] private string _cameraView
        {
            get => _cameraController.currentPose;
            set
            {
                ReplayerConfig.instance.CameraView = value;
                _cameraController.SetCameraPose(value);
            }
        }
        [UIValue("camera-fov")] private int _fieldOfView
        {
            get => _cameraController.fieldOfView;
            set
            {
                _cameraController.fieldOfView = value;
                ReplayerConfig.instance.CameraFOV = value;
            }
        }
        [UIValue("movement-lerp")] private bool _movementLerp
        {
            get => _movementEmulator.lerpEnabled;
            set
            {
                _movementEmulator.lerpEnabled = value;
                ReplayerConfig.instance.MovementLerp = value;
                NotifyPropertyChanged(nameof(_movementLerp));
            }
        }
        [UIValue("show-head")] private bool _showHead
        {
            get => _controllersManager.head.gameObject.activeInHierarchy;
            set
            {
                _controllersManager.ShowNode(XRNode.Head, value);
                NotifyPropertyChanged(nameof(_showHead));
            }
        }
        [UIValue("show-left-saber")] private bool _showLeftSaber
        {
            get => _controllersManager.leftSaber.gameObject.activeInHierarchy;
            set
            {
                _controllersManager.ShowNode(XRNode.LeftHand, value);
                NotifyPropertyChanged(nameof(_showLeftSaber));
            }
        }
        [UIValue("show-right-saber")] private bool _showRightSaber
        {
            get => _controllersManager.rightSaber.gameObject.activeInHierarchy;
            set
            {
                _controllersManager.ShowNode(XRNode.RightHand, value);
                NotifyPropertyChanged(nameof(_showRightSaber));
            }
        }

        #endregion
    }
}
