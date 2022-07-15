using System;
using BeatLeader.DataManager;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class NotificationIcon : ReeUIComponentV2 {
        #region Initialize/Dispose

        protected override void OnInitialize() {
            SetupImage();

            ModVersionChecker.IsUpToDateChangedEvent += OnModIsUpToDateChanged;
            RankedPlaylistManager.IsUpToDateChangedEvent += OnPlaylistIsUpToDateChanged;
            OculusMigrationManager.IsMigrationRequiredChangedEvent += OnOculusMigrationRequiredChanged;

            UpdateState();
        }

        protected override void OnDispose() {
            ModVersionChecker.IsUpToDateChangedEvent -= OnModIsUpToDateChanged;
            RankedPlaylistManager.IsUpToDateChangedEvent -= OnPlaylistIsUpToDateChanged;
            OculusMigrationManager.IsMigrationRequiredChangedEvent -= OnOculusMigrationRequiredChanged;
        }

        #endregion

        #region State

        private void OnOculusMigrationRequiredChanged(bool value) {
            UpdateState();
        }

        private void OnModIsUpToDateChanged(bool value) {
            UpdateState();
        }

        private void OnPlaylistIsUpToDateChanged(bool value) {
            UpdateState();
        }

        private void UpdateState() {
            if (!ModVersionChecker.IsUpToDate) {
                SetState(State.Critical);
                return;
            }

            if (!RankedPlaylistManager.IsUpToDate || OculusMigrationManager.IsMigrationRequired) {
                SetState(State.Warning);
                return;
            }

            SetState(State.Normal);
        }

        private void SetState(State state) {
            _image.color = state switch {
                State.Normal => NormalColor,
                State.Warning => WarningColor,
                State.Critical => CriticalColor,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        private enum State {
            Normal,
            Warning,
            Critical
        }

        #endregion

        #region ImageComponent

        private static Color NormalColor => new Color(0.0f, 0.0f, 0.0f, 0.0f);
        private static Color WarningColor => new Color(1.0f, 1.0f, 0.3f, 1.0f);
        private static Color CriticalColor => new Color(1.0f, 0.3f, 0.3f, 1.0f);

        [UIComponent("image"), UsedImplicitly]
        private ImageView _image;

        private void SetupImage() {
            _image.transform.localPosition = new Vector3(4f, 4f);
            _image.raycastTarget = false;
        }

        #endregion
    }
}