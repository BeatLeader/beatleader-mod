using System.Collections.Generic;
using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Utils {
    class ScoreUtil : MonoBehaviour {

        internal static bool Submission = true;
        internal static bool SiraSubmission = true;
        internal static bool BS_UtilsSubmission = true;

        private HttpUtils _httpUtils;
        private Dictionary<string, float> _modifiers;

        [Inject]
        public void Construct(HttpUtils httpUtils) {
            _httpUtils = httpUtils;
        }

        private void Start() {
            StartCoroutine(_httpUtils.GetData<Dictionary<string, float>>(BLConstants.MODIFIERS_URL,
                modifiers => {
                    _modifiers = modifiers;
                },
                () => {
                    Plugin.Log.Error("Can't fetch values for modifiers");
                },
                3)
            );
        }

        public void ProcessReplay(Replay replay) {
            if (replay.info.score <= 0) return; // no lightshow here

            bool practice = replay.info.speed != 0;
            bool fail = replay.info.failTime > 0;

            if (practice || fail) {
                Plugin.Log.Debug("Practice/fail, only local replay would be saved");

                FileManager.WriteReplay(replay); // save the last replay
                return;
            }

            var localReplay = FileManager.ReadReplay(FileManager.ToFileName(replay));
            int localHighScore = localReplay == null ? int.MinValue : ScoreWithModifiers(replay);
            Plugin.Log.Debug($"Local PB from replay: {localHighScore}");

            // int serverHighScore = TODO
            // still todo

            if (localHighScore < replay.info.score) {
                Plugin.Log.Debug("New PB, upload incoming");
                if (ShouldSubmit()) {
                    FileManager.WriteReplay(replay);
                    StartCoroutine(_httpUtils.UploadReplay(replay));
                } else {
                    Plugin.Log.Debug("Score submission was disabled");
                }
            } else {
                Plugin.Log.Debug("No new PB, score would not be uploaded/rewritten");
            }
        }

        private int ScoreWithModifiers(Replay replay) {
            float factor = 1;
            string modifiers = replay.info.modifiers;
            int score = replay.info.score;

            // well .. that's unfortunate -.-
            if (modifiers == null || modifiers.Length == 0) { return score; }

            foreach (string mod in modifiers.Split(',')) {
                if (_modifiers.ContainsKey(mod)) {
                    factor += _modifiers[mod];
                }
            }
            return (int)(score * factor);
        }

        internal static void EnableSubmission() {
            Submission = true;
            SiraSubmission = true;
            BS_UtilsSubmission = true;
        }

        private bool ShouldSubmit() {
            return Submission && SiraSubmission && BS_UtilsSubmission;
        }
    }
}
