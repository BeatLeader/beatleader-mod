using BeatLeader.Models;

namespace BeatLeader.Utils {
    class ScoreUtil {

        public static bool Submission = true;
        public static bool SiraSubmission = true;
        public static bool BS_UtilsSubmission = true;

        public static async void ProcessReplay(Replay replay) {
            if (replay.info.score <= 0) return; // no lightshow here

            bool practice = replay.info.speed != 0;
            bool fail = replay.info.failTime > 0;

            if (practice || fail) {
                Plugin.Log.Debug("Practice/fail, only local replay would be saved");

                FileManager.WriteReplay(replay); // save the last replay
                return;
            }

            var localReplay = FileManager.ReadReplay(FileManager.ToFileName(replay));
            int localHighScore = localReplay == null ? int.MinValue : localReplay.info.score;
            Plugin.Log.Debug($"Local PB from replay = {localHighScore}");

            // int serverHighScore = TODO


            if (localHighScore < replay.info.score) {
                Plugin.Log.Debug("New PB, upload incoming");
                FileManager.WriteReplay(replay);
                if (ShouldSubmit()) {
                    await UploadManager.UploadReplay(replay);
                }
            } else {
                Plugin.Log.Debug("No new PB, score would not be uploaded/rewritten");
            }
        }

        internal static void EnableSubmission() {
            Submission = true;
            SiraSubmission = true;
            BS_UtilsSubmission = true;
        }

        private static bool ShouldSubmit() {
            return Submission && SiraSubmission && BS_UtilsSubmission;
        }
    }
}
