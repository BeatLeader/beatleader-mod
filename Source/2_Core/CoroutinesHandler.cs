using System;
using System.Collections;

namespace BeatLeader {
    internal class CoroutinesHandler : MonoSingleton<CoroutinesHandler> {
        public new void StartCoroutine(IEnumerator coroutine) {
            gameObject.SetActive(true);
            base.StartCoroutine(coroutine);
        }
        public void StartCoroutine(IEnumerator coroutine, Action finishCallback) {
            gameObject.SetActive(true);
            base.StartCoroutine(CallbackCoroutine(coroutine, finishCallback));
        }

        private IEnumerator CallbackCoroutine(IEnumerator coroutine, Action callback) {
            yield return coroutine;
            callback?.Invoke();
        }
    }
}
