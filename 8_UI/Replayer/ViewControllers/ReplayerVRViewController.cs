using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Replayer.Movement;
using BeatLeader.Components;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.FloatingScreen;
using UnityEngine.XR;
using UnityEngine;
using Zenject;
using BeatLeader.Replayer.Camera;

namespace BeatLeader.ViewControllers
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Views.ReplayerVRView.bsml")]
    internal class ReplayerVRViewController : MonoBehaviour
    {
        [Inject] private readonly VRControllersManager _vrControllersManager;
        [Inject] private readonly ReplayerCameraController _cameraController;
        [Inject] private readonly DiContainer _container;

        [UIValue("toolbar")] private Toolbar _toolbar;
        [UIValue("floating-controls")] private FloatingControls _floatingControls;

        private FloatingScreen _floating;

        private void Start()
        {
            _floatingControls = ReeUIComponentV2.Instantiate<FloatingControls>(transform);
            _toolbar = ReeUIComponentV2WithContainer.InstantiateInContainer<Toolbar>(_container, transform);

            var container = new GameObject("Container");
            var viewContainer = new GameObject("ViewContainer");
            viewContainer.transform.SetParent(container.transform, false);

            _floating = FloatingScreen.CreateFloatingScreen(new Vector2(100, 55), true, Vector3.zero, Quaternion.identity);
            _floating.ParseInObjectHierarchy(BSMLUtility.ReadViewDefinition<ReplayerVRViewController>(), this);

            _floating.transform.SetParent(viewContainer.transform, false);
            _floating.HandleSide = FloatingScreen.Side.Bottom;
            _floating.HighlightHandle = true;
            _floating.handle.transform.localPosition = new Vector3(11, -23.5f, 0);
            _floating.handle.transform.localScale = new Vector3(20, 3.67f, 3.67f);

            _floatingControls.Floating = _floating;
            _floatingControls.Head = _cameraController.Camera.transform;

            _vrControllersManager.AttachToTheNode(XRNode.GameController, container.transform);
            _vrControllersManager.ShowMenuControllers();
        }
    }
}
