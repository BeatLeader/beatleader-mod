using BeatLeader.Models;
using Newtonsoft.Json;
using Reactive;
using Reactive.Components;

namespace BeatLeader {
    /// <summary>
    /// A simple serializable color set for general purposes.
    /// </summary>
    internal class SerializableColorSet {
        public SerializableColorSet() {
            _state = new(ToColorSet());
        }

        public SerializableColor ActiveColor {
            get;
            set {
                field = value;
                RefreshState();
            }
        }

        public SerializableColor HoveredColor {
            get;
            set {
                field = value;
                RefreshState();
            }
        }

        public SerializableColor NotInteractableColor {
            get;
            set {
                field = value;
                RefreshState();
            }
        }

        public SerializableColor Color {
            get;
            set {
                field = value;
                RefreshState();
            }
        }

        [JsonIgnore]
        public IState<ColorSet> State => _state;

        private readonly State<ColorSet> _state;

        public ColorSet ToColorSet() {
            return new ColorSet {
                Color = Color,
                HoveredColor = HoveredColor,
                ActiveColor = ActiveColor,
                HoveredActiveColor = ActiveColor,
                NotInteractableColor = NotInteractableColor
            };
        }

        private void RefreshState() {
            _state.Value = ToColorSet();
        }

        public static implicit operator SerializableColorSet(ColorSet set) {
            return new SerializableColorSet {
                ActiveColor = set.GetColor(GraphicState.Active),
                HoveredColor = set.GetColor(GraphicState.Hovered),
                NotInteractableColor = set.GetColor(GraphicState.NonInteractable),
                Color = set.Color
            };
        }
    }
}