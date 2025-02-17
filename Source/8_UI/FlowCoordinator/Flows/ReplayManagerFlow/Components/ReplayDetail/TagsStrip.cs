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

        private IReplayMetadata? _metadata;
        private IReplayTagManager? _tagManager;

        public void SetMetadata(IReplayMetadata metadata) {
            _metadata = metadata;
            StartAnimation();
        }

        public void Setup(IReplayTagManager tagManager, IReactiveComponent? background) {
            _tagManager = tagManager;
            //background?.WithAlphaOnModalOpen(_tagSelectorModal);
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

        private readonly ReactivePool<IReplayTag, TagPanel> _tagsPool = new() { DetachOnDespawn = false };

        private void ReloadTags() {
            var index = 0;
            _tagsPool.DespawnAll();
            foreach (var tag in _metadata!.Tags) {
                if (index > ShownTagsCount - 1) break;
                SpawnTag(tag, false);
                index++;
            }
        }

        private void SpawnTag(IReplayTag tag, bool animated) {
            if (_tagsPool.SpawnedComponents.TryGetValue(tag, out var panel)) {
                panel.StateAnimationFinishedEvent -= HandleTagStateAnimationFinished;
            } else {
                panel = _tagsPool.Spawn(tag);
            }
            panel.AsFlexItem(size: new() { y = 4f });
            panel.Interactable = false;
            panel.SetTag(tag, animated);
            _tagsContainer.Children.Add(panel);
        }

        private void DespawnTag(IReplayTag tag, bool animated) {
            if (!_tagsPool.SpawnedComponents.TryGetValue(tag, out var panel)) return;
            panel.StateAnimationFinishedEvent += HandleTagStateAnimationFinished;
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
        private Dummy _tagsContainer = null!;
        private Image _contentContainer = null!;
        private Image _editButtonContainer = null!;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new TagSelectorModal {
                            BuildImmediate = true
                        }
                        .WithAnchor(this, RelativePlacement.BottomCenter)
                        .WithOpenListener(HandleTagSelectorOpened)
                        .WithCloseListener(HandleTagSelectorClosed)
                        .Bind(ref _tagSelectorModal),
                    //
                    new Image {
                            Sprite = BundleLoader.Sprites.backgroundLeft,
                            Children = {
                                //tags container
                                new Dummy()
                                    .AsFlexGroup(
                                        gap: 1f,
                                        alignItems: Align.Center,
                                        padding: new() { left = 1f }
                                    )
                                    .AsFlexItem(grow: 1f)
                                    .Bind(ref _tagsContainer),
                                //tags label
                                new Label {
                                    Color = UIStyle.SecondaryTextColor,
                                    FontSize = 3.5f,
                                    Alignment = TextAlignmentOptions.Capline
                                }.AsFlexItem(
                                    size: new() { x = "auto" },
                                    margin: new() { left = 1f, right = 1f }
                                ).Bind(ref _tagsLabel)
                            }
                        }
                        .AsBlurBackground()
                        .AsFlexGroup()
                        .AsFlexItem(grow: 1f)
                        .Bind(ref _contentContainer),
                    //
                    new Image {
                            Sprite = BundleLoader.Sprites.backgroundRight,
                            Children = {
                                new ImageButton {
                                    Image = {
                                        Sprite = BundleLoader.Sprites.editIcon,
                                        Material = BundleLoader.Materials.uiAdditiveGlowMaterial
                                    },
                                    Colors = UIStyle.ButtonColorSet
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

        #endregion

        #region Callbacks

        private void HandleTagSelectorOpened(IModal modal, bool finished) {
            if (finished) return;
            var tagSelector = _tagSelectorModal.Component;
            if (_metadata == null || _tagManager == null) {
                throw new UninitializedComponentException();
            }
            tagSelector.Setup(_tagManager);
            tagSelector.SelectTags(_metadata.Tags);
            tagSelector.WithSizeDelta(50f, 36f);
            tagSelector.SelectedTagAddedEvent += HandleSelectedTagAdded;
            tagSelector.SelectedTagRemovedEvent += HandleSelectedTagRemoved;
        }

        private void HandleTagSelectorClosed(IModal modal, bool finished) {
            if (finished) return;
            var tagSelector = _tagSelectorModal.Component;
            tagSelector.SelectedTagAddedEvent -= HandleSelectedTagAdded;
            tagSelector.SelectedTagRemovedEvent -= HandleSelectedTagRemoved;
        }

        private void HandleTagStateAnimationFinished(IReplayTag tag) {
            var panel = _tagsPool.SpawnedComponents[tag];
            panel.StateAnimationFinishedEvent -= HandleTagStateAnimationFinished;
            _tagsPool.Despawn(panel);
            //spawning another tag if needed
            var spawned = _tagsPool.SpawnedComponents.Count;
            if (_metadata != null && spawned < ShownTagsCount && _metadata.Tags.Count > spawned) {
                tag = _metadata.Tags.First(x => !_tagsPool.SpawnedComponents.ContainsKey(x));
                SpawnTag(tag, true);
            }
        }

        private void HandleSelectedTagAdded(IReplayTag tag) {
            _metadata!.Tags.Add(tag);
            if (_tagsPool.SpawnedComponents.Count < ShownTagsCount ||
                _tagsPool.SpawnedComponents.ContainsKey(tag)
            ) {
                SpawnTag(tag, true);
            }
            RefreshTagsLabel();
        }

        private void HandleSelectedTagRemoved(IReplayTag tag) {
            _metadata!.Tags.Remove(tag);
            DespawnTag(tag, true);
            RefreshTagsLabel();
        }

        #endregion
    }
}