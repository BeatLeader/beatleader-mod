using BeatLeader.Replayer.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer
{
    public class HotkeyManager : ITickable
    {
        [InjectOptional] private readonly UI2DManager _2DManager;
        [Inject] private readonly PlaybackController _playbackController;
        [Inject] private readonly ReplayerCameraController _cameraController;

        public KeyCode HideUIHotkey = KeyCode.H;
        public KeyCode HideCursorHotkey = KeyCode.C;
        public KeyCode PauseHotkey = KeyCode.P;
        public KeyCode SwitchViewRightHotkey = KeyCode.RightArrow;
        public KeyCode SwitchViewLeftHotkey = KeyCode.LeftArrow;
        public KeyCode IncFOVHotkey = KeyCode.UpArrow;
        public KeyCode DecFOVHotkey = KeyCode.DownArrow;

        public int maxFov = 110;
        public int minFov = 70;

        public event Action<bool> OnCursorVisibilityChanged;

        public void Tick()
        {
            if (Input.GetKeyDown(HideUIHotkey) && _2DManager != null)
            {
                _2DManager.showUI = !_2DManager.showUI;
            }
            if (Input.GetKeyDown(HideCursorHotkey))
            {
                InputManager.SwitchCursor();
                OnCursorVisibilityChanged?.Invoke(Cursor.visible);
            }
            if (Input.GetKeyDown(PauseHotkey))
            {
                _playbackController.Pause(!_playbackController.IsPaused);
            }
            if (Input.GetKeyDown(SwitchViewRightHotkey))
            {
                SwitchView(1);
            }
            if (Input.GetKeyDown(SwitchViewLeftHotkey))
            {
                SwitchView(-1);
            }
            if (Input.GetKeyDown(IncFOVHotkey))
            {
                if (_cameraController.FieldOfView < maxFov)
                    _cameraController.FieldOfView += 1;
            }
            if (Input.GetKeyDown(DecFOVHotkey))
            {
                if (_cameraController.FieldOfView > minFov)
                    _cameraController.FieldOfView -= 1;
            }
        }

        private void SwitchView(int offset)
        {
            var poses = _cameraController.PoseProviders;
            var idx = poses.Select(x => x.Name).ToList().IndexOf(_cameraController.CurrentPoseName);
            if (idx != -1 && idx + offset >= 0 && idx + offset <= poses.Count - 1)
            {
                _cameraController.SetCameraPose(poses[idx + offset]);
            }
        }
    }
}
