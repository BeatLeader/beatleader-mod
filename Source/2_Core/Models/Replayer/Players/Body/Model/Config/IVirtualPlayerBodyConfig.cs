using System;
using System.Collections.Generic;

namespace BeatLeader.Models {
    public interface IVirtualPlayerBodyConfig {
        IReadOnlyDictionary<string, IVirtualPlayerBodyPartConfig> BodyParts { get; }
        
        /// <summary>
        /// Setting mask allows you to hide specific body parts which are bound to the specified nodes.
        /// 1 means that the mask does not apply and 0 means that it does.
        /// </summary>
        BodyNode AnchorMask { get; set; }
        
        /// <summary>
        /// Invokes when sibling or self is updated. Provides null param when invocation belongs to the config itself
        /// </summary>
        event Action<IVirtualPlayerBodyPartConfig?>? ConfigUpdatedEvent;
    }
}