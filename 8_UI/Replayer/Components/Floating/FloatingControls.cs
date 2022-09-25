using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components
{
    [SerializeAutomatically]
    internal class FloatingControls : ReeUIComponentV2
    {
        public FloatingScreen Floating
        {
            get => _viewFloating;
            set
            {
                _viewFloating = value;
                _poseListener.TransformToListen = _viewFloating.transform;
                _resetter.Transform = _viewFloating.transform;

                LoadSettings();
            }
        }
        public Transform Head
        {
            get => _head;
            set
            {
                _head = value;
                _resetFloating.transform.SetParent(_head, false);
                _resetFloating.transform.localPosition = new Vector3(0, 0, 0.7f);
            }
        }
        private FloatingConfig _Config => ConfigFileData.Instance.FloatingConfig;

        [UIValue("pin-button")] private ToggleButton _pinButton;
        [UIValue("align-button")] private SimpleButton _alignButton;
        [UIObject("spacer-one")] private GameObject _spacerOne; 
        [UIObject("spacer-two")] private GameObject _spacerTwo; 

        private TransformListener _poseListener;
        private FloatingResetter _resetter;
        private FloatingScreen _viewFloating;
        private FloatingScreen _resetFloating;
        private Transform _head;

        protected override void OnInstantiate()
        {
            _pinButton = Instantiate<ToggleButton>(transform);
            _alignButton = Instantiate<SimpleButton>(transform);
        }
        protected override void OnInitialize()
        {
            _pinButton.OnToggle += HandlePinToggled;
            _pinButton.DisabledSprite = BSMLUtility.LoadSprite("#pin-icon");
            _pinButton.EnabledColor = Color.cyan;

            _alignButton.OnClick += HandleAlignPressed;
            _alignButton.Sprite = BSMLUtility.LoadSprite("#align-icon");
            _alignButton.HighlightedColor = Color.cyan;

            _resetFloating = FloatingScreen.CreateFloatingScreen(new Vector2(6, 6), false, Vector3.zero, Quaternion.identity);
            _resetFloating.GetComponent<BaseRaycaster>().TryDestroy();

            _resetter = _resetFloating.gameObject.AddComponent<FloatingResetter>();
            _resetter.resetPose = new Pose(ConfigDefaults.FloatingConfig.Position, ConfigDefaults.FloatingConfig.Rotation);

            _poseListener = gameObject.AddComponent<TransformListener>();
            _poseListener.PoseChangedEvent += HandlePoseChanged;
            _poseListener.StartListening();
        }
        private void LoadSettings()
        {
            _viewFloating.transform.SetLocalPositionAndRotation(_Config.Position, _Config.Rotation);
            _pinButton.Toggle(_Config.IsPinned);
        }

        private void HandlePinToggled(bool pin)
        {
            _Config.IsPinned = pin;
            Floating.handle.gameObject.SetActive(!pin);
            _spacerOne.SetActive(!pin);
            _spacerTwo.SetActive(!pin);
        }
        private void HandleAlignPressed()
        {
            var pos = new Vector3();
            var rot = new Vector3();
            for (int i = 0; i <= 2; i++)
            {
                pos[i] = MathUtils.GetClosestCoordinate(_viewFloating.transform.localPosition[i], _Config.GridPosIncrement);
                rot[i] = MathUtils.GetClosestCoordinate(_viewFloating.transform.localEulerAngles[i], _Config.GridRotIncrement);
            }
            _viewFloating.transform.SetLocalPositionAndRotation(pos, Quaternion.Euler(rot));
        }
        private void HandlePoseChanged(Pose pose)
        {
            _Config.Position = _viewFloating.transform.localPosition;
            _Config.Rotation = _viewFloating.transform.localRotation;
        }
    }
}
