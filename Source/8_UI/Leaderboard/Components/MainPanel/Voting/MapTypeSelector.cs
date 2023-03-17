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

                AccColor = ChooseColor(value, MapType.Acc);
                TechColor = ChooseColor(value, MapType.Tech);
                MidspeedColor = ChooseColor(value, MapType.Midspeed);
                SpeedColor = ChooseColor(value, MapType.Speed);

                OnStateChangedEvent?.Invoke(value);
            }
        }

        private static string ChooseColor(MapType mask, MapType targetType) {
            if (mask == MapType.Unknown) return UndecidedColor;
            return mask.HasFlag(targetType) ? ActiveColor : InactiveColor;
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
            if (CurrentState.HasFlag(MapType.Acc)) {
                CurrentState &= ~MapType.Acc;
            } else {
                CurrentState |= MapType.Acc;
            }
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
            if (CurrentState.HasFlag(MapType.Tech)) {
                CurrentState &= ~MapType.Tech;
            } else {
                CurrentState |= MapType.Tech;
            }
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
            if (CurrentState.HasFlag(MapType.Midspeed)) {
                CurrentState &= ~MapType.Midspeed;
            } else {
                CurrentState |= MapType.Midspeed;
            }
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
            if (CurrentState.HasFlag(MapType.Speed)) {
                CurrentState &= ~MapType.Speed;
            } else {
                CurrentState |= MapType.Speed;
            }
        }

        #endregion
    }
}