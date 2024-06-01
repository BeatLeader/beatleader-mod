using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class TagSelector : ReactiveComponent {
        #region Tags

        public IReadOnlyCollection<IReplayTag> SelectedTags => _selectedTagsList.Items;

        public event Action<IReplayTag>? SelectedTagAddedEvent;
        public event Action<IReplayTag>? SelectedTagRemovedEvent;
        
        public void SelectFromMetadata(IReplayMetadata metadata) {
            _selectedTagsList.Items.Clear();
            _selectedTagsList.Items.AddRange(metadata.Tags);
            _selectedTagsList.Refresh();
            ReloadAllTags(metadata.Tags);
        }
        
        public void ClearSelectedTags() {
            _selectedTagsList.Items.Clear();
            _selectedTagsList.Refresh();
            ReloadAllTags();
        }

        private void ReloadAllTags(IEnumerable<IReplayTag>? except = null) {
            var tags = _tagManager!.Tags;
            _allTagsList.Items.Clear();
            _allTagsList.Items.AddRange(except != null ? tags.Except(except) : tags);
            _allTagsList.Refresh();
        }

        private void AddSelectedTag(IReplayTag tag) {
            SelectedTagAddedEvent?.Invoke(tag);
            NotifyPropertyChanged(nameof(SelectedTags));
        }

        private void RemoveSelectedTag(IReplayTag tag) {
            SelectedTagRemovedEvent?.Invoke(tag);
            NotifyPropertyChanged(nameof(SelectedTags));
        }

        #endregion

        #region TagsList

        private class TagsListCell : ReactiveTableCell<IReplayTag> {
            #region Construct

            private Label _nameLabel = null!;
            private ImageButton _button = null!;

            protected override GameObject Construct() {
                return new ImageButton {
                    Image = {
                        Sprite = BundleLoader.Sprites.rectangle,
                        Material = GameResources.UINoGlowMaterial
                    },
                    GrowOnHover = false,
                    HoverLerpMul = float.MaxValue,
                    ColorizeOnHover = false,
                    Children = {
                        new Label {
                            Alignment = TextAlignmentOptions.Midline
                        }.AsFlexItem(grow: 1f).Bind(ref _nameLabel)
                    }
                }.WithRectSize(6f, 0f).AsFlexGroup(padding: 1f).Bind(ref _button).Use();
            }

            #endregion

            #region Cell

            protected override void Init(IReplayTag item) {
                _nameLabel.Text = item.Name;
                OnCellStateChange(false, false);
            }

            public override void OnCellStateChange(bool selected, bool highlighted) {
                _nameLabel.Color = highlighted || selected ? UIStyle.TextColor : UIStyle.SecondaryTextColor;
                _button.Color = Item!.Color.ColorWithAlpha(highlighted || selected ? 0.5f : 0.3f);
            }

            #endregion
        }

        private class TagsList : ReactiveListComponentBase<IReplayTag, TagsListCell> {
            protected override float CellSize => 6f;
        }

        #endregion

        #region Setup

        private IReplayTagManager? _tagManager;

        public void Setup(IReplayTagManager? tagManager) {
            if (_tagManager == tagManager) return;
            if (_tagManager != null) {
                _tagManager.TagCreatedEvent -= HandleTagCreated;
                _tagManager.TagDeletedEvent -= HandleTagDeleted;
            }
            _tagManager = tagManager;
            _tagCreationDialog.Setup(tagManager);
            _allTagsList.Items.Clear();
            _selectedTagsList.Items.Clear();
            _selectedTagsList.Refresh();
            if (_tagManager != null) {
                _allTagsList.Items.AddRange(_tagManager.Tags);
                _allTagsList.Refresh();
                _tagManager.TagCreatedEvent += HandleTagCreated;
                _tagManager.TagDeletedEvent += HandleTagDeleted;
            }
        }

        private void HandleTagCreated(IReplayTag tag) {
            SynchronizationContext.Current.Send(
                _ => {
                    _allTagsList.Items.Add(tag);
                    _allTagsList.Refresh();
                },
                null
            );
        }

        private void HandleTagDeleted(IReplayTag tag) {
            SynchronizationContext.Current.Send(
                _ => {
                    _allTagsList.Items.Remove(tag);
                    _selectedTagsList.Items.Remove(tag);
                    _allTagsList.Refresh();
                    _selectedTagsList.Refresh();
                },
                null
            );
        }

        #endregion

        #region Construct

        private TagCreationDialog _tagCreationDialog = null!;
        private TagsList _allTagsList = null!;
        private TagsList _selectedTagsList = null!;

        protected override GameObject Construct() {
            static ReactiveComponentBase CreateTagsList(
                string text,
                Func<Button> actionButton,
                ref TagsList list
            ) {
                return new Image {
                    Children = {
                        //top panel
                        new DialogHeader {
                            Text = text
                        }.AsFlexItem(basis: 6f),
                        //list
                        new TagsList()
                            .WithListener(
                                x => x.SelectedItemsIndexes,
                                x => actionButton().Interactable = x.Count > 0
                            )
                            .AsFlexItem(grow: 1f)
                            .Bind(ref list)
                    }
                }.AsBlurBackground().AsFlexGroup(direction: FlexDirection.Column);
            }

            static Button CreateButton(Sprite sprite) {
                return new BsButton {
                    Children = {
                        new Image {
                            Sprite = sprite,
                            PreserveAspect = true
                        }.AsFlexItem(grow: 1f)
                    }
                }.AsFlexGroup(
                    padding: 1f
                ).AsFlexItem(
                    basis: 4f
                );
            }

            static Button CreateActionButton(
                Sprite sprite,
                Func<TagsList> sourceList,
                Func<TagsList> targetList,
                Action<IReplayTag> processCallback
            ) {
                return CreateButton(sprite)
                    .WithClickListener(
                        () => {
                            var list = sourceList();
                            var tList = targetList();
                            foreach (var item in list.SelectedItems.ToArray()) {
                                tList.Items.Add(item);
                                list.Items.Remove(item);
                                processCallback(item);
                            }
                            list.ClearSelection();
                            list.Refresh();
                            tList.Refresh();
                        }
                    );
            }

            Scrollbar selectedListScrollbar = null!;
            Button addButton = null!;
            Button removeButton = null!;
            return new Dummy {
                Children = {
                    //all tags list
                    CreateTagsList(
                        "All Tags",
                        () => addButton,
                        ref _allTagsList
                    ).AsFlexItem(basis: 30f),
                    //all tags scrollbar
                    new Scrollbar()
                        .AsFlexItem(basis: 2f)
                        .With(x => _allTagsList.Scrollbar = x),

                    //add & remove buttons
                    new Image {
                        Children = {
                            //add button
                            CreateActionButton(
                                BundleLoader.RightArrowIcon,
                                () => _allTagsList,
                                () => _selectedTagsList,
                                AddSelectedTag
                            ).Bind(ref addButton),
                            //remove button
                            CreateActionButton(
                                BundleLoader.LeftArrowIcon,
                                () => _selectedTagsList,
                                () => _allTagsList,
                                RemoveSelectedTag
                            ).Bind(ref removeButton),

                            //creation modal
                            new TagCreationDialog().Bind(ref _tagCreationDialog),
                            //creation button
                            CreateButton(BundleLoader.Sprites.plusIcon)
                                .WithModal(_tagCreationDialog, offset: new(0f, 10f)),
                        }
                    }.AsFlexGroup(
                        direction: FlexDirection.Column,
                        gap: 2f,
                        padding: 1f
                    ).AsFlexItem(
                        basis: 8f,
                        alignSelf: Align.Center
                    ).AsBlurBackground(),

                    //added tags scrollbar
                    new Scrollbar()
                        .AsFlexItem(basis: 2f)
                        .Bind(ref selectedListScrollbar),
                    //added tags list
                    CreateTagsList(
                        "Added Tags",
                        () => removeButton,
                        ref _selectedTagsList
                    ).With(
                        _ => _selectedTagsList.Scrollbar = selectedListScrollbar
                    ).AsFlexItem(basis: 30f),
                }
            }.With(
                _ => {
                    _allTagsList.Refresh();
                    _selectedTagsList.Refresh();
                }
            ).AsFlexGroup(gap: 1f).WithRectSize(40f, 61f).Use();
        }

        #endregion
    }
}