using BeatLeader.Replayer;
using HMUI;
using UnityEngine;
using Zenject;

namespace BeatLeader.UI.Replayer.Desktop {
    internal class ReplayerDesktopUIBinder : MonoBehaviour {
        [Inject] private readonly ReplayerCameraController _cameraController = null!;
        [Inject] private readonly ReplayerDesktopScreenSystem _screenSystem = null!;
        [Inject] private readonly ReplayerDesktopViewController _viewController = null!;

        private void Awake() {
            _screenSystem.Screen.SetRootViewController(_viewController, ViewController.AnimationType.None);
            _screenSystem.SetRenderCamera(_cameraController.ViewableCamera!.Camera!);
        }
    }
}