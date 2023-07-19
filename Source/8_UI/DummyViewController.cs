using System;
using HMUI;
using IPA.Utilities;
using UnityEngine;
using Screen = HMUI.Screen;

namespace BeatLeader.Components {
    internal class DummyViewController : ViewController {
        public static DummyViewController Wrap(ViewController controller, bool deactivateAfterTransition = false) {
            var dummy = controller.gameObject.AddComponent<DummyViewController>();
            dummy.Init(controller);
            dummy.deactivateAfterTransition = deactivateAfterTransition;
            return dummy;
        }

        protected Transform? _originalParent;
        protected Screen? _originalScreen;
        protected ViewController? _originalParentController;
        protected ViewController? _originalViewController;

        public bool deactivateAfterTransition;

        public void Init(ViewController originalViewController) {
            _originalViewController = originalViewController;
        }

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (_originalViewController == null) throw new ArgumentNullException(nameof(_originalViewController));
            _originalScreen = _originalViewController.screen;
            _originalParentController = _originalViewController.parentViewController;
            _originalParent = transform.parent;
            _originalViewController!.__Init(screen, parentViewController, null);
            _originalViewController.__Activate(addedToHierarchy, screenSystemEnabling);
        }

        public override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            if (deactivateAfterTransition) {
                _originalViewController!.__Deactivate(removedFromHierarchy, false, screenSystemDisabling);
            }
            _originalViewController.SetField("_screen", _originalScreen);
            _originalViewController.SetField("_parentViewController", _originalParentController);
            transform.SetParent(_originalParent);
        }
    }
}