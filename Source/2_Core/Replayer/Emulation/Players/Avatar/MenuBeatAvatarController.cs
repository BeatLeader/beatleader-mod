using System.Collections;
using BeatSaber.BeatAvatarAdapter.AvatarEditor;
using BeatSaber.BeatAvatarSDK;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using UnityEngine;

namespace BeatLeader.Replayer.Emulation {
    public class MenuBeatAvatarController : BeatAvatarController {
        #region Setup

        private AvatarTweenController _tweenController = null!;
        private Spinner _spinner = null!;

        protected override void Awake() {
            _tweenController = GetComponent<AvatarTweenController>();
            //spinner
            _spinner = new Spinner {
                ContentTransform = {
                    localScale = Vector3.one * 0.02f,
                    localPosition = new(0f, 1.3f, 0.1f)
                },
                Enabled = false
            }.WithSizeDelta(13f, 13f);
            
            BeatSaberUtils.AddCanvas(_spinner);
            
            _spinner.Use(transform);
            gameObject.SetActive(false);
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

        private Coroutine? _hideCoroutine;

        public void Present(bool animated = true) {
            gameObject.SetActive(true);
            
            if (animated) {
                if (_hideCoroutine != null) {
                    _tweenController._sharedCoroutineStarter.StopCoroutine(_hideCoroutine);
                }
                
                _tweenController.PresentAvatar();
            }
        }

        public void Hide() {
            _hideCoroutine = _tweenController._sharedCoroutineStarter.StartCoroutine(DisappearAnimationWrapper());
        }

        private IEnumerator DisappearAnimationWrapper() {
            yield return _tweenController.DisappearAnimation();
            _hideCoroutine = null;
        }

        #endregion
    }
}