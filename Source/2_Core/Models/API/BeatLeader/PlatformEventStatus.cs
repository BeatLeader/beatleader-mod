using System;
using System.Linq;
using JetBrains.Annotations;

namespace BeatLeader.Models;

[PublicAPI]
public class PlatformEventStatus {
    public required PlatformEventMap? today;
    public required PlatformEventMap[] previousDays;
    public required PlatformEvent eventDescription;

    private DateTime? _startTime;

    public DateTime StartDate() {
        if (_startTime == null) {
            var minTime = previousDays.Min(x => x.startTime);

            if (today != null && today.startTime < minTime) {
                minTime = today.startTime;
            }

            _startTime = minTime.AsUnixTime();
        }

        return _startTime!.Value;
    }
}