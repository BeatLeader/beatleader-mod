using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using InputUtils = BeatLeader.Utils.InputUtils;

namespace BeatLeader.UI.Replayer {
    internal class SettingsControlsView : ReactiveComponent {
        private readonly SettingsControlsTabs _tabs = new();

        public void Setup(ReplayerControlsSettings settings, ReplayerFloatingUISettings? floatingSettings) {
            _tabs.Setup(settings, floatingSettings);
        }

        protected override GameObject Construct() {
            return _tabs.CreateView().Use();
        }
    }

    internal class SettingsControlsTabs {
        #region Setup

        private ReplayerControlsSettings? _settings;
        private ReplayerFloatingUISettings? _floatingSettings;

        public void Setup(ReplayerControlsSettings settings, ReplayerFloatingUISettings? floatingSettings) {
            _settings = settings;
            _floatingSettings = floatingSettings;

            if (_enabledToggle != null) _enabledToggle.SetActive(settings.Enabled, false, true);
            if (_fpfcToggle != null) _fpfcToggle.SetActive(settings.FpfcControlsEnabled, false, true);
            if (_seekHandDropdown != null) _seekHandDropdown.Select(settings.SeekOnRightHand);
            _seekStepSlider.SetValueSilent(settings.SeekStepSeconds);
            _speedStepSlider.SetValueSilent(settings.SpeedStep * 100f);

            if (floatingSettings == null) return;
            if (_uiHandDropdown != null) _uiHandDropdown.Select(floatingSettings.AttachToRightHand);
            if (_uiOffsetXSlider != null) _uiOffsetXSlider.SetValueSilent(floatingSettings.HandOffset.position.x);
            if (_uiOffsetYSlider != null) _uiOffsetYSlider.SetValueSilent(floatingSettings.HandOffset.position.y);
            if (_uiOffsetZSlider != null) _uiOffsetZSlider.SetValueSilent(floatingSettings.HandOffset.position.z);
            if (_uiScaleSlider != null) _uiScaleSlider.SetValueSilent(floatingSettings.HandScale * 100f);
        }

        #endregion

        #region Construct

        private Toggle _enabledToggle = null!;
        private Toggle _fpfcToggle = null!;
        private TextDropdown<bool> _seekHandDropdown = null!;
        private Slider _seekStepSlider = null!;
        private Slider _speedStepSlider = null!;
        private TextDropdown<bool> _uiHandDropdown = null!;
        private Slider _uiOffsetXSlider = null!;
        private Slider _uiOffsetYSlider = null!;
        private Slider _uiOffsetZSlider = null!;
        private Slider _uiScaleSlider = null!;

        private const string ControllersTab = "Controllers";
        private const string UiOnHandTab = "UIOnHand";

        public Layout CreateView() {
            var tabControl = new TextSegmentedControl<string>();
            var viewContainer = new KeyedContainer<string> {
                Control = tabControl
            };

            AddTabs(tabControl, viewContainer);

            return new Layout {
                Children = {
                    tabControl
                        .AsFlexItem(flexGrow: 1f)
                        .InBackground(
                            color: new(0.1f, 0.1f, 0.1f, 1f),
                            pixelsPerUnit: 7f
                        )
                        .AsFlexGroup()
                        .AsFlexItem(size: new() { y = 6f }),
                    viewContainer.AsFlexItem(flexGrow: 1f)
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                gap: 1f
            );
        }

        public void AddTabs(TextSegmentedControl<string> tabControl, KeyedContainer<string> viewContainer) {
            tabControl.Items.Add(ControllersTab, InputUtils.UsesFPFC ? "Hotkeys" : "Controllers");
            viewContainer.Items[ControllersTab] = CreateControllersTab();

            if (!InputUtils.UsesFPFC) {
                tabControl.Items.Add(UiOnHandTab, "UI on hand");
                viewContainer.Items[UiOnHandTab] = CreateUiOnHandTab();
            }
        }

        private Layout CreateControllersTab() {
            var children = new List<ILayoutItem> { };
            if (InputUtils.UsesFPFC) {
                children.Add(CreateFpfcToggle());
            } else {
                children.Add(CreateEnabledToggle());
                children.Add(CreateSeekHandDropdown());
            }
            children.Add(CreateSeekStepSlider());
            children.Add(CreateSpeedStepSlider());

            return CreateScrollableContent(children);
        }

        private Layout CreateUiOnHandTab() {
            return CreateScrollableContent(new List<ILayoutItem> {
                CreateUiHandDropdown(),
                CreateUiOffsetSlider(0, "UI X offset"),
                CreateUiOffsetSlider(1, "UI Y offset"),
                CreateUiOffsetSlider(2, "UI Z offset"),
                CreateUiScaleSlider()
            });
        }

        private static Layout CreateScrollableContent(List<ILayoutItem> children) {
            var content = new Background()
            .With(x => x.Children.AddRange(children))
            .AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.FlexStart,
                gap: 2f,
                padding: 2f,
                constrainVertical: false
            ).AsBackground(
                color: new(0.1f, 0.1f, 0.1f, 1f)
            ).AsFlexItem(size: new() { x = 100.pct() });

            return new Layout {
                Children = {
                    new ScrollArea {
                        ScrollContent = content
                    }.AsFlexItem(flexGrow: 1f).Export(out var scrollArea),
                    new Scrollbar()
                        .AsFlexItem()
                        .With(x => scrollArea.Scrollbar = x)
                }
            }.AsFlexGroup(gap: 1f);
        }

