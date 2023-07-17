using System.Collections.Generic;
using LevelEndStateType = LevelCompletionResults.LevelEndStateType;
using LevelEndAction = LevelCompletionResults.LevelEndAction;

namespace BeatLeader.Models.Activity {
    public class PlayEndData {
        private readonly LevelEndType _endType;
        private readonly float _time;

        public LevelEndType EndType => _endType;
        public float Time => _time;

        public PlayEndData(LevelCompletionResults results) {
            _endType = ToLevelEndAction(results);
            _time = results.endSongTime;
        }

        public PlayEndData(LevelEndType endType, float time) {
            _endType = endType;
            _time = time;
        }

        private readonly Dictionary<(LevelEndStateType, LevelEndAction), LevelEndType> _types = new() {
            { ( LevelEndStateType.Failed    , LevelEndAction.Restart ), LevelEndType.Fail    }, // level failed, autorestart option enabled
            { ( LevelEndStateType.Failed    , LevelEndAction.None    ), LevelEndType.Fail    }, // level failed
            { ( LevelEndStateType.Incomplete, LevelEndAction.Restart ), LevelEndType.Restart }, // restart level (from pause, etc)
            { ( LevelEndStateType.Incomplete, LevelEndAction.Quit    ), LevelEndType.Quit    }, // exit to menu (from pause, etc)
            { ( LevelEndStateType.Incomplete, LevelEndAction.None    ), LevelEndType.Quit    }, // MP disconnect/ Give up
            { ( LevelEndStateType.Cleared   , LevelEndAction.None    ), LevelEndType.Clear   }, // success level end
        };

        private LevelEndType ToLevelEndAction(LevelCompletionResults results) {
            LevelEndStateType levelEndStateType = results.levelEndStateType;
            LevelEndAction levelEndAction = results.levelEndAction;

            return _types.TryGetValue((levelEndStateType, levelEndAction), out var type) ? type : LevelEndType.Unknown;
        }

        public enum LevelEndType {
            Unknown = 0,
            Clear = 1,
            Fail = 2,
            Restart = 3,
            Quit = 4
        }
    }
}
