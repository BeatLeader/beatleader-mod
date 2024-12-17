using System.Linq;
using BeatLeader.API.Methods;
using BeatLeader.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BeatLeader.Components {
    [RequireComponent(typeof(RectTransform))]
    internal class BonusOrnamentStorePanel : MonoBehaviour {
        #region Setup

        private TreeStatus? _treeStatus;
        private StaticScreen _screen = null!;

        private int _totalPages;
        private int _page;

        public void Setup(ChristmasOrnamentPool pool) {
            foreach (var cell in _cells) {
                cell.Setup(pool, gameObject);
            }
        }

        public void Present() {
            _screen.Present();
        }

        public void Dismiss() {
            _screen.Dismiss();
        }

        #endregion

        #region Cells

        private readonly OrnamentStoreCell[] _cells = new OrnamentStoreCell[MaxVerticalCells];

        private void Reload() {
            if (_treeStatus?.bonusOrnaments is not { } ornaments) {
                return;
            }
            _totalPages = Mathf.CeilToInt(ornaments.Length / (float)MaxVerticalCells);
            _page = 0;
        }

        private void RefreshPage() {
            RefreshButtons();
            if (_treeStatus?.bonusOrnaments is not { } ornaments) {
                return;
            }
            var size = ornaments.Length;
            for (var i = 0; i < MaxVerticalCells; i++) {
                var idx = i + MaxVerticalCells * _page;
                var cell = _cells[i];

                if (idx >= size) {
                    cell.gameObject.SetActive(false);
                } else {
                    var ornament = ornaments[idx];
                    cell.gameObject.SetActive(true);
                    cell.SetBonusOrnamentStatus(ornament);
                }
            }
        }

        private void RefreshButtons() {
            _upButton.interactable = _page > 0;
            _downButton.interactable = _page < _totalPages - 1;
        }

        #endregion

        #region UI
        
        private static readonly Sprite arrowSprite = Resources.FindObjectsOfTypeAll<Sprite>().First(x => x.name == "ArrowIcon");
        private static readonly Material noGlowMaterial = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "UINoGlow");

        private const int MaxVerticalCells = 3;
        private const int CellSize = 10;
        private const int GapSize = 1;

        private Button _upButton = null!;
        private Button _downButton = null!;
        
        // Hardcode because of the schedule as usual
        private void Awake() {
            _screen = gameObject.AddComponent<StaticScreen>();

            var group = new GameObject("VerticalLayoutGroup").AddComponent<VerticalLayoutGroup>();
            group.childAlignment = TextAnchor.MiddleCenter;
            group.spacing = GapSize;
            group.childControlHeight = false;
            group.childForceExpandHeight = false;

            var trans = group.GetComponent<RectTransform>();
            var cellsHeight = CalcSize(MaxVerticalCells);
            trans.SetParent(transform, false);
            trans.sizeDelta = new Vector2(CellSize, cellsHeight);

            for (var i = 0; i < MaxVerticalCells; i++) {
                var cell = new GameObject("OrnamentStoreCell").AddComponent<OrnamentStoreCell>();
                cell.transform.SetParent(group.transform, false);
                _cells[i] = cell;
            }

            var buttonOffset = cellsHeight / 2f + 2.5f;
            
            var upButton = CreateButton(HandleUpButtonClicked, out _upButton);
            upButton.transform.SetParent(transform, false);
            upButton.transform.localPosition = new Vector3(0f, buttonOffset);
            upButton.transform.localEulerAngles = new Vector3(0f, 0f, 180f);

            var downButton = CreateButton(HandleDownButtonClicked, out _downButton);
            downButton.transform.SetParent(transform, false);
            downButton.transform.localPosition = new Vector3(0f, -buttonOffset);
            
            TreeMapRequest.AddStateListener(HandleTreeStatusRequestState);
            TreeMapRequest.SendRequest();
        }

        private static GameObject CreateButton(UnityAction callback, out Button button) {
            var go = new GameObject("Button");

            button = go.AddComponent<Button>();
            button.onClick.AddListener(callback);
            button.colors = new ColorBlock {
                normalColor = Color.white.ColorWithAlpha(0.5f),
                highlightedColor = Color.white,
                disabledColor = Color.black.ColorWithAlpha(0.5f),
                colorMultiplier = 1f
            };

            var image = go.AddComponent<Image>();
            image.sprite = arrowSprite;
            image.material = noGlowMaterial;
            image.preserveAspect = true;
            button.image = image;
            
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(3f, 5f);

            return go;
        }

        private static int CalcSize(int count) {
            return CellSize * count + GapSize * (count - 1);
        }

        private void OnDestroy() {
            TreeMapRequest.RemoveStateListener(HandleTreeStatusRequestState);
        }

        #endregion

        #region Callbacks

        private void HandleUpButtonClicked() {
            if (_page == 0) {
                return;
            }
            _page--;
            RefreshPage();
        }

        private void HandleDownButtonClicked() {
            if (_page == _totalPages - 1) {
                return;
            }
            _page++;
            RefreshPage();
        }

        private void HandleTreeStatusRequestState(API.RequestState state, TreeStatus status, string? failReason) {
            if (state != API.RequestState.Finished) {
                return;
            }
            _treeStatus = status;
            Reload();
            RefreshPage();
        }

        #endregion
    }
}