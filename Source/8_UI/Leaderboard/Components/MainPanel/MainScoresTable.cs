using System;
using System.Collections;
using System.Collections.Generic;
using BeatLeader.API;
using BeatLeader.DataManager;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Leaderboard.Components.MainPanel.ScoresTable.bsml")]
    internal class MainScoresTable : AbstractScoresTable<ScoreRow> {
        #region Properties

        protected override int RowsCount => 10;
        protected override float RowWidth => 80;
        protected override float Spacing => 1.3f;
        protected override ScoreRowCellType CellTypeMask => PluginConfig.LeaderboardTableMask;

        #endregion

        #region Initialize/Dispose

        protected override void OnInitialize() {
            base.OnInitialize();

            ScoresRequest.StateChangedEvent += OnScoresRequestStateChanged;
            ClanScoresRequest.StateChangedEvent += OnScoresRequestStateChanged;

            LeaderboardState.IsVisibleChangedEvent += OnLeaderboardVisibleChanged;
            PluginConfig.LeaderboardTableMaskChangedEvent += OnLeaderboardTableMaskChanged;
            HiddenPlayersCache.HiddenPlayersUpdatedEvent += UpdateLayout;
            
            LeaderboardEvents.BattleRoyaleEnabledEvent += OnBattleRoyaleEnabledChanged;
            LeaderboardEvents.ScoreInfoButtonWasPressed += OnScoreClicked;
            
            OnLeaderboardTableMaskChanged(PluginConfig.LeaderboardTableMask);
        }

        protected override void OnDispose() {
            base.OnDispose();

            ScoresRequest.StateChangedEvent -= OnScoresRequestStateChanged;
            ClanScoresRequest.StateChangedEvent -= OnScoresRequestStateChanged;

            LeaderboardState.IsVisibleChangedEvent -= OnLeaderboardVisibleChanged;
            PluginConfig.LeaderboardTableMaskChangedEvent -= OnLeaderboardTableMaskChanged;
            HiddenPlayersCache.HiddenPlayersUpdatedEvent -= UpdateLayout;
            
            LeaderboardEvents.BattleRoyaleEnabledEvent -= OnBattleRoyaleEnabledChanged;
            LeaderboardEvents.ScoreInfoButtonWasPressed -= OnScoreClicked;
        }

        #endregion

        #region BattleRoyale
        
        private readonly HashSet<IScoreRowContent> _selectedContents = new();
        private bool _battleRoyaleEnabled;
        
        private void StartBattleRoyaleAnimation() {
            IEnumerator Coroutine() {
                yield return FadeOutCoroutine();
                yield return new WaitForSeconds(0.05f);
                yield return FadeInCoroutine(_content!);
            }

            StartCoroutine(Coroutine());
        }
        
        private void OnScoreClicked(Score score) {
            if (!_battleRoyaleEnabled) {
                return;
            }

            if (_selectedContents.Contains(score)) {
                _selectedContents.Remove(score);
            } else {
                _selectedContents.Add(score);
            }

            RefreshCells();
        }
        
        private void OnBattleRoyaleEnabledChanged(bool brEnabled) {
            _battleRoyaleEnabled = brEnabled;

            if (brEnabled) {
                _selectedContents.Clear();
            }

            RefreshCells();
            StartBattleRoyaleAnimation();
        }
        
        private void RefreshCells() {
            if (_content == null) {
                return;
            }

            if (_content.ExtraRowContent != null) {
                _extraRow.SetContent(_content.ExtraRowContent);
            }

            for (var i = 0; i < RowsCount; i++) {
                if (i >= _content.MainRowContents.Count) continue;

                var row = _mainRows[i];
                row.SetContent(_content.MainRowContents[i]);

                if (_battleRoyaleEnabled) {
                    row.SetCustomHighlight(content => content != null && _selectedContents.Contains(content));
                } else {
                    row.SetCustomHighlight(null);
                }
            }
        }

        #endregion

        #region Events

        private void OnScoresRequestStateChanged(WebRequests.IWebRequest<ScoresTableContent> instance, WebRequests.RequestState state, string? failReason) {
            if (state is not WebRequests.RequestState.Finished) {
                PresentContent(null);
                return;
            }

            PresentContent(instance.Result);
        }

        private void OnLeaderboardVisibleChanged(bool isVisible) {
            if (isVisible) return;
            StartAnimation();
        }

        private void OnLeaderboardTableMaskChanged(ScoreRowCellType value) {
            UpdateLayout();
        }

        #endregion
    }
}