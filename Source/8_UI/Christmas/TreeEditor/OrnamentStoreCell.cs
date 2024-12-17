using BeatLeader.Models;
using BeatLeader.UI.MainMenu;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeatLeader.Components {
    [RequireComponent(typeof(RectTransform))]
    internal class OrnamentStoreCell : MonoBehaviour {
        private GameObject _parent = null!;

        private ChristmasTreeOrnament _previewInstance = null!;
        private ChristmasOrnamentPool _pool = null!;
        private TMP_Text _text = null!;
        private HoverHint _hint = null;
        private ClickableImage? _previewImage = null;

        private int _bundleId;
        private MapDetail? _song = null;

        public void Setup(ChristmasOrnamentPool pool, GameObject parent) {
            _pool = pool;
            _parent = parent;
        }

        public void SetOpeningDayIndex(int dayIndex) {
            _text.text = $"{19 + dayIndex}th";
            if (dayIndex == 6) {
                _text.color = Color.yellow;
            }
        }

        public async void SetOrnamentStatus(DailyTreeStatus status) {
            if (status.score != null) {
                _bundleId = status.bundleId;
                await _pool.PreloadAsync(status.bundleId);
                ReloadNextInstance();
                if (_previewImage != null) {
                    _previewImage.gameObject.SetActive(false);
                }
            } else {
                _song = status.song;
                if (_previewImage == null) {
                    var previewImage = new GameObject("PreviewImage").AddComponent<ClickableImage>();
                    previewImage.material = Utilities.ImageResources.NoGlowMat;

                    var trans = previewImage.GetComponent<RectTransform>();
                    trans.SetParent(transform, false);
                    trans.sizeDelta = 10f * Vector2.one;
                    _previewImage = previewImage;

                    _previewImage.OnClickEvent += OnClickEvent;

                    SmoothHoverController.Scale(_previewImage.gameObject, 1.0f, 1.2f);
                }
                _previewImage.gameObject.SetActive(true);
                _hint.text = $"Pass {status.song.name} from {status.song.mapper} to unlock this ornament";

                await _previewImage.SetImageAsync($"https://cdn.assets.beatleader.xyz/project_tree_ornament_preview_{status.bundleId}.png");
            }
        }

        public async void SetBonusOrnamentStatus(BonusOrnament status) {
            _bundleId = status.bundleId;
            await _pool.PreloadAsync(status.bundleId);
            ReloadNextInstance(true);
            if (_previewImage != null) {
                _previewImage.gameObject.SetActive(false);
            }
            if (status.description.Length > 0) {
                _hint.text = status.description;
            }
        }

        private void Awake() {
            var image = gameObject.AddComponent<Image>();
            image.sprite = BundleLoader.OrnamentCellBG;
            image.pixelsPerUnitMultiplier = 5f;
            image.type = Image.Type.Sliced;
            image.color = Color.black;

            _hint = BeatSaberUI.DiContainer.InstantiateComponent<HoverHint>(gameObject);

            _text = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            _text.alignment = TextAlignmentOptions.Center;
            _text.fontSize = 5f;
            _text.color = Color.red;

            var trans = _text.GetComponent<RectTransform>();
            trans.SetParent(transform, false);
            trans.sizeDelta = 10f * Vector2.one;

            trans = GetComponent<RectTransform>();
            trans.sizeDelta = 10f * Vector2.one;
        }

        private void ReloadNextInstance(bool despawn = false) {
            if (despawn && _previewInstance != null) {
                _previewInstance.gameObject.SetActive(false);
                _pool.Despawn(_previewInstance);
            }
            _previewInstance = _pool.Spawn(_bundleId, transform, new Vector2(0f, 2.5f));
            _previewInstance.OrnamentGrabbedEvent += HandlePreviewOrnamentGrabbed;
        }

        private void HandlePreviewOrnamentGrabbed(ChristmasTreeOrnament ornament) {
            ornament.OrnamentGrabbedEvent -= HandlePreviewOrnamentGrabbed;
            ReloadNextInstance();
        }

        private void OnClickEvent(PointerEventData _) {
            MapDownloadDialog.OpenSongOrDownloadDialog(_song, transform);
        }
    }
}