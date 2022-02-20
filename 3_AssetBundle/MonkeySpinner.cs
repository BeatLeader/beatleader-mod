using UnityEngine;

namespace BeatLeader {
    public class MonkeySpinner : MonoBehaviour {
        [SerializeField] private Vector3 angularVelocity;

        private void Update() {
            transform.Rotate(angularVelocity * Time.deltaTime);
        }
    }
}