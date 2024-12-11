using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    [RequireComponent(typeof(RectTransform))]
    internal class OrnamentStoreCell : MonoBehaviour {
        private ChristmasTreeOrnament _previewInstance = null!;
        private ChristmasOrnamentPool _pool = null!;
        private TMP_Text _text = null!;
        private int _bundleId;

        public void Setup(ChristmasOrnamentPool pool) {
            _pool = pool;
        }

        public void SetOpeningDay(int day) {
            _text.text = $"Day {day}";
        }

        public async void SetBundleId(int bundleId) {
            _bundleId = bundleId;
            await _pool.PreloadAsync(bundleId);
            ReloadNextInstance();
        }

        private void Awake() {
            var image = gameObject.AddComponent<Image>();
            image.sprite = BundleLoader.OrnamentCellBG;
            image.pixelsPerUnitMultiplier = 5f;
            image.type = Image.Type.Sliced;

            _text = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            _text.alignment = TextAlignmentOptions.Center;
            _text.fontSize = 4f;
            _text.color = Color.red;
            var trans = _text.GetComponent<RectTransform>();
            trans.SetParent(transform, false);
            trans.sizeDelta = 10f * Vector2.one;
        }

        private void ReloadNextInstance() {
            _previewInstance = _pool.Spawn(_bundleId, transform, new Vector2(0f, 2.5f));
            _previewInstance.OrnamentGrabbedEvent += HandlePreviewOrnamentGrabbed;
        }

        private void HandlePreviewOrnamentGrabbed(ChristmasTreeOrnament ornament) {
            ornament.OrnamentGrabbedEvent -= HandlePreviewOrnamentGrabbed;
            ReloadNextInstance();
        }
    }
}