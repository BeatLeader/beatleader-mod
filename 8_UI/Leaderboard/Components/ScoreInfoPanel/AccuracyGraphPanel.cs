using System;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScoreInfoPanel.AccuracyGraph.bsml")]
    internal class AccuracyGraphPanel : ReeUIComponent {
        #region Initialize

        private AccuracyGraph _accuracyGraph;    
        private readonly CurvedCanvasSettingsHelper _curvedCanvasSettingsHelper = new();

        protected override void OnInitialize() {
            var go = Object.Instantiate(BundleLoader.AccuracyGraphPrefab, _graphContainer, true);
            _accuracyGraph = go.GetComponent<AccuracyGraph>();
        }

        #endregion

        #region SetScoreStats

        public void SetScoreStats(ScoreStats scoreStats) {
            var tracker = scoreStats.scoreGraphTracker;
            
            var canvasSettings = _curvedCanvasSettingsHelper.GetCurvedCanvasSettings(_accuracyGraph.canvas);
            _accuracyGraph.SetPoints(tracker.graph, canvasSettings.radius);
        }

        #endregion

        #region SetActive

        public void SetActive(bool value) {
            Active = value;
        }

        #endregion

        #region Active

        private bool _active = true;

        [UIValue("active"), UsedImplicitly]
        private bool Active {
            get => _active;
            set {
                if (_active.Equals(value)) return;
                _active = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region GraphContainer

        [UIComponent("graph-container"), UsedImplicitly]
        private RectTransform _graphContainer;

        #endregion
    }
}