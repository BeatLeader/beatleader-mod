using System;
using System.Collections.Generic;
using HMUI;
using JetBrains.Annotations;
using LeaderboardCore.Managers;
using LeaderboardCore.Models;
using Zenject;

using BeatLeader.Models;
using BeatLeader.Manager;

namespace BeatLeader
{
    [UsedImplicitly]
    internal class BeatLeaderCustomLeaderboard : CustomLeaderboard, IInitializable, IDisposable
    {
        #region Inject

        private readonly CustomLeaderboardManager _customLeaderboardManager;
        private readonly LeaderboardPanel _leaderboardPanel;
        private readonly LeaderboardView _leaderboardView;
        private readonly LeaderboardEvents _leaderboardEvents;

        public BeatLeaderCustomLeaderboard(
            CustomLeaderboardManager customLeaderboardManager,
            LeaderboardPanel panelViewController,
            LeaderboardView leaderboardViewController,
            LeaderboardEvents leaderboardEvents
        )
        {
            _customLeaderboardManager = customLeaderboardManager;
            _leaderboardPanel = panelViewController;
            _leaderboardView = leaderboardViewController;
            _leaderboardEvents = leaderboardEvents;
        }

        #endregion

        #region CustomLeaderboard Implementation

        protected override ViewController panelViewController => _leaderboardPanel;
        protected override ViewController leaderboardViewController => _leaderboardView;

        #endregion

        #region Initialize & Dispose   (Register/UnRegister)

        public void Initialize()
        {
            _customLeaderboardManager.Register(this);
            _leaderboardEvents.ScoresRequestStartedEvent += OnScoreRequestStarter;
            _leaderboardEvents.ScoresFetchedEvent += OnScoresFetched;
            _leaderboardEvents.UserProfileStartedEvent += OnProfileRequestStarter;
            _leaderboardEvents.UserProfileFetchedEvent += OnProfileFetched;
        }

        public void Dispose()
        {
            _customLeaderboardManager.Unregister(this);
            _leaderboardEvents.ScoresRequestStartedEvent -= OnScoreRequestStarter;
            _leaderboardEvents.ScoresFetchedEvent -= OnScoresFetched;
            _leaderboardEvents.UserProfileStartedEvent -= OnProfileRequestStarter;
            _leaderboardEvents.UserProfileFetchedEvent -= OnProfileFetched;
        }

        #endregion

        #region Score related events

        private void OnScoreRequestStarter()
        {
            _leaderboardView.PlaceholderText = "Loading...";
        }

        private void OnScoresFetched(List<Score> scores)
        {
            Plugin.Log.Debug("Processing scores for UI");
            if (scores.Count > 0)
            {
                var txt = "";
                scores.ForEach(score =>
                {
                    //txt += $"#{score.rank}\t{score.player.name}\t\t{score.pp}pp\t{score.baseScore} ({(int)(score.accuracy*10000) / 100}%)\n";
                    txt += String.Format("#{0,-3} {1,-20} {2,6:.00}pp {3,-10} ({4:.00}%) \n", score.rank, score.player.name, score.pp, score.baseScore, score.accuracy * 100);
                });
                Plugin.Log.Debug("Updating score panel");
                Plugin.Log.Debug(txt);
                _leaderboardView.PlaceholderText = txt;
            }
            else
            {
                _leaderboardView.PlaceholderText = "No scores found";
            }
        }

        #endregion

        #region Profile related events

        private void OnProfileRequestStarter()
        {
            _leaderboardPanel.PlaceholderText = "Loading...";
        }

        private void OnProfileFetched(Profile p)
        {
            if (p == null)
            {
                _leaderboardPanel.PlaceholderText = "Internet error or profile does not exist.";
            }
            else
            {
                string txt = String.Format("#{0} ({1} #{2}) {3}\n{4:.00}pp", p.rank, p.country, p.countryRank, p.name, p.pp);
                _leaderboardPanel.PlaceholderText = txt;
            }
        }

        #endregion
    }
}