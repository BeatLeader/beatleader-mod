using System;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class RankabilitySelector : ReeUIComponentV2 {
        #region State

        public event Action<State> OnStateChangedEvent;

        private State _state = State.Undecided;

        public float FloatValue => _state switch {
            State.Undecided => 0,
            State.ForRank => 1,
            State.NotForRank => -1,
            _ => throw new ArgumentOutOfRangeException()
        };

        public State CurrentState {
            get => _state;
            private set {
                if (_state.Equals(value)) return;
                _state = value;

                switch (value) {
                    case State.Undecided:
                        NoColor = UndecidedColor;
                        YesColor = UndecidedColor;
                        break;
                    case State.ForRank:
                        NoColor = InactiveColor;
                        YesColor = YesActiveColor;
                        break;
                    case State.NotForRank:
                        NoColor = NoActiveColor;
                        YesColor = InactiveColor;
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }

                OnStateChangedEvent?.Invoke(value);
            }
        }

        public void Reset() {
            CurrentState = State.Undecided;
        }

        public enum State {
            Undecided,
            ForRank,
            NotForRank
        }

        #endregion

        #region Colors

        private const string UndecidedColor = "#888888";
        private const string InactiveColor = "#555555";
        private const string NoActiveColor = "#FF6666";
        private const string YesActiveColor = "#66FF66";

        #endregion

        #region NO button

        private string _noColor = UndecidedColor;

        [UIValue("no-color"), UsedImplicitly]
        private string NoColor {
            get => _noColor;
            set {
                if (_noColor.Equals(value)) return;
                _noColor = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("no-on-click"), UsedImplicitly]
        private void NoOnClick() {
            CurrentState = State.NotForRank;
        }

        #endregion

        #region YES button

        private string _yesColor = UndecidedColor;

        [UIValue("yes-color"), UsedImplicitly]
        private string YesColor {
            get => _yesColor;
            set {
                if (_yesColor.Equals(value)) return;
                _yesColor = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("yes-on-click"), UsedImplicitly]
        private void YesOnClick() {
            CurrentState = State.ForRank;
        }

        #endregion
    }
}