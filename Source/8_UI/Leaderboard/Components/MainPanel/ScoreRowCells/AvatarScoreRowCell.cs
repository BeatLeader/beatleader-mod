using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class AvatarScoreRowCell : AbstractScoreRowCell {
        #region Components

        private const float Size = 4.0f;

        [UIValue("player-avatar"), UsedImplicitly]
        private PlayerAvatar _playerAvatar;

        private void Awake() {
            _playerAvatar = Instantiate<PlayerAvatar>(transform);
            _playerAvatar.Setup(true);
        }

        #endregion

        #region Implementation

        public struct Data {
            public readonly string url;
            public readonly ProfileSettings? profileSettings;

            public Data(string url, ProfileSettings? profileSettings) {
                this.url = url;
                this.profileSettings = profileSettings;
            }
        }

        public override void SetValue(object? value) {
            if (value is Data data) {
                _playerAvatar.SetAvatar(data.url, data.profileSettings);
                isEmpty = false;
            } else {
                _playerAvatar.SetAvatar(null, null);
                isEmpty = true;
            }
        }

        public override void SetAlpha(float value) {
            _playerAvatar.SetAlpha(value);
        }

        protected override float CalculatePreferredWidth() {
            return Size;
        }

        #endregion
    }
}