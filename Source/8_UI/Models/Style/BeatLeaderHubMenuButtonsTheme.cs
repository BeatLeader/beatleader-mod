using Newtonsoft.Json;
using Reactive;

namespace BeatLeader {
    internal class BeatLeaderHubMenuButtonsTheme {
        public BeatLeaderHubMenuButtonsTheme() {
            _state = new(this);
        }

        public SerializableColorSet ReplayManagerButtonColors {
            get;
            set {
                field = value;
                _state.EnforceInvalidation();
            }
        }

        public SerializableColorSet BattleRoyaleButtonColors {
            get;
            set {
                field = value;
                _state.EnforceInvalidation();
            }
        }

        public SerializableColorSet SettingsButtonColors {
            get;
            set {
                field = value;
                _state.EnforceInvalidation();
            }
        }

        public SerializableColorSet EditAvatarButtonColors {
            get;
            set {
                field = value;
                _state.EnforceInvalidation();
            }
        }

        [JsonIgnore]
        public IState<BeatLeaderHubMenuButtonsTheme> State => _state;

        private readonly State<BeatLeaderHubMenuButtonsTheme> _state;
    }
}