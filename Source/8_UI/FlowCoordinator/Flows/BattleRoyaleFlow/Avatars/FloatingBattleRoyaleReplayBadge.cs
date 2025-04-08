using BeatLeader.Utils;
using Reactive;
using Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class FloatingBattleRoyaleReplayBadge : ReactiveComponent {
        #region Setup

        private Transform? _headTransform;

        public void SetData(BattleRoyaleReplay replay) {
            _badge.SetData(replay).RunCatching();
        }

        public void Setup(Transform head) {
            _headTransform = head;
        }

        #endregion

        #region Construct

        private BattleRoyaleReplayBadge _badge = null!;

        protected override GameObject Construct() {
            return new Dummy().With(
                x => new BattleRoyaleReplayBadge {
                    ContentTransform = {
                        localEulerAngles = new(0f, 180f, 0f)
                    }
                }.Bind(ref _badge).Use(x.ContentTransform)
            ).Use();
        }

        protected override void OnInitialize() {
            ReactiveUtils.AddCanvas(this);
            ContentTransform.localScale = Vector3.one * 0.025f;
        }

        protected override void OnUpdate() {
            if (_headTransform == null) return;
            ContentTransform.LookAt(_headTransform);
        }

        #endregion
    }
}