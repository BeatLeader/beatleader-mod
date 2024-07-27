using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class FloatingObject : MonoBehaviour {
        public float amplitude = 0.5f;
        public float speed = 1.0f;
        public float rotationAmplitude = 20f;
        public float rotationSpeed = 0.5f;

        private Vector3 _startPos;

        private void Start() {
            _startPos = transform.position;
        }

        private void Update() {
            var trans = transform;
            //
            var posY = _startPos.y + Mathf.Sin(speed * Time.time) * amplitude;
            var pos = trans.position;
            trans.position = new Vector3(pos.x, posY, pos.z);
            //
            var rotY = Mathf.Sin(rotationSpeed * Time.time) * rotationAmplitude;
            trans.localEulerAngles = new(0f, rotY, 0f);
        }
    }
}