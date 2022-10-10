using BeatLeader.ViewControllers;
using UnityEngine;
using Zenject;
using HMUI;
using BeatLeader.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatLeader.Replayer.Emulation;
using BeatLeader.Utils;
using System;

namespace BeatLeader.UI
{
    internal class ReplayerUIBinder : MonoBehaviour
    {
        #region Injection

        [Inject] private readonly VRControllersProvider _controllerProvider;
        [Inject] private readonly ScreenSpaceScreen _screenSpaceScreen;
        [Inject] private readonly Replayer2DViewController _screenViewController;
        [Inject] private readonly ReplayerVRViewController _vrViewController;
        [Inject] private readonly Models.ReplayLaunchData _launchData;

        #endregion

        #region ShowUI & UI Type & Screen 

        public bool ShowUI
        {
            get => Screen.gameObject.activeSelf;
            set
            {
                Screen?.gameObject.SetActive(value);
                UIVisibilityChangedEvent?.Invoke(value);
            }
        }
        public InputUtils.InputType InstalledUIType { get; private set; }
        public HMUI.Screen Screen { get; private set; }

        #endregion

        #region Events

        public event Action<bool> UIVisibilityChangedEvent;

        #endregion

        #region Setup

        private ViewController _viewController;

        private void Start()
        {
            bool isInFPFC = InputUtils.IsInFPFC;

            InstalledUIType =isInFPFC ? InputUtils.InputType.FPFC : InputUtils.InputType.VR;
            Screen = isInFPFC ? _screenSpaceScreen : SetupFloatingScreen();
            _viewController = isInFPFC ? _screenViewController : _vrViewController;

            Screen.SetRootViewController(_viewController, ViewController.AnimationType.None);
            ShowUI = _launchData.ActualSettings.ShowUI;
        }

        private FloatingScreen SetupFloatingScreen()
        {
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

            container.transform.SetParent(_controllerProvider.MenuHandsContainer, false);
            _controllerProvider.ShowMenuControllers();

            return floating;
        }

        #endregion
    }
}
