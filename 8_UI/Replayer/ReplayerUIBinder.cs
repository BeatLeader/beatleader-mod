using BeatLeader.ViewControllers;
using UnityEngine;
using Zenject;
using HMUI;
using BeatLeader.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatLeader.Replayer.Emulation;
using BeatLeader.Utils;
using System;

namespace BeatLeader.UI {
    internal class ReplayerUIBinder : MonoBehaviour {
        #region Injection

        [Inject] private readonly VRControllersAccessor _controllerProvider;
        [Inject] private readonly ScreenSpaceScreen _screenSpaceScreen;
        [Inject] private readonly Replayer2DViewController _screenViewController;
        [Inject] private readonly ReplayerVRViewController _vrViewController;
        [Inject] private readonly Models.ReplayLaunchData _launchData;

        #endregion

        #region UI Visibility

        public bool AlwaysShowUI {
            get => Screen.gameObject.activeSelf;
            set {
                Screen?.gameObject.SetActive(value);
                UIVisibilityChangedEvent?.Invoke(value);
            }
        }

        #endregion

        #region Events

        public event Action<bool> UIVisibilityChangedEvent;

        #endregion

        #region Setup

        public HMUI.Screen Screen { get; private set; }

        private ViewController _viewController;

        private void Start() {
            bool isInFPFC = InputUtils.IsInFPFC;

            Screen = isInFPFC ? _screenSpaceScreen : SetupFloatingScreen();
            _viewController = isInFPFC ? _screenViewController : _vrViewController;

            Screen.SetRootViewController(_viewController, ViewController.AnimationType.None);
            AlwaysShowUI = _launchData.ActualSettings.ShowUI;
        }

        private FloatingScreen SetupFloatingScreen() {
            var container = new GameObject("Container");
            var viewContainer = new GameObject("ViewContainer");
            viewContainer.transform.SetParent(container.transform, false);

            var floating = FloatingScreen.CreateFloatingScreen(
                new(100, 55), true, Vector3.zero, Quaternion.identity);

            floating.transform.SetParent(viewContainer.transform, false);
            floating.HandleSide = FloatingScreen.Side.Bottom;
            floating.HighlightHandle = true;

            floating.handle.transform.localPosition = new(11, -23.5f, 0);
            floating.handle.transform.localScale = new(20, 3.67f, 3.67f);

            container.transform.SetParent(_controllerProvider.HandsContainer, false);
            _controllerProvider.ShowHands();

            return floating;
        }

        #endregion
    }
}
