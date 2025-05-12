using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class ReplaySettingsView : ReactiveComponent {
        #region DeletionModal

        private class DeletionModal : ModalBase {
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
                    DeletionStage.Warning2 => "<color=red>YOU WON'T BE ABLE TO RECOVER THE DATA! Do you REALLY want to proceed?",
                    DeletionStage.Finish   => $"Successfully deleted {_deletedReplaysCount} replays.",
                    _                      => _messageLabel.Text
                };
                if (stage is DeletionStage.Warning2) {
                    _okButton.Color = Color.red;
                } else {
                    _okButton.Color = BeatSaberStyle.PrimaryButtonColor;
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
                return new Background {
                    Children = {
                        new Label {
                                EnableWrapping = true
                            }
                            .AsFlexItem(flexGrow: 1f, size: new() { y = "auto" })
                            .Bind(ref _messageLabel),

                        new Layout {
                            Children = {
                                //ok button
                                new BsPrimaryButton {
                                        Text = "OK",
                                        OnClick = HandleOkButtonClicked,
                                        Skew = 0f
                                    }
                                    .AsFlexItem(flexGrow: 1f)
                                    .Bind(ref _okButton),

                                //cancel button
                                new BsButton {
                                        Text = "Cancel",
                                        OnClick = () => CloseInternal(),
                                        Skew = 0f
                                    }
                                    .AsFlexItem(flexGrow: 1f)
                                    .Bind(ref _cancelButton)
                            }
                        }.AsFlexGroup(padding: 1f, gap: 1f).AsFlexItem(size: new() { y = 8f })
                    }
                }.AsBlurBackground().AsFlexGroup(
                    direction: FlexDirection.Column,
                    padding: 1f
                ).AsFlexItem(size: new() { x = 58f, y = 24f }).Use();
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
                        _deletionStage++;
                        _deletedReplaysCount = ReplayManager.DeleteAllReplays();
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

        #region Construct

        private DeletionModal _deletionModal = null!;

        protected override GameObject Construct() {
            new DeletionModal()
                .WithAlphaAnimation(() => Canvas!.gameObject)
                .WithJumpAnimation()
                .WithAnchor(this, RelativePlacement.Center)
                .Bind(ref _deletionModal);
            
            return new Layout {
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

                    //delete all button
                    new BsPrimaryButton {
                            Text = "Delete All Replays",
                            Color = Color.red
                        }
                        .WithModal(_deletionModal)
                        .AsFlexItem(
                            size: new() { x = 30f, y = 7f },
                            alignSelf: Align.Center
                        )
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.FlexStart,
                alignItems: Align.Stretch,
                gap: 1f
            ).Use();
        }

        #endregion
    }
}