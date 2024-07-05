using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class SettingsAvatarView : ReactiveComponent {
        #region Setup

        private IVirtualPlayerBodySpawner? _bodySpawner;
        private BodySettings _bodySettings = new();

        public void Setup(
            IVirtualPlayerBodySpawner bodySpawner,
            BodySettings? bodySettings = null
        ) {
            _bodySpawner = bodySpawner;
            _bodySettings = bodySettings ?? _bodySettings;
            ReloadBodyModels();
        }

        #endregion

        #region Models

        private IVirtualPlayerBodyModel? _selectedModel;

        private void SelectModel(IVirtualPlayerBodyModel model) {
            _selectedModel = model;
            ReloadConfigGroup(model, _bodySpawner!.BodyConfigs[model]);
            ReloadCategories();
        }

        private void ReloadBodyModels() {
            _modelsDropdown.Items.Clear();
            foreach (var model in _bodySpawner!.BodyModels) {
                _modelsDropdown.Items.Add(model, model.Name);
            }
            _modelsDropdown.Interactable = _modelsDropdown.Items.Count > 1;
            _selectedModel = _bodySpawner!.BodyModels.FirstOrDefault();
            if (_selectedModel != null) _modelsDropdown.Select(_selectedModel);
        }

        #endregion

        #region Config

        private record PartConfigWithModel(
            IVirtualPlayerBodyPartModel PartModel,
            IVirtualPlayerBodyPartConfig PartConfig
        );

        private record PartConfigsGroup(
            string? GroupName,
            IEnumerable<PartConfigWithModel> Configs
        );

        private readonly Dictionary<IVirtualPlayerBodyModel, IEnumerable<PartConfigsGroup>> _groupedModelConfigs = new();

        private void ReloadConfigGroup(IVirtualPlayerBodyModel model, IVirtualPlayerBodyConfig config) {
            if (_groupedModelConfigs.TryGetValue(model, out var groupedConfigs)) return;
            //creating if needed
            var modelsDict = model.Parts.ToDictionary(static x => x.Id);
            groupedConfigs = config.BodyParts
                .Select(x => new PartConfigWithModel(modelsDict[x.Key], x.Value))
                .GroupBy(static x => x.PartModel.Category ?? "Other")
                .Select(static x => new PartConfigsGroup(x.Key, x))
                .ToArray();
            _groupedModelConfigs[model] = groupedConfigs;
        }

        #endregion

        #region PartControl

        private class PartControl : ReactiveComponent {
            #region Setup

            private PartConfigWithModel? _config;
            private bool _ignoreUpdates;

            public void Setup(PartConfigWithModel config) {
                if (_config != null) {
                    _config.PartConfig.ConfigUpdatedEvent -= HandleConfigUpdated;
                }
                _config = config;
                _config.PartConfig.ConfigUpdatedEvent += HandleConfigUpdated;
                //
                _namedRail.Label.Text = config.PartModel.Name;
                HandleConfigUpdated();
            }

            protected override void OnDestroy() {
                if (_config != null) {
                    _config.PartConfig.ConfigUpdatedEvent -= HandleConfigUpdated;
                }
            }

            #endregion

            #region Construct

            private Toggle _toggle = null!;
            private NamedRail _namedRail = null!;

            protected override GameObject Construct() {
                return new Toggle()
                    .WithListener(
                        x => x.Active,
                        HandleToggleStateChanged
                    )
                    .Bind(ref _toggle)
                    .InNamedRail("")
                    .Bind(ref _namedRail)
                    .Use();
            }

            #endregion

            #region Callbacks

            private void HandleConfigUpdated() {
                if (_ignoreUpdates) return;
                _toggle.SetActive(_config!.PartConfig.Active, false, true);
                _toggle.Interactable = !_config.PartConfig.ControlledByMask;
            }

            private void HandleToggleStateChanged(bool state) {
                if (_config == null) return;
                _ignoreUpdates = true;
                _config.PartConfig.PotentiallyActive = state;
                _ignoreUpdates = false;
            }

            #endregion
        }

        private readonly ReactivePool<PartControl> _controlsPool = new() { DetachOnDespawn = false };

        #endregion

        #region PartCategory

        private class PartCategory : ReactiveComponent {
            #region Setup

            public void Setup(ReactivePool<PartControl> pool, PartConfigsGroup group) {
                _nameLabel.Text = group.GroupName ?? "Uncategorized";
                _controlsContainer.Children.Clear();
                foreach (var model in group.Configs) {
                    var control = pool.Spawn();
                    control.Setup(model);
                    control.AsFlexItem();
                    _controlsContainer.Children.Add(control);
                }
            }

            #endregion

            #region Construct

            private Label _nameLabel = null!;
            private Dummy _controlsContainer = null!;

            protected override GameObject Construct() {
                return new Image {
                    Children = {
                        //header
                        new Image {
                            Sprite = BundleLoader.Sprites.backgroundTop,
                            PixelsPerUnit = 10f,
                            Color = new(0.08f, 0.08f, 0.08f, 1f),
                            Children = {
                                new Label {
                                    FontStyle = FontStyles.Bold,
                                    Alignment = TextAlignmentOptions.Center
                                }.AsFlexItem(size: new() { y = "auto" }).Bind(ref _nameLabel),
                            }
                        }.AsFlexGroup().AsFlexItem(),
                        //
                        new Dummy()
                            .AsFlexGroup(
                                direction: FlexDirection.Column,
                                padding: 1f,
                                gap: 1f
                            )
                            .AsFlexItem()
                            .Bind(ref _controlsContainer)
                    }
                }.AsFlexGroup(
                    direction: FlexDirection.Column
                ).AsBackground(
                    color: new(0.1f, 0.1f, 0.1f, 1f)
                ).Use();
            }

            #endregion
        }

        private readonly ReactivePool<PartCategory> _categoriesPool = new() { DetachOnDespawn = false };

        private void ReloadCategories() {
            _categoriesPool.DespawnAll();
            _controlsPool.DespawnAll();
            _scrollContainer.Children.Clear();
            var configGroups = _groupedModelConfigs[_selectedModel!];
            foreach (var config in configGroups) {
                var category = _categoriesPool.Spawn();
                category.Setup(_controlsPool, config);
                category.AsFlexItem();
                _scrollContainer.Children.Add(category);
            }
        }

        #endregion

        #region Construct

        private TextDropdown<IVirtualPlayerBodyModel> _modelsDropdown = null!;
        private Dummy _scrollContainer = null!;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new TextDropdown<IVirtualPlayerBodyModel> {
                            Skew = 0f
                        }
                        .Bind(ref _modelsDropdown)
                        .WithListener(
                            x => x.SelectedKey,
                            HandleModelSelected
                        )
                        .InNamedRail("Model")
                        .AsFlexItem(),
                    //
                    new Dummy {
                        Children = {
                            new ScrollArea {
                                ScrollContent = new Dummy()
                                    .AsRootFlexGroup(direction: FlexDirection.Column, gap: 1f)
                                    .AsFlexItem(size: new() { y = "auto" })
                                    .WithRectExpand()
                                    .Bind(ref _scrollContainer)
                            }.AsFlexItem(grow: 1f).Export(out var scrollArea),
                            //
                            new Scrollbar().With(x => scrollArea.Scrollbar = x).AsFlexItem()
                        }
                    }.AsFlexGroup(gap: 1f).AsFlexItem(grow: 1f)
                }
            }.AsFlexGroup(direction: FlexDirection.Column, gap: 2f).Use();
        }

        #endregion

        #region Callbacks

        private void HandleModelSelected(IVirtualPlayerBodyModel model) {
            SelectModel(model);
        }

        #endregion
    }
}