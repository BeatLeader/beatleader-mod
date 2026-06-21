using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using UnityEngine;
using InputUtils = BeatLeader.Utils.InputUtils;

namespace BeatLeader.UI.Replayer {
    internal class SettingsControlsView : ReactiveComponent {
        #region Setup

        private ReplayerControlsSettings? _settings;

        public void Setup(ReplayerControlsSettings settings) {
            _settings = settings;

            if (_enabledToggle != null) _enabledToggle.SetActive(settings.Enabled, false, true);
            if (_fpfcToggle != null) _fpfcToggle.SetActive(settings.FpfcControlsEnabled, false, true);
            if (_seekHandDropdown != null) _seekHandDropdown.Select(settings.SeekOnRightHand);
            _seekStepSlider.SetValueSilent(settings.SeekStepSeconds);
            _speedStepSlider.SetValueSilent(settings.SpeedStep * 100f);
        }

        #endregion

        #region Construct

        private Toggle _enabledToggle = null!;
        private Toggle _fpfcToggle = null!;
        private TextDropdown<bool> _seekHandDropdown = null!;
        private Slider _seekStepSlider = null!;
        private Slider _speedStepSlider = null!;

        protected override GameObject Construct() {
            var children = new List<ILayoutItem> { };
            if (InputUtils.UsesFPFC) {
                children.Add(CreateFpfcToggle());
            } else {
                children.Add(CreateEnabledToggle());
                children.Add(CreateSeekHandDropdown());
            }
            children.Add(CreateSeekStepSlider());
            children.Add(CreateSpeedStepSlider());

            return new Background()
            .With(x => x.Children.AddRange(children))
            .AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.FlexStart,
                gap: 2f,
                padding: 2f
            ).AsBackground(
                color: new(0.1f, 0.1f, 0.1f, 1f)
            ).Use();
        }

        private ILayoutItem CreateEnabledToggle() {
            return new Toggle().WithListener(
                x => x.Active,
                x => {
                    if (_settings != null) _settings.Enabled = x;
                }
            ).Bind(ref _enabledToggle).InNamedRail("Enable stick controls");
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
            ).Bind(ref _seekHandDropdown).InNamedRail("Seek stick");
        }

        private ILayoutItem CreateFpfcToggle() {
            return new Toggle().WithListener(
                x => x.Active,
                x => {
                    if (_settings != null) _settings.FpfcControlsEnabled = x;
                }
            ).Bind(ref _fpfcToggle).InNamedRail("Enable FPFC hotkeys");
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

        #endregion
    }
}
