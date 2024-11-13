using System.Linq;
using BeatLeader.API.Methods;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class TextNewsPanel : ReeUIComponentV2 {
        #region UI Components

        [UIValue("post"), UsedImplicitly]
        private TextNewsPostPanel _post = null!;
        
        [UIObject("empty-text"), UsedImplicitly]
        private GameObject _emptyText = null!;

        private void Awake() {
            _post = Instantiate<TextNewsPostPanel>(transform);
        }

        #endregion

        #region Setup

        private static readonly NewsPost loadingPost = new() {
            owner = "Loading...",
            body = "Loading..."
        };

        private static readonly NewsPost failedToLoadPost = new() {
            owner = "Error",
            body = "The post has failed to load"
        };

        public void Reload() {
            NewsRequest.SendRequest();
        }

        protected override void OnInitialize() {
            NewsRequest.AddStateListener(OnRequestStateChanged);
        }

        protected override void OnDispose() {
            NewsRequest.RemoveStateListener(OnRequestStateChanged);
        }

        #endregion

        #region Request

        private void SetEmptyActive(bool active) {
            _emptyText.SetActive(active);
            _post.GetRootTransform().gameObject.SetActive(!active);
        }
        
        private void OnRequestStateChanged(API.RequestState state, Paged<NewsPost> result, string failReason) {
            switch (state) {
                case API.RequestState.Started:
                    _post.Setup(loadingPost);
                    SetEmptyActive(false);
                    break;
                case API.RequestState.Failed:
                    _post.Setup(failedToLoadPost);
                    SetEmptyActive(false);
                    break;
                case API.RequestState.Finished: {
                    var post = result.data.FirstOrDefault();
                    var hasPost = post != null; 
                    SetEmptyActive(!hasPost);
                    if (hasPost) _post.Setup(post!);
                    break;
                }
            }
        }

        #endregion
    }
}