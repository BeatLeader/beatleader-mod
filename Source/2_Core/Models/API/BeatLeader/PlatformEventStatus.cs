using JetBrains.Annotations;
using Newtonsoft.Json;

namespace BeatLeader.Models;

[PublicAPI]
public class PlatformEventStatus {
    public required PlatformEventMap today;
    public required PlatformEventMap[] previousDays;

    [JsonIgnore]
    public string headerText = "Nothing yet";
}