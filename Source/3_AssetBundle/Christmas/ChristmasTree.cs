using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;

#nullable disable

namespace BeatLeader {
    public class ChristmasTree : MonoBehaviour {
        [SerializeField] private ChristmasTreeLevel[] _levels;
        [SerializeField] private CapsuleCollider _collider;
        [SerializeField] private ChristmasTreeAnimator _animator;
        public bool gizmos;

        public Collider AreaCollider => _collider;

        public void Present() {
            _animator.TargetScale = 1f;
        }

        public void Dismiss() {
            _animator.TargetScale = 0f;
        }

        public void MoveTo(Vector3 pos) {
            _animator.TargetPosition = pos;
        }

        public void ScaleTo(float size) {
            _animator.TargetScale = size;
        }

        #region LoadOrnaments

        private readonly List<GameObject> _ornaments = new();

        internal async Task LoadOrnaments(ChristmasTreeSettings settings) {
            var size = settings.ornaments.Length;
            var tasks = new Task<GameObject>[size];

            foreach (var ornament in _ornaments) {
                Destroy(ornament);
            }
            _ornaments.Clear();

            for (var i = 0; i < size; i++) {
                tasks[i] = ChristmasOrnamentLoader.LoadOrnamentInstanceAsync(settings.ornaments[i].bundleId);
            }
            await Task.WhenAll(tasks);

            for (var i = 0; i < size; i++) {
                var instance = tasks[i].Result;
                var trans = instance.transform;
                trans.SetParent(transform, false);
                trans.SetLocalPose(settings.ornaments[i].pose);
                _ornaments.Add(instance);
            }
        }

        #endregion

        #region Editor

        public Vector3 Align(Vector3 pos) {
            var y = pos.y;

            var level = _levels.FirstOrDefault(level => y >= level.bottomHeight && y <= level.topHeight);
            if (level == null) {
                Debug.LogWarning("Position does not align with any tree level.");
                return pos;
            }

            var t = (y - level.bottomHeight) / (level.topHeight - level.bottomHeight);
            var radiusAtHeight = Mathf.Lerp(level.bottomRadius, level.topRadius, t);

            var xz = new Vector2(pos.x, pos.z);
            xz = radiusAtHeight * (xz == Vector2.zero ? Vector2.right : xz.normalized);

            return new Vector3(xz.x, pos.y, xz.y);
        }

        private void OnDrawGizmos() {
            if (!gizmos) return;
            foreach (var t in _levels) {
                t.Draw(transform.lossyScale.x);
            }
        }

        #endregion
    }
}