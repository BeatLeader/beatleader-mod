using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using UnityEngine;
using UnityEngine.UI;
using Dummy = BeatLeader.UI.Reactive.Components.Dummy;

namespace BeatLeader.UI.Hub {
    internal class FloatingBattleRoyaleReplayBadge : ReactiveComponent {
        #region Setup

        private Transform? _headTransform;

        public async void SetData(IBattleRoyaleReplay replay) {
            await _badge.SetData(replay);
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