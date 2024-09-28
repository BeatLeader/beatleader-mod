using BeatSaber.BeatAvatarAdapter.AvatarEditor;
using BeatSaber.BeatAvatarSDK;
using Reactive;
using Reactive.BeatSaber.Components;
using UnityEngine;

namespace BeatLeader.Replayer.Emulation {
    public class MenuBeatAvatarController : BeatAvatarController {
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

        private AvatarTweenController _tweenController = null!;
        private Spinner _spinner = null!;
        private Animator _animator = null!;

        protected override void Awake() {
            _tweenController = GetComponent<AvatarTweenController>();
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
            base.Awake();
        }

        #endregion

        #region Visuals

        public override void SetVisuals(AvatarData? data, bool animated = false) {
            if (isActiveAndEnabled && animated) {
                _tweenController.PopAll();
            }
            base.SetVisuals(data, animated);
        }

        public void SetLoading(bool loading) {
            _spinner.Enabled = loading;
        }

        #endregion

        #region Present & Hide 
        
        public void Present(bool animated = true) {
            gameObject.SetActive(true);
            if (animated) _tweenController.PresentAvatar();
        }

        public void Hide() {
            _tweenController.HideAvatar();
        }
        
        #endregion
    }
}