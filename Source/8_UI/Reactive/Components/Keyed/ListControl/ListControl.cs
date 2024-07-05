using System;
using System.Collections.Generic;
using BeatLeader.UI.Reactive.Yoga;
using BeatLeader.Utils;
using HMUI;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class ListControl<TKey, TParam, TCell> : ReactiveComponent, IKeyedControlComponent<TKey, TParam>
        where TCell : IReactiveComponent, ILayoutItem, IPreviewableCell, IKeyedControlComponentCellBase<TKey, TParam>, new() {
        #region ListControl

        public IDictionary<TKey, TParam> Items => _items;
        public TKey SelectedKey => _selectedKey ?? throw new InvalidOperationException("Key cannot be acquired when Items is empty");

        private bool CanSelectPrevious => _selectedIndex > 0;
        private bool CanSelectNext => _selectedIndex < _items.Count - 1;

        public event Action<TKey>? SelectedKeyChangedEvent;

        private readonly ObservableDictionary<TKey, TParam> _items = new();
        private readonly List<TKey> _keys = new();
        private TCell _cell = default!;
        private TKey? _selectedKey;
        private int _selectedIndex = -1;

        public void SelectSilent(TKey key) {
            _selectedKey = key;
            _cell.Init(key, _items[key]);
            _cell.Enabled = true;
            ValidateIndex();
            RefreshButtons();
        }

        public void Select(TKey key) {
            if (key!.Equals(_selectedKey)) return;
            SelectSilent(key);
            SelectedKeyChangedEvent?.Invoke(_selectedKey!);
            NotifyPropertyChanged(nameof(SelectedKey));
        }

        private void SelectNext() {
            ValidateIndex();
            if (!CanSelectNext) return;
            Select(_keys[++_selectedIndex]);
        }

        private void SelectPrev() {
            ValidateIndex();
            if (!CanSelectPrevious) return;
            Select(_keys[--_selectedIndex]);
        }

        private void ValidateIndex() {
            _selectedIndex = _keys.FindIndex(_selectedKey);
        }

        private void TrySelect() {
            RefreshButtons();
            if (_selectedKey != null) return;
            if (_items.Count == 0) {
                _cell.Enabled = false;
                return;
            }
            Select(_keys[0]);
        }

        private void RefreshButtons() {
            _nextButton.Interactable = CanSelectNext;
            _prevButton.Interactable = CanSelectPrevious;
        }

        #endregion

        #region Construct

        private ButtonBase _nextButton = null!;
        private ButtonBase _prevButton = null!;

        protected override GameObject Construct() {
            static ButtonBase CreateButton(
                bool applyColor1,
                float iconRotation,
                Justify justify,
                YogaFrame position
            ) {
                var colorSet = new StateColorSet {
                    HoveredColor = Color.white.ColorWithAlpha(0.3f),
                    Color = Color.clear
                };
                return new ImageButton {
                    Image = {
                        Color = Color.white,
                        Sprite = BundleLoader.Sprites.background,
                        PixelsPerUnit = 12f,
                        GradientDirection = ImageView.GradientDirection.Horizontal,
                        Material = GameResources.UINoGlowMaterial
                    },
                    GrowOnHover = false,
                    HoverLerpMul = float.MaxValue,
                    Colors = null,
                    GradientColors0 = applyColor1 ? null : colorSet,
                    GradientColors1 = applyColor1 ? colorSet : null,
                    Children = {
                        //icon
                        new Image {
                            Sprite = GameResources.Sprites.ArrowIcon,
                            PreserveAspect = true,
                            Color = Color.white.ColorWithAlpha(0.8f),
                            ContentTransform = {
                                localEulerAngles = new(0f, 0f, iconRotation)
                            }
                        }.AsFlexItem(aspectRatio: 1f).Export(out var icon)
                    }
                }.WithListener(
                    x => x.Interactable,
                    x => icon.Color = x ?
                        Color.white.ColorWithAlpha(0.8f) :
                        (Color.white * 0.9f).ColorWithAlpha(0.25f)
                ).AsFlexGroup(
                    padding: 1.5f,
                    justifyContent: justify
                ).AsFlexItem(
                    size: new() { x = "50%", y = "100%" },
                    position: position
                );
            }

            return new Image {
                Children = {
                    //
                    new TCell {
                        UsedAsPreview = true
                    }.AsFlexItem(
                        grow: 1f,
                        margin: new() { left = 5f, right = 5f }
                    ).Bind(ref _cell),
                    //buttons
                    new Dummy {
                        Children = {
                            CreateButton(
                                false,
                                270f,
                                Justify.FlexStart,
                                new() { top = 0f, left = 0f }
                            ).WithClickListener(SelectPrev).Bind(ref _prevButton),
                            //
                            CreateButton(
                                true,
                                90f,
                                Justify.FlexEnd,
                                new() { top = 0f, right = 0f }
                            ).WithClickListener(SelectNext).Bind(ref _nextButton)
                        }
                    }.AsFlexGroup().WithRectExpand()
                }
            }.AsFlexGroup().AsBackground(color: UIStyle.InputColorSet.Color).Use();
        }

        #endregion

        #region Setup

        protected override void OnInitialize() {
            this.AsFlexItem(size: new() { x = 40f, y = 6f });
            _items.ItemAddedEvent += HandleItemAdded;
            _items.ItemRemovedEvent += HandleItemRemoved;
            RefreshButtons();
        }

        #endregion

        #region Callbacks

        private void HandleItemAdded(TKey key, TParam param) {
            _keys.Add(key);
            TrySelect();
            NotifyPropertyChanged(nameof(Items));
        }

        private void HandleItemRemoved(TKey key) {
            _keys.Remove(key);
            if (_selectedKey?.Equals(key) ?? false) {
                _selectedKey = default;
            }
            TrySelect();
            NotifyPropertyChanged(nameof(Items));
        }

        #endregion
    }
}