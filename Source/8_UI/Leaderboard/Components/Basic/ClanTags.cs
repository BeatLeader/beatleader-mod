using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ClanTags : ReeUIComponentV2 {
        #region Components

        private const int TotalCount = 3;

        [UIValue("tags"), UsedImplicitly]
        private ClanTag[] _tags = new ClanTag[TotalCount];

        private void Awake() {
            for (var i = 0; i < TotalCount; i++) {
                _tags[i] = Instantiate<ClanTag>(transform);
            }
        }

        #endregion

        #region SetValues

        private int _count;

        public void SetClans(Clan[] clans) {
            _count = Mathf.Min(clans.Length, TotalCount);

            for (var i = 0; i < TotalCount; i++) {
                if (i < clans.Length) {
                    _tags[i].SetValue(clans[i]);
                } else {
                    _tags[i].Clear();
                }
            }

            _container.gameObject.SetActive(_count > 0);
        }

        public void SetAlpha(float value) {
            for (var i = 0; i < TotalCount; i++) {
                _tags[i].SetAlpha(value);
            }
        }

        #endregion

        #region CalculatePreferredWidth

        private const float Spacing = 0.3f;

        public float CalculatePreferredWidth() {
            var result = 0.0f;

            for (var i = 0; i < _count; i++) {
                result += _tags[i].CalculatePreferredWidth();
            }

            if (_count > 1) result += Spacing * (_count - 1);

            return result;
        }

        #endregion

        #region Container

        [UIComponent("container"), UsedImplicitly]
        private RectTransform _container;

        #endregion
    }
}