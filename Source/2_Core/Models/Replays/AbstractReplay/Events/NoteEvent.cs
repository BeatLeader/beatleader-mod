using System.Runtime.InteropServices;

namespace BeatLeader.Models.AbstractReplay {
    [StructLayout(LayoutKind.Auto)]
    public readonly struct NoteEvent {
        public enum NoteEventType {
            Unknown = -1,
            GoodCut = 0,
            BadCut = 1,
            Miss = 2,
            BombCut = 3,
        }

        public NoteEvent(
            int noteId,
            float eventTime,
            float spawnTime,
            NoteEventType eventType,
            float beforeCutRating,
            float afterCutRating,
            NoteCutInfo noteCutInfo
        ) {
            this.noteId = noteId;
            this.eventTime = eventTime;
            this.spawnTime = spawnTime;
            this.eventType = eventType;
            this.beforeCutRating = beforeCutRating;
            this.afterCutRating = afterCutRating;
            this.noteCutInfo = noteCutInfo;
        }

        public float CutTime => spawnTime - noteCutInfo.timeDeviation;

        public readonly int noteId;
        public readonly float eventTime;
        public readonly float spawnTime;
        public readonly NoteEventType eventType;
        public readonly float beforeCutRating;
        public readonly float afterCutRating;
        public readonly NoteCutInfo noteCutInfo;

        public NoteEvent MirrorX() {
            var nci = noteCutInfo;

            var cutPoint = nci.cutPoint.MirrorOnYZPlane();
            var cutNormal = nci.cutNormal.MirrorOnYZPlane();
            var notePosition = nci.notePosition.MirrorOnYZPlane();

            var cutDirDeviation = -nci.cutDirDeviation;
            var cutAngle = -nci.cutAngle;

            var saberDir = nci.saberDir.MirrorOnYZPlane();
            var saberType = nci.saberType is SaberType.SaberB ? SaberType.SaberA : nci.saberType;

            var mirroredCutInfo = new NoteCutInfo(
                nci.noteData,
                nci.speedOK,
                nci.directionOK,
                nci.saberTypeOK,
                nci.wasCutTooSoon,
                nci.saberSpeed,
                saberDir,
                saberType,
                nci.timeDeviation,
                cutDirDeviation,
                cutPoint,
                cutNormal,
                nci.cutDistanceToCenter,
                cutAngle,
                nci.worldRotation,
                nci.inverseWorldRotation,
                nci.noteRotation,
                notePosition,
                nci.saberMovementData
            );

            return new NoteEvent(
                noteId,
                eventTime,
                spawnTime,
                eventType,
                beforeCutRating,
                afterCutRating,
                mirroredCutInfo
            );
        }
    }
}