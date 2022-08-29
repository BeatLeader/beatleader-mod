using System;
using BeatLeader.API.Methods;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class VotingButton : ReeUIComponentV2 {
        #region Init / Dispose

        protected override void OnInitialize() {
            SetMaterial();

            VoteRequest.AddStateListener(OnVoteRequestStateChanged);
            VoteStatusRequest.AddStateListener(OnVoteStatusRequestStateChanged);
        }

        protected override void OnDispose() {
            VoteRequest.RemoveStateListener(OnVoteRequestStateChanged);
            VoteStatusRequest.RemoveStateListener(OnVoteStatusRequestStateChanged);
        }

        #endregion

        #region ColorScheme

        private static readonly int SpinnerValuePropertyId = Shader.PropertyToID("_SpinnerValue");
        private static readonly int GradientValuePropertyId = Shader.PropertyToID("_GradientValue");
        private static readonly int StatePropertyId = Shader.PropertyToID("_State");
        private static readonly int TintPropertyId = Shader.PropertyToID("_Tint");

        private static readonly ColorScheme LoadingColorScheme = new(1, 0, 0, new Color(0.2f, 0.2f, 0.2f, 0.0f));
        private static readonly ColorScheme LockedColorScheme = new(0, 0, 0, new Color(0.2f, 0.2f, 0.2f, 0.0f));
        private static readonly ColorScheme UnlockedColorScheme = new(0, 1, 1, new Color(1f, 1f, 1f, 0.7f));
        private static readonly ColorScheme DoneColorScheme = new(0, 0, 2, new Color(0.2f, 0.8f, 0.2f, 0.4f));

        private static void ApplyColorScheme(State state, Material material) {
            switch (state) {
                case State.Loading:
                    LoadingColorScheme.Apply(material);
                    break;
                case State.Locked:
                    LockedColorScheme.Apply(material);
                    break;
                case State.Unlocked:
                    UnlockedColorScheme.Apply(material);
                    break;
                case State.Done:
                    DoneColorScheme.Apply(material);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private readonly struct ColorScheme {
            private readonly float _spinnerValue;
            private readonly float _gradientValue;
            private readonly float _state;
            private readonly Color _tint;

            public ColorScheme(float spinnerValue, float gradientValue, float state, Color tint) {
                _spinnerValue = spinnerValue;
                _gradientValue = gradientValue;
                _state = state;
                _tint = tint;
            }

            public void Apply(Material material) {
                material.SetFloat(SpinnerValuePropertyId, _spinnerValue);
                material.SetFloat(GradientValuePropertyId, _gradientValue);
                material.SetFloat(StatePropertyId, _state);
                material.SetColor(TintPropertyId, _tint);
            }
        }

        #endregion

        #region State

        private bool _loadingStatus;
        private bool _sendingVote;
        private VoteStatus _voteStatus;
        private State _state;

        private void SetState(State state) {
            if (_state.Equals(state)) return;
            _state = state;
            ApplyColorScheme(state, _material);
        }

        private void UpdateState() {
            if (_sendingVote || _loadingStatus) {
                SetState(State.Loading);
                return;
            }

            switch (_voteStatus) {
                case VoteStatus.CanVote:
                    SetState(State.Unlocked);
                    break;
                case VoteStatus.Voted:
                    SetState(State.Done);
                    break;
                case VoteStatus.CantVote:
                default:
                    SetState(State.Locked);
                    break;
            }
        }

        private enum State {
            Loading,
            Locked,
            Unlocked,
            Done
        }

        #endregion

        #region Events

        private void OnVoteStatusRequestStateChanged(API.RequestState state, VoteStatus result, string failReason) {
            _loadingStatus = state is API.RequestState.Started;
            if (state is API.RequestState.Finished) _voteStatus = result;
            UpdateState();
        }

        private void OnVoteRequestStateChanged(API.RequestState state, VoteStatus result, string failReason) {
            _sendingVote = state is API.RequestState.Started;
            if (state is API.RequestState.Finished) _voteStatus = result;
            UpdateState();
        }

        [UIAction("voting-menu-on-click"), UsedImplicitly]
        private void VotingMenuOnClick() {
            if (_state != State.Unlocked) return;
            LeaderboardEvents.NotifyVotingWasPressed();
        }

        #endregion

        #region ImageComponent

        private static Color IdleColor => new Color(0, 0, 0, 1);
        private static Color HoverColor => new Color(1, 0, 0, 1);

        [UIComponent("image-component"), UsedImplicitly]
        private ClickableImage _imageComponent;

        private Material _material;

        private void SetMaterial() {
            _material = BundleLoader.VotingButtonMaterial;
            _imageComponent.material = _material;
            _imageComponent.DefaultColor = IdleColor;
            _imageComponent.HighlightColor = HoverColor;
        }

        #endregion
    }
}