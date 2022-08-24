using System;
using BeatLeader.API.Methods;
using BeatLeader.Models;

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
            return Submission && SiraSubmission && BS_UtilsSubmission;
        }

        #endregion

        #region ProcessReplay

        public static Action<Replay> ReplayUploadStartedEvent;

        public static void ProcessReplay(Replay replay) {
            if (replay.info.score <= 0) { // no lightshow here
                Plugin.Log.Debug("Zero score, skip replay processing");
                return;
            }

            bool practice = replay.info.speed != 0;
            bool fail = replay.info.failTime > 0;

            if (practice || fail) {
                Plugin.Log.Debug("Practice/fail, only local replay would be saved");

                FileManager.WriteReplay(replay); // save the last replay
                return;
            }

            if (ShouldSubmit()) {
                Plugin.Log.Debug("Uploading replay");
                FileManager.WriteReplay(replay);
                UploadReplay(replay);
            } else {
                Plugin.Log.Debug("Score submission was disabled");
            }
        }

        public static void UploadReplay(Replay replay) {
            ReplayUploadStartedEvent?.Invoke(replay);
            UploadReplayRequest.SendRequest(replay);
        }

        #endregion
    }
}
