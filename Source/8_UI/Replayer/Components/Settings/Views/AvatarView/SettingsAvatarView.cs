using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
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

        private readonly Dictionary<IVirtualPlayerBodyModel, IEnumerable<AvatarPartConfigsGroup>> _groupedModelConfigs = new();

        private void ReloadConfigGroup(IVirtualPlayerBodyModel model, IVirtualPlayerBodyConfig config) {
            if (_groupedModelConfigs.TryGetValue(model, out var groupedConfigs)) return;
            //creating if needed
            var modelsDict = model.Parts.ToDictionary(static x => x.Id);
            groupedConfigs = config.BodyParts
                .Select(x => new AvatarPartConfigWithModel(modelsDict[x.Key], x.Value))
                .GroupBy(static x => x.PartModel.Category ?? "Other")
                .Select(static x => new AvatarPartConfigsGroup(x.Key, x))
                .ToArray();
            _groupedModelConfigs[model] = groupedConfigs;
        }

        #endregion
        
        #region Parts

        private readonly ReactivePool<AvatarPartCategory> _categoriesPool = new() { DetachOnDespawn = false };
        private readonly ReactivePool<AvatarPartControl> _controlsPool = new() { DetachOnDespawn = false };

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