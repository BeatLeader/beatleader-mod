using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using BeatLeader.API;
using BeatLeader.Models;
using BeatLeader.Models.Replay;

namespace BeatLeader.Utils {
    internal static class ScoreUtil {
        #region Submission

        internal static bool Submission = true;
        internal static bool SiraSubmission = true;
        internal static bool BS_UtilsSubmission = true;

        internal static void EnableSubmission() {
            Submission = true;
            SiraSubmission = true;
            BS_UtilsSubmission = true;
        }

        private static bool ShouldSubmit() {
            return Submission && (SiraSubmission || OnlySpeedTicketDisablesSubmission()) && BS_UtilsSubmission;
        }

        internal const string SpeedTicketSource = "BeatLeader";
        internal const string SpeedTicketReason = "BFS and BSF scores go to BeatLeader only";

        internal static SiraUtil.Submissions.Submission? SiraSubmissionService;

        private static readonly PropertyInfo? ticketSourceProperty = typeof(SiraUtil.Submissions.Ticket)
            .GetProperty("Source", BindingFlags.Instance | BindingFlags.NonPublic);

        private static bool OnlySpeedTicketDisablesSubmission() {
            var tickets = SiraSubmissionService?.Tickets();
            if (tickets == null || tickets.Length == 0) return false;
            return tickets.All(ticket => ticketSourceProperty?.GetValue(ticket) as string == SpeedTicketSource
                && ticket.Reasons().All(reason => reason == SpeedTicketReason));
        }

        #endregion

        #region ProcessReplay

        public static Action<Replay, PlayEndData>? ReplayUploadStartedEvent;

        public static void ProcessReplay(Replay replay, PlayEndData data) {
            if (replay.info.speed != 0) {
                data = new PlayEndData(LevelEndType.Practice, data.Time);
            }

            if (!ShouldSubmit()) {
                Plugin.Log.Debug("Score submission was disabled");
                SaveReplay(replay, data);
                return;
            }

            Plugin.Log.Debug("Uploading replay");
            switch (data.EndType) {
                case LevelEndType.Unknown:
                    Plugin.Log.Debug("Unknown level end Type");
                    break;
                default:
                    UploadReplay(replay, data);
                    break;
            }

            SaveReplay(replay, data);
        }

        private static void SaveReplay(Replay replay, PlayEndData data) {
            ReplayManager.SaveReplayAsync(replay, data, CancellationToken.None).RunCatching();
        }

        public static void UploadReplay(Replay replay, PlayEndData data) {
            ReplayUploadStartedEvent?.Invoke(replay, data);
            UploadReplayRequest.Send(replay, data);
        }

        #endregion
    }
}