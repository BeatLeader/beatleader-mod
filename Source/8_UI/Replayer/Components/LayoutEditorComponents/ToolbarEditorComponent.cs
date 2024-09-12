using BeatLeader.Components;
using BeatLeader.Models;
using Reactive;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class ToolbarEditorComponent : LayoutEditorComponent {
        #region Setup

        public void Setup(
            IReplayPauseController pauseController,
            IReplayFinishController finishController,
            IReplayTimeController timeController,
            IVirtualPlayersManager playersManager,
            ICameraController cameraController,
            IVirtualPlayerBodySpawner bodySpawner,
            ReplayLaunchData launchData,
            ILayoutEditor? layoutEditor,
            IReplayWatermark watermark
        ) {
            _toolbar.Setup(
                pauseController,
                finishController,
                timeController,
                playersManager,
                cameraController,
                bodySpawner,
                launchData,
                layoutEditor,
                watermark,
                true
            );
        }

        #endregion

        #region LayoutEditorComponent

        public override string ComponentName => "Toolbar";
        protected override Vector2 MinSize => new(90, 66);

        private ToolbarWithSettings _toolbar = null!;

        protected override void ConstructInternal(Transform parent) {
            new ToolbarWithSettings().WithRectExpand().Bind(ref _toolbar).Use(parent);
        }

        #endregion
    }
}