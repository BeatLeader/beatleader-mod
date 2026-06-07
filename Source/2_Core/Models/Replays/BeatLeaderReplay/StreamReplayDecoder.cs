namespace BeatLeader.Models.Replay;

public static class StreamReplayDecoder {
    private const int ReplayMagic = 0x442d3d69;
    private const byte ReplayVersion = 1;
    private const int MaxStringLength = 300;

    public static ReplayInfo? DecodeReplayInfo(UnmanagedFileReader reader) {
        if (!TryDecodeHeader(reader)) {
            return null;
        }

        var type = (StructType)reader.ReadByte();
        return type == StructType.info
            ? DecodeInfo(reader)
            : null;
    }

    private static bool TryDecodeHeader(UnmanagedFileReader reader) {
        var magic = reader.ReadInt32();
        var version = reader.ReadByte();

        return magic == ReplayMagic && version == ReplayVersion;
    }

    private static ReplayInfo DecodeInfo(UnmanagedFileReader reader) {
        var result = new ReplayInfo();

        result.version = DecodeString(reader);
        result.gameVersion = DecodeString(reader);
        result.timestamp = DecodeString(reader);

        result.playerID = DecodeString(reader);
        result.playerName = DecodeName(reader);
        result.platform = DecodeString(reader);

        result.trackingSytem = DecodeString(reader);
        result.hmd = DecodeString(reader);
        result.controller = DecodeString(reader);

        result.hash = DecodeString(reader);
        result.songName = DecodeString(reader);
        result.mapper = DecodeString(reader);
        result.difficulty = DecodeString(reader);

        result.score = reader.ReadInt32();
        result.mode = DecodeString(reader);
        result.environment = DecodeString(reader);
        result.modifiers = DecodeString(reader);
        result.jumpDistance = reader.ReadSingle();
        result.leftHanded = reader.ReadBool();
        result.height = reader.ReadSingle();

        result.startTime = reader.ReadSingle();
        result.failTime = reader.ReadSingle();
        result.speed = reader.ReadSingle();

        return result;
    }

    private static string DecodeName(UnmanagedFileReader reader) {
        var length = reader.PeekInt32(0);
        if (length is < 0 or > MaxStringLength) {
            return DecodeString(reader);
        }

        var lengthOffset = 0;
        if (length > 0) {
            while (true) {
                var platformLength = reader.PeekInt32(length + 4 + lengthOffset);

                if (platformLength is 6 or 5 or 8) {
                    break;
                }

                lengthOffset++;
            }
        }

        reader.Skip(4);
        return reader.ReadUtf8String(length + lengthOffset);
    }

    private static string DecodeString(UnmanagedFileReader reader) {
        while (true) {
            var length = reader.PeekInt32(0);

            if (length is >= 0 and <= MaxStringLength) {
                reader.Skip(4);
                return reader.ReadUtf8String(length);
            }

            reader.Skip(1);
        }
    }
}