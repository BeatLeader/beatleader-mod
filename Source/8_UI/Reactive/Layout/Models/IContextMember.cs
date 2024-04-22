using System;

namespace BeatLeader.UI.Reactive {
    internal interface IContextMember {
        Type? ContextType { get; }
        
        object CreateContext();
        void ProvideContext(object context);
    }
}