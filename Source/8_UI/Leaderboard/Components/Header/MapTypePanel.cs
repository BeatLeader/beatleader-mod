using BeatLeader.DataManager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class MapTypePanel : ReeUIComponentV2 {

        [UIComponent("background"), UsedImplicitly]
        private ImageView _background = null!;

        [UIComponent("icon"), UsedImplicitly]
        private ImageView _icon = null!;

        private string _hoverHint = "";

        [UIValue("hover-hint"), UsedImplicitly]
        public string HoverHint {
            get => _hoverHint;
            set {
                if (_hoverHint.Equals(value)) return;
                _hoverHint = value;
                NotifyPropertyChanged();
            }
        }

        protected override void OnInitialize() {
            _background.raycastTarget = true;
        }

        private MapsTypeDescription? _mapType = null;

        public void SetActive(bool value) {
            _background.gameObject.SetActive(value);
        }

        public void SetValues(MapsTypeDescription mapType) {
            _mapType = mapType;
            if (_mapType != null) {
                _icon.sprite = _mapType.Sprite;
                HoverHint = $"{_mapType.Name} | {_mapType.Description}";
            }
        }
    }
}