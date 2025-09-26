using System.Linq;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Utils;
using HMUI;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class BeatLeaderNewsViewController : ViewController {
        #region Setup

        private NewsViewPanel _newsPanel = null!;

        private void Awake() {
            _newsPanel = new NewsViewPanel();
            _newsPanel.Use(transform);

            LoadPlatformEvents().RunCatching();

            UpdateScreen();
        }

        private void OnEnable() {
            if (!_initialized) return;
            UpdateScreen();
        }

        private void OnDisable() {
            RevertScreenChanges();
        }

        public override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
#pragma warning disable CS0618 // Type or member is obsolete
            ReeModalSystem.CloseAll();
#pragma warning restore CS0618 // Type or member is obsolete
            gameObject.SetActive(false);
        }

        #endregion

        #region Data

        private async Task LoadPlatformEvents() {
            var events = await PlatformEventsRequest.Send().Join();
            var eventsList = events.Result?.data;

            _newsPanel.SetEvents(events.RequestState, eventsList);

            await Task.Delay(2000);
            var specialEvent = eventsList?.FirstOrDefault(x => x.IsHappening());
            if (specialEvent != null) {
                var req = await PlatformEventStatusRequest.Send(specialEvent.id).Join();
                // TODO: remove when the server gets fixed
                req.Result.eventDescription = specialEvent;

                _newsPanel.SetSpecialEvent(req.Result);
            }
        }

        #endregion

        #region Screen Changes

        private static Vector2 TargetScreenSize => new Vector2(186, 80);

        private RectTransform _screenTransform = null!;
        private Vector2 _originalScreenSize;
        private bool _initialized;

        private bool LazyInitializeScreen() {
            if (_initialized) return true;
            if (screen == null) return false;
            _screenTransform = screen.GetComponent<RectTransform>();
            _originalScreenSize = _screenTransform.sizeDelta;
            _initialized = true;
            return true;
        }

        private void UpdateScreen() {
            if (!LazyInitializeScreen()) return;
            _screenTransform.sizeDelta = TargetScreenSize;
        }

        private void RevertScreenChanges() {
            if (!LazyInitializeScreen()) return;
            _screenTransform.sizeDelta = _originalScreenSize;
        }

        #endregion
    }
}