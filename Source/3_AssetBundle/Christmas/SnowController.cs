using UnityEngine;

#nullable disable

namespace BeatLeader {
    public class SnowController : MonoBehaviour {
        [SerializeField]
        private ParticleSystem _particleSystem;

        // To cover scene transitions
        private void OnTransformParentChanged() {
            if (!_particleSystem.emission.enabled) {
                return;
            }
            Play(true);
        }

        public void Play(bool immediately) {
            var emission = _particleSystem.emission;
            emission.enabled = true;
            if (immediately) {
                var duration = _particleSystem.main.startLifetimeMultiplier;
                _particleSystem.Simulate(duration, true, true, true);
            }
            _particleSystem.Play();
        }

        public void Stop() {
            var emission = _particleSystem.emission;
            emission.enabled = false;
            _particleSystem.Stop();
            _particleSystem.Clear();
        }
    }
}