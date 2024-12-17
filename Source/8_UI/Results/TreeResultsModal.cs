using BeatLeader.API.Methods;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace BeatLeader.Results {
    internal class TreeResultsModal : AbstractReeModal<object> {

        [UIComponent("date-text"), UsedImplicitly] private TMP_Text _dateText = null!;

        GameObject present;
        List<(int, string)> existingOrnamentIds;
        List<(int, string)> ornamentsToShow;
        List<Task<GameObject>> tasks;

        bool active = false;
        bool animating = false;

        protected override void OnInitialize() {
            existingOrnamentIds = TreeMapRequest.treeStatus?.GetOrnamentIds() ?? new();
            ornamentsToShow = null;

            if (TreeMapRequest.treeStatus != null) {
                _dateText.SetText($"December {TreeMapRequest.treeStatus.today.startTime.AsUnixTime().Day}th completed!");
            }
            TreeMapRequest.SendRequest();
            TreeMapRequest.AddStateListener(OnRequestStateChanged);
        }

        protected override void OnDestroy() {
            TreeMapRequest.RemoveStateListener(OnRequestStateChanged);
        }

        protected override void OnResume() {
            if (present != null) {
                Destroy(present);
            }
            active = false;
            animating = false;
            
            tasks = new List<Task<GameObject>>();

            present = Instantiate(BundleLoader.ChristmasPresent, null, false);
            present.transform.SetParent(transform, false);
            
            present.transform.localPosition += Vector3.up * 20f;
        }

        private void OnRequestStateChanged(API.RequestState state, TreeStatus result, string failReason) {
            switch (state) {
                case API.RequestState.Finished: {
                    ornamentsToShow = new List<(int, string)> { (1, null), (4, null), (5, null) };

                    //ornamentsToShow = result.GetOrnamentIds().Where(o => !existingOrnamentIds.Any(e => e.Item1 == o.Item1)).ToList();
                    if (ornamentsToShow.Count() == 0) {
                        Close();
                    }
                    break;
                }
                default:
                    Close();
                    break;
            }
        }

        protected override void OnRootStateChange(bool active) { 
            this.active = active;

            if (active) {
                StartCoroutine(AnimatePresent());
            }
        }
        
        private IEnumerator AnimatePresent() {
            float dropDuration = 0.8f;
            float elapsedTime = 0;
            Vector3 startPos = present.transform.localPosition;
            Vector3 targetPos = startPos - Vector3.up * 20f;

            while (elapsedTime < dropDuration) {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / dropDuration;
                
                // Enhanced bouncy spring effect
                float bounce = Mathf.Sin(progress * Mathf.PI * 4) * (1 - progress) * 1.5f;
                Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
                currentPos.y += bounce;
                
                present.transform.localPosition = currentPos;
                yield return null;
            }

            yield return new WaitUntil(() => ornamentsToShow != null);

            for (var i = 0; i < ornamentsToShow.Count; i++) {
                tasks.Add(ChristmasOrnamentLoader.LoadOrnamentPrefabAsync(ornamentsToShow[i].Item1));
            }

            if (tasks.Count == 0 || tasks.All(t => t.IsCompleted)) {
                yield return new WaitForSeconds(2f);
            } else {
                yield return new WaitUntil(() => Task.WhenAll(tasks).IsCompleted);
            }

            float shakeDuration = 0.3f;
            elapsedTime = 0;
            Vector3 originalPos = present.transform.localPosition;
            
            while (elapsedTime < shakeDuration) {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / shakeDuration;
                
                float shake = Mathf.Sin(progress * Mathf.PI * 8) * (1 - progress) * 0.1f;
                present.transform.localPosition = originalPos + new Vector3(shake, 0, shake);
                
                yield return null;
            }
            
            present.transform.localPosition = originalPos;

            var lid = present.transform.Find("ChristmasPresentLid").gameObject;
            // Animate lid blasting open
            float openDuration = 0.3f;
            elapsedTime = 0;
            Quaternion startRotation = lid.transform.localRotation;
            Quaternion targetRotation = startRotation * Quaternion.Euler(0, 0, 80);

            while (elapsedTime < openDuration) {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / openDuration;
                float angle = Mathf.Lerp(0, 80, progress) + Mathf.Sin(progress * Mathf.PI) * 20;
                lid.transform.localRotation = startRotation * Quaternion.Euler(0, 0, angle);
                yield return null;
            }

            var blast = new GameObject("Blast");
            blast.transform.SetParent(present.transform);
            blast.transform.localPosition = Vector3.zero;
            blast.transform.localScale = Vector3.zero;

            elapsedTime = 0;
            float blastDuration = 0.3f;
            while (elapsedTime < blastDuration) {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / blastDuration;
                float scale = (1 - (progress * progress)) * 3f; // Quadratic falloff
                blast.transform.localScale = new Vector3(scale, scale, scale);
                yield return null;
            }
            Destroy(blast);

            // Create and animate ornaments
            List<GameObject> ornaments = tasks.Select(t => t.Result).ToList();
            foreach (var ornamentPrefab in ornaments) {
                var ornament = Instantiate(ornamentPrefab, null, false);
                ornament.transform.SetParent(transform);
                ornament.transform.position = present.transform.position;
                float angle = UnityEngine.Random.Range(230f, 310f);
                float radius = UnityEngine.Random.Range(0.3f, 0.8f); // Reduced radius range
                Vector3 targetPos2 = present.transform.position + new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                    UnityEngine.Random.Range(0.3f, 0.8f),
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius
                );

                StartCoroutine(AnimateOrnament(ornament, targetPos2));
            }
        }

        private IEnumerator AnimateOrnament(GameObject ornament, Vector3 targetPos) {
            float duration = 0.35f; // Shorter duration for snappier movement
            float elapsedTime = 0;
            Vector3 startPos = ornament.transform.position;

            while (elapsedTime < duration) {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                
                float height = Mathf.Sin(progress * Mathf.PI) * 0.8f;
                float overshoot = Mathf.Sin(progress * Mathf.PI * 3) * (1 - progress) * 0.3f;
                Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
                currentPos += Vector3.up * height;
                currentPos += (targetPos - startPos).normalized * overshoot;
                
                ornament.transform.position = currentPos;
                yield return null;
            }

            ornament.transform.position = targetPos;
        }

        [UIAction("OnClosePressed"), UsedImplicitly]
        void OnCloseButtonClick() {
            Close();
        }

        [UIAction("OnDecoratePressed"), UsedImplicitly]
        void OnDecorateButtonClick() {
            LeaderboardEvents.NotifyTreeEditorWasRequested();
            Close();
        }
    }
}
