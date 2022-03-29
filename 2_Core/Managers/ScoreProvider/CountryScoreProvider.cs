using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.ScoreProvider {
    internal class CountryScoreProvider : AbstractScoreProvider {

        public CountryScoreProvider(HttpUtils httpUtils) : base(httpUtils) {
        }

        public override ScoresScope getScope() {
            return ScoresScope.Country;
        }
    }
}
