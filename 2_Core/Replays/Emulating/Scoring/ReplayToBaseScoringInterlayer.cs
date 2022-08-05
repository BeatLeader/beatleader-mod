using IPA.Utilities;
using ReplayNoteCutInfo = BeatLeader.Models.NoteCutInfo;
using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Scoring
{
    public class ReplayToBaseScoringInterlayer : IScoringInterlayer
    {
        [Inject] private readonly GoodCutScoringElement.Pool _elementsPool;

        public T Convert<T>(ScoringData data) where T : ScoringElement
        {
            if (typeof(T) != typeof(GoodCutScoringElement)) return default;

            var multiplierEventType = ScoreMultiplierCounter.MultiplierEventType.Positive;
            if (data.noteData.scoringType is NoteData.ScoringType.Ignore or NoteData.ScoringType.NoScore)
                multiplierEventType = ScoreMultiplierCounter.MultiplierEventType.Neutral;

            GoodCutScoringElement scoringElement = _elementsPool.Spawn();
            CutScoreBuffer buffer = CreateCutScoreBuffer(data);
            scoringElement.SetField("_cutScoreBuffer", buffer);
            ((ScoringElement)scoringElement).SetProperty("noteData", buffer.noteCutInfo.noteData);
            scoringElement.SetField("_multiplierEventType", multiplierEventType);
            scoringElement.SetField("_wouldBeCorrectCutBestPossibleMultiplierEventType", multiplierEventType);
            ((ScoringElement)scoringElement).SetProperty("isFinished", true);

            return (T)(object)scoringElement;
        }
        private CutScoreBuffer CreateCutScoreBuffer(ScoringData data)
        {
            CutScoreBuffer buffer = new CutScoreBuffer();

            buffer.SetField("_noteCutInfo", ReplayNoteCutInfo.Parse(data.noteEvent.noteCutInfo, data.noteData,
                data.worldRotation, data.inverseWorldRotation, data.noteRotation, data.notePosition));
            ScoreModel.NoteScoreDefinition noteScoreDefinition = ScoreModel.GetNoteScoreDefinition(data.noteData.scoringType);
            buffer.SetField("_noteScoreDefinition", noteScoreDefinition);

            if (noteScoreDefinition.fixedCutScore == 0)
            {
                buffer.SetField("_beforeCutScore", (int)Mathf.Clamp(Mathf.Round(70 * data.noteEvent.noteCutInfo.beforeCutRating), 0, 70));
                buffer.SetField("_afterCutScore", (int)Mathf.Clamp(Mathf.Round(30 * data.noteEvent.noteCutInfo.afterCutRating), 0, 30));
                buffer.SetField("_centerDistanceCutScore", (int)Mathf.Round(15 * (1 - Mathf.Clamp(data.noteEvent.noteCutInfo.cutDistanceToCenter / 0.3f, 0.0f, 1.0f))));
            }

            SaberSwingRatingCounter saberSwingRatingCounter = buffer.GetField<SaberSwingRatingCounter, CutScoreBuffer>("_saberSwingRatingCounter");
            saberSwingRatingCounter.SetField("_beforeCutRating", data.noteEvent.noteCutInfo.beforeCutRating);
            saberSwingRatingCounter.SetField("_afterCutRating", data.noteEvent.noteCutInfo.afterCutRating);

            buffer.SetField("_initialized", true);
            buffer.SetField("_isFinished", true);

            return buffer;
        }
    }
}
