using System.Linq;
using HMUI;
using IPA.Utilities;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal abstract class AnimatedModalComponentBase : NewModalComponentBase {
        #region Panel Animation

        private static AnimationCurve? _presentAlphaAnimationCurve;
        private static AnimationCurve? _presentScaleXAnimationCurve;
        private static AnimationCurve? _presentScaleYAnimationCurve;

        private static AnimationCurve? _dismissAlphaAnimationCurve;
        private static AnimationCurve? _dismissScaleXAnimationCurve;
        private static AnimationCurve? _dismissScaleYAnimationCurve;

        private static PanelAnimationSO? _presentAnimation;
        private static PanelAnimationSO? _dismissAnimation;

        private static void InitAnimations() {
            if (_dismissAnimation == null) {
                _dismissAnimation = Resources
                    .FindObjectsOfTypeAll<PanelAnimationSO>()
                    .First(x => x.name == "HMUI.DefaultDismissPanelAnimationsSO");
                //TODO: asm pub
                _dismissAlphaAnimationCurve = _dismissAnimation.GetField<AnimationCurve, PanelAnimationSO>("_alphaAnimationCurve");
                _dismissScaleXAnimationCurve = _dismissAnimation.GetField<AnimationCurve, PanelAnimationSO>("_scaleXAnimationCurve");
                _dismissScaleYAnimationCurve = _dismissAnimation.GetField<AnimationCurve, PanelAnimationSO>("_scaleYAnimationCurve");
            }
            //
            if (_presentAnimation == null) {
                _presentAnimation ??= Resources
                    .FindObjectsOfTypeAll<PanelAnimationSO>()
                    .First(x => x.name == "HMUI.DefaultPresentPanelAnimationsSO");
                //TODO: asm pub
                _presentAlphaAnimationCurve = _presentAnimation.GetField<AnimationCurve, PanelAnimationSO>("_alphaAnimationCurve");
                _presentScaleXAnimationCurve = _presentAnimation.GetField<AnimationCurve, PanelAnimationSO>("_scaleXAnimationCurve");
                _presentScaleYAnimationCurve = _presentAnimation.GetField<AnimationCurve, PanelAnimationSO>("_scaleYAnimationCurve");
            }
        }

        protected override void OnInstantiate() {
            InitAnimations();
        }

        protected override void OnInitialize() {
            _canvasGroup = Content.AddComponent<CanvasGroup>();
        }

        #endregion

        #region Animation

        private CanvasGroup _canvasGroup = null!;
        
        protected override void OnOpenAnimationProgressChanged(float progress) {
            float x, y, alpha;
            if (IsOpened) {
                alpha = _presentAlphaAnimationCurve!.Evaluate(progress);
                x = _presentScaleXAnimationCurve!.Evaluate(progress);
                y = _presentScaleYAnimationCurve!.Evaluate(progress);
            } else {
                progress = 1f - progress;
                alpha = _dismissAlphaAnimationCurve!.Evaluate(progress);
                x = _dismissScaleXAnimationCurve!.Evaluate(progress);
                y = _dismissScaleYAnimationCurve!.Evaluate(progress);
            }
            ContentTransform.localScale = new(x, y, 1f);
            _canvasGroup.alpha = alpha;
        }

        #endregion
    }
}