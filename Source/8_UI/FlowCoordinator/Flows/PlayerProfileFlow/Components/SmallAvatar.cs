using BeatLeader.Components;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class SmallAvatar : ReactiveComponent {
        public string? Url {
            get => _url.Value;
            set => _url.Value = value;
        }

        private State<string?> _url = null!;

        protected override GameObject Construct() {
            _url = Remember<string?>(null);

            return new ReeWrapperV2<PlayerAvatar>()
                .With(static x => x.ReeComponent.Setup(true))
                .On(_url, static (x, y) => x.ReeComponent.SetAvatar(y, null))
                .Use();
        }
    }
}
