using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    [RequireComponent(typeof(RectTransform))]
    //TODO: Add a text at the bottom displaying items left + cell frame
    internal class OrnamentStoreCell : MonoBehaviour {
        private readonly Stack<ChristmasTreeOrnament> _cachedOrnaments = new();
        private ChristmasTreeOrnament _previewInstance = null!;
        private int _bundleId;

        public async void Setup(int bundleId) {
            _bundleId = bundleId;
            await ChristmasOrnamentLoader.EnsureOrnamentPrefabLoaded(bundleId);
            ReloadPreviewInstance();
        }

        private void Awake() {
            var image = gameObject.AddComponent<Image>();
            image.sprite = BundleLoader.WhiteBG;
            image.color = Color.white.ColorWithAlpha(0.3f);
            image.pixelsPerUnitMultiplier = 5f;
            image.type = Image.Type.Sliced;
        }

        private void ReloadPreviewInstance() {
            _previewInstance = GetOrnament();
            _previewInstance.OrnamentGrabbedEvent += HandlePreviewOrnamentGrabbed;
        }

        private ChristmasTreeOrnament GetOrnament() {
            ChristmasTreeOrnament ornament;
            if (_cachedOrnaments.Count > 0) {
                ornament = _cachedOrnaments.Pop();
            } else {
                // supposing that it was previously loaded (sorry, but I don't have any passion to implement it in a proper way)
                var go = ChristmasOrnamentLoader.LoadOrnamentInstanceAsync(_bundleId).Result;
                go.transform.SetParent(transform, false);
                ornament = go.AddComponent<ChristmasTreeOrnament>();
            }
            ornament.OrnamentDeinitEvent += HandleOrnamentInstanceDeinitialized;
            return ornament;
        }

        private void HandleOrnamentInstanceDeinitialized(ChristmasTreeOrnament ornament) {
            _cachedOrnaments.Push(ornament);
        }

        private void HandlePreviewOrnamentGrabbed(ChristmasTreeOrnament ornament) {
            ornament.OrnamentGrabbedEvent -= HandlePreviewOrnamentGrabbed;
            ReloadPreviewInstance();
        }
    }
}