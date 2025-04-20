using BeatLeader.API;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal struct ClanScorePanelContext {
        public BeatmapKey beatmapKey;
        public ClanScore clanScore;
        public Player clanPlayer;
    }

    internal class ClanScorePanel : AbstractReeModal<ClanScorePanelContext> {
        #region Components

        [UIComponent("PaginationPanel"), UsedImplicitly]
        private ImageView _paginationPanel = null!;

        [UIComponent("MiddlePanel"), UsedImplicitly]
        private ImageView _middlePanel = null!;

        [UIComponent("BottomPanel"), UsedImplicitly]
        private ImageView _bottomPanel = null!;

        [UIComponent("TableContainer"), UsedImplicitly]
        private Transform _tableContainer = null!;

        [UIComponent("PageUpButton"), UsedImplicitly]
        private ClickableImage _pageUpButton = null!;

        [UIComponent("AroundMeButton"), UsedImplicitly]
        private ClickableImage _aroundMeButton = null!;

        [UIComponent("PageDownButton"), UsedImplicitly]
        private ClickableImage _pageDownButton = null!;

        [UIComponent("TabSelector"), UsedImplicitly]
        private TextSegmentedControl _tabSelector = null!;

        [UIValue("MiniProfile"), UsedImplicitly]
        private ClanMiniProfile _miniProfile = null!;

        [UIValue("ScoresTable"), UsedImplicitly]
        private ClanScoresTable _scoresTable = null!;

        private void Awake() {
            _miniProfile = Instantiate<ClanMiniProfile>(transform);
            _scoresTable = Instantiate<ClanScoresTable>(transform);
        }

        #endregion

        #region Init / Dispose

        protected override void OnInitialize() {
            base.OnInitialize();

            _pageUpButton.transform.Rotate(0, 0, 180);
            _tableContainer.localScale = new Vector3(0.85f, 0.85f, 0.85f);

            _paginationPanel.raycastTarget = true;
            _middlePanel.raycastTarget = true;
            _bottomPanel.raycastTarget = true;
            _bottomPanel._skew = 0.18f;

            _tabSelector._hideCellBackground = false;
            _tabSelector.SetTexts(new[] { "Scores", "Players" });

            ScoresOfClanRequest.StateChangedEvent += OnScoresRequestStateChanged;

            ClanPlayersRequest.StateChangedEvent += OnScoresRequestStateChanged;
        }

        protected override void OnDispose() {
            ScoresOfClanRequest.StateChangedEvent -= OnScoresRequestStateChanged;

            ClanPlayersRequest.StateChangedEvent -= OnScoresRequestStateChanged;
        }

        private void Update() {
            UpdatePaginationIfDirty();
            UpdateRequestIfDirty();
        }

        #endregion

        #region Events

        private void OnScoresRequestStateChanged(WebRequests.IWebRequest<ScoresTableContent> instance, WebRequests.RequestState state, string? failReason) {
            if (state != WebRequests.RequestState.Finished || instance.Result == null) {
                _hasPreviousPage = false;
                _canSeek = false;
                _hasNextPage = false;
            } else {
                _currentPage = instance.Result.CurrentPage;
                _hasPreviousPage = instance.Result.HasPreviousPage;
                _canSeek = instance.Result.SeekAvailable;
                _hasNextPage = instance.Result.HasNextPage;
            }

            MarkPaginationDirty();
        }

        protected override void OnContextChanged() {
            _miniProfile.SetClan(Context.clanScore.clan);
            _currentPage = 1;
            _paginationType = PaginationType.Page;
            MarkRequestDirty();
        }

        [UIAction("UpOnClick"), UsedImplicitly]
        private void UpOnClick() {
            if (!_hasPreviousPage) return;
            _currentPage -= 1;
            _paginationType = PaginationType.Page;
            MarkRequestDirty();
        }

        [UIAction("AroundMeOnClick"), UsedImplicitly]
        private void AroundMeOnClick() {
            if (!_canSeek) return;
            _paginationType = PaginationType.Seek;
            MarkRequestDirty();
        }

        [UIAction("DownOnClick"), UsedImplicitly]
        private void DownOnClick() {
            if (!_hasNextPage) return;
            _currentPage += 1;
            _paginationType = PaginationType.Page;
            MarkRequestDirty();
        }

        [UIAction("OnSelectCell"), UsedImplicitly]
        private void OnDidSelectCell(SegmentedControl segmentedControl, int i) {
            _currentPage = 1;
            _requestType = i switch {
                0 => RequestType.Scores,
                _ => RequestType.Players
            };
            MarkRequestDirty();
        }

        #endregion

        #region Request

        private enum PaginationType {
            Page,
            Seek
        }

        private enum RequestType {
            Scores,
            Players
        }

        private bool _requestDirty;
        private PaginationType _paginationType = PaginationType.Page;
        private RequestType _requestType = RequestType.Scores;

        private void MarkRequestDirty() {
            _requestDirty = true;
        }

        private void UpdateRequestIfDirty() {
            if (!_requestDirty) return;
            _requestDirty = false;

            switch (_requestType) {
                case RequestType.Scores: {
                    SendScoresRequest();
                    break;
                }
                case RequestType.Players: {
                    SendPlayersRequest();
                    break;
                }
            }
        }

        private void SendScoresRequest() {
            switch (_paginationType) {
                case PaginationType.Page: {
                    ScoresOfClanRequest.SendPage(
                        Context.beatmapKey,
                        Context.clanPlayer.id,
                        Context.clanScore.clan.tag,
                        ScoresContexts.General.Key,
                        _currentPage
                    );
                    break;
                }
                case PaginationType.Seek: {
                    ScoresOfClanRequest.SendSeek(
                        Context.beatmapKey,
                        Context.clanPlayer.id,
                        Context.clanScore.clan.tag,
                        ScoresContexts.General.Key
                    );
                    break;
                }
            }
        }

        private void SendPlayersRequest() {
            switch (_paginationType) {
                case PaginationType.Page: {
                    ClanPlayersRequest.SendPage(
                        Context.beatmapKey,
                        Context.clanPlayer.id,
                        Context.clanScore.clan.tag,
                        _currentPage
                    );
                    break;
                }
                case PaginationType.Seek: {
                    ClanPlayersRequest.SendSeek(
                        Context.beatmapKey,
                        Context.clanPlayer.id,
                        Context.clanScore.clan.tag
                    );
                    break;
                }
            }
        }

        #endregion

        #region Pagination

        private static readonly Color HoveredColor = new Color(0.5f, 0.5f, 0.5f);
        private static readonly Color ActiveColor = new Color(1.0f, 1.0f, 1.0f);
        private static readonly Color InactiveColor = new Color(0.2f, 0.2f, 0.2f);

        private bool _paginationDirty;

        private int _currentPage = 1;
        private bool _hasPreviousPage;
        private bool _canSeek;
        private bool _hasNextPage;

        private void MarkPaginationDirty() {
            _paginationDirty = true;
        }

        private void UpdatePaginationIfDirty() {
            if (!_paginationDirty) return;
            _paginationDirty = false;

            SetColors(_pageUpButton, _hasPreviousPage);
            SetColors(_aroundMeButton, _canSeek);
            SetColors(_pageDownButton, _hasNextPage);
        }

        private static void SetColors(ClickableImage image, bool interactable) {
            if (interactable) {
                image.DefaultColor = ActiveColor;
                image.HighlightColor = HoveredColor;
            } else {
                image.DefaultColor = InactiveColor;
                image.HighlightColor = InactiveColor;
            }
        }

        #endregion
    }
}