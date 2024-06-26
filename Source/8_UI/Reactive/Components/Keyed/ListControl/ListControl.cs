using System;
using System.Collections.Generic;
using BeatLeader.UI.Reactive.Yoga;
using BeatLeader.Utils;
using HMUI;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class ListControl<TKey, TParam, TCell> : ReactiveComponent, IKeyedControlComponent<TKey, TParam>
        where TCell : IReactiveComponent, ILayoutItem, IKeyedControlComponentCellBase<TKey, TParam>, new() {
        #region ListControl

        public IDictionary<TKey, TParam> Items => _items;

        public TKey SelectedKey {
            get => _selectedKey ?? throw new InvalidOperationException("Key cannot be acquired when Items is empty");
            private set {
                if (value!.Equals(_selectedKey)) return;
                _selectedKey = value;
                SelectedKeyChangedEvent?.Invoke(value);
                NotifyPropertyChanged();
            }
        }

        private bool CanSelectPrevious => _selectedIndex > 0;
        private bool CanSelectNext => _selectedIndex < _items.Count - 1;

        public event Action<TKey>? SelectedKeyChangedEvent;

        private readonly ObservableDictionary<TKey, TParam> _items = new();
        private readonly List<TKey> _keys = new();
        private TCell _cell = default!;
        private TKey? _selectedKey;
        private int _selectedIndex = -1;

        public void SelectSilent(TKey key) {
            _cell.Init(key, _items[key]);
            _cell.Enabled = true;
        }
        
        public void Select(TKey key) {
            SelectSilent(key);
            SelectedKey = key;
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
            if (_selectedIndex == -1) {
                _selectedIndex = _keys.FindIndex(_selectedKey);
            }
        }

        private void TrySelect() {
            if (_selectedKey != null) return;
            if (_items.Count == 0) {
                _cell.Enabled = false;
                return;
            }
            Select(_keys[0]);
        }

        #endregion

        #region Construct

        protected override GameObject Construct() {
            //temporary solution
            static ButtonBase CreateButton(
                bool applyColor1,
                float iconRotation,
                Justify justify,
                YogaFrame position
            ) {
                return new ImageButton {
                    Image = {
                        Sprite = BundleLoader.Sprites.background,
                        PixelsPerUnit = 12f,
                        GradientDirection = ImageView.GradientDirection.Horizontal,
                        Material = GameResources.UINoGlowMaterial
                    },
                    GrowOnHover = false,
                    HoverLerpMul = float.MaxValue,
                    Colors = null,
                    Children = {
                        //icon
                        new Image {
                            Sprite = GameResources.Sprites.ArrowIcon,
                            PreserveAspect = true,
                            Color = Color.white.ColorWithAlpha(0.8f),
                            ContentTransform = {
                                localEulerAngles = new(0f, 0f, iconRotation)
                            }
                        }.AsFlexItem(aspectRatio: 1f)
                    }
                }.With(
                    x => {
                        var animatedSet = new StateColorSet {
                            HoveredColor = Color.white.ColorWithAlpha(0.3f),
                            Color = Color.clear
                        };
                        x.Image.Color = Color.white;
                        if (!applyColor1) {
                            x.GradientColors0 = animatedSet;
                        } else {
                            x.GradientColors1 = animatedSet;
                        }
                    }
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
                    CreateButton(
                        false,
                        270f,
                        Justify.FlexStart,
                        new() { top = 0f, left = 0f }
                    ).WithClickListener(SelectPrev),
                    //
                    new TCell().AsFlexItem(
                        grow: 1f,
                        margin: new() { left = 5f, right = 5f }
                    ).Bind(ref _cell),
                    //
                    CreateButton(
                        true,
                        90f,
                        Justify.FlexEnd,
                        new() { top = 0f, right = 0f }
                    ).WithClickListener(SelectNext)
                }
            }.AsFlexGroup().AsBackground(color: UIStyle.InputColorSet.Color).Use();
        }

        #endregion

        #region Setup

        protected override void OnInitialize() {
            _items.ItemAddedEvent += HandleItemAdded;
            _items.ItemRemovedEvent += HandleItemRemoved;
            this.AsFlexItem(size: new() { x = 40f, y = 6f });
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