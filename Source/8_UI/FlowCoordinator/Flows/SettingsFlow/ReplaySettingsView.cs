using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class ReplaySettingsView : ReactiveComponent {
        #region DeletionModal

        private static BsPrimaryButtonColors CreatePrimaryButtonColors(Color color) {
            Color.RGBToHSV(color, out var h, out var s, out var v);
            var highlightColor = Color.HSVToRGB(h, s, v * 1.2f);
            var colors = BeatSaberStyle.BsPrimaryButtonColors;

            return colors with {
                BackgroundColors = colors.BackgroundColors with {
                    GradientColors0 = new ColorSet {
                        Color = color,
                        HoveredColor = highlightColor
                    },
                    GradientColors1 = new ColorSet {
                        Color = color.ColorWithAlpha(0.5f),
                        HoveredColor = highlightColor.ColorWithAlpha(0.5f)
                    }
                },
                BorderColors = new ColorSet {
                    Color = highlightColor.ColorWithAlpha(0.7f)
                },
                OutlineColors = new ColorSet {
                    Color = highlightColor.ColorWithAlpha(0.3f)
                }
            };
        }

        private static readonly BsPrimaryButtonColors redPrimaryButtonColors = CreatePrimaryButtonColors(Color.red);

        private class DeletionModal : ModalBase {
            #region Visuals

            private enum DeletionStage {
                Warning1,
                Warning2,
                Finish
            }

            private State<DeletionStage> _stage = null!;
            private int _deletedReplaysCount;

            private string GetMessage(DeletionStage stage) {
                return stage switch {
                    DeletionStage.Warning1 => "This action will delete ALL of your local replays!",
                    DeletionStage.Warning2 => "<color=red>YOU WON'T BE ABLE TO RECOVER THE DATA! Do you REALLY want to proceed?",
                    _                      => $"Successfully deleted {_deletedReplaysCount} replays."
                };
            }

            #endregion

            #region Construct

            [StateGen]
            protected override IReactiveComponent ConstructContent() {
                _stage = Remember(DeletionStage.Warning1);

                return new Background {
                    FlexController = {
                        FlexDirection = FlexDirection.Column,
                        Padding = 1f
                    },

                    FlexItem = {
                        Size = new() { x = 58f, y = 24f }
                    },

                    Children = {
                        new Label {
                            FlexItem = {
                                FlexGrow = 1f,
                                Size = new() { y = "auto" }
                            },

                            EnableWrapping = true,
                            sText = _stage.Map(GetMessage)
                        },

                        new Layout {
                            FlexController = {
                                Padding = 1f,
                                Gap = 1f
                            },

                            FlexItem = {
                                Size = new() { y = 8f }
                            },

                            Children = {
                                //ok button
                                new BsPrimaryTextButton {
                                    FlexItem = {
                                        FlexGrow = 1f,
                                        Size = new() { y = YogaValue.Auto }
                                    },

                                    Text = "OK",
                                    OnClick = HandleOkButtonClicked,
                                    Skew = 0f,
                                    sColors = _stage.Map(x => x is DeletionStage.Warning2 ? redPrimaryButtonColors : BeatSaberStyle.BsPrimaryButtonColors)
                                },

                                //cancel button
                                new BsTextButton {
                                    FlexItem = {
                                        FlexGrow = 1f,
                                        Size = new() { y = YogaValue.Auto }
                                    },

                                    Text = "Cancel",
                                    OnClick = () => IsPushed = false,
                                    Skew = 0f,
                                    sEnabled = _stage.Map(x => x is not DeletionStage.Finish)
                                }
                            }
                        }
                    }
                }.AsBlurBackground();
            }

            protected override void OnInitialize() {
                OnPushed = () => _stage.Value = DeletionStage.Warning1;
            }

            #endregion

            #region Callbacks

            private void HandleOkButtonClicked() {
                switch (_stage.Value) {
                    case DeletionStage.Warning1:
                        _stage.Value = DeletionStage.Warning2;
                        break;
                    case DeletionStage.Warning2:
                        _deletedReplaysCount = ReplayManager.DeleteAllReplays();
                        _stage.Value = DeletionStage.Finish;
                        break;
                    case DeletionStage.Finish:
                        IsPushed = false;
                        break;
                }
            }

            #endregion
        }

        #endregion

        #region ReplaySaveOptions

        private static bool GetReplaySaveFlag(ReplaySaveOption option) {
            return ConfigFileData.Instance.ReplaySavingOptions.HasFlag(option);
        }

        private static void WriteReplaySaveFlag(ReplaySaveOption option, bool value) {
            if (value) {
                ConfigFileData.Instance.ReplaySavingOptions |= option;
            } else {
                ConfigFileData.Instance.ReplaySavingOptions &= ~option;
            }
        }

        #endregion

        private ReplaySaveOption _initialReplaySavingOptions;
        private bool _initialOverrideOldReplays;

        public void CancelSelection() {
            ConfigFileData.Instance.ReplaySavingOptions = _initialReplaySavingOptions;
            ConfigFileData.Instance.OverrideOldReplays = _initialOverrideOldReplays;

            _saveOnFailToggle.Toggled = GetReplaySaveFlag(ReplaySaveOption.Fail);
            _saveOnExitToggle.Toggled = GetReplaySaveFlag(ReplaySaveOption.Exit);
            _saveOnPracticeToggle.Toggled = GetReplaySaveFlag(ReplaySaveOption.Practice);
            _keepLatestToggle.Toggled = ConfigFileData.Instance.OverrideOldReplays;
        }

        #region Construct

        private DeletionModal _deletionModal = null!;
        private BsToggle _saveOnFailToggle = null!;
        private BsToggle _saveOnExitToggle = null!;
        private BsToggle _saveOnPracticeToggle = null!;
        private BsToggle _keepLatestToggle = null!;

        protected override GameObject Construct() {
            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    JustifyContent = Justify.FlexStart,
                    AlignItems = Align.Stretch,
                    Gap = 1f
                },

                Children = {
                    //fail toggle
                    new BsToggle {
                            Toggled = GetReplaySaveFlag(ReplaySaveOption.Fail),
                            OnToggleStateChanged = x => WriteReplaySaveFlag(ReplaySaveOption.Fail, x)
                        }
                        .Bind(ref _saveOnFailToggle)
                        .InNamedRail("Save On Fail"),

                    //exit toggle
                    new BsToggle {
                            Toggled = GetReplaySaveFlag(ReplaySaveOption.Exit),
                            OnToggleStateChanged = x => WriteReplaySaveFlag(ReplaySaveOption.Exit, x)
                        }
                        .Bind(ref _saveOnExitToggle)
                        .InNamedRail("Save On Exit"),

                    new BsToggle {
                            Toggled = GetReplaySaveFlag(ReplaySaveOption.Practice),
                            OnToggleStateChanged = x => WriteReplaySaveFlag(ReplaySaveOption.Practice, x)
                        }
                        .Bind(ref _saveOnPracticeToggle)
                        .InNamedRail("Save On Practice"),

                    //override old toggle
                    new BsToggle {
                            Toggled = ConfigFileData.Instance.OverrideOldReplays,
                            OnToggleStateChanged = x => ConfigFileData.Instance.OverrideOldReplays = x
                        }
                        .Bind(ref _keepLatestToggle)
                        .InNamedRail("Keep Latest Only"),

                    //delete all button
                    new BsPrimaryTextButton {
                        FlexItem = {
                            Size = new() { x = 30f, y = 7f },
                            AlignSelf = Align.Center
                        },

                        Text = "Delete All Replays",
                        Colors = redPrimaryButtonColors,
                        OnClick = () => _deletionModal.IsPushed = true
                    },

                    new DeletionModal()
                        .WithPlacement(RelativePlacement.Center)
                        .Bind(ref _deletionModal)
                }
            }.Use();
        }

        protected override void OnInitialize() {
            _deletionModal.PlacementAnchor = ContentTransform;
            _initialReplaySavingOptions = ConfigFileData.Instance.ReplaySavingOptions;
            _initialOverrideOldReplays = ConfigFileData.Instance.OverrideOldReplays;
        }

        #endregion
    }
}