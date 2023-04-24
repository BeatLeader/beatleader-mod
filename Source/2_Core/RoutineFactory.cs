using System.Collections;
using UnityEngine;

namespace BeatLeader {
    internal interface IManagedRoutine {
        void Run();
        void Cancel();
    }

    internal class RoutineContainer : MonoBehaviour, IManagedRoutine {
        private IEnumerator? _routine;

        public void Init(IEnumerator routine) => _routine = routine;

        public void Run() {
            if (_routine == null) return;
            StartCoroutine(Coroutine(_routine));
            _routine = null;
        }
        
        public void Cancel() => DestroyImmediate(this);

        private IEnumerator Coroutine(IEnumerator mainCoroutine) {
            yield return mainCoroutine;
            Cancel();
        }
    }

    internal class RoutineFactory : PersistentSingleton<RoutineFactory> {
        public static IManagedRoutine ReleaseManagedRoutine(IEnumerator coroutine) {
            var routine = new GameObject(nameof(RoutineContainer)).AddComponent<RoutineContainer>();
            routine.Init(coroutine);
            return routine;
        }

        public static IManagedRoutine StartManagedRoutine(IEnumerator coroutine) {
            var routine = ReleaseManagedRoutine(coroutine);
            routine.Run();
            return routine;
        }
        
        public static void StartUnmanagedCoroutine(IEnumerator coroutine) {
            instance.gameObject.SetActive(true);
            instance.StartCoroutine(coroutine);
        }
    }
}