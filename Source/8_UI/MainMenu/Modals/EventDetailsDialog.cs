﻿using BeatLeader.API.Methods;
using BeatLeader.Interop;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class EventDetailsDialog : AbstractReeModal<PlatformEvent> {
        #region Components

        [UIObject("loading-container"), UsedImplicitly] 
        private GameObject _loadingContainer = null!;

        [UIObject("event-container"), UsedImplicitly] 
        private GameObject _eventContainer = null!;

        [UIComponent("event-description"), UsedImplicitly]
        private TMP_Text _eventDescription = null!;

        private bool _downloadButtonActive;

        [UIValue("download-button-active"), UsedImplicitly]
        private bool DownloadButtonActive {
            get => _downloadButtonActive;
            set {
                if (_downloadButtonActive.Equals(value)) return;
                _downloadButtonActive = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Events

        protected override void OnContextChanged() {
            _eventDescription.SetText(Context.description ?? "");
            DownloadButtonActive = Context.downloadable;
        }

        protected override void OnResume() {
            _eventContainer.SetActive(true);
            _loadingContainer.SetActive(false);
        }

        [UIAction("download-button-click"), UsedImplicitly]
        private void HandleDownloadButtonClicked() {
            _eventContainer.SetActive(false);
            _loadingContainer.SetActive(true);
            offClickCloses = false;

            StopAllCoroutines();
            StartCoroutine(PlaylistRequest.SendRequest(Context.playlistId.ToString(), OnSuccess, OnFail));
            return;

            async void OnSuccess(byte[] bytes) {
                var filename = Context.name.Replace(" ", "_");
                FileManager.DeletePlaylist(filename);

                if (FileManager.TrySaveRankedPlaylist(filename, bytes)) {
                    PlaylistsLibInterop.TryRefreshPlaylists(true);
                    SongCore.Loader.Instance.RefreshSongs(false);
                    await Task.Delay(TimeSpan.FromSeconds(2));

                    var playlist = PlaylistsLibInterop.TryFindPlaylist(filename);
                    if (playlist != null) {
                        BeatmapKey beatmapKey = new BeatmapKey();
                        var x = new LevelSelectionFlowCoordinator.State(
                            SelectLevelCategoryViewController.LevelCategory.CustomSongs, 
                            playlist, 
                            in beatmapKey, 
                            null);
                        FindObjectOfType<SoloFreePlayFlowCoordinator>().Setup(x);
                        Close();
                        (GameObject.Find("SoloButton") ?? GameObject.Find("Wrapper/BeatmapWithModifiers/BeatmapSelection/EditButton"))
				        ?.GetComponent<NoTransitionsButton>()?.onClick.Invoke();
                    }
                }

                offClickCloses = true;
                Close();
            }

            void OnFail(string reason) {
                Plugin.Log.Debug($"Event {Context.name} playlist update failed: {reason}");
                offClickCloses = true;
                Close();
            }
        }

        #endregion
    }
}