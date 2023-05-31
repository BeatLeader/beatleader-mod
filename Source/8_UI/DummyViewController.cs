using System;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    [PublicAPI]
    internal class DummyViewController : ViewController {
        public static DummyViewController Wrap(ViewController controller, bool deactivateAfterTransition = false) {
            var dummy = controller.gameObject.AddComponent<DummyViewController>();
            dummy.Init(controller);
            dummy.deactivateAfterTransition = deactivateAfterTransition;
            return dummy;
        }
        
        protected Transform? _originalParent;
        protected ViewController? _originalViewController;

        public bool deactivateAfterTransition;

        public void Init(ViewController originalViewController) {
            _originalViewController = originalViewController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (_originalViewController == null) throw new ArgumentNullException(nameof(_originalViewController));
            _originalParent = transform.parent;
            _originalViewController.__Activate(addedToHierarchy, screenSystemEnabling);
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            if (deactivateAfterTransition) {
                _originalViewController!.__Deactivate(removedFromHierarchy, false, screenSystemDisabling);
            }
            transform.SetParent(_originalParent);
        }
    }
}