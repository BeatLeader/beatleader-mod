using BeatLeader.API.Methods;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.UI.MainMenu {
    internal class MapNewsPanel : ReeUIComponentV2 {
        #region UI Components

        [UIValue("header"), UsedImplicitly]
        private NewsHeader _header = null!;

        [UIValue("map1"), UsedImplicitly]
        private MapPreviewPanel _map1Preview = null!;

        [UIValue("map2"), UsedImplicitly]
        private MapPreviewPanel _map2Preview = null!;

        [UIValue("map3"), UsedImplicitly]
        private MapPreviewPanel _map3Preview = null!;

        private void Awake() {
            _header = Instantiate<NewsHeader>(transform);
            _map1Preview = Instantiate<MapPreviewPanel>(transform);
            _map2Preview = Instantiate<MapPreviewPanel>(transform);
            _map3Preview = Instantiate<MapPreviewPanel>(transform);
        }

        #endregion

        #region Setup

        private static readonly MapData loadingMapData = new() {
            song = new() {
                name = "Loading...",
                author = "..."
            }
        };
        
        private static readonly MapData failedMapData = new() {
            song = new() {
                name = "Error",
                author = ""
            }
        };
        
        public void Reload() {
            TrendingMapsRequest.SendRequest();
        }

        protected override void OnInitialize() {
            _header.Setup("Trending Maps");
            
            TrendingMapsRequest.AddStateListener(OnRequestStateChanged);
        }

        protected override void OnDispose() {
            TrendingMapsRequest.RemoveStateListener(OnRequestStateChanged);
        }

        #endregion

        #region Request

        private void OnRequestStateChanged(API.RequestState state, Paged<MapData> result, string failReason) {
            switch (state) {
                case API.RequestState.Started:
                    _map1Preview.Setup(loadingMapData);
                    _map2Preview.Setup(loadingMapData);
                    _map3Preview.Setup(loadingMapData);
                    break;
                case API.RequestState.Failed:
                    _map1Preview.Setup(failedMapData);
                    _map2Preview.Setup(failedMapData);
                    _map3Preview.Setup(failedMapData);
                    break;
                case API.RequestState.Finished: {
                    _map1Preview.Setup(result.data[0]);
                    _map2Preview.Setup(result.data[1]);
                    _map3Preview.Setup(result.data[2]);
                    break;
                }
            }
        }

        #endregion
    }
}