using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.BSML_Addons;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.UI.Replayer {
    [BSMLComponent(Namespace = "Replayer")]
    //TODO: remove attribute after deprecated components deletion
    [ViewDefinition("NewSettings.ReplayerSettingsPanel")]
    internal class ReplayerSettingsPanel : ReeUIComponentV3<ReplayerSettingsPanel> {
        #region UI Components

        [UIObject("view-selector-container"), UsedImplicitly]
        private GameObject _segmentedControlContainer = null!;

        [UIValue("view-selector"), UsedImplicitly]
        private ViewSegmentedControl _viewSegmentedControl = null!;

        [UIValue("view-container"), UsedImplicitly]
        private ViewSegmentedControlContainer _viewSegmentedControlContainer = null!;

        [UIComponent("camera-view"), UsedImplicitly]
        private SettingsCameraView _cameraView = null!;

        [UIComponent("avatar-view"), UsedImplicitly]
        private SettingsAvatarView _avatarView = null!;
        
        [UIComponent("ui-view"), UsedImplicitly]
        private SettingsUIView _uiView = null!;
        
        #endregion

        #region Setup

        public void Setup(
            ReplayerSettings settings,
            ICameraController cameraController,
            IVirtualPlayerBodySpawner bodySpawner,
            ILayoutEditor layoutEditor
        ) {
            _cameraView.Setup(cameraController, settings.CameraSettings!);
            _cameraView.ReloadCameraViewParams();
            _avatarView.Setup(bodySpawner, settings.BodySettings);
            _uiView.Setup(layoutEditor);
        }

        protected override void OnInstantiate() {
            _viewSegmentedControl = ViewSegmentedControl.Instantiate(transform);
            _viewSegmentedControl.InheritSize = true;
            _viewSegmentedControl.Pad = new(2, 2, 2, 2);
            _viewSegmentedControlContainer = ViewSegmentedControlContainer.Instantiate(transform);
            _viewSegmentedControlContainer.InheritSize = true;
            _viewSegmentedControl.SetDataSource(_viewSegmentedControlContainer);
        }

        protected override void OnInitialize() {
            _cameraView.cameraViewParams.AddRange(
                new SettingsCameraView.ICameraViewParams[] {
                    PlayerViewCameraParams.Instantiate(transform),
                    FlyingViewCameraParams.Instantiate(transform)
                }
            );

            _viewSegmentedControlContainer.AddViewsFromChildren();
            _viewSegmentedControl.Reload();

            var img = Content.AddComponent<AdvancedImageView>();
            img.sprite = BundleLoader.WhiteBG;
            img.material = GameResources.UIFogBackgroundMaterial;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 7f;
            Content.AddComponent<Mask>();

            var selectorImg = _segmentedControlContainer.AddComponent<AdvancedImageView>();
            selectorImg.sprite = BundleLoader.WhiteBG;
            selectorImg.material = GameResources.UINoGlowMaterial;
            selectorImg.type = Image.Type.Sliced;
            selectorImg.pixelsPerUnitMultiplier = 100f;
            selectorImg.color = new(0.1f, 0.1f, 0.1f, 1f);
        }

        #endregion

        #region ViewSegmentedControl

        public enum SettingsView {
            CameraView,
            AvatarView,
            UIView,
            OtherView
        }

        public interface ISegmentedControlView : ISegmentedControlView<SettingsView, (string, Sprite)> { }

        private class ViewSegmentedControl : ReeSegmentedControlComponentBase<ViewSegmentedControl, ViewSegmentedControl.Cell, SettingsView, (string, Sprite)> {
            public class Cell : ReeSegmentedControlCell<Cell, SettingsView, (string, Sprite)> {
                #region Setup

                private Button _button = null!;

                protected override void Init((string, Sprite) value) {
                    var (placeholder, icon) = value;
                    _button.Icon.sprite = icon;
                    _button.Text.text = placeholder;
                }

                protected override GameObject Construct(Transform parent) {
                    _button = Button.Instantiate(parent);
                    _button.BaseScale = Vector3.one;
                    _button.InheritSize = true;
                    _button.Sticky = true;
                    _button.StateChangedEvent += HandleButtonStateChanged;
                    return _button.Content;
                }

                protected override void OnStart() {
                    ContentTransform.parent.localScale = Vector3.one;
                }

                #endregion

                #region Button

                private class Button : ColoredButtonComponentBase<Button> {
                    #region Setup

                    public AdvancedImageView Icon { get; private set; } = null!;
                    public TMP_Text Text { get; private set; } = null!;

                    protected override LayoutGroupType LayoutGroupDirection => LayoutGroupType.Vertical;

                    protected override void OnContentConstruct(Transform parent) {
                        LayoutGroup.childControlHeight = false;
                        LayoutGroup.childForceExpandHeight = false;
                        LayoutGroup.childAlignment = TextAnchor.MiddleCenter;

                        var iconGo = parent.gameObject.CreateChild("Icon");
                        var iconFitter = iconGo.AddComponent<AspectRatioFitter>();
                        iconFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
                        Icon = iconGo.AddComponent<AdvancedImageView>();
                        Icon.material = BundleLoader.UIAdditiveGlowMaterial;
                        Icon.preserveAspect = true;

                        var textGo = parent.gameObject.CreateChild("Text");
                        var textFitter = textGo.AddComponent<ContentSizeFitter>();
                        textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                        Text = textGo.AddComponent<TextMeshProUGUI>();
                        Text.material = BundleLoader.UIAdditiveGlowMaterial;
                        Text.alignment = TextAlignmentOptions.Center;
                        Text.fontSize = 3.6f;
                        Text.enableWordWrapping = false;
                    }

                    #endregion

                    #region ApplyColor

                    protected override void ApplyColor(Color color, float t) {
                        Icon.color = color;
                        Text.color = color;
                    }

                    #endregion
                }

                #endregion

                #region Callbacks

                public override void OnCellStateChange(bool selected) {
                    _button.Click(selected);
                }

                private void HandleButtonStateChanged(bool state) {
                    if (!state) {
                        _button.Click(true);
                    }
                    SelectSelf();
                }

                #endregion
            }
        }

        private class ViewSegmentedControlContainer : SegmentedControlContainerBase<ViewSegmentedControlContainer, SettingsView, (string, Sprite)> { }

        #endregion
    }
}