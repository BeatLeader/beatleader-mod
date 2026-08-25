using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class FloatingScreenHandFollower : MonoBehaviour {
        private const float BaseScreenScale = 0.02f;
        private const float MinScale = 0.25f;
        private const float MaxScale = 1.5f;

        private ReplayerFloatingUISettings? _settings;
        private MenuControllersManager? _menuControllersManager;
        private Transform? _originalParent;
        private bool _attached;

        public void Setup(ReplayerFloatingUISettings settings, MenuControllersManager menuControllersManager) {
            _settings = settings;
            _menuControllersManager = menuControllersManager;
            _originalParent ??= transform.parent;
            if (!settings.AttachToHand) {
                RestoreFloatingPose(settings);
            }
        }

        private void LateUpdate() {
            var settings = _settings;
            if (settings == null) return;

            if (!settings.AttachToHand) {
                if (_attached) {
                    _attached = false;
                    RestoreFloatingPose(settings);
                }
                return;
            }

            var handTransform = GetHandTransform(settings);
            if (handTransform == null) return;

            if (!_attached || transform.parent != handTransform) {
                transform.SetParent(handTransform, false);
            }
            _attached = true;
            transform.SetLocalPose(settings.HandOffset);
            transform.localScale = Vector3.one * (BaseScreenScale * Mathf.Clamp(settings.HandScale, MinScale, MaxScale));
        }

        private Transform? GetHandTransform(ReplayerFloatingUISettings settings) {
            var controller = settings.AttachToRightHand ? _menuControllersManager?.RightHand : _menuControllersManager?.LeftHand;
            if (controller == null) return null;
            return controller._viewAnchorTransform != null ? controller._viewAnchorTransform : controller.transform;
        }

        private void RestoreFloatingPose(ReplayerFloatingUISettings settings) {
            if (transform.parent != _originalParent) {
                transform.SetParent(_originalParent, false);
            }
            transform.SetLocalPose(settings.Pose);
            transform.localScale = Vector3.one * BaseScreenScale;
        }
    }
}
