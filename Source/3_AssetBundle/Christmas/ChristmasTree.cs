using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.API.Methods;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;

#nullable disable

namespace BeatLeader {
    public class ChristmasTree : MonoBehaviour {
        [SerializeField] private ChristmasTreeLevel[] _levels;
        [SerializeField] private ChristmasTreeAnimator _animator;
        [SerializeField] private ChristmasTreeMover _mover;
        [SerializeField] private Transform _mesh;
        [SerializeField] private float _radius;
        public bool gizmos;

        public Transform Origin => _mesh;
        internal ChristmasOrnamentPool OrnamentsPool { get; private set; }

        private bool _moverFull;
        private bool _moverRestricted;

        public void SetMoverFull(bool value) {
            _moverFull = value;
            _mover.SetEnabled(_moverFull, _moverRestricted);
        }

        public void SetMoverRestricted(bool value) {
            _moverRestricted = value;
            _mover.SetEnabled(_moverFull, _moverRestricted);
        }

        public void Present() {
            _animator.TargetScale = _settings.gameTreePose.scale.x;
        }

        public void Dismiss() {
            _animator.TargetScale = 0f;
        }

        public void MoveTo(Vector3 pos, bool immediate = false) {
            _animator.TargetPosition = pos;
            if (immediate) {
                _animator.EvaluatePosImmediate();
            }
        }

        public void ScaleTo(float size, bool immediate = false) {
            _animator.TargetScale = size;
            if (immediate) {
                _animator.EvaluateScaleImmediate();
            }
        }

        private void Awake() {
            OrnamentsPool = new(this);
        }

        #region Settings

        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private ChristmasTreeSettings _settings = new() {
            gameTreePose = new FullSerializablePose {
                position = new Vector3(2.7f, 0f, 4f),
                rotation = new Quaternion(0, 0, 0, 1),
                scale = Vector3.one
            },
            ornaments = Array.Empty<ChristmasTreeOrnamentSettings>()
        };

        public async Task LoadSettings(ChristmasTreeSettings settings, bool move = true) {
            await _semaphore.WaitAsync();

            _settings = settings;
            await LoadOrnaments(settings);
            if (move) {
                MoveTo(settings.gameTreePose.position, true);
                ScaleTo(settings.gameTreePose.scale.x);
            }

            _semaphore.Release();
        }

        #endregion

        #region Ornaments

        internal IReadOnlyCollection<ChristmasTreeOrnament> Ornaments => _ornaments;

        private readonly HashSet<ChristmasTreeOrnament> _ornaments = new();

        internal void AddOrnament(ChristmasTreeOrnament ornament) {
            _ornaments.Add(ornament);
        }

        internal void RemoveOrnament(ChristmasTreeOrnament ornament) {
            _ornaments.Remove(ornament);
        }

        private async Task LoadOrnaments(ChristmasTreeSettings settings) {
            var size = settings.ornaments.Length;
            var tasks = new Task[size];

            foreach (var ornament in _ornaments) {
                OrnamentsPool.Despawn(ornament);
            }
            _ornaments.Clear();

            for (var i = 0; i < size; i++) {
                tasks[i] = OrnamentsPool.PreloadAsync(settings.ornaments[i].bundleId);
            }
            await Task.WhenAll(tasks);

            for (var i = 0; i < size; i++) {
                var ornament = settings.ornaments[i];
                var instance = OrnamentsPool.Spawn(ornament.bundleId, transform, default);
                instance.transform.SetLocalPose(settings.ornaments[i].pose);
                _ornaments.Add(instance);
            }
        }

        #endregion

        #region Math

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

        public bool HasAreaContact(Vector3 pos) {
            var mul = _mesh.localScale;
            pos = _mesh.InverseTransformPoint(pos);
            return Mathf.Abs(pos.x) <= _radius * mul.x && Mathf.Abs(pos.z) <= _radius * mul.z;
        }

        #endregion

        #region Editor

        private void OnDrawGizmos() {
            if (!gizmos) return;
            foreach (var t in _levels) {
                t.Draw(transform.lossyScale.x);
            }
        }

        #endregion
    }
}