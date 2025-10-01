using System;
using JetBrains.Annotations;

namespace BeatLeader.Models;

[PublicAPI]
public class PlatformEventMap {
    public required long startTime;
    public required long endTime;
    public required MapDetail song;
    public required int day;
    public Score? score;
    public PlatformEventPoints? points;

    public bool IsHappening() {
        var now = DateTime.UtcNow.ToUnixTime();
        return now >= startTime && now <= endTime;
    }

    public bool IsCompleted() {
        return score != null;
    }

    public TimeSpan ExpiresIn() {
        return endTime.AsUnixTime() - DateTime.UtcNow;
    }

    public DateTime EndDate() {
        return endTime.AsUnixTime();
    }

    public DateTime StartDate() {
        return startTime.AsUnixTime();
    }
}