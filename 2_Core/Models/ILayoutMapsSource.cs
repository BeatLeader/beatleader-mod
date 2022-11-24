using BeatLeader.Components;
using System.Collections.Generic;

namespace BeatLeader.Models {
    internal interface ILayoutMapsSource {
        IDictionary<string, LayoutMapData> Maps { get; }
    }
}
