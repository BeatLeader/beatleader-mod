﻿using System.Text;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using ModestTree;
using TMPro;

namespace BeatLeader.Components {
    internal class ScoreOverviewPage1 : ReeUIComponentV2 {
        #region Config

        [UIValue("font-size")]
        private const int FontSize = 4;

        #endregion
        
        #region SetScore

        public void SetScore(Score score) {
            TimeSetText = GetTimeSetString(score);
            ScoreText = GetStringWithLabel(FormatUtils.FormatScore(score.modifiedScore), "score");
            _accText.Text1 = GetStringWithLabel(
                FormatUtils.FormatAcc(score.accuracy),
                "accuracy " + (!score.fullCombo ? $"<size=60%><color={Good}>[?]</color></size>" : ""));
            _accText.Text2 = GetStringWithLabel(
                FormatUtils.FormatAcc(score.fcAccuracy), 
                $"<color={Good}>fc accuracy</color>");
            _accText.HoverEnabled = !score.fullCombo;
            PpText = GetStringWithLabel(FormatUtils.FormatPP(score.pp), "pp");
            DetailsText = GetDetailsString(score);
        }

        #endregion

        #region Init

        protected override void OnInstantiate() {
            _accText = Instantiate<HoverText>(transform);
            _accText.TextObject.alignment = TextAlignmentOptions.Center;
            _accText.TextObject.fontSize = FontSize;
            _accText.TextObject.enableWordWrapping = false;
        }

        #endregion
        
        #region SetActive

        public void SetActive(bool value) {
            Active = value;
        }

        #endregion

        #region FormatDetailsString

        private const string Neutral = "#FFFFFF";
        private const string Faded = "#888888";
        private const string Good = "#88FF88";
        private const string Bad = "#FF8888";

        private static string GetStringWithLabel(string value, string label) {
            var sb = new StringBuilder();
            sb.Append("<line-height=80%>");
            sb.Append($"<color={Faded}><size=70%>{label}\n</size></color>");
            sb.Append(value);
            return sb.ToString();
        }

        private static string GetTimeSetString(Score score) {
            var sb = new StringBuilder();
            sb.Append("<line-height=80%>");
            sb.Append($"<color={Neutral}>{FormatUtils.GetRelativeTimeString(score.timeSet)}</color>");
            sb.Append($"<color={Faded}><size=70%>   on   </size></color>");
            sb.AppendLine($"<color={Neutral}>{FormatUtils.GetHeadsetNameById(score.hmd)}</color>");
            var controllerName = FormatUtils.GetControllerNameById(score.controller);
            if (!controllerName.Equals("Unknown")) {
                sb.Append($"<color={Faded}><size=75%>using   </size></color>");
                sb.AppendLine($"<color={Neutral}><size=80%>{controllerName}</size></color></voffset>");
            }
            return sb.ToString();
        }

        private static string GetDetailsString(Score score) {
            var sb = new StringBuilder();

            sb.Append("<line-height=80%>");
            sb.Append($"<color={Faded}>Pauses: <color={Neutral}>{score.pauses}    ");
            sb.AppendLine(score.modifiers.IsEmpty()
                ? $"<color={Faded}>No Modifiers"
                : $"<color={Faded}>Modifiers: <color={Neutral}>{score.modifiers}"
            );

            if (score.fullCombo) sb.Append($"<color={Good}>Full Combo</color>    ");
            if (score.missedNotes > 0) sb.Append($"<color={Faded}>Misses: <color={Bad}>{score.missedNotes}</color>    ");
            if (score.badCuts > 0) sb.Append($"<color={Faded}>Bad cuts: <color={Bad}>{score.badCuts}</color>    ");
            if (score.bombCuts > 0) sb.Append($"<color={Faded}>Bomb cuts: <color={Bad}>{score.bombCuts}</color>    ");
            if (score.wallsHit > 0) sb.Append($"<color={Faded}>Walls hit: <color={Bad}>{score.wallsHit}</color>    ");

            return sb.ToString();
        }

        #endregion

        #region Active

        private bool _active = true;

        [UIValue("active"), UsedImplicitly]
        private bool Active {
            get => _active;
            set {
                if (_active.Equals(value)) return;
                _active = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region TimeSetText

        private string _timeSetText = "";

        [UIValue("timeset-text"), UsedImplicitly]
        public string TimeSetText {
            get => _timeSetText;
            set {
                if (_timeSetText.Equals(value)) return;
                _timeSetText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region ScoreText

        private string _scoreText = "";

        [UIValue("score-text"), UsedImplicitly]
        public string ScoreText {
            get => _scoreText;
            set {
                if (_scoreText.Equals(value)) return;
                _scoreText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region AccText

        [UIValue("acc-text"), UsedImplicitly]
        private HoverText _accText = null!;

        #endregion

        #region PpText

        private string _ppText = "";

        [UIValue("pp-text"), UsedImplicitly]
        public string PpText {
            get => _ppText;
            set {
                if (_ppText.Equals(value)) return;
                _ppText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region DetailsText

        private string _detailsText = "";

        [UIValue("details-text"), UsedImplicitly]
        public string DetailsText {
            get => _detailsText;
            set {
                if (_detailsText.Equals(value)) return;
                _detailsText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}