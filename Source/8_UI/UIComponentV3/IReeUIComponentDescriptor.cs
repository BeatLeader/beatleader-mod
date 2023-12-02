using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Parser;
using UnityEngine;

namespace BeatLeader.Components {
    internal interface IReeUIComponentDescriptor<T> {
        string ComponentName { get; }

        IDictionary<string, Action<T, BSMLParserParams, object>>? ExternalProperties { get; }

        IEnumerable<Func<T, Component>>? ExternalComponents { get; }
    }
}