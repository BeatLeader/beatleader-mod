using System;
using Zenject;

namespace BeatLeader.Models
{
    internal abstract class ZenjectComponentModel : ILateInitializable, IInitializable, IDisposable
    {
        public virtual void LateInitialize() { }
        public virtual void Initialize() { }
        public virtual void Dispose() { }
    }
}
