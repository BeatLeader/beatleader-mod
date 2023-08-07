using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.UI.BSML_Addons {
    internal interface IUIComponentDescriptor<T> {
        string ComponentName { get; }

        IDictionary<string, Action<T, object>>? ExternalProperties { get; }

        IEnumerable<Func<T, Component>>? ExternalComponents { get; }
    }
}