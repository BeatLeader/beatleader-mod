using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Utils;
using BGLib.UnityExtension;
using UnityEngine;

#nullable disable

namespace BeatLeader {
    public class ChristmasTree : MonoBehaviour {
        [SerializeField]
        private ChristmasTreeLevel[] _levels;

        [SerializeField]
        private Transform _animationContainer;

        public bool gizmos;
        public float animationSpeed = 10f;

        private float _targetScale;
        private float _targetRotation;
        private bool _set = true;

        #region Animation

        public void Present() {
            _targetScale = 1f;
            _targetRotation = 0f;
            _set = false;
        }

        public void Dismiss() {
            _targetScale = 0f;
            _targetRotation = -180f;
            _set = false;
        }

        private void Awake() {
            _animationContainer.localScale = Vector3.zero;
            _animationContainer.localEulerAngles = new Vector3(0f, -180f, 0f);
        }

        private void Update() {
            if (_set) return;

            var t = Time.deltaTime * animationSpeed;
            var targetScale = _targetScale * Vector3.one;

            var scale = Vector3.Lerp(_animationContainer.localScale, targetScale, t);
            var rotation = Mathf.Lerp(_animationContainer.localEulerAngles.y, _targetRotation, t);

            if (Mathf.Abs(scale.x - targetScale.x) <= 0.001f) {
                scale = targetScale;
                rotation = _targetRotation;
                _set = true;
            }

            _animationContainer.localScale = scale;
            _animationContainer.localEulerAngles = new Vector3(0f, rotation, 0f);
        }

        #endregion

        #region LoadOrnaments

        private static readonly Dictionary<int, GameObject?> prefabs = new();
        private readonly List<GameObject> _ornaments = new();

        internal async Task LoadOrnaments(ChristmasTreeSettings settings) {
            var size = settings.ornaments.Length;
            var tasks = new Task[size];

            foreach (var ornament in _ornaments) {
                Destroy(ornament);
            }
            _ornaments.Clear();

            for (var i = 0; i < size; i++) {
                tasks[i] = LoadOrnamentPrefabAsync(settings.ornaments[i]);
            }
            await Task.WhenAll(tasks);

            for (var i = 0; i < size; i++) {
                var ornament = settings.ornaments[i];
                var prefab = prefabs[ornament.bundleId];
                if (prefab == null) {
                    continue;
                }
                var instance = Instantiate(prefab, transform, false);
                instance.transform.SetLocalPose(ornament.pose);
                _ornaments.Add(instance);
            }
        }

        private static async Task LoadOrnamentPrefabAsync(ChristmasTreeOrnament ornament) {
            var id = ornament.bundleId;
            if (prefabs.ContainsKey(id)) {
                return;
            }
            prefabs[id] = null;
            Plugin.Log.Info($"Loading ornament bundle {id}.");

            var path = $"https://cdn.assets.beatleader.xyz/project_tree_ornament_{id}.bundle";
            var res = await WebUtils.HttpClient.GetAsync(path);

            if (!res.IsSuccessStatusCode) {
                Plugin.Log.Error($"Failed to download ornament from {path}: {res.StatusCode}");
                return;
            }

            try {
                using (var stream = await res.Content.ReadAsStreamAsync()) {
                    var bundle = await AssetBundle.LoadFromStreamAsync(stream);

                    var prefab = await bundle.LoadAllAssetsAsync<GameObject>();
                    if (prefab == null) {
                        throw new Exception("Prefab is null");
                    }
                    prefabs[id] = (GameObject)prefab;
                    Plugin.Log.Info($"Loaded ornament {id}.");
                        
                    bundle.Unload(false);
                }
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to load ornament: {ex}");
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