using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal interface IDropdownComponentCell<TKey, in TParam> : IKeyedControlComponentCell<TKey, TParam>, ISkewedComponent {
        bool UsedAsPreview { set; }
    }

    /// <typeparam name="TKey">An item key</typeparam>
    /// <typeparam name="TParam">A param to be passed with key to provide additional info</typeparam>
    /// <typeparam name="TCell">A cell component</typeparam>
    internal class Dropdown<TKey, TParam, TCell> : ReactiveComponent, ISkewedComponent, IKeyedControlComponent<TKey, TParam>
        where TCell : IReactiveComponent, ILayoutItem, IDropdownComponentCell<TKey, TParam>, new() {
        #region OptionsModal

        private struct DropdownOption {
            public TKey key;
            public TParam param;

            public override int GetHashCode() {
                return key?.GetHashCode() ?? 0;
            }

            public override bool Equals(object? obj) {
                return obj is DropdownOption opt && opt.key!.Equals(key);
            }
        }

        private class OptionsModal : ModalComponentBase {
            #region CellWrapper

            private class DropdownCellWrapper : ReactiveTableCell<DropdownOption> {
                #region Setup

                private TCell _cell = default!;

                protected override void Init(DropdownOption item) {
                    _cell.Init(item.key, item.param);
                }

                public override void OnCellStateChange(bool selected, bool highlighted) {
                    _cell.OnCellStateChange(selected);
                }

                #endregion

                #region Construct

                protected override GameObject Construct() {
                    return new TCell().Bind(ref _cell).Use(null);
                }

                protected override void OnInitialize() {
                    _cell.CellAskedToBeSelectedEvent += HandleCellAskedToBeSelected;
                }

                #endregion

                #region Callbacks

                private void HandleCellAskedToBeSelected(TKey key) {
                    List!.Select(new() { key = key });
                }

                #endregion
            }

            #endregion

            #region Setup

            private TCell _previewCell = default!;
            private DropdownOption _selectedOption;

            public void Setup(TCell cell) {
                _previewCell = cell;
            }

            public void RefreshPreviewCell() {
                var option = List.SelectedItems.FirstOrDefault();
                if (option.Equals(default)) return;
                _selectedOption = option;
                _previewCell.Init(_selectedOption.key, _selectedOption.param);
            }

            #endregion

            #region Construct

            private const int MaxDisplayedItems = 5;
            private const float ItemSize = 6f;

            public ListComponentBase<DropdownOption> List => _list;

            private ReactiveList<DropdownOption, DropdownCellWrapper> _list = null!;

            protected override void OnOpen() {
                var height = Mathf.Clamp(List.Items.Count, 1, MaxDisplayedItems) * ItemSize + 2;
                this.WithRectSize(height, 40f);
            }

            protected override GameObject Construct() {
                return new Dummy {
                    Children = {
                        new Image {
                            Children = {
                                new ReactiveList<DropdownOption, DropdownCellWrapper>(ItemSize)
                                    .WithListener(
                                        x => x.SelectedItems,
                                        _ => {
                                            RefreshPreviewCell();
                                            CloseInternal();
                                        }
                                    )
                                    .AsFlexItem(grow: 1f)
                                    .Bind(ref _list)
                            }
                        }.AsBlurBackground().AsFlexGroup(
                            padding: new() { top = 1f, bottom = 1f }
                        ).AsFlexItem(grow: 1f),
                        //scrollbar
                        new Scrollbar()
                            .AsFlexItem(size: new() { x = 2f })
                            .With(x => List.Scrollbar = x)
                    }
                }.AsFlexGroup(gap: 2f).Use();
            }

            #endregion
        }

        #endregion

        #region Dropdown

        public IDictionary<TKey, TParam> Items => _items;

        public TKey SelectedKey {
            get => _selectedKey.Value ?? throw new InvalidOperationException("Key cannot be acquired when Items is empty");
            private set {
                _selectedKey = value;
                SelectedKeyChangedEvent?.Invoke(value);
                NotifyPropertyChanged();
            }
        }

        public event Action<TKey>? SelectedKeyChangedEvent;

        private readonly ObservableDictionary<TKey, TParam> _items = new();
        private Optional<TKey> _selectedKey;

        public void Select(TKey key) {
            List.ClearSelection();
            List.Select(new DropdownOption { key = key });
            SelectedKey = key;
            _modal.RefreshPreviewCell();
        }

        private void RefreshSelection() {
            if (_selectedKey.HasValue || Items.Count <= 0) return;
            Select(Items.Keys.First());
        }

        #endregion

        #region Props

        public float Skew {
            get => _skew;
            set {
                _skew = value;
                _button.Image.Skew = value;
                _previewCell.Skew = value;
            }
        }

        private float _skew;

        #endregion

        #region Construct

        private ListComponentBase<DropdownOption> List => _modal.List;

        private OptionsModal _modal = null!;
        private ImageButton _button = null!;
        private TCell _previewCell = default!;

        protected override GameObject Construct() {
            return new ImageButton {
                Image = {
                    Sprite = BundleLoader.Sprites.background,
                    PixelsPerUnit = 12f,
                    Material = GameResources.UINoGlowMaterial
                },
                GrowOnHover = false,
                HoverLerpMul = float.MaxValue,
                Colors = UIStyle.ControlColorSet,
                Children = {
                    new TCell {
                        UsedAsPreview = true
                    }.AsFlexItem(grow: 1f).Bind(ref _previewCell),
                    //icon
                    new Image {
                        Sprite = GameResources.Sprites.ArrowIcon,
                        Color = Color.white.ColorWithAlpha(0.8f),
                        PreserveAspect = true
                    }.AsFlexItem(
                        size: new() { x = 3f }
                    ),
                    //modal
                    new OptionsModal()
                        .With(x => x.Setup(_previewCell))
                        .Bind(ref _modal)
                }
            }.AsFlexGroup(
                padding: new() { left = 2f, right = 2f }
            ).WithModal(_modal).Bind(ref _button).Use();
        }

        protected override void OnInitialize() {
            this.AsFlexItem(size: new() { x = 36f, y = 6f });
            Skew = UIStyle.Skew;
            _items.ItemAddedEvent += HandleItemAdded;
            _items.ItemRemovedEvent += HandleItemRemoved;
            _items.AllItemsRemovedEvent += HandleAllItemsRemoved;
            List.WithListener(
                x => x.SelectedItems,
                x => {
                    var item = x.FirstOrDefault();
                    if (item.Equals(default)) return;
                    SelectedKey = item.key;
                }
            );
        }

        #endregion

        #region Callbacks

        private void HandleItemAdded(TKey key, TParam param) {
            List.Items.Add(
                new() {
                    key = key,
                    param = param
                }
            );
            List.Refresh(false);
            RefreshSelection();
            NotifyPropertyChanged(nameof(Items));
        }

        private void HandleItemRemoved(TKey key) {
            List.Items.Remove(new() { key = key });
            List.Refresh();
            RefreshSelection();
            NotifyPropertyChanged(nameof(Items));
        }

        private void HandleAllItemsRemoved() {
            List.Items.Clear();
            List.Refresh();
            RefreshSelection();
            NotifyPropertyChanged(nameof(Items));
        }

        #endregion
    }
}