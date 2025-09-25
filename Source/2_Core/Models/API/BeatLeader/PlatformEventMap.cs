using System;
using JetBrains.Annotations;

namespace BeatLeader.Models;

[PublicAPI]
public class PlatformEventMap {
    public required long startTime;
    public required long endTime;
    public required MapDetail song;
    
    public bool IsHappening() {
        var now = DateTime.UtcNow.ToUnixTime();
        return now >= startTime && endTime <= now;
    }

    public TimeSpan ExpiresIn() {
        return endTime.AsUnixTime() - DateTime.UtcNow;
    }
}