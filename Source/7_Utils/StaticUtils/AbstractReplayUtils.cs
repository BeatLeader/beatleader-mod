using BeatLeader.Models.AbstractReplay;

namespace BeatLeader.Utils {
    internal static class AbstractReplayUtils {
        public static NoteCutInfo SaturateNoteCutInfo(this NoteCutInfo cutInfo, NoteData data) {
            return new NoteCutInfo(
                data,
                cutInfo.speedOK,
                cutInfo.directionOK,
                cutInfo.saberTypeOK,
                cutInfo.wasCutTooSoon,
                cutInfo.saberSpeed,
                cutInfo.saberDir,
                cutInfo.saberType,
                cutInfo.timeDeviation,
                cutInfo.cutDirDeviation,
                cutInfo.cutPoint,
                cutInfo.cutNormal,
                cutInfo.cutDistanceToCenter,
                cutInfo.cutAngle,
                cutInfo.worldRotation,
                cutInfo.inverseWorldRotation,
                cutInfo.noteRotation,
                cutInfo.notePosition,
                cutInfo.saberMovementData
            );
        }

        public static PlayerSpecificSettings GetPlayerSettingsByReplay(this PlayerSpecificSettings settings, IReplay replay) {
            return settings.CopyWith(replay.ReplayData.LeftHanded, replay.ReplayData.FixedHeight, replay.ReplayData.FixedHeight is null);
        }
    }
}