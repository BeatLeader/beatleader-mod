using System;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class MapTypeSelector : ReeUIComponentV2 {
        #region State

        public event Action<MapType> OnStateChangedEvent;

        private MapType _state = MapType.Unknown;

        public MapType CurrentState {
            get => _state;
            private set {
                if (_state.Equals(value)) return;
                _state = value;

                switch (value) {
                    case MapType.Unknown:
                        AccColor = TechColor = MidspeedColor = SpeedColor = UndecidedColor;
                        break;
                    case MapType.Acc:
                        AccColor = ActiveColor;
                        TechColor = MidspeedColor = SpeedColor = InactiveColor;
                        break;
                    case MapType.Tech:
                        TechColor = ActiveColor;
                        AccColor = MidspeedColor = SpeedColor = InactiveColor;
                        break;
                    case MapType.Midspeed:
                        MidspeedColor = ActiveColor;
                        AccColor = TechColor = SpeedColor = InactiveColor;
                        break;
                    case MapType.Speed:
                        SpeedColor = ActiveColor;
                        AccColor = TechColor = MidspeedColor = InactiveColor;
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }

                OnStateChangedEvent?.Invoke(value);
            }
        }

        public void Reset() {
            CurrentState = MapType.Unknown;
        }

        #endregion

        #region Colors

        private const string UndecidedColor = "#888888";
        private const string InactiveColor = "#555555";
        private const string ActiveColor = "#99ffff";

        #endregion

        #region ACC button

        private string _accColor = UndecidedColor;

        [UIValue("acc-color"), UsedImplicitly]
        private string AccColor {
            get => _accColor;
            set {
                if (_accColor.Equals(value)) return;
                _accColor = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("acc-on-click"), UsedImplicitly]
        private void AccOnClick() {
            CurrentState = MapType.Acc;
        }

        #endregion

        #region TECH button

        private string _techColor = UndecidedColor;

        [UIValue("tech-color"), UsedImplicitly]
        private string TechColor {
            get => _techColor;
            set {
                if (_techColor.Equals(value)) return;
                _techColor = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("tech-on-click"), UsedImplicitly]
        private void TechOnClick() {
            CurrentState = MapType.Tech;
        }

        #endregion

        #region MIDSPEED button

        private string _midspeedColor = UndecidedColor;

        [UIValue("midspeed-color"), UsedImplicitly]
        private string MidspeedColor {
            get => _midspeedColor;
            set {
                if (_midspeedColor.Equals(value)) return;
                _midspeedColor = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("midspeed-on-click"), UsedImplicitly]
        private void MidspeedOnClick() {
            CurrentState = MapType.Midspeed;
        }

        #endregion

        #region SPEED button

        private string _speedColor = UndecidedColor;

        [UIValue("speed-color"), UsedImplicitly]
        private string SpeedColor {
            get => _speedColor;
            set {
                if (_speedColor.Equals(value)) return;
                _speedColor = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("speed-on-click"), UsedImplicitly]
        private void SpeedOnClick() {
            CurrentState = MapType.Speed;
        }

        #endregion
    }
}