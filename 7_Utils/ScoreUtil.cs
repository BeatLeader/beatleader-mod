using System.Collections;
using System.Collections.Generic;
using BeatLeader.Models;
using UnityEngine;
using Zenject;

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
        
        private Dictionary<string, float> _modifiers;

        private IEnumerator UpdateModifiersIfNeeded() {
            if (_modifiers != null) yield break;
            yield return HttpUtils.GetData<Dictionary<string, float>>(BLConstants.MODIFIERS_URL,
                modifiers => _modifiers = modifiers,
                () => Plugin.Log.Error("Can't fetch values for modifiers"),
                3);
        }

        private int ScoreWithModifiers(Replay replay) {
            float factor = 1;
            string modifiers = replay.info.modifiers;
            int score = replay.info.score;

            // well .. that's unfortunate -.-
            if (_modifiers == null || _modifiers.Count == 0) { return score; }

            foreach (string mod in modifiers.Split(',')) {
                if (_modifiers.ContainsKey(mod)) {
                    factor += _modifiers[mod];
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
            if (replay.info.score <= 0) yield break; // no lightshow here

            bool practice = replay.info.speed != 0;
            bool fail = replay.info.failTime > 0;

            if (practice || fail) {
                Plugin.Log.Debug("Practice/fail, only local replay would be saved");

                FileManager.WriteReplay(replay); // save the last replay
                yield break;
            }

            yield return UpdateModifiersIfNeeded();

            var localReplay = FileManager.ReadReplay(FileManager.ToFileName(replay));
            int localHighScore = localReplay == null ? int.MinValue : ScoreWithModifiers(localReplay);
            Plugin.Log.Debug($"Local PB from replay: {localHighScore}");

            // int serverHighScore = TODO
            // still todo

            if (localHighScore < ScoreWithModifiers(replay)) {
                Plugin.Log.Debug("New PB, upload incoming");
                if (ShouldSubmit()) {
                    FileManager.WriteReplay(replay);
                    yield return HttpUtils.UploadReplay(replay);
                } else {
                    Plugin.Log.Debug("Score submission was disabled");
                }
            } else {
                Plugin.Log.Debug("No new PB, score would not be uploaded/rewritten");
            }
        }
        
        #endregion
    }
}
