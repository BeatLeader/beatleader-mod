using System.Linq;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class TagsStrip : ReactiveComponent {
        #region AllowEdit

        public bool AllowEdit {
            get => _allowEdit;
            set {
                _allowEdit = value;
                _editButtonContainer.Enabled = value;
                _contentContainer.Sprite = value ? BundleLoader.Sprites.backgroundLeft : BundleLoader.Sprites.background;
            }
        }

        private bool _allowEdit = true;

        #endregion

        #region Setup

        private ReplayMetadata? _metadata;

        public void SetMetadata(ReplayMetadata? metadata) {
            var shouldAnimate = false;

            // Compare by values if no obvious signs of change
            if (_metadata != null && _metadata.Tags.Count == metadata?.Tags.Count) {
                using var newNum = metadata.Tags.GetEnumerator();

                foreach (var oldItem in _metadata.Tags) {
                    newNum.MoveNext();

                    if (oldItem != newNum.Current) {
                        shouldAnimate = true;
                        break;
                    }
                }
            } else {
                shouldAnimate = true;
            }

            if (_metadata != null) {
                _metadata.TagAddedEvent -= HandleTagAddedExternal;
                _metadata.TagRemovedEvent -= HandleTagRemovedExternal;
            }
            _metadata = metadata;
            if (_metadata != null) {
                _metadata.TagAddedEvent += HandleTagAddedExternal;
                _metadata.TagRemovedEvent += HandleTagRemovedExternal;
            }

            if (shouldAnimate) {
                StartAnimation();
            }
        }

        #endregion

        #region Animation

        private float _speed = 10f;
        private float _progress = 1f;
        private float _target = 1f;
        private bool _needToSwapProgress;

        private void StartAnimation() {
            _target = 0f;
            _needToSwapProgress = true;
        }

        protected override void OnUpdate() {
            _progress = Mathf.Lerp(_progress, _target, Time.deltaTime * _speed);
            RefreshVisuals();
        }

        private void RefreshVisuals() {
            if (_progress <= 0.5f && _needToSwapProgress) {
                _target = 1f;
                _needToSwapProgress = false;
                ReloadTags();
                RefreshTagsLabel();
            }
            var t = Mathf.Abs((_progress - 0.5f) * 2.0f);
            _containerGroup.alpha = t;
            ContentTransform.localScale = new Vector3(1.0f, t, 1.0f);
            _contentContainer.Color = _contentContainer.Color.ColorWithAlpha(t);
        }

        #endregion

        #region Tags

        public int ShownTagsCount = 3;

        private readonly ReactivePool<ReplayTag, TagPanel> _tagsPool = new() { DetachOnDespawn = false };

        private void ReloadTags() {
            var index = 0;
            _tagsPool.DespawnAll();
            foreach (var tag in _metadata!.Tags) {
                if (index > ShownTagsCount - 1) break;
                SpawnTag(tag, false);
                index++;
            }
        }

        private void SpawnTag(ReplayTag tag, bool animated) {
            if (_tagsPool.SpawnedComponents.TryGetValue(tag, out var panel)) {
                panel.DisappearAnimationFinishedEvent -= HandleTagDisappearAnimationFinished;
            } else {
                panel = _tagsPool.Spawn(tag);
                panel.FixedHeight = 4f;
            }

            panel.Interactable = false;
            panel.SetTag(tag, animated);

            _tagsContainer.Children.Add(panel);
        }

        private void DespawnTag(ReplayTag tag, bool animated) {
            if (!_tagsPool.SpawnedComponents.TryGetValue(tag, out var panel)) {
                return;
            }

            panel.DisappearAnimationFinishedEvent += HandleTagDisappearAnimationFinished;
            panel.SetTagPresented(false, !animated);
        }

        private void RefreshTagsLabel() {
            var count = _metadata?.Tags.Count;
            var delta = count - ShownTagsCount;
            _tagsContainer.Enabled = count > 0;
            _tagsLabel.Enabled = delta > 0 || count == 0;
            _tagsLabel.Text = count == 0 ? "No tags" : $"And {delta} more";
        }

        #endregion

        #region Construct

        private CanvasGroup _containerGroup = null!;
        private TagSelectorModal _tagSelectorModal = null!;
        private Label _tagsLabel = null!;
        private Layout _tagsContainer = null!;
        private Image _contentContainer = null!;
        private Image _editButtonContainer = null!;

        protected override GameObject Construct() {
            new TagSelectorModal()
                .With(x => x.BuildImmediate())
                .WithAnchor(this, RelativePlacement.BottomCenter)
                .WithOpenListener(HandleTagSelectorOpened)
                .WithCloseListener(HandleTagSelectorClosed)
                .Bind(ref _tagSelectorModal);

            return new Layout {
                Children = {
                    new Background {
                            Sprite = BundleLoader.Sprites.backgroundLeft,
                            Children = {
                                //tags container
                                new Layout()
                                    .AsFlexGroup(
                                        gap: 1f,
                                        alignItems: Align.Center,
                                        justifyContent: Justify.SpaceAround,
                                        padding: new() { left = 1f }
                                    )
                                    .AsFlexItem(flexGrow: 1f)
                                    .Bind(ref _tagsContainer),
                                //tags label
                                new Label {
                                        Color = UIStyle.SecondaryTextColor,
                                        FontSize = 3.5f,
                                        Alignment = TextAlignmentOptions.Capline
                                    }
                                    .AsFlexItem(margin: new() { left = 1f, right = 1f })
                                    .Bind(ref _tagsLabel)
                            }
                        }
                        .AsBlurBackground()
                        .AsFlexGroup(justifyContent: Justify.SpaceAround)
                        .AsFlexItem(flexGrow: 1f)
                        .Bind(ref _contentContainer),
                    //
                    new Background {
                            Sprite = BundleLoader.Sprites.backgroundRight,
                            Children = {
                                new ImageButton {
                                    Image = {
                                        Sprite = BundleLoader.Sprites.editIcon,
                                        Material = BundleLoader.Materials.uiAdditiveGlowMaterial
                                    },
                                    Colors = UIStyle.GlowingButtonColorSet
                                }.WithModal(_tagSelectorModal).AsFlexItem(size: 3f)
                            }
                        }
                        .AsBlurBackground()
                        .AsFlexGroup(padding: 1f)
                        .AsFlexItem()
                        .Bind(ref _editButtonContainer)
                }
            }.WithNativeComponent(out _containerGroup).AsFlexGroup(gap: 0.5f).Use();
        }

        protected override void OnDestroy() {
            if (_metadata != null) {
                _metadata.TagAddedEvent -= HandleTagAddedExternal;
                _metadata.TagRemovedEvent -= HandleTagRemovedExternal;
            }
        }

        #endregion

        #region Callbacks

        private void HandleTagSelectorOpened(IModal modal, bool finished) {
            if (finished) return;
            var tagSelector = _tagSelectorModal.Component;

            if (_metadata == null) {
                throw new UninitializedComponentException();
            }

            tagSelector.SelectTags(_metadata.Tags);
            tagSelector.AsFlexItem(size: new() { x = 50.pt(), y = 36.pt() });
            tagSelector.SelectedTagAddedEvent += HandleSelectedTagAdded;
            tagSelector.SelectedTagRemovedEvent += HandleSelectedTagRemoved;
        }

        private void HandleTagSelectorClosed(IModal modal, bool finished) {
            if (finished) return;
            var tagSelector = _tagSelectorModal.Component;
            tagSelector.SelectedTagAddedEvent -= HandleSelectedTagAdded;
            tagSelector.SelectedTagRemovedEvent -= HandleSelectedTagRemoved;
        }

        private void HandleTagDisappearAnimationFinished(ReplayTag tag) {
            var panel = _tagsPool.SpawnedComponents[tag];
            panel.DisappearAnimationFinishedEvent -= HandleTagDisappearAnimationFinished;
            _tagsPool.Despawn(panel);

            // Spawning another tag if needed
            var spawned = _tagsPool.SpawnedComponents.Count;
            if (_metadata != null && spawned < ShownTagsCount && _metadata.Tags.Count > spawned) {
                tag = _metadata.Tags.First(x => !_tagsPool.SpawnedComponents.ContainsKey(x));
                SpawnTag(tag, true);
            }
        }

        private void HandleSelectedTagAdded(ReplayTag tag) {
            _metadata!.Tags.Add(tag);
        }

        private void HandleSelectedTagRemoved(ReplayTag tag) {
            _metadata!.Tags.Remove(tag);
        }

        private void HandleTagAddedExternal(ReplayTag tag) {
            if (_tagsPool.SpawnedComponents.Count < ShownTagsCount ||
                _tagsPool.SpawnedComponents.ContainsKey(tag)
               ) {
                SpawnTag(tag, true);
            }
            RefreshTagsLabel();
        }

        private void HandleTagRemovedExternal(ReplayTag tag) {
            DespawnTag(tag, true);
            RefreshTagsLabel();
        }

        #endregion
    }
}