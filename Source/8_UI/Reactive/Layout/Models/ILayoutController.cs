using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal interface ILayoutController : IContextMember {
        event Action? LayoutControllerUpdatedEvent;
        
        void ReloadChildren(IEnumerable<ILayoutItem> children);
        void ReloadDimensions(Rect controllerRect);
        
        /// <summary>
        /// Called every time layout update is queued. 
        /// </summary>
        void Recalculate(bool root);
        
        /// <summary>
        /// Called every time layout update is needed. No matter is controller the root or not.
        /// </summary>
        void ApplyChildren();
        
        /// <summary>
        /// Called in case controller is the root of the layout hierarchy, so it can control itself.
        /// </summary>
        void ApplySelf(ILayoutItem item);
    }
}