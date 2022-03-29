using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.ScoreProvider {
    internal class GlobalScoreProvider : AbstractScoreProvider, IScoreProvider {

        public GlobalScoreProvider(HttpUtils httpUtils) : base(httpUtils) {
        }

        public override ScoresScope getScope() {
            return ScoresScope.Global;
        }
    }
}
