using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Utils
{
    internal class Repeater : IDisposable
    {
        protected Repeater() { }

        public Action Action { get; private set; }
        public bool IsRunning { get; protected set; }
        public int Delay { get; set; }

        protected CancellationTokenSource _cancellationSource;

        public void Dispose()
        {
            Stop();
        }
        public void Reload()
        {
            Stop();
            Run();
        }
        public void Stop()
        {
            _cancellationSource.Cancel();
        }
        public void Run()
        {
            _cancellationSource = new();
            //just don't know how to run it correctly, please ping Hermanest#0535 on beatleader server if you know 
            Task.Run(() => RepeaterTask(_cancellationSource.Token, Action, Delay));
        }

        protected static async Task RepeaterTask(CancellationToken token, Action action, int delay)
        {
            while (true)
            {
                await Task.Delay(delay, token);
                if (token.IsCancellationRequested) return;
                action();
            }
        }

        public static Repeater Create(Action action, bool run = true)
        {
            return Create(action, 0, run);
        }
        public static Repeater Create(Action action, int delay, bool run = true)
        {
            if (action == null) return null;

            var repeater = new Repeater();
            repeater.Delay = delay;
            repeater.Action = action;
            if (run) repeater.Run();

            return repeater;
        }
    }
}
