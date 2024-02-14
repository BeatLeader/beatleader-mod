using System.Text;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using ModestTree;
using TMPro;

namespace BeatLeader.Components
{
    internal class ScoreOverviewPage1 : ReeUIComponentV2
    {
        #region Config

        [UIValue("font-size")]
        private const int FontSize = 4;

        #endregion

        #region SetScore

        public void SetScore(Score score)
        {
            TimeSetText = GetTimeSetString(score);
            ScoreText = GetStringWithLabel(FormatUtils.FormatScore(score.modifiedScore), "<bll>ls-score</bll>");
            _accText.Text1 = GetStringWithLabel(
                FormatUtils.FormatAcc(score.accuracy),
                "<bll>ls-accuracy</bll> " + (!score.fullCombo ? $"<size=60%><color={Good}>[?]</color></size>" : ""));
            _accText.Text2 = GetStringWithLabel(
                FormatUtils.FormatAcc(score.fcAccuracy),
                $"<color={Good}>FC <bll>ls-accuracy</bll></color>");
            _accText.HoverEnabled = !score.fullCombo;
            _ppText.Text1 = GetStringWithLabel(
                FormatUtils.FormatPP(score.pp),
                "pp " + (!score.fullCombo && score.pp != 0 ? $"<size=60%><color={Good}>[?]</color></size>" : ""));
            _ppText.Text2 = GetStringWithLabel(
                FormatUtils.FormatPP(score.fcPp),
                $"<color={Good}>FC pp</color>");
            _ppText.HoverEnabled = !score.fullCombo && score.fcPp != 0;
            DetailsText = GetDetailsString(score);
        }

        #endregion

        #region Init

        protected override void OnInstantiate()
        {
            _accText = Instantiate<HoverText>(transform);
            _accText.TextObject.alignment = TextAlignmentOptions.Center;
            _accText.TextObject.fontSize = FontSize;
            _accText.TextObject.enableWordWrapping = false;
            _ppText = Instantiate<HoverText>(transform);
            _ppText.TextObject.alignment = TextAlignmentOptions.Center;
            _ppText.TextObject.fontSize = FontSize;
            _ppText.TextObject.enableWordWrapping = false;
        }

        #endregion

        #region SetActive

        public void SetActive(bool value)
        {
            Active = value;
        }

        #endregion

        #region FormatDetailsString

        private const string Neutral = "#FFFFFF";
        private const string Faded = "#888888";
        private const string Good = "#88FF88";
        private const string Bad = "#FF8888";

        private static string GetStringWithLabel(string value, string label)
        {
            var sb = new StringBuilder();
            sb.Append("<line-height=80%>");
            sb.Append($"<color={Faded}><size=70%>{label}\n</size></color>");
            sb.Append(value);
            return sb.ToString();
        }

        private static string GetTimeSetString(Score score)
        {
            var absoluteTimeString = $"<size=100%><color={Neutral}>{FormatUtils.GetDateTimeString(score.timeSet)}</color></size>";
            var relativeTimeString = $"<size=80%><color={Neutral}>   ({FormatUtils.GetRelativeTimeString(score.timeSet, false)})</color></size>";
            var headsetString = $"<size=100%><color={Neutral}>{FormatUtils.GetHeadsetNameById(score.hmd)}</color></size>";

            var sb = new StringBuilder(200);

            var font = BLLocalization.GetLanguageFont();
            if (font != null) sb.Append($"<font={font.name}>");

            sb.Append(absoluteTimeString);
            sb.Append(relativeTimeString);
            sb.AppendLine($"<color={Faded}><size=75%>");

            var controllerName = FormatUtils.GetControllerNameById(score.controller);
            if (!controllerName.Equals("Unknown"))
            {
                var controllersString = $"<size=90%><color={Neutral}>{controllerName}</color></size>";
                var localized = BLLocalization.GetTranslation("ls-hmd-with-controllers")
                    .Replace("<hmd>", headsetString)
                    .Replace("<controllers>", controllersString);
                sb.Append(localized);
            }
            else
            {
                var localized = BLLocalization.GetTranslation("ls-hmd-no-controllers")
                    .Replace("<hmd>", headsetString);
                sb.Append(localized);
            }

            sb.Append("</size></color>");
            return sb.ToString();
        }

        private static string GetDetailsString(Score score)
        {
            var sb = new StringBuilder();

            sb.Append($"<color={Faded}><bll>ls-pauses</bll>: <color={Neutral}>{score.pauses}    ");

            if (score.modifiers.IsEmpty())
            {
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine($"<color={Faded}><bll>ls-modifiers</bll>: <color={Neutral}>{score.modifiers}");
            }

            if (score.fullCombo) sb.Append($"<color={Good}>Full Combo</color>    ");
            if (score.missedNotes > 0) sb.Append($"<color={Faded}><bll>ls-misses</bll>: <color={Bad}>{score.missedNotes}</color>    ");
            if (score.badCuts > 0) sb.Append($"<color={Faded}><bll>ls-bad-cuts</bll>: <color={Bad}>{score.badCuts}</color>    ");
            if (score.bombCuts > 0) sb.Append($"<color={Faded}><bll>ls-bomb-cuts</bll>: <color={Bad}>{score.bombCuts}</color>    ");
            if (score.wallsHit > 0) sb.Append($"<color={Faded}><bll>ls-walls-hit</bll>: <color={Bad}>{score.wallsHit}</color>    ");

            return sb.ToString();
        }

        #endregion

        #region Active

        private bool _active = true;

        [UIValue("active"), UsedImplicitly]
        private bool Active
        {
            get => _active;
            set
            {
                if (_active.Equals(value)) return;
                _active = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region TimeSetText

        private string _timeSetText = "";

        [UIValue("timeset-text"), UsedImplicitly]
        public string TimeSetText
        {
            get => _timeSetText;
            set
            {
                if (_timeSetText.Equals(value)) return;
                _timeSetText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region ScoreText

        private string _scoreText = "";

        [UIValue("score-text"), UsedImplicitly]
        public string ScoreText
        {
            get => _scoreText;
            set
            {
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

        [UIValue("pp-text"), UsedImplicitly]
        private HoverText _ppText = null!;

        #endregion

        #region DetailsText

        private string _detailsText = "";

        [UIValue("details-text"), UsedImplicitly]
        public string DetailsText
        {
            get => _detailsText;
            set
            {
                if (_detailsText.Equals(value)) return;
                _detailsText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}