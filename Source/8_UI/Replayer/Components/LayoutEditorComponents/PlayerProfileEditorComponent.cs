using BeatLeader.Components;
using BeatLeader.Models;
using Reactive;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class PlayerProfileEditorComponent : LayoutEditorComponent {
        #region Setup

        public void Setup(IPlayer? player) {
            _miniProfile.SetPlayer(player);
        }

        #endregion

        #region LayoutEditorComponent

        protected override Vector2 MinSize { get; } = new(60, 24);
        protected override Vector2 MaxSize { get; } = new(int.MaxValue, 24);
        public override string ComponentName => "Player Profile";

        private QuickMiniProfile _miniProfile = null!;

        protected override void ConstructInternal(Transform parent) {
            _miniProfile = new QuickMiniProfile {
                UseAlternativeBlur = true
            };
            _miniProfile.WithRectExpand().Use(parent);
        }

        #endregion
    }
}