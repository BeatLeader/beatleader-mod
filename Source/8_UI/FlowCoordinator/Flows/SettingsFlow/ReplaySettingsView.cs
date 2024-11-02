using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class ReplaySettingsView : ReactiveComponent {
        #region DeletionModal

        private class DeletionModal : ModalComponentBase {
            #region Setup

            private IReplayFileManager? _replayFileManager;

            public void Setup(IReplayFileManager replayFileManager) {
                _replayFileManager = replayFileManager;
            }

            #endregion

            #region Visuals

            private enum DeletionStage {
                Warning1,
                Warning2,
                Finish
            }

            private DeletionStage _deletionStage;
            private int _deletedReplaysCount;

            private void RefreshVisuals(DeletionStage stage) {
                _messageLabel.Text = stage switch {
                    DeletionStage.Warning1 => "This action will delete ALL of your local replays!",
                    DeletionStage.Warning2 => "YOU WON'T BE ABLE TO RECOVER THE DATA! Do you REALLY want to proceed?",
                    DeletionStage.Finish => $"Successfully deleted {_deletedReplaysCount} replays.",
                    _ => _messageLabel.Text
                };
                if (stage is DeletionStage.Warning2) {
                    _okButton.WithAccentColor(Color.red);
                } else {
                    _okButton.Colors = UIStyle.PrimaryButtonColorSet;
                }
                _cancelButton.Enabled = stage is not DeletionStage.Finish;
            }

            protected override void OnOpen(bool finished) {
                _deletionStage = DeletionStage.Warning1;
                RefreshVisuals(DeletionStage.Warning1);
            }

            #endregion

            #region Construct

            protected override bool AllowExternalClose => false;

            private Label _messageLabel = null!;
            private BsPrimaryButton _okButton = null!;
            private BsButton _cancelButton = null!;

            protected override GameObject Construct() {
                return new Image {
                    Children = {
                        new Label {
                                EnableWrapping = true
                            }
                            .AsFlexItem(grow: 1f, size: new() { y = "auto" })
                            .Bind(ref _messageLabel),
                        //
                        new Dummy {
                            Children = {
                                //ok button
                                new BsPrimaryButton {
                                        OnClick = HandleOkButtonClicked
                                    }
                                    .WithLabel("OK")
                                    .AsFlexItem(grow: 1f)
                                    .Bind(ref _okButton),
                                //cancel button
                                new BsButton {
                                        OnClick = () => CloseInternal()
                                    }
                                    .WithLabel("Cancel")
                                    .AsFlexItem(grow: 1f)
                                    .Bind(ref _cancelButton)
                            }
                        }.AsFlexGroup(padding: 1f, gap: 1f).AsFlexItem(size: new() { y = 8f })
                    }
                }.AsBlurBackground().AsFlexGroup(
                    direction: FlexDirection.Column,
                    padding: 1f
                ).WithSizeDelta(58f, 24f).Use();
            }

            protected override void OnInitialize() {
                base.OnInitialize();
                Content.GetOrAddComponent<CanvasGroup>().ignoreParentGroups = true;
            }

            #endregion

            #region Callbacks

            private void HandleOkButtonClicked() {
                switch (_deletionStage) {
                    case DeletionStage.Warning1:
                        _deletionStage++;
                        break;
                    case DeletionStage.Warning2:
                        if (_replayFileManager == null) return;
                        _deletionStage++;
                        _deletedReplaysCount = _replayFileManager.DeleteAllReplays();
                        break;
                    case DeletionStage.Finish:
                        CloseInternal();
                        break;
                }
                RefreshVisuals(_deletionStage);
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

        #region Setup

        public void Setup(IReplayFileManager replayFileManager) {
            _deletionModal.Setup(replayFileManager);
        }

        #endregion

        #region Construct

        private DeletionModal _deletionModal = null!;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new DeletionModal()
                        .WithAlphaAnimation(() => Canvas!.gameObject)
                        .WithJumpAnimation()
                        .WithAnchor(() => ContentTransform, RelativePlacement.Center)
                        .Bind(ref _deletionModal),
                    //
                    new ReeWrapperV2<ReplayerSettingsPanel>()
                        .WithRectExpand()
                        .InBlurBackground()
                        .AsFlexItem(size: new() { x = 45f, y = 14f }),
                    //toggles
                    new Image {
                        Children = {
                            //fail toggle
                            new Toggle()
                                .With(x => x.SetActive(GetReplaySaveFlag(ReplaySaveOption.Fail), false))
                                .WithListener(
                                    x => x.Active,
                                    x => WriteReplaySaveFlag(ReplaySaveOption.Fail, x)
                                )
                                .InNamedRail("Save On Fail"),

                            //exit toggle
                            new Toggle()
                                .With(x => x.SetActive(GetReplaySaveFlag(ReplaySaveOption.Exit), false))
                                .WithListener(
                                    x => x.Active,
                                    x => WriteReplaySaveFlag(ReplaySaveOption.Exit, x)
                                )
                                .InNamedRail("Save On Exit"),

                            //override old toggle
                            new Toggle()
                                .With(x => x.SetActive(ConfigFileData.Instance.OverrideOldReplays, false))
                                .WithListener(
                                    x => x.Active,
                                    x => ConfigFileData.Instance.OverrideOldReplays = x
                                )
                                .InNamedRail("Keep Latest Only"),
                        }
                    }.AsBlurBackground().AsFlexGroup(
                        direction: FlexDirection.Column,
                        gap: 1f,
                        padding: 2f
                    ).AsFlexItem(size: new() { x = 50f }),
                    //delete all button
                    new BsPrimaryButton()
                        .WithModal(_deletionModal)
                        .WithLabel("Delete All Replays")
                        .WithAccentColor(Color.red)
                        .AsFlexItem(
                            size: new() { x = 30f },
                            alignSelf: Align.Center
                        )
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.FlexStart,
                alignItems: Align.Center,
                gap: 1f
            ).Use();
        }

        #endregion
    }
}