using HMUI;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class BeatLeaderNewsViewController : ViewController {
        #region Setup

        private void Awake() {
            new Layout {
                Children = {
                    new TextNewsPanel(),
                    
                    new Layout {
                        Children = {
                            new KeyedContainer<string> {
                                Items = {
                                    ["default"] = new Layout {
                                        Children = {
                                            new EventNewsPanel(),
                                            new MapNewsPanel(),
                                        }
                                    }
                                    .AsFlexGroup(direction: FlexDirection.Column, gap: 1f)
                                    .AsFlexItem(size: 100.pct()),
                                    
                                    ["seasonal"] = new Layout()
                                }
                            }
                            .AsFlexGroup()
                            .AsFlexItem(flex: 1f)
                            .Export(out var container),
                            
                            // Selector
                            new Background {
                                Children = {
                                    new TextSegmentedControl<string> {
                                        Items = {
                                            ["default"] = "<i>Main",
                                            ["seasonal"] = "<i>Seasonal"
                                        }
                                    }
                                    .With(x => container.Control = x)
                                    .AsFlexItem(size: new() { y = 5f }, flexGrow: 1f)
                                }
                            }
                            .AsBackground(color: Color.black.ColorWithAlpha(0.5f))
                            .AsFlexItem(margin: new() { top = 1f })
                            .AsFlexGroup()
                        }
                    }
                    .AsFlexGroup(direction: FlexDirection.Column)
                    .AsFlexItem(
                        flex: 1f, 
                        size: new() { x = 70f }
                    )
                }
            }.AsFlexGroup(
                direction: FlexDirection.Row,
                constrainHorizontal: false,
                constrainVertical: false,
                gap: 1f,
                padding: new() { left = 30f }
            )
            .AsFlexItem()
            .Use(transform);

            UpdateScreen();
        }

        private void OnEnable() {
            if (!_initialized) return;
            UpdateScreen();
        }

        private void OnDisable() {
            RevertScreenChanges();
        }

        public override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            ReeModalSystem.CloseAll();
            gameObject.SetActive(false);
        }

        #endregion

        #region Screen Changes

        private static Vector2 TargetScreenSize => new Vector2(186, 80);

        private RectTransform _screenTransform;
        private Vector2 _originalScreenSize;
        private bool _initialized;

        private bool LazyInitializeScreen() {
            if (_initialized) return true;
            if (screen == null) return false;
            _screenTransform = screen.GetComponent<RectTransform>();
            _originalScreenSize = _screenTransform.sizeDelta;
            _initialized = true;
            return true;
        }

        private void UpdateScreen() {
            if (!LazyInitializeScreen()) return;
            _screenTransform.sizeDelta = TargetScreenSize;
        }

        private void RevertScreenChanges() {
            if (!LazyInitializeScreen()) return;
            _screenTransform.sizeDelta = _originalScreenSize;
        }

        #endregion
    }
}