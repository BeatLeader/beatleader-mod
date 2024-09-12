using BeatLeader.Utils;
using IPA.Utilities;
using Reactive;
using Reactive.BeatSaber.Components;
using UnityEngine;

namespace BeatLeader.Replayer.Emulation {
    public class AvatarController : MonoBehaviour {
        #region Setup

        public bool PlayAnimation {
            get => _animator.enabled;
            set {
                _animator.enabled = value;
                if (value) return;
                var trans = transform;
                trans.localPosition = Vector3.zero;
                trans.localEulerAngles = Vector3.zero;
            }
        }

        public AvatarPoseController PoseController { get; private set; } = null!;
        public Transform HeadTransform { get; private set; } = null!;

        private AvatarTweenController _tweenController = null!;
        private AvatarVisualController _visualController = null!;
        private Animator _animator = null!;
        private Spinner _spinner = null!;

        private void Awake() {
            _tweenController = GetComponent<AvatarTweenController>();
            _visualController = GetComponentInChildren<AvatarVisualController>();
            PoseController = GetComponentInChildren<AvatarPoseController>();
            //TODO: asm pub
            HeadTransform = PoseController.GetField<Transform, AvatarPoseController>("_headTransform");
            _animator = GetComponent<Animator>();
            //spinner
            _spinner = new Spinner {
                ContentTransform = {
                    localScale = Vector3.one * 0.02f,
                    localPosition = new(0f, 1.3f, 0.1f)
                },
                Enabled = false
            }.WithSizeDelta(13f, 13f);
            ReactiveUtils.AddCanvas(_spinner);
            _spinner.Use(transform);
            //avatar layer
            SetVisuals(null);
            gameObject.SetActive(false);
        }

        public void MakeMasked() {
            foreach (var item in transform.GetChildren(false)) {
                item.gameObject.layer = 10;
            }
        }

        #endregion

        #region Present & Dismiss

        public void Present(bool animated = true) {
            gameObject.SetActive(true);
            if (animated) _tweenController.PresentAvatar();
        }

        public void Hide() {
            _tweenController.HideAvatar();
        }

        #endregion

        #region Visuals

        public void SetVisuals(AvatarData? data, bool animated = false) {
            if (isActiveAndEnabled && animated) {
                _tweenController.PopAll();
            }
            _visualController.UpdateAvatarVisual(data ?? AvatarUtils.DefaultAvatarData);
        }

        public void SetLoading(bool loading) {
            _spinner.Enabled = loading;
        }

        #endregion
    }
}