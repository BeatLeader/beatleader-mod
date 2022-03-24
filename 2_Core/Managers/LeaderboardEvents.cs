using System;
using System.Collections.Generic;

using BeatLeader.Models;

namespace BeatLeader.Manager
{
    internal class LeaderboardEvents
    {
        // Called before a score request to server started
        public event Action ScoresRequestStartedEvent;

        // Called after a score data response is processed
        public event Action<List<Score>> ScoresFetchedEvent;

        // Called before a profile request to server started
        public event Action UserProfileStartedEvent;

        // Called after a profile data response is processed
        public event Action<Profile> UserProfileFetchedEvent;

        public void ScoreRequestStarted()
        {
            Action action = this.ScoresRequestStartedEvent;
            action?.Invoke();
        }

        public void PublishScores(List<Score> scores)
        {
            Action<List<Score>> action = this.ScoresFetchedEvent;
            action?.Invoke(scores);
        }

        public void ProfileRequestStarted()
        {
            Action action = this.UserProfileStartedEvent;
            action?.Invoke();
        }

        public void PublishProfile(Profile profile)
        {
            Action<Profile> action = this.UserProfileFetchedEvent;
            action?.Invoke(profile);
        }

    }
}
