using System;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ReplayerSettingsMenu : ReeUIComponentV2 {
        #region Events

        public event Action? DeleteAllButtonClickedEvent;

        #endregion

        #region Replay Saving Settings

        [UIValue("save-after-fail"), UsedImplicitly]
        private bool SaveAfterFail {
            get => GetFlag(ReplaySaveOption.Fail);
            set => WriteFlag(ReplaySaveOption.Fail, value);
        }

        [UIValue("save-after-exit"), UsedImplicitly]
        private bool SaveAfterExit {
            get => GetFlag(ReplaySaveOption.Exit);
            set => WriteFlag(ReplaySaveOption.Exit, value);
        }

        [UIValue("save-zero-score"), UsedImplicitly]
        private bool SaveZeroScore {
            get => GetFlag(ReplaySaveOption.ZeroScore);
            set => WriteFlag(ReplaySaveOption.ZeroScore, value);
        }

        [UIValue("save-practice"), UsedImplicitly]
        private bool SavePractice {
            get => GetFlag(ReplaySaveOption.Practice);
            set => WriteFlag(ReplaySaveOption.Practice, value);
        }
        
        [UIValue("save-ost"), UsedImplicitly]
        private bool SaveOst {
            get => GetFlag(ReplaySaveOption.OST);
            set => WriteFlag(ReplaySaveOption.OST, value);
        }

        [UIValue("override-old"), UsedImplicitly]
        private bool OverrideOld {
            get => ConfigFileData.Instance.OverrideOldReplays;
            set => ConfigFileData.Instance.OverrideOldReplays = value;
        }

        [UIValue("save-replays"), UsedImplicitly]
        private bool SaveReplays {
            get => ConfigFileData.Instance.SaveLocalReplays;
            set {
                ConfigFileData.Instance.SaveLocalReplays = value;
                RefreshToggles();
            }
        }

        private void RefreshToggles() {
            if (!_isInitialized) return;
            var interactable = SaveReplays;
            foreach (var toggle in _togglesContainer
                .GetComponentsInChildren<ToggleSetting>()) {
                toggle.Interactable = interactable;
            }
        }
        
        private static bool GetFlag(ReplaySaveOption option) {
            return ConfigFileData.Instance.ReplaySavingOptions.HasFlag(option);
        }

        private static void WriteFlag(ReplaySaveOption option, bool value) {
            if (value) ConfigFileData.Instance.ReplaySavingOptions |= option;
            else ConfigFileData.Instance.ReplaySavingOptions &= ~option;
        }

        #endregion

        #region UI Components

        [UIValue("replayer-settings"), UsedImplicitly]
        private ReplayerSettingsPanel _settingsPanel = null!;

        [UIObject("replay-saving-settings")]
        private readonly GameObject _replaySavingSettings = null!;

        [UIComponent("toggles-container")]
        private readonly Transform _togglesContainer = null!;
        
        [UIComponent("save-replays-container")]
        private readonly Transform _saveReplaysContainer = null!;

        [UIComponent("delete-all-button")]
        private readonly Transform _deleteAllButton = null!;

        [UIComponent("deletion-modal")]
        private readonly ModalView _deletionModal = null!;

        [UIComponent("deletion-modal-text")]
        private readonly TextMeshProUGUI _deletionModalText = null!;

        [UIComponent("deletion-modal-delete-button")]
        private readonly Transform _deletionModalDeleteButton = null!;

        [UIObject("deletion-modal-delete-button-blue")]
        private readonly GameObject _deletionModalDeleteButtonBlue = null!;

        [UIObject("deletion-modal-buttons-container")]
        private readonly GameObject _deletionModalButtonsContainer = null!;

        [UIObject("deletion-modal-loading-container")]
        private readonly GameObject _deletionModalLoadingContainer = null!;

        [UIObject("deletion-modal-finish-container")]
        private readonly GameObject _deletionModalFinishContainer = null!;

        private ImageView? _confirmationButtonBg;
        private ImageView? _confirmationButtonOutline;
        private ImageView? _confirmationButtonBorder;

        private ImageView? _deleteAllButtonBg;
        private ImageView? _deleteAllButtonUnderline;
        private TextMeshProUGUI? _deleteAllButtonText;

        #endregion

        #region Init

        private IReplayManager _replayManager = null!;
        private bool _isInitialized;

        public void Setup(IReplayManager replayManager) {
            _replayManager = replayManager;
        }

        protected override void OnInstantiate() {
            _settingsPanel = Instantiate<ReplayerSettingsPanel>(transform);
        }

        protected override void OnInitialize() {
            foreach (var text in _replaySavingSettings.GetComponentsInChildren<TMP_Text>()) text.fontSizeMax = 3.5f;
            _saveReplaysContainer.localScale = new(0.8f, 0.8f);

            _deleteAllButton.GetComponent<NoTransitionsButton>().selectionStateDidChangeEvent += RefreshDeleteAllButton;
            _deleteAllButton.GetComponent<ButtonStaticAnimations>().enabled = false;
            _deletionModalDeleteButton.GetComponent<NoTransitionsButton>().selectionStateDidChangeEvent += RefreshConfirmationButton;
            _deletionModalDeleteButton.GetComponent<ButtonStaticAnimations>().enabled = false;

            _confirmationButtonBg = _deletionModalDeleteButton.Find("BG").GetComponent<ImageView>();
            _confirmationButtonOutline = _deletionModalDeleteButton.Find("OutlineWrapper/Outline").GetComponent<ImageView>();
            _confirmationButtonBorder = _deletionModalDeleteButton.Find("Border").GetComponent<ImageView>();

            _deleteAllButtonBg = _deleteAllButton.Find("BG").GetComponent<ImageView>();
            _deleteAllButtonUnderline = _deleteAllButton.Find("Underline").GetComponent<ImageView>();
            _deleteAllButtonText = _deleteAllButton.Find("Content/Text").GetComponent<TextMeshProUGUI>();
            
            _isInitialized = true;
            RefreshToggles();
        }

        #endregion

        #region SettingsMenu

        private void RefreshDeleteAllButton(NoTransitionsButton.SelectionState state) {
            _deleteAllButtonBg!.color = Color.red.ColorWithAlpha(state is NoTransitionsButton.SelectionState.Highlighted ? 0.4f : 0.2f);
            _deleteAllButtonUnderline!.color = Color.red;
            _deleteAllButtonText!.color = Color.red;
        }

        #endregion

        #region DeletionModal

        private enum ModalState {
            FirstConfirmation,
            SecondConfirmation,
            ProcessingDeletion,
            Finish
        }

        private const string FirstConfirmationMessage = "Do you really want to delete all local replays?";
        private const string SecondConfirmationMessage = "ALL OF YOUR DATA WILL BE ERASED! Do you REALLY want to proceed?";
        private const string FinishMessage = "Successfully deleted {0} replays";

        private ModalState _modalState;
        private int _deletedReplaysCount;

        private void RefreshConfirmationButton(NoTransitionsButton.SelectionState state) {
            _confirmationButtonBg!.color0 = Color.red;
            _confirmationButtonBg.color1 = Color.red.ColorWithAlpha(0.7f);
            _confirmationButtonOutline!.enabled = state is NoTransitionsButton.SelectionState.Highlighted;
            _confirmationButtonOutline.color = Color.red.ColorWithAlpha(0.5f);
            _confirmationButtonBorder!.color = Color.red;
        }

        private void SetupModal(ModalState state) {
            var fos = state is ModalState.FirstConfirmation or ModalState.SecondConfirmation;
            _deletionModalButtonsContainer.SetActive(fos);
            if (fos) {
                var first = state is ModalState.FirstConfirmation;
                _deletionModalDeleteButtonBlue.SetActive(first);
                _deletionModalDeleteButton.gameObject.SetActive(!first);
            }
            _deletionModalText.gameObject.SetActive(state is not ModalState.ProcessingDeletion);
            _deletionModalText.color = state is not ModalState.SecondConfirmation ? Color.white : Color.red;
            _deletionModalText.text = state switch {
                ModalState.FirstConfirmation => FirstConfirmationMessage,
                ModalState.SecondConfirmation => SecondConfirmationMessage,
                ModalState.Finish => string.Format(FinishMessage, _deletedReplaysCount),
                _ => _deletionModalText.text
            };
            _deletionModalLoadingContainer.SetActive(state is ModalState.ProcessingDeletion);
            _deletionModalFinishContainer.SetActive(state is ModalState.Finish);
            _modalState = state;
        }

        #endregion

        #region Deletion

        private CancellationTokenSource? _cancellationTokenSource;

        private async Task DeleteAllReplaysAsync() {
            if (_replayManager is null) throw new UninitializedComponentException();
            SetupModal(ModalState.ProcessingDeletion);
            _cancellationTokenSource = new();
            var removedPaths = await _replayManager.DeleteAllReplaysAsync(_cancellationTokenSource.Token);
            _deletedReplaysCount = removedPaths?.Length ?? 0;
            SetupModal(ModalState.Finish);
            _cancellationTokenSource = null;
        }

        #endregion

        #region Callbacks

        [UIAction("cancel-replays-deletion"), UsedImplicitly]
        private void HandleCancelDeletionButtonClicked() {
            _cancellationTokenSource?.Cancel();
            _deletionModal.Hide(true);
        }

        [UIAction("open-deletion-modal"), UsedImplicitly]
        private void HandleDeletionModalButtonClicked() {
            DeleteAllButtonClickedEvent?.Invoke();
            SetupModal(ModalState.FirstConfirmation);
            _deletionModal.Show(true, true);
        }

        [UIAction("delete-replays"), UsedImplicitly]
        private void HandleDeleteButtonClicked() {
            switch (_modalState) {
                case ModalState.FirstConfirmation:
                    SetupModal(ModalState.SecondConfirmation);
                    break;
                case ModalState.SecondConfirmation:
                    _ = DeleteAllReplaysAsync();
                    break;
            }
        }

        #endregion
    }
}