using System.Collections.Generic;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ContextsModal : AbstractReeModal<object> {
        #region Components

        [UIComponent("container"), UsedImplicitly]
        private Transform _container;

        protected override void OnInitialize() {
            base.OnInitialize();
            InitializeOptions();
            PluginConfig.ScoresContextListChangedEvent += OnScoresContextListWasChanged;
        }

        protected override void OnDispose() {
            base.OnDispose();
            PluginConfig.ScoresContextListChangedEvent -= OnScoresContextListWasChanged;
        }

        protected void OnScoresContextListWasChanged() {
            InitializeOptions();
        }

        #endregion

        #region Options

        private readonly List<ContextsModalOption> _options = new(4);

        private void InitializeOptions() {
            DespawnOptions();

            foreach (var scoresContext in ScoresContexts.AllContexts) {
                var option = Instantiate<ContextsModalOption>(_container);
                option.ManualInit(_container);
                option.SetContext(scoresContext);
                option.OnClick += () => {
                    PluginConfig.ScoresContext = scoresContext.Id;
                    Close();
                };
                _options.Add(option);
            }
        }

        private void DespawnOptions() {
            foreach (var option in _options) {
                Destroy(option.gameObject);
            }

            _options.Clear();
        }

        #endregion
    }
}