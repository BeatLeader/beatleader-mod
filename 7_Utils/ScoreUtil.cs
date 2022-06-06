using System.Collections;
using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader.Utils {
    internal class ScoreUtil : PersistentSingleton<ScoreUtil> {
        #region Submission

        internal static bool Submission = true;
        internal static bool SiraSubmission = true;
        internal static bool BS_UtilsSubmission = true;

        internal static void EnableSubmission() {
            Submission = true;
            SiraSubmission = true;
            BS_UtilsSubmission = true;
        }

        private bool ShouldSubmit() {
            return Submission && SiraSubmission && BS_UtilsSubmission;
        }

        #endregion

        #region Modifiers

        private int ScoreWithModifiers(Replay replay) {
            float factor = 1;
            string replayModifiers = replay.info.modifiers;
            int score = replay.info.score;

            // well .. that's unfortunate -.-
            if (!(ModifiersUtils.instance.HasModifiers)) { return score; }

            var modifiers = ModifiersUtils.instance.Modifiers;
            foreach (string mod in replayModifiers.Split(',')) {
                if (modifiers.ContainsKey(mod)) {
                    factor += modifiers[mod];
                }
            }
            return (int)(score * factor);
        }

        #endregion

        #region ProcessReplay

        public void ProcessReplayAsync(Replay replay) {
            StartCoroutine(ProcessReplayCoroutine(replay));
        }

        private IEnumerator ProcessReplayCoroutine(Replay replay) {
            if (replay.info.score <= 0) { // no lightshow here
                Plugin.Log.Debug("Zero score, skip replay processing");
                yield break;
            }

            bool practice = replay.info.speed != 0;
            bool fail = replay.info.failTime > 0;

            if (practice || fail) {
                Plugin.Log.Debug("Practice/fail, only local replay would be saved");

                FileManager.WriteReplay(replay); // save the last replay
                yield break;
            }

            if (ShouldSubmit()) {
                Plugin.Log.Debug("Uploading replay");
                FileManager.WriteReplay(replay);
                yield return HttpUtils.UploadReplay(replay);
            } else {
                Plugin.Log.Debug("Score submission was disabled");
            }
        }

        #endregion
    }
}
