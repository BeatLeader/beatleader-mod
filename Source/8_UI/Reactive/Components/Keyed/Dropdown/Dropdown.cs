using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    /// <typeparam name="TKey">An item key</typeparam>
    /// <typeparam name="TParam">A param to be passed with key to provide additional info</typeparam>
    /// <typeparam name="TCell">A cell component</typeparam>
    internal class Dropdown<TKey, TParam, TCell> : ReactiveComponent, ISkewedComponent, IInteractableComponent, IKeyedControlComponent<TKey, TParam>
        where TCell : IReactiveComponent, ILayoutItem, ISkewedComponent, IPreviewableCell, IKeyedControlComponentCell<TKey, TParam>, new() {
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

        private class OptionsModal : AnimatedModalComponentBase {
            #region CellWrapper

            public class DropdownCellWrapper : TableComponentCell<DropdownOption> {
                #region Setup

                private TCell _cell = default!;

                protected override void OnInit(DropdownOption item) {
                    _cell.Init(item.key, item.param);
                }

                protected override void OnCellStateChange(bool selected) {
                    _cell.OnCellStateChange(selected);
                }

                #endregion

                #region Construct

                protected override GameObject Construct() {
                    return new TCell().Bind(ref _cell).Use(null);
                }

                protected override void OnInitialize() {
                    this.WithSizeDelta(0f, 6f);
                    _cell.CellAskedToBeSelectedEvent += HandleCellAskedToBeSelected;
                }

                #endregion

                #region Callbacks

                private void HandleCellAskedToBeSelected(TKey key) {
                    SelectSelf(true);
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
                if (Table.SelectedIndexes.Count == 0) return;
                var index = Table.SelectedIndexes.First();
                _selectedOption = Table.FilteredItems[index];
                _previewCell.Init(_selectedOption.key, _selectedOption.param);
            }

            #endregion

            #region Construct

            private const int MaxDisplayedItems = 5;
            private const float ItemSize = 6f;

            public Table<DropdownOption, DropdownCellWrapper> Table => _table;

            private Table<DropdownOption, DropdownCellWrapper> _table = null!;

            protected override void OnOpen(bool finished) {
                if (finished) return;
                var height = Mathf.Clamp(Table.Items.Count, 1, MaxDisplayedItems) * ItemSize + 2;
                this.WithSizeDelta(40f, height);
            }

            protected override GameObject Construct() {
                return new Dummy {
                    Children = {
                        new Image {
                            Children = {
                                new Table<DropdownOption, DropdownCellWrapper>()
                                    .WithListener(
                                        x => x.SelectedIndexes,
                                        _ => {
                                            RefreshPreviewCell();
                                            CloseInternal();
                                        }
                                    )
                                    .AsFlexItem(grow: 1f)
                                    .Bind(ref _table)
                            }
                        }.AsBlurBackground().AsFlexGroup(
                            padding: new() { top = 1f, bottom = 1f }
                        ).AsFlexItem(grow: 1f),
                        //scrollbar
                        new Scrollbar()
                            .AsFlexItem(size: new() { x = 2f })
                            .With(x => Table.Scrollbar = x)
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
            Table.ClearSelection();
            Table.Select(new DropdownOption { key = key });
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

        public bool Interactable {
            get => _interactable;
            set {
                _interactable = value;
                _canvasGroup.alpha = value ? 1f : 0.25f;
                _button.Interactable = value;
            }
        }

        private float _skew;
        private bool _interactable = true;

        #endregion

        #region Construct

        private Table<DropdownOption, OptionsModal.DropdownCellWrapper> Table => _modal.Table;

        private OptionsModal _modal = null!;
        private ImageButton _button = null!;
        private TCell _previewCell = default!;
        private CanvasGroup _canvasGroup = null!;

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
                        .WithAnchor(() => ContentTransform, RelativePlacement.Center, unbindOnceOpened: false)
                        .With(x => x.Setup(_previewCell))
                        .Bind(ref _modal)
                }
            }.WithNativeComponent(out _canvasGroup).AsFlexGroup(
                padding: new() { left = 2f, right = 2f }
            ).WithModal(_modal).Bind(ref _button).Use();
        }

        protected override void OnInitialize() {
            this.AsFlexItem(size: new() { x = 36f, y = 6f });
            Skew = UIStyle.Skew;
            _items.ItemAddedEvent += HandleItemAdded;
            _items.ItemRemovedEvent += HandleItemRemoved;
            _items.AllItemsRemovedEvent += HandleAllItemsRemoved;
            Table.WithListener(
                x => x.SelectedIndexes,
                x => {
                    if (x.Count == 0) return;
                    var index = x.First();
                    var item = Table.FilteredItems[index];
                    SelectedKey = item.key;
                }
            );
        }

        #endregion

        #region Callbacks

        private void HandleItemAdded(TKey key, TParam param) {
            Table.Items.Add(
                new() {
                    key = key,
                    param = param
                }
            );
            Table.Refresh(false);
            RefreshSelection();
            NotifyPropertyChanged(nameof(Items));
        }

        private void HandleItemRemoved(TKey key) {
            Table.Items.Remove(new() { key = key });
            Table.Refresh();
            RefreshSelection();
            NotifyPropertyChanged(nameof(Items));
        }

        private void HandleAllItemsRemoved() {
            Table.Items.Clear();
            Table.Refresh();
            RefreshSelection();
            NotifyPropertyChanged(nameof(Items));
        }

        #endregion
    }
}