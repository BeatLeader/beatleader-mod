using BeatLeader.Models;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Yoga;

namespace BeatLeader.UI.Hub {
    internal class TagDeletionDialog : DialogBase {
        #region Setup

        public ReplayTag? Tag {
            get => _tag.Value;
            set => _tag.Value = value;
        }

        #endregion

        #region Construct

        private State<ReplayTag?> _tag = null!;

        [StateGen]
        protected override IReactiveComponent ConstructDialogContent() {
            _tag = Remember<ReplayTag?>(null);

            return new Label {
                FlexItem = {
                    Size = new() { x = 60.pt, y = 12.pt }
                },

                EnableWrapping = true,
                sText = _tag.Map(static x => $"Do you really want to delete {x?.Name}?")
            };
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            Title = "Delete Tag";

            OkButton.OnClick = HandleOkButtonClicked;
            CancelButton.OnClick = () => IsPushed = false;
        }

        #endregion

        #region Callbacks

        private void HandleOkButtonClicked() {
            IsPushed = false;

            if (Tag != null) {
                ReplayMetadataManager.DeleteTag(Tag.Name);
            }
        }

        #endregion
    }
}