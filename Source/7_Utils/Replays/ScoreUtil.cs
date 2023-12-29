using System;
using BeatLeader.API.Methods;
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
            return Submission && SiraSubmission && BS_UtilsSubmission;
        }

        #endregion

        #region ProcessReplay

        public static Action<Replay>? ReplayUploadStartedEvent;

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
                case LevelEndType.Clear:
                    UploadReplay(replay);
                    break;
                case LevelEndType.Unknown:
                    Plugin.Log.Debug("Unknown level end Type");
                    break;
                default:
                    UploadPlay(replay, data);
                    break;
            }

            SaveReplay(replay, data);
            
            static void SaveReplay(Replay replay, PlayEndData data) {
                //_ = ReplayManager.Instance.SaveReplayAsync(replay, data, default);
            }
        }

        public static void UploadReplay(Replay replay) {
            ReplayUploadStartedEvent?.Invoke(replay);
            UploadReplayRequest.SendRequest(replay);
        }

        public static void UploadPlay(Replay replay, PlayEndData data) {
            UploadPlayRequest.SendRequest(replay, data);
        }

        #endregion
    }
}