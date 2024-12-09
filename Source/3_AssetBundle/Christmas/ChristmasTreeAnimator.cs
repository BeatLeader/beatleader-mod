using UnityEngine;

#nullable disable

namespace BeatLeader {
    public class ChristmasTreeAnimator : MonoBehaviour {
        [SerializeField] private float _animationSpeed = 10f;

        public Vector3 TargetPosition {
            set {
                _positionAnimator.Value = transform.position;
                _positionAnimator.SetTarget(value);
            }
        }

        public float TargetRotation {
            set {
                _rotationAnimator.Value = transform.localEulerAngles;
                _rotationAnimator.SetTarget(value * Vector3.one);
            }
        }

        public float TargetScale {
            set {
                _scaleAnimator.Value = transform.localScale;
                _scaleAnimator.SetTarget(value * Vector3.one);
            }
        }

        private readonly ValueAnimator _scaleAnimator = new();
        private readonly ValueAnimator _positionAnimator = new();
        private readonly ValueAnimator _rotationAnimator = new();

        private void Update() {
            _scaleAnimator.Update();
            _positionAnimator.Update();
            _rotationAnimator.Update();

            if (!_scaleAnimator.Set) {
                transform.localScale = _scaleAnimator.Value;
            }
            if (!_rotationAnimator.Set) {
                transform.localEulerAngles = _rotationAnimator.Value;
            }
            if (!_positionAnimator.Set) {
                transform.position = _positionAnimator.Value;
            }
            
            _scaleAnimator.LateUpdate();
            _positionAnimator.LateUpdate();
            _rotationAnimator.LateUpdate();
        }

        private void Awake() {
            _scaleAnimator.LerpCoefficient = _animationSpeed;
            _positionAnimator.LerpCoefficient = _animationSpeed;
            _rotationAnimator.LerpCoefficient = _animationSpeed;

            transform.localScale = Vector3.zero;
            transform.localEulerAngles = new Vector3(0f, -180f, 0f);
        }
    }
}