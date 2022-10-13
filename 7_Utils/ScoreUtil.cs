using System;
using BeatLeader.API.Methods;
using BeatLeader.Models;
using BeatLeader.Models.Activity;

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

        public static void ProcessReplay(Replay replay, PlayEndData data) {
            if (replay.info.score <= 0) { // no lightshow here
                Plugin.Log.Debug("Zero score, skip replay processing");
                FileManager.TryWriteTempReplay(replay);
                return;
            }

            bool practice = replay.info.speed != 0;
            if (practice) {
                Plugin.Log.Debug("Practice, only local replay would be saved");
                FileManager.TryWriteReplay(replay); // save the last replay
                return;
            }

            if (ShouldSubmit()) {
                Plugin.Log.Debug("Uploading replay");
                FileManager.TryWriteReplay(replay);

                switch (data.EndType) {
                    case PlayEndData.LevelEndType.Clear: {
                            UploadReplay(replay);
                            break;
                        }
                    case PlayEndData.LevelEndType.Unknown: {
                            Plugin.Log.Debug("Unknown level end Type");
                            break;
                        }
                    default: {
                            UploadPlay(replay, data);
                            break;
                        }
                }
            } else {
                FileManager.TryWriteTempReplay(replay);
                Plugin.Log.Debug("Score submission was disabled");
            }
        }

        public static void UploadReplay(Replay replay) {
            ReplayUploadStartedEvent?.Invoke(replay);
            UploadReplayRequest.SendRequest(replay);
        }

        public static void UploadPlay(Replay replay, PlayEndData data) {
            replay.frames = new(); // remove the frame data
            UploadPlayRequest.SendRequest(replay, data);
        }

        #endregion
    }
}