        private ILayoutItem CreateEnabledToggle() {
            return new Toggle().WithListener(
                x => x.Active,
                x => {
                    if (_settings != null) _settings.Enabled = x;
                }
            ).Bind(ref _enabledToggle).InNamedRail("Enable VR controls");
        }

        private ILayoutItem CreateSeekHandDropdown() {
            return new TextDropdown<bool> {
                Skew = 0f,
                Items = {
                    { false, "Left hand" },
                    { true, "Right hand" }
                }
            }.WithListener(
                x => x.SelectedKey,
                x => {
                    if (_settings != null) _settings.SeekOnRightHand = x;
                }
            ).Bind(ref _seekHandDropdown).InNamedRail("Seek hand");
        }

        private ILayoutItem CreateFpfcToggle() {
            return new Toggle().WithListener(
                x => x.Active,
                x => {
                    if (_settings != null) _settings.FpfcControlsEnabled = x;
                }
            ).Bind(ref _fpfcToggle).InNamedRail("Enable FPFC hotkeys");
        }

        private ILayoutItem CreateUiHandDropdown() {
            return new TextDropdown<bool> {
                Skew = 0f,
                Items = {
                    { false, "Left hand" },
                    { true, "Right hand" }
                }
            }.WithListener(
                x => x.SelectedKey,
                x => {
                    if (_floatingSettings != null) _floatingSettings.AttachToRightHand = x;
                }
            ).Bind(ref _uiHandDropdown).InNamedRail("UI hand");
        }

        private ILayoutItem CreateUiOffsetSlider(int axis, string label) {
            var slider = new Slider {
                ValueRange = new() { Start = axis == 2 ? 0.05f : -0.4f, End = axis == 2 ? 0.6f : 0.4f },
                ValueStep = 0.01f,
                ValueFormatter = x => $"{x:0.00}m"
            }.WithListener(
                x => x.Value,
                x => SetUiHandOffset(axis, x)
            );

            switch (axis) {
                case 0:
                    slider.Bind(ref _uiOffsetXSlider);
                    break;
                case 1:
                    slider.Bind(ref _uiOffsetYSlider);
                    break;
                case 2:
                    slider.Bind(ref _uiOffsetZSlider);
                    break;
            }

            return slider.InNamedRail(label);
        }

        private ILayoutItem CreateUiScaleSlider() {
            return new Slider {
                ValueRange = new() { Start = 25f, End = 150f },
                ValueStep = 5f,
                ValueFormatter = x => $"{x:0}%"
            }.WithListener(
                x => x.Value,
                x => {
                    if (_floatingSettings != null) _floatingSettings.HandScale = x / 100f;
                }
            ).Bind(ref _uiScaleSlider).InNamedRail("UI scale");
        }

        private ILayoutItem CreateSeekStepSlider() {
            return new Slider {
                ValueRange = new() { Start = 1f, End = 30f },
                ValueStep = 1f,
                ValueFormatter = x => $"{x}s"
            }.WithListener(
                x => x.Value,
                x => {
                    if (_settings != null) _settings.SeekStepSeconds = x;
                }
            ).Bind(ref _seekStepSlider).InNamedRail("Seek step");
        }

        private ILayoutItem CreateSpeedStepSlider() {
            return new Slider {
                ValueRange = new() { Start = 5f, End = 50f },
                ValueStep = 5f,
                ValueFormatter = x => $"{x}%"
            }.WithListener(
                x => x.Value,
                x => {
                    if (_settings != null) _settings.SpeedStep = x / 100f;
                }
            ).Bind(ref _speedStepSlider).InNamedRail("Speed step");
        }

        private void SetUiHandOffset(int axis, float value) {
            if (_floatingSettings == null) return;

            var offset = _floatingSettings.HandOffset;
            switch (axis) {
                case 0:
                    offset.position.x = value;
                    break;
                case 1:
                    offset.position.y = value;
                    break;
                case 2:
                    offset.position.z = value;
                    break;
            }
            _floatingSettings.HandOffset = offset;
        }

        #endregion
    }
}
