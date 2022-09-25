using BeatLeader.Components;
using BeatLeader.Replayer.Camera;
using BeatLeader.Utils;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer
{
    public class HotkeysManager : ITickable
    {
        [InjectOptional] private readonly UI2DManager _2DManager;
        [InjectOptional] private readonly ReplayerCameraController _cameraController;
        [Inject] private readonly Models.ReplayLaunchData _replayData;
        [Inject] private readonly PlaybackController _playbackController;

        public KeyCode HideUIHotkey = KeyCode.H;
        public KeyCode HideCursorHotkey = KeyCode.C;
        public KeyCode PauseHotkey = KeyCode.P;
        public KeyCode SwitchViewRightHotkey = KeyCode.RightArrow;
        public KeyCode SwitchViewLeftHotkey = KeyCode.LeftArrow;
        public KeyCode IncFOVHotkey = KeyCode.UpArrow;
        public KeyCode DecFOVHotkey = KeyCode.DownArrow;

        public event Action<bool> CursorVisibilityChangedEvent;

        public void Tick()
        {
            if (Input.GetKeyDown(HideUIHotkey))
            {
                if (_2DManager != null) _2DManager.ShowUI = !_2DManager.ShowUI;
            }
            if (Input.GetKeyDown(HideCursorHotkey))
            {
                InputManager.SwitchCursor();
                CursorVisibilityChangedEvent?.Invoke(Cursor.visible);
            }
            if (Input.GetKeyDown(PauseHotkey))
            {
                _playbackController.Pause(!_playbackController.IsPaused);
            }
            if (Input.GetKeyDown(SwitchViewRightHotkey))
            {
                SwitchCameraView(1);
            }
            if (Input.GetKeyDown(SwitchViewLeftHotkey))
            {
                SwitchCameraView(-1);
            }
            if (_cameraController != null)
            {
                if (Input.GetKeyDown(IncFOVHotkey) && _cameraController.FieldOfView < _replayData.actualSettings.MaxFOV)
                {
                    _cameraController.FieldOfView += 1;
                }
                if (Input.GetKeyDown(DecFOVHotkey) && _cameraController.FieldOfView > _replayData.actualSettings.MinFOV)
                {
                    _cameraController.FieldOfView -= 1;
                }
            }
        }
        private void SwitchCameraView(int offset)
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
