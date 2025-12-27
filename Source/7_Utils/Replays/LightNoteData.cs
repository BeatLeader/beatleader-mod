using System.Runtime.InteropServices;

namespace BeatLeader.Utils;

[StructLayout(LayoutKind.Auto)]
public readonly struct LightNoteData {
    public readonly ColorType colorType;
    public readonly NoteData.ScoringType scoringType;
    public readonly NoteData.GameplayType gameplayType;
    public readonly bool isArcHead;
    public readonly bool isArcTail;
    public readonly int lineIndex;
    public readonly NoteLineLayer noteLineLayer;
    public readonly NoteCutDirection cutDirection;

    public LightNoteData(
        ColorType colorType,
        NoteData.ScoringType scoringType, 
        NoteData.GameplayType gameplayType,
        bool isArcHead,
        bool isArcTail,
        int lineIndex,
        NoteLineLayer noteLineLayer, 
        NoteCutDirection cutDirection
    ) {
        this.colorType = colorType;
        this.scoringType = scoringType;
        this.gameplayType = gameplayType;
        this.isArcHead = isArcHead;
        this.isArcTail = isArcTail;
        this.lineIndex = lineIndex;
        this.noteLineLayer = noteLineLayer;
        this.cutDirection = cutDirection;
    }

    public LightNoteData MirrorX(int lineCount) {
        return new LightNoteData(
            colorType.Opposite(),
            scoringType,
            gameplayType,
            isArcHead,
            isArcTail,
            lineCount - 1 - lineIndex,
            noteLineLayer,
            cutDirection.Mirrored()
        );
    }
}